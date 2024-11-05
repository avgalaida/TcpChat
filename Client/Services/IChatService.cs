using Client.Models;
using System.Net;

namespace Client.Services;
public interface IChatService
{
    event EventHandler<ChatMessage> MessageReceived;
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task SendMessageAsync(string message);
    IPEndPoint LocalEndPoint { get; }
}
