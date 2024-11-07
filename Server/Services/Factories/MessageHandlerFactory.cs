using Server.Models.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Services.Client;
using Server.Services.Handlers;

namespace Server.Services.Factories;

public class MessageHandlerFactory : IMessageHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageHandlerFactory> _logger;

    public MessageHandlerFactory(IServiceProvider serviceProvider, ILogger<MessageHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

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