using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Client.Services;
using Client.ViewModels;
using Client.Utilities;
using Client.Views;

namespace Client;

/// <summary>
/// Основной класс приложения, отвечающий за инициализацию и настройку зависимостей, а также запуск главного окна.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Провайдер сервисов, используемый для разрешения зависимостей.
    /// </summary>
    public IServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// Вызывается при запуске приложения. Настраивает сервисы и отображает главное окно.
    /// </summary>
    /// <param name="e">Аргументы события запуска.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = ServiceProvider.GetRequiredService<ChatViewModel>();
        mainWindow.Show();
    }

    /// <summary>
    /// Конфигурирует сервисы и регистрирует их в контейнере зависимостей.
    /// </summary>
    /// <param name="services">Коллекция сервисов для регистрации.</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // Регистрация ViewModel
        services.AddSingleton<ChatViewModel>();

        // Регистрация окна
        services.AddTransient<MainWindow>();

        // Регистрация сервисов и утилит с передачей параметров
        services.AddSingleton<IChatService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ChatService>>();
            var serializer = provider.GetRequiredService<IMessageSerializer>();
            // Параметры подключения могут быть вынесены в конфигурационный файл или настройки
            return new ChatService("127.0.0.1", 3000, logger, serializer);
        });

        services.AddSingleton<IMessageSerializer, MessageSerializer>();

        // Настройка логгирования
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Debug); // Установите уровень логирования по необходимости
        });
    }
}
