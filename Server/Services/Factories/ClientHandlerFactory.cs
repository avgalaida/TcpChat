using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Server.Services.Client;
using Server.Services.Server;

namespace Server.Services.Factories;

public class ClientHandlerFactory : IClientHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ClientHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IClientHandler CreateClientHandler(TcpClient tcpClient, IChatServer server)
    {
        return ActivatorUtilities.CreateInstance<ClientHandler>(_serviceProvider, tcpClient, server);
    }
}