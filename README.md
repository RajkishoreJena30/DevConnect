# DevConnect - ASP.NET Core Web API

A learning project built with ASP.NET Core Web API, Entity Framework Core, and JWT Authentication.

---

## Concepts Covered

📄 See detailed notes: [Docs/Notes.md](Docs/Notes.md)  
📄 CORS detailed notes: [Docs/CORS.md](Docs/CORS.md)  
📄 Database concepts: [Docs/Database.md](Docs/Database.md)  
📄 Architecture & Design Patterns: [Docs/Architecture.md](Docs/Architecture.md)

### 1. Project Setup
- ASP.NET Core Web API project creation
- `Program.cs` — service registration & middleware pipeline
- `appsettings.json` vs `appsettings.Development.json`
- Swagger / OpenAPI for API documentation & testing

---

### 2. Entity Framework Core (EF Core)
- Creating **Models** (`Books`, `User`)
- Creating **DbContext** (`FirstAPIContext`, `DevConnectDbContext`)
- **Multiple DbContexts** in the same project
- **Migrations** — `Add-Migration`, `Update-Database`
- **Seed Data** — pre-populating the Books table
- `DbSet<T>` for table access

---

### 3. REST API Controllers
- `[ApiController]`, `[Route]` attributes
- HTTP verbs — `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Route parameters — `{id}`
- Return types — `ActionResult`, `IActionResult`
- Response helpers — `Ok()`, `NotFound()`, `BadRequest()`, `NoContent()`, `CreatedAtAction()`

---

### 4. Authentication & Security
- **JWT (JSON Web Token)** — generation & validation
- **BCrypt** — password hashing & verification
- `[Authorize]` — protecting endpoints
- `[Authorize(Roles = "Admin")]` — role-based access control
- Reading user identity from JWT claims (`ClaimTypes.NameIdentifier`)

---

### 5. DTOs (Data Transfer Objects)
- `RegisterDTO`, `LoginDTO`, `AuthResponseDTO`, `UpdateProfileDTO`
- Separating **input/output shapes** from DB models
- Hiding sensitive fields (`PasswordHash`) from API responses
- **Model vs DTO** — Model is about storage, DTO is about communication

---

### 6. Configuration & Secrets Management
- `JwtSettings` in config (`Key`, `Issuer`, `Audience`, `ExpiryInDays`)
- Keeping secrets out of Git using `appsettings.Development.json`
- `.gitignore` — excluding sensitive files from source control

---

### 8. CORS (Cross-Origin Resource Sharing)
- **What is CORS** — browser security feature that blocks requests from a different origin (domain, port, or protocol)
- Configured in `Program.cs` using `AddCors()` and `UseCors()`
- **Named Policy** — `"AllowFrontend"` applied globally
- `WithOrigins()` — whitelist specific frontend URLs
- `AllowAnyHeader()` — allow all request headers
- `AllowAnyMethod()` — allow GET, POST, PUT, DELETE etc.
- `AllowCredentials()` — allow cookies and auth headers
- **Middleware order** — `UseCors()` must come before `UseAuthentication()` and `UseAuthorization()`
- CORS only applies to **browsers** — Postman, Swagger, curl are not affected

📄 See detailed notes: [Docs/CORS.md](Docs/CORS.md)

---

### 7. Git & Source Control
- `git init`, `git add`, `git commit`
- `.gitignore` — generated via `dotnet new gitignore`
- `safe.directory` config fix for ownership issues
- `git rm --cached` — untracking already-staged files

---

## Project Structure

```
DevConnect/
├── Controllers/
│   ├── BooksController.cs       → CRUD for Books (learning exercise)
│   ├── AuthController.cs        → Register + Login endpoints
│   └── UsersController.cs       → Profile + Admin endpoints
├── Models/
│   ├── Books.cs
│   └── User.cs
├── Data/
│   ├── FirstAPIContext.cs        → DbContext for Books (learning)
│   └── DevConnectDbContext.cs    → DbContext for Users (production-ready)
├── DTOs/
│   └── UserDto.cs               → RegisterDTO, LoginDTO, AuthResponseDTO, UpdateProfileDTO
├── Migrations/
│   ├── (Books migrations)
│   └── DevConnectDb/
│       └── (User migrations)
├── appsettings.json             → Committed (no secrets)
├── appsettings.Development.json → Local only, gitignored (secrets here)
└── Program.cs                   → App entry point
```

---

## API Endpoints

### Auth (Public)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Login and receive JWT token |

### Users (Requires JWT Token)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users/profile` | Get own profile |
| `PUT` | `/api/users/profile` | Update own profile |
| `GET` | `/api/users` | Get all users *(Admin only)* |
| `DELETE` | `/api/users/{id}` | Delete a user *(Admin only)* |

### Books (Public)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/books` | Get all books |
| `GET` | `/api/books/{id}` | Get book by ID |
| `POST` | `/api/books` | Add a new book |

### Posts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts` | Public | Get all posts |
| `GET` | `/api/posts/{id}` | Public | Get post by ID |
| `GET` | `/api/posts/my` | 🔒 JWT | Get own posts |
| `POST` | `/api/posts` | 🔒 JWT | Create a post |
| `PUT` | `/api/posts/{id}` | 🔒 JWT | Update own post |
| `DELETE` | `/api/posts/{id}` | 🔒 JWT/Admin | Delete post |

### Likes
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts/{postId}/likes` | Public | Get like count + liked by me |
| `POST` | `/api/posts/{postId}/likes` | 🔒 JWT | Toggle like / unlike |

### Comments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/posts/{postId}/comments` | Public | Get all comments |
| `POST` | `/api/posts/{postId}/comments` | 🔒 JWT | Add a comment |
| `PUT` | `/api/posts/{postId}/comments/{id}` | 🔒 JWT | Edit own comment |
| `DELETE` | `/api/posts/{postId}/comments/{id}` | 🔒 JWT/Admin | Delete comment |

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or SQL Express)

### Setup

1. Clone the repository:
   ```bash
   git clone <repo-url>
   ```

2. Create `appsettings.Development.json` with your local settings (this file is gitignored):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "your-local-connection-string"
     },
     "JwtSettings": {
       "Key": "your-secret-key-minimum-32-characters-long",
       "Issuer": "DevConnect",
       "Audience": "DevConnectUsers",
       "ExpiryInDays": 7
     }
   }
   ```

3. Run migrations:
   ```bash
   dotnet ef database update --context DevConnectDbContext
   ```

4. Run the project:
   ```bash
   dotnet run
   ```

5. Open Swagger UI: `https://localhost:7238/swagger`

---

## Using JWT in Swagger

1. Call `POST /api/auth/register` to create a user
2. Call `POST /api/auth/login` to get a token
3. Click **Authorize** in Swagger and enter: `Bearer <your-token>`
4. All `[Authorize]` endpoints are now accessible


## Building a Developer Platform with ASP.NET Core and Next.js
- JWT Authentication in ASP.NET Core: What I Implemented and Why
- How I Designed Posts, Likes, and Comments APIs for DevConnect
- Lessons from Connecting a Next.js Frontend to a .NET Backend
- Common CORS Problems in Local Development and How I Fixed Them
- Using DTOs and AutoMapper to Keep ASP.NET Core APIs Clean
- What I Learned While Building a Multi-Page Developer Community App
- Why Public Preview and Protected Content Works Well for Developer Platforms
- My Approach to Structuring Controllers, Services, and Repositories in .NET
- Problems I Faced While Fetching Local APIs in Next.js
- If you want practical posts, these are the best:

- How I Built JWT Login and Registration in ASP.NET Core
- Implementing Likes and Comments for a Developer Community App
- Fixing Localhost Fetch Issues Between Next.js and ASP.NET Core
- Designing a Better Developer Dashboard UI
- From Single Page to Multi-Page App: Refactoring My Next.js Frontend