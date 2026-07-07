# DevConnect — Demo Prep & Revision Plan

> **Goal:** Be demo-ready for the manager + senior developer review on **July 14, 2026**.
> **Prep window:** July 7 → July 14 (7 days).
> This plan maps every concept in the DevConnect project to interview/demo-ready C# & .NET topics.

---

## Guiding Principle

For every topic:

1. **Open the actual file** in the project.
2. **Explain out loud** — *what / why / how*.
3. **Anticipate one** "why did you do it this way?" question.

> The manager and senior dev will probe **reasoning**, not syntax.

---

## Day 1 — Mon July 7 · C# Language Foundations

Warm up on the language before the framework.

- **OOP:** classes, interfaces, inheritance — point to `Interfaces/` + `Services/`, `Repositories/`.
- **`async`/`await`, `Task<T>`** — your repo methods (`ToListAsync`, `SaveChangesAsync`).
- **LINQ** — `Where`, `Select`, `Skip`, `Take`, `FirstOrDefaultAsync` in `Repositories/PostRepository.cs`.
- **Generics** — `PagedResult<T>`, `IValidator<T>`.
- **Nullable reference types**, `record` vs `class`, DTOs vs models.
- **Practice:** use `coding/practice.cs` to rewrite one LINQ query 3 ways.

### C# Language Topics — Full Checklist

Use this as the complete C# revision list. Tie each item back to a file in the project where possible.

**Fundamentals**
- Value types vs reference types; stack vs heap.
- `var`, implicit typing, constants (`const` vs `readonly`).
- Boxing / unboxing.
- Nullable value types (`int?`) and nullable reference types (`string?`).
- String handling: interpolation `$"..."`, `StringBuilder`, `string` immutability.

**OOP**
- Classes, objects, fields, properties (auto-properties, `init`).
- Encapsulation, inheritance, polymorphism, abstraction.
- Interfaces vs abstract classes — when to use which.
- `virtual` / `override` / `sealed` / `new` modifiers.
- Access modifiers: `public`, `private`, `protected`, `internal`.
- Static classes and members.

**Modern C# Types**
- `record` vs `class` vs `struct` (value equality).
- Enums.
- Tuples and deconstruction.
- Pattern matching (`switch` expressions, `is`, property patterns).

**Generics & Collections**
- Generic classes/methods (`PagedResult<T>`, `IValidator<T>`).
- Constraints (`where T : class`).
- `List<T>`, `Dictionary<TKey,TValue>`, `IEnumerable<T>`, `IQueryable<T>`.

**LINQ**
- Query vs method syntax.
- Common operators: `Where`, `Select`, `OrderBy`, `Skip`, `Take`, `First/FirstOrDefault`, `Any`, `Count`.
- Deferred vs immediate execution.
- `IEnumerable` vs `IQueryable` (in-memory vs SQL translation).

**Async & Concurrency**
- `async` / `await`, `Task` and `Task<T>`.
- Why async matters for I/O (DB, HTTP).
- `ConfigureAwait`, common deadlock pitfalls.

**Error Handling**
- `try` / `catch` / `finally`, `throw` vs `throw ex`.
- Custom exceptions, exception filters (`when`).
- `using` statement / `IDisposable` for resource cleanup.

**Delegates & Functional**
- Delegates, `Func<>`, `Action<>`, `Predicate<>`.
- Lambda expressions.
- Events (basic awareness).

**Language Extras**
- Extension methods.
- Object/collection initializers.
- Null-conditional `?.` and null-coalescing `??` / `??=`.
- Expression-bodied members `=>`.

## Day 2 — Tue July 8 · ASP.NET Core Fundamentals

- **Request pipeline & middleware order** (`Program.cs`) — CORS → Auth → Authorization. Know *why* order matters.
- **Dependency Injection:** `Scoped` vs `Singleton` vs `Transient` — why repos/services are `Scoped`.
- **REST design + HTTP status codes** (200/201/204/400/401/403/404) — see `Docs/RequestFlow.md`.
- **Model binding:** `[FromBody]`, `[FromQuery]`, `[FromRoute]`.
- **Drill:** trace one full request from `PostsController` → service → repo → DB and back.

### ASP.NET Core Topics — Full Checklist

**Hosting & Startup**
- `WebApplication.CreateBuilder` / minimal hosting model.
- `builder.Services` (DI registration) vs `app` (middleware pipeline).
- `appsettings.json` + environment-specific config (`appsettings.Development.json`).
- `IConfiguration`, binding config sections, `IOptions<T>` pattern.
- Environments: `IWebHostEnvironment`, `IsDevelopment()`.

**Dependency Injection**
- Lifetimes: `AddScoped` vs `AddSingleton` vs `AddTransient` — pick and justify.
- Constructor injection; why to depend on interfaces, not concretes.
- Captive dependency pitfall (injecting Scoped into Singleton).

**Middleware Pipeline**
- What middleware is; `next()` and short-circuiting.
- Correct order: Exception → HTTPS → CORS → Authentication → Authorization → Endpoints.
- Built-in vs custom middleware.

**Controllers & Routing**
- `[ApiController]`, attribute routing (`[Route]`, `[HttpGet]`, `[HttpPost]`).
- `IActionResult` / `ActionResult<T>`; helpers `Ok()`, `Created()`, `NotFound()`, `BadRequest()`.
- Model binding sources; automatic model-state validation.
- REST conventions and correct status codes.

## Day 3 — Wed July 9 · EF Core & Database

- **DbContext, `DbSet`, navigation properties, relationships** — `Data/DevConnectDbContext.cs`, `Models/`.
- **Migrations:** `add-migration` / `database update`, what the snapshot file is.
- **Cascade delete**, one-to-many (User → Posts → Likes/Comments).
- **Deferred execution & the N+1 problem** (why you use `Include`/projection).
- Reference: `Docs/Database.md`.

### EF Core Topics — Full Checklist

**Core Concepts**
- ORM: what it is and why (maps C# objects ↔ tables).
- `DbContext` and `DbSet<T>`; `DbContext` lifetime (Scoped).
- Code-First vs Database-First (you use Code-First).
- Fluent API vs Data Annotations for configuration.

**Modeling & Relationships**
- Primary keys, foreign keys, navigation properties.
- One-to-many (User → Posts → Likes/Comments), many-to-many.
- Cascade delete behavior and when to restrict it.

**Migrations**
- `Add-Migration` / `dotnet ef migrations add`.
- `Update-Database` / `dotnet ef database update`.
- The model snapshot file and migration history table (`__EFMigrationsHistory`).

**Querying**
- LINQ → SQL translation; `IQueryable` deferred execution.
- Eager (`Include`/`ThenInclude`) vs lazy vs explicit loading.
- **N+1 problem** and how to avoid it (projection / `Include`).
- `AsNoTracking()` for read-only queries (performance).
- Change tracking and `SaveChangesAsync()`.

## Day 4 — Thu July 10 · Architecture & Clean Code

- **Service–Repository pattern** & separation of concerns — `Docs/Architecture.md`.
- **DTOs** (why never expose `PasswordHash`) — `DTOs/`.
- **AutoMapper** — `Mappings/MappingProfile.cs`.
- **FluentValidation** — `Validators/AuthValidators.cs`, `Validators/PostValidators.cs`.
- **Talking point:** "Why separate Service from Repository?" (testability, single responsibility).

### Architecture Topics — Full Checklist

**Layering & Patterns**
- Layered architecture: Controller → Service → Repository → DbContext.
- Repository pattern: abstracts data access behind an interface.
- Service layer: business logic, orchestration, mapping.
- Separation of concerns and single responsibility.

**SOLID Principles**
- **S**ingle Responsibility — each class one job.
- **O**pen/Closed — extend without modifying.
- **L**iskov Substitution — subtypes replaceable.
- **I**nterface Segregation — small focused interfaces.
- **D**ependency Inversion — depend on abstractions (your interfaces + DI).

**Clean Code Practices**
- DTOs to decouple API contract from domain models.
- AutoMapper to remove boilerplate mapping.
- FluentValidation to keep validation out of models/controllers.
- Meaningful names, small methods, guard clauses.

## Day 5 — Fri July 11 · Security & Auth

> This is where senior devs dig hardest.

- **JWT:** claims, signature, expiry, `Bearer` flow — `Services/AuthService.cs`, `Docs/AuthSecurity.md`.
- **Password hashing (BCrypt)** — never store plaintext.
- **Role-based authorization:** `[Authorize(Roles="Admin")]` + ownership checks.
- **OIDC/OAuth** (Google + GitHub) callback flow.
- **CORS** — what problem it actually solves (`Docs/CORS.md`).
- **Anticipate:** "Where is the JWT secret stored / how would you rotate it?"

### Security Topics — Full Checklist

**Authentication vs Authorization**
- AuthN = who you are; AuthZ = what you can do.
- `[Authorize]` vs `[AllowAnonymous]`.

**JWT**
- Structure: header, payload (claims), signature.
- Signing key, issuer, audience, expiry validation.
- `Bearer` scheme; `Authorization: Bearer <token>` header.
- Stateless auth — no server session; pros/cons.

**Password Security**
- Hashing vs encryption; why BCrypt (salt + slow hash).
- Never store or log plaintext passwords.

**Authorization Models**
- Role-based (`[Authorize(Roles = "Admin")]`).
- Claims-based authorization; resource/ownership checks.

**External Auth (OIDC / OAuth2)**
- OAuth2 authorization-code flow.
- OIDC adds identity (ID token) on top of OAuth2.
- Google + GitHub provider setup and callback handling.

**Web Security Basics**
- CORS: browser same-origin policy and allowed origins.
- HTTPS, secrets management (never commit secrets).
- Common threats awareness: SQL injection (EF parameterizes), XSS, CSRF.

## Day 6 — Sat July 12 · Cross-Cutting & Testing

- **Output caching** — `Docs/Caching.md`, when *not* to cache.
- **Serilog structured logging** — `Docs/Serilog.md`.
- **Pagination & sorting** — `Docs/Pagination.md`, `Docs/Sorting.md`.
- **Swagger/OpenAPI** — `Docs/Swagger.md`.
- **Unit vs Integration tests** — walk through `DevConnect.Tests/`, mocking with Moq, why you test the service layer.
- **Action:** run `dotnet test` and be ready to show green results live.

### Cross-Cutting & Testing Topics — Full Checklist

**Caching**
- Output caching: what it caches and for how long.
- When NOT to cache (user-specific / frequently changing data).
- Cache invalidation awareness.

**Logging**
- Structured logging with Serilog (properties, not just strings).
- Log levels: Information, Warning, Error.
- Sinks (console, file) and configuration in `appsettings.json`.

**API Features**
- Pagination: `Skip`/`Take`, `PagedResult<T>`, total count metadata.
- Sorting: safe sort-field whitelisting, ascending/descending.
- Swagger/OpenAPI: auto docs, JWT auth button, DTO schemas.

**Testing**
- Unit vs integration tests — scope and speed trade-offs.
- xUnit basics: `[Fact]`, `[Theory]`, Arrange-Act-Assert.
- Mocking dependencies with Moq (isolate the unit under test).
- Why test the service layer (business logic).
- In-memory / test DB for integration tests.
- `dotnet test` and reading results.

## Day 7 — Sun July 13 · Full Dress Rehearsal

- Run backend + `devconnectwebapp` frontend end-to-end.
- Do a **timed 15–20 min demo** out loud: register → login → create post → like/comment → paginate → Swagger → logs → tests.
- Prepare a **1-page cheat sheet** of the 18 concepts (use the master table in `Docs/ConceptsCovered.md`).
- Write down 5 likely questions + your answers.

## July 14 — Demo Day

- Start API + Swagger + frontend **before** they arrive; keep terminals ready.
- Demo order: architecture overview → live feature walkthrough → code deep-dive on 2–3 topics you're strongest in → tests → "what I'd improve next."

---

## Suggested Demo Narrative (5 mins)

1. **Problem/scope** — "DevConnect is a social API: users, posts, likes, comments."
2. **Architecture** — controller → service → repository → EF Core, with DTOs + AutoMapper.
3. **Security** — JWT + roles + OAuth.
4. **Quality** — validation, logging, caching, pagination, tests.
5. **What's next** — refactors/features you'd add.

---

## Quick Tips

- Prepare for the classic follow-ups: *Scoped vs Singleton*, *why DTOs*, *how JWT validation works*, *N+1 problem*, *why interfaces*.
- If asked something you didn't implement, say how you'd approach it — reasoning matters more than coverage.
- Keep `Docs/ConceptsCovered.md` open as your safety net.

---

## Daily Checklist

| Day | Date | Focus | Done |
|-----|------|-------|------|
| 1 | Jul 7 | C# Language Foundations | ☐ |
| 2 | Jul 8 | ASP.NET Core Fundamentals | ☐ |
| 3 | Jul 9 | EF Core & Database | ☐ |
| 4 | Jul 10 | Architecture & Clean Code | ☐ |
| 5 | Jul 11 | Security & Auth | ☐ |
| 6 | Jul 12 | Cross-Cutting & Testing | ☐ |
| 7 | Jul 13 | Full Dress Rehearsal | ☐ |
| — | Jul 14 | **Demo Day** | ☐ |
