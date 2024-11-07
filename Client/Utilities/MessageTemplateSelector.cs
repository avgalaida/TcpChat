using System.Windows.Controls;
using System.Windows;
using Client.Models;

namespace Client.Utilities;

public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate SentMessageTemplate { get; set; }
    public DataTemplate ReceivedMessageTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is DisplayChatMessage message)
        {
            return message.IsSentByUser ? SentMessageTemplate : ReceivedMessageTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}