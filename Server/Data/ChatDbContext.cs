using Microsoft.EntityFrameworkCore;
using Server.Models.Messages;

namespace Server.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<OutgoingChatMessage> Messages { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OutgoingChatMessage>(entity =>
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
}