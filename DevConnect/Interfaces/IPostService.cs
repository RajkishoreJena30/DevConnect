using DevConnect.DTOs;

namespace DevConnect.Interfaces
{
    public interface IPostService
    {
        Task<List<PostResponseDTO>> GetAllPostsAsync();
        Task<PostResponseDTO?> GetPostByIdAsync(int id);
        Task<List<PostResponseDTO>> GetMyPostsAsync(int userId);
        Task<PostResponseDTO> CreatePostAsync(int userId, CreatePostDTO dto);
        Task<bool> UpdatePostAsync(int postId, int userId, CreatePostDTO dto);
        Task<bool> DeletePostAsync(int postId, int userId, string role);
        Task<PagedResult<PostResponseDTO>> GetPagedPostsAsync(PostQueryParams query);
    }
}