using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.Services;
using Client.Utilities;

namespace Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;

        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

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
                IPEndPoint localEndPoint = _chatService.LocalEndPoint;
                string senderInfo;

                if (localEndPoint != null)
                {
                    // Преобразуем адрес к IPv4, если он представлен в IPv6-формате с маппингом IPv4
                    IPAddress ipv4Address = localEndPoint.Address.MapToIPv4();
                    senderInfo = $"{ipv4Address}:{localEndPoint.Port}";
                }
                else
                {
                    senderInfo = "Неизвестен";
                }

                var message = new ChatMessage
                {
                    Sender = senderInfo,
                    Content = NewMessage,
                    Timestamp = DateTime.Now,
                    IsSentByUser = true
                };

                Messages.Add(message);

                try
                {
                    await _chatService.SendMessageAsync(NewMessage);
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

        private void OnMessageReceived(object sender, ChatMessage message)
        {
            message.IsSentByUser = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }

        private void AddSystemMessage(string content)
        {
            Messages.Add(new ChatMessage
            {
                Sender = "Система",
                Content = content,
                Timestamp = DateTime.Now,
                IsSentByUser = false
            });
        }

    }
}