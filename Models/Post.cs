using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PostService.Models;

public class Post
{
    [Key]
    [Required]
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int UserId { get; set; }  
    public int HobbyId { get; set; }  
    public DateTime CreatedAt { get; set; }
    
    public List<string> ImageUrls { get; set; } = new List<string>();
}