# Architecture & Design Patterns in DevConnect

---

## Status Summary

| Concept | Implemented? | Where |
|---------|-------------|-------|
| **DTOs** | ✅ Yes | `DTOs/UserDto.cs`, `DTOs/PostInteractionDTO.cs` |
| **Service-Repository Pattern** | ✅ Yes | `Interfaces/`, `Repositories/PostRepository.cs`, `Services/PostService.cs` |
| **AutoMapper** | ✅ Yes | `Mappings/MappingProfile.cs` — Post, Comment, User mappings |
| **FluentValidation** | ✅ Yes | `Validators/AuthValidators.cs`, `Validators/PostValidators.cs` |
| **SOLID** | ⚠️ Partial | SRP partially followed; others not applied |
| **DRY** | ⚠️ Partial | `GetUserId()` logic repeated in every controller |

---

## 1. DTOs (Data Transfer Objects) ✅ Implemented

### What is a DTO?
A DTO is an object used to carry data between layers, exposing only what is needed.

### Implemented in DevConnect

| DTO | File | Purpose |
|-----|------|---------|
| `RegisterDTO` | `DTOs/UserDto.cs` | Input for user registration |
| `LoginDTO` | `DTOs/UserDto.cs` | Input for login |
| `AuthResponseDTO` | `DTOs/UserDto.cs` | Output — JWT token + user info |
| `UpdateProfileDTO` | `DTOs/UserDto.cs` | Input for profile update |
| `CreatePostDTO` | `DTOs/UserDto.cs` | Input for creating/updating a post |
| `PostResponseDTO` | `DTOs/UserDto.cs` | Output — post data with author name |
| `CreateCommentDTO` | `DTOs/PostInteractionDTO.cs` | Input for adding/editing a comment |
| `CommentResponseDTO` | `DTOs/PostInteractionDTO.cs` | Output — comment with author name |
| `LikeResponseDTO` | `DTOs/PostInteractionDTO.cs` | Output — like count + liked by me |

### What's Missing
- DTOs are all in one or two files — should be split per feature for large projects
- No **validation attributes** on DTO properties (`[Required]`, `[EmailAddress]` etc.)

---

## 2. Service-Repository Pattern ✅ Implemented (Posts)

### What is it?

**Repository Pattern** — abstracts database access behind an interface.  
**Service Pattern** — contains business logic, sits between Controller and Repository.

```
Before (❌ tightly coupled):
Controller → DbContext directly

Now (✅ proper layering):
Controller → IPostService → IPostRepository → DbContext
```

### Why It Matters
| Without Pattern | With Pattern |
|-----------------|--------------|
| Business logic mixed with DB calls in controller | Business logic in Service, DB in Repository |
| Hard to unit test (DbContext is hard to mock) | Easy to mock `IPostService` in tests |
| Duplicate DB queries across controllers | Centralised DB access in Repository |
| Hard to swap database | Just replace repository implementation |

### Implemented Files

#### `Interfaces/IPostRepository.cs` — DB contract
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
}
```

#### `Interfaces/IPostService.cs` — Business logic contract
```csharp
public interface IPostService
{
    Task<List<PostResponseDTO>> GetAllPostsAsync();
    Task<PostResponseDTO?> GetPostByIdAsync(int id);
    Task<List<PostResponseDTO>> GetMyPostsAsync(int userId);
    Task<PostResponseDTO> CreatePostAsync(int userId, CreatePostDTO dto);
    Task<bool> UpdatePostAsync(int postId, int userId, CreatePostDTO dto);
    Task<bool> DeletePostAsync(int postId, int userId, string role);
}
```

#### `Repositories/PostRepository.cs` — EF Core data access
```csharp
public class PostRepository : IPostRepository
{
    private readonly DevConnectDbContext _context;
    public PostRepository(DevConnectDbContext context) => _context = context;

    public async Task<List<Post>> GetAllAsync() =>
        await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .ToListAsync();

    public async Task<Post> CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Posts.AnyAsync(p => p.Id == id);
}
```

#### `Services/PostService.cs` — Business logic
```csharp
public class PostService : IPostService
{
    private readonly IPostRepository _repo;
    private readonly IMapper _mapper;

    public PostService(IPostRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PostResponseDTO> CreatePostAsync(int userId, CreatePostDTO dto)
    {
        var post = _mapper.Map<Post>(dto);  // DTO → Model
        post.UserId = userId;               // assign owner from JWT
        var created = await _repo.CreateAsync(post);
        return _mapper.Map<PostResponseDTO>(created);
    }

    public async Task<bool> UpdatePostAsync(int postId, int userId, CreatePostDTO dto)
    {
        var post = await _repo.GetByIdAsync(postId);
        if (post == null || post.UserId != userId) return false; // not found or not owner
        _mapper.Map(dto, post);
        post.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(post);
        return true;
    }

    public async Task<bool> DeletePostAsync(int postId, int userId, string role)
    {
        var post = await _repo.GetByIdAsync(postId);
        if (post == null) return false;
        if (post.UserId != userId && role != "Admin") return false; // forbidden
        await _repo.DeleteAsync(post);
        return true;
    }
}
```

#### `Controllers/PostsController.cs` — thin controller, no DB logic
```csharp
public class PostsController : ControllerBase
{
    private readonly IPostService _postService; // ← interface, not concrete class

    public PostsController(IPostService postService) => _postService = postService;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _postService.GetAllPostsAsync());

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreatePostDTO dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await _postService.CreatePostAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }
}
```

#### Registration in `Program.cs`
```csharp
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
```

> **Note:** `IUserRepository` / `UserService` for Auth are next to implement — `AuthController` still uses `DevConnectDbContext` directly.

---

## 3. AutoMapper ✅ Implemented

### What is AutoMapper?
AutoMapper automatically maps properties from one object type to another — removing manual mapping code.

### Before vs After
```csharp
// Before (❌ manual mapping — repeated everywhere)
var result = new PostResponseDTO
{
    Id = post.Id,
    Title = post.Title,
    Content = post.Content,
    CreatedAt = post.CreatedAt,
    AuthorName = post.User.Name,
    LikesCount = post.Likes.Count
};

// After (✅ AutoMapper — one line)
var result = _mapper.Map<PostResponseDTO>(post);
```

### Installed Packages
```
AutoMapper 10.0.0
AutoMapper.Extensions.Microsoft.DependencyInjection 7.0.0
```

### `Mappings/MappingProfile.cs` — actual implementation
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Post → PostResponseDTO
        // AutoMapper auto-maps: Id, Title, Content, UserId, CreatedAt, UpdatedAt
        // Manual config for computed/navigation properties:
        CreateMap<Post, PostResponseDTO>()
            .ForMember(dest => dest.AuthorName,    opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.LikesCount,    opt => opt.MapFrom(src => src.Likes.Count))
            .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count));

        // CreatePostDTO → Post (ignore fields set manually)
        CreateMap<CreatePostDTO, Post>()
            .ForMember(dest => dest.UserId,    opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Comment → CommentResponseDTO
        CreateMap<Comment, CommentResponseDTO>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.Name));

        // RegisterDTO → User (PasswordHash set manually after BCrypt hashing)
        CreateMap<RegisterDTO, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,    opt => opt.Ignore());
    }
}
```

### Key Concepts

| Scenario | Code |
|----------|------|
| Map single object | `_mapper.Map<PostResponseDTO>(post)` |
| Map list | `_mapper.Map<List<PostResponseDTO>>(posts)` |
| Map DTO onto existing model (update) | `_mapper.Map(dto, post)` |
| Ignore a property | `.ForMember(dest => dest.X, opt => opt.Ignore())` |
| Custom source mapping | `.ForMember(dest => dest.X, opt => opt.MapFrom(src => src.Y.Z))` |

### Registration in `Program.cs`
```csharp
// Scans the assembly containing MappingProfile and registers all Profile classes
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

### Usage in `PostService.cs`
```csharp
// Inject IMapper
public PostService(IPostRepository repo, IMapper mapper)
{
    _repo = repo;
    _mapper = mapper;
}

// Map DTO → Model when creating
var post = _mapper.Map<Post>(dto);

// Map Model → DTO when returning response
return _mapper.Map<PostResponseDTO>(created);

// Map updated values onto existing model (preserves Id, CreatedAt, etc.)
_mapper.Map(dto, post);
```

### Usage in `AuthController.cs`
```csharp
// RegisterDTO → User (avoids manually setting Name, Email, Role, CreatedAt)
var user = _mapper.Map<User>(dto);
user.PasswordHash = BC.HashPassword(dto.Password); // set manually — excluded from map
```

---

## 4. FluentValidation ✅ Implemented

### What is FluentValidation?
FluentValidation provides a clean, fluent API to define validation rules for your DTOs — separate from the DTO class itself.

### Installed Packages
```
FluentValidation 11.0.1
FluentValidation.AspNetCore 11.0.1
FluentValidation.DependencyInjectionExtensions 11.0.1
```

> ⚠️ **Important — v11 Breaking Change:**  
> `AddFluentValidationAutoValidation()` was **removed in v11**. Auto-pipeline validation no longer exists.  
> You must inject `IValidator<T>` and call `.ValidateAsync(dto)` manually in controllers.

### `Validators/AuthValidators.cs` — actual implementation
```csharp
// Validates RegisterDTO — injected into AuthController
public class RegisterValidator : AbstractValidator<RegisterDTO>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain a number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character.");
    }
}

// Validates LoginDTO
public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
```

### `Validators/PostValidators.cs` — actual implementation
```csharp
// Validates CreatePostDTO
public class CreatePostValidator : AbstractValidator<CreatePostDTO>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.")
            .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters.");
    }
}

// Validates CreateCommentDTO
public class CreateCommentValidator : AbstractValidator<CreateCommentDTO>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment cannot be empty.")
            .MinimumLength(2).WithMessage("Comment must be at least 2 characters.")
            .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
    }
}
```

### Registration in `Program.cs`
```csharp
// Scans the assembly and registers all validators automatically
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
```

### How to Use in a Controller (v11 manual pattern)
```csharp
[HttpPost("register")]
public async Task<ActionResult<AuthResponseDTO>> Register(
    RegisterDTO dto,
    [FromServices] IValidator<RegisterDTO> validator)  // injected from DI
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
        return BadRequest(result.Errors.Select(e => e.ErrorMessage));

    // proceed with registration...
}
```

### Available Rule Methods
| Method | Purpose |
|--------|---------|
| `.NotEmpty()` | Field must not be null or whitespace |
| `.MinimumLength(n)` | Minimum string length |
| `.MaximumLength(n)` | Maximum string length |
| `.EmailAddress()` | Must be a valid email format |
| `.Matches("regex")` | Must match the given regex pattern |
| `.GreaterThan(n)` | Numeric value must be greater than n |
| `.WithMessage("...")` | Custom error message for the preceding rule |

---

## 5. SOLID Principles ⚠️ Partial

### What is SOLID?
| Letter | Principle | Meaning |
|--------|-----------|---------|
| **S** | Single Responsibility | A class should do only one thing |
| **O** | Open/Closed | Open for extension, closed for modification |
| **L** | Liskov Substitution | Subclasses should be replaceable by their parent |
| **I** | Interface Segregation | Don't force classes to implement unused interfaces |
| **D** | Dependency Inversion | Depend on abstractions, not concrete classes |

### Current Status in DevConnect

#### S — Single Responsibility ⚠️ Partial
```
AuthController does:
  ✅ Handles HTTP request/response  ← controller responsibility
  ❌ Hashes passwords               ← should be in a service
  ❌ Generates JWT tokens           ← should be in a service
  ❌ Queries the database           ← should be in a repository
```
**Fix:** Extract business logic into `AuthService`, DB access into `UserRepository`.

#### O — Open/Closed ❌ Not applied
Currently you'd need to modify controllers to change behaviour.  
**Fix:** Use interfaces + dependency injection so you can swap implementations without modifying existing code.

#### L — Liskov Substitution ✅ Implicitly followed
All controllers inherit from `ControllerBase` and don't override behaviour in a breaking way.

#### I — Interface Segregation ❌ Not applied yet
**Fix:** When creating services, create small, focused interfaces (`IUserService`, `IPostService`) rather than one large `IAppService`.

#### D — Dependency Inversion ⚠️ Partial
```csharp
// Current — depends on concrete class (❌)
private readonly DevConnectDbContext _context;

// Target — depends on abstraction (✅)
private readonly IUserRepository _userRepository;
```
**Fix:** Implement the Repository pattern (see Section 2).

---

## 6. DRY (Don't Repeat Yourself) ⚠️ Partial

### What is DRY?
Every piece of knowledge should have a single, unambiguous representation in the codebase. If you copy-paste code, that's a DRY violation.

### Current Violations in DevConnect

#### 1. `GetUserId()` repeated in 3 controllers
```csharp
// Repeated in PostsController, LikesController, CommentsController, UsersController
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```
**Fix:** Create a base controller:
```csharp
public abstract class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// Then all controllers inherit from it
public class PostsController : BaseApiController
{
    public async Task<IActionResult> CreatePost(CreatePostDTO dto)
    {
        var userId = GetCurrentUserId(); // ← clean, no repetition
    }
}
```

#### 2. Manual DTO mapping repeated everywhere
```csharp
// Same mapping pattern in PostsController, CommentsController...
new PostResponseDTO { Id = p.Id, Title = p.Title, ... }
```
**Fix:** Use AutoMapper (see Section 3).

#### 3. `NotFound()` / `FindAsync()` pattern repeated
```csharp
// Same pattern in every controller
var entity = await _context.X.FindAsync(id);
if (entity == null) return NotFound();
```
**Fix:** Move into Repository with standardised methods.

---

## Implementation Roadmap

```
Phase 1 — Quick wins ✅ Done
  ✅ AutoMapper installed and MappingProfile created
  ✅ FluentValidation installed and validators created

Phase 2 — AutoMapper ✅ Done
  ✅ AutoMapper 10.0.0 installed
  ✅ Mappings/MappingProfile.cs — Post, Comment, User mappings
  ✅ PostService uses _mapper.Map<>() throughout
  ✅ AuthController uses _mapper.Map<User>(dto) for registration

Phase 3 — FluentValidation ✅ Done
  ✅ FluentValidation 11.0.1 installed
  ✅ Validators/AuthValidators.cs — RegisterValidator, LoginValidator
  ✅ Validators/PostValidators.cs — CreatePostValidator, CreateCommentValidator
  ✅ Registered via AddValidatorsFromAssemblyContaining<RegisterValidator>()
  ⚠️  Manual injection pattern required (v11 removed auto-pipeline validation)

Phase 4 — Service-Repository Pattern ✅ Done (Posts)
  ✅ Interfaces/IPostRepository.cs
  ✅ Interfaces/IPostService.cs
  ✅ Repositories/PostRepository.cs
  ✅ Services/PostService.cs
  ✅ PostsController refactored — no DbContext, no business logic
  ❌ IUserRepository / UserService — AuthController still uses DbContext directly

Phase 5 — SOLID / DRY cleanup ❌ Pending
  ❌ BaseApiController with GetCurrentUserId() — userId logic repeated in controllers
  ❌ IUserRepository + UserRepository + IUserService + UserService
```
