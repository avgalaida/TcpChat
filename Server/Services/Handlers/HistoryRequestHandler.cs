using Server.Models.Messages;
using Server.Repository;
using Microsoft.Extensions.Logging;
using Server.Services.Client;

namespace Server.Services.Handlers;

/// <summary>
/// Обработчик для запросов истории сообщений.
/// </summary>
public class HistoryRequestHandler : IMessageHandler<HistoryRequest>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<HistoryRequestHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="HistoryRequestHandler"/>.
    /// </summary>
    /// <param name="messageRepository">Репозиторий сообщений.</param>
    /// <param name="logger">Экземпляр логгера для записи логов.</param>
    public HistoryRequestHandler(IMessageRepository messageRepository, ILogger<HistoryRequestHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает запрос истории сообщений асинхронно.
    /// </summary>
    /// <param name="message">Запрос истории сообщений.</param>
    /// <param name="clientHandler">Обработчик клиента, от которого получен запрос.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки запроса.</returns>
    public async Task HandleAsync(HistoryRequest message, IClientHandler clientHandler)
    {
        try
        {
            _logger.LogInformation(
                $"Обрабатывается HistoryRequest от клиента {clientHandler.ClientId}: страница {message.Page}, размер {message.PageSize}");

            var page = Math.Max(1, message.Page);
            var pageSize = Math.Clamp(message.PageSize, 1, 100);

            var messages = await _messageRepository.GetMessagesAsync(page, pageSize);
            var totalMessages = await _messageRepository.CountAsync();

            var historyResponse = new HistoryResponse
            {
                Type = MessageType.HistoryResponse,
                TotalMessages = totalMessages,
                Page = page,
                PageSize = pageSize,
                Messages = messages
            };

            await clientHandler.SendMessageAsync(historyResponse);
            _logger.LogInformation($"История сообщений отправлена клиенту {clientHandler.ClientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке HistoryRequest от клиента {clientHandler.ClientId}");
        }
    }
}