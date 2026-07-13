# Feature: 🔖 Bookmarks / Saved Posts

> A hands-on, step-by-step guide to add a **Bookmarks (Saved Posts)** feature to the
> DevConnect backend. Implement it manually to revise every architectural concept.
> Backend only — frontend comes in a later phase.

---

## Why this feature?

It's a real social-app feature (great demo value) and it naturally exercises *every*
layer of the architecture, while introducing **3 new concepts** not yet covered.

| Concept | Revised (already in project) | 🆕 New (introduced here) |
|---|---|---|
| REST, DI, DTOs, AutoMapper, FluentValidation | ✅ | |
| Service–Repository pattern, EF Core relationships | ✅ | |
| JWT `[Authorize]`, claims, Output-cache invalidation | ✅ | |
| Pagination + Sorting | ✅ | |
| Unit testing (NUnit + Moq) | ✅ | |
| **Filtering / Search** (query a text term) | | 🆕 |
| **Idempotent toggle endpoint** (POST that adds *or* removes) | | 🆕 |
| **Aggregate stats endpoint** (`GroupBy` / count) | | 🆕 |

The `Bookmark` entity is modeled exactly like the existing `Like` (a join between
`User` and `Post` with a unique index), reinforcing a pattern already in the project.

---

## Quick Steps (at a glance)

The end-to-end order to implement any new feature in this project:

1. **Add Model** — new entity (`Models/Bookmark.cs`)
2. **Update existing models** — add navigation props to `User` & `Post`
3. **Update DbContext** — add `DbSet` + relationships in `OnModelCreating`
4. **Add DTOs** — request/response/query objects (`DTOs/BookmarkDTO.cs`)
5. **Add Repository interface** — `Interfaces/IBookmarkRepository.cs`
6. **Add Service interface** — `Interfaces/IBookmarkService.cs`
7. **Add Repository** — EF Core data access (`Repositories/BookmarkRepository.cs`)
8. **Add Service** — business logic + mapping (`Services/BookmarkService.cs`)
9. **Add Validator** — FluentValidation rules (`Validators/BookmarkValidators.cs`)
10. **Add Controller** — REST endpoints (`Controllers/BookmarksController.cs`)
11. **Update `Program.cs`** — register repo + service in DI
12. **Add migration** — `Add-Migration AddBookmarks -Context DevConnectDbContext`
13. **Update database** — `Update-Database -Context DevConnectDbContext`

> Tip: also add unit tests (`DevConnect.Tests/`) and verify in Swagger before wrapping up.

---

## Step 1 — Create the `Bookmark` model

📄 `DevConnect/Models/Bookmark.cs` (mirrors `Like.cs`)

```csharp
namespace DevConnect.Models
{
    public class Bookmark
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

---

## Step 2 — Add navigation collections

📄 `DevConnect/Models/User.cs` — add one line beside `Likes`/`Comments`:

```csharp
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Bookmark> Bookmarks { get; set; }   // 🆕
```

📄 `DevConnect/Models/Post.cs` — add one line beside `Likes`/`Comments`:

```csharp
        public ICollection<Like> Likes { get; set;} 
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Bookmark> Bookmarks { get; set; }   // 🆕
```

---

## Step 3 — Register the entity + relationships in the DbContext

📄 `DevConnect/Data/DevConnectDbContext.cs`

Add the `DbSet`:

```csharp
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }   // 🆕
```

Add fluent config inside `OnModelCreating` (mirrors the `Like` config — cascade on
Post, NoAction on User, unique index to prevent duplicate saves):

```csharp
            // Bookmark → Post (one-to-many, cascade)
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Post)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bookmark → User (one-to-many, no cascade to avoid multiple cascade paths)
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // A user can bookmark a post only once
            modelBuilder.Entity<Bookmark>()
                .HasIndex(b => new { b.PostId, b.UserId })
                .IsUnique();
```

---

## Step 4 — Add DTOs

📄 `DevConnect/DTOs/BookmarkDTO.cs` (new file)

```csharp
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
```

---

## Step 5 — Define the interfaces

📄 `DevConnect/Interfaces/IBookmarkRepository.cs`

```csharp
using DevConnect.DTOs;
using DevConnect.Models;

namespace DevConnect.Interfaces
{
    public interface IBookmarkRepository
    {
        Task<Bookmark?> GetAsync(int userId, int postId);
        Task AddAsync(Bookmark bookmark);
        Task RemoveAsync(Bookmark bookmark);
        Task<bool> PostExistsAsync(int postId);

        // Paginated + sorted + filtered list of the posts a user saved
        Task<(List<Post> Posts, int TotalCount)> GetMyBookmarkedPostsAsync(
            int userId, BookmarkQueryParams query);

        // 🆕 Aggregate: most-bookmarked posts
        Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take);
    }
}
```

📄 `DevConnect/Interfaces/IBookmarkService.cs`

```csharp
using DevConnect.DTOs;

namespace DevConnect.Interfaces
{
    public interface IBookmarkService
    {
        Task<BookmarkResponseDTO?> ToggleBookmarkAsync(int userId, int postId);
        Task<PagedResult<PostResponseDTO>> GetMyBookmarksAsync(int userId, BookmarkQueryParams query);
        Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take);
    }
}
```

---

## Step 6 — Implement the repository

📄 `DevConnect/Repositories/BookmarkRepository.cs`

```csharp
using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace DevConnect.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly DevConnectDbContext _context;

        public BookmarkRepository(DevConnectDbContext context)
        {
            _context = context;
        }

        public async Task<Bookmark?> GetAsync(int userId, int postId) =>
            await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);

        public async Task AddAsync(Bookmark bookmark)
        {
            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Bookmark bookmark)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PostExistsAsync(int postId) =>
            await _context.Posts.AnyAsync(p => p.Id == postId);

        public async Task<(List<Post> Posts, int TotalCount)> GetMyBookmarkedPostsAsync(
            int userId, BookmarkQueryParams query)
        {
            // Start from Bookmarks so ordering by "when I saved it" is possible
            var q = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Post).ThenInclude(p => p.User)
                .Include(b => b.Post).ThenInclude(p => p.Likes)
                .Include(b => b.Post).ThenInclude(p => p.Comments)
                .AsQueryable();

            // 🆕 Filtering / search (case-insensitive contains on title or content)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                q = q.Where(b =>
                    b.Post.Title.Contains(term) || b.Post.Content.Contains(term));
            }

            // Sorting
            q = (query.SortBy.ToLower(), query.SortDirection.ToLower()) switch
            {
                ("title", "asc")  => q.OrderBy(b => b.Post.Title),
                ("title", _)      => q.OrderByDescending(b => b.Post.Title),
                ("createdat","asc") => q.OrderBy(b => b.CreatedAt),
                _                 => q.OrderByDescending(b => b.CreatedAt), // default
            };

            var totalCount = await q.CountAsync();

            var posts = await q
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => b.Post)   // project bookmark → its post
                .ToListAsync();

            return (posts, totalCount);
        }

        // 🆕 Aggregate with GroupBy — "most saved posts"
        public async Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take) =>
            await _context.Bookmarks
                .GroupBy(b => new { b.PostId, b.Post.Title })
                .Select(g => new BookmarkStatsDTO
                {
                    PostId = g.Key.PostId,
                    Title = g.Key.Title,
                    BookmarkCount = g.Count()
                })
                .OrderByDescending(s => s.BookmarkCount)
                .Take(take)
                .ToListAsync();
    }
}
```

---

## Step 7 — Implement the service

📄 `DevConnect/Services/BookmarkService.cs`

```csharp
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
```

> Note: no new AutoMapper mapping is needed for the list — it reuses the existing
> `Post → PostResponseDTO` map. That reinforces *why* DTO mapping is centralized.

---

## Step 8 — (Optional) FluentValidation for the query

📄 `DevConnect/Validators/BookmarkValidators.cs`

```csharp
using DevConnect.DTOs;
using FluentValidation;

namespace DevConnect.Validators
{
    public class BookmarkQueryValidator : AbstractValidator<BookmarkQueryParams>
    {
        public BookmarkQueryValidator()
        {
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.SortBy)
                .Must(s => s is "createdAt" or "title")
                .WithMessage("SortBy must be 'createdAt' or 'title'.");
            RuleFor(x => x.Search)
                .MaximumLength(100).When(x => x.Search != null);
        }
    }
}
```

---

## Step 9 — Create the controller

📄 `DevConnect/Controllers/BookmarksController.cs`

```csharp
using DevConnect.DTOs;
using DevConnect.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevConnect.Controllers
{
    [ApiController]
    public class BookmarksController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarksController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // POST api/posts/5/bookmark  → toggle save/unsave
        [HttpPost("api/posts/{postId}/bookmark")]
        [Authorize]
        public async Task<IActionResult> Toggle(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookmarkService.ToggleBookmarkAsync(userId, postId);
            return result == null ? NotFound("Post not found.") : Ok(result);
        }

        // GET api/bookmarks?pageNumber=1&pageSize=10&sortBy=title&search=react
        [HttpGet("api/bookmarks")]
        [Authorize]
        public async Task<IActionResult> GetMyBookmarks([FromQuery] BookmarkQueryParams query)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _bookmarkService.GetMyBookmarksAsync(userId, query));
        }

        // GET api/bookmarks/top?take=5  → 🆕 trending / most-saved (public)
        [HttpGet("api/bookmarks/top")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTop([FromQuery] int take = 5)
        {
            return Ok(await _bookmarkService.GetTopBookmarkedAsync(take));
        }
    }
}
```

---

## Step 10 — Register in the DI container

📄 `DevConnect/Program.cs` — beside the existing `AddScoped` lines:

```csharp
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
```

---

## Step 11 — Create & apply the EF migration

Because there are **two** DbContexts, you must specify the context.

**Option A — .NET CLI** (run from the solution root):

```powershell
dotnet ef migrations add AddBookmarks --project DevConnect --startup-project DevConnect --context DevConnectDbContext
dotnet ef database update --project DevConnect --startup-project DevConnect --context DevConnectDbContext
```

**Option B — Package Manager Console** (Visual Studio, uses EF Core cmdlets):

```powershell
Add-Migration AddBookmarks -Context DevConnectDbContext -Project DevConnect -StartupProject DevConnect
Update-Database -Context DevConnectDbContext -Project DevConnect -StartupProject DevConnect

Add-Migration AddBookmarks -Context DevConnectDbContext
Update-Database -Context DevConnectDbContext

```

> PMC requires the `Microsoft.EntityFrameworkCore.Tools` package. If `DevConnect` is
> already the startup project and the **Default project** dropdown, you can shorten to
> `Add-Migration AddBookmarks -Context DevConnectDbContext`. To undo the last
> (unapplied) migration: `Remove-Migration -Context DevConnectDbContext`.

---

## Step 12 — Unit tests (NUnit + Moq)

📄 `DevConnect.Tests/Services/BookmarkServiceTests.cs`

```csharp
using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using DevConnect.Services;
using Moq;

namespace DevConnect.Tests.Services;

[TestFixture]
public class BookmarkServiceTests
{
    private Mock<IBookmarkRepository> _repoMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private BookmarkService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IBookmarkRepository>();
        _mapperMock = new Mock<IMapper>();
        _service = new BookmarkService(_repoMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Toggle_WhenPostMissing_ReturnsNull()
    {
        _repoMock.Setup(r => r.PostExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Toggle_WhenNotYetBookmarked_AddsAndReturnsTrue()
    {
        _repoMock.Setup(r => r.PostExistsAsync(5)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetAsync(1, 5)).ReturnsAsync((Bookmark?)null);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 5);

        Assert.That(result!.Bookmarked, Is.True);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Bookmark>()), Times.Once);
    }

    [Test]
    public async Task Toggle_WhenAlreadyBookmarked_RemovesAndReturnsFalse()
    {
        var existing = new Bookmark { Id = 3, UserId = 1, PostId = 5 };
        _repoMock.Setup(r => r.PostExistsAsync(5)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetAsync(1, 5)).ReturnsAsync(existing);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 5);

        Assert.That(result!.Bookmarked, Is.False);
        _repoMock.Verify(r => r.RemoveAsync(existing), Times.Once);
    }
}
```

Run:

```powershell
dotnet test


dotnet test --filter "FullyQualifiedName!~Integration"
```

---

## Step 13 — Try it in Swagger

1. `dotnet run --project DevConnect`
2. Login (`/api/auth/login`) → copy the JWT → click **Authorize** in Swagger.
3. `POST /api/posts/1/bookmark` → returns `{ "bookmarked": true }`; call again → `false` (toggle 🆕).
4. `GET /api/bookmarks?search=react&sortBy=title` → paginated, filtered, sorted list.
5. `GET /api/bookmarks/top?take=5` → aggregate stats (public).

---

## Optional bonus showcase concepts (later, quick wins)

- **Rate limiting** (.NET 8 built-in `AddRateLimiter`) — throttle the toggle endpoint.
- **Health checks** (`AddHealthChecks().AddDbContextCheck<DevConnectDbContext>()` → `/health`).
- **Global exception middleware** returning RFC-7807 ProblemDetails.
