using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartCafe.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartCafe.Services
{
    public class JwtService(
        IConfiguration config,
        UserManager<IdentityUser> userManager) : IJwtService
    {
        // ─────────────────────────────────────────────────────
        // 1. Generate SHORT-LIVED Access Token (15 minutes)
        // ─────────────────────────────────────────────────────
        public async Task<string> GenerateToken(IdentityUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
            ];

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),   // ← Short-lived: 15 min
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─────────────────────────────────────────────────────
        // 2. Generate LONG-LIVED Refresh Token (random string)
        //    This is just a secure random value — NOT a JWT
        // ─────────────────────────────────────────────────────
        public string GenerateRefreshToken()
        {
            // Creates a cryptographically secure random 64-byte value
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes); // e.g. "dGhpcyBpcy..."
        }

        // ─────────────────────────────────────────────────────
        // 3. Extract UserId from an EXPIRED access token
        //    Used in the /refresh endpoint to know WHICH user
        //    is requesting a new access token
        // ─────────────────────────────────────────────────────
        public string? GetUserIdFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,   // ← Important! We IGNORE expiry here
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                token, tokenValidationParameters, out SecurityToken securityToken);

            // Make sure it really was a JWT signed with HS256
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
