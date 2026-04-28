# ASP.NET Core — Key Concepts Notes

---

## 1. REST API

### What is REST?
**REST (Representational State Transfer)** is an architectural style for designing APIs using standard HTTP methods to perform operations on resources.

### Core Principles
| Principle | Description |
|-----------|-------------|
| **Stateless** | Each request contains all info needed — server stores no session |
| **Resource-based** | Everything is a resource identified by a URL (`/api/users`, `/api/books`) |
| **HTTP Methods** | Use the right verb for the right action |
| **Uniform Interface** | Consistent URL patterns and responses |

### HTTP Methods
| Method | Action | Example |
|--------|--------|---------|
| `GET` | Read data | `GET /api/users` |
| `POST` | Create data | `POST /api/auth/register` |
| `PUT` | Update (full replace) | `PUT /api/users/profile` |
| `PATCH` | Update (partial) | `PATCH /api/users/1` |
| `DELETE` | Delete data | `DELETE /api/users/1` |

### HTTP Status Codes Used in DevConnect
| Code | Meaning | Used When |
|------|---------|-----------|
| `200 OK` | Success | `Ok(data)` |
| `201 Created` | Resource created | `CreatedAtAction(...)` |
| `204 No Content` | Success, no body | `NoContent()` after update/delete |
| `400 Bad Request` | Invalid input | `BadRequest("message")` |
| `401 Unauthorized` | Not authenticated | JWT token missing/invalid |
| `403 Forbidden` | Authenticated but not authorized | Non-admin accessing admin endpoint |
| `404 Not Found` | Resource doesn't exist | `NotFound()` |
| `500 Internal Server Error` | Server crashed | Unhandled exception |

### Implemented in DevConnect
```
GET    /api/books              → Get all books
GET    /api/books/{id}         → Get book by ID
POST   /api/books              → Add a book
POST   /api/auth/register      → Register user
POST   /api/auth/login         → Login
GET    /api/users/profile      → Get own profile
PUT    /api/users/profile      → Update own profile
GET    /api/users              → Get all users (Admin)
DELETE /api/users/{id}         → Delete user (Admin)
```

---

## 2. Controllers

### What is a Controller?
A **Controller** is a class that handles incoming HTTP requests, processes them, and returns a response. In ASP.NET Core Web API, all controllers inherit from `ControllerBase`.

### Key Attributes
| Attribute | Purpose |
|-----------|---------|
| `[ApiController]` | Enables automatic model validation, binding, and error responses |
| `[Route("api/[controller]")]` | Sets base URL — `[controller]` is replaced with class name minus "Controller" |
| `[HttpGet]`, `[HttpPost]` etc. | Maps method to HTTP verb |
| `[Authorize]` | Requires valid JWT token |
| `[Authorize(Roles = "Admin")]` | Requires specific role |

### Controller Structure in DevConnect

```csharp
[Route("api/[controller]")]    // → /api/users
[ApiController]
[Authorize]                    // All endpoints require JWT
public class UsersController : ControllerBase
{
    private readonly DevConnectDbContext _context;

    // Constructor Injection — DI provides the DbContext
    public UsersController(DevConnectDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]       // → GET /api/users/profile
    public async Task<ActionResult<User>> GetProfile() { ... }

    [HttpPut("profile")]       // → PUT /api/users/profile
    public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto) { ... }

    [HttpGet]                  // → GET /api/users
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<User>>> GetAllUsers() { ... }

    [HttpDelete("{id}")]       // → DELETE /api/users/1
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id) { ... }
}
```

### Controllers in DevConnect
| Controller | Responsibility |
|------------|----------------|
| `BooksController` | CRUD for Books (learning exercise, no auth) |
| `AuthController` | Register + Login, returns JWT token |
| `UsersController` | Profile management + Admin operations |

---

## 3. Routing

### What is Routing?
**Routing** is the mechanism that maps incoming HTTP request URLs to specific controller actions (methods).

### Types of Routing

#### Attribute Routing (used in DevConnect)
Defined directly on controllers and methods using attributes:
```csharp
[Route("api/[controller]")]        // Base route
public class AuthController : ControllerBase
{
    [HttpPost("register")]          // → POST /api/auth/register
    public IActionResult Register() { ... }

    [HttpPost("login")]             // → POST /api/auth/login
    public IActionResult Login() { ... }
}
```

#### Route Parameters
```csharp
[HttpGet("{id}")]                   // → GET /api/books/5
public async Task<ActionResult<Books>> GetBookById(int id)
{
    // id is automatically bound from the URL
}
```

#### Route Tokens
| Token | Replaced With |
|-------|--------------|
| `[controller]` | Class name minus "Controller" → `BooksController` → `books` |
| `[action]` | Method name |

### How Routes Are Registered
```csharp
// Program.cs
app.MapControllers();   // Scans all [ApiController] classes and registers routes
```

### Route Priority in DevConnect
```
/api/auth/register    → AuthController.Register()
/api/auth/login       → AuthController.Login()
/api/users/profile    → UsersController.GetProfile() or UpdateProfile()
/api/users            → UsersController.GetAllUsers()
/api/users/{id}       → UsersController.DeleteUser(id)
/api/books            → BooksController.GetBooks()
/api/books/{id}       → BooksController.GetBookById(id)
```

---

## 4. Middleware

### What is Middleware?
**Middleware** is software assembled into a pipeline to handle HTTP requests and responses. Each piece of middleware:
- Receives the request
- Does some work (auth check, logging, CORS headers etc.)
- Either passes to the next middleware or short-circuits (returns a response)

### The Pipeline — How it Works
```
HTTP Request
     │
     ▼
┌─────────────────────┐
│  UseHttpsRedirection│  → Redirects http → https
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│  UseCors            │  → Adds CORS headers to response
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│  UseAuthentication  │  → Validates JWT token, sets User identity
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│  UseAuthorization   │  → Checks [Authorize] attributes
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│  MapControllers     │  → Routes to correct controller action
└──────────┬──────────┘
           │
     HTTP Response
```

### Middleware in DevConnect (`Program.cs`)
```csharp
app.UseHttpsRedirection();             // 1. Force HTTPS
app.UseCors("AllowFrontend");          // 2. CORS headers (before auth!)
app.UseAuthentication();               // 3. Read & validate JWT token
app.UseAuthorization();                // 4. Enforce [Authorize] rules
app.MapControllers();                  // 5. Route to controller
```

### ⚠️ Order Matters!
- `UseCors()` **must** come before `UseAuthentication()` — otherwise CORS headers aren't added to 401 responses
- `UseAuthentication()` **must** come before `UseAuthorization()` — you need to know WHO the user is before checking WHAT they can do

### Middleware Registered as Services (before `app.Build()`)
```csharp
builder.Services.AddCors(...)           // Registers CORS
builder.Services.AddControllers()       // Registers MVC Controllers
builder.Services.AddAuthentication(...) // Registers JWT Auth
builder.Services.AddSwaggerGen()        // Registers Swagger
builder.Services.AddDbContext<...>()    // Registers EF Core
```

---

## 5. Dependency Injection (DI)

### What is DI?
**Dependency Injection** is a design pattern where objects receive their dependencies from an external source rather than creating them themselves.

> Instead of `new DbContext()` inside your controller, ASP.NET Core **injects** it automatically.

### Without DI (Bad ❌)
```csharp
public class UsersController : ControllerBase
{
    public IActionResult GetUsers()
    {
        var context = new DevConnectDbContext();   // tightly coupled
        // ...
    }
}
```

### With DI (Good ✅)
```csharp
public class UsersController : ControllerBase
{
    private readonly DevConnectDbContext _context;

    // ASP.NET Core provides the context automatically
    public UsersController(DevConnectDbContext context)
    {
        _context = context;
    }
}
```

### Service Lifetimes
| Lifetime | Method | Created | Used For |
|----------|--------|---------|----------|
| **Transient** | `AddTransient<T>()` | Every time requested | Lightweight, stateless services |
| **Scoped** | `AddScoped<T>()` | Once per HTTP request | DbContext (default for EF Core) |
| **Singleton** | `AddSingleton<T>()` | Once for app lifetime | Config, caching |

### Services Registered in DevConnect
```csharp
// Program.cs
builder.Services.AddControllers();           // Transient — MVC controllers
builder.Services.AddDbContext<FirstAPIContext>(...)       // Scoped — per request
builder.Services.AddDbContext<DevConnectDbContext>(...)   // Scoped — per request
builder.Services.AddAuthentication(...)      // Framework managed
builder.Services.AddCors(...)               // Framework managed
```

### How DI Flows in DevConnect
```
HTTP Request → AuthController needs DevConnectDbContext + IConfiguration
                      │
             DI Container checks registrations
                      │
             Creates DevConnectDbContext (Scoped — once per request)
             Provides IConfiguration (Singleton — same instance always)
                      │
             Injects into AuthController constructor
                      │
             Controller uses them, request ends
                      │
             DevConnectDbContext is disposed (Scoped lifetime ends)
```

---

## 6. `appsettings.json`

### What is it?
`appsettings.json` is the **central configuration file** for ASP.NET Core applications. It stores settings like connection strings, JWT config, logging levels, allowed hosts etc.

### Structure in DevConnect

**`appsettings.json`** — Committed to Git (no secrets):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": ""       ← empty, filled locally
  },
  "JwtSettings": {
    "Key": "",                    ← empty, filled locally
    "Issuer": "DevConnect",
    "Audience": "DevConnectUsers",
    "ExpiryInDays": 7
  }
}
```

**`appsettings.Development.json`** — Gitignored (has real secrets):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=...;Initial Catalog=DevConnect;..."
  },
  "JwtSettings": {
    "Key": "DevConnect-this-is-a-secret-key-for-jwt-token-generation"
  }
}
```

### Reading Configuration in Code

#### Using `IConfiguration` (used in AuthController)
```csharp
public AuthController(DevConnectDbContext context, IConfiguration config)
{
    _config = config;
}

// Reading values
var key = _config["JwtSettings:Key"];
var issuer = _config["JwtSettings:Issuer"];
var connStr = _config.GetConnectionString("DefaultConnection");
```

#### In `Program.cs`
```csharp
builder.Services.AddDbContext<DevConnectDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Configuration Priority (lowest → highest)
```
appsettings.json
    ↓ overridden by
appsettings.Development.json   (when ASPNETCORE_ENVIRONMENT=Development)
    ↓ overridden by
User Secrets                   (local dev only)
    ↓ overridden by
Environment Variables          (production/deployment)
```

### ⚠️ What NOT to Commit to Git
- Real connection strings
- JWT secret keys
- API keys, passwords
- Any sensitive credentials

These belong in `appsettings.Development.json` (gitignored) or User Secrets.

---

## Not Yet Implemented in DevConnect

| Concept | Description | How to Add |
|---------|-------------|------------|
| **Logging** | `ILogger<T>` for structured logging | Inject `ILogger<UsersController>` in controllers |
| **Validation** | `[Required]`, `[EmailAddress]` on DTOs | Add Data Annotations to DTO properties |
| **Global Error Handling** | Catch all unhandled exceptions | Add `UseExceptionHandler()` middleware |
| **Pagination** | Return paged results | Add `page` & `size` query params to `GetAllUsers` |
| **Filtering & Sorting** | Query params on list endpoints | Add `?role=Admin&sort=name` support |
| **Unit Testing** | Test controllers in isolation | Add xUnit + Moq project |
| **AutoMapper** | Auto map Model ↔ DTO | Install `AutoMapper` NuGet package |
| **FluentValidation** | Advanced DTO validation | Install `FluentValidation.AspNetCore` |
| **Refresh Tokens** | Keep users logged in | Add `RefreshToken` model & endpoint |
