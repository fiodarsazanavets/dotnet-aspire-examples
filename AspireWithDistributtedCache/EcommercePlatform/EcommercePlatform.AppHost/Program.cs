var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisCommander();

var apiService = builder.AddProject<Projects.EcommercePlatform_ApiService>("apiservice")
    .WithReference(cache);


builder.AddProject<Projects.EcommercePlatform_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
