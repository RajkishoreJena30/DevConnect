using DevConnect.DTOs;
using DevConnect.Validators;
using FluentValidation.TestHelper;

namespace DevConnect.Tests.Validators;

// ─────────────────────────────────────────────────────────────────────────────
// Validator tests use FluentValidation's built-in TestHelper extension methods:
//   - ShouldHaveValidationErrorFor(x => x.Field) — expects a rule to fail
//   - ShouldNotHaveValidationErrorFor(x => x.Field) — expects a rule to pass
// No mocking needed — validators are pure logic with no dependencies.
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class AuthValidatorTests
{
    private RegisterValidator _registerValidator = null!;
    private LoginValidator _loginValidator = null!;

    [SetUp]
    public void SetUp()
    {
        _registerValidator = new RegisterValidator();
        _loginValidator    = new LoginValidator();
    }

    // ── RegisterValidator ────────────────────────────────────────────────────

    [Test]
    public void Register_EmptyName_ShouldFail()
    {
        var model = new RegisterDTO { Name = "", Email = "a@b.com", Password = "Valid@1234" };
        var result = _registerValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Register_ShortName_ShouldFail()
    {
        var model = new RegisterDTO { Name = "A", Email = "a@b.com", Password = "Valid@1234" };
        var result = _registerValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Register_InvalidEmail_ShouldFail()
    {
        var model = new RegisterDTO { Name = "Alice", Email = "not-an-email", Password = "Valid@1234" };
        var result = _registerValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── Password complexity rules ────────────────────────────────────────────

    // NUnit [TestCase] = parameterised tests — one test method, multiple inputs.
    // Equivalent to xUnit's [Theory] + [InlineData].
    [TestCase("short")]          // too short
    [TestCase("alllowercase1!")] // no uppercase
    [TestCase("ALLUPPERCASE1!")] // no lowercase
    [TestCase("NoSpecial1234")]  // no special char
    [TestCase("NoNumber@Abc!")]  // no digit
    public void Register_WeakPassword_ShouldFail(string password)
    {
        var model = new RegisterDTO { Name = "Alice", Email = "a@b.com", Password = password };
        var result = _registerValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Register_ValidData_ShouldPass()
    {
        var model = new RegisterDTO
        {
            Name     = "Alice",
            Email    = "alice@example.com",
            Password = "StrongPass@1"
        };
        var result = _registerValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── LoginValidator ───────────────────────────────────────────────────────

    [Test]
    public void Login_EmptyEmail_ShouldFail()
    {
        var model = new LoginDTO { Email = "", Password = "anything" };
        var result = _loginValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Login_ValidData_ShouldPass()
    {
        var model = new LoginDTO { Email = "user@test.com", Password = "Pass@1234" };
        var result = _loginValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Post & Comment Validator Tests
// ─────────────────────────────────────────────────────────────────────────────
[TestFixture]
public class PostValidatorTests
{
    private CreatePostValidator    _postValidator    = null!;
    private CreateCommentValidator _commentValidator = null!;

    [SetUp]
    public void SetUp()
    {
        _postValidator    = new CreatePostValidator();
        _commentValidator = new CreateCommentValidator();
    }

    // ── Post rules ───────────────────────────────────────────────────────────

    [Test]
    public void Post_EmptyTitle_ShouldFail()
    {
        var model = new CreatePostDTO { Title = "", Content = "Valid content here for test" };
        _postValidator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Post_TooLongTitle_ShouldFail()
    {
        var model = new CreatePostDTO
        {
            Title   = new string('A', 101), // 101 chars > max 100
            Content = "Valid content body text here"
        };
        _postValidator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Post_ShortContent_ShouldFail()
    {
        var model = new CreatePostDTO { Title = "Good Title", Content = "Short" }; // < 10 chars
        _postValidator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Post_ValidData_ShouldPass()
    {
        var model = new CreatePostDTO
        {
            Title   = "My First Post",
            Content = "This is a valid post content that is long enough."
        };
        _postValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    // ── Comment rules ────────────────────────────────────────────────────────

    [Test]
    public void Comment_EmptyContent_ShouldFail()
    {
        var model = new CreateCommentDTO { Content = "" };
        _commentValidator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Comment_TooLongContent_ShouldFail()
    {
        var model = new CreateCommentDTO { Content = new string('X', 501) }; // > 500
        _commentValidator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Comment_ValidContent_ShouldPass()
    {
        var model = new CreateCommentDTO { Content = "Great post!" };
        _commentValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }
}
