using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TraceWebApi.EntityFrameworkCore;

public class ChatRepository : IChatRepository
{
    private readonly TraceAppDbContext _context;

    public ChatRepository(TraceAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync()
    {
        return await _context.ChatMessages.OrderByDescending(m => m.Timestamp).Take(50).ToListAsync();
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetAllMessagesAsync()
    {
        return await _context.ChatMessages.OrderBy(m => m.Timestamp).ToListAsync();
    }

    public async Task<List<ChatMessage>> GetTodayMessagesAsync()
    {
        DateTime today = DateTime.Now;

        DateTime startOfDay = today.Date;

        DateTime endOfDay = today.Date.AddDays(1).AddTicks(-1);

        return await _context.ChatMessages
                             .Where(m => m.Timestamp >= startOfDay && m.Timestamp <= endOfDay)
                             .OrderBy(m => m.Timestamp)
                             .ToListAsync();
    }

}
