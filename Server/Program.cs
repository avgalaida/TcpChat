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

Console.OutputEncoding = System.Text.Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddSingleton<IMessageSerializer, MessageSerializer>();

services.AddTransient<IMessageHandler<IncomingChatMessage>, ChatMessageHandler>();
services.AddTransient<IMessageHandler<HistoryRequest>, HistoryRequestHandler>();

services.AddSingleton<IMessageHandlerFactory, MessageHandlerFactory>();

services.AddSingleton<IClientHandlerFactory, ClientHandlerFactory>();

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

using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate();
}

var server = serviceProvider.GetRequiredService<IChatServer>();
await server.StartAsync();