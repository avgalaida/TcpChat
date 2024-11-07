namespace Server.Models.Messages;

/// <summary>
/// Класс, представляющий ответ с историей сообщений.
/// </summary>
public class HistoryResponse : BaseMessage
{
    /// <summary>
    /// Общее количество сообщений.
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// Номер страницы с историей сообщений.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы с историей сообщений.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Коллекция сообщений в ответе.
    /// </summary>
    public IEnumerable<OutgoingChatMessage> Messages { get; set; }
}