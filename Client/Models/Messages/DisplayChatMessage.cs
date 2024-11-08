namespace Client.Models;

/// <summary>
/// Представляет отображаемое сообщение чата в пользовательском интерфейсе.
/// Содержит информацию о отправителе, содержимом сообщения, времени отправки и статусе отправки.
/// </summary>
public class DisplayChatMessage
{
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
    /// Указывает, было ли сообщение отправлено текущим пользователем.
    /// </summary>
    public bool IsSentByUser { get; set; }
}