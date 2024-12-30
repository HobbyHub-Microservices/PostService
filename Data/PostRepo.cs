using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data;

public class PostRepo : IPostRepo
{
    private readonly AppDbContext _context;

    public PostRepo(AppDbContext context)
    {
        _context = context;
    }
    
    public bool SaveChanges()
    {
        return (_context.SaveChanges() >= 0);
    }

    public async Task CreatePost(Post post)
    {
        if (post == null)
        {
            throw new ArgumentNullException(nameof(post));
        }
        
        await  _context.Posts.AddAsync(post);
        
        // await _messageBus.PublishAsync(new PostCreatedEvent(post));
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ViewPost>> GetAllPostsAsync()
    {
        return await _context.ViewPosts.ToListAsync();
    }

    public async Task<ViewPost> GetPostByIdAsync(int postId)
    {
        return await _context.ViewPosts.FirstOrDefaultAsync(p => p.PostId == postId);
    }

    public async Task<IEnumerable<ViewPost>> GetPostByUserName(string UserName)
    {
       return await _context.ViewPosts.Where(p => p.UserName == UserName).ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
    {
        return await _context.Posts
            .Where(p => p.UserId == userId) // Replace with actual logic for User lookup  
            .ToListAsync();
    }
    
    

    public async Task<IEnumerable<Post>> GetPostsByHobbyIdAsync(int hobbyId)
    {
        return await _context.Posts
            .Where(p => p.HobbyId == hobbyId)  // Replace with actual logic for Hobby lookup
            .ToListAsync();
    }
}