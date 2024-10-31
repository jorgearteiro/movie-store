using System.Net.Http;
using System.Text.Json;
using System.Threading;

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

}
