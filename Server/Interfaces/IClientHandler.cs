namespace Server.Interfaces;

public interface IClientHandler
{
    Task ProcessAsync();

    Task SendMessageAsync(string message);

    void Disconnect();

    string ClientId { get; }
}