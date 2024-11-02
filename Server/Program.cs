using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Server.Data;
using Server.Interfaces;
using Server.Services;

var services = new ServiceCollection();

services.AddSingleton<IChatServer, ChatServer>();
services.AddTransient<IClientHandler, ClientHandler>();
services.AddScoped<IMessageRepository, MessageRepository>();
services.AddDbContext<ChatDbContext>();

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
    loggingBuilder.AddNLog("NLog.config");
});

var serviceProvider = services.BuildServiceProvider();

var server = serviceProvider.GetRequiredService<IChatServer>();
await server.StartAsync();