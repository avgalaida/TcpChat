using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Services.Client;
using Server.Services.Factories;
using Server.Services.Server;
using Server.Utilities;

namespace Tests.Server;

/// <summary>
/// Набор тестов для проверки поведения фабрики <see cref="ClientHandlerFactory"/>.
/// </summary>
public class ClientHandlerFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IChatServer> _mockChatServer;
    private readonly Mock<ILogger<ClientHandler>> _mockLogger;
    private readonly Mock<IMessageSerializer> _mockMessageSerializer;
    private readonly Mock<IMessageHandlerFactory> _mockMessageHandlerFactory;
    private readonly ClientHandlerFactory _clientHandlerFactory;

    /// <summary>
    /// Инициализация тестового окружения с настройкой моков.
    /// </summary>
    public ClientHandlerFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockChatServer = new Mock<IChatServer>();
        _mockLogger = new Mock<ILogger<ClientHandler>>();
        _mockMessageSerializer = new Mock<IMessageSerializer>();
        _mockMessageHandlerFactory = new Mock<IMessageHandlerFactory>();

        // Настройка возврата зависимостей из IServiceProvider
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<ClientHandler>)))
            .Returns(_mockLogger.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IMessageSerializer)))
            .Returns(_mockMessageSerializer.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IMessageHandlerFactory)))
            .Returns(_mockMessageHandlerFactory.Object);

        _clientHandlerFactory = new ClientHandlerFactory(_mockServiceProvider.Object);
    }

    /// <summary>
    /// Проверяет, что <see cref="ClientHandlerFactory.CreateClientHandler"/> создает экземпляр <see cref="ClientHandler"/>.
    /// </summary>
    [Fact]
    public void CreateClientHandler_ShouldCreateClientHandler()
    {
        // Настройка тестового сервера и клиента
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var tcpClient = new TcpClient();
        tcpClient.Connect(endpoint);

        // Act
        var clientHandler = _clientHandlerFactory.CreateClientHandler(tcpClient, _mockChatServer.Object);

        // Assert
        Assert.NotNull(clientHandler);
        Assert.IsType<ClientHandler>(clientHandler);

        // Завершение
        listener.Stop();
        tcpClient.Close();
    }
}