var builder = DistributedApplication.CreateBuilder(args);

var tables = builder.AddAzureStorage("storage")
                    .RunAsEmulator()
                    .AddTables("tables");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(tables);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
