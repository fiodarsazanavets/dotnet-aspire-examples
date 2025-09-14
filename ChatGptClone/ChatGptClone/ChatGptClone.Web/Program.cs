using ChatGptClone.Web;
using ChatGptClone.Web.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddSignalR().AddHubOptions<ChatHub>(o => o.EnableDetailedErrors = true);

builder.Services.AddOutputCache();

builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

builder.Services.AddHttpClient("ollama-phi35", c =>
{
    c.BaseAddress = new Uri("https+http://phi3-5");
})
.AddServiceDiscovery()
.AddStandardResilienceHandler();

builder.Services.AddSingleton(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama-phi35");

    IKernelBuilder kb = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
    kb.AddOllamaChatCompletion(
        modelId: "phi3.5",
        httpClient: http
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
