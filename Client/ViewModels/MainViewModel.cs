using System.Collections.ObjectModel;
using System.Windows.Input;
using Client.Models;
using Client.Services;
using Client.Utilities;

namespace Client.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IChatService _chatService;

    public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

    private string _newMessage;
    public string NewMessage
    {
        get => _newMessage;
        set => SetProperty(ref _newMessage, value);
    }

    public ICommand SendMessageCommand { get; }

    public MainViewModel(IChatService chatService)
    {
        _chatService = chatService;

        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), CanSendMessage);

        _chatService.MessageReceived += OnMessageReceived;

        _chatService.Connect();
    }

    private void OnMessageReceived(object sender, ChatMessage message)
    {
        message.IsSentByUser = false;

        App.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(message);
        });
    }

    private async Task SendMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewMessage))
        {
            var message = new ChatMessage
            {
                Content = NewMessage,
                Timestamp = DateTime.Now,
                IsSentByUser = true
            };

            Messages.Add(message);

            await _chatService.SendMessageAsync(NewMessage);
            NewMessage = string.Empty;
        }
    }

    private bool CanSendMessage()
    {
        return !string.IsNullOrWhiteSpace(NewMessage);
    }
}
