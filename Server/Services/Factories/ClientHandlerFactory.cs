using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Server.Services.Client;
using Server.Services.Server;

namespace Server.Services.Factories;

/// <summary>
/// Реализация фабрики для создания обработчиков клиентов.
/// </summary>
public class ClientHandlerFactory : IClientHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ClientHandlerFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">Поставщик служб для создания зависимостей.</param>
    public ClientHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Создает новый обработчик клиента с помощью <see cref="ActivatorUtilities"/>.
    /// </summary>
    /// <param name="tcpClient">TCP-клиент для связи.</param>
    /// <param name="server">Ссылка на сервер.</param>
    /// <returns>Экземпляр <see cref="IClientHandler"/>.</returns>
    public IClientHandler CreateClientHandler(TcpClient tcpClient, IChatServer server)
    {
        return ActivatorUtilities.CreateInstance<ClientHandler>(_serviceProvider, tcpClient, server);
    }
}