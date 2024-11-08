using System.Windows;
using System.Windows.Controls;
using Client.Models;

namespace Client.Utilities;

/// <summary>
/// Селектор шаблонов для выбора соответствующего <see cref="DataTemplate"/> в зависимости от типа сообщения.
/// </summary>
public class MessageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Шаблон для отправленных сообщений.
    /// </summary>
    public DataTemplate SentMessageTemplate { get; set; }

    /// <summary>
    /// Шаблон для полученных сообщений.
    /// </summary>
    public DataTemplate ReceivedMessageTemplate { get; set; }

    /// <summary>
    /// Выбирает соответствующий шаблон на основе типа сообщения.
    /// </summary>
    /// <param name="item">Данные элемента.</param>
    /// <param name="container">Элемент контейнера.</param>
    /// <returns>Выбранный <see cref="DataTemplate"/> или базовый шаблон, если не найдено соответствие.</returns>
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is DisplayChatMessage message)
        {
            if (message.IsSentByUser)
            {
                if (SentMessageTemplate == null)
                {
                    throw new InvalidOperationException($"{nameof(SentMessageTemplate)} не установлен.");
                }
                return SentMessageTemplate;
            }
            else
            {
                if (ReceivedMessageTemplate == null)
                {
                    throw new InvalidOperationException($"{nameof(ReceivedMessageTemplate)} не установлен.");
                }
                return ReceivedMessageTemplate;
            }
        }

        return base.SelectTemplate(item, container);
    }
}
