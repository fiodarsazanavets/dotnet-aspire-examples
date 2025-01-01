using FoodDelivery.ApiService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

builder.Services.AddSingleton<LocationUpdater>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<LocationUpdater>());

var app = builder.Build();

app.UseExceptionHandler();

app.MapHub<LocationHub>("/locationHub");

app.MapDefaultEndpoints();

app.Run();