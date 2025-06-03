using System.ComponentModel.DataAnnotations;

namespace MovieStore.Web;
public class Movie
{
  public int Id { get; set; }
  
  [Required(ErrorMessage = "Title is required")]
  [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
  public string? Title { get; set; }
  
  [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
  public string? FileName { get; set; }
  
  public byte[]? FileContent { get; set; }
  
  [StringLength(100, ErrorMessage = "Content type cannot exceed 100 characters")]
  public string? ContentType { get; set; }
}

