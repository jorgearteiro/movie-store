using Microsoft.EntityFrameworkCore;
using MovieStore.ApiService.Models;

namespace MovieStore.ApiService.Data;

public class MovieStoreContext : DbContext
{
    public MovieStoreContext(DbContextOptions<MovieStoreContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "AKS Container Storage Kafka Demo", FileName = "aks-storage-kafka.mp4" },
            new Movie { Id = 2, Title = "The Matrix", FileName = "matrix.mp4" },
            new Movie { Id = 3, Title = "Interstellar", FileName = "interstellar.mp4" },
            new Movie { Id = 4, Title = "The Dark Knight", FileName = "dark_knight.mp4" },
            new Movie { Id = 5, Title = "Pulp Fiction", FileName = "pulp_fiction.mp4" }
        );
    }
}