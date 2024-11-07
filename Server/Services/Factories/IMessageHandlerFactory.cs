using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Factories;

/// <summary>
/// Интерфейс для фабрики обработки сообщений.
/// </summary>
public interface IMessageHandlerFactory
{
    /// <summary>
    /// Асинхронно обрабатывает сообщение.
    /// </summary>
    /// <param name="message">Сообщение для обработки.</param>
    /// <param name="clientHandler">Обработчик клиента, от которого получено сообщение.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки сообщения.</returns>
    Task HandleMessageAsync(BaseMessage message, IClientHandler clientHandler);
}