using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PostService.AsyncDataServices;
using PostService.Data;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Controllers;


[Route("api/[controller]")]
[ApiController]

public class PostController : ControllerBase
{
    private readonly IPostRepo _repo;
    private readonly IMapper _mapper;
    private readonly IMesssageBusClient _messageBusClient;


    public PostController(
        IPostRepo repo,
        IMapper mapper,
        IMesssageBusClient messageBusClient
        )
    {
        _messageBusClient = messageBusClient;
        _repo = repo;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostViewDto>>> GetAllPosts()
    {
        Console.WriteLine("--> Getting posts from DB...");

        // Await the asynchronous method
        var postItems = await _repo.GetAllPostsAsync();

        // Map the result to PostViewDto
        return Ok(_mapper.Map<IEnumerable<PostViewDto>>(postItems));
    }

    [HttpGet("{id}", Name = "GetPostById")]
    public async Task<ActionResult<PostViewDto>> GetPostById(int id)
    {
        var postItem = await _repo.GetPostByIdAsync(id); // Await the async method

        if (postItem != null)
        {
            return Ok(_mapper.Map<PostViewDto>(postItem));
        }

        return NotFound(); // Return 404 if no post is found
    }

    [HttpPost]
    public async Task<ActionResult<PostViewDto>> CreatePost(PostCreateDTO postCreateDto)
    {
        var post = _mapper.Map<Post>(postCreateDto);
        await _repo.CreatePost(post);
        _repo.SaveChanges();
        
        var postViewDto = _mapper.Map<PostViewDto>(post);
        
        var postPublishedDto = new PostPublishedDTO
        {
            Id = post.PostId,
            UserId = post.UserId,
            HobbyId = post.HobbyId,
            Event = "Post_Published"
            
        };
        
        _messageBusClient.PublishNewPost(postPublishedDto);
        
        return CreatedAtRoute(nameof(GetPostById), new { Id = postViewDto.PostId }, postViewDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PostViewDto>> UpdatePost(int id, PostCreateDTO postCreateDto)
    {
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<PostViewDto>> DeletePost(int id)
    {
        return Ok();
    }
}