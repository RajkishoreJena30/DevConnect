using DevConnect.Data;
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
    }
}