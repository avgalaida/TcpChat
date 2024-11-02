using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Interfaces;

namespace Server.Services;

public class ChatServer : IChatServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, IClientHandler> _clients;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatServer> _logger;

    public ChatServer(IServiceProvider serviceProvider, ILogger<ChatServer> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _clients = new ConcurrentDictionary<string, IClientHandler>();

        var port = configuration.GetValue<int>("ServerSettings:Port");
        _listener = new TcpListener(IPAddress.Any, port);

        _logger.LogInformation($"Сервер инициализирован на порту {port}.");
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _logger.LogInformation("Сервер запущен и ожидает подключения клиентов.");

        while (true)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            _logger.LogInformation($"Получено новое подключение от {tcpClient.Client.RemoteEndPoint}");

            var clientHandler = ActivatorUtilities.CreateInstance<ClientHandler>(_serviceProvider, tcpClient, this);
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
        else
        {
            _logger.LogWarning($"Не удалось добавить клиента: {client.ClientId}");
        }
    }

    public void RemoveClient(IClientHandler client)
    {
        if (_clients.TryRemove(client.ClientId, out _))
        {
            _logger.LogInformation($"Клиент удален: {client.ClientId}");
        }
        else
        {
            _logger.LogWarning($"Не удалось удалить клиента: {client.ClientId}");
        }
    }

    public async Task BroadcastMessageAsync(string message, string sender)
    {
        var tasks = new List<Task>();

        foreach (var client in _clients.Values)
        {
            if (client.ClientId != sender)
            {
                tasks.Add(SendMessageToClientAsync(client, $"{sender}: {message}"));
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при рассылке сообщений.");
        }
    }

    private async Task SendMessageToClientAsync(IClientHandler client, string message)
    {
        try
        {
            await client.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при отправке сообщения клиенту {client.ClientId}. Клиент будет отключен.");
            client.Disconnect();
            RemoveClient(client);
        }
    }
}