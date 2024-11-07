using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Server;

/// <summary>
/// Интерфейс для реализации сервера чата.
/// </summary>
public interface IChatServer
{
    /// <summary>
    /// Асинхронно запускает сервер.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию запуска сервера.</returns>
    Task StartAsync();

    /// <summary>
    /// Добавляет клиента к серверу.
    /// </summary>
    /// <param name="client">Обработчик клиента.</param>
    void AddClient(IClientHandler client);

    /// <summary>
    /// Удаляет клиента с сервера.
    /// </summary>
    /// <param name="client">Обработчик клиента.</param>
    void RemoveClient(IClientHandler client);

    /// <summary>
    /// Асинхронно рассылает сообщение всем подключенным клиентам, кроме отправителя.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <returns>Задача, представляющая асинхронную операцию рассылки сообщения.</returns>
    Task BroadcastMessageAsync(OutgoingChatMessage message);
}