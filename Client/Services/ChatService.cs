using Client.Models;
using System.Net.Sockets;

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
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public Task SendMessageAsync(string message)
    {
        throw new NotImplementedException();
    }
}