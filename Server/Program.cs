using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Server.Models.Messages;
using Server.Repository;
using Server.Services.Client;
using Server.Services.Factories;
using Server.Services.Handlers;
using Server.Services.Server;
using Server.Utilities;

// Устанавливаем кодировку консольного вывода
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Создаем конфигурацию приложения, используя файл appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Инициализируем коллекцию служб для DI-контейнера
var services = new ServiceCollection();

// Регистрация конфигурации
services.AddSingleton<IConfiguration>(configuration);

// Регистрация сервисов для приложения
services.AddSingleton<IMessageSerializer, MessageSerializer>();

// Регистрация обработчиков сообщений
services.AddTransient<IMessageHandler<IncomingChatMessage>, ChatMessageHandler>();
services.AddTransient<IMessageHandler<HistoryRequest>, HistoryRequestHandler>();

// Регистрация фабрик
services.AddSingleton<IMessageHandlerFactory, MessageHandlerFactory>();
services.AddSingleton<IClientHandlerFactory, ClientHandlerFactory>();

// Регистрация сервера и обработчиков клиентов
services.AddSingleton<IChatServer, ChatServer>();
services.AddTransient<IClientHandler, ClientHandler>();
services.AddScoped<IMessageRepository, MessageRepository>();

// Настройка контекста базы данных с использованием SQLite
services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// Настройка логирования с использованием NLog
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
    loggingBuilder.AddNLog("NLog.config");
});

// Построение провайдера служб
var serviceProvider = services.BuildServiceProvider();

// Создание области для выполнения миграций базы данных
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate(); // Выполнение миграции базы данных
}

// Создаем источник токена отмены
using var cts = new CancellationTokenSource();

// Добавляем обработчик для завершения программы при нажатии Ctrl+C
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Завершается работа сервера...");
};

// Запуск сервера
var server = serviceProvider.GetRequiredService<IChatServer>();
await server.StartAsync(cts.Token); // Передаем токен отмены для управления завершением