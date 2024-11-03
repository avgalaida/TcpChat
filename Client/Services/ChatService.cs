using Client.Models;
using System.Net.Sockets;
using System.Text;

namespace Client.Services;

public class ChatService : IChatService
{
    public event EventHandler<ChatMessage> MessageReceived;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private readonly string _serverIp;
    private readonly int _serverPort;

    public ChatService(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
    }

    public void Connect()
    {
        _tcpClient = new TcpClient();
        _tcpClient.Connect(_serverIp, _serverPort);
        _stream = _tcpClient.GetStream();

        Task.Run(ReceiveMessagesAsync);
    }

    public void Disconnect()
    {
        _stream?.Close();
        _tcpClient?.Close();
    }

    public async Task SendMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    private async Task ReceiveMessagesAsync()
    {
        try
        {
            while (true)
            {
                var message = await ReadMessageAsync();
                if (message == null)
                {
                    break;
                }

                var chatMessage = new ChatMessage
                {
                    Sender = ExtractSender(message),
                    Content = ExtractContent(message),
                    Timestamp = DateTime.Now
                };

                MessageReceived?.Invoke(this, chatMessage);
            }
        }
        catch (Exception ex)
        {
            // TODO
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task<string> ReadMessageAsync()
    {
        var lengthBuffer = new byte[4];
        var bytesRead = await _stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
        if (bytesRead == 0)
        {
            return null;
        }

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

        var messageBuffer = new byte[messageLength];
        int totalBytesRead = 0;
        while (totalBytesRead < messageLength)
        {
            bytesRead = await _stream.ReadAsync(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
            if (bytesRead == 0)
            {
                return null;
            }
            totalBytesRead += bytesRead;
        }

        var message = Encoding.UTF8.GetString(messageBuffer);
        return message;
    }

    private string ExtractSender(string message)
    {
        var parts = message.Split(new[] { ':' }, 2);
        return parts.Length > 1 ? parts[0] : "Неизвестно";
    }

    private string ExtractContent(string message)
    {
        var parts = message.Split(new[] { ':' }, 2);
        return parts.Length > 1 ? parts[1].Trim() : message;
    }

}