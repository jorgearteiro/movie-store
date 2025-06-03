using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Text;

namespace MovieStore.Web;

public class MovieService(HttpClient httpClient)
{
    public async Task<List<Movie>> GetMoviesAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<Movie>? movies = null;

            await foreach (var movie in httpClient.GetFromJsonAsAsyncEnumerable<Movie>("/movies", options, cancellationToken))
            {
                if (movies?.Count >= maxItems)
                {
                  break;
                }
                if (movie is not null)
                {
                  movies ??= [];
                  movies.Add(movie);
                }
            }

            return movies?.ToList() ?? [];
    }

    public async Task<Movie?> AddMovieAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Serialize(movie, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/movies", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Movie>(responseContent, options);
        }

        return null;
    }

    public async Task<Movie?> AddMovieWithFileAsync(string title, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        // Add title
        content.Add(new StringContent(title), "title");

        // Add file
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await httpClient.PostAsync("/movies/upload", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Movie>(responseContent, options);
        }

        return null;
    }
    public async Task<bool> DeleteMovieAsync(int movieId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/movies/{movieId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

}
