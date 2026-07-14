# C# Concepts — Basic to Advanced (Hands-On Roadmap)

> A structured, level-by-level reference of the C# concepts needed to hand-write
> good-quality, layered feature code (like the DevConnect **Bookmarks** feature).
> Each concept includes a **definition**, a short **description**, and a **code example**.
> Master each level before moving to the next.

---

## Table of Contents

- [Level 1 — Basics](#level-1--basics)
  - [1. Types & Properties](#1-types--properties)
  - [2. Access Modifiers & Namespaces](#2-access-modifiers--namespaces)
  - [3. Constructors & Object Initializers](#3-constructors--object-initializers)
  - [4. Methods & Parameters](#4-methods--parameters)
  - [5. Control Flow](#5-control-flow)
- [Level 2 — Core C#](#level-2--core-c)
  - [6. Collections](#6-collections)
  - [7. Null Handling](#7-null-handling)
  - [8. String Operations](#8-string-operations)
  - [9. Static Helpers](#9-static-helpers)
- [Level 3 — The Two That Matter Most](#level-3--the-two-that-matter-most)
  - [10. async / await & Task](#10-asyncawait--task)
  - [11. LINQ & Deferred Execution](#11-linq--deferred-execution)
  - [12. IQueryable vs IEnumerable](#12-iqueryable-vs-ienumerable)
- [Level 4 — Abstraction & Design](#level-4--abstraction--design)
  - [13. Interfaces](#13-interfaces)
  - [14. Generics](#14-generics)
  - [15. Dependency Injection & Lifetimes](#15-dependency-injection--lifetimes)
  - [16. Separation of Concerns](#16-separation-of-concerns)
- [Level 5 — Advanced / Idiomatic Polish](#level-5--advanced--idiomatic-polish)
  - [17. Pattern Matching & switch Expressions](#17-pattern-matching--switch-expressions)
  - [18. Value Tuples](#18-value-tuples)
  - [19. Expression-Bodied Members](#19-expression-bodied-members)
  - [20. Records](#20-records)
  - [21. Extension Methods](#21-extension-methods)
  - [22. Exception Handling](#22-exception-handling)
- [Level 6 — Expert Depth](#level-6--expert-depth)
  - [23. Expression Trees (IQueryable internals)](#23-expression-trees-iqueryable-internals)
  - [24. CancellationToken](#24-cancellationtoken)
  - [25. IAsyncEnumerable & Streaming](#25-iasyncenumerable--streaming)
  - [26. Concurrency & Race Conditions](#26-concurrency--race-conditions)
- [The 20% That Gives 80% Value](#the-20-that-gives-80-value)

---

# Level 1 — Basics

## 1. Types & Properties

**Definition:** A *type* defines the shape of data (a `class`, `struct`, `record`, `enum`, or built-in type). A *property* is a member that exposes data with `get`/`set` accessors.

**Description:** Classes are reference types (heap, shared by reference); structs are value types (copied by value). Auto-implemented properties generate the backing field for you. This is the foundation of every model and DTO.

```csharp
public class Bookmark
{
    public int Id { get; set; }                       // auto-property
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // default value
}

// Value type (struct) — copied, not referenced
public struct Point { public int X; public int Y; }
```

---

## 2. Access Modifiers & Namespaces

**Definition:** *Access modifiers* (`public`, `private`, `protected`, `internal`) control visibility. A *namespace* groups related types under a logical name.

**Description:** Keep internals `private` and expose only what callers need (`public`). Namespaces prevent name clashes and organize the codebase by layer.

```csharp
namespace DevConnect.Services   // logical grouping
{
    public class BookmarkService          // visible everywhere
    {
        private readonly IBookmarkRepository _repo;  // hidden from outside
    }
}
```

---

## 3. Constructors & Object Initializers

**Definition:** A *constructor* runs when an object is created. An *object initializer* sets properties inline right after construction using `{ }`.

**Description:** Use constructors to require mandatory dependencies (great with DI). Use object initializers for quick, readable object creation.

```csharp
public class BookmarkService
{
    private readonly IBookmarkRepository _repo;

    public BookmarkService(IBookmarkRepository repo)  // constructor injection
    {
        _repo = repo;
    }
}

// Object initializer
var bookmark = new Bookmark { UserId = 1, PostId = 5 };
```

---

## 4. Methods & Parameters

**Definition:** A *method* is a named block of code with a return type and optional parameters (including default, `ref`, `out`, and `params`).

**Description:** Methods are your units of behavior. Optional parameters give sensible defaults; `out` returns extra values.

```csharp
// Optional parameter with a default value
public Task<List<BookmarkStatsDTO>> GetTopBookmarkedAsync(int take = 5) => ...

// out parameter
public bool TryParseId(string raw, out int id) => int.TryParse(raw, out id);
```

---

## 5. Control Flow

**Definition:** Statements that decide which code runs: `if`/`else`, `switch`, loops (`for`, `foreach`, `while`), and the ternary operator `?:`.

**Description:** Prefer **guard clauses** (early returns) to reduce nesting and keep methods flat and readable.

```csharp
public async Task<BookmarkResponseDTO?> ToggleBookmarkAsync(int userId, int postId)
{
    if (!await _repo.PostExistsAsync(postId))
        return null;                       // guard clause — exit early

    var existing = await _repo.GetAsync(userId, postId);
    return existing != null                // ternary
        ? new BookmarkResponseDTO { Bookmarked = false, PostId = postId }
        : new BookmarkResponseDTO { Bookmarked = true, PostId = postId };
}
```

---

# Level 2 — Core C#

## 6. Collections

**Definition:** Data structures that hold multiple items: `List<T>`, arrays, `Dictionary<TKey,TValue>`, `HashSet<T>`, and interfaces like `IEnumerable<T>`, `ICollection<T>`, `IReadOnlyList<T>`.

**Description:** Choose by need — `List<T>` for ordered mutable lists, `Dictionary` for key lookups, `HashSet` for uniqueness. Navigation properties typically use `ICollection<T>` because EF Core needs `Add`/`Remove` support without committing to a concrete list type.

```csharp
public class Post
{
    public ICollection<Like> Likes { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Bookmark> Bookmarks { get; set; }
}

var idToName = new Dictionary<int, string> { [1] = "Alice", [2] = "Bob" };
var unique = new HashSet<int> { 1, 2, 2, 3 };   // → { 1, 2, 3 }
```

---

## 7. Null Handling

**Definition:** Features for representing and safely working with the absence of a value: nullable value types (`int?`), **nullable reference types** (`string?`), and operators `??`, `??=`, `?.`, and the null-forgiving `!`.

**Description:** With nullable reference types enabled, the compiler warns about possible `null` dereferences. Use `?.` to short-circuit, `??` to provide fallbacks, and `null!` only when you *know* a value is set (e.g., EF-populated navigation props).

```csharp
public class Bookmark
{
    public Post Post { get; set; } = null!;   // EF sets this; suppress the warning
}

string? search = query.Search;               // may be null
var term = search?.Trim() ?? string.Empty;   // null-safe + fallback

Bookmark? existing = await _repo.GetAsync(userId, postId);
if (existing is null) { /* not bookmarked yet */ }
```

---

## 8. String Operations

**Definition:** Built-in methods on `string` for searching, trimming, casing, and formatting text.

**Description:** Common in filtering/search logic. Prefer `string.IsNullOrWhiteSpace` over manual null+empty checks. Use string interpolation (`$"..."`) for readable formatting.

```csharp
if (!string.IsNullOrWhiteSpace(query.Search))
{
    var term = query.Search.Trim();
    q = q.Where(b => b.Post.Title.Contains(term)
                  || b.Post.Content.Contains(term));
}

var msg = $"User {userId} bookmarked post {postId}";  // interpolation
```

---

## 9. Static Helpers

**Definition:** Utility methods on static classes (e.g., `Math`, `string`, `Convert`) that don't need an instance.

**Description:** Use them to keep logic concise and correct — e.g., clamp user-supplied paging values to a safe range.

```csharp
query.PageNumber = Math.Max(1, query.PageNumber);       // never below 1
query.PageSize   = Math.Clamp(query.PageSize, 1, 100);  // keep within 1..100
```

---

# Level 3 — The Two That Matter Most

## 10. async / await & Task

**Definition:** `Task`/`Task<T>` represents an asynchronous operation; `async`/`await` let you write non-blocking code that reads like synchronous code.

**Description:** I/O-bound work (DB, HTTP) should be async so threads aren't blocked waiting. `await` unwraps the result and yields control until the operation completes. Never use `async void` (except event handlers) — you can't await it or catch its exceptions. Use `Task.WhenAll` to run independent tasks in parallel.

```csharp
public async Task<bool> PostExistsAsync(int postId) =>
    await _context.Posts.AnyAsync(p => p.Id == postId);

// Run independent calls in parallel, then await both
var postTask     = _repo.GetAsync(userId, postId);
var existsTask   = _repo.PostExistsAsync(postId);
await Task.WhenAll(postTask, existsTask);
var post   = postTask.Result;
var exists = existsTask.Result;
```

---

## 11. LINQ & Deferred Execution

**Definition:** *Language Integrated Query* — a uniform syntax (`Where`, `Select`, `OrderBy`, `GroupBy`, `Count`, etc.) for querying collections and databases. *Deferred execution* means the query runs only when enumerated (`ToList`, `Count`, `foreach`).

**Description:** You compose a query in steps; nothing executes until a terminal operation forces it. This is why EF Core can translate the whole chain into one SQL statement.

```csharp
var q = _context.Bookmarks.Where(b => b.UserId == userId);  // NOT executed yet

if (!string.IsNullOrWhiteSpace(query.Search))
    q = q.Where(b => b.Post.Title.Contains(query.Search));  // still building

int total = await q.CountAsync();          // executes now (SELECT COUNT)
var page  = await q.Skip(0).Take(10).ToListAsync();  // executes now (SELECT ...)
```

**Aggregation with `GroupBy`:**

```csharp
var stats = await _context.Bookmarks
    .GroupBy(b => new { b.PostId, b.Post.Title })
    .Select(g => new BookmarkStatsDTO
    {
        PostId = g.Key.PostId,
        Title = g.Key.Title,
        BookmarkCount = g.Count()
    })
    .OrderByDescending(s => s.BookmarkCount)
    .Take(5)
    .ToListAsync();
```

---

## 12. IQueryable vs IEnumerable

**Definition:** `IQueryable<T>` builds an **expression tree** translated to the data source (SQL); `IEnumerable<T>` executes LINQ **in memory** using delegates.

**Description:** The single most important EF concept. If you keep working with `IQueryable`, filtering/sorting/paging happens in the **database**. The moment you call `ToList()`/`AsEnumerable()`, everything after runs **in your app's memory** — which can pull huge datasets.

```csharp
// GOOD: filter runs in SQL (efficient)
IQueryable<Bookmark> q = _context.Bookmarks.Where(b => b.UserId == userId);
var page = await q.Skip(0).Take(10).ToListAsync();

// BAD: ToList() pulls ALL rows into memory, THEN filters in the app
var all = await _context.Bookmarks.ToListAsync();
var slow = all.Where(b => b.UserId == userId).Take(10);  // in-memory
```

---

# Level 4 — Abstraction & Design

## 13. Interfaces

**Definition:** A contract that declares members a type must implement, with no implementation itself.

**Description:** Interfaces enable loose coupling and testability — depend on the abstraction, not the concrete class. Services depend on `IBookmarkRepository`, so tests can substitute a mock.

```csharp
public interface IBookmarkRepository
{
    Task<Bookmark?> GetAsync(int userId, int postId);
    Task AddAsync(Bookmark bookmark);
    Task RemoveAsync(Bookmark bookmark);
    Task<bool> PostExistsAsync(int postId);
}

public class BookmarkRepository : IBookmarkRepository { /* EF Core impl */ }
```

---

## 14. Generics

**Definition:** Types and methods parameterized by a type argument (`T`), enabling reuse with compile-time type safety.

**Description:** Avoids duplication and casting. `PagedResult<T>` works for posts, comments, or anything else without rewriting the wrapper.

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// Reused for different element types
PagedResult<PostResponseDTO> bookmarks = ...;
PagedResult<CommentResponseDTO> comments = ...;
```

---

## 15. Dependency Injection & Lifetimes

**Definition:** *DI* supplies a class's dependencies from the outside (usually via constructor). *Lifetimes* control how long the container reuses an instance: `Transient` (new each time), `Scoped` (one per request), `Singleton` (one for the app).

**Description:** Register abstractions to implementations once; the container builds the object graph. `DbContext` and repositories/services are **Scoped** — one instance per HTTP request — because `DbContext` is not thread-safe and tracks per-request state.

```csharp
// Program.cs
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();

// Consumed via constructor — never 'new'ed manually
public BookmarkService(IBookmarkRepository repo, IMapper mapper) { ... }
```

| Lifetime | New instance | Typical use |
|----------|--------------|-------------|
| `Transient` | Every injection | Lightweight, stateless helpers |
| `Scoped` | Once per request | `DbContext`, repositories, services |
| `Singleton` | Once per app | Caches, config, stateless singletons |

---

## 16. Separation of Concerns

**Definition:** Each layer/class has one responsibility; concerns don't bleed across boundaries.

**Description:** Controller parses HTTP and returns status codes; Service holds business rules and mapping; Repository does data access only. This keeps code testable and changeable.

```csharp
// Controller — HTTP only
[HttpPost("api/posts/{postId}/bookmark")]
[Authorize]
public async Task<IActionResult> Toggle(int postId)
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await _bookmarkService.ToggleBookmarkAsync(userId, postId);
    return result == null ? NotFound("Post not found.") : Ok(result);
}

// Service — business rules   |   Repository — EF Core queries only
```

---

# Level 5 — Advanced / Idiomatic Polish

## 17. Pattern Matching & switch Expressions

**Definition:** Concise syntax to test a value's shape/type and extract data: `is` patterns, property patterns, and `switch` **expressions** that return a value.

**Description:** Replaces long `if/else` chains with readable, exhaustive branches. Tuple patterns are perfect for multi-key decisions like sorting.

```csharp
q = (query.SortBy.ToLower(), query.SortDirection.ToLower()) switch
{
    ("title",     "asc") => q.OrderBy(b => b.Post.Title),
    ("title",     _)     => q.OrderByDescending(b => b.Post.Title),
    ("createdat", "asc") => q.OrderBy(b => b.CreatedAt),
    _                    => q.OrderByDescending(b => b.CreatedAt), // default
};

// is / property patterns
if (result is { Bookmarked: true }) { /* just saved */ }
```

---

## 18. Value Tuples

**Definition:** Lightweight, ad-hoc grouping of multiple values with optional names: `(List<Post> Posts, int TotalCount)`.

**Description:** Great for returning two related values without creating a class. If the shape is reused widely or gains behavior, promote it to a named type instead.

```csharp
public async Task<(List<Post> Posts, int TotalCount)> GetMyBookmarkedPostsAsync(
    int userId, BookmarkQueryParams query)
{
    var total = await q.CountAsync();
    var posts = await q.Skip(0).Take(query.PageSize).Select(b => b.Post).ToListAsync();
    return (posts, total);
}

// Deconstruction at the call site
var (posts, totalCount) = await _repo.GetMyBookmarkedPostsAsync(userId, query);
```

---

## 19. Expression-Bodied Members

**Definition:** A concise `=>` syntax for members whose body is a single expression.

**Description:** Reduces boilerplate for simple methods, properties, and constructors.

```csharp
public async Task<Bookmark?> GetAsync(int userId, int postId) =>
    await _context.Bookmarks
        .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);

public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
```

---

## 20. Records

**Definition:** A reference type (`record`) or value type (`record struct`) with built-in value equality, concise syntax, and non-destructive mutation via `with`.

**Description:** Ideal for immutable DTOs and value objects where equality should be by content, not reference.

```csharp
public record BookmarkResponse(bool Bookmarked, int PostId);

var a = new BookmarkResponse(true, 5);
var b = a with { Bookmarked = false };   // non-destructive copy
bool same = a == new BookmarkResponse(true, 5);  // true — value equality
```

---

## 21. Extension Methods

**Definition:** Static methods that appear as instance methods on an existing type, declared with `this` on the first parameter.

**Description:** Let you add reusable behavior (like `Skip`/`Take`/`Include`) without modifying the original type. Great for factoring out repeated query logic.

```csharp
public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> source, int pageNumber, int pageSize) =>
        source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
}

// Usage — reads like a built-in method
var page = await q.ApplyPaging(query.PageNumber, query.PageSize).ToListAsync();
```

---

## 22. Exception Handling

**Definition:** Structured error handling with `try`/`catch`/`finally`, custom exception types, and centralized handling middleware.

**Description:** Validate at boundaries and fail fast with guard clauses; reserve exceptions for truly exceptional conditions. Centralize cross-cutting handling (e.g., RFC 7807 ProblemDetails) instead of `try/catch` in every method.

```csharp
// Custom exception
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// Global handler (Program.cs) — one place, consistent responses
builder.Services.AddProblemDetails();
app.UseExceptionHandler();   // maps unhandled exceptions to ProblemDetails
```

---

# Level 6 — Expert Depth

## 23. Expression Trees (IQueryable internals)

**Definition:** `Expression<Func<...>>` represents code **as data** (a tree), which LINQ providers (EF Core) translate into SQL.

**Description:** Explains *why* a lambda on `IQueryable` becomes SQL while the same lambda on `IEnumerable` compiles to a delegate. Also explains why some C# methods "can't be translated."

```csharp
// Compiled delegate — runs in-memory (IEnumerable)
Func<Bookmark, bool> del = b => b.UserId == 1;

// Expression tree — inspected & translated to SQL (IQueryable)
Expression<Func<Bookmark, bool>> expr = b => b.UserId == 1;

_context.Bookmarks.Where(expr);   // → WHERE UserId = 1
```

---

## 24. CancellationToken

**Definition:** A token that signals a request to cancel an in-progress async operation.

**Description:** Propagate the request's `CancellationToken` into async DB calls so work stops when the client disconnects — saving resources.

```csharp
public async Task<List<Post>> GetAsync(int userId, CancellationToken ct) =>
    await _context.Bookmarks
        .Where(b => b.UserId == userId)
        .Select(b => b.Post)
        .ToListAsync(ct);   // cancels cleanly if the request is aborted
```

---

## 25. IAsyncEnumerable & Streaming

**Definition:** `IAsyncEnumerable<T>` streams items asynchronously with `await foreach`, instead of materializing an entire list.

**Description:** Useful for large result sets — process rows as they arrive without loading everything into memory.

```csharp
public async IAsyncEnumerable<Post> StreamBookmarkedPostsAsync(int userId)
{
    await foreach (var b in _context.Bookmarks
        .Where(x => x.UserId == userId)
        .Select(x => x.Post)
        .AsAsyncEnumerable())
    {
        yield return b;
    }
}
```

---

## 26. Concurrency & Race Conditions

**Definition:** Correct behavior when multiple operations run at the same time — preventing duplicate/lost updates via unique constraints or optimistic concurrency tokens.

**Description:** A unique index guards against double-inserts (e.g., bookmarking the same post twice from two rapid clicks). Optimistic concurrency (`[Timestamp]`/`RowVersion`) detects conflicting updates.

```csharp
// DbContext — DB-level guard against duplicate bookmarks
modelBuilder.Entity<Bookmark>()
    .HasIndex(b => new { b.PostId, b.UserId })
    .IsUnique();

// Optimistic concurrency token
public class Post
{
    [Timestamp] public byte[] RowVersion { get; set; } = default!;
}
// A conflicting update throws DbUpdateConcurrencyException
```

---

# The 20% That Gives 80% Value

If you can rebuild a repository + service from an empty file using only these four,
you can hand-write this class of feature at good quality:

1. **`async` / `await`** — non-blocking I/O, `Task<T>`, `Task.WhenAll`
2. **LINQ + `IQueryable` deferred execution** — where the SQL actually runs
3. **Interfaces + Generics + DI** — testable, loosely coupled design
4. **Nullable reference types + Pattern matching** — clean, safe, idiomatic code

> **Practice challenge:** Recreate `BookmarkRepository` and `BookmarkService` from a
> blank file — filter → sort → count → page → project, plus the idempotent toggle —
> then diff against the real implementation in the project.
