using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Interfaces;
using SmartCafe.Models;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IJwtService jwtService,
        UserManager<IdentityUser> userManager,
        SmartCafeDbContext context
        ) : ControllerBase
    {
       
        [HttpPost("login")]
        [EndpointSummary("Login")]
        public async Task<IActionResult> Login(LoginInfo info)
        {
            // 1. Validate credentials
           
            var user = await userManager.FindByNameAsync(info.UserName);
           
            if (user == null) return Unauthorized();
            var userInfo = await context.UserInfos
        .FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (userInfo == null || userInfo.Status ==false)
            {
                return Unauthorized(new DefaultResponseModel
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "Account is inactive or disabled."
                });
            }

            bool checkPassword = await userManager.CheckPasswordAsync(user, info.Password);
            if (!checkPassword) return Unauthorized();

            // 2. Generate access token (short-lived: 15 min)
            string accessToken = await jwtService.GenerateToken(user);

            // 3. Generate refresh token (long-lived: 7 days, random string)
            string refreshTokenValue = jwtService.GenerateRefreshToken();

            // 4. Revoke any OLD refresh tokens for this user (one session at a time)
            var oldTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();
            foreach (var old in oldTokens)
                old.IsRevoked = true;

            // 5. Save new refresh token to database
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),   // lives for 7 days
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Login successfully",
                Data = new
                {
                    accessToken = accessToken,        // send to: Authorization header
                    refreshToken = refreshTokenValue, // send to: localStorage / secure cookie
                    expiresIn = 15 * 60              // 900 seconds (15 min)
                }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/Auth/refresh
        // Client sends expired accessToken + valid refreshToken
        // Returns a NEW accessToken + NEW refreshToken (rotation)
        // ─────────────────────────────────────────────────────────────
        [HttpPost("refresh")]
        [EndpointSummary("Refresh Access Token")]
        public async Task<IActionResult> Refresh(RequestDtos.RefreshTokenRequest request)
        {
            // 1. Extract userId from the EXPIRED access token
            //    (ValidateLifetime = false so we can still read it after expiry)
            var userId = jwtService.GetUserIdFromExpiredToken(request.AccessToken);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new DefaultResponseModel
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid access token"
                });
            }

            // 2. Find the refresh token in DB and validate it
            var storedToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.Token == request.RefreshToken &&
                    rt.UserId == userId &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow);   // not expired

            if (storedToken == null)
            {
                return Unauthorized(new DefaultResponseModel
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "Refresh token is invalid or expired. Please login again."
                });
            }

            // 3. Find the user
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new DefaultResponseModel
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "User not found"
                });
            }

            // 4. Revoke the OLD refresh token (Refresh Token Rotation)
            //    Each refresh produces a brand new refresh token — old one is dead
            storedToken.IsRevoked = true;

            // 5. Generate new pair
            string newAccessToken = await jwtService.GenerateToken(user);
            string newRefreshTokenValue = jwtService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await context.RefreshTokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();

            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Token refreshed successfully",
                Data = new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshTokenValue,
                    expiresIn = 15 * 60
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        [EndpointSummary("Logout — revoke refresh token")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var token = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (token != null)
            {
                token.IsRevoked = true;
                await context.SaveChangesAsync();
            }

            return Ok(new DefaultResponseModel
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Logged out successfully"
            });
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/Auth/profile
        // ─────────────────────────────────────────────────────────────
        [HttpGet("profile")]
        [Authorize]
        [EndpointSummary("Get user profile")]
        public async Task<IActionResult> UserProfile()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Data = null
                });
            }
            var identityUser = await userManager.FindByNameAsync(userName);
            if (identityUser == null)
            {
                return Unauthorized(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Data = null
                });
            }
            var role = await userManager.GetRolesAsync(identityUser);
            var userRole = role.FirstOrDefault() ?? "User";
            var userInfo = await context.UserInfos.FirstOrDefaultAsync(u => u.UserId == identityUser.Id);
            var profileImage = userInfo?.ProfileImage ?? "";

            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "User Profile Retrieved Successfully",
                Data = new
                {
                    userId = identityUser.Id,
                    userName = identityUser.UserName,
                    email = identityUser.Email,
                    phoneNumber = identityUser.PhoneNumber,
                    profileImage = profileImage,
                    role = userRole
                }
            });
        }
    }
}
