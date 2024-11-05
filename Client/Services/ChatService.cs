using System.Net.Sockets;
using System.Text;
using Client.Models;

namespace Client.Services;
public class ChatService : IChatService
{
    public event EventHandler<ChatMessage> MessageReceived;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;

    private readonly string _serverIp;
    private readonly int _serverPort;

    public ChatService(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
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

            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
        }

        await Task.Run(() => Disconnect());
    }

    public async Task SendMessageAsync(string message)
    {
        if (_stream == null)
            throw new InvalidOperationException("Not connected to the server.");

        var messageBytes = Encoding.UTF8.GetBytes(message);
        var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
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
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    private async Task<string> ReadMessageAsync(CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        var bytesRead = await ReadExactAsync(_stream, lengthBuffer, 0, lengthBuffer.Length, cancellationToken);
        if (bytesRead == 0)
        {
            return null;
        }

        var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

        var messageBuffer = new byte[messageLength];
        bytesRead = await ReadExactAsync(_stream, messageBuffer, 0, messageLength, cancellationToken);
        if (bytesRead == 0)
        {
            return null;
        }

        var message = Encoding.UTF8.GetString(messageBuffer);
        return message;
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

    private void Disconnect()
    {
        try
        {
            _stream?.Close();
            _tcpClient?.Close();
        }
        catch (Exception ex)
        {
        }
    }
}
