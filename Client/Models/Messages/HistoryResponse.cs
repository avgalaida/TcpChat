namespace Client.Models.Messages;

/// <summary>
/// Сообщение-ответ, содержащее историю чата.
/// Включает общее количество сообщений, номер текущей страницы, размер страницы и список сообщений.
/// </summary>
public class HistoryResponse : BaseMessage
{
    /// <summary>
    /// Общее количество сообщений в истории чата.
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// Номер страницы истории, которая была получена.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Количество сообщений на странице истории.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Список сообщений, содержащихся на текущей странице истории.
    /// </summary>
    public List<IncomingChatMessage> Messages { get; set; }
}