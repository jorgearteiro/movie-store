
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "moviestore")
    .AddDatabase("moviestore");

var apiService = builder.AddProject<Projects.MovieStore_ApiService>("apiservice")
    .WithReference(postgres);

builder.AddProject<Projects.MovieStore_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddDockerComposeEnvironment("docker-compose");

builder.AddKubernetesEnvironment("kube");

builder.Build().Run();
