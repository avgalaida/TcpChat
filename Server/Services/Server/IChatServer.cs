using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Server;

public interface IChatServer
{
    Task StartAsync();
    void AddClient(IClientHandler client);
    void RemoveClient(IClientHandler client);
    Task BroadcastMessageAsync(OutgoingChatMessage message);
}