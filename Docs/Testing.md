# Testing in DevConnect — NUnit · Mocking · Test Containers

> **Goal of this document** — explain the three testing tools used in the
> `DevConnect.Tests` project, show how each one works, and map every concept
> to a concrete example from the codebase.

---

## Table of Contents

1. [Why Test?](#1-why-test)
2. [Project Structure](#2-project-structure)
3. [NUnit — Test Framework](#3-nunit--test-framework)
4. [Mock — Mocking Library](#4-Mock-mocking-library)
5. [EF Core InMemory — Lightweight Fake DB](#5-ef-core-inmemory--lightweight-fake-db)
6. [Test Containers — Real DB in a Docker Container](#6-Test Containers-real-db-in-a-docker-container)
7. [Running the Tests](#7-running-the-tests)
8. [Quick Reference — NUnit Attributes](#8-quick-reference--nunit-attributes)

---

## 1. Why Test?

| Without Tests | With Tests |
|---|---|
| Manual testing after every change | Automated verification in seconds |
| Bugs discovered by users | Bugs caught before they ship |
| Afraid to refactor | Refactor with confidence |
| No documentation of expected behavior | Tests act as living documentation |

**Three layers of testing used here:**

```
┌───────────────────────────────────────┐
│  Integration Tests (Test Containers)   │  ← Real SQL Server in Docker
├───────────────────────────────────────┤
│  Unit Tests — Services / Controllers  │  ← Mock (fake dependencies)
├───────────────────────────────────────┤
│  Unit Tests — Validators              │  ← Pure logic, no mocks needed
└───────────────────────────────────────┘
```

---

## 2. Project Structure

```
DevConnect.Tests/
├── GlobalUsings.cs                          ← global using NUnit.Framework
├── UnitTest1.cs                             ← placeholder (ignore)
│
├── Services/
│   ├── PostServiceTests.cs                  ← Unit tests for PostService (Mock)
│   └── AuthService.cs                       ← Unit tests for AuthService (InMemory)
│
├── Controllers/
│   ├── PostsControllerTests.cs              ← Unit tests for PostsController (Mock)
│   └── AuthControllerTests.cs              ← Unit tests for AuthController (InMemory + Mock)
│
├── Validators/
│   └── ValidatorTests.cs                    ← NUnit parameterised validator tests
│
└── Integration/
    └── PostRepositoryIntegrationTests.cs    ← Test Containers (real SQL Server)
```

---

## 3. NUnit — Test Framework

NUnit is the test **framework** — it discovers tests, runs them, and reports results.

### Core Attributes

| Attribute | Purpose | Example |
|---|---|---|
| `[TestFixture]` | Marks the class as a test class | `public class PostServiceTests` |
| `[Test]` | Marks a single test method | `public async Task GetAll_Returns200()` |
| `[SetUp]` | Runs **before every** test method | Create fresh mocks |
| `[TearDown]` | Runs **after every** test method | Dispose DB connections |
| `[OneTimeSetUp]` | Runs **once** before all tests in the class | Start Docker container |
| `[OneTimeTearDown]` | Runs **once** after all tests in the class | Stop Docker container |
| `[TestCase(...)]` | Parameterized test — one method, many inputs | Password strength rules |

### Assertion Style — Constraint Model

NUnit's preferred way is `Assert.That(actual, constraint)`:

```csharp
// ── Value equality ────────────────────────────────────────────────────
Assert.That(result.Id,    Is.EqualTo(5));
Assert.That(result.Title, Is.EqualTo("Hello"));

// ── Null checks ───────────────────────────────────────────────────────
Assert.That(result, Is.Not.Null);
Assert.That(result, Is.Null);

// ── Boolean ───────────────────────────────────────────────────────────
Assert.That(result, Is.True);
Assert.That(result, Is.False);

// ── Collection ────────────────────────────────────────────────────────
Assert.That(list, Has.Count.EqualTo(2));
Assert.That(list, Is.Empty);
Assert.That(list.All(p => p.UserId == 10), Is.True);

// ── Type check ────────────────────────────────────────────────────────
Assert.That(result, Is.InstanceOf<NotFoundResult>());

// ── Greater / Less ────────────────────────────────────────────────────
Assert.That(id, Is.GreaterThan(0));
```

### SetUp / TearDown Lifecycle

```csharp
[TestFixture]
public class PostServiceTests
{
    private Mock<IPostRepository> _repoMock;
    private PostService _service;

    // Runs before EVERY test — creates fresh state
    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IPostRepository>();
        _service  = new PostService(_repoMock.Object, new Mock<IMapper>().Object);
    }

    [Test]
    public async Task Test_One() { /* ... */ }

    [Test]
    public async Task Test_Two() { /* ... */ }

    // Runs after EVERY test — clean up resources
    [TearDown]
    public void TearDown()
    {
        // dispose anything opened in SetUp
    }
}
```

### Parameterized Tests with `[TestCase]`

Instead of writing one test per input, list all inputs in `[TestCase]`:

```csharp
// Tests all four weak password variants with a single method
[TestCase("short")]           // too short
[TestCase("alllowercase1!")]  // no uppercase
[TestCase("ALLUPPERCASE1!")]  // no lowercase
[TestCase("NoSpecial1234")]   // no special character
public void Register_WeakPassword_ShouldFail(string password)
{
    var model  = new RegisterDTO { Name = "Alice", Email = "a@b.com", Password = password };
    var result = _validator.TestValidate(model);
    result.ShouldHaveValidationErrorFor(x => x.Password);
}
```

> **From the codebase:** [DevConnect.Tests/Validators/ValidatorTests.cs](../DevConnect.Tests/Validators/ValidatorTests.cs)

---

## 4. Mocking with Mock Objects

**Mocking** replaces a real dependency (database, external API) with a
controllable fake so the test focuses only on the class under test.

```
Real test                         Mocked test
─────────────────────────────     ─────────────────────────────
PostService                       PostService
    └── PostRepository (EF)           └── Mock<IPostRepository>
            └── SQL Server                   └── returns data we define
```

### Creating a Mock

```csharp
// Mock creates a fake that implements IPostRepository
var repoMock = new Mock<IPostRepository>();

// .Object gives you the fake instance to inject
var service = new PostService(repoMock.Object, mapperMock.Object);
```

### Setup — Tell the Mock What to Return

```csharp
// When GetAllAsync() is called, return this list
repoMock
    .Setup(r => r.GetAllAsync())
    .ReturnsAsync(new List<Post> { post1, post2 });

// When GetByIdAsync(5) is called, return post
repoMock
    .Setup(r => r.GetByIdAsync(5))
    .ReturnsAsync(post);

// When GetByIdAsync(99) is called, return null (not found)
repoMock
    .Setup(r => r.GetByIdAsync(99))
    .ReturnsAsync((Post?)null);
```

### Verify — Confirm the Mock Was Called

```csharp
// Assert that CreateAsync was called exactly once with that post
repoMock.Verify(r => r.CreateAsync(post), Times.Once);

// Assert that UpdateAsync was NEVER called (e.g. non-owner tried to update)
repoMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
```

### `It.IsAny<T>()` — Wildcard Matcher

```csharp
// Match any Post instance (don't care about the exact value)
repoMock.Verify(r => r.DeleteAsync(It.IsAny<Post>()), Times.Once);
```

### Full Example — PostService unit test

```csharp
[Test]
public async Task CreatePostAsync_CreatesAndReturnsPost()
{
    // ── Arrange ─────────────────────────────────────────────────────
    var userId    = 10;
    var createDto = new CreatePostDTO { Title = "Hello", Content = "World content" };
    var post      = new Post { Id = 1, UserId = userId, Title = "Hello" };
    var responseDto = new PostResponseDTO { Id = 1, UserId = userId };

    // Tell mapper how to convert DTO → Post
    _mapperMock.Setup(m => m.Map<Post>(createDto)).Returns(post);
    // Tell repo to return the post after save
    _repoMock.Setup(r => r.CreateAsync(post)).ReturnsAsync(post);
    // Tell mapper how to convert Post → DTO
    _mapperMock.Setup(m => m.Map<PostResponseDTO>(post)).Returns(responseDto);

    // ── Act ──────────────────────────────────────────────────────────
    var result = await _service.CreatePostAsync(userId, createDto);

    // ── Assert ───────────────────────────────────────────────────────
    Assert.That(result.Id,     Is.EqualTo(1));
    Assert.That(result.UserId, Is.EqualTo(userId));
    _repoMock.Verify(r => r.CreateAsync(post), Times.Once);
}
```

> **From the codebase:** [DevConnect.Tests/Services/PostServiceTests.cs](../DevConnect.Tests/Services/PostServiceTests.cs)

### Mocking the HTTP User (Controller Tests)

Controllers use `User.FindFirstValue(ClaimTypes.NameIdentifier)` to read the
logged-in user's ID from the JWT token. In tests we inject a fake `ClaimsPrincipal`:

```csharp
private static ClaimsPrincipal FakeUser(int userId = 10, string role = "User")
{
    var claims   = new[] {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, role)
    };
    var identity = new ClaimsIdentity(claims, "Test");
    return new ClaimsPrincipal(identity);
}

// Inject into the controller's HttpContext
_controller = new PostsController(_serviceMock.Object)
{
    ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext { User = FakeUser() }
    }
};
```

> **From the codebase:** [DevConnect.Tests/Controllers/PostsControllerTests.cs](../DevConnect.Tests/Controllers/PostsControllerTests.cs)

---

## 5. EF Core InMemory — Lightweight Fake DB

For classes that use `DevConnectDbContext` directly (like `AuthService` and
`AuthController`), an in-memory database is easier than mocking every `DbSet`.

```csharp
// Each test gets its own isolated in-memory database
var options = new DbContextOptionsBuilder<DevConnectDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique per test
    .Options;

var context = new DevConnectDbContext(options);
```

### When to use InMemory vs Mock

| Scenario | Tool |
|---|---|
| Class uses `DbContext` directly | EF InMemory |
| Class depends on an interface (`IPostRepository`) | Mock |
| Need to test SQL-specific features (indexes, constraints) | Test Containers |

> **Limitation:** InMemory does not enforce database constraints (unique indexes,
> foreign keys). For that, use Test Containers.

---

## 6. Test Containers — Real DB in a Docker Container

Test Containers starts an actual Docker container during the test run, giving you
a real SQL Server instance with full constraint enforcement and migration support.

### Prerequisites

- Docker Desktop must be running
- Package: `Test Containers.MsSql`

### Lifecycle in Code

```csharp
[TestFixture]
public class PostRepositoryIntegrationTests
{
    private MsSqlContainer _sqlContainer;
    private DevConnectDbContext _context;
    private PostRepository _repo;

    // ── Runs ONCE — start the Docker container ──────────────────────
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await _sqlContainer.StartAsync(); // blocks until SQL Server is ready

        var options = new DbContextOptionsBuilder<DevConnectDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _context = new DevConnectDbContext(options);
        await _context.Database.EnsureCreatedAsync(); // apply schema

        _repo = new PostRepository(_context);
    }

    // ── Runs before EVERY test — reset data ─────────────────────────
    [SetUp]
    public async Task SetUp()
    {
        _context.Posts.RemoveRange(_context.Posts);
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
    }

    // ── Example test ─────────────────────────────────────────────────
    [Test]
    public async Task CreateAsync_PersistsPostToRealDatabase()
    {
        var user = new User { Name = "Bob", Email = "bob@test.com",
                              PasswordHash = "h", Role = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var post    = new Post { Title = "Hello", Content = "Real SQL.", UserId = user.Id };
        var created = await _repo.CreateAsync(post);

        // SQL Server assigns a real auto-increment ID
        Assert.That(created.Id, Is.GreaterThan(0));
    }

    // ── Runs ONCE — stop and remove the Docker container ────────────
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
        await _sqlContainer.StopAsync();
        await _sqlContainer.DisposeAsync();
    }
}
```

> **From the codebase:** [DevConnect.Tests/Integration/PostRepositoryIntegrationTests.cs](../DevConnect.Tests/Integration/PostRepositoryIntegrationTests.cs)

### What Test Containers Tests Cover

| Test | What it proves |
|---|---|
| `CreateAsync_PersistsPostToRealDatabase` | SQL Server assigns identity ID correctly |
| `GetAllAsync_ReturnsPostsWithUser` | EF `Include()` loads navigation properties |
| `GetByIdAsync_ReturnsCorrectPost` | Querying by primary key works |
| `GetByIdAsync_NonExistent_ReturnsNull` | Returns null for missing record |
| `UpdateAsync_SavesChanges` | Changes are persisted to disk |
| `DeleteAsync_RemovesPost` | Row is deleted from the table |
| `ExistsAsync_ReturnsCorrectly` | AnyAsync returns true/false correctly |
| `GetByUserIdAsync_ReturnsOnlyUserPosts` | WHERE filter scopes results correctly |

---

## 7. Running the Tests

### Run all tests

```bash
dotnet test DevConnect.Tests/DevConnect.Tests.csproj
```

### Run only unit tests (skip integration / Docker)

```bash
dotnet test --filter "Namespace!=DevConnect.Tests.Integration"
```

### Run only integration tests

```bash
dotnet test --filter "Namespace=DevConnect.Tests.Integration"
```

### Run a specific test by name

```bash
dotnet test --filter "Name=GetAllPostsAsync_ReturnsAllPosts"
```

### Run with verbose output

```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 8. Quick Reference — NUnit Attributes

```csharp
[TestFixture]          // class is a test container
[Test]                 // single test method
[SetUp]                // before each test
[TearDown]             // after each test
[OneTimeSetUp]         // before ALL tests in the class (once)
[OneTimeTearDown]      // after ALL tests in the class (once)
[TestCase(value)]      // parameterised test input
[Ignore("reason")]     // skip a test
[Category("Integration")] // tag for filtering
```

### Assert Constraints Quick Reference

```csharp
// Equality
Is.EqualTo(expected)
Is.Not.EqualTo(expected)

// Null
Is.Null
Is.Not.Null

// Type
Is.InstanceOf<T>()

// Booleans
Is.True  /  Is.False

// Collections
Has.Count.EqualTo(n)
Is.Empty
Contains.Item(value)

// Comparison
Is.GreaterThan(n)
Is.LessThan(n)
Is.GreaterThanOrEqualTo(n)

// Strings
Does.Contain("substring")
Does.StartWith("prefix")
Does.Match("regex")

// Exceptions
Assert.ThrowsAsync<InvalidOperationException>(() => service.Method());
```

---

## Summary — Which Tool for Which Layer?

```
┌──────────────────────┬─────────────────────────────┬───────────────────────────┐
│ Layer                │ Tool                        │ Why                       │
├──────────────────────┼─────────────────────────────┼───────────────────────────┤
│ Validators           │ NUnit + FluentValidation    │ Pure logic, no DB needed  │
│                      │ TestHelper                  │                           │
├──────────────────────┼─────────────────────────────┼───────────────────────────┤
│ Service layer        │ NUnit + Mock                 │ Mock IRepository to avoid │
│ (PostService)        │                             │ touching the database     │
├──────────────────────┼─────────────────────────────┼───────────────────────────┤
│ Service layer        │ NUnit + EF InMemory         │ AuthService uses DbContext │
│ (AuthService)        │                             │ directly, no interface    │
├──────────────────────┼─────────────────────────────┼───────────────────────────┤
│ Controller layer     │ NUnit + Mock                 │ Mock the service layer,   │
│                      │ + FakeUser ClaimsPrincipal  │ test HTTP status codes    │
├──────────────────────┼─────────────────────────────┼───────────────────────────┤
│ Repository layer     │ NUnit + Test Containers      │ Real SQL Server to test   │
│ (PostRepository)     │ (MsSqlContainer)            │ EF queries + constraints  │
└──────────────────────┴─────────────────────────────┴───────────────────────────┘
```
