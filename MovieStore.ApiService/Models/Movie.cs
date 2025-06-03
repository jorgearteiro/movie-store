using System.ComponentModel.DataAnnotations;

namespace MovieStore.ApiService.Models;

public class Movie
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? FileName { get; set; }
    
    public byte[]? FileContent { get; set; }
    
    [MaxLength(100)]
    public string? ContentType { get; set; }
}