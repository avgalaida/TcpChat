namespace Server.Interfaces;

public interface IChatServer
{
    Task StartAsync();

    void BroadcastMessage(string message, string sender);

    void RemoveClient(IClientHandler client);
}