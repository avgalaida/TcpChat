using Server.Models.Messages;

namespace Server.Interfaces;

public interface IChatServer
{
    Task StartAsync();
    void AddClient(IClientHandler client);
    void RemoveClient(IClientHandler client);
    Task BroadcastMessageAsync(ChatMessage message, string sender);
}