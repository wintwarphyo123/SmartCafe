using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCafe.Interfaces;
using SmartCafe.Models;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IJwtService jwtService,
        UserManager<IdentityUser> userManager
        ):ControllerBase
    {
        [HttpPost("login")]
        [EndpointSummary("Login")]
        public async Task<IActionResult> Login(LoginInfo info)
        {
            var user = await userManager.FindByNameAsync(info.UserName);
            if (user == null)
            {
                return Unauthorized();
            }
            bool checkPassword = await userManager.CheckPasswordAsync(user, info.Password);
            if (!checkPassword)
            {
                return Unauthorized();
            }
            string token = await jwtService.GenerateToken(user);
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "login successfully",
                Data = token
            });
        }

        [HttpGet("profile")]
        [Authorize]
        [EndpointSummary("get user profile")]
        public async Task<IActionResult> UserProfile()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status401Unauthorized,
                    Message="Unauthorized",
                    Data=null
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
                    role = userRole
                }
            });
        }
    }
}
