---
marp: true
theme: default
paginate: true
size: 16:9
header: "DevConnect — C# / .NET Demo"
footer: "July 14, 2026"
---

<!-- _class: lead -->
<!-- _paginate: false -->

# DevConnect
## A Full-Stack Social API

**3 Months of C# & .NET — Learning Showcase**

Presented by: *[Your Name]*
July 14, 2026

---

# Agenda

1. What is DevConnect?
2. Tech Stack
3. Architecture Overview
4. Live Demo
5. Deep Dive — Key Concepts
6. Security
7. Quality: Testing, Logging, Caching
8. What I Learned & What's Next
9. Q&A

---

# What is DevConnect?

A **social networking REST API** where users can:

- Register / log in (JWT + Google/GitHub OAuth)
- Create, read, update, delete **posts**
- **Like** and **comment** on posts
- Browse posts with **pagination & sorting**

> Built to consolidate everything I learned in C# and .NET.

---

# Tech Stack

| Layer | Technology |
|-------|------------|
| Language | C# 12 |
| Backend | ASP.NET Core Web API (.NET 8) |
| ORM / Data | Entity Framework Core 9 (Code-First) |
| Database | SQL Server |
| Auth | JWT Bearer + OAuth2 / OIDC (Google, GitHub) |
| Password | BCrypt hashing |
| Mapping / Validation | AutoMapper + FluentValidation |
| Performance | Output Caching (tag-based invalidation) |
| API Docs | Swagger / OpenAPI |
| Logging | Serilog (console + rolling file) |
| Testing | NUnit + Moq + EF InMemory / Testcontainers |
| Frontend | Next.js (React + TypeScript) |

---

# Backend NuGet Packages

| Purpose | Package | Version |
|---------|---------|---------|
| ORM | `Microsoft.EntityFrameworkCore` | 9.0.6 |
| SQL provider | `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.4 |
| Migrations | `Microsoft.EntityFrameworkCore.Tools` | 9.0.4 |
| JWT auth | `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.26 |
| Google login | `Microsoft.AspNetCore.Authentication.Google` | 7.0.0 |
| GitHub login | `AspNet.Security.OAuth.GitHub` | 7.0.0 |
| Password hashing | `BCrypt.Net-Next` | 3.1.0 |
| Object mapping | `AutoMapper` (+ DI extension) | 12.0.1 |
| Validation | `FluentValidation` (+ AspNetCore / DI) | 11.0.1 |
| Logging | `Serilog.AspNetCore` (+ Console / File sinks) | 10.0.0 |
| API docs | `Swashbuckle.AspNetCore` (Swagger) | 6.6.2 |

> Managed via NuGet in `DevConnect.csproj`.

---

# Architecture Overview

```
┌────────────┐   ┌────────────┐   ┌────────────┐   ┌──────────┐   ┌────────────┐
│ Controller │──▶│  Service   │──▶│ Repository │──▶│ DbContext│──▶│ SQL Server │
│   (HTTP)   │   │ (business) │   │ (EF query) │   │ (EF Core)│   │            │
└────────────┘   └────────────┘   └────────────┘   └──────────┘   └────────────┘
```

- **Controllers** — handle HTTP, return status codes
- **Services** — business logic, mapping, ownership rules
- **Repositories** — EF Core queries only
- **DTOs** — decouple API from DB models
- **DI** — everything wired in `Program.cs`

> Note: full chain is used by **Posts**; Auth/Users/Comments/Likes use `DbContext` directly.

---

# Why This Architecture?

- **Separation of concerns** — each layer one job
- **Testable** — mock the repository, test the service
- **Maintainable** — change DB logic without touching controllers
- **SOLID** — depends on interfaces, not concrete classes

> "Thin controllers, smart services, dumb repositories."

---

# Project Folder Structure

```
DevConnect.sln
├─ DevConnect/                 ← Web API project
│  ├─ Program.cs               ← startup, DI, middleware pipeline
│  ├─ appsettings.json         ← config (JWT, DB, Serilog, OAuth)
│  ├─ Controllers/             ← HTTP endpoints (Posts, Auth, ...)
│  ├─ Services/                ← business logic (PostService, AuthService)
│  ├─ Repositories/            ← EF Core data access (PostRepository)
│  ├─ Interfaces/              ← contracts for DI + testing
│  ├─ Data/                    ← DbContext(s)
│  ├─ Models/                  ← entities (User, Post, Like, Comment)
│  ├─ DTOs/                    ← request/response objects
│  ├─ Mappings/                ← AutoMapper profiles
│  ├─ Validators/              ← FluentValidation rules
│  ├─ Migrations/              ← EF Core schema history
│  └─ Logs/                    ← Serilog rolling log files
├─ DevConnect.Tests/           ← NUnit + Moq tests (unit + integration)
└─ devconnectwebapp/           ← Next.js frontend (React + TS)
```

> Folders mirror the layers: **Controller ▸ Service ▸ Repository ▸ Data**.

---

# Folder Structure — Why It Matters

| Folder | Responsibility | Key principle |
|--------|----------------|---------------|
| `Controllers/` | Parse HTTP, return status codes | Thin, no logic |
| `Services/` | Business rules, orchestration | Single responsibility |
| `Repositories/` | EF Core queries only | Data-access isolation |
| `Interfaces/` | Abstractions for DI | Dependency inversion |
| `DTOs/` | Shape API input/output | Hide internal fields |
| `Mappings/` + `Validators/` | Cross-cutting concerns | Keep models clean |

> Predictable structure = easy to navigate, test, and onboard.

---

# System Design

<style scoped>
pre { font-size: 16px; line-height: 1.15; }
</style>

```
        ┌────────────────┐
        │  User / Browser  │
        └────────┬───────┘
                 │  HTTPS
                 ▼
   ┌─────────────────────┐
   │   Next.js Frontend   │
   └────────┬──────────┘
            │  JWT
            ▼
   ┌─────────────────────┐    ┌────────────────┐
   │  ASP.NET Core Web API  ├──▶│ Google / GitHub  │
   │  (middleware pipeline) │    └────────────────┘
   └────────┬──────────┘
            │
            ▼
         EF Core  ▶  SQL Server
```

- Pipeline: **Serilog ▸ CORS ▸ Cache ▸ AuthN ▸ AuthZ**
- **Stateless** JWT auth — scales horizontally
- **Output cache** shields DB from read-heavy traffic

---

# Post API — Request Flow

**Example: `POST /api/posts` (create a post)**

<style scoped>
pre { font-size: 16px; line-height: 1.2; }
</style>

```
Client
  │  POST /api/posts  (+ Bearer token)
  ▼
Middleware       ─ validate JWT (signature, expiry)
  │  userId from claims
  ▼
PostsController  ─ CreatePostAsync(userId, dto)
  ▼
PostService      ─ AutoMapper: dto → Post
  ▼
PostRepository   ─ INSERT + SaveChangesAsync
  ▼
SQL Server       ─ returns saved post
  │
  ▲  response travels back up the chain
  │
Controller        ─ EvictByTag("posts")
  ▼
Client  ◀──  201 Created + PostResponseDTO
```

---

# Post API — Read Path (cached + paged)

**`GET /api/posts?pageNumber=1&pageSize=10&sortBy=likes`**

1. **Output cache** checked first — hit → return instantly
2. Miss → `PostsController.GetAll` → `PostService.GetPagedPostsAsync`
3. Service **clamps** page bounds (`PageSize` 1–100)
4. Repository builds `IQueryable`: `Include` → **sort** → `Skip`/`Take`
5. Returns `PagedResult<T>` with `totalCount`, `totalPages`
6. Response cached for 30s under tag `posts`

> Writes (create/update/delete) **evict** the `posts` tag → no stale reads.

---

<!-- _class: lead -->

# Live Demo

Register → Login → Create Post → Like & Comment
→ Pagination → Swagger → Logs

---

# Deep Dive: Dependency Injection

```csharp
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

- **Scoped** = one instance per HTTP request
- Constructors receive dependencies automatically
- Enables mocking in unit tests

---

# Deep Dive: DTOs & AutoMapper

**Problem:** never expose `PasswordHash` or internal fields.

```csharp
CreateMap<Post, PostResponseDTO>()
    .ForMember(d => d.AuthorName,
               o => o.MapFrom(s => s.User.Username));
```

- **DTOs** shape exactly what the client needs
- **AutoMapper** removes repetitive mapping code

---

# Deep Dive: EF Core

- Models → tables via **navigation properties**
- LINQ → **SQL** (translated, deferred execution)
- **Migrations** version the schema
- **Pagination** with `Skip` / `Take`

```csharp
var posts = await q
    .Skip((page - 1) * size)
    .Take(size)
    .ToListAsync();
```

---

# Security: Authentication

**JWT Flow:**

```
1. Login → verify password (BCrypt)
2. Issue signed JWT with claims (id, email, role)
3. Client sends: Authorization: Bearer <token>
4. Middleware validates signature + expiry
```

- Passwords **hashed** with BCrypt (never plaintext)
- Also supports **Google & GitHub** OAuth login

---

# Security: Authorization

- **Role-based:** `[Authorize(Roles = "Admin")]`
- **Ownership checks** in the service layer

```csharp
if (post.UserId != userId && role != "Admin")
    return false; // can't edit others' posts
```

- **CORS** allows only trusted frontend origins

---

# Quality: Cross-Cutting Concerns

| Feature | Purpose |
|---------|---------|
| **Serilog** | Structured, queryable logs |
| **Output Caching** | Faster read-heavy endpoints |
| **Pagination & Sorting** | Scalable list responses |
| **FluentValidation** | Clean, reusable input rules |
| **Swagger** | Interactive API docs |

---

# Quality: Testing

- **Unit tests** — service logic with **Moq** (isolated)
- **Integration tests** — repository against a test DB
- Framework: **NUnit** (Arrange–Act–Assert)

```bash
dotnet test   # all green ✅
```

> Focused on the service layer — where business rules live.

---

# What I Learned

- Building a clean, layered .NET Web API end-to-end
- Real-world **auth** (JWT + OAuth/OIDC)
- **EF Core** modeling, migrations, query performance
- Writing **testable** code with DI and interfaces
- Connecting a **React/Next.js** frontend to the API

---

# What's Next

- Refresh tokens for longer sessions
- Global exception-handling middleware
- Rate limiting
- Higher test coverage
- Secrets in a vault (Azure Key Vault)
- Caching & indexing for scale

---

<!-- _class: lead -->

# Thank You!

## Questions?

*DevConnect — C# / .NET Learning Showcase*
