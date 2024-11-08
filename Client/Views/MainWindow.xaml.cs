using Client.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace Client.Views;

/// <summary>
/// Основное окно приложения чата. Обрабатывает взаимодействие пользователя с интерфейсом и связывает его с логикой в ViewModel.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += MainWindow_DataContextChanged;
    }

    /// <summary>
    /// Обработчик изменения контекста данных (DataContext).
    /// Подписывается на события коллекции сообщений в ViewModel.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения свойства.</param>
    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ChatViewModel oldViewModel)
        {
            // Отписка от событий старого ViewModel для предотвращения утечек памяти
            oldViewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
            oldViewModel.SendMessageCommand.CanExecuteChanged -= SendMessageCommand_CanExecuteChanged;
        }

        if (DataContext is ChatViewModel viewModel)
        {
            // Подписка на изменения в коллекции Messages
            viewModel.Messages.CollectionChanged += Messages_CollectionChanged;

            // Подписка на изменения состояния команды SendMessageCommand
            viewModel.SendMessageCommand.CanExecuteChanged += SendMessageCommand_CanExecuteChanged;
        }
    }

    /// <summary>
    /// Обработчик события изменения коллекции сообщений. Автоматически прокручивает список сообщений при добавлении новых.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения коллекции.</param>
    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (MessagesListBox != null)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewStartingIndex == 0)
                    {
                        // Прокрутка вверх при подгрузке истории
                        MessagesListBox.ScrollIntoView(MessagesListBox.Items[0]);
                    }
                    else if (e.NewStartingIndex == MessagesListBox.Items.Count - 1)
                    {
                        // Прокрутка вниз при добавлении нового сообщения
                        MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// Обработчик события изменения состояния команды <see cref="ChatViewModel.SendMessageCommand"/>.
    /// Обновляет интерфейс в зависимости от возможности выполнения команды.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события.</param>
    private void SendMessageCommand_CanExecuteChanged(object sender, EventArgs e)
    {
        // Здесь можно добавить логику, если требуется обновлять UI при изменении состояния команды
    }

    /// <summary>
    /// Обработчик нажатия клавиш в текстовом поле для ввода сообщений.
    /// Отправляет сообщение при нажатии клавиши Enter.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события нажатия клавиши.</param>
    private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ChatViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
            {
                viewModel.SendMessageCommand.Execute(null);
            }

            e.Handled = true;
        }
    }
}