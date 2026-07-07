# DevConnect — Demo Cheat Sheet

> Keep this open during the demo. One-line reminders only.

---

## Demo Flow (order)
Architecture overview → live walkthrough (register → login → post → like/comment → paginate) → Swagger → logs → `dotnet test` → "what's next".

## Startup Checklist
- [ ] API running + Swagger open (`/swagger`)
- [ ] Frontend `devconnectwebapp` running
- [ ] Terminal ready for `dotnet test`
- [ ] Logs visible (`Logs/`)
- [ ] This sheet + `Docs/DemoQnA.md` open

---

## One-Line Answers

| Topic | One-liner |
|-------|-----------|
| Value vs reference | Value copies data; reference copies a pointer to heap object. |
| `record` vs `class` | `record` = immutable + value equality (DTOs); `class` = identity/state (models). |
| async/await | Frees thread during I/O so server handles other requests. |
| IEnumerable vs IQueryable | In-memory vs translated to SQL by EF. |
| Deferred execution | LINQ runs only when enumerated (`ToListAsync`). |
| Interface vs abstract | Contract (many) vs shared base (one). |
| Middleware order | CORS → Auth → Authorization → Endpoints. |
| DI lifetimes | Scoped = per request (repos/DbContext); Singleton = app-wide; Transient = every use. |
| Why interfaces | Swappable + mockable for tests. |
| ORM / EF Core | Maps C# ↔ tables, LINQ → SQL, migrations. |
| Migration | Versioned schema change: `Add-Migration` → `Update-Database`. |
| N+1 problem | 1 + N queries; fix with `Include`/projection. |
| AsNoTracking | Skip change tracking on read-only = faster. |
| Service–Repository | Repo = data access; Service = business logic; thin controllers. |
| Why DTOs | Decouple API from model; hide `PasswordHash`. |
| SOLID | SRP layers, DIP via interfaces + DI. |
| AutoMapper | Removes manual model↔DTO mapping. |
| FluentValidation | Validation rules in dedicated classes. |
| JWT | Signed token w/ claims; `Authorization: Bearer`; validated each request. |
| BCrypt | Salted, slow hash — breach-safe passwords. |
| AuthN vs AuthZ | Who you are vs what you can do. |
| Role auth | Role claim in JWT + `[Authorize(Roles="Admin")]` + ownership check. |
| OAuth/OIDC | Login via Google/GitHub; issue own JWT on callback. |
| CORS | Allows specific frontend origins to call API. |
| Caching | Output cache on GETs; not for user-specific/changing data. |
| Serilog | Structured logs (properties) → queryable. |
| Unit vs integration | Isolated + mocked vs real components together. |

---

## Key Files (point to these live)
| Concept | File |
|---------|------|
| DI + pipeline | `Program.cs` |
| Controllers | `Controllers/PostsController.cs`, `AuthController.cs` |
| Service logic | `Services/PostService.cs`, `AuthService.cs` |
| Data access | `Repositories/PostRepository.cs` |
| DB context | `Data/DevConnectDbContext.cs` |
| Models | `Models/Post.cs`, `User.cs`, `Like.cs`, `Comment.cs` |
| DTOs | `DTOs/PostInteractionDTO.cs`, `UserDto.cs` |
| Mapping | `Mappings/MappingProfile.cs` |
| Validation | `Validators/AuthValidators.cs`, `PostValidators.cs` |
| Tests | `DevConnect.Tests/` |

## REST Endpoints
```
GET/POST/PUT/DELETE /api/posts        posts (paged + sorted)
GET                 /api/posts/my     my posts
POST                /api/auth/register register
POST                /api/auth/login    login → JWT
GET                 /api/auth/login-google | login-github  OAuth
```

## "What's Next" (closing line)
Refresh tokens · global exception middleware · rate limiting · more tests · secrets in a vault.

---
**Stay calm. Explain the *why*. If you don't know, say how you'd find out.**
