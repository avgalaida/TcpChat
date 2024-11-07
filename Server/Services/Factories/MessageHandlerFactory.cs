using Server.Models.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Services.Client;
using Server.Services.Handlers;

namespace Server.Services.Factories;

/// <summary>
/// Реализация фабрики для обработки сообщений.
/// </summary>
public class MessageHandlerFactory : IMessageHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageHandlerFactory> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MessageHandlerFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">Поставщик служб для создания зависимостей.</param>
    /// <param name="logger">Логгер для записи логов.</param>
    public MessageHandlerFactory(IServiceProvider serviceProvider, ILogger<MessageHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Асинхронно обрабатывает сообщение, используя соответствующий обработчик.
    /// </summary>
    /// <param name="message">Сообщение для обработки.</param>
    /// <param name="clientHandler">Обработчик клиента, от которого получено сообщение.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки сообщения.</returns>
    public async Task HandleMessageAsync(BaseMessage message, IClientHandler clientHandler)
    {
        if (message == null)
            return;

        try
        {
            switch (message)
            {
                case IncomingChatMessage chatMessage:
                    var chatHandler = _serviceProvider.GetRequiredService<IMessageHandler<IncomingChatMessage>>();
                    await chatHandler.HandleAsync(chatMessage, clientHandler);
                    break;
                case HistoryRequest historyRequest:
                    var historyHandler = _serviceProvider.GetRequiredService<IMessageHandler<HistoryRequest>>();
                    await historyHandler.HandleAsync(historyRequest, clientHandler);
                    break;
                default:
                    _logger.LogWarning($"Неизвестный тип сообщения: {message.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке сообщения типа {message.Type}");
        }
    }
}