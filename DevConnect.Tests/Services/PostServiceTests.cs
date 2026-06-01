using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using DevConnect.Services;
using Moq;

namespace DevConnect.Tests.Services;

// ─────────────────────────────────────────────────────────────────────────────
// NUnit attribute: [TestFixture] marks this class as a test class.
// Equivalent to xUnit's implicit class discovery.
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class PostServiceTests
{
    // Moq creates a fake (mock) of the interface — no real DB needed.
    private Mock<IPostRepository> _repoMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private PostService _service = null!;

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static Post MakePost(int id = 1, int userId = 10) => new Post
    {
        Id        = id,
        Title     = $"Post {id}",
        Content   = "Some content",
        UserId    = userId,
        User      = new User { Id = userId, Name = "Alice", Email = "alice@test.com" },
        Likes     = new List<Like>(),
        Comments  = new List<Comment>()
    };

    private static PostResponseDTO MakeResponse(Post p) => new PostResponseDTO
    {
        Id          = p.Id,
        Title       = p.Title,
        Content     = p.Content,
        AuthorName  = p.User.Name,
        UserId      = p.UserId
    };

    // ─────────────────────────────────────────────────────────────────────────
    // [SetUp] runs BEFORE every test — creates fresh mocks & the SUT (System
    // Under Test).  Equivalent to a constructor approach in xUnit.
    // ─────────────────────────────────────────────────────────────────────────
    [SetUp]
    public void SetUp()
    {
        _repoMock   = new Mock<IPostRepository>();
        _mapperMock = new Mock<IMapper>();

        // PostService receives its dependencies through constructor injection.
        _service = new PostService(_repoMock.Object, _mapperMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetAllPostsAsync
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetAllPostsAsync_ReturnsAllPosts()
    {
        // ── Arrange ─────────────────────────────────────────────────────────
        // .Setup() tells Moq what the mock should return when the method is called.
        var posts = new List<Post> { MakePost(1), MakePost(2) };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(posts);

        var dtos = posts.Select(MakeResponse).ToList();
        _mapperMock.Setup(m => m.Map<List<PostResponseDTO>>(posts)).Returns(dtos);

        // ── Act ──────────────────────────────────────────────────────────────
        var result = await _service.GetAllPostsAsync();

        // ── Assert ───────────────────────────────────────────────────────────
        // NUnit: Assert.That(...) with constraint model (preferred over Assert.AreEqual).
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetPostByIdAsync — post found
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetPostByIdAsync_WhenFound_ReturnsDto()
    {
        // Arrange
        var post = MakePost(5);
        var dto  = MakeResponse(post);

        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(post);
        _mapperMock.Setup(m => m.Map<PostResponseDTO>(post)).Returns(dto);

        // Act
        var result = await _service.GetPostByIdAsync(5);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(5));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetPostByIdAsync — post NOT found → returns null
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetPostByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange — returns null (post does not exist)
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Post?)null);

        // Act
        var result = await _service.GetPostByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: GetMyPostsAsync
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task GetMyPostsAsync_ReturnOnlyUserPosts()
    {
        // Arrange
        var userId = 10;
        var posts  = new List<Post> { MakePost(1, userId), MakePost(2, userId) };
        var dtos   = posts.Select(MakeResponse).ToList();

        _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(posts);
        _mapperMock.Setup(m => m.Map<List<PostResponseDTO>>(posts)).Returns(dtos);

        // Act
        var result = await _service.GetMyPostsAsync(userId);

        // Assert
        Assert.That(result.All(p => p.UserId == userId), Is.True);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: CreatePostAsync — verifies post is created and DTO is returned
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task CreatePostAsync_CreatesAndReturnsPost()
    {
        // Arrange
        var userId     = 10;
        var createDto  = new CreatePostDTO { Title = "Hello", Content = "World content here" };
        var post       = MakePost(1, userId);
        var responseDto = MakeResponse(post);

        // mapper: CreatePostDTO → Post model
        _mapperMock.Setup(m => m.Map<Post>(createDto)).Returns(post);
        _repoMock.Setup(r => r.CreateAsync(post)).ReturnsAsync(post);
        // mapper: Post model → PostResponseDTO
        _mapperMock.Setup(m => m.Map<PostResponseDTO>(post)).Returns(responseDto);

        // Act
        var result = await _service.CreatePostAsync(userId, createDto);

        // Assert
        Assert.That(result.Id,     Is.EqualTo(1));
        Assert.That(result.UserId, Is.EqualTo(userId));

        // .Verify() confirms the mock method was actually called exactly once.
        _repoMock.Verify(r => r.CreateAsync(post), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: UpdatePostAsync — owner updates their post → true
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task UpdatePostAsync_WhenOwner_ReturnsTrue()
    {
        // Arrange
        var post      = MakePost(1, userId: 10);
        var updateDto = new CreatePostDTO { Title = "Updated", Content = "Updated content here" };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

        // Act
        var result = await _service.UpdatePostAsync(1, userId: 10, updateDto);

        // Assert
        Assert.That(result, Is.True);
        _repoMock.Verify(r => r.UpdateAsync(post), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: UpdatePostAsync — wrong user tries to update → false
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task UpdatePostAsync_WhenNotOwner_ReturnsFalse()
    {
        // Arrange
        var post      = MakePost(1, userId: 10);
        var updateDto = new CreatePostDTO { Title = "Hack", Content = "Hack content change value" };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

        // Act — userId 99 is NOT the owner (owner is 10)
        var result = await _service.UpdatePostAsync(1, userId: 99, updateDto);

        // Assert
        Assert.That(result, Is.False);
        // UpdateAsync should NEVER have been called
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DeletePostAsync — owner deletes → true
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task DeletePostAsync_WhenOwner_ReturnsTrue()
    {
        var post = MakePost(1, userId: 10);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

        var result = await _service.DeletePostAsync(1, userId: 10, role: "User");

        Assert.That(result, Is.True);
        _repoMock.Verify(r => r.DeleteAsync(post), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DeletePostAsync — Admin can delete any post
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task DeletePostAsync_WhenAdmin_ReturnsTrue()
    {
        var post = MakePost(1, userId: 10);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

        // Admin (userId = 99) deletes post owned by userId = 10
        var result = await _service.DeletePostAsync(1, userId: 99, role: "Admin");

        Assert.That(result, Is.True);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST: DeletePostAsync — post does not exist → false
    // ─────────────────────────────────────────────────────────────────────────
    [Test]
    public async Task DeletePostAsync_WhenNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Post?)null);

        var result = await _service.DeletePostAsync(99, userId: 10, role: "User");

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // [TearDown] runs AFTER every test — useful for cleanup.
    // ─────────────────────────────────────────────────────────────────────────
    [TearDown]
    public void TearDown()
    {
        // Nothing to clean up here — mocks are re-created in [SetUp].
        // Use [TearDown] for disposing DB connections, temp files, etc.
    }
}

