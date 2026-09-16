using Microsoft.AspNetCore.Identity;

namespace SmartCafe.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateToken(IdentityUser user);

        // Generates a secure random refresh token string
        string GenerateRefreshToken();

        // Extracts the userId from an EXPIRED access token (used during refresh)
        string? GetUserIdFromExpiredToken(string token);
    }
}
