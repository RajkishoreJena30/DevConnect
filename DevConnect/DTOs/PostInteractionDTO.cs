namespace DevConnect.DTOs
{
    public class CreateCommentDTO
    {
        public string Content { get; set; } = string.Empty;
    }


    public class CommentResponseDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class LikeResponseDTO
    {
        public int TotalLikes { get; set; }
        public bool LikedByMe { get; set; }
    }
}
