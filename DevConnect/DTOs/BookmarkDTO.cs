namespace DevConnect.DTOs
{
    // Output of the toggle endpoint
    public class BookmarkResponseDTO
    {
        public bool Bookmarked { get; set; }   // true = saved, false = removed
        public int PostId { get; set; }
    }

    // Query params for "my bookmarks" — reuses your pagination/sorting idea
    // and adds a 🆕 Search term (filtering).
    public class BookmarkQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "createdAt";   // createdAt | title
        public string SortDirection { get; set; } = "desc"; // asc | desc
        public string? Search { get; set; }                 // 🆕 filter by title/content
    }

    // Output of the aggregate stats endpoint 🆕
    public class BookmarkStatsDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int BookmarkCount { get; set; }
    }
}
