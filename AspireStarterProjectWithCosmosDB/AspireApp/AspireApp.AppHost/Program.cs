var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");

var cosmosdb = cosmos.AddDatabase("cosmosdb");
cosmosdb.RunAsEmulator();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(cosmosdb);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
