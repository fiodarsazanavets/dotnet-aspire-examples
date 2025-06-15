var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WaitFor(rabbitmq)
    .WithReference(rabbitmq);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WaitFor(rabbitmq)
    .WithReference(rabbitmq);

builder.Build().Run();
