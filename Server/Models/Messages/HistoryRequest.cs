namespace Server.Models.Messages;

/// <summary>
/// Класс, представляющий запрос на получение истории сообщений.
/// </summary>
public class HistoryRequest : BaseMessage
{
    /// <summary>
    /// Номер страницы для запроса истории.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы для запроса истории.
    /// </summary>
    public int PageSize { get; set; }
}