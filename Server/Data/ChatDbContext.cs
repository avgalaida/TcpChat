using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class ChatDbContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chat.db");
    }
}