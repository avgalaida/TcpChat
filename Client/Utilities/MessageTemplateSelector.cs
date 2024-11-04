using Client.Models;
using System.Windows.Controls;
using System.Windows;

namespace Client.Utilities;

public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate SentMessageTemplate { get; set; }
    public DataTemplate ReceivedMessageTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ChatMessage message)
        {
            return message.IsSentByUser ? SentMessageTemplate : ReceivedMessageTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}