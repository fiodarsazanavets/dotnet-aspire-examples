var builder = DistributedApplication.CreateBuilder(args);

var queues = builder.AddAzureStorage("storage")
                    .RunAsEmulator()
                    .AddQueues("queues");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(queues);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(queues);

builder.Build().Run();
