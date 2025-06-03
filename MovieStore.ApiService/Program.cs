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
var connectionString = builder.Configuration.GetConnectionString("moviestore") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=moviestore;Username=postgres;Password=postgres";

// Use SQLite for development/testing when no PostgreSQL is available
var usePostgres = connectionString.Contains("Host=") || connectionString.Contains("Server=");

builder.Services.AddDbContext<MovieStoreContext>(options =>
{
    if (usePostgres)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite("Data Source=moviestore.db");
    }
});

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

videoApi.MapPost("/upload", async (HttpRequest request, MovieStoreContext context) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart form data");
    }

    var form = await request.ReadFormAsync();

    var title = form["title"].ToString();
    if (string.IsNullOrEmpty(title))
    {
        return Results.BadRequest("Title is required");
    }

    var file = form.Files.FirstOrDefault();
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("File is required");
    }

    // Read file content
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);

    var movie = new Movie
    {
        Title = title,
        FileName = file.FileName,
        FileContent = memoryStream.ToArray(),
        ContentType = file.ContentType
    };

    context.Movies.Add(movie);
    await context.SaveChangesAsync();

    return Results.Created($"/movies/{movie.Id}", movie);
});

videoApi.MapDelete("/{id}", async (int id, MovieStoreContext context) =>
{
    var movie = await context.Movies.FindAsync(id);
    if (movie == null)
    {
        return Results.NotFound();
    }
    
    context.Movies.Remove(movie);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

videoApi.MapGet("/stream/{id}", async (int id, MovieStoreContext context) =>
{
    var movie = await context.Movies.FindAsync(id);
    if (movie == null)
    {
        return Results.NotFound();
    }

    // Serve from database if file content is available
    if (movie.FileContent != null && movie.FileContent.Length > 0)
    {
        var contentType = movie.ContentType ?? "video/mp4";
        var stream = new MemoryStream(movie.FileContent);
        return Results.File(stream, contentType: contentType, fileDownloadName: movie.FileName, enableRangeProcessing: true);
    }

    // Fallback to file system for existing movies
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