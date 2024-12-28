using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PostService.Data;
using PostService.DTOs;

namespace PostService.Controllers;


[Route("api/[controller]")]
[ApiController]

public class PostController : ControllerBase
{
    private readonly IPostRepo _repo;
    private readonly IMapper _mapper;

    public PostController(
        IPostRepo repo,
        IMapper mapper
        )
    {
        _repo = repo;
        _mapper = mapper;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<PostViewDto>> GetAllPosts()
    {
        Console.WriteLine("--> Getting posts from DB...");

        var postItem = _repo.GetAllPostsAsync();

        return Ok(_mapper.Map<IEnumerable<PostViewDto>>(postItem));
    }
    
}