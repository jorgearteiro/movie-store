using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MovieStore.ApiService.Data;
using MovieStore.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add Entity Framework and PostgreSQL
builder.AddNpgsqlDbContext<MovieStoreContext>("moviestore");

// Add CORS
builder.Services.AddCors
    (
    options =>
    {
        options.AddDefaultPolicy
        (
            builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                       .WithExposedHeaders("strict-origin-when-cross-origin");
            }
        );
    }
);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MovieStoreContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var videoApi = app.MapGroup("/movies");

videoApi.MapGet("/", async (MovieStoreContext context) => 
    await context.Movies.ToListAsync());

videoApi.MapGet("/{id}", async (int id, MovieStoreContext context) =>
{
    var movie = await context.Movies.FindAsync(id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

videoApi.MapPost("/", async (Movie movie, MovieStoreContext context) =>
{
    context.Movies.Add(movie);
    await context.SaveChangesAsync();
    return Results.Created($"/movies/{movie.Id}", movie);
});

videoApi.MapGet("/stream/{id}", async (int id, MovieStoreContext context) =>
{
    var movie = await context.Movies.FindAsync(id);
    if (movie == null)
    {
        return Results.NotFound();
    }

    string path = Path.Combine(AppContext.BaseDirectory, "files/", movie.FileName ?? "");

    if (!System.IO.File.Exists(path))
    {
        //copy from catalog to files
        string pathCatalog = Path.Combine(AppContext.BaseDirectory, "catalog/", "aks-storage-kafka.mp4");//movie.FileName);
        if (!System.IO.File.Exists(pathCatalog))
        {
            return Results.NotFound();
        }
        System.IO.File.Copy(pathCatalog, path);
    }

    var filestream = System.IO.File.OpenRead(path);
    return Results.File(filestream, contentType: "video/mp4", fileDownloadName: movie.FileName, enableRangeProcessing: true);
});

app.MapDefaultEndpoints();

app.UseCors();

app.Run();

[JsonSerializable(typeof(List<Movie>[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}