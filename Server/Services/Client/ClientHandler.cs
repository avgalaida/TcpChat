using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Server.Models.Messages;
using Server.Repository;
using Server.Services.Factories;
using Server.Services.Server;
using Server.Utilities;

namespace Server.Services.Client;

public class ClientHandler : IClientHandler
{
    private readonly TcpClient _tcpClient;
    private readonly IChatServer _server;
    private readonly IMessageRepository _messageRepository;
    private readonly NetworkStream _stream;
    private readonly ILogger<ClientHandler> _logger;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMessageHandlerFactory _handlerFactory;

    private const int MaxMessageLength = 10 * 1024 * 1024; // 10 MB

    public string ClientId { get; }

    public IPEndPoint RemoteEndPoint => _tcpClient.Client.RemoteEndPoint as IPEndPoint;

    public ClientHandler(
        TcpClient tcpClient,
        IChatServer server,
        IMessageRepository messageRepository,
        ILogger<ClientHandler> logger,
        IMessageSerializer messageSerializer,
        IMessageHandlerFactory handlerFactory)
    {
        _tcpClient = tcpClient;
        _server = server;
        _messageRepository = messageRepository;
        _logger = logger;
        _messageSerializer = messageSerializer;
        _handlerFactory = handlerFactory;
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
                var messageJson = await ReadMessageAsync();
                if (messageJson == null)
                    break;

                var baseMessage = _messageSerializer.Deserialize(messageJson);
                if (baseMessage == null)
                {
                    _logger.LogWarning($"Некорректное сообщение от клиента {ClientId}.");
                    continue;
                }

                await _handlerFactory.HandleMessageAsync(baseMessage, this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка у клиента {ClientId}");
        }
        finally
        {
            Disconnect();
            _server.RemoveClient(this);
        }
    }

    public async Task SendMessageAsync(BaseMessage message)
    {
        var serializedMessage = _messageSerializer.Serialize(message);
        await SendRawMessageAsync(serializedMessage);
    }

    private async Task SendRawMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);

        _logger.LogDebug($"Отправлено сообщение клиенту {ClientId}");
    }

    public void Disconnect()
    {
        try
        {
            _stream.Close();
            _tcpClient.Close();
            _logger.LogInformation($"Соединение с клиентом {ClientId} закрыто.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отключении клиента {ClientId}: {ex.Message}");
        }
    }

    private async Task<string> ReadMessageAsync()
    {
        var lengthBuffer = new byte[4];
        var bytesRead = await ReadExactAsync(lengthBuffer);
        if (bytesRead == 0)
            return null;

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
        if (messageLength <= 0 || messageLength > MaxMessageLength)
        {
            _logger.LogWarning($"Некорректная длина сообщения от клиента {ClientId}: {messageLength}");
            return null;
        }

        var messageBuffer = new byte[messageLength];
        bytesRead = await ReadExactAsync(messageBuffer);
        if (bytesRead == 0)
            return null;

        return Encoding.UTF8.GetString(messageBuffer);
    }

    private async Task<int> ReadExactAsync(byte[] buffer)
    {
        var totalBytesRead = 0;
        while (totalBytesRead < buffer.Length)
        {
            var bytesRead = await _stream.ReadAsync(buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead));
            if (bytesRead == 0)
                return totalBytesRead;
            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }
}