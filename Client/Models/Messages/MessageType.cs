namespace Client.Models.Messages;

/// <summary>
/// Перечисление, определяющее типы сообщений, используемых в системе чата.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Тип сообщения: обычное текстовое сообщение чата.
    /// </summary>
    ChatMessage,

    /// <summary>
    /// Тип сообщения: запрос на получение истории чата.
    /// </summary>
    HistoryRequest,

    /// <summary>
    /// Тип сообщения: ответ с историей чата.
    /// </summary>
    HistoryResponse
}