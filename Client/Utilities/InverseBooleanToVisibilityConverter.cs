using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Utilities;

/// <summary>
/// Конвертер для инверсии значения типа <see cref="bool"/> в <see cref="Visibility"/>.
/// </summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Единственный экземпляр конвертера.
    /// </summary>
    public static readonly InverseBooleanToVisibilityConverter Instance = new InverseBooleanToVisibilityConverter();

    // Закрытый конструктор для реализации паттерна Singleton
    private InverseBooleanToVisibilityConverter() { }

    /// <summary>
    /// Преобразует значение типа <see cref="bool"/> в <see cref="Visibility"/> с инверсией.
    /// </summary>
    /// <param name="value">Значение для преобразования.</param>
    /// <param name="targetType">Целевой тип.</param>
    /// <param name="parameter">Параметр конвертации.</param>
    /// <param name="culture">Информация о культуре.</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/>, если <paramref name="value"/> равно <c>false</c> или <c>null</c>;
    /// <see cref="Visibility.Collapsed"/> в противном случае.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return boolean ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    /// <summary>
    /// Преобразует значение <see cref="Visibility"/> обратно в <see cref="bool"/> с инверсией.
    /// </summary>
    /// <param name="value">Значение для преобразования.</param>
    /// <param name="targetType">Целевой тип.</param>
    /// <param name="parameter">Параметр конвертации.</param>
    /// <param name="culture">Информация о культуре.</param>
    /// <returns>
    /// <c>true</c>, если <paramref name="value"/> равно <see cref="Visibility.Collapsed"/> или <see cref="Visibility.Hidden"/>;
    /// <c>false</c> в противном случае.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return false;
    }
}
