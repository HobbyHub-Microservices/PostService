using AutoMapper;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Profiles;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<PostCreateDTO, Post>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => Guid.NewGuid())) // Auto-generate the PostId
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Set the created timestamp to current time

        // Map from Post to PostViewDto (for returning data in viewable format)
        CreateMap<Post, PostViewDto>()
            .ForMember(dest => dest.HobbyName, opt => opt.MapFrom(src => "HOBBY_NAME_PLACEHOLDER")) // Placeholder, assuming you'll fetch this data from another service
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => "USER_NAME_PLACEHOLDER")); // Placeholder, assuming you'll fetch this data from another service
    }
}