var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(rabbitmq);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(rabbitmq);

builder.Build().Run();
