namespace PostService.DTOs;

public class PostPublishedDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int HobbyId { get; set; }
    
    public string Event { get; set; } 
}