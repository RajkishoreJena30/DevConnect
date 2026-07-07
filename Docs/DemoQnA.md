# DevConnect — Likely Demo Q&A

> Practice answers for the manager + senior developer review on **July 14, 2026**.
> Answers are tied to the actual DevConnect codebase. Say the reasoning out loud, not just the definition.

---

## C# Language

**1. What's the difference between a value type and a reference type?**
Value types (`int`, `struct`, `enum`) hold data directly and are copied on assignment; reference types (`class`, `string`, arrays) hold a reference to an object on the heap. Two reference variables can point to the same object.

**2. `record` vs `class` — when would you use each?**
`class` for entities with identity and mutable state (my EF models like `Post`). `record` for immutable data with value-based equality — good for DTOs where two objects with the same values should be "equal."

**3. What does `async`/`await` actually do?**
It frees the thread while waiting on I/O (DB, HTTP) instead of blocking it. My repository methods use `ToListAsync`/`SaveChangesAsync` so the server can handle other requests during DB calls.

**4. `IEnumerable` vs `IQueryable`?**
`IEnumerable` runs in memory (LINQ-to-Objects). `IQueryable` builds an expression tree that EF Core translates to SQL, so filtering/paging happens in the database. My pagination uses `IQueryable` so `Skip`/`Take` become SQL, not in-memory filtering.

**5. What is deferred execution in LINQ?**
The query doesn't run until it's enumerated (`ToListAsync`, `Count`). This lets me build up a query (`Where` + `OrderBy` + `Skip`/`Take`) and execute it once.

**6. Interface vs abstract class?**
Interface = a pure contract with no implementation; a class can implement many. Abstract class = shared base with some implementation but can't be instantiated, single inheritance. I use interfaces (`IPostService`, `IPostRepository`) for DI and testability.

---

## ASP.NET Core

**7. Explain the request pipeline / middleware order.**
Each request flows through ordered middleware. Order matters: CORS before Authentication, Authentication before Authorization, then endpoints. If CORS came after auth, preflight requests would fail.

**8. Scoped vs Singleton vs Transient — why are your repos Scoped?**
Scoped = one instance per HTTP request. My repositories and `DbContext` are Scoped so all work in a single request shares one tracked context. Singleton would share a `DbContext` across requests (unsafe); Transient would create too many.

**9. What is dependency injection and why use it?**
The framework creates and injects dependencies instead of classes `new`-ing them. It decouples code, makes it testable (I can inject mocks), and centralizes lifetime management. Registered in `Program.cs`.

**10. Why depend on interfaces instead of concrete classes?**
So I can swap implementations and mock them in unit tests. Controllers depend on `IPostService`, not `PostService`.

**11. How does model binding work?**
ASP.NET Core maps request data to parameters — `[FromBody]` for JSON, `[FromQuery]` for query strings (my `PostQueryParams`), `[FromRoute]` for URL segments.

---

## EF Core & Database

**12. What is an ORM and why use EF Core?**
It maps C# classes to tables and translates LINQ to SQL, so I write strongly-typed queries instead of raw SQL and get change tracking and migrations.

**13. What's a migration and how do you apply one?**
A migration is a versioned schema change generated from model changes (`Add-Migration`) and applied with `Update-Database`. History is tracked in `__EFMigrationsHistory`.

**14. What is the N+1 problem and how do you avoid it?**
Loading a list then lazily loading each related entity = 1 + N queries. I avoid it with `Include` or projection so related data comes in one query.

**15. What is `AsNoTracking` and when do you use it?**
It skips change tracking for read-only queries, improving performance. Good for GET/list endpoints where I don't update the entities.

**16. How are your relationships modeled?**
One-to-many: User → Posts, Post → Likes/Comments, using navigation properties and foreign keys, with cascade delete so a deleted post removes its likes/comments.

---

## Architecture

**17. Why the Service–Repository pattern?**
Repository isolates data access; Service holds business logic. This keeps controllers thin, improves testability, and follows single responsibility. I can test the service with a mocked repository.

**18. Why use DTOs instead of returning models directly?**
To decouple the API contract from the database model and avoid leaking sensitive fields like `PasswordHash`. DTOs also shape exactly what the client needs.

**19. How does your project follow SOLID?**
Single-responsibility layers, interfaces for abstraction (DIP), small focused interfaces (ISP), and DI so high-level code depends on abstractions.

**20. Why AutoMapper and FluentValidation?**
AutoMapper removes repetitive model↔DTO mapping. FluentValidation moves validation rules into dedicated classes, keeping models and controllers clean and rules reusable.

---

## Security

**21. How does JWT authentication work in your app?**
On login I verify the password (BCrypt), then issue a signed JWT with claims (userId, email, role). The client sends it as `Authorization: Bearer <token>`; middleware validates signature, issuer, audience, and expiry on every request.

**22. Why hash passwords, and why BCrypt?**
So a database breach doesn't expose passwords. BCrypt adds a salt and is deliberately slow, making brute-force attacks expensive.

**23. Difference between authentication and authorization?**
Authentication verifies identity (JWT/login). Authorization decides what that identity can do (`[Authorize(Roles = "Admin")]`, ownership checks in `PostService`).

**24. How does role-based authorization work here?**
The user's role is a claim in the JWT. `[Authorize(Roles = "Admin")]` enforces it, and my service double-checks ownership so users can only edit their own posts unless they're Admin.

**25. What is OAuth/OIDC and how did you use it?**
OAuth2 lets users log in via Google/GitHub without a password; OIDC adds an identity token. On callback I find or create the user and issue my own DevConnect JWT.

**26. What problem does CORS solve?**
Browsers block cross-origin requests by default. CORS explicitly allows my frontend origins (e.g. localhost:3000) to call the API.

**27. Where is your JWT secret stored and how would you secure it in production?**
In config for dev; in production I'd use environment variables / a secrets manager (Azure Key Vault), never commit it, and rotate keys periodically.

---

## Cross-Cutting & Testing

**28. What did you cache and when should you NOT cache?**
I use output caching on read-heavy GET endpoints. Don't cache user-specific or frequently changing data, or you risk serving stale/wrong results.

**29. Why structured logging with Serilog?**
It logs properties, not just text, so logs are queryable and richer for diagnostics. Configured with levels and sinks in `appsettings.json`.

**30. Unit vs integration tests — what's the difference and what did you test?**
Unit tests isolate one class with mocked dependencies (service logic with Moq). Integration tests exercise real components together, like the repository against a test database. I focused unit tests on the service/business logic.

**31. Why test the service layer specifically?**
That's where business rules live (ownership checks, clamping page size, mapping). Testing it gives the most confidence per test.

---

## Wrap-Up Questions (be ready for these)

**32. What would you improve or add next?**
Refresh tokens, global exception-handling middleware, more test coverage, rate limiting, and moving secrets to a vault.

**33. What was the hardest part and what did you learn?**
Pick one real example (e.g. wiring OIDC callbacks or getting middleware order right) and explain how you debugged it.

**34. If traffic grew 100x, what would you change?**
Add caching layers, `AsNoTracking` on reads, database indexing, pagination everywhere, and consider async/queueing for heavy work.
