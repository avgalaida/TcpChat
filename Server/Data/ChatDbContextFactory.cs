using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Server.Data;

/// <summary>
/// Фабрика для создания экземпляров <see cref="ChatDbContext"/> во время разработки.
/// </summary>
public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    /// <summary>
    /// Создает новый экземпляр контекста базы данных для использования инструментами разработки.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Экземпляр <see cref="ChatDbContext"/>.</returns>
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlite(connectionString);

        return new ChatDbContext(optionsBuilder.Options);
    }
}