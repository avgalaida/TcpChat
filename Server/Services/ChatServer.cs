using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Interfaces;

namespace Server.Services;

public class ChatServer : IChatServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentBag<IClientHandler> _clients;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatServer> _logger;

    public ChatServer(IServiceProvider serviceProvider, ILogger<ChatServer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _clients = new ConcurrentBag<IClientHandler>();
        _listener = new TcpListener(IPAddress.Any, 3000);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _logger.LogInformation("Сервер запущен на порту 3000.");

        while (true)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            var clientHandler = ActivatorUtilities.CreateInstance<ClientHandler>(_serviceProvider, tcpClient, this);
            _clients.Add(clientHandler);
            _ = clientHandler.ProcessAsync();
        }
    }

    public void BroadcastMessage(string message, string sender)
    {
        foreach (var client in _clients)
        {
            if (client.ClientId != sender)
            {
                client.SendMessageAsync($"{sender}: {message}");
            }
        }
    }

    public void RemoveClient(IClientHandler client)
    {
        _clients.TryTake(out client);
        _logger.LogInformation($"Клиент отключился: {client.ClientId}");
    }
}