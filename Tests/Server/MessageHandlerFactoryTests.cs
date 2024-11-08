using Microsoft.Extensions.Logging;
using Moq;
using Server.Models.Messages;
using Server.Services.Client;
using Server.Services.Factories;
using Server.Services.Handlers;

namespace Tests.Server;

/// <summary>
/// Набор тестов для проверки поведения фабрики <see cref="MessageHandlerFactory"/>.
/// </summary>
public class MessageHandlerFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<MessageHandlerFactory>> _mockLogger;
    private readonly Mock<IMessageHandler<IncomingChatMessage>> _mockChatMessageHandler;
    private readonly MessageHandlerFactory _messageHandlerFactory;

    /// <summary>
    /// Инициализация тестового окружения с настройкой моков.
    /// </summary>
    public MessageHandlerFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<MessageHandlerFactory>>();
        _mockChatMessageHandler = new Mock<IMessageHandler<IncomingChatMessage>>();

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IMessageHandler<IncomingChatMessage>)))
            .Returns(_mockChatMessageHandler.Object);

        _messageHandlerFactory = new MessageHandlerFactory(_mockServiceProvider.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Проверяет, что метод <see cref="MessageHandlerFactory.HandleMessageAsync"/> корректно обрабатывает сообщение типа <see cref="IncomingChatMessage"/>.
    /// </summary>
    [Fact]
    public async Task HandleMessageAsync_ShouldHandleIncomingChatMessage()
    {
        // Arrange
        var mockClientHandler = new Mock<IClientHandler>();
        var incomingMessage = new IncomingChatMessage { Content = "Hello" };

        // Act
        await _messageHandlerFactory.HandleMessageAsync(incomingMessage, mockClientHandler.Object);

        // Assert
        _mockChatMessageHandler.Verify(handler => handler.HandleAsync(incomingMessage, mockClientHandler.Object),
            Times.Once);
    }

    /// <summary>
    /// Проверяет, что метод <see cref="MessageHandlerFactory.HandleMessageAsync"/> логирует ошибку при выбросе исключения.
    /// </summary>
    [Fact]
    public async Task HandleMessageAsync_ShouldLogErrorWhenExceptionThrown()
    {
        // Arrange
        var mockClientHandler = new Mock<IClientHandler>();
        var incomingMessage = new IncomingChatMessage { Content = "Test Content" };

        _mockChatMessageHandler
            .Setup(handler => handler.HandleAsync(incomingMessage, mockClientHandler.Object))
            .Throws(new InvalidOperationException("Ошибка при обработке сообщения"));

        // Act
        await _messageHandlerFactory.HandleMessageAsync(incomingMessage, mockClientHandler.Object);

        // Assert
        _mockLogger.Verify(
            log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Ошибка при обработке сообщения")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}