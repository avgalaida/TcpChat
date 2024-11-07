using System.Net.Sockets;
using System.Text;
using Client.Models.Messages;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Client.Services;
public class ChatService : IChatService
{
    public event EventHandler<ServerChatMessage> MessageReceived;
    public event EventHandler<HistoryResponse> HistoryReceived;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;

    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly ILogger<ChatService> _logger;

    private const int MaxMessageLength = 10 * 1024 * 1024; // 10 МБ

    public IPEndPoint LocalEndPoint
    {
        get
        {
            if (_tcpClient?.Client?.LocalEndPoint is IPEndPoint endPoint)
            {
                return endPoint;
            }
            return null;
        }
    }

    public ChatService(string serverIp, int serverPort, ILogger<ChatService> logger)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _logger = logger;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_serverIp, _serverPort);
            _stream = _tcpClient.GetStream();
            _cts = new CancellationTokenSource();

            _ = Task.Run(() => ReceiveMessagesAsync(_cts.Token), _cts.Token);

            _logger.LogInformation("Подключение к серверу успешно.");
            return true;
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "SocketException при подключении к серверу.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception при подключении к серверу.");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        await Task.Run(() => Disconnect());
    }

    public async Task SendChatMessageAsync(string content)
    {
        if (_stream == null)
        {
            _logger.LogWarning("Попытка отправить сообщение без подключения к серверу.");
            return;
        }

        var sendMessage = new SendChatMessage
        {
            Type = MessageType.ChatMessage,
            Content = content
        };

        var json = JsonConvert.SerializeObject(sendMessage);
        await SendMessageAsync(json);
    }

    public async Task RequestHistoryAsync(int page, int pageSize)
    {
        if (_stream == null)
        {
            _logger.LogWarning("Попытка запросить историю без подключения к серверу.");
            return;
        }

        // Валидация параметров
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var historyRequest = new HistoryRequest
        {
            Type = MessageType.HistoryRequest,
            Page = page,
            PageSize = pageSize
        };

        var json = JsonConvert.SerializeObject(historyRequest);
        await SendMessageAsync(json);
    }

    private async Task SendMessageAsync(string message)
    {
        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            _logger.LogDebug("Отправлено сообщение: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения.");
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await ReadMessageAsync(cancellationToken);
                if (message == null)
                {
                    _logger.LogInformation("Соединение закрыто сервером.");
                    break;
                }

                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
                if (baseMessage == null)
                {
                    _logger.LogWarning("Получено некорректное сообщение от сервера.");
                    continue;
                }

                switch (baseMessage.Type)
                {
                    case MessageType.ChatMessage:
                        var serverChatMessage = JsonConvert.DeserializeObject<ServerChatMessage>(message);
                        if (serverChatMessage != null)
                        {
                            MessageReceived?.Invoke(this, serverChatMessage);
                            _logger.LogInformation("Получено сообщение: {Content} от {Sender}", serverChatMessage.Content, serverChatMessage.Sender);
                        }
                        break;

                    case MessageType.HistoryResponse:
                        var historyResponse = JsonConvert.DeserializeObject<HistoryResponse>(message);
                        if (historyResponse != null)
                        {
                            HistoryReceived?.Invoke(this, historyResponse);
                            _logger.LogInformation("Получен ответ истории: Страница {Page} из {TotalPages}", historyResponse.Page, Math.Ceiling((double)historyResponse.TotalMessages / historyResponse.PageSize));
                        }
                        break;

                    default:
                        _logger.LogWarning("Получен неизвестный тип сообщения: {Type}", baseMessage.Type);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Задача получения сообщений отменена.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении сообщений.");
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    private async Task<string> ReadMessageAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lengthBuffer = new byte[4];
            var bytesRead = await ReadExactAsync(_stream, lengthBuffer, 0, lengthBuffer.Length, cancellationToken);
            if (bytesRead == 0)
            {
                return null;
            }

            var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            if (messageLength <= 0 || messageLength > MaxMessageLength)
            {
                _logger.LogWarning("Получено сообщение с некорректной длиной: {Length}", messageLength);
                return null;
            }

            var messageBuffer = new byte[messageLength];
            bytesRead = await ReadExactAsync(_stream, messageBuffer, 0, messageLength, cancellationToken);
            if (bytesRead == 0)
            {
                return null;
            }

            var message = Encoding.UTF8.GetString(messageBuffer);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при чтении сообщения.");
            return null;
        }
    }

    private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int size, CancellationToken cancellationToken)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < size)
        {
            int bytesRead = await stream.ReadAsync(buffer, offset + totalBytesRead, size - totalBytesRead, cancellationToken);
            if (bytesRead == 0)
            {
                // Соединение закрыто
                return 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }

    private void Disconnect()
    {
        try
        {
            _stream?.Close();
            _tcpClient?.Close();
            _logger.LogInformation("Соединение с сервером закрыто.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отключении от сервера.");
        }
    }
}