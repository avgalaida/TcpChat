namespace Client.Models.Messages;

/// <summary>
/// Сообщение-запрос на получение истории чата.
/// Содержит информацию о странице и размере страницы для пагинации истории сообщений.
/// </summary>
public class HistoryRequest : BaseMessage
{
    /// <summary>
    /// Номер страницы истории, которую необходимо получить.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Количество сообщений на странице истории.
    /// </summary>
    public int PageSize { get; set; }
}
