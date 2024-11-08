using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Models.Messages;
using Client.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Client.Services
{
    public class ChatService : IChatService
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly ILogger<ChatService> _logger;
        private readonly IMessageSerializer _serializer;

        private const int MaxMessageLength = 10 * 1024 * 1024; // 10 МБ

        public event EventHandler<IncomingChatMessage> MessageReceived;
        public event EventHandler<HistoryResponse> HistoryReceived;
        public event EventHandler<Exception> OnError;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public IPEndPoint LocalEndPoint => _tcpClient?.Client?.LocalEndPoint as IPEndPoint;

        public ChatService(string serverIp, int serverPort, ILogger<ChatService> logger, IMessageSerializer serializer)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _logger = logger;
            _serializer = serializer;
        }
        public string GetFormattedLocalEndPoint()
        {
            if (LocalEndPoint != null)
            {
                var ip = LocalEndPoint.Address.MapToIPv4().ToString();
                var port = LocalEndPoint.Port;
                return $"{ip}:{port}";
            }
            return string.Empty;
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
                _logger.LogError(ex, "Ошибка подключения к серверу.");
                OnError?.Invoke(this, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Общая ошибка подключения.");
                OnError?.Invoke(this, ex);
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

            // Подготовка клиента к повторному подключению
            _tcpClient = new TcpClient();
        }

        public async Task SendChatMessageAsync(string content)
        {
            if (_stream == null)
            {
                var ex = new InvalidOperationException("Попытка отправить сообщение без подключения.");
                OnError?.Invoke(this, ex);
                throw ex;
            }

            var message = new IncomingChatMessage { Type = MessageType.ChatMessage, Content = content };
            var serializedMessage = _serializer.Serialize(message);

            await SendMessageAsync(serializedMessage);
        }

        public async Task RequestHistoryAsync(int page, int pageSize)
        {
            if (_stream == null)
            {
                var ex = new InvalidOperationException("Попытка запросить историю без подключения.");
                OnError?.Invoke(this, ex);
                throw ex;
            }

            var request = new HistoryRequest { Type = MessageType.HistoryRequest, Page = page, PageSize = pageSize };
            var serializedRequest = _serializer.Serialize(request);

            await SendMessageAsync(serializedRequest);
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
                OnError?.Invoke(this, ex);
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

                    var baseMessage = _serializer.Deserialize(message);
                    if (baseMessage == null)
                    {
                        _logger.LogWarning("Получено некорректное сообщение от сервера.");
                        continue;
                    }

                    switch (baseMessage)
                    {
                        case IncomingChatMessage chatMessage:
                            MessageReceived?.Invoke(this, chatMessage);
                            _logger.LogInformation("Получено сообщение: {Content} от {Sender}", chatMessage.Content, chatMessage.Sender);
                            break;

                        case HistoryResponse historyResponse:
                            HistoryReceived?.Invoke(this, historyResponse);
                            _logger.LogInformation("Получена история сообщений.");
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
                OnError?.Invoke(this, ex);
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
                    _logger.LogWarning("Некорректная длина сообщения: {Length}", messageLength);
                    return null;
                }

                var messageBuffer = new byte[messageLength];
                bytesRead = await ReadExactAsync(_stream, messageBuffer, 0, messageLength, cancellationToken);
                if (bytesRead == 0)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(messageBuffer);
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
                OnError?.Invoke(this, ex);
            }
        }
    }
}