namespace DevConnect.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                public DateTime? UpdatedAt { get; set; }   // ← add this (nullable — only set on edit)

        // Foreign Key — links Post to a User
        public int UserId { get; set; }

        // Navigation Property — EF Core uses this to JOIN tables
        public User User { get; set; } = null!;
        public ICollection<Like> Likes { get; set;} 
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Bookmark> Bookmarks { get; set; }
    }
}
