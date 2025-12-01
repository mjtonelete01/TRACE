using System.Collections.Generic;
using System.Threading.Tasks;

public interface IChatService
{
    Task<IEnumerable<ChatMessage>> GetMessagesAsync();
    Task SaveMessageAsync(string user, string message);
    Task<List<ChatMessage>> GetChatHistoryAsync();
}
