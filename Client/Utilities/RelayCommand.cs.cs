using System.Windows.Input;

namespace Client.Utilities;

/// <summary>
/// Реализация интерфейса <see cref="ICommand"/> для связывания действий в пользовательском интерфейсе с логикой в ViewModel.
/// Поддерживает как синхронные, так и асинхронные команды.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    private bool _isExecuting;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RelayCommand"/> для асинхронных действий.
    /// </summary>
    /// <param name="executeAsync">Асинхронный делегат, выполняющий действие.</param>
    /// <param name="canExecute">Функция, определяющая, может ли команда быть выполнена. По умолчанию <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="executeAsync"/> равно <c>null</c>.</exception>
    public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RelayCommand"/> для синхронных действий.
    /// </summary>
    /// <param name="execute">Делегат, выполняющий действие.</param>
    /// <param name="canExecute">Функция, определяющая, может ли команда быть выполнена. По умолчанию <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="execute"/> равно <c>null</c>.</exception>
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Событие, возникающее при изменении состояния, определяющего, может ли команда быть выполнена.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Определяет, может ли команда быть выполнена в текущем состоянии.
    /// </summary>
    /// <param name="parameter">Параметр команды (не используется).</param>
    /// <returns><c>true</c>, если команда может быть выполнена; иначе <c>false</c>.</returns>
    public bool CanExecute(object parameter)
    {
        if (_isExecuting)
            return false;

        return _canExecute == null || _canExecute();
    }

    /// <summary>
    /// Выполняет команду. Если команда асинхронная, выполняет ее асинхронно.
    /// </summary>
    /// <param name="parameter">Параметр команды (не используется).</param>
    public async void Execute(object parameter)
    {
        if (!CanExecute(parameter))
            return;

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            if (_executeAsync != null)
            {
                await _executeAsync();
            }
            else
            {
                _execute();
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Вызывает событие <see cref="CanExecuteChanged"/>, уведомляя систему о необходимости повторной проверки возможности выполнения команды.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
