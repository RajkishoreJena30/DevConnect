using DevConnect.DTOs;

namespace DevConnect.Interfaces
{
    public interface IBookmarkService
    {
        Task<BookmarkResponseDTO?> ToggleBookmarkAsync(int userId, int postId);
        Task<PagedResult<PostResponseDTO>> GetMyBookmarksAsync(int userId, BookmarkQueryParams query);
        Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take);
    }
}