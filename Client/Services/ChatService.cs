using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Models.Messages;
using Client.Utilities;
using Microsoft.Extensions.Logging;

namespace Client.Services;

/// <summary>
/// Сервис для управления чатом через TCP-соединение.
/// </summary>
public class ChatService : IChatService, IDisposable
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;
    private bool _disposed;

    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly ILogger<ChatService> _logger;
    private readonly IMessageSerializer _serializer;

    private const int MaxMessageLength = 10 * 1024 * 1024; // 10 МБ

    /// <summary>
    /// Событие, возникающее при получении нового сообщения чата.
    /// </summary>
    public event EventHandler<IncomingChatMessage> MessageReceived;

    /// <summary>
    /// Событие, возникающее при получении истории сообщений.
    /// </summary>
    public event EventHandler<HistoryResponse> HistoryReceived;

    /// <summary>
    /// Событие, возникающее при возникновении ошибки.
    /// </summary>
    public event EventHandler<Exception> OnError;

    /// <summary>
    /// Указывает, подключен ли сервис к серверу.
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected ?? false;

    /// <summary>
    /// Возвращает локальную конечную точку соединения.
    /// </summary>
    public IPEndPoint LocalEndPoint => _tcpClient?.Client?.LocalEndPoint as IPEndPoint;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ChatService"/>.
    /// </summary>
    /// <param name="serverIp">IP-адрес сервера.</param>
    /// <param name="serverPort">Порт сервера.</param>
    /// <param name="logger">Логгер для ведения журналов.</param>
    /// <param name="serializer">Сериализатор сообщений.</param>
    public ChatService(string serverIp, int serverPort, ILogger<ChatService> logger, IMessageSerializer serializer)
    {
        _serverIp = serverIp ?? throw new ArgumentNullException(nameof(serverIp));
        _serverPort = serverPort;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Возвращает локальную конечную точку в формате "IP:Port".
    /// </summary>
    /// <returns>Строка с локальной конечной точкой.</returns>
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

    /// <summary>
    /// Асинхронно устанавливает соединение с сервером.
    /// </summary>
    /// <returns>Возвращает <c>true</c>, если соединение успешно установлено; иначе <c>false</c>.</returns>
    public async Task<bool> ConnectAsync()
    {
        EnsureNotDisposed();

        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_serverIp, _serverPort);
            _stream = _tcpClient.GetStream();
            _cts = new CancellationTokenSource();

            // Запуск задачи приема сообщений
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

    /// <summary>
    /// Асинхронно отключает соединение от сервера.
    /// </summary>
    public async Task DisconnectAsync()
    {
        EnsureNotDisposed();

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

    /// <summary>
    /// Асинхронно отправляет сообщение в чат.
    /// </summary>
    /// <param name="content">Содержимое сообщения.</param>
    /// <exception cref="InvalidOperationException">Если нет установленного соединения.</exception>
    public async Task SendChatMessageAsync(string content)
    {
        EnsureNotDisposed();
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Содержимое сообщения не может быть пустым.", nameof(content));

        if (_stream == null)
        {
            var ex = new InvalidOperationException("Попытка отправить сообщение без подключения.");
            _logger.LogError(ex, "Попытка отправить сообщение без подключения.");
            OnError?.Invoke(this, ex);
            throw ex;
        }

        var message = new IncomingChatMessage
        {
            Type = MessageType.ChatMessage,
            Content = content
        };
        var serializedMessage = _serializer.Serialize(message);

        await SendMessageAsync(serializedMessage);
    }

    /// <summary>
    /// Асинхронно запрашивает историю сообщений.
    /// </summary>
    /// <param name="page">Номер страницы истории.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <exception cref="InvalidOperationException">Если нет установленного соединения.</exception>
    public async Task RequestHistoryAsync(int page, int pageSize)
    {
        EnsureNotDisposed();

        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Номер страницы должен быть больше нуля.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Размер страницы должен быть больше нуля.");

        if (_stream == null)
        {
            var ex = new InvalidOperationException("Попытка запросить историю без подключения.");
            _logger.LogError(ex, "Попытка запросить историю без подключения.");
            OnError?.Invoke(this, ex);
            throw ex;
        }

        var request = new HistoryRequest
        {
            Type = MessageType.HistoryRequest,
            Page = page,
            PageSize = pageSize
        };
        var serializedRequest = _serializer.Serialize(request);

        await SendMessageAsync(serializedRequest);
    }

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром <see cref="ChatService"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        DisconnectAsync().GetAwaiter().GetResult();

        _stream?.Dispose();
        _tcpClient?.Dispose();
        _cts?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Отправляет сообщение на сервер.
    /// </summary>
    /// <param name="message">Сериализованное сообщение.</param>
    private async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            _logger.LogWarning("Попытка отправить пустое сообщение.");
            return;
        }

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

    /// <summary>
    /// Асинхронно получает сообщения от сервера.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
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

                HandleIncomingMessage(baseMessage);
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

    /// <summary>
    /// Обрабатывает полученное сообщение в зависимости от его типа.
    /// </summary>
    /// <param name="baseMessage">Базовое сообщение.</param>
    private void HandleIncomingMessage(BaseMessage baseMessage)
    {
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

    /// <summary>
    /// Асинхронно читает сообщение из потока.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Содержимое сообщения в виде строки или <c>null</c>, если соединение закрыто.</returns>
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

    /// <summary>
    /// Асинхронно читает заданное количество байт из потока.
    /// </summary>
    /// <param name="stream">Поток для чтения.</param>
    /// <param name="buffer">Буфер для данных.</param>
    /// <param name="offset">Смещение в буфере.</param>
    /// <param name="size">Количество байт для чтения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество прочитанных байт.</returns>
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

    /// <summary>
    /// Отключает от сервера и освобождает ресурсы.
    /// </summary>
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

    /// <summary>
    /// Проверяет, не был ли объект уже освобожден.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Если объект уже освобожден.</exception>
    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChatService));
    }
}
