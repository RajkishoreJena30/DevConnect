using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;

namespace DevConnect.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;   // talks to DB
        private readonly IMapper _mapper;         // converts Model ↔ DTO

        // Both injected via DI — registered in Program.cs
        public PostService(IPostRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Get all posts → map Post model to PostResponseDTO
        public async Task<List<PostResponseDTO>> GetAllPostsAsync()
        {
            var posts = await _repo.GetAllAsync();
            return _mapper.Map<List<PostResponseDTO>>(posts);
        }

        // Get single post by ID → return null if not found
        public async Task<PostResponseDTO?> GetPostByIdAsync(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            return post == null ? null : _mapper.Map<PostResponseDTO>(post);
        }

        // Get posts for the currently logged in user
        public async Task<List<PostResponseDTO>> GetMyPostsAsync(int userId)
        {
            var posts = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<List<PostResponseDTO>>(posts);
        }

        // Create new post — userId comes from JWT token in controller
        public async Task<PostResponseDTO> CreatePostAsync(int userId, CreatePostDTO dto)
        {
            var post = _mapper.Map<Post>(dto);  // map DTO → Model
            post.UserId = userId;               // assign owner
            var created = await _repo.CreateAsync(post);
            return _mapper.Map<PostResponseDTO>(created); // map Model → DTO
        }

        // Update post — only owner can update
        public async Task<bool> UpdatePostAsync(int postId, int userId, CreatePostDTO dto)
        {
            var post = await _repo.GetByIdAsync(postId);
            if (post == null || post.UserId != userId) return false; // not found or not owner

            _mapper.Map(dto, post);            // map new values onto existing model
            post.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(post);
            return true;
        }

        // Delete post — owner OR Admin can delete
        public async Task<bool> DeletePostAsync(int postId, int userId, string role)
        {
            var post = await _repo.GetByIdAsync(postId);
            if (post == null) return false;
            if (post.UserId != userId && role != "Admin") return false; // forbidden

            await _repo.DeleteAsync(post);
            return true;
        }
    }
}