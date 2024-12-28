using System.ComponentModel.DataAnnotations;

namespace PostService.DTOs;

public class PostCreateDTO
{
    [Required, MaxLength(255)]
    public string Title { get; set; }
    [Required]
    public string Content { get; set; }
    public Guid UserId { get; set; }
    public Guid HobbyId { get; set; }
}