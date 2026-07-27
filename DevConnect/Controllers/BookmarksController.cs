using DevConnect.DTOs;
using DevConnect.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevConnect.Controllers
{
    [ApiController]
    public class BookmarksController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarksController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // POST api/posts/5/bookmark  → toggle save/unsave
        [HttpPost("api/posts/{postId}/bookmark")]
        [Authorize]
        public async Task<IActionResult> Toggle(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookmarkService.ToggleBookmarkAsync(userId, postId);
            return result == null ? NotFound("Post not found.") : Ok(result);
        }

        // GET api/bookmarks?pageNumber=1&pageSize=10&sortBy=title&search=react
        [HttpGet("api/bookmarks")]
        [Authorize]
        public async Task<IActionResult> GetMyBookmarks([FromQuery] BookmarkQueryParams query)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _bookmarkService.GetMyBookmarksAsync(userId, query));
        }

        // GET api/bookmarks/top?take=5  → 🆕 trending / most-saved (public)
        [HttpGet("api/bookmarks/top")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTop([FromQuery] int take = 5)
        {
            return Ok(await _bookmarkService.GetTopBookmarkedAsync(take));
        }
    }
}