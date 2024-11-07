using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Models.Messages;

namespace Server.Data;
public class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }
    private readonly IConfiguration _configuration;

    public ChatDbContext(DbContextOptions<ChatDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id); 
            entity.Property(e => e.Sender)
                    .IsRequired()
                    .HasMaxLength(100); 

            entity.Property(e => e.Content)
                    .IsRequired();

            entity.Property(e => e.Timestamp)
                    .IsRequired();

            entity.Property(e => e.SenderIp)
                    .IsRequired()
                    .HasMaxLength(45); // IPv6 поддержка

            entity.Property(e => e.SenderPort)
                    .IsRequired();

            entity.HasIndex(e => e.Timestamp);
        });
    }
}