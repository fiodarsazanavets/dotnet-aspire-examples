var builder = DistributedApplication.CreateBuilder(args);

var queues = builder.AddAzureStorage("storage")
                    .RunAsEmulator()
                    .AddQueues("queues");

builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(queues)
    .WithReference(queues);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WaitFor(queues)
    .WithReference(queues);

builder.Build().Run();
