using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Models;

namespace DevConnect.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── POST MAPPINGS ──────────────────────────────────────

            // Post Model → PostResponseDTO
            // AutoMapper auto maps: Id, Title, Content, UserId, CreatedAt, UpdatedAt
            // Manual config needed for: AuthorName, LikesCount, CommentsCount
            CreateMap<Post, PostResponseDTO>()
                .ForMember(
                    dest => dest.AuthorName,           // destination field
                    opt => opt.MapFrom(src => src.User.Name))  // source field (navigation)
                .ForMember(
                    dest => dest.LikesCount,
                    opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(
                    dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments.Count));

            // CreatePostDTO → Post Model (when creating a new post)
            // Ignore UserId — we set it manually in PostService from JWT token
            CreateMap<CreatePostDTO, Post>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // ── COMMENT MAPPINGS ───────────────────────────────────

            // Comment Model → CommentResponseDTO
            CreateMap<Comment, CommentResponseDTO>()
                .ForMember(
                    dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.User.Name));

            // ── USER/AUTH MAPPINGS ─────────────────────────────────

            // RegisterDTO → User Model (when registering)
            // Ignore PasswordHash — we set it manually after BCrypt hashing
            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}