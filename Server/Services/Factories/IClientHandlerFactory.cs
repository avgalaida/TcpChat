using System.Net.Sockets;
using Server.Services.Client;
using Server.Services.Server;

namespace Server.Services.Factories;

public interface IClientHandlerFactory
{
    IClientHandler CreateClientHandler(TcpClient tcpClient, IChatServer server);
}