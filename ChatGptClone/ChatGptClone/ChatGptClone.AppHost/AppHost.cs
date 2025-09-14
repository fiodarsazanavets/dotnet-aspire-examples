var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithOpenWebUI();

var phi35 = ollama.AddModel("phi3-5");

builder.AddProject<Projects.ChatGptClone_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(phi35)
    .WithReference(phi35);

builder.Build().Run();
