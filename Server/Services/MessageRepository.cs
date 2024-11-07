using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Interfaces;
using Server.Models;

namespace Server.Services;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _context;

    public MessageRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int page, int pageSize)
    {
        return await _context.Messages
            .OrderByDescending(m => m.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Messages.CountAsync();
    }
}