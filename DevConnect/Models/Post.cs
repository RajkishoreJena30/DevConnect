namespace DevConnect.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key — links Post to a User
        public int UserId { get; set; }

        // Navigation Property — EF Core uses this to JOIN tables
        public User User { get; set; } = null!;
    }
}
