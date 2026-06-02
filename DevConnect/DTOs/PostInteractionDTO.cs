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

    public class PostQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        // Allowed values: "createdAt" | "title" | "likes"
        public string SortBy { get; set; } = "createdAt";
        // Allowed values: "asc" | "desc"
        public string SortDirection { get; set; } = "desc";
    }


    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
