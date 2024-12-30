namespace PostService.DTOs;

public class PostViewDto
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string HobbyName { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<string> ImageUrls { get; set; }
}