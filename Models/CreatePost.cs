using System.ComponentModel.DataAnnotations;

namespace PostService.Models;

public class CreatePost
{
    [Required, MaxLength(255)]
    public string Title { get; set; }
    [Required]
    public string Content { get; set; }
    public int UserId { get; set; }
    public int HobbyId { get; set; }
}