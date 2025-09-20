using ChatGptClone.Web;
using ChatGptClone.Web.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddSignalR().AddHubOptions<ChatHub>(o => o.EnableDetailedErrors = true);

builder.Services.AddOutputCache();

builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

var phiConnectionString = builder.Configuration.GetConnectionString("phi35");
var csBuilder = new DbConnectionStringBuilder { ConnectionString = phiConnectionString };

if (!csBuilder.TryGetValue("Endpoint", out var ollamaEndpoint))
{
    throw new InvalidDataException("Ollama connection string is not properly configured.");
}

builder.Services.AddSingleton(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama");

    IKernelBuilder kb = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
    kb.AddOllamaChatCompletion(
        modelId: "phi3.5",
        endpoint: new Uri((string)ollamaEndpoint)
    );
#pragma warning restore SKEXP0070

    return kb.Build();
});

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapHub<ChatHub>("/chat").DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
