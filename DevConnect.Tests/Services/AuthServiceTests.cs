using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Models;
using DevConnect.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ─────────────────────────────────────────────────────────────────────────────
// AuthService tests use EF Core's InMemory provider instead of Moq because
// AuthService directly calls _context (no repository abstraction).
// InMemory = lightweight fake DB that lives in RAM for the duration of the test.
// ─────────────────────────────────────────────────────────────────────────────
namespace DevConnect.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private DevConnectDbContext _context = null!;
    private IConfiguration _config = null!;
    private AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        // Each test gets its own in-memory database — prevents state leaking.
        var options = new DbContextOptionsBuilder<DevConnectDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DevConnectDbContext(options);

        // Build a minimal IConfiguration that AuthService needs for JWT generation.
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Key"]          = "Super_Secret_Key_For_Testing_Must_Be_32_Chars!",
                ["JwtSettings:Issuer"]       = "DevConnect",
                ["JwtSettings:Audience"]     = "DevConnectUsers",
                ["JwtSettings:ExpiryInDays"] = "7"
            })
            .Build();

        _service = new AuthService(_context, _config);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GenerateToken returns a non-empty JWT string
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        // Arrange
        var user = new User
        {
            Id           = 1,
            Name         = "Alice",
            Email        = "alice@test.com",
            Role         = "User",
            PasswordHash = "hashed",
            Provider     = "Local"
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert — a JWT has two dots (header.payload.signature)
        Assert.That(token, Is.Not.Empty);
        Assert.That(token.Split('.').Length, Is.EqualTo(3),
            "A valid JWT must contain exactly 3 dot-separated segments.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: FindOrCreateOidcUserAsync — brand new user is created
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task FindOrCreateOidcUser_NewUser_CreatesAndReturns()
    {
        // Arrange
        var dto = new OidcUserDTO
        {
            Email          = "bob@github.com",
            Name           = "Bob",
            Provider       = "GitHub",
            ProviderUserId = "gh-123"
        };

        // Act
        var user = await _service.FindOrCreateOidcUserAsync(dto);

        // Assert
        Assert.That(user.Email,          Is.EqualTo("bob@github.com"));
        Assert.That(user.Provider,       Is.EqualTo("GitHub"));
        Assert.That(user.ProviderUserId, Is.EqualTo("gh-123"));
        Assert.That(_context.Users.Count(), Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: FindOrCreateOidcUserAsync — returning user is found, NOT duplicated
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task FindOrCreateOidcUser_ExistingProvider_ReturnsSameUser()
    {
        // Arrange — seed an existing user
        var existing = new User
        {
            Id             = 1,
            Name           = "Alice",
            Email          = "alice@google.com",
            Provider       = "Google",
            ProviderUserId = "g-456",
            Role           = "User",
            PasswordHash   = string.Empty
        };
        _context.Users.Add(existing);
        await _context.SaveChangesAsync();

        var dto = new OidcUserDTO
        {
            Email          = "alice@google.com",
            Name           = "Alice",
            Provider       = "Google",
            ProviderUserId = "g-456"
        };

        // Act
        var user = await _service.FindOrCreateOidcUserAsync(dto);

        // Assert — no new user created, still 1 record
        Assert.That(user.Id, Is.EqualTo(existing.Id));
        Assert.That(_context.Users.Count(), Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: FindOrCreateOidcUserAsync — email exists locally, OIDC fields linked
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task FindOrCreateOidcUser_ExistingEmail_LinksProvider()
    {
        // Arrange — user registered locally (no OIDC)
        var local = new User
        {
            Id           = 1,
            Name         = "Charlie",
            Email        = "charlie@example.com",
            Role         = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
        };
        _context.Users.Add(local);
        await _context.SaveChangesAsync();

        var dto = new OidcUserDTO
        {
            Email          = "charlie@example.com",
            Name           = "Charlie",
            Provider       = "Google",
            ProviderUserId = "g-789"
        };

        // Act
        var user = await _service.FindOrCreateOidcUserAsync(dto);

        // Assert — same user, now has OIDC fields
        Assert.That(user.Id,             Is.EqualTo(1));
        Assert.That(user.Provider,       Is.EqualTo("Google"));
        Assert.That(user.ProviderUserId, Is.EqualTo("g-789"));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
}
