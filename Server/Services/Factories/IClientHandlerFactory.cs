using System.Net.Sockets;
using Server.Services.Client;
using Server.Services.Server;

namespace Server.Services.Factories;

/// <summary>
/// Интерфейс для фабрики создания обработчиков клиентов.
/// </summary>
public interface IClientHandlerFactory
{
    /// <summary>
    /// Создает обработчик клиента.
    /// </summary>
    /// <param name="tcpClient">TCP-клиент для связи.</param>
    /// <param name="server">Ссылка на сервер.</param>
    /// <returns>Экземпляр обработчика клиента.</returns>
    IClientHandler CreateClientHandler(TcpClient tcpClient, IChatServer server);
}