using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models.Messages;

namespace Server.Repository;

/// <summary>
/// Реализация репозитория для работы с сообщениями.
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MessageRepository"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных для работы с сообщениями.</param>
    public MessageRepository(ChatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Сохраняет исходящее сообщение в базе данных.
    /// </summary>
    /// <param name="message">Сообщение для сохранения.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task SaveMessageAsync(OutgoingChatMessage message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Получает список исходящих сообщений с пагинацией.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <returns>Задача, представляющая асинхронную операцию, возвращающая список сообщений.</returns>
    public async Task<List<OutgoingChatMessage>> GetMessagesAsync(int page, int pageSize)
    {
        return await _context.Messages
            .OrderByDescending(m => m.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Подсчитывает общее количество сообщений в базе данных.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию, возвращающая количество сообщений.</returns>
    public async Task<int> CountAsync()
    {
        return await _context.Messages.CountAsync();
    }
}