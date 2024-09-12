var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");
cosmos.RunAsEmulator();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(cosmos);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
