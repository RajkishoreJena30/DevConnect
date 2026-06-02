using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace DevConnect.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DevConnectDbContext _context;

        public PostRepository(DevConnectDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllAsync() =>
            await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .ToListAsync();

        public async Task<Post?> GetByIdAsync(int id) =>
            await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Post>> GetByUserIdAsync(int userId) =>
            await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .ToListAsync();

        public async Task<Post> CreateAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _context.Posts.AnyAsync(p => p.Id == id);

        public async Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(PostQueryParams query)
        {
            var q = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .AsQueryable();

            // Sorting
            q = (query.SortBy.ToLower(), query.SortDirection.ToLower()) switch
            {
                ("title", "asc") => q.OrderBy(p => p.Title),
                ("title", _) => q.OrderByDescending(p => p.Title),
                ("likes", "asc") => q.OrderBy(p => p.Likes.Count),
                ("likes", _) => q.OrderByDescending(p => p.Likes.Count),
                ("createdat", "asc") => q.OrderBy(p => p.CreatedAt),
                _ => q.OrderByDescending(p => p.CreatedAt), // default
            };

            var totalCount = await q.CountAsync();

            var posts = await q
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (posts, totalCount);
        }
    }
}