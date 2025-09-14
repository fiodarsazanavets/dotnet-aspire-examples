using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

public static IResourceBuilder<ContainerResource> AddOllama(
    this IDistributedApplicationBuilder builder,
    string name,
    string model,
    bool useGpu = false,
    int httpPort = 11434,
    bool usePodman = false)
{
    var ollama = builder.AddContainer(name, "ollama/ollama")
        .WithHttpEndpoint(targetPort: 11434, port: httpPort)
        .WithEnvironment("OLLAMA_HOST", "0.0.0.0:11434")
        .WithVolume("ollama", "/root/.ollama")
        .WithHttpHealthCheck("/api/tags");

    if (useGpu)
    {
        if (usePodman) ollama.WithContainerRuntimeArgs("--device", "nvidia.com/gpu=all");
        else ollama.WithContainerRuntimeArgs("--gpus=all");
    }

    // Optional: pre-pull a model via a tiny entrypoint script you mount with WithBindMount(...)
    // or prefer the Community Toolkit approach below, which handles this for you.

    return ollama;
}
