using Server.Models.Messages;
using Server.Services.Client;
using Server.Services.Factories;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Server.Services.Server;

/// <summary>
/// Реализация сервера чата, принимающая подключения клиентов и передающая сообщения.
/// </summary>
public class ChatServer : IChatServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, IClientHandler> _clients = new();
    private readonly IClientHandlerFactory _clientHandlerFactory;
    private readonly ILogger<ChatServer> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ChatServer"/>.
    /// </summary>
    /// <param name="clientHandlerFactory">Фабрика для создания обработчиков клиентов.</param>
    /// <param name="logger">Экземпляр логгера для записи логов.</param>
    /// <param name="configuration">Конфигурация приложения для получения настроек сервера.</param>
    public ChatServer(IClientHandlerFactory clientHandlerFactory, ILogger<ChatServer> logger,
        IConfiguration configuration)
    {
        var port = configuration.GetValue<int>("ServerSettings:Port");
        _listener = new TcpListener(IPAddress.Any, port);
        _clientHandlerFactory = clientHandlerFactory;
        _logger = logger;

        _logger.LogInformation($"Сервер инициализирован на порту {port}.");
    }

    /// <summary>
    /// Асинхронно запускает сервер и ожидает подключения клиентов до тех пор, 
    /// пока не будет получена команда на отмену через <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены операции, который позволяет завершить работу сервера.</param>
    /// <returns>Задача, представляющая асинхронную операцию запуска сервера.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        _logger.LogInformation("Сервер запущен и ожидает подключения клиентов.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                _logger.LogInformation($"Новое подключение от {tcpClient.Client.RemoteEndPoint}");

                var clientHandler = _clientHandlerFactory.CreateClientHandler(tcpClient, this);
                AddClient(clientHandler);
                _ = clientHandler.ProcessAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Операция сервера была отменена.");
        }
        finally
        {
            _listener.Stop();
            _logger.LogInformation("Сервер остановлен.");
        }
    }

    /// <summary>
    /// Добавляет клиента к серверу.
    /// </summary>
    /// <param name="client">Обработчик клиента для добавления.</param>
    public void AddClient(IClientHandler client)
    {
        if (_clients.TryAdd(client.ClientId, client))
        {
            _logger.LogInformation($"Клиент добавлен: {client.ClientId}");
        }
    }

    /// <summary>
    /// Удаляет клиента с сервера.
    /// </summary>
    /// <param name="client">Обработчик клиента для удаления.</param>
    public void RemoveClient(IClientHandler client)
    {
        if (_clients.TryRemove(client.ClientId, out _))
        {
            _logger.LogInformation($"Клиент удален: {client.ClientId}");
        }
    }

    /// <summary>
    /// Асинхронно рассылает сообщение всем подключенным клиентам, кроме отправителя.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <returns>Задача, представляющая асинхронную операцию рассылки сообщения.</returns>
    public async Task BroadcastMessageAsync(OutgoingChatMessage message)
    {
        var senderEndPoint = $"{message.SenderIp}:{message.SenderPort}";

        var tasks = _clients.Values
            .Where(client => client.RemoteEndPoint.ToString() != senderEndPoint)
            .Select(client => SendMessageToClientAsync(client, message));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Асинхронно отправляет сообщение указанному клиенту.
    /// </summary>
    /// <param name="client">Клиент, которому отправляется сообщение.</param>
    /// <param name="message">Сообщение для отправки.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки сообщения.</returns>
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