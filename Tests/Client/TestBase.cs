// Подключение пространства имён System.Windows, необходимого для работы с WPF Application.
using System.Windows;

namespace Tests.Client;

/// <summary>
/// Базовый класс для всех тестовых классов, связанных с ViewModel.
/// 
/// Этот класс обеспечивает инициализацию WPF Application, если оно ещё не инициализировано.
/// Инициализация Application необходима для корректной работы WPF-компонентов в тестовом окружении.
/// Без установленного Application.Current многие WPF-функции, такие как Dispatcher, могут не работать корректно,
/// что приведёт к ошибкам или некорректному поведению во время выполнения тестов.
/// </summary>
public class TestBase
{
    /// <summary>
    /// Конструктор класса TestBase.
    /// 
    /// Проверяет, инициализировано ли свойство Application.Current.
    /// Если оно равно null, создаёт новый экземпляр Application, что автоматически устанавливает Application.Current.
    /// Это гарантирует, что WPF-инфраструктура доступна во время выполнения тестов.
    /// </summary>
    public TestBase()
    {
        if (Application.Current == null)
        {
            _ = new Application();
        }
    }
}