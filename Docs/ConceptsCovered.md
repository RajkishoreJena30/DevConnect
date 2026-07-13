# DevConnect — All Concepts Covered

> A single-page reference for every concept implemented in this project:
> what it is, where it lives, and how it works.

---

## Master Status Table

| # | Concept | Status | Primary File(s) |
|---|---------|--------|-----------------|
| 1 | REST API | ✅ Implemented | All controllers |
| 2 | Dependency Injection | ✅ Implemented | `Program.cs` |
| 3 | DTOs | ✅ Implemented | `DTOs/UserDto.cs`, `DTOs/PostInteractionDTO.cs` |
| 4 | AutoMapper | ✅ Implemented | `Mappings/MappingProfile.cs` |
| 5 | FluentValidation | ✅ Implemented | `Validators/AuthValidators.cs`, `Validators/PostValidators.cs` |
| 6 | Service-Repository Pattern | ✅ Implemented | `Interfaces/`, `Services/`, `Repositories/` |
| 7 | Entity Framework Core | ✅ Implemented | `Data/`, `Migrations/`, `Models/` |
| 8 | JWT Authentication | ✅ Implemented | `Services/AuthService.cs`, `Controllers/AuthController.cs` |
| 9 | OIDC / OAuth (Google + GitHub) | ✅ Implemented | `Program.cs`, `Controllers/AuthController.cs` |
| 10 | Role-Based Authorization | ✅ Implemented | `Models/User.cs`, `Controllers/` |
| 11 | CORS | ✅ Implemented | `Program.cs` |
| 12 | Swagger / OpenAPI | ✅ Implemented | `Program.cs` |
| 13 | Pagination | ✅ Implemented | `DTOs/`, `Interfaces/`, `Repositories/`, `Services/`, `Controllers/PostsController.cs` |
| 14 | Sorting | ✅ Implemented | `Repositories/PostRepository.cs`, `DTOs/PostInteractionDTO.cs` |
| 15 | Output Caching | ✅ Implemented | `Program.cs`, `Controllers/PostsController.cs` |
| 16 | Serilog Structured Logging | ✅ Implemented | `Program.cs`, `appsettings.json` |
| 17 | Unit Testing | ✅ Implemented | `DevConnect.Tests/Services/`, `DevConnect.Tests/Controllers/` |
| 18 | Integration Testing | ✅ Implemented | `DevConnect.Tests/Integration/` |

---

## 1. REST API ✅

### What
Architectural style using HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`) on resource-based URLs.

### Where implemented
Every controller in `Controllers/`.

### How
```
GET    /api/posts              → list posts (paginated + sorted, cached)
GET    /api/posts/{id}         → single post (cached)
GET    /api/posts/my           → authenticated user's posts
POST   /api/posts              → create post
PUT    /api/posts/{id}         → update post (owner only)
DELETE /api/posts/{id}         → delete post (owner or Admin)

GET    /api/posts/{postId}/comments            → list comments for a post
POST   /api/posts/{postId}/comments            → add comment
PUT    /api/posts/{postId}/comments/{id}       → update comment (owner)
DELETE /api/posts/{postId}/comments/{id}       → delete comment (owner or Admin)

GET    /api/posts/{postId}/likes   → like count + liked-by-me (anonymous allowed)
POST   /api/posts/{postId}/likes   → toggle like/unlike

POST   /api/auth/register            → register user
POST   /api/auth/login               → login, returns JWT
GET    /api/auth/google              → redirect to Google OAuth
GET    /api/auth/google/callback     → Google OIDC callback, issues JWT
GET    /api/auth/github              → redirect to GitHub OAuth
GET    /api/auth/github/callback     → GitHub OAuth callback, issues JWT

GET    /api/users/profile   → own profile (auth)
PUT    /api/users/profile   → update own profile (auth)
GET    /api/users           → list all users (Admin only)
DELETE /api/users/{id}      → delete user (Admin only)

GET    /api/books           → list books (early EF Core learning feature)
GET    /api/books/{id}       → single book
```

HTTP status codes used: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`.

---

## 2. Dependency Injection ✅

### What
ASP.NET Core's built-in IoC container. Services are registered once and injected into constructors automatically.

### Where implemented
`Program.cs` — all service registrations.

### How
```csharp
// Scoped — one instance per HTTP request
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Singleton-like — AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Validators — scanned from assembly
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
```

Controllers receive dependencies via constructor parameters — never created manually.

---

## 3. DTOs (Data Transfer Objects) ✅

### What
Objects that carry only the data a layer needs — prevent leaking internal model fields (e.g. `PasswordHash`) to the client.

### Where implemented
`DTOs/UserDto.cs` and `DTOs/PostInteractionDTO.cs`.

### How

| DTO | Direction | Purpose |
|-----|-----------|---------|
| `RegisterDTO` | Input | User registration |
| `LoginDTO` | Input | Login credentials |
| `AuthResponseDTO` | Output | JWT token + user info |
| `UpdateProfileDTO` | Input | Profile update |
| `CreatePostDTO` | Input | Create / update a post |
| `PostResponseDTO` | Output | Post with author name |
| `PostQueryParams` | Input | Pagination + sort query params |
| `PagedResult<T>` | Output | Paginated list wrapper |
| `CreateCommentDTO` | Input | New comment body |
| `CommentResponseDTO` | Output | Comment with author name |
| `LikeResponseDTO` | Output | Total likes + liked-by-me flag |

---

## 4. AutoMapper ✅

### What
Library that eliminates manual property-by-property mapping between model and DTO.

### Where implemented
`Mappings/MappingProfile.cs`, registered in `Program.cs`.

### How
```csharp
// MappingProfile.cs
CreateMap<Post, PostResponseDTO>()
    .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.Username));

CreateMap<CreatePostDTO, Post>();
```

Used in `PostService.cs`:
```csharp
var post = _mapper.Map<Post>(dto);             // DTO → Model (create)
_mapper.Map(dto, post);                         // DTO → Model (update in-place)
return _mapper.Map<PostResponseDTO>(created);  // Model → DTO (response)
```

---

## 5. FluentValidation ✅

### What
Validation library that moves validation rules out of models into dedicated validator classes.

### Where implemented
`Validators/AuthValidators.cs`, `Validators/PostValidators.cs`.

### How
```csharp
// AuthValidators.cs
public class RegisterValidator : AbstractValidator<RegisterDTO>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(6);
    }
}
```

Validators are registered from assembly in `Program.cs`:
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
```

Injected and invoked manually in controllers (`IValidator<RegisterDTO>`).

---

## 6. Service-Repository Pattern ✅

### What
Two-layer separation: Repository handles all DB access; Service holds all business logic.

```
Controller → IPostService → IPostRepository → DbContext
```

### Where implemented

| Layer | Interface | Implementation |
|-------|-----------|----------------|
| Repository | `Interfaces/IPostRepository.cs` | `Repositories/PostRepository.cs` |
| Service | `Interfaces/IPostService.cs` | `Services/PostService.cs` |
| Auth Service | `Interfaces/IAuthService.cs` | `Services/AuthService.cs` |

### How
- **Repository** runs raw EF Core queries (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).
- **Service** orchestrates: calls repository, maps models to DTOs, applies business rules (e.g. ownership check before delete).
- **Controller** only parses HTTP input, calls service, returns `IActionResult`.

> **Scope note:** Only the **Posts** feature uses the full Controller → Service → Repository chain.
> `AuthController`, `UsersController`, `CommentsController`, and `LikesController` access `DevConnectDbContext`
> directly (a simpler pattern used earlier in the learning journey). `AuthController` still delegates token
> and OIDC user logic to `IAuthService`.

---

## 6a. Repository Pattern — Benefits & Implementation ✅

### Why use a Repository?

The Repository pattern puts a well-defined interface between business logic and the database.

| Benefit | What it means in DevConnect |
|---------|-----------------------------|
| **Separation of concerns** | `PostService` never writes EF Core queries; it just calls `IPostRepository`. Data-access details stay in one place. |
| **Testability** | Unit tests mock `IPostRepository` with Moq, so `PostService` logic is tested with **no real database**. |
| **Loose coupling (DIP)** | Service depends on the `IPostRepository` **abstraction**, not the concrete `PostRepository` or `DbContext`. |
| **Swappable data source** | The EF Core implementation could be replaced (e.g. Dapper, another DB) without touching the service. |
| **Centralized query logic** | Reusable queries like `GetPagedAsync` (Include + sort + paging) live in one class instead of being duplicated across controllers. |
| **Cleaner services** | Business rules (ownership checks, DTO mapping, bound clamping) are not tangled with `SaveChangesAsync` calls. |

### How it's implemented here

**1. The contract** — `Interfaces/IPostRepository.cs`:
```csharp
public interface IPostRepository
{
    Task<List<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(int id);
    Task<List<Post>> GetByUserIdAsync(int userId);
    Task<Post> CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<bool> ExistsAsync(int id);
    Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(PostQueryParams query);
}
```

**2. The implementation** — `Repositories/PostRepository.cs` (Entity Framework Core (EF Core) only):
```csharp
public class PostRepository : IPostRepository
{
    private readonly DevConnectDbContext _context;
    public PostRepository(DevConnectDbContext context) => _context = context;

    public async Task<Post> CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
    // GetPagedAsync handles Include + sorting + Skip/Take ...
}
```

**3. Registration (DI)** — `Program.cs`:
```csharp
builder.Services.AddScoped<IPostRepository, PostRepository>();
```

**4. Consumption** — `Services/PostService.cs` depends only on the interface:
```csharp
public class PostService : IPostService
{
    private readonly IPostRepository _repo;   // ← abstraction, not DbContext
    private readonly IMapper _mapper;

    public PostService(IPostRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }
    // service applies business rules, then calls _repo.*
}
```

### The payoff — a repository-free unit test
```csharp
var mockRepo = new Mock<IPostRepository>();
mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakePosts);

var service = new PostService(mockRepo.Object, mapper);
var result = await service.GetAllPostsAsync(); // no database involved
```

---

## 7. Entity Framework Core ✅

### What
ORM that maps C# models to database tables and translates LINQ queries to SQL.

### Where implemented
`Data/DevConnectDbContext.cs`, `Models/`, `Migrations/DevConnectDb/`.

### How

**Models:**

| Model | Table | Relationships |
|-------|-------|---------------|
| `User` | Users | Has many Posts, Likes, Comments |
| `Post` | Posts | Belongs to User; has many Likes, Comments |
| `Like` | Likes | Belongs to User and Post |
| `Comment` | Comments | Belongs to User and Post |

**Relationships configured** via EF Core navigation properties (cascade delete on Likes/Comments when Post deleted).

**Migrations** stored in `Migrations/DevConnectDb/` — applied via `dotnet ef database update`.

> **Two DbContexts:** the app registers both `DevConnectDbContext` (main app: Users, Posts, Likes, Comments)
> and `FirstAPIContext` (early `Books` learning feature). Both are registered in `Program.cs` and use the
> same SQL Server connection string.

---

## 8. JWT Authentication ✅

### What
JSON Web Token — a signed, self-contained token the client sends in every request to prove identity.

### Where implemented
- Token generation: `Services/AuthService.cs`
- Token validation middleware: `Program.cs`
- Endpoint protection: `[Authorize]` attributes in controllers

### How
```
1. Client: POST /api/auth/login { email, password }
2. AuthService validates password hash (BCrypt)
3. AuthService creates JWT with claims: userId, email, role
4. Client stores token and sends it as: Authorization: Bearer <token>
5. ASP.NET Core middleware validates signature, expiry, issuer, audience
6. [Authorize] endpoints allow or reject based on token presence
```

Token settings come from `appsettings.json`:
```json
"JwtSettings": {
  "Key": "...",
  "Issuer": "DevConnect",
  "Audience": "DevConnectUsers",
  "ExpiryInDays": 7
}
```

---

## 9. OIDC / OAuth — Google + GitHub ✅

### What
External social login using OAuth2/OIDC — user authenticates with Google or GitHub instead of providing a password.

### Where implemented
- Provider registration: `Program.cs`
- Callback logic: `Controllers/AuthController.cs`
- OIDC fields on user: `Models/User.cs`, `Migrations/DevConnectDb/20260514130622_oidc fields added to user`

### How
```csharp
// Program.cs
.AddGoogle(options =>
{
    options.ClientId = config["Google:ClientId"];
    options.ClientSecret = config["Google:ClientSecret"];
    options.Scope.Add("email");
    options.Scope.Add("profile");
})
.AddGitHub(options =>
{
    options.ClientId = config["GitHub:ClientId"];
    options.ClientSecret = config["GitHub:ClientSecret"];
    options.Scope.Add("user:email");
});
```

Flow:
```
GET /api/auth/google           →  redirect to Google login
GET /api/auth/google/callback  →  Google redirects back here
GET /api/auth/github           →  redirect to GitHub login
GET /api/auth/github/callback  →  GitHub redirects back here
Callback: AuthService.FindOrCreateOidcUserAsync(...) finds/creates user, issues DevConnect JWT
```

---

## 10. Role-Based Authorization ✅

### What
Users are assigned a role; endpoints enforce role requirements using `[Authorize(Roles = "...")]`.

### Where implemented
`Models/User.cs` (Role property), controllers (`[Authorize]`, `[Authorize(Roles = "Admin")]`), `PostService.cs` (ownership + role check).

### How
```csharp
// User model
public string Role { get; set; } = "User"; // "User" | "Admin"

// Stored as claim in JWT
claims.Add(new Claim(ClaimTypes.Role, user.Role));

// Endpoint enforcement
[Authorize(Roles = "Admin")]
public IActionResult GetAllUsers() { ... }

// Ownership + role check in service
if (post.UserId != userId && role != "Admin") return false;
```

---

## 11. CORS ✅

### What
Browser security policy — tells the browser which frontend origins are allowed to call the API.

### Where implemented
`Program.cs` — `AddCors` + `UseCors`.

### How
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                  "http://localhost:3000",  // React
                  "http://localhost:4200"   // Angular
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Middleware order: must come before Authentication
app.UseCors("AllowFrontend");
```

---

## 12. Swagger / OpenAPI ✅

### What
Interactive API documentation UI generated automatically from controller code and attributes.

### Where implemented
`Program.cs` — `AddSwaggerGen` + `UseSwagger` + `UseSwaggerUI`.

### How
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DevConnect API", Version = "v1" });

    // JWT Bearer button in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });
});

// Enabled in development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Access at: `https://localhost:{port}/swagger`

---

## 13. Pagination ✅

### What
Returns a fixed-size page of results instead of the entire table, with metadata about total records.

### Where implemented

| File | Role |
|------|------|
| `DTOs/PostInteractionDTO.cs` | `PostQueryParams` (input), `PagedResult<T>` (output) |
| `Interfaces/IPostRepository.cs` | `GetPagedAsync(PostQueryParams)` contract |
| `Repositories/PostRepository.cs` | `Skip` + `Take` EF Core query |
| `Interfaces/IPostService.cs` | `GetPagedPostsAsync(PostQueryParams)` contract |
| `Services/PostService.cs` | Clamps bounds, maps results, builds `PagedResult<T>` |
| `Controllers/PostsController.cs` | `[FromQuery] PostQueryParams` on `GET /api/posts` |

### How
```csharp
// Repository — core paging logic
var totalCount = await q.CountAsync();
var posts = await q
    .Skip((query.PageNumber - 1) * query.PageSize)
    .Take(query.PageSize)
    .ToListAsync();

// Service — bound clamping
query.PageNumber = Math.Max(1, query.PageNumber);
query.PageSize   = Math.Clamp(query.PageSize, 1, 100);
```

### Response shape
```json
{
  "items": [...],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

### Example request
```
GET /api/posts?pageNumber=2&pageSize=10
```

---

## 14. Sorting ✅

### What
Client chooses which field and direction to order results by.

### Where implemented
`Repositories/PostRepository.cs` — switch expression inside `GetPagedAsync`.

### How
```csharp
q = (query.SortBy.ToLower(), query.SortDirection.ToLower()) switch
{
    ("title",     "asc")  => q.OrderBy(p => p.Title),
    ("title",     _)      => q.OrderByDescending(p => p.Title),
    ("likes",     "asc")  => q.OrderBy(p => p.Likes.Count),
    ("likes",     _)      => q.OrderByDescending(p => p.Likes.Count),
    ("createdat", "asc")  => q.OrderBy(p => p.CreatedAt),
    _                     => q.OrderByDescending(p => p.CreatedAt), // default
};
```

Supported `sortBy` values: `createdAt` (default), `title`, `likes`.  
Supported `sortDirection` values: `desc` (default), `asc`.

### Example request
```
GET /api/posts?sortBy=likes&sortDirection=desc&pageNumber=1&pageSize=5
```

---

## 15. Output Caching ✅

### What
ASP.NET Core 8 built-in response cache — stores the full HTTP response and serves it without hitting the DB again, until invalidated.

### Where implemented

| File | What |
|------|------|
| `Program.cs` | `AddOutputCache` policy + `UseOutputCache` middleware |
| `Controllers/PostsController.cs` | `[OutputCache(PolicyName = "Posts")]` on GET actions + `EvictByTagAsync` on writes |

### How
```csharp
// Program.cs — register policy
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Posts", builder =>
        builder.Expire(TimeSpan.FromSeconds(30))
               .Tag("posts"));
});

app.UseOutputCache(); // before UseAuthentication
```

```csharp
// Controller — cache read endpoints
[HttpGet]
[OutputCache(PolicyName = "Posts")]
public async Task<IActionResult> GetAll([FromQuery] PostQueryParams query) => ...

// Controller — invalidate on every write
await _cache.EvictByTagAsync("posts", HttpContext.RequestAborted);
```

**Invalidation rules:**
- `POST /api/posts` (Create) → always evicts
- `PUT /api/posts/{id}` (Update) → evicts only if update succeeded
- `DELETE /api/posts/{id}` (Delete) → evicts only if delete succeeded
- `GET /api/posts/my` → not cached (per-user, auth-protected)

---

## 16. Serilog Structured Logging ✅

### What
Replaces the default ASP.NET Core logger with Serilog — writes structured (JSON-like) log events to console and rolling log files.

### Where implemented

| File | What |
|------|------|
| `Program.cs` | Bootstrap logger + `UseSerilog` + `UseSerilogRequestLogging` |
| `appsettings.json` | `"Serilog"` configuration section (levels, sinks, enrichers) |
| `DevConnect.csproj` | `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` packages |

### How
```csharp
// Program.cs — early bootstrap logger (before DI is built)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build())
    .CreateBootstrapLogger();

// Replace host logger
builder.Host.UseSerilog((ctx, services, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .ReadFrom.Services(services));

// Per-request HTTP log line
app.UseSerilogRequestLogging();
```

```json
// appsettings.json
"Serilog": {
  "MinimumLevel": { "Default": "Information", "Override": { "Microsoft": "Warning" } },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "Logs/devconnect-.log", "rollingInterval": "Day" } }
  ]
}
```

Existing `ILogger<T>` injections in controllers/services route through Serilog automatically — no controller changes needed.

---

## 17. Unit Testing ✅

### What
Tests that verify a single class in isolation by replacing all dependencies with mocks.

### Where implemented
`DevConnect.Tests/Services/PostServiceTests.cs`, `DevConnect.Tests/Services/AuthServiceTests.cs`, `DevConnect.Tests/Controllers/PostsControllerTests.cs`, `DevConnect.Tests/Controllers/AuthControllerTests.cs`, `DevConnect.Tests/Validators/ValidatorTests.cs`.

### How
- **Framework:** NUnit
- **Mocking:** Moq (mocks `IPostRepository`, `IPostService`, `IMapper`)
- **Pattern:** Arrange → Act → Assert

```csharp
// Example: PostServiceTests
[Test]
public async Task GetAllPostsAsync_ReturnsMappedDTOs()
{
    _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakePosts);
    _mockMapper.Setup(m => m.Map<List<PostResponseDTO>>(fakePosts)).Returns(fakeDTOs);

    var result = await _service.GetAllPostsAsync();

    Assert.That(result, Is.EqualTo(fakeDTOs));
}
```

---

## 18. Integration Testing ✅

### What
Tests the full stack — real database queries — without mocking the repository layer.

### Where implemented
`DevConnect.Tests/Integration/PostRepositoryIntegrationTests.cs`.

### How
- **EF Core InMemory** provider used as lightweight fake DB for repository-level tests.
- **Testcontainers** (Docker-based SQL Server) used for full fidelity when needed.

```csharp
// InMemory setup
var options = new DbContextOptionsBuilder<DevConnectDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Build();
var context = new DevConnectDbContext(options);
var repo = new PostRepository(context);
```

---

## Concept Flow Diagram

```
HTTP Request
     │
     ▼
┌─────────────────────────────┐
│  Serilog Request Logging    │  ← logs method, path, status, elapsed
├─────────────────────────────┤
│  CORS Middleware            │  ← allows/blocks by origin
├─────────────────────────────┤
│  Output Cache               │  ← serve cached response or continue
├─────────────────────────────┤
│  Authentication Middleware  │  ← validates JWT
├─────────────────────────────┤
│  Authorization Middleware   │  ← checks [Authorize] & roles
├─────────────────────────────┤
│  Controller                 │  ← parses input, calls service
│    ↕ FluentValidation       │
├─────────────────────────────┤
│  Service                    │  ← business logic, ownership checks
│    ↕ AutoMapper             │
├─────────────────────────────┤
│  Repository                 │  ← EF Core, pagination, sorting
│    ↕ DbContext              │
├─────────────────────────────┤
│  SQL Server Database        │
└─────────────────────────────┘
```

---

## Backend NuGet Packages — Detailed Reference

> Every package referenced in `DevConnect/DevConnect.csproj`, what it does, and where it is used in the code.

### Data Access — Entity Framework Core

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `Microsoft.EntityFrameworkCore` | 9.0.6 | Core ORM — `DbContext`, `DbSet<T>`, LINQ-to-SQL, change tracking. | `Data/DevConnectDbContext.cs`, `Data/FirstAPIContext.cs`, all repositories/controllers doing queries. |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.4 | SQL Server database provider for EF Core. | `Program.cs` — `options.UseSqlServer(...)` for both DbContexts. |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.4 | Design-time tools for migrations (`Add-Migration`, `Update-Database`). | Used from CLI/PMC; generates files under `Migrations/DevConnectDb/`. |

### Authentication & Security

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.26 | Validates incoming JWT bearer tokens (signature, issuer, audience, expiry). | `Program.cs` — `AddAuthentication().AddJwtBearer(...)`; enforced by `[Authorize]` in controllers. |
| `Microsoft.AspNetCore.Authentication.Google` | 7.0.0 | Google OAuth2 / OIDC login provider. | `Program.cs` — `.AddGoogle(...)`; `AuthController.GoogleLogin` / `GoogleCallback`. |
| `AspNet.Security.OAuth.GitHub` | 7.0.0 | GitHub OAuth login provider (community package). | `Program.cs` — `.AddGitHub(...)`; `AuthController.GitHubLogin` / `GitHubCallback`. |
| `BCrypt.Net-Next` | 3.1.0 | Salted password hashing and verification. | `AuthController` / `AuthService` — `BC.HashPassword(...)` on register, `BC.Verify(...)` on login. |

### Mapping & Validation

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `AutoMapper` | 12.0.1 | Object-to-object mapping between Models and DTOs. | `Mappings/MappingProfile.cs`; `PostService`, `AuthController` via `IMapper`. |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 7.0.0 | Registers AutoMapper into the DI container and scans profiles. | `Program.cs` — `AddAutoMapper(typeof(MappingProfile))`. |
| `FluentValidation` | 11.0.1 | Fluent, class-based input validation rules. | `Validators/AuthValidators.cs`, `Validators/PostValidators.cs`. |
| `FluentValidation.AspNetCore` | 10.3.1 | ASP.NET Core integration helpers for FluentValidation. | Referenced in `Program.cs` (`using FluentValidation.AspNetCore`). |
| `FluentValidation.DependencyInjectionExtensions` | 11.0.1 | Registers validators from an assembly into DI. | `Program.cs` — `AddValidatorsFromAssemblyContaining<RegisterValidator>()`. |

### Logging

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `Serilog.AspNetCore` | 10.0.0 | Serilog integration + request logging middleware. | `Program.cs` — `UseSerilog(...)`, `UseSerilogRequestLogging()`. |
| `Serilog.Settings.Configuration` | 10.0.0 | Reads Serilog config from `appsettings.json`. | `Program.cs` — `ReadFrom.Configuration(...)`; `"Serilog"` section in `appsettings.json`. |
| `Serilog.Sinks.Console` | 6.1.1 | Writes log events to the console. | Configured in `appsettings.json` `WriteTo`. |
| `Serilog.Sinks.File` | 7.0.0 | Writes rolling log files. | `appsettings.json` — file sink to `Logs/devconnect-.log`; output in `Logs/`. |

### API Documentation

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `Swashbuckle.AspNetCore` | 6.6.2 | Generates Swagger / OpenAPI docs and interactive UI (with JWT auth button). | `Program.cs` — `AddSwaggerGen(...)`, `UseSwagger()`, `UseSwaggerUI()`; served at `/swagger`. |

### Test Project Packages (`DevConnect.Tests/DevConnect.Tests.csproj`)

| Package | Version | Purpose | Where used |
|---------|---------|---------|-----------|
| `NUnit` | 4.2.2 | Unit testing framework (`[Test]`, `Assert`). | All test classes under `DevConnect.Tests/`. |
| `NUnit3TestAdapter` | 4.6.0 | Lets the test runner / `dotnet test` discover NUnit tests. | Test discovery/execution. |
| `Microsoft.NET.Test.Sdk` | 17.11.1 | Base test host and build targets. | Required by the test project. |
| `Moq` | 4.20.72 | Mocking framework for dependencies. | `PostServiceTests`, `AuthControllerTests`, etc. — `new Mock<IPostRepository>()`. |
| `Microsoft.EntityFrameworkCore.InMemory` | 9.0.6 | In-memory EF provider for fast repository tests. | `Integration/PostRepositoryIntegrationTests.cs`. |
| `Testcontainers.MsSql` | 3.9.0 | Spins up a real SQL Server in Docker for full-fidelity integration tests. | Integration tests needing a real database. |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.0 | In-memory test host for controller/endpoint tests. | Controller integration tests. |
| `coverlet.collector` | 6.0.0 | Collects code-coverage during `dotnet test`. | Coverage reporting. |
