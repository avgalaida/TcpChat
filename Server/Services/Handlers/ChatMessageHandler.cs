using Server.Models.Messages;
using Server.Repository;
using Microsoft.Extensions.Logging;
using Server.Services.Client;
using Server.Services.Server;

namespace Server.Services.Handlers;
public class ChatMessageHandler : IMessageHandler<IncomingChatMessage>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatServer _chatServer;
    private readonly ILogger<ChatMessageHandler> _logger;

    public ChatMessageHandler(
        IMessageRepository messageRepository,
        IChatServer chatServer,
        ILogger<ChatMessageHandler> logger)
    {
        _messageRepository = messageRepository;
        _chatServer = chatServer;
        _logger = logger;
    }

    public async Task HandleAsync(IncomingChatMessage message, IClientHandler clientHandler)
    {
        try
        {
            _logger.LogInformation($"Получено сообщение от клиента {clientHandler.ClientId}: {message.Content}");

            var remoteEndPoint = clientHandler.RemoteEndPoint;
            var outgoingMessage = new OutgoingChatMessage
            {
                Id = Guid.NewGuid(),
                Type = MessageType.ChatMessage,
                Sender = $"User_{clientHandler.ClientId[..8]}",
                Content = message.Content,
                Timestamp = DateTime.UtcNow,
                SenderIp = remoteEndPoint?.Address.ToString() ?? "Unknown",
                SenderPort = remoteEndPoint?.Port ?? 0
            };

            _logger.LogInformation($"Создано исходящее сообщение: {outgoingMessage.Content}");

            await _messageRepository.SaveMessageAsync(outgoingMessage);
            _logger.LogInformation($"Сообщение от клиента {clientHandler.ClientId} сохранено в базе данных.");

            await _chatServer.BroadcastMessageAsync(outgoingMessage);
            _logger.LogInformation($"Сообщение от клиента {clientHandler.ClientId} разослано другим клиентам.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке сообщения от клиента {clientHandler.ClientId}");
        }
    }
}