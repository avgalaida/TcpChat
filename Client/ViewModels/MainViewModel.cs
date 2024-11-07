using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Client.Models.Messages;
using Client.Models;
using Client.Services;
using Client.Utilities;

namespace Client.ViewModels;
public class MainViewModel : ViewModelBase
{
    private readonly IChatService _chatService;

    public ObservableCollection<DisplayChatMessage> Messages { get; } = new ObservableCollection<DisplayChatMessage>();

    private string _newMessage;
    public string NewMessage
    {
        get => _newMessage;
        set
        {
            if (SetProperty(ref _newMessage, value))
            {
                // Обновляем доступность команды отправки сообщения
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (SetProperty(ref _isConnected, value))
            {
                // Обновляем доступность команд подключения и отключения
                ((RelayCommand)ReconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }
    }

    private string _connectionStatus;
    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set => SetProperty(ref _connectionStatus, value);
    }

    public ICommand SendMessageCommand { get; }
    public ICommand ReconnectCommand { get; }
    public ICommand DisconnectCommand { get; }

    public MainViewModel(IChatService chatService)
    {
        _chatService = chatService;

        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), CanSendMessage);
        ReconnectCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected);
        DisconnectCommand = new RelayCommand(async () => await DisconnectAsync(), () => IsConnected);

        _chatService.MessageReceived += OnMessageReceived;
        _chatService.HistoryReceived += OnHistoryReceived;

        // Инициализируем состояние подключения
        IsConnected = false;
        ConnectionStatus = "Отключено";
    }

    public async Task ConnectAsync()
    {
        try
        {
            var connected = await _chatService.ConnectAsync();
            if (connected)
            {
                IsConnected = true;
                ConnectionStatus = "Подключено";
                AddSystemMessage("Успешно подключено к серверу.");
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к серверу. Проверьте соединение и попробуйте снова.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _chatService.DisconnectAsync();
            IsConnected = false;
            ConnectionStatus = "Отключено";
            AddSystemMessage("Отключено от сервера.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка отключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SendMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewMessage) && IsConnected)
        {
            // Добавляем сообщение в коллекцию с IsSentByUser = true
            var displayMessage = new DisplayChatMessage
            {
                Sender = "Вы",
                Content = NewMessage,
                Timestamp = DateTime.Now,
                IsSentByUser = true
            };
            Messages.Add(displayMessage);

            try
            {
                // Отправляем сообщение через сервис
                await _chatService.SendChatMessageAsync(NewMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось отправить сообщение: {ex.Message}", "Ошибка отправки", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NewMessage = string.Empty;
        }
    }

    private bool CanSendMessage()
    {
        return IsConnected && !string.IsNullOrWhiteSpace(NewMessage);
    }

    private void OnMessageReceived(object sender, ServerChatMessage serverMessage)
    {
        // Определяем, является ли сообщение отправленным самим собой
        bool isSentByUser = false;
        if (_chatService.LocalEndPoint != null)
        {
            isSentByUser = serverMessage.SenderIp == _chatService.LocalEndPoint.Address.ToString() &&
                            serverMessage.SenderPort == _chatService.LocalEndPoint.Port;
        }

        // Создаём DisplayChatMessage для отображения в UI
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
                bool isSentByUser = false;
                if (_chatService.LocalEndPoint != null)
                {
                    isSentByUser = msg.SenderIp == _chatService.LocalEndPoint.Address.ToString() &&
                                    msg.SenderPort == _chatService.LocalEndPoint.Port;
                }

                var displayMessage = new DisplayChatMessage
                {
                    Sender = isSentByUser ? "Вы" : $"{msg.SenderIp}:{msg.SenderPort}",
                    Content = msg.Content,
                    Timestamp = msg.Timestamp,
                    IsSentByUser = isSentByUser
                };
                Messages.Add(displayMessage);
            }
        });
    }

    private void AddSystemMessage(string content)
    {
        Messages.Add(new DisplayChatMessage
        {
            Sender = "Система",
            Content = content,
            Timestamp = DateTime.Now,
            IsSentByUser = false
        });
    }
}
