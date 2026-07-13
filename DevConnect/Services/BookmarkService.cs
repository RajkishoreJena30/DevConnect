using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;

namespace DevConnect.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _repo;
        private readonly IMapper _mapper;

        public BookmarkService(IBookmarkRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // 🆕 Idempotent toggle: one endpoint that saves OR removes.
        // Returns null when the post doesn't exist.
        public async Task<BookmarkResponseDTO?> ToggleBookmarkAsync(int userId, int postId)
        {
            if (!await _repo.PostExistsAsync(postId)) return null;

            var existing = await _repo.GetAsync(userId, postId);

            if (existing != null)
            {
                await _repo.RemoveAsync(existing);
                return new BookmarkResponseDTO { Bookmarked = false, PostId = postId };
            }

            await _repo.AddAsync(new Bookmark { UserId = userId, PostId = postId });
            return new BookmarkResponseDTO { Bookmarked = true, PostId = postId };
        }

        public async Task<PagedResult<PostResponseDTO>> GetMyBookmarksAsync(
            int userId, BookmarkQueryParams query)
        {
            query.PageNumber = Math.Max(1, query.PageNumber);
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);

            var (posts, totalCount) = await _repo.GetMyBookmarkedPostsAsync(userId, query);

            return new PagedResult<PostResponseDTO>
            {
                Items = _mapper.Map<List<PostResponseDTO>>(posts),  // reuses existing Post→DTO map
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take) =>
            _repo.GetTopBookmarkedAsync(Math.Clamp(take, 1, 50));
    }
}