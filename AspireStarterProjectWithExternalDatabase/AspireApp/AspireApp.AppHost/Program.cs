var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddConnectionString("sqldb");

var apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(sqldb)
    .WithReference(sqldb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
