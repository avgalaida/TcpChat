using Client.Models;

namespace Client.Services;
public interface IChatService
{
    event EventHandler<ChatMessage> MessageReceived;

    void Connect();
    Task SendMessageAsync(string message);
    void Disconnect();
}
