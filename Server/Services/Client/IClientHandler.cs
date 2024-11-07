using System.Net;
using Server.Models.Messages;

namespace Server.Services.Client;

public interface IClientHandler
{
    string ClientId { get; }
    IPEndPoint RemoteEndPoint { get; }
    Task ProcessAsync();
    Task SendMessageAsync(BaseMessage message);
    void Disconnect();
}