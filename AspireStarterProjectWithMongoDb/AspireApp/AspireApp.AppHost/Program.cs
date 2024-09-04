var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo");
var mongodb = mongo.AddDatabase("mongodb");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(mongodb);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
