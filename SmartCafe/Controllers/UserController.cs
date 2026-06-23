using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Interfaces;
using SmartCafe.Models;
using SmartCafe.Services;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(SmartCafeDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConvertion convertion,
        IFileService FileService) :ControllerBase
    {
        [HttpGet]
        [EndpointSummary("GetUserInformation")]
        public async Task<IActionResult> GetInfo()
        {
            var userInfos = await context.UserInfos
                .Where(u => u.Status == true)
                .ToListAsync();


            var identityUsers = await userManager.Users.ToListAsync();

            var userData = (from info in userInfos
                            join identityUser in identityUsers on info.UserId equals identityUser.Id
                            select new
                            {
                                userId = info.UserId,
                                userName = info.UserName,
                                email = identityUser.Email,
                                phoneNumber=identityUser.PhoneNumber,
                                role = info.Role,
                                status = info.Status,
                                joinDate = info.JoinDate,
                                profileImage = info.ProfileImage
                            }).ToList();

            if (userData.Count != 0)
            {

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "User Info",
                    Data = userData
                });
            }
            else
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "User doesn't exist",
                    Data = null
                });
            }
        }


        [HttpPost]
        [EndpointSummary("Create User")]
        public async Task<IActionResult> CreateUser(UserInfoModel model)
        {
            if (string.IsNullOrEmpty(model.Role))
            {
                return BadRequest("Role Name is required");
            }
            //string to enum
            if (!Enum.TryParse<RoleStatus>(model.Role, true, out var parsedRole))
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Role! Only 'Admin' or 'KitchenStaff' are allowed.",
                    Data = null
                });
            }
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid Input");
            }
            //enum to string
            string finalizedRoleName = parsedRole.ToString();
            if (!await roleManager.RoleExistsAsync(finalizedRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(finalizedRoleName));
            }
            IdentityUser user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = !string.IsNullOrEmpty(model.Email),
                PhoneNumber=model.PhoneNumber,
                PhoneNumberConfirmed=!string.IsNullOrEmpty(model.PhoneNumber),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            IdentityResult result =
                await userManager.CreateAsync(user, model.Password);//save to aspNetUser

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            IdentityResult setRole =
                await userManager.AddToRoleAsync(user, finalizedRoleName);

            if (!setRole.Succeeded)
            {
                return BadRequest(setRole.Errors);
            }
            UserInfo userInfo = new()
            {
                UserId = user.Id,
                UserName = model.UserName,
                Role = finalizedRoleName,
                JoinDate = DateOnly.FromDateTime(DateTime.Now),
                //ProfileImage=model.ProfileImage,
                Status = true
            };
            context.UserInfos.Add(userInfo);
            bool isSaved=await context.SaveChangesAsync()>0;

            if (isSaved)
            {
                if (!string.IsNullOrEmpty(model.ProfileImage))
                {
                    string base64Data = model.ProfileImage;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    string extension = convertion.GetFileExtension(base64Data);
                    string fileName = $"{userInfo.UserId}{extension}";

                    userInfo.ProfileImage = $"images/user/{fileName}";

                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    using (MemoryStream memoryStream = new(imageBytes))
                    {
                        memoryStream.Position = 0;
                        IFormFile formFile = new FormFile(memoryStream, 0, imageBytes.Length, "fileUpload", fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = $"image/{extension.Replace(".", "")}"
                        };

                        string fileServiceError = string.Empty;
                        bool imageSavedResult = await FileService.WriteImageDocker(formFile, $"{userInfo.UserId}", "user");
                        if (!imageSavedResult)
                        {
                            return BadRequest(new DefaultResponseModel()
                            {
                                Success = false,
                                Statuscode = StatusCodes.Status400BadRequest,
                                Message = $"Failed to save category image. Error: {fileServiceError}",
                                Data = null
                            });
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }


                return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "success",
                Data = userInfo
            });
        }

        [HttpPut("{id}")]
        [EndpointSummary("Update User")]
        public async Task<IActionResult> UpdateUserInfo(string id,UserInfoModel model)
        {
            if(id != model.UserId)
            {
                return BadRequest("User id is not matched");
            }
            var userInfo = await context.UserInfos.FirstOrDefaultAsync(u => u.UserId == id);
            if(userInfo== null)
            {
                return NotFound("User Not found");
            }
            var identityUser = await userManager.FindByIdAsync(id);
            if(identityUser == null)
            {
                return NotFound("identity user Not found");
            }
            identityUser.UserName = model.UserName;
            identityUser.Email = model.Email;
            var updateResult = await userManager.UpdateAsync(identityUser);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }
            if (!string.IsNullOrWhiteSpace(model.Password))//check password
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(identityUser);
                var passwordResult = await userManager.ResetPasswordAsync(
                    identityUser, token, model.Password);

                if (!passwordResult.Succeeded)
                    return BadRequest(passwordResult.Errors);
            }

            var currentRoles = await userManager.GetRolesAsync(identityUser);
            await userManager.RemoveFromRolesAsync(identityUser, currentRoles);
            await userManager.AddToRoleAsync(identityUser, model.Role);

            userInfo.UserName = model.UserName;
            userInfo.Status = model.Status;
            userInfo.Role = model.Role;
            userInfo.JoinDate = model.JoinDate;

            //userInfo.ProfileImage = model.ProfileImage;

            if (!string.IsNullOrEmpty(model.ProfileImage) &&
               !model.ProfileImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string extension = convertion.GetFileExtension(model.ProfileImage);
                string fileName = $"{userInfo.UserId}{extension}";

                byte[] imageBytes = Convert.FromBase64String(model.ProfileImage);
                using MemoryStream memoryStream = new(imageBytes);
                IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "fileUpload", fileName);

                // Save image
                _ = await FileService.WriteImageDocker(formFile, userInfo.UserId.ToString(), "user");

                // Set new image path
                userInfo.ProfileImage = $"images/user/{fileName}";
            }
            else
            {
                // retain existing image
                userInfo.ProfileImage = userInfo.ProfileImage;
            }

            bool isSaved = await context.SaveChangesAsync() > 0;

            if (isSaved)
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Category updated successfully",
                    Data = userInfo
                });
            }
            else
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Category updated failed",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Delete User")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var userInfo = await context.UserInfos.FindAsync(id);
            if (userInfo == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data does not exist",
                    Data = null
                });
            }
            else
            {
                userInfo.Status = false;
                context.UserInfos.Update(userInfo);
                bool isSaved = await context.SaveChangesAsync() > 0;
                if (isSaved)
                {
                    return Ok(new DefaultResponseModel()
                    {
                        Success = true,
                        Statuscode = StatusCodes.Status200OK,
                        Message = "Deleted successfully",
                        Data = null
                    });
                }
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Delete failed",
                    Data = null
                });
            }
        }
    }
}
