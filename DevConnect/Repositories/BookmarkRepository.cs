using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace DevConnect.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly DevConnectDbContext _context;

        public BookmarkRepository(DevConnectDbContext context)
        {
            _context = context;
        }

        public async Task<Bookmark?> GetAsync(int userId, int postId) =>
            await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);

        public async Task AddAsync(Bookmark bookmark)
        {
            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Bookmark bookmark)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PostExistsAsync(int postId) =>
            await _context.Posts.AnyAsync(p => p.Id == postId);

        public async Task<(List<Post> Posts, int TotalCount)> GetMyBookmarkedPostsAsync(
            int userId, BookmarkQueryParams query)
        {
            // Start from Bookmarks so ordering by "when I saved it" is possible
            var q = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Post).ThenInclude(p => p.User)
                .Include(b => b.Post).ThenInclude(p => p.Likes)
                .Include(b => b.Post).ThenInclude(p => p.Comments)
                .AsQueryable();

            // 🆕 Filtering / search (case-insensitive contains on title or content)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                q = q.Where(b =>
                    b.Post.Title.Contains(term) || b.Post.Content.Contains(term));
            }

            // Sorting
            q = (query.SortBy.ToLower(), query.SortDirection.ToLower()) switch
            {
                ("title", "asc") => q.OrderBy(b => b.Post.Title),
                ("title", _) => q.OrderByDescending(b => b.Post.Title),
                ("createdat", "asc") => q.OrderBy(b => b.CreatedAt),
                _ => q.OrderByDescending(b => b.CreatedAt), // default
            };

            var totalCount = await q.CountAsync();

            var posts = await q
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => b.Post)   // project bookmark → its post
                .ToListAsync();

            return (posts, totalCount);
        }

        // 🆕 Aggregate with GroupBy — "most saved posts"
        public async Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take) =>
            await _context.Bookmarks
                .GroupBy(b => new { b.PostId, b.Post.Title })
                .Select(g => new BookmarkStatsDTO
                {
                    PostId = g.Key.PostId,
                    Title = g.Key.Title,
                    BookmarkCount = g.Count()
                })
                .OrderByDescending(s => s.BookmarkCount)
                .Take(take)
                .ToListAsync();
    }
}