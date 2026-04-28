# DevConnect - ASP.NET Core Web API

A learning project built with ASP.NET Core Web API, Entity Framework Core, and JWT Authentication.

---

## Concepts Covered

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
