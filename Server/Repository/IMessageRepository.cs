using Server.Models.Messages;

namespace Server.Repository;

/// <summary>
/// Интерфейс для репозитория сообщений.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Асинхронно сохраняет исходящее сообщение в хранилище.
    /// </summary>
    /// <param name="message">Сообщение для сохранения.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task SaveMessageAsync(OutgoingChatMessage message);

    /// <summary>
    /// Асинхронно получает список исходящих сообщений с пагинацией.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <returns>Задача, представляющая асинхронную операцию, возвращающая список сообщений.</returns>
    Task<List<OutgoingChatMessage>> GetMessagesAsync(int page, int pageSize);

    /// <summary>
    /// Асинхронно подсчитывает общее количество сообщений.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию, возвращающая количество сообщений.</returns>
    Task<int> CountAsync();
}