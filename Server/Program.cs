using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using Server.Data;
using Server.Interfaces;
using Server.Services;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddSingleton<IChatServer, ChatServer>();
services.AddTransient<IClientHandler, ClientHandler>();
services.AddScoped<IMessageRepository, MessageRepository>();

services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
    loggingBuilder.AddNLog("NLog.config");
});

var serviceProvider = services.BuildServiceProvider();

var server = serviceProvider.GetRequiredService<IChatServer>();
await server.StartAsync();