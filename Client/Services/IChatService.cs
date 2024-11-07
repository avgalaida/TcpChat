using Client.Models.Messages;
using System.Net;

namespace Client.Services;
public interface IChatService
{
    event EventHandler<ServerChatMessage> MessageReceived;
    event EventHandler<HistoryResponse> HistoryReceived;

    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task SendChatMessageAsync(string content);
    Task RequestHistoryAsync(int page, int pageSize);
    IPEndPoint LocalEndPoint { get; }
}
