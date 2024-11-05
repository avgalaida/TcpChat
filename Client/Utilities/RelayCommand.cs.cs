using System.Windows.Input;

namespace Client.Utilities;
public class RelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    private bool _isExecuting;

    // Конструктор для асинхронных команд
    public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    // Конструктор для синхронных команд
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        if (_isExecuting)
            return false;

        return _canExecute == null || _canExecute();
    }

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
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}