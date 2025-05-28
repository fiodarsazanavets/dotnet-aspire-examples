var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");
cosmos.RunAsEmulator();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(cosmos)
    .WithReference(cosmos);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WaitFor(apiService)
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
