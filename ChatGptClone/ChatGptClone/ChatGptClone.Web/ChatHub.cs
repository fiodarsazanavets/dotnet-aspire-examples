using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace ChatGptClone.Web;

public class ChatHub : Hub
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IChatHistoryService _chatHistoryService;

    public ChatHub(Kernel kernel, IChatCompletionService chat, IChatHistoryService chatHistoryService)
    {
        _kernel = kernel;
        _chat = chat;
        _chatHistoryService = chatHistoryService;
    }

    public async IAsyncEnumerable<string> StreamAnswer(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _chatHistoryService.AddUserMessage(Context.ConnectionId, prompt);

        var settings = new PromptExecutionSettings
        {
        };

        await foreach (var delta in _chat.GetStreamingChatMessageContentsAsync(
            _chatHistoryService.GetChatHistory(Context.ConnectionId), settings, _kernel, cancellationToken))
        {
            if (!string.IsNullOrEmpty(delta.Content))
            {
                yield return delta.Content;
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[ChatHub] Connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[ChatHub] Disconnected: {Context.ConnectionId} {exception?.Message}");
        _chatHistoryService.RemoveHistory(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}
