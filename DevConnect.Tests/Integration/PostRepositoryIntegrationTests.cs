using DevConnect.Data;
using DevConnect.Models;
using DevConnect.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace DevConnect.Tests.Integration;

// ─────────────────────────────────────────────────────────────────────────────
// TESTCONTAINERS INTEGRATION TESTS
//
// What are Testcontainers?
//   A library that spins up real Docker containers during test execution.
//   Here we launch a real SQL Server container, apply EF Core migrations,
//   and run PostRepository against the actual database engine.
//
// Why?
//   - InMemory DB can't test SQL-specific behaviour (indexes, constraints, etc.)
//   - Testcontainers gives us a real DB without a permanent test server.
//   - Container is created ONCE per class ([OneTimeSetUp]) and torn down after.
//
// Prerequisites:
//   - Docker Desktop must be running on the machine.
//   - Testcontainers.MsSql NuGet package installed.
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class PostRepositoryIntegrationTests
{
    // MsSqlContainer builder — configures the SQL Server Docker image.
    private MsSqlContainer _sqlContainer = null!;
    private DevConnectDbContext _context = null!;
    private PostRepository _repo = null!;

    // ─────────────────────────────────────────────────────────────────────────
    // [OneTimeSetUp] — runs ONCE before ALL tests in this fixture.
    // Use it for expensive setup (e.g., starting a container, creating a DB).
    // Compare with [SetUp] which runs before EVERY single test.
    // ─────────────────────────────────────────────────────────────────────────
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // 1. Build the container — pulls mcr.microsoft.com/mssql/server image.
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        // 2. Start the container (blocks until SQL Server is ready).
        await _sqlContainer.StartAsync();

        // 3. Build EF Core context pointing to the containerised SQL Server.
        var options = new DbContextOptionsBuilder<DevConnectDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _context = new DevConnectDbContext(options);

        // 4. Apply all pending EF Core migrations to create the schema.
        //    EnsureCreated() works too for simple cases, but Migrate()
        //    respects your real migration history.
        await _context.Database.EnsureCreatedAsync();

        _repo = new PostRepository(_context);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // [SetUp] — runs before every test.
    // We clear posts so each test starts with a clean state.
    // ─────────────────────────────────────────────────────────────────────────
    [SetUp]
    public async Task SetUp()
    {
        // Remove all posts (and cascade likes/comments) between tests.
        _context.Posts.RemoveRange(_context.Posts);
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
    }

    // ── Helper — creates and saves a User + Post ──────────────────────────────
    private async Task<(User user, Post post)> SeedOnePostAsync()
    {
        var user = new User
        {
            Name         = "Alice",
            Email        = $"alice-{Guid.NewGuid()}@test.com",
            PasswordHash = "hashed",
            Role         = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var post = new Post
        {
            Title   = "Integration Post",
            Content = "Written against a real SQL Server container.",
            UserId  = user.Id
        };
        await _repo.CreateAsync(post);
        return (user, post);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: CreateAsync — post is persisted to real SQL Server
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task CreateAsync_PersistsPostToRealDatabase()
    {
        // Arrange
        var user = new User
        {
            Name = "Bob", Email = "bob@test.com", PasswordHash = "h", Role = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var post = new Post { Title = "Hello DB", Content = "Real SQL Server.", UserId = user.Id };

        // Act
        var created = await _repo.CreateAsync(post);

        // Assert — ID is assigned by SQL Server identity column
        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(await _context.Posts.CountAsync(), Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetAllAsync — returns all posts with navigation properties loaded
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetAllAsync_ReturnsPostsWithUser()
    {
        // Arrange
        await SeedOnePostAsync();

        // Act
        var posts = await _repo.GetAllAsync();

        // Assert — navigation property (User) should be loaded by Include()
        Assert.That(posts, Has.Count.EqualTo(1));
        Assert.That(posts[0].User, Is.Not.Null);
        Assert.That(posts[0].User.Name, Is.EqualTo("Alice"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetByIdAsync — returns correct post
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetByIdAsync_ReturnsCorrectPost()
    {
        var (_, post) = await SeedOnePostAsync();

        var found = await _repo.GetByIdAsync(post.Id);

        Assert.That(found,          Is.Not.Null);
        Assert.That(found!.Title,   Is.EqualTo("Integration Post"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetByIdAsync — non-existent ID returns null
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(99999);
        Assert.That(found, Is.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: UpdateAsync — changes are saved to the real DB
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task UpdateAsync_SavesChanges()
    {
        var (_, post) = await SeedOnePostAsync();

        post.Title = "Updated Title";
        await _repo.UpdateAsync(post);

        // Reload from DB to confirm persistence
        var updated = await _context.Posts.FindAsync(post.Id);
        Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DeleteAsync — post is removed from the real DB
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task DeleteAsync_RemovesPost()
    {
        var (_, post) = await SeedOnePostAsync();

        await _repo.DeleteAsync(post);

        Assert.That(await _context.Posts.CountAsync(), Is.EqualTo(0));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: ExistsAsync — true for existing, false for missing
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task ExistsAsync_ReturnsCorrectly()
    {
        var (_, post) = await SeedOnePostAsync();

        Assert.That(await _repo.ExistsAsync(post.Id), Is.True);
        Assert.That(await _repo.ExistsAsync(99999),   Is.False);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetByUserIdAsync — returns only that user's posts
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetByUserIdAsync_ReturnsOnlyUserPosts()
    {
        var (user, _) = await SeedOnePostAsync();

        // Add a second user + post
        var user2 = new User
        {
            Name = "Dave", Email = "dave@test.com", PasswordHash = "h", Role = "User"
        };
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();
        await _repo.CreateAsync(new Post
        {
            Title = "Dave's Post", Content = "Dave wrote this.", UserId = user2.Id
        });

        // Act — fetch only Alice's posts
        var alicePosts = await _repo.GetByUserIdAsync(user.Id);

        Assert.That(alicePosts, Has.Count.EqualTo(1));
        Assert.That(alicePosts[0].Title, Is.EqualTo("Integration Post"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // [OneTimeTearDown] — runs ONCE after ALL tests.
    // Stop and remove the Docker container.
    // ─────────────────────────────────────────────────────────────────────────
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
        await _sqlContainer.StopAsync();
        await _sqlContainer.DisposeAsync();
    }
}
