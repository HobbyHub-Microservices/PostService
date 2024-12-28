using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
    {
            
    }
    
    public DbSet<Post> Posts { get; set; }
    
}