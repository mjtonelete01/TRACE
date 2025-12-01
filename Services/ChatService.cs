using System.Collections.Generic;
using System.Threading.Tasks;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync()
    {
        return await _chatRepository.GetMessagesAsync();
    }

    public async Task SaveMessageAsync(string user, string message)
    {
        var chatMessage = new ChatMessage { User = user, Message = message };
        await _chatRepository.AddMessageAsync(chatMessage);
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync()
    {
        return await _chatRepository.GetTodayMessagesAsync();
    }

}
