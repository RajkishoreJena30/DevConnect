using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Interfaces;
using DevConnect.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevConnect.Services
{
    public class AuthService : IAuthService
    {
        private readonly DevConnectDbContext _context;
        private readonly IConfiguration _config;

        // Both injected via DI
        public AuthService(DevConnectDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Generate JWT token — moved from AuthController
        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("provider", user.Provider ?? "Local")  // track login method
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

        // Find user by ProviderUserId OR create new user from OIDC data
        public async Task<User> FindOrCreateOidcUserAsync(OidcUserDTO dto)
        {
            // Check if user already logged in via this provider before
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Provider == dto.Provider &&
                    u.ProviderUserId == dto.ProviderUserId);

            if (existingUser != null)
                return existingUser; // returning user — just generate token

            // Check if email already exists (registered locally before)
            var emailUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (emailUser != null)
            {
                // Link OIDC provider to existing local account
                emailUser.Provider = dto.Provider;
                emailUser.ProviderUserId = dto.ProviderUserId;
                await _context.SaveChangesAsync();
                return emailUser;
            }

            // Brand new user — create from OIDC data
            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Provider = dto.Provider,
                ProviderUserId = dto.ProviderUserId,
                Role = "User",
                PasswordHash = string.Empty  // no password for OIDC users (column is NOT NULL)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
    }
}