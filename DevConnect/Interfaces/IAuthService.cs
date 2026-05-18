using DevConnect.DTOs;
using DevConnect.Models;

namespace DevConnect.Interfaces
{
    public interface IAuthService
    {
        // Generate JWT token for a user
        string GenerateToken(User user);

        // Find existing user by provider ID OR create new user from OIDC data
        Task<User> FindOrCreateOidcUserAsync(OidcUserDTO dto);
    }
}