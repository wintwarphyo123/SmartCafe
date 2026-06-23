using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartCafe.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCafe.Services
{
    public class JwtService(
        IConfiguration config,
        UserManager<IdentityUser> userManager) : IJwtService
    {
        public async Task<String> GenerateToken(IdentityUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            List<Claim> claims = [
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),
                    new Claim(
                        ClaimTypes.Name,
                        user.UserName!),
                        new Claim(
                            ClaimTypes.Email,
                            user.Email??""),
                ];
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
