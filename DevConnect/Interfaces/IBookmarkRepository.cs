using DevConnect.DTOs;
using DevConnect.Models;

namespace DevConnect.Interfaces
{
    public interface IBookmarkRepository
    {
        Task<Bookmark?> GetAsync(int userId, int postId);
        Task AddAsync(Bookmark bookmark);
        Task RemoveAsync(Bookmark bookmark);
        Task<bool> PostExistsAsync(int postId);

        // Paginated + sorted + filtered list of the posts a user saved
        Task<(List<Post> Posts, int TotalCount)> GetMyBookmarkedPostsAsync(
            int userId, BookmarkQueryParams query);

        // 🆕 Aggregate: most-bookmarked posts
        Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take);
    }
}