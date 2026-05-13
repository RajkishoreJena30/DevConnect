using DevConnect.Models;

namespace DevConnect.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task<List<Post>> GetByUserIdAsync(int userId);
        Task<Post> CreateAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);
        Task<bool> ExistsAsync(int id);
    }
}