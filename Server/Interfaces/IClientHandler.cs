namespace Server.Interfaces;

public interface IClientHandler
{
    string ClientId { get; }
    Task ProcessAsync();
    Task SendMessageAsync(string message);
    void Disconnect();
}