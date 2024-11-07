using Microsoft.EntityFrameworkCore;
using Server.Models.Messages;

namespace Server.Data;

/// <summary>
/// Контекст базы данных для приложения чата.
/// </summary>
public class ChatDbContext : DbContext
{
    /// <summary>
    /// Набор данных для исходящих сообщений чата.
    /// </summary>
    public DbSet<OutgoingChatMessage> Messages { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ChatDbContext"/> с заданными параметрами.
    /// </summary>
    /// <param name="options">Параметры контекста базы данных.</param>
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Настраивает модель базы данных при ее создании.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutgoingChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id); // Установка первичного ключа
            entity.Property(e => e.Sender)
                .IsRequired()
                .HasMaxLength(100); // Ограничение длины имени отправителя

            entity.Property(e => e.Content)
                .IsRequired(); // Содержимое сообщения обязательно

            entity.Property(e => e.Timestamp)
                .IsRequired(); // Обязательная метка времени

            entity.Property(e => e.SenderIp)
                .IsRequired()
                .HasMaxLength(45); // Поддержка IPv6 с максимальной длиной

            entity.Property(e => e.SenderPort)
                .IsRequired(); // Обязательный порт отправителя

            entity.HasIndex(e => e.Timestamp); // Индекс по метке времени для оптимизации запросов
        });
    }
}