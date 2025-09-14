using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatGptClone.Web
{
    public interface IChatHistoryService
    {
        void AddUserMessage(string connectionId, string message);
        ChatHistory GetChatHistory(string connectionId);
        void RemoveHistory(string connectionId);
    }
}
