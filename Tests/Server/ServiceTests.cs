using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Models.Messages;
using Server.Services.Client;
using Server.Services.Factories;
using Server.Services.Server;

namespace Tests.Server;

/// <summary>
/// Набор тестов для проверки поведения сервисов, связанных с сервером чата.
/// </summary>
public class ServiceTests
{
    private readonly Mock<IClientHandlerFactory> _mockClientHandlerFactory;
    private readonly Mock<ILogger<ChatServer>> _mockLogger;
    private readonly Mock<IClientHandler> _mockClientHandler;
    private readonly ChatServer _chatServer;

    /// <summary>
    /// Инициализация тестового окружения с моками и конфигурацией.
    /// </summary>
    public ServiceTests()
    {
        _mockClientHandlerFactory = new Mock<IClientHandlerFactory>();
        _mockLogger = new Mock<ILogger<ChatServer>>();
        _mockClientHandler = new Mock<IClientHandler>();
        _mockClientHandler.SetupGet(c => c.ClientId).Returns("123");
        _mockClientHandler.SetupGet(c => c.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Loopback, 5000));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ServerSettings:Port", "12345" }
            })
            .Build();

        _chatServer = new ChatServer(_mockClientHandlerFactory.Object, _mockLogger.Object, configuration);
    }

    /// <summary>
    /// Тест на добавление клиента к серверу и проверку логирования.
    /// </summary>
    [Fact]
    public void AddClient_ShouldAddClient()
    {
        // Act
        _chatServer.AddClient(_mockClientHandler.Object);

        // Assert
        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Клиент добавлен")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Тест на проверку, что сообщение отправляется всем подключенным клиентам, кроме отправителя.
    /// </summary>
    [Fact]
    public async Task BroadcastMessageAsync_ShouldSendMessagesToAllClientsExceptSender()
    {
        // Arrange
        _mockClientHandler.SetupGet(c => c.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333));
        _chatServer.AddClient(_mockClientHandler.Object);

        var message = new OutgoingChatMessage
        {
            SenderIp = "127.0.0.1",
            SenderPort = 3333,
            Content = "Test message"
        };

        // Act
        await _chatServer.BroadcastMessageAsync(message);

        // Assert
        _mockClientHandler.Verify(c => c.SendMessageAsync(It.IsAny<OutgoingChatMessage>()), Times.Never);
    }

    /// <summary>
    /// Тест на удаление клиента с сервера и проверку логирования.
    /// </summary>
    [Fact]
    public void RemoveClient_ShouldRemoveClient()
    {
        // Arrange
        _chatServer.AddClient(_mockClientHandler.Object);

        // Act
        _chatServer.RemoveClient(_mockClientHandler.Object);

        // Assert
        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Клиент удален")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    /// <summary>
    /// Тест на проверку логирования при запуске и остановке сервера.
    /// </summary>
    [Fact]
    public async Task StartAsync_ShouldLogInformation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await _chatServer.StartAsync(cts.Token);

        // Assert
        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Сервер запущен")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Сервер остановлен")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}