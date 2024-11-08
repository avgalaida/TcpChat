using System.Collections.ObjectModel;
using System.Reflection;
using Client.Models;
using Client.Services;
using Client.ViewModels;
using FluentAssertions;
using Moq;

namespace Tests.Client;

/// <summary>
/// Набор тестов для ChatViewModel.
/// </summary>
public class ChatViewModelTests : TestBase
{
    private readonly Mock<IChatService> _chatServiceMock;
    private readonly ChatViewModel _viewModel;

    /// <summary>
    /// Инициализация моков и ChatViewModel.
    /// </summary>
    public ChatViewModelTests()
    {
        _chatServiceMock = new Mock<IChatService>();
        _viewModel = new ChatViewModel(_chatServiceMock.Object);
    }

    /// <summary>
    /// Проверяет, что конструктор инициализирует все свойства правильно.
    /// </summary>
    [StaFact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act

        // Assert
        _viewModel.Messages.Should().BeOfType<ObservableCollection<DisplayChatMessage>>();
        _viewModel.Messages.Should().BeEmpty();
        _viewModel.IsConnected.Should().BeFalse();
        _viewModel.ConnectionStatus.Should().Be("Отключен");
        _viewModel.NewMessage.Should().BeNull();
    }

    /// <summary>
    /// Проверяет успешное подключение и обновление свойств.
    /// </summary>
    [StaFact]
    public async Task ConnectAsync_ShouldSetIsConnectedToTrue_WhenConnectionIsSuccessful()
    {
        // Arrange
        _chatServiceMock.Setup(s => s.ConnectAsync()).ReturnsAsync(true);

        // Act
        await _viewModel.ConnectAsync();

        // Assert
        _viewModel.IsConnected.Should().BeTrue();
        _viewModel.ConnectionStatus.Should().Be("Подключен");
        _chatServiceMock.Verify(s => s.ConnectAsync(), Times.Once);
    }

    /// <summary>
    /// Проверяет неуспешное подключение и обновление свойств.
    /// </summary>
    [StaFact]
    public async Task ConnectAsync_ShouldSetIsConnectedToFalse_WhenConnectionFails()
    {
        // Arrange
        _chatServiceMock.Setup(s => s.ConnectAsync()).ReturnsAsync(false);

        // Act
        await _viewModel.ConnectAsync();

        // Assert
        _viewModel.IsConnected.Should().BeFalse();
        _viewModel.ConnectionStatus.Should().Be("Отключен");
        _chatServiceMock.Verify(s => s.ConnectAsync(), Times.Once);
    }

    /// <summary>
    /// Проверяет успешное отключение и обновление свойств.
    /// </summary>
    [StaFact]
    public async Task DisconnectAsync_ShouldSetIsConnectedToFalse_WhenDisconnectionIsSuccessful()
    {
        // Arrange
        _chatServiceMock.Setup(s => s.DisconnectAsync()).Returns(Task.CompletedTask);
        _viewModel.IsConnected = true;
        _viewModel.ConnectionStatus = "Подключен";

        // Act
        await _viewModel.DisconnectAsync();
         
        // Assert
        _viewModel.IsConnected.Should().BeFalse();
        _viewModel.ConnectionStatus.Should().Be("Отключен");
        _chatServiceMock.Verify(s => s.DisconnectAsync(), Times.Once);
    }

    /// <summary>
    /// Проверяет отправку сообщения при валидном сообщении и подключении.
    /// </summary>
    [StaFact]
    public async Task SendMessageAsync_ShouldAddMessageToMessages_WhenMessageIsValidAndConnected()
    {
        // Arrange
        _viewModel.IsConnected = true;
        _viewModel.NewMessage = "Hello World";
        _chatServiceMock.Setup(s => s.SendChatMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _chatServiceMock.Setup(s => s.GetFormattedLocalEndPoint()).Returns("You");

        // Act
        await _viewModel.SendMessageAsync();

        // Assert
        _viewModel.Messages.Should().ContainSingle();
        var message = _viewModel.Messages[0];
        message.Content.Should().Be("Hello World");
        message.Sender.Should().Be("You");
        message.IsSentByUser.Should().BeTrue();
        _viewModel.NewMessage.Should().BeEmpty();
        _chatServiceMock.Verify(s => s.SendChatMessageAsync("Hello World"), Times.Once);
    }

    /// <summary>
    /// Проверяет, что сообщение не добавляется, если не подключены.
    /// </summary>
    [StaFact]
    public async Task SendMessageAsync_ShouldNotAddMessage_WhenNotConnected()
    {
        // Arrange
        _viewModel.IsConnected = false;
        _viewModel.NewMessage = "Hello World";

        // Act
        await _viewModel.SendMessageAsync();

        // Assert
        _viewModel.Messages.Should().BeEmpty();
        _chatServiceMock.Verify(s => s.SendChatMessageAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что сообщение не добавляется, если оно пустое.
    /// </summary>
    [StaFact]
    public async Task SendMessageAsync_ShouldNotAddMessage_WhenMessageIsEmpty()
    {
        // Arrange
        _viewModel.IsConnected = true;
        _viewModel.NewMessage = string.Empty;

        // Act
        await _viewModel.SendMessageAsync();
         
        // Assert
        _viewModel.Messages.Should().BeEmpty();
        _chatServiceMock.Verify(s => s.SendChatMessageAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что текущая страница истории увеличивается после запроса истории.
    /// </summary>
    [StaFact]
    public async Task RequestHistoryAsync_ShouldIncrementCurrentPage_AfterRequestingHistory()
    {
        // Arrange
        _viewModel.IsConnected = true;
        _chatServiceMock.Setup(s => s.RequestHistoryAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var initialPage = _viewModel.GetType().GetField("_currentPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_viewModel);
        initialPage.Should().Be(1);

        // Act
        await _viewModel.RequestHistoryAsync();

        // Assert
        var updatedPage = _viewModel.GetType().GetField("_currentPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_viewModel);
        updatedPage.Should().Be(2);
        _chatServiceMock.Verify(s => s.RequestHistoryAsync(1, 10), Times.Once);
    }

    /// <summary>
    /// Проверяет обработку исключения при подключении.
    /// </summary>
    [StaFact]
    public async Task ConnectAsync_ShouldHandleException_WhenServiceThrows()
    {
        // Arrange
        var exception = new Exception("Connection failed");
        _chatServiceMock.Setup(s => s.ConnectAsync()).ThrowsAsync(exception);

        // Act
        await _viewModel.ConnectAsync();

        // Assert
        _viewModel.IsConnected.Should().BeFalse();
        _viewModel.ConnectionStatus.Should().Be("Отключен");
        _chatServiceMock.Verify(s => s.ConnectAsync(), Times.Once);
    }

    /// <summary>
    /// Проверяет, что изменение свойства NewMessage вызывает событие PropertyChanged.
    /// </summary>
    [StaFact]
    public void NewMessage_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.NewMessage))
                propertyChangedRaised = true;
        };

        // Act
        _viewModel.NewMessage = "Test Message"; 

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что сообщение не добавляется, если оно невалидно (null, пусто, пробелы).
    /// </summary>
    [StaTheory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessageAsync_ShouldNotAddMessage_WhenMessageIsInvalid(string message)
    {
        // Arrange
        _viewModel.IsConnected = true;
        _viewModel.NewMessage = message;

        // Act
        await _viewModel.SendMessageAsync();   

        // Assert
        _viewModel.Messages.Should().BeEmpty();
        _chatServiceMock.Verify(s => s.SendChatMessageAsync(It.IsAny<string>()), Times.Never);
    }
}
