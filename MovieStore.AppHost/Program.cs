
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MovieStore_ApiService>("apiservice");

builder.AddProject<Projects.MovieStore_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddDockerComposeEnvironment("docker-compose");

builder.AddKubernetesEnvironment("kube");

builder.Build().Run();
