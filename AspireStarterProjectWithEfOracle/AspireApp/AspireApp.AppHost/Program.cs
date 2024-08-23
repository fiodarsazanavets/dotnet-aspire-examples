var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);
var oracle = builder.AddOracle("oracle", password);
var oracledb = oracle.AddDatabase("oracledb");

var apiService = builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(oracledb);


builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
