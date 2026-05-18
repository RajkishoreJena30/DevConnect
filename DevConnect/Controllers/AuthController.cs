using BC = BCrypt.Net.BCrypt;
using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DevConnect.Interfaces;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;

namespace DevConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DevConnectDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;  // ← add this

        public AuthController(DevConnectDbContext context, IConfiguration config, IMapper mapper, IAuthService authService)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(user => user.Email == dto.Email))
                return BadRequest("Email already exists.");

         // Before — manual mapping:
        // var user = new User { Name = dto.Name, Email = dto.Email, ... }

        // After — AutoMapper:
        var user = _mapper.Map<User>(dto);              // RegisterDTO → User
        user.PasswordHash = BC.HashPassword(dto.Password); // set manually (ignored in map)

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDTO
            {
                Token = _authService.GenerateToken(user),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return Ok(new AuthResponseDTO
            {
                Token = _authService.GenerateToken(user),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }


        // GET: api/auth/google
        // Redirects browser to Google login page
        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))  // where Google sends user back
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // GET: api/auth/google/callback
        // Google redirects here after successful login
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Read what Google sent back
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Unauthorized("Google login failed.");

            // Extract claims from Google response
            var claims = result.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null || googleId == null)
                return BadRequest("Could not retrieve user info from Google.");

            // Find existing user OR create new user in our DB
            var user = await _authService.FindOrCreateOidcUserAsync(new OidcUserDTO
            {
                Email = email,
                Name = name ?? email,
                Provider = "Google",
                ProviderUserId = googleId
            });

            // Return our own JWT token (same as local login)
            return Ok(new AuthResponseDTO
            {
                Token = _authService.GenerateToken(user),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        // GET: api/auth/github
        [HttpGet("github")]
        public IActionResult GitHubLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GitHubCallback))
            };
            return Challenge(props, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        // GET: api/auth/github/callback
        [HttpGet("github/callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            var result = await HttpContext.AuthenticateAsync(
                GitHubAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return Unauthorized("GitHub login failed.");

            var claims = result.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var githubId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null || githubId == null)
                return BadRequest("Could not retrieve user info from GitHub.");

            var user = await _authService.FindOrCreateOidcUserAsync(new OidcUserDTO
            {
                Email = email,
                Name = name ?? email,
                Provider = "GitHub",
                ProviderUserId = githubId
            });

            return Ok(new AuthResponseDTO
            {
                Token = _authService.GenerateToken(user),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }


        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                    int.Parse(_config["JwtSettings:ExpiryInDays"]!)),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}