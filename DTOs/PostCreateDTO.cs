using System.ComponentModel.DataAnnotations;

namespace PostService.DTOs;

public class PostCreateDTO
{
    [Required, MaxLength(255)]
    public string Title { get; set; }
    [Required]
    public string Content { get; set; }
    public int UserId { get; set; }
    public int HobbyId { get; set; }
    
    public List<string> ImageUrls { get; set; } = new List<string>();
}