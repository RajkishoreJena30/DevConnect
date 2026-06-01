using AutoMapper;
using DevConnect.Controllers;
using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DevConnect.Tests.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// AuthController depends on DevConnectDbContext directly (not a service layer),
// so we use EF InMemory for DB + Moq for IAuthService and IMapper.
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class AuthControllerTests
{
    private DevConnectDbContext _context = null!;
    private Mock<IConfiguration> _configMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private Mock<IAuthService> _authServiceMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DevConnectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context          = new DevConnectDbContext(options);
        _configMock       = new Mock<IConfiguration>();
        _mapperMock       = new Mock<IMapper>();
        _authServiceMock  = new Mock<IAuthService>();

        _controller = new AuthController(
            _context, _configMock.Object, _mapperMock.Object, _authServiceMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: Register — new user → 200 OK with token
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Register_NewUser_Returns200WithToken()
    {
        // Arrange
        var dto  = new RegisterDTO { Name = "Alice", Email = "alice@test.com", Password = "Pass@1234" };
        var user = new User { Id = 1, Name = dto.Name, Email = dto.Email, Role = "User", PasswordHash = string.Empty };

        // Moq: mapper converts RegisterDTO → User
        _mapperMock.Setup(m => m.Map<User>(dto)).Returns(user);
        // Moq: auth service generates a token string
        _authServiceMock.Setup(a => a.GenerateToken(user)).Returns("fake.jwt.token");

        // Act — Register returns ActionResult<AuthResponseDTO>; .Result gives the inner IActionResult
        var actionResult = await _controller.Register(dto);
        var result = actionResult.Result as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        var response = result.Value as AuthResponseDTO;
        Assert.That(response!.Token, Is.EqualTo("fake.jwt.token"));
        Assert.That(response.Email,  Is.EqualTo("alice@test.com"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: Register — duplicate email → 400 BadRequest
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Register_DuplicateEmail_Returns400()
    {
        // Seed a user with the same email
        _context.Users.Add(new User
        {
            Id = 1, Name = "Existing", Email = "dupe@test.com",
            Role = "User", PasswordHash = "hashed"
        });
        await _context.SaveChangesAsync();

        var dto = new RegisterDTO { Name = "New", Email = "dupe@test.com", Password = "Pass@1234" };

        var actionResult = await _controller.Register(dto);

        Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: Login — correct credentials → 200 OK with token
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Seed a user with a real BCrypt hash
        var hashed = BCrypt.Net.BCrypt.HashPassword("Pass@1234");
        var user   = new User
        {
            Id = 1, Name = "Bob", Email = "bob@test.com",
            Role = "User", PasswordHash = hashed
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _authServiceMock.Setup(a => a.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var dto = new LoginDTO { Email = "bob@test.com", Password = "Pass@1234" };

        // Act — Login returns ActionResult<AuthResponseDTO>; .Result gives the inner IActionResult
        var actionResult = await _controller.Login(dto);
        var result = actionResult.Result as OkObjectResult;

        // Assert
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.That((result.Value as AuthResponseDTO)!.Token, Is.EqualTo("jwt-token"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: Login — wrong password → 401 Unauthorized
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Login_WrongPassword_Returns401()
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword("RealPassword@1");
        _context.Users.Add(new User
        {
            Id = 1, Name = "Carol", Email = "carol@test.com",
            Role = "User", PasswordHash = hashed
        });
        await _context.SaveChangesAsync();

        var dto = new LoginDTO { Email = "carol@test.com", Password = "WrongPassword@1" };

        var actionResult = await _controller.Login(dto);

        Assert.That(actionResult.Result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [TearDown]
    public void TearDown() => _context.Dispose();
}
