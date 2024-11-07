using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Interfaces;
using Server.Models.Messages;

namespace Server.Services;
public class ClientHandler : IClientHandler
{
    private readonly TcpClient _tcpClient;
    private readonly IChatServer _server;
    private readonly IMessageRepository _messageRepository;
    private readonly NetworkStream _stream;
    private readonly ILogger<ClientHandler> _logger;

    private const int MaxMessageLength = 10 * 1024 * 1024; // Максимальная длина сообщения: 10 МБ

    public string ClientId { get; private set; }

    public ClientHandler(TcpClient tcpClient, IChatServer server, IMessageRepository messageRepository,
        ILogger<ClientHandler> logger)
    {
        _tcpClient = tcpClient;
        _server = server;
        _messageRepository = messageRepository;
        _logger = logger;
        _stream = tcpClient.GetStream();
        ClientId = Guid.NewGuid().ToString();

        _logger.LogInformation($"Клиент подключен: {ClientId}, IP: {_tcpClient.Client.RemoteEndPoint}");
    }

    public async Task ProcessAsync()
    {
        try
        {
            while (true)
            {
                string message = await ReadMessageAsync();

                if (message == null)
                {
                    break;
                }

                _logger.LogInformation($"Получено сообщение от {ClientId}: {message}");

                BaseMessage baseMessage;
                try
                {
                    baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
                    if (baseMessage == null)
                    {
                        _logger.LogWarning($"Некорректное сообщение от клиента {ClientId}.");
                        continue;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"Ошибка десериализации сообщения от клиента {ClientId}: {ex.Message}");
                    continue;
                }

                switch (baseMessage.Type)
                {
                    case MessageType.ChatMessage:
                        await HandleChatMessageAsync(message);
                        break;

                    case MessageType.HistoryRequest:
                        await HandleHistoryRequestAsync(message);
                        break;

                    default:
                        _logger.LogWarning($"Неизвестный тип сообщения от клиента {ClientId}: {baseMessage.Type}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка у клиента {ClientId}: {ex.Message}");
        }
        finally
        {
            _logger.LogInformation($"Клиент {ClientId} отключается.");
            Disconnect();
            _server.RemoveClient(this);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

            // Отправка длины сообщения (4 байта, little-endian)
            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            // Отправка самого сообщения
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            _logger.LogDebug($"Отправлено сообщение клиенту {ClientId}: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отправке сообщения клиенту {ClientId}");
            throw;
        }
    }

    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _tcpClient?.Close();
            _logger.LogInformation($"Соединение с клиентом {ClientId} закрыто.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отключении клиента {ClientId}: {ex.Message}");
        }
    }

    private async Task<string> ReadMessageAsync()
    {
        try
        {
            // Чтение длины сообщения (4 байта)
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await ReadExactAsync(_stream, lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead == 0)
            {
                _logger.LogWarning($"Клиент {ClientId} отключился при чтении длины сообщения.");
                return null;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            if (messageLength <= 0 || messageLength > MaxMessageLength)
            {
                _logger.LogWarning($"Клиент {ClientId} отправил некорректную длину сообщения: {messageLength}.");
                return null;
            }

            // Чтение самого сообщения
            byte[] messageBuffer = new byte[messageLength];
            bytesRead = await ReadExactAsync(_stream, messageBuffer, 0, messageLength);
            if (bytesRead == 0)
            {
                _logger.LogWarning($"Клиент {ClientId} отключился при чтении сообщения.");
                return null;
            }

            string message = Encoding.UTF8.GetString(messageBuffer);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при чтении сообщения от клиента {ClientId}");
            return null;
        }
    }

    private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int size)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < size)
        {
            int bytesRead = await stream.ReadAsync(buffer, offset + totalBytesRead, size - totalBytesRead);
            if (bytesRead == 0)
            {
                // Соединение закрыто
                return totalBytesRead;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }

    private async Task HandleChatMessageAsync(string message)
    {
        try
        {
            var incomingChatMessage = JsonConvert.DeserializeObject<IncomingChatMessage>(message);
            if (incomingChatMessage == null)
            {
                _logger.LogWarning($"Некорректный формат ChatMessage от клиента {ClientId}.");
                return;
            }

            // Получение информации о клиенте
            var remoteEndPoint = _tcpClient.Client.RemoteEndPoint as IPEndPoint;
            string senderIp = remoteEndPoint?.Address.ToString() ?? "Unknown";
            int senderPort = remoteEndPoint?.Port ?? 0;

            // Создание исходящего сообщения
            var outgoingChatMessage = new OutgoingChatMessage
            {
                Id = GenerateMessageId(),
                Type = MessageType.ChatMessage,
                Sender = GetSenderName(),
                Content = incomingChatMessage.Content,
                Timestamp = DateTime.UtcNow,
                SenderIp = senderIp,
                SenderPort = senderPort
            };

            // Сохранение сообщения в репозитории
            await _messageRepository.SaveMessageAsync(outgoingChatMessage);
            _logger.LogInformation($"Сохранено сообщение от {ClientId}: {outgoingChatMessage.Content}");

            // Рассылка сообщения другим клиентам
            await _server.BroadcastMessageAsync(outgoingChatMessage, ClientId);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning($"Ошибка десериализации ChatMessage от клиента {ClientId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке ChatMessage от клиента {ClientId}: {ex.Message}");
        }
    }

    private async Task HandleHistoryRequestAsync(string message)
    {
        try
        {
            var historyRequest = JsonConvert.DeserializeObject<HistoryRequest>(message);
            if (historyRequest == null)
            {
                _logger.LogWarning($"Некорректный формат HistoryRequest от клиента {ClientId}.");
                return;
            }

            // Валидация параметров
            int page = historyRequest.Page < 1 ? 1 : historyRequest.Page;
            int pageSize = historyRequest.PageSize < 1 ? 10 : historyRequest.PageSize;
            if (pageSize > 100) pageSize = 100; // Максимальный размер страницы

            // Получение сообщений из репозитория
            var messages = await _messageRepository.GetMessagesAsync(page, pageSize);
            int totalMessages = await _messageRepository.CountAsync();

            // Создание ответа с историей
            var historyResponse = new HistoryResponse
            {
                Type = MessageType.HistoryResponse,
                TotalMessages = totalMessages,
                Page = page,
                PageSize = pageSize,
                Messages = messages
            };

            string jsonResponse = JsonConvert.SerializeObject(historyResponse);
            await SendMessageAsync(jsonResponse);

            _logger.LogInformation($"Отправлена история сообщений клиенту {ClientId}: Страница {page}, Размер {pageSize}");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning($"Ошибка десериализации HistoryRequest от клиента {ClientId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке HistoryRequest от клиента {ClientId}: {ex.Message}");
        }
    }

    private Guid GenerateMessageId()
    {
        return Guid.NewGuid();
    }

    private string GetSenderName()
    {
        // Реализуйте логику получения имени отправителя
        // Например, можно использовать имя пользователя из аутентификации
        // Для примера используем часть ClientId
        return $"User_{ClientId.Substring(0, 8)}"; // Пример: User_391f813b
    }
}