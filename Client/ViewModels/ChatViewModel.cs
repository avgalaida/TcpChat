using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.Models.Messages;
using Client.Services;
using Client.Utilities;

namespace Client.ViewModels;

/// <summary>
/// ViewModel для управления чатом. Обрабатывает отправку и получение сообщений, управление соединением и запрос историй.
/// </summary>
public class ChatViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private ObservableCollection<DisplayChatMessage> _messages;
    private string _newMessage;
    private bool _isConnected;
    private string _connectionStatus;
    private int _currentPage = 1;
    private const int PageSize = 10;

    /// <summary>
    /// Коллекция сообщений для отображения в интерфейсе.
    /// </summary>
    public ObservableCollection<DisplayChatMessage> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    /// <summary>
    /// Текст нового сообщения, вводимого пользователем.
    /// </summary>
    public string NewMessage
    {
        get => _newMessage;
        set => SetProperty(ref _newMessage, value);
    }

    /// <summary>
    /// Указывает, подключен ли клиент к серверу.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                // Обновляем состояния команд при изменении состояния подключения
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ReconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RequestHistoryCommand).RaiseCanExecuteChanged();
                ConnectionStatus = value ? "Подключен" : "Отключен";
            }
        }
    }

    /// <summary>
    /// Текущий статус подключения для отображения в интерфейсе.
    /// </summary>
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    /// <summary>
    /// Команда для отправки сообщения.
    /// </summary>
    public ICommand SendMessageCommand { get; }

    /// <summary>
    /// Команда для повторного подключения к серверу.
    /// </summary>
    public ICommand ReconnectCommand { get; }

    /// <summary>
    /// Команда для отключения от сервера.
    /// </summary>
    public ICommand DisconnectCommand { get; }

    /// <summary>
    /// Команда для запроса истории сообщений.
    /// </summary>
    public ICommand RequestHistoryCommand { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ChatViewModel"/>.
    /// </summary>
    /// <param name="chatService">Сервис для управления чатом.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="chatService"/> равно <c>null</c>.</exception>
    public ChatViewModel(IChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        Messages = new ObservableCollection<DisplayChatMessage>();
        ConnectionStatus = "Отключен";

        // Инициализация команд
        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), () => IsConnected && !string.IsNullOrEmpty(NewMessage));
        ReconnectCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected);
        DisconnectCommand = new RelayCommand(async () => await DisconnectAsync(), () => IsConnected);
        RequestHistoryCommand = new RelayCommand(async () => await RequestHistoryAsync(), () => IsConnected);

        // Подписка на события сервиса чата
        _chatService.MessageReceived += OnMessageReceived;
        _chatService.HistoryReceived += OnHistoryReceived;
    }

    /// <summary>
    /// Асинхронно устанавливает соединение с сервером чата.
    /// </summary>
    /// <returns>Возвращает <c>true</c>, если соединение успешно установлено; иначе <c>false</c>.</returns>
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

    /// <summary>
    /// Асинхронно отключает соединение от сервера чата.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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

    /// <summary>
    /// Асинхронно отправляет новое сообщение в чат.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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

    /// <summary>
    /// Асинхронно запрашивает историю сообщений с сервера.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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

    /// <summary>
    /// Обработчик события получения нового сообщения от сервера.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="serverMessage">Полученное сообщение от сервера.</param>
    private void OnMessageReceived(object sender, IncomingChatMessage serverMessage)
    {
        var isSentByUser = IsMessageSentByUser(serverMessage);
        var displayMessage = new DisplayChatMessage
        {
            Sender = $"{serverMessage.SenderIp}:{serverMessage.SenderPort}",
            Content = serverMessage.Content,
            Timestamp = serverMessage.Timestamp,
            IsSentByUser = isSentByUser
        };

        // Обновление коллекции сообщений в UI-потоке
        Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(displayMessage);
        });
    }

    /// <summary>
    /// Обработчик события получения истории сообщений от сервера.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="history">Ответ с историей сообщений.</param>
    private void OnHistoryReceived(object sender, HistoryResponse history)
    {
        // Обновление коллекции сообщений в UI-потоке
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var msg in history.Messages)
            {
                var isSentByUser = IsMessageSentByUser(msg);
                var displayMessage = new DisplayChatMessage
                {
                    Sender = $"{msg.SenderIp}:{msg.SenderPort}",
                    Content = msg.Content,
                    Timestamp = msg.Timestamp,
                    IsSentByUser = isSentByUser
                };
                Messages.Insert(0, displayMessage);
            }
        });
    }

    /// <summary>
    /// Определяет, было ли сообщение отправлено пользователем.
    /// </summary>
    /// <param name="message">Сообщение для проверки.</param>
    /// <returns><c>true</c>, если сообщение отправлено пользователем; иначе <c>false</c>.</returns>
    private bool IsMessageSentByUser(IncomingChatMessage message)
    {
        return _chatService.LocalEndPoint != null &&
                message.SenderIp == _chatService.LocalEndPoint.Address.ToString() &&
                message.SenderPort == _chatService.LocalEndPoint.Port;
    }

    /// <summary>
    /// Добавляет системное сообщение в чат.
    /// </summary>
    /// <param name="content">Содержимое системного сообщения.</param>
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

    /// <summary>
    /// Отображает сообщение об ошибке пользователю.
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке.</param>
    private void ShowErrorMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}
