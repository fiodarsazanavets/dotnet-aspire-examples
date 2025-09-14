using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatGptClone.Web;

public class ChatHistoryService : IChatHistoryService
{
    private readonly Dictionary<string, ChatHistory> _chatHistories = new();

    public void AddUserMessage(string connectionId, string message)
    {
        if (!_chatHistories.TryGetValue(connectionId, out ChatHistory? value))
        {
            value = [];
            _chatHistories[connectionId] = value;
        }

        value.AddUserMessage(message);
    }

    public ChatHistory GetChatHistory(string connectionId)
    {
        return _chatHistories[connectionId];
    }

    public void RemoveHistory(string connectionId)
    {
        _chatHistories.Remove(connectionId);
    }
}
