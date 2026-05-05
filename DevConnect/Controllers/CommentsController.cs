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
    [Route("api/posts/{postId}/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly DevConnectDbContext _context;
        public CommentsController(DevConnectDbContext context) => _context = context;

        // GET: api/posts/1/comments
        [HttpGet]
        public async Task<ActionResult<List<CommentResponseDTO>>> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.User.Name,
                    PostId = c.PostId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/posts/1/comments
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentResponseDTO>> AddComment(
            int postId, CreateCommentDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound("Post not found.");

            var comment = new Comment
            {
                Content = dto.Content,
                PostId = postId,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            await _context.Entry(comment).Reference(c => c.User).LoadAsync();

            return CreatedAtAction(nameof(GetComments), new { postId }, new CommentResponseDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorName = comment.User.Name,
                PostId = comment.PostId,
                CreatedAt = comment.CreatedAt
            });
        }

        // PUT: api/posts/1/comments/5
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(
            int postId, int commentId, CreateCommentDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

            if (comment == null) return NotFound();
            if (comment.UserId != userId) return Forbid();

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/posts/1/comments/5
        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

            if (comment == null) return NotFound();
            if (comment.UserId != userId && role != "Admin") return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
