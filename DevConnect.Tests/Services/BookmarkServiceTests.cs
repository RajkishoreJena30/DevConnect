using AutoMapper;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using DevConnect.Services;
using Moq;

namespace DevConnect.Tests.Services;

[TestFixture]
public class BookmarkServiceTests
{
    private Mock<IBookmarkRepository> _repoMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private BookmarkService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IBookmarkRepository>();
        _mapperMock = new Mock<IMapper>();
        _service = new BookmarkService(_repoMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Toggle_WhenPostMissing_ReturnsNull()
    {
        _repoMock.Setup(r => r.PostExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Toggle_WhenNotYetBookmarked_AddsAndReturnsTrue()
    {
        _repoMock.Setup(r => r.PostExistsAsync(5)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetAsync(1, 5)).ReturnsAsync((Bookmark?)null);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 5);

        Assert.That(result!.Bookmarked, Is.True);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Bookmark>()), Times.Once);
    }

    [Test]
    public async Task Toggle_WhenAlreadyBookmarked_RemovesAndReturnsFalse()
    {
        var existing = new Bookmark { Id = 3, UserId = 1, PostId = 5 };
        _repoMock.Setup(r => r.PostExistsAsync(5)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetAsync(1, 5)).ReturnsAsync(existing);

        var result = await _service.ToggleBookmarkAsync(userId: 1, postId: 5);

        Assert.That(result!.Bookmarked, Is.False);
        _repoMock.Verify(r => r.RemoveAsync(existing), Times.Once);
    }
}
