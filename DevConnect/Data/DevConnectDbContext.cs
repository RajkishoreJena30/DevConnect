using Microsoft.EntityFrameworkCore;
using DevConnect.Models;

namespace DevConnect.Data
{
    public class DevConnectDbContext : DbContext
    {
        public DevConnectDbContext(DbContextOptions<DevConnectDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One User has many Posts — cascade delete
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
