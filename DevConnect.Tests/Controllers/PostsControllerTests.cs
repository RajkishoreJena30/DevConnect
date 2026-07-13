using DevConnect.Controllers;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Moq;
using System.Security.Claims;

namespace DevConnect.Tests.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Controller tests verify HTTP-layer behaviour:
//   - Correct status codes (200, 201, 204, 404)
//   - Correct response bodies
//   - Correct delegations to the service layer
//
// The controller depends only on IPostService, so we mock that interface.
// We also fake the ClaimsPrincipal (User) so [Authorize] routes work without
// a real JWT token.
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class PostsControllerTests
{
    private Mock<IPostService> _serviceMock = null!;
    private Mock<IOutputCacheStore> _cacheMock = null!;
    private PostsController _controller = null!;

    // ── Helper: fake logged-in user with a known ID ──────────────────────────
    private static ClaimsPrincipal FakeUser(int userId = 10, string role = "User")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity  = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IPostService>();
        _cacheMock   = new Mock<IOutputCacheStore>();
        _controller  = new PostsController(_serviceMock.Object, _cacheMock.Object)
        {
            // Inject the fake HttpContext so User.FindFirstValue() works.
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = FakeUser() }
            }
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GET /api/posts → 200 OK + list
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetAll_Returns200_WithPosts()
    {
        // Arrange
        var paged = new PagedResult<PostResponseDTO>
        {
            Items = new List<PostResponseDTO>
            {
                new() { Id = 1, Title = "Post 1" },
                new() { Id = 2, Title = "Post 2" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize   = 10
        };
        _serviceMock.Setup(s => s.GetPagedPostsAsync(It.IsAny<PostQueryParams>()))
                    .ReturnsAsync(paged);

        // Act
        var result = await _controller.GetAll(new PostQueryParams()) as OkObjectResult;

        // Assert — check HTTP status code and body
        Assert.That(result,        Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.That((result.Value as PagedResult<PostResponseDTO>)!.Items.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GET /api/posts/{id} — post found → 200 OK
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetById_WhenFound_Returns200()
    {
        var dto = new PostResponseDTO { Id = 5, Title = "Hello" };
        _serviceMock.Setup(s => s.GetPostByIdAsync(5)).ReturnsAsync(dto);

        var result = await _controller.GetById(5) as OkObjectResult;

        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.That((result.Value as PostResponseDTO)!.Id, Is.EqualTo(5));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GET /api/posts/{id} — post not found → 404 NotFound
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetById_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetPostByIdAsync(99)).ReturnsAsync((PostResponseDTO?)null);

        var result = await _controller.GetById(99);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: POST /api/posts → 201 Created
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Create_Returns201WithCreatedPost()
    {
        var dto      = new CreatePostDTO { Title = "New", Content = "Content here for test" };
        var response = new PostResponseDTO { Id = 1, Title = "New" };

        _serviceMock.Setup(s => s.CreatePostAsync(10, dto)).ReturnsAsync(response);

        var result = await _controller.Create(dto) as CreatedAtActionResult;

        Assert.That(result,              Is.Not.Null);
        Assert.That(result!.StatusCode,  Is.EqualTo(201));
        Assert.That((result.Value as PostResponseDTO)!.Id, Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: PUT /api/posts/{id} — owner updates → 204 NoContent
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Update_WhenOwner_Returns204()
    {
        var dto = new CreatePostDTO { Title = "Updated", Content = "Updated content value" };
        _serviceMock.Setup(s => s.UpdatePostAsync(1, 10, dto)).ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: PUT /api/posts/{id} — not owner → 404 NotFound
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Update_WhenNotOwner_Returns404()
    {
        var dto = new CreatePostDTO { Title = "Hack", Content = "Hacked content value" };
        _serviceMock.Setup(s => s.UpdatePostAsync(1, 10, dto)).ReturnsAsync(false);

        var result = await _controller.Update(1, dto);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DELETE /api/posts/{id} → 204 NoContent
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Delete_WhenOwner_Returns204()
    {
        _serviceMock.Setup(s => s.DeletePostAsync(1, 10, "User")).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DELETE /api/posts/{id} — post not found → 404
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeletePostAsync(99, 10, "User")).ReturnsAsync(false);

        var result = await _controller.Delete(99);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
