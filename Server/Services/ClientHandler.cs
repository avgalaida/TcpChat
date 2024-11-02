using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Server.Interfaces;
using Server.Models;

namespace Server.Services;

public class ClientHandler : IClientHandler
{
    private readonly TcpClient _tcpClient;
    private readonly IChatServer _server;
    private readonly IMessageRepository _messageRepository;
    private readonly NetworkStream _stream;
    private readonly ILogger<ClientHandler> _logger;

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

                var chatMessage = new ChatMessage
                {
                    Sender = ClientId,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };

                await _messageRepository.SaveMessageAsync(chatMessage);

                _server.BroadcastMessage(message, ClientId);
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

    public async Task SendMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public void Disconnect()
    {
        _stream?.Close();
        _tcpClient?.Close();
    }

    private async Task<string> ReadMessageAsync()
    {
        var lengthBuffer = new byte[4];
        var bytesRead = await _stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
        if (bytesRead == 0)
        {
            return null; // Клиент отключился
        }

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

        var messageBuffer = new byte[messageLength];
        var totalBytesRead = 0;
        while (totalBytesRead < messageLength)
        {
            bytesRead = await _stream.ReadAsync(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
            if (bytesRead == 0)
            {
                return null; // Клиент отключился
            }

            totalBytesRead += bytesRead;
        }

        var message = Encoding.UTF8.GetString(messageBuffer);
        return message;
    }
}