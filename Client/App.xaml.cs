using Client.Services;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Client;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddSingleton<IChatService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ChatService>>();
            return new ChatService("127.0.0.1", 3000, logger);
        });

        services.AddTransient<MainViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();

        var mainWindow = new Views.MainWindow(mainViewModel);
        mainWindow.Show();
    }
}