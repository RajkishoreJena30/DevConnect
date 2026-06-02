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
GET    /api/posts              → list posts (paginated + sorted)
GET    /api/posts/{id}         → single post
GET    /api/posts/my           → authenticated user's posts
POST   /api/posts              → create post
PUT    /api/posts/{id}         → update post (owner only)
DELETE /api/posts/{id}         → delete post (owner or Admin)

POST   /api/auth/register      → register user
POST   /api/auth/login         → login, returns JWT
GET    /api/auth/login-google  → redirect to Google OAuth
GET    /api/auth/login-github  → redirect to GitHub OAuth
GET    /api/auth/callback      → OIDC callback, issues JWT
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
GET /api/auth/login-google  →  redirect to Google
Google redirects back       →  /api/auth/callback
Callback: find or create user in DB, issue DevConnect JWT
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
`DevConnect.Tests/Services/PostServiceTests.cs`, `DevConnect.Tests/Controllers/PostsControllerTests.cs`, `DevConnect.Tests/Validators/ValidatorTests.cs`.

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
