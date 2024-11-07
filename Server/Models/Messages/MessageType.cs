namespace Server.Models.Messages;

/// <summary>
/// Перечисление типов сообщений.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Сообщение чата.
    /// </summary>
    ChatMessage,

    /// <summary>
    /// Запрос истории сообщений.
    /// </summary>
    HistoryRequest,

    /// <summary>
    /// Ответ с историей сообщений.
    /// </summary>
    HistoryResponse
}