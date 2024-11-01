using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

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

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var moviesCatalog = new List<MovieObj> {
    new(1, "AKS Container Storage Kafka Demo", "aks-storage-kafka.mp4"),
    new(2, "The Matrix", "matrix.mp4"),
    new(3, "Interstellar", "interstellar.mp4"),
    new(4, "The Dark Knight", "dark_knight.mp4"),
    new(5, "Pulp Fiction", "pulp_fiction.mp4")
};

var videoApi = app.MapGroup("/movies");

videoApi.MapGet("/", async () => await Task.FromResult(moviesCatalog));

videoApi.MapGet("/{id}", (int id) =>
    moviesCatalog.FirstOrDefault(a => a.Id == id) is { } movie
        ? Results.Ok(movie)
        : Results.NotFound());

videoApi.MapGet("/stream/{id}", (int id) =>
{
    var movie = moviesCatalog.FirstOrDefault(m => m.Id == id);
    if (movie == null)
    {
        return Results.NotFound();
    }

    string path = Path.Combine(AppContext.BaseDirectory, "files/", movie.FileName);

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

public class MovieObj
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }

    public MovieObj(int id, string title, string fileName)
    {
        Id = id;
        Title = title;
        FileName = fileName;
    }
}

[JsonSerializable(typeof(List<MovieObj>[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}