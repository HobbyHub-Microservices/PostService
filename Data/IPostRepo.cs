using PostService.Models;

namespace PostService.Data;

public interface IPostRepo
{
    bool SaveChanges();  // Saves changes to the database (for Create, Update, Delete operations)

    Task CreatePost(Post post);  // Creates a new post (command operation)

    // Read side (Query)
    Task<IEnumerable<ViewPost>> GetAllPostsAsync();  // Retrieves all posts (query operation)

    Task<ViewPost> GetPostByIdAsync(int postId);  // Retrieves a post by its ID (query operation)

    Task<IEnumerable<ViewPost>> GetPostByUserName(string UserName);
    
    Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);  // Retrieves posts by a specific user (query operation)

    Task<IEnumerable<Post>> GetPostsByHobbyIdAsync(int hobbyId);  // Retrieves posts by a specific hobby (query operation)
}