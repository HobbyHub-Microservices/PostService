using AutoMapper;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Profiles;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<PostCreateDTO, Post>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)) // Set the created timestamp to current time
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Map UserId directly if it's an int in PostCreateDTO
            .ForMember(dest => dest.HobbyId, opt => opt.MapFrom(src => src.HobbyId)); // Map UserId directly if it's an int in PostCreateDTO
        
        // Map from Post to PostViewDto (for returning data in viewable format)
        CreateMap<Post, PostViewDto>();

    }
}
