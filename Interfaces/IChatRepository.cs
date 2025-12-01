using System.Collections.Generic;
using System.Threading.Tasks;
using TraceWebApi.Interfaces;

public interface IChatRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesAsync();
    Task AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetAllMessagesAsync();
    Task<List<ChatMessage>> GetTodayMessagesAsync();
}
