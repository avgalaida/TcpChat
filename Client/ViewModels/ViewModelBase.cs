using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.ViewModels;

/// <summary>
/// Базовый класс для ViewModel, реализующий интерфейс <see cref="INotifyPropertyChanged"/>.
/// Предоставляет механизм уведомления об изменении свойств для связывания данных.
/// </summary>
public class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Вызывает событие <see cref="PropertyChanged"/>, уведомляя систему о том, что значение свойства изменилось.
    /// </summary>
    /// <param name="propertyName">Имя свойства, значение которого изменилось. Автоматически определяется, если не указано.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Устанавливает значение поля и вызывает уведомление об изменении свойства, если новое значение отличается от старого.
    /// </summary>
    /// <typeparam name="T">Тип свойства.</typeparam>
    /// <param name="field">Ссылка на поле, хранящее значение свойства.</param>
    /// <param name="value">Новое значение свойства.</param>
    /// <param name="propertyName">Имя свойства. Автоматически определяется, если не указано.</param>
    /// <returns><c>true</c>, если значение изменилось; иначе <c>false</c>.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}