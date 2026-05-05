using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DevConnect.Controllers
{
    [Route("api/posts/{postId}/likes")]
    [ApiController]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly DevConnectDbContext _context;
        public LikesController(DevConnectDbContext context) => _context = context;

        // GET: api/posts/1/likes
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LikeResponseDTO>> GetLikes(int postId)
        {
            var userId = int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

            var likes = await _context.Likes
                .Where(like => like.PostId == postId)
                .ToListAsync();

            return Ok(new LikeResponseDTO
            {
                TotalLikes = likes.Count,
                LikedByMe = likes.Any(like => like.UserId == userId)
            });
        }

        // POST: api/posts/1/likes  → Toggle like
        [HttpPost]
        public async Task<ActionResult<LikeResponseDTO>> ToggleLike(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound("Post not found.");

            var existing = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existing != null)
                _context.Likes.Remove(existing);   // Unlike
            else
                _context.Likes.Add(new Like { PostId = postId, UserId = userId }); // Like

            await _context.SaveChangesAsync();

            var totalLikes = await _context.Likes.CountAsync(l => l.PostId == postId);
            return Ok(new LikeResponseDTO
            {
                TotalLikes = totalLikes,
                LikedByMe = existing == null  // true if just liked
            });
        }
    }
}
