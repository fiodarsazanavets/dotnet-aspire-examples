var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder.AddAzureStorage("storage")
                    .RunAsEmulator()
                    .AddBlobs("blobs");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(blobs);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
