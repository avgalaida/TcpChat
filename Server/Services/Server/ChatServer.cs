using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Server.Models.Messages;
using Server.Services.Client;
using Server.Services.Factories;

namespace Server.Services.Server;

public class ChatServer : IChatServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, IClientHandler> _clients = new();
    private readonly IClientHandlerFactory _clientHandlerFactory;
    private readonly ILogger<ChatServer> _logger;

    public ChatServer(IClientHandlerFactory clientHandlerFactory, ILogger<ChatServer> logger, IConfiguration configuration)
    {
        var port = configuration.GetValue<int>("ServerSettings:Port");
        _listener = new TcpListener(IPAddress.Any, port);
        _clientHandlerFactory = clientHandlerFactory;
        _logger = logger;

        _logger.LogInformation($"Сервер инициализирован на порту {port}.");
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _logger.LogInformation("Сервер запущен и ожидает подключения клиентов.");

        while (true)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            _logger.LogInformation($"Новое подключение от {tcpClient.Client.RemoteEndPoint}");

            var clientHandler =_clientHandlerFactory.CreateClientHandler(tcpClient, this);
            AddClient(clientHandler);
            _ = clientHandler.ProcessAsync();
        }
    }

    public void AddClient(IClientHandler client)
    {
        if (_clients.TryAdd(client.ClientId, client))
        {
            _logger.LogInformation($"Клиент добавлен: {client.ClientId}");
        }
    }

    public void RemoveClient(IClientHandler client)
    {
        if (_clients.TryRemove(client.ClientId, out _))
        {
            _logger.LogInformation($"Клиент удален: {client.ClientId}");
        }
    }

    public async Task BroadcastMessageAsync(OutgoingChatMessage message)
    {
        var tasks = _clients.Values
            .Where(client => client.ClientId != message.Sender)
            .Select(client => SendMessageToClientAsync(client, message));

        await Task.WhenAll(tasks);
    }

    private async Task SendMessageToClientAsync(IClientHandler client, OutgoingChatMessage message)
    {
        try
        {
            await client.SendMessageAsync(message);
        }
        catch
        {
            client.Disconnect();
            RemoveClient(client);
        }
    }
}