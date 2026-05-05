# Architecture & Design Patterns in DevConnect

---

## Status Summary

| Concept | Implemented? | Where |
|---------|-------------|-------|
| **DTOs** | ✅ Yes | `DTOs/UserDto.cs`, `DTOs/PostInteractionDTO.cs` |
| **Service-Repository Pattern** | ❌ Not yet | Controllers talk directly to DbContext |
| **AutoMapper** | ❌ Not yet | Manual mapping in every controller |
| **FluentValidation** | ❌ Not yet | No input validation on DTOs |
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

## 2. Service-Repository Pattern ❌ Not Yet Implemented

### What is it?

**Repository Pattern** — abstracts database access behind an interface.  
**Service Pattern** — contains business logic, sits between Controller and Repository.

```
Current (❌ tightly coupled):
Controller → DbContext directly

Target (✅ proper layering):
Controller → IService → IRepository → DbContext
```

### Why It Matters
| Without Pattern | With Pattern |
|-----------------|--------------|
| Business logic mixed with DB calls in controller | Business logic in Service, DB in Repository |
| Hard to unit test (DbContext is hard to mock) | Easy to mock `IUserService` in tests |
| Duplicate DB queries across controllers | Centralised DB access |
| Hard to swap database | Just replace repository implementation |

### How to Implement — Step by Step

#### Step 1 — Create `Interfaces` folder and define interfaces

**`Interfaces/IUserRepository.cs`**
```csharp
namespace DevConnect.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}
```

**`Interfaces/IUserService.cs`**
```csharp
namespace DevConnect.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<User?> GetProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateProfileDTO dto);
    }
}
```

#### Step 2 — Create `Repositories` folder and implement

**`Repositories/UserRepository.cs`**
```csharp
public class UserRepository : IUserRepository
{
    private readonly DevConnectDbContext _context;
    public UserRepository(DevConnectDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id) 
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<List<User>> GetAllAsync()
        => await _context.Users.ToListAsync();

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
```

#### Step 3 — Create `Services` folder and implement

**`Services/UserService.cs`**
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;

    public UserService(IUserRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
    {
        if (await _repo.GetByEmailAsync(dto.Email) != null)
            throw new Exception("Email already exists.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _repo.AddAsync(user);
        return new AuthResponseDTO { Token = GenerateToken(user), Name = user.Name, ... };
    }
}
```

#### Step 4 — Register in `Program.cs`
```csharp
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

#### Step 5 — Update Controller to use Service (not DbContext directly)
```csharp
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO dto)
    {
        var result = await _userService.RegisterAsync(dto);
        return Ok(result);
    }
}
```

---

## 3. AutoMapper ❌ Not Yet Implemented

### What is AutoMapper?
AutoMapper automatically maps properties from one object type to another — removing manual mapping code.

### Current Problem (Manual Mapping — repeated everywhere)
```csharp
// This pattern is repeated in PostsController, CommentsController, etc.
var result = new PostResponseDTO
{
    Id = post.Id,
    Title = post.Title,
    Content = post.Content,
    CreatedAt = post.CreatedAt,
    AuthorName = post.User.Name
};
```

### How to Implement

#### Step 1 — Install NuGet Package
```powershell
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

#### Step 2 — Create `Profiles/MappingProfile.cs`
```csharp
using AutoMapper;

namespace DevConnect.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<User, AuthResponseDTO>();
            CreateMap<UpdateProfileDTO, User>();

            // Post mappings
            CreateMap<Post, PostResponseDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.Name));

            CreateMap<CreatePostDTO, Post>();

            // Comment mappings
            CreateMap<Comment, CommentResponseDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
```

#### Step 3 — Register in `Program.cs`
```csharp
builder.Services.AddAutoMapper(typeof(Program));
```

#### Step 4 — Use in Controllers
```csharp
public class PostsController : ControllerBase
{
    private readonly IMapper _mapper;

    public PostsController(DevConnectDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPosts()
    {
        var posts = await _context.Posts.Include(p => p.User).ToListAsync();
        return Ok(_mapper.Map<List<PostResponseDTO>>(posts)); // ← one line instead of Select()
    }
}
```

---

## 4. FluentValidation ❌ Not Yet Implemented

### What is FluentValidation?
FluentValidation provides a clean, fluent API to define validation rules for your DTOs — separate from the DTO class itself.

### Current Problem (No Validation)
```csharp
// RegisterDTO has no validation — empty name, invalid email, weak password all accepted
public class RegisterDTO
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### How to Implement

#### Step 1 — Install NuGet Package
```powershell
dotnet add package FluentValidation.AspNetCore
```

#### Step 2 — Create `Validators` folder

**`Validators/RegisterDTOValidator.cs`**
```csharp
using FluentValidation;

namespace DevConnect.Validators
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
        }
    }
}
```

**`Validators/CreatePostDTOValidator.cs`**
```csharp
public class CreatePostDTOValidator : AbstractValidator<CreatePostDTO>
{
    public CreatePostDTOValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.");
    }
}
```

#### Step 3 — Register in `Program.cs`
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

#### How it Works
Once registered, validation runs **automatically** before the controller action is called. If validation fails, ASP.NET Core returns `400 Bad Request` with error details — no manual validation code needed in controllers.

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
Phase 1 — Quick wins (low effort, high value)
  → Create BaseApiController with GetCurrentUserId()
  → Add [Required]/[EmailAddress] attributes to DTOs (or FluentValidation)

Phase 2 — AutoMapper
  → Install AutoMapper
  → Create MappingProfile
  → Replace manual mappings in controllers

Phase 3 — FluentValidation
  → Install FluentValidation
  → Create validators for RegisterDTO, CreatePostDTO, CreateCommentDTO

Phase 4 — Service-Repository Pattern
  → Create Interfaces/
  → Create Repositories/
  → Create Services/
  → Register in Program.cs
  → Refactor controllers to use services

Phase 5 — SOLID cleanup
  → All principles naturally satisfied after Phases 1-4
```
