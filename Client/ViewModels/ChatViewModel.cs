using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.Models.Messages;
using Client.Services;
using Client.Utilities;

namespace Client.ViewModels;
public class ChatViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private ObservableCollection<DisplayChatMessage> _messages;
    private string _newMessage;
    private bool _isConnected;
    private string _connectionStatus;
    private int _currentPage = 1;
    private const int PageSize = 10;

    public ObservableCollection<DisplayChatMessage> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    public string NewMessage
    {
        get => _newMessage;
        set => SetProperty(ref _newMessage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ReconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RequestHistoryCommand).RaiseCanExecuteChanged();
                ConnectionStatus = value ? "Подключен" : "Отключен";
            }
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public ICommand SendMessageCommand { get; }
    public ICommand ReconnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand RequestHistoryCommand { get; }

    public ChatViewModel(IChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        Messages = new ObservableCollection<DisplayChatMessage>();
        ConnectionStatus = "Отключен";

        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), () => IsConnected && !string.IsNullOrEmpty(NewMessage));
        ReconnectCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected);
        DisconnectCommand = new RelayCommand(async () => await DisconnectAsync(), () => IsConnected);
        RequestHistoryCommand = new RelayCommand(async () => await RequestHistoryAsync(), () => IsConnected);

        _chatService.MessageReceived += OnMessageReceived;
        _chatService.HistoryReceived += OnHistoryReceived;
    }

    private async Task ConnectAsync()
    {
        try
        {
            if (await _chatService.ConnectAsync())
            {
                IsConnected = true;
                _currentPage = 1; // Сброс текущей страницы при новом подключении
                Messages.Clear();
                AddSystemMessage("Успешно подключено к серверу.");
            }
            else
            {
                ShowErrorMessage("Не удалось подключиться к серверу. Проверьте соединение.");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Ошибка подключения: {ex.Message}");
        }
    }

    private async Task DisconnectAsync()
    {
        try
        {
            await _chatService.DisconnectAsync();
            IsConnected = false;
            AddSystemMessage("Отключено от сервера.");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Ошибка отключения: {ex.Message}");
        }
    }

    private async Task SendMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewMessage) && IsConnected)
        {
            var displayMessage = new DisplayChatMessage
            {
                Sender = _chatService.GetFormattedLocalEndPoint(),
                Content = NewMessage,
                Timestamp = DateTime.Now,
                IsSentByUser = true
            };
            Messages.Add(displayMessage);

            try
            {
                await _chatService.SendChatMessageAsync(NewMessage);
                NewMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Не удалось отправить сообщение: {ex.Message}");
            }
        }
    }

    private async Task RequestHistoryAsync()
    {
        if (IsConnected)
        {
            try
            {
                await _chatService.RequestHistoryAsync(_currentPage, PageSize);
                _currentPage++; // Увеличиваем номер текущей страницы после запроса
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка запроса истории: {ex.Message}");
            }
        }
    }

    private void OnMessageReceived(object sender, IncomingChatMessage serverMessage)
    {
        var isSentByUser = IsMessageSentByUser(serverMessage);
        var displayMessage = new DisplayChatMessage
        {
            Sender = isSentByUser ? "Вы" : $"{serverMessage.SenderIp}:{serverMessage.SenderPort}",
            Content = serverMessage.Content,
            Timestamp = serverMessage.Timestamp,
            IsSentByUser = isSentByUser
        };

        Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(displayMessage);
        });
    }



    private void OnHistoryReceived(object sender, HistoryResponse history)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var msg in history.Messages)
            {
                var isSentByUser = IsMessageSentByUser(msg);
                var displayMessage = new DisplayChatMessage
                {
                    Sender = isSentByUser ? "Вы" : $"{msg.SenderIp}:{msg.SenderPort}",
                    Content = msg.Content,
                    Timestamp = msg.Timestamp,
                    IsSentByUser = isSentByUser
                };
                Messages.Insert(0, displayMessage);
            }
        });
    }

    private bool IsMessageSentByUser(IncomingChatMessage message)
    {
        return _chatService.LocalEndPoint != null &&
                message.SenderIp == _chatService.LocalEndPoint.Address.ToString() &&
                message.SenderPort == _chatService.LocalEndPoint.Port;
    }

    private void AddSystemMessage(string content)
    {
        var systemMessage = new DisplayChatMessage
        {
            Sender = "Система",
            Content = content,
            Timestamp = DateTime.Now,
            IsSentByUser = false
        };
        Application.Current.Dispatcher.Invoke(() => Messages.Add(systemMessage));
    }

    private void ShowErrorMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}