using Client.Services;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Client;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IChatService>(provider => new ChatService("127.0.0.1", 3333));
        services.AddTransient<MainViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = new Views.MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}


