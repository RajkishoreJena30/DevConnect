using DevConnect.DTOs;
using DevConnect.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace DevConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService; // ← interface not concrete class
        private readonly IOutputCacheStore _cache;


        public PostsController(IPostService postService, IOutputCacheStore cache)
        {
            _postService = postService;
            _cache = cache;
        }


        //[HttpGet]
        //public async Task<IActionResult> GetAll() =>
        //    Ok(await _postService.GetAllPostsAsync());

        [HttpGet]
        [OutputCache(PolicyName = "Posts")]
        public async Task<IActionResult> GetAll([FromQuery] PostQueryParams query) =>
              Ok(await _postService.GetPagedPostsAsync(query));

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "Posts")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            return post == null ? NotFound() : Ok(post);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _postService.GetMyPostsAsync(userId));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var post = await _postService.CreatePostAsync(userId, dto);
            await _cache.EvictByTagAsync("posts", HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, CreatePostDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _postService.UpdatePostAsync(id, userId, dto);
            if (result) await _cache.EvictByTagAsync("posts", HttpContext.RequestAborted);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            var result = await _postService.DeletePostAsync(id, userId, role);
            if (result) await _cache.EvictByTagAsync("posts", HttpContext.RequestAborted);
            return result ? NoContent() : NotFound();
        }
    }
}