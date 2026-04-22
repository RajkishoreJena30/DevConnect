using DevConnect.Models;
using Microsoft.EntityFrameworkCore;
namespace DevConnect.Data
{
    public class FirstAPIContext:DbContext
    {
        public FirstAPIContext(DbContextOptions<FirstAPIContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Books>().HasData(
                  new Books
                  {
                      Id = 1,
                      Title = "Title",
                      Author = "Author",
                      YearOfPublished = 2021
                  },
                new Books
                {
                    Id = 2,
                    Title = "Title2",
                    Author = "Author2",
                    YearOfPublished = 2024
                },
                new Books
                {
                    Id = 3,
                    Title = "Title3",
                    Author = "Author3",
                    YearOfPublished = 2025
                }
              );
        }

        public DbSet<Books> Books { get; set; }
    }
}
