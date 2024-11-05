using Client.Models;

namespace Client.Services;
public interface IChatService
{
    event EventHandler<ChatMessage> MessageReceived;
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task SendMessageAsync(string message);
}
