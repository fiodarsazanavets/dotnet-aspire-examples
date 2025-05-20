var builder = DistributedApplication.CreateBuilder(args);

var oracle = builder.AddOracle("oracle").WithLifetime(ContainerLifetime.Persistent);
var oracledb = oracle.AddDatabase("oracledb");

var apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(oracledb)
    .WithReference(oracledb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
