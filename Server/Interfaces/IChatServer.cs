namespace Server.Interfaces;

public interface IChatServer
{
    Task StartAsync();
    void AddClient(IClientHandler client);
    void RemoveClient(IClientHandler client);
    Task BroadcastMessageAsync(string message, string sender);
}