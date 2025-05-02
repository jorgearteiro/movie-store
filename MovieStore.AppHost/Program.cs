
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MovieStore_ApiService>("apiservice");

builder.AddProject<Projects.MovieStore_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddDockerComposePublisher();

builder.AddKubernetesPublisher();

builder.Build().Run();
