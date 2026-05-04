using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DevConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly DevConnectDbContext _context;

        public PostsController(DevConnectDbContext context)
        {
            _context = context;
        }

        // GET: api/posts  — Get all posts with author name
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<PostResponseDTO>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Select(p => new PostResponseDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.User.Name
                })
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/posts/5  — Get single post
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostResponseDTO>> GetPostById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            return Ok(new PostResponseDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                AuthorName = post.User.Name
            });
        }

        // GET: api/posts/my  — Get own posts
        [HttpGet("my")]
        public async Task<ActionResult<List<PostResponseDTO>>> GetMyPosts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Select(p => new PostResponseDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.User.Name
                })
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/posts  — Create post (logged in user)
        [HttpPost]
        public async Task<ActionResult<PostResponseDTO>> CreatePost(CreatePostDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = userId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, new PostResponseDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                AuthorName = user.Name
            });
        }

        // PUT: api/posts/5  — Update own post only
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, CreatePostDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid(); // Can't edit someone else's post

            post.Title = dto.Title;
            post.Content = dto.Content;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/posts/5  — Delete own post (or Admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            // Only post owner or Admin can delete
            if (post.UserId != userId && userRole != "Admin")
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
