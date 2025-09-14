using ChatGptClone.Web.Components;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient("ollama-phi35", c =>
{
    c.BaseAddress = new Uri("https+http://phi3-5");
});

builder.Services.AddSingleton(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama-phi35");

    IKernelBuilder kb = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
    kb.AddOllamaChatCompletion(
        modelId: "phi3-5",
        httpClient: http
    );
#pragma warning restore SKEXP0070

    return kb.Build();
});

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
