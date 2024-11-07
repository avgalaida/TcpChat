namespace Server.Models.Messages;

/// <summary>
/// Класс, представляющий исходящее сообщение чата.
/// </summary>
public class OutgoingChatMessage : BaseMessage
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя отправителя сообщения.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// Содержимое сообщения.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Метка времени отправки сообщения.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP-адрес отправителя.
    /// </summary>
    public string SenderIp { get; set; }

    /// <summary>
    /// Порт отправителя.
    /// </summary>
    public int SenderPort { get; set; }
}