﻿using System.Text.Json;
using AutoMapper;
using HobbyService.DTO;

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
            case EventType.HobbyEdited:
                Console.WriteLine(message);
                // addUser(message);
                break;
            case EventType.Undetermined:
                Console.WriteLine(message);
                break;
            case EventType.HobbyDeleted:
                Console.WriteLine(message);
                // SendHobbyToPost(message);
                break;
       
        }
    }
    
    private EventType DetermineEventType(string notificationMessage)
    {
        Console.WriteLine("--> DetermineEventType");
     
        try
        {
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
        
            if (eventType?.Event == "Hobby_Edited")
            {
                Console.WriteLine("--> Hobby_Edited");
                return EventType.HobbyEdited;
            }
            
            if (eventType?.Event == "Hobby_Deleted")
            {
                Console.WriteLine("--> Hobby_Deleted");
                return EventType.HobbyDeleted; 
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not determine event type: {ex.Message}");
        }

        Console.WriteLine("--> Unknown event type detected");
        return EventType.Undetermined;
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
    HobbyEdited,
    HobbyDeleted,
    Undetermined
}