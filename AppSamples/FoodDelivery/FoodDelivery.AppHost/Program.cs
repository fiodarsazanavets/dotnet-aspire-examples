var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder
    .AddProject<Projects.FoodDelivery_ApiService>("apiservice");

builder.AddProject<Projects.FoodDelivery_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
