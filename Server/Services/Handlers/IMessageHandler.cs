using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Handlers;

/// <summary>
/// Интерфейс обработчика сообщений.
/// </summary>
/// <typeparam name="TMessage">Тип сообщения, наследующийся от <see cref="BaseMessage"/>.</typeparam>
public interface IMessageHandler<TMessage> where TMessage : BaseMessage
{
    /// <summary>
    /// Асинхронно обрабатывает сообщение.
    /// </summary>
    /// <param name="message">Сообщение для обработки.</param>
    /// <param name="clientHandler">Обработчик клиента, от которого получено сообщение.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки сообщения.</returns>
    Task HandleAsync(TMessage message, IClientHandler clientHandler);
}