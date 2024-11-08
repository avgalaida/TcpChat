namespace Client.Models.Messages;

/// <summary>
/// Сообщение чата, полученное от другого пользователя.
/// Содержит информацию о отправителе, содержимом сообщения и времени отправки.
/// </summary>
public class IncomingChatMessage : BaseMessage
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя или идентификатор отправителя сообщения.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// Содержимое сообщения.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Временная метка, указывающая когда сообщение было отправлено.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP-адрес отправителя сообщения.
    /// </summary>
    public string SenderIp { get; set; }

    /// <summary>
    /// Порт отправителя сообщения.
    /// </summary>
    public int SenderPort { get; set; }
}