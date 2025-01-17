using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.AsyncDataServices;
using PostService.Data;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]

public class PostController : ControllerBase
{
    private readonly IPostRepo _repo;
    private readonly IMapper _mapper;
    private readonly bool IntegrationMode;
    private readonly IConfiguration _config;

    public PostController(
        IPostRepo repo,
        IMapper mapper,
        IMesssageBusClient messageBusClient,
        IConfiguration configuration
        )
    {
        _config = configuration;
        _repo = repo;
        _mapper = mapper;
        IntegrationMode = _config.GetValue<bool>("IntegrationMode");
    }
          private bool IsValidJwt()
        {

            var expectedJwt = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJYc3kxV3dzZTBhaGZrOHZheDI5V2pOR3luLVJzYmxzdjJjNWlsWnZ1bU1jIn0.eyJleHAiOjE3MzYyNzA5MDUsImlhdCI6MTczNjI3MDYwNSwianRpIjoiZjcxMmVjZjYtOWJhNy00N2MwLThiMzktZDhmY2VhOWVlNjg4IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrLWhvYmJ5aHViLmF1c3RyYWxpYWNlbnRyYWwuY2xvdWRhcHAuYXp1cmUuY29tL3JlYWxtcy9Ib2JieUh1YiIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiI0MGRlNmNhNS1mNmQ0LTRkYjgtYTE5Zi1jNTIxMzFhMDMzNTIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJ1c2VyLXNlcnZpY2UiLCJzaWQiOiI1NmRjZDAxZi1mNjJiLTRkZmEtYWY3MS05NTdkNzNiZjdhYjgiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbImh0dHBzOi8vaG9iYnlodWIuYXVzdHJhbGlhY2VudHJhbC5jbG91ZGFwcC5henVyZS5jb20vKiJdLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1ob2JieWh1YiIsIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6InByb2ZpbGUgZW1haWwiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6InRlc3QgdGVzdCIsInByZWZlcnJlZF91c2VybmFtZSI6InBvc3RtYW4iLCJnaXZlbl9uYW1lIjoidGVzdCIsImZhbWlseV9uYW1lIjoidGVzdCIsImVtYWlsIjoicG9zdG1hbkBwb3N0bWFuLm5sIn0.OwdnatvORryLz7Y-ikh9ekpOgR4Kbz-HzonNbD6W6XSd0oOUYrUIE86cyNGKNnm0A3fCBg7A7q4ToLR1maXoGwGXOiutcT-mjYPIjevSq5yf5oz-MQec8e4MheFpvfGJOUY1cCxEszZQUNCzifmPyOUF7hmxPft9SowdhkcEHBvvECZ658Ye9dcaZUx5FrZcZPk9WMCCMatxY6zpPZV7fwXUC3n1vdn_B_OS9IZHdeRZgd4lyR_SQTlwg9_mUGkAD-EFRBl0O2Dez4BalOA79rGc82N0g0JBcb_i6lN61VLMPDYm4AQ1HA790ng96ZrpLXyfnMw3g8-ZYX-epwA_VQ";
            
                // Extract JWT from headers
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
                if (authorizationHeader.StartsWith("Bearer "))
                {
                    Console.WriteLine("INTEGRATION MODE: CHECKED -> STARTS WITH BEARER");
                    var jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
                    Console.WriteLine("INTEGRATION MODE: CHECKED -> JWT TOKEN");
                    return jwt == expectedJwt; // Validate JWT value
                }

                return false; // No valid JWT provided
                
        }
          
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostViewDto>>> GetAllPosts()
    {
        Console.WriteLine("--> Getting posts from DB...");

        // Await the asynchronous method
        var postItems = await _repo.GetAllPostsAsync();

        // Map the result to PostViewDto
        return Ok(_mapper.Map<IEnumerable<PostViewDto>>(postItems));
    }
    
    [Authorize]
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
    
    [Authorize]
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
        
        // _messageBusClient.PublishNewPost(postPublishedDto);
        
        return CreatedAtRoute(nameof(GetPostById), new { Id = postViewDto.PostId }, postViewDto);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<PostViewDto>> UpdatePost(int id, PostCreateDTO postCreateDto)
    {
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<PostViewDto>> DeletePost(int id)
    {
        return Ok();
    }
    
    
    
    [AllowAnonymous]
    [HttpGet("test/{id}", Name = "GetTestPostById")]
    public async Task<ActionResult<PostViewDto>> GetTestPostById(int id)
    {
        if (!IntegrationMode)
        {
            return NotFound();
        }
        if (!IsValidJwt())
        {
            Console.WriteLine("Invalid jwt inside integration test");
            return Unauthorized();
        }

        var postItem = await _repo.GetPostByIdAsync(id); // Await the async method

        if (postItem != null)
        {
            return Ok(_mapper.Map<PostViewDto>(postItem));
        }

        return NotFound(); // Return 404 if no post is found
    }
    
    [AllowAnonymous]
    [HttpPost("test")]
    public async Task<ActionResult<PostViewDto>> CreateTestPost(PostCreateDTO postCreateDto)
    {
        Console.WriteLine("--> Posting test post/ Integration test");
        if (!IntegrationMode)
        {
            return NotFound();
        }
        if (!IsValidJwt())
        {
            Console.WriteLine("Invalid jwt inside integration test");
            return Unauthorized();
        }
        
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
        
        // _messageBusClient.PublishNewPost(postPublishedDto);
        
        return CreatedAtRoute(nameof(GetPostById), new { Id = postViewDto.PostId }, postViewDto);
    }
    
  
}