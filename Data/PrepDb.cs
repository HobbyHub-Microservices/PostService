using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data;

public class PrepDb
{
    
    public static void PrepPopulations(IApplicationBuilder app, bool isProduction)
    {
        using(var serviceScope = app.ApplicationServices.CreateScope())
        {
            SeedData(serviceScope.ServiceProvider.GetRequiredService<AppDbContext>(), isProduction);
        }
    }

    private static void SeedData(AppDbContext context, bool isProduction)
    {
        // if (isProduction)
        // {
            Console.WriteLine("--> Trying to apply migrations...");
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
            
        // }
    }
}