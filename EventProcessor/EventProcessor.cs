using System.Text.Json;
using AutoMapper;
using HobbyService.DTO;
using PostService.Data;
using PostService.DTOs;

namespace PostService.EventProcessor;

public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapper _mapper;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _mapper = mapper;
    }
    public void ProcessEvent(string message)
    {
        var eventType = DetermineEventType(message);
        switch (eventType)
        {
            
            case EventType.Undetermined:
                Console.WriteLine(message);
                break;
            case EventType.HobbyDeleted:
                Console.WriteLine(message);
                DeleteHobby(message);
                break;
            case EventType.UserDeleted:
                DeleteUser(message);
                break;
       
        }
    }

    private async Task DeleteHobby(string hobbyPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IPostRepo>();
            var hobbyPublishedEventDto = JsonSerializer.Deserialize<HobbyDeletePublishedDto>(hobbyPublishedMessage);
    
            try
            {
                // Perform the update using the repository method
                await repo.DeletedHobby(hobbyPublishedEventDto.Id);
                Console.WriteLine($"Successfully deleted hobbies");
     
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not add User to DB {ex.Message}");
            }
        }
    }
    
    private async Task DeleteUser(string userPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IPostRepo>();
            var userPublishedEventDto = JsonSerializer.Deserialize<UserDeletePublishedDto>(userPublishedMessage);
    
            try
            {
                Console.WriteLine($"UserId = {userPublishedEventDto.Id}");
                // Perform the update using the repository method
                await repo.DeletedUserPosts(userPublishedEventDto.Id);
                Console.WriteLine($"Successfully deleted user posts");
     
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not add User to DB {ex.Message}");
            }
        }
    }

    private EventType DetermineEventType(string notificationMessage)
    {
        Console.WriteLine("--> DetermineEventType");
     
        try
        {
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
        
            
            if (eventType?.Event == "Hobby_Deleted")
            {
                Console.WriteLine("--> Hobby_Deleted");
                return EventType.HobbyDeleted; 
            }
            

            if (eventType?.Event == "User_Deleted")
            {
                Console.WriteLine("--> User_Deleted");
                return EventType.UserDeleted;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not determine event type: {ex.Message}");
        }

        Console.WriteLine("--> Unknown event type detected");
        return EventType.Undetermined;;
    }

    // private void addUser(string userPublishedMessage)
    // {
    //     using (var scope = _serviceScopeFactory.CreateScope())
    //     {
    //         var repo = scope.ServiceProvider.GetRequiredService<IHobbyRepo>();
    //         var userPublishedEventDto = JsonSerializer.Deserialize<UserPublishedDto>(userPublishedMessage);
    //
    //         try
    //         {
    //             var user = _mapper.Map<User>(userPublishedEventDto);
    //             if (!repo.ExternalUserExists(user.ExternalId))
    //             {
    //                 repo.CreateUser(user);
    //                 repo.SaveChanges();
    //                 Console.WriteLine("--> User added");
    //             }
    //             else
    //             {
    //                 Console.WriteLine("--> User already exists");
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"Could not add User to DB {ex.Message}");
    //         }
    //     }
    // }

}
enum EventType
{
    HobbyDeleted,
    UserDeleted,
    Undetermined
}