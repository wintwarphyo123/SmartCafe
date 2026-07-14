using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Hubs;
using SmartCafe.Interfaces;
using SmartCafe.Models;
using SmartCafe.Services;
namespace SmartCafe.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(
        SmartCafeDbContext context,
        IConvertion convertion,
        IFileService FileService,
        IHubContext<NotificationHubs> hubContext
        ) :ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [EndpointSummary("Get Category Data")]
        public async Task<IActionResult> GetCategories()
        {
            var categoryList=await context.Categories
                .Where(c=>c.DeletedAt==null)
                .Select(c=> new ResponseDtos.AllCategories()
                {
                    Id=c.CategoryId,
                    CategoryName=c.CategoryName,
                    CategoryImage=c.CategoryImage,
                    isActive=c.IsActive
                }).ToListAsync();   
            if(categoryList.Any())//test date exist or not
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = categoryList
                });
            }
            else
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode= StatusCodes.Status404NotFound,
                    Message="No Data exist",
                    Data=null
                   
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Deleted")]
        [EndpointSummary("Get Deleted Data")]
        public async Task<IActionResult> GetDeletedData()
        {
            var categoryData = await context.Categories
                .Where(c => c.DeletedAt != null)
                .Select(c => new ResponseDtos.AllCategories()
                {
                    Id = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryImage = c.CategoryImage,
                    isActive = c.IsActive
                })
                .ToListAsync();
            if(categoryData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = categoryData
                });
            }
            else
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "No Data exist",
                    Data = null
                    
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        [EndpointSummary("Get category by id")]
        public async Task<IActionResult> FindCategory(int id)
        {
            var categoryData = await context.Categories.FirstOrDefaultAsync(c=>c.CategoryId==id && c.DeletedAt==null);
            if (categoryData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Category Data not found",
                    Data = null
                });
            }
            else
            {

                var categoryDto=new ResponseDtos.AllCategories()
                {
                    Id=categoryData.CategoryId,
                    CategoryName = categoryData.CategoryName,
                    CategoryImage= categoryData.CategoryImage
                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = categoryDto
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [EndpointSummary("Create new Category")]
        public async Task<IActionResult> CreateCategory(RequestDtos.RequestCategory category)
        {
            // 1. Create and add the new entity instance
            Category categoryData = new()
            {
                CategoryName = category.CategoryName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
              
            context.Categories.Add(categoryData);
            bool isSaved = await context.SaveChangesAsync() > 0;
            if (isSaved)
            {
                if (!string.IsNullOrEmpty(category.CategoryImage))
                {
                    string base64Data = category.CategoryImage;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    string extension = convertion.GetFileExtension(base64Data);
                    string fileName = $"{categoryData.CategoryId}{extension}";

                    categoryData.CategoryImage = $"images/category/{fileName}";

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
                        bool imageSavedResult = await FileService.WriteImageDocker(formFile, $"{categoryData.CategoryId}", "category");
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

                var responseData = new ResponseDtos.AllCategories()
                {
                    Id = categoryData.CategoryId,
                    CategoryName = categoryData.CategoryName,
                    CategoryImage = categoryData.CategoryImage

                };

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Category created successfully",
                    Data = responseData // Returning the newly created data object is usually safer
                });
            }

            return BadRequest(new DefaultResponseModel()
            {
                Success = false,
                Statuscode = StatusCodes.Status400BadRequest,
                Message = "Category creation failed",
                Data = null
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [EndpointSummary("Update Category")]
        public async Task<IActionResult> UpdateCategory(int id, RequestDtos.RequestCategory categorydto)
        {
            Category? existingCategory=await context.Categories.FirstOrDefaultAsync(c=>c.CategoryId== id);
            if(existingCategory==null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data doesn't exist",
                    Data = null

                });
            }
            existingCategory.CategoryName = categorydto.CategoryName;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            existingCategory.IsActive = true;

            if (!string.IsNullOrEmpty(categorydto.CategoryImage) &&
               !categorydto.CategoryImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string extension = convertion.GetFileExtension(categorydto.CategoryImage);
                string fileName = $"{existingCategory.CategoryId}{extension}";

                byte[] imageBytes = Convert.FromBase64String(categorydto.CategoryImage);
                using MemoryStream memoryStream = new(imageBytes);
                IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "fileUpload", fileName);

                // Save image
                _ = await FileService.WriteImageDocker(formFile, existingCategory.CategoryId.ToString(), "category");

                // Set new image path
                existingCategory.CategoryImage = $"images/category/{fileName}";
            }
            else
            {
                // retain existing image
                existingCategory.CategoryImage = existingCategory.CategoryImage;
            }

            //context.Attach(categorydto).State = EntityState.Modified;
            bool isSaved= await context.SaveChangesAsync() > 0;

            if (isSaved)
            {
                var responseData = new ResponseDtos.AllCategories()
                {
                    Id = existingCategory.CategoryId,
                    CategoryName = existingCategory.CategoryName,
                    CategoryImage = existingCategory.CategoryImage

                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Category updated successfully",
                    Data = responseData
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/update-status")]
        [EndpointSummary("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var categoryData = await context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (categoryData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data is missed",
                    Data = null
                });
            }
            else
            {
                categoryData.IsActive = !categoryData.IsActive;
                context.Categories.Update(categoryData);
                await context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("ReceiveCategoryUpdate", new
                {
                    categoryId = categoryData.CategoryId,
                    isActive = categoryData.IsActive,
                    action = "status_change"
                });
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Status change successfully",
                    Data = categoryData
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Restore")]
        [EndpointSummary("Restore Deleted Data")]
        public async Task<IActionResult> RestoreData(int id)
        {
            var catData=await context.Categories.FirstOrDefaultAsync(c=>c.CategoryId == id);
            if(catData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data is missed",
                    Data = null
                });
            }
            bool isNameConflict = await context.Categories.AnyAsync(c => c.CategoryName == catData.CategoryName && c.DeletedAt == null);
            if (isNameConflict)
            {
                return Conflict(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status409Conflict,
                    Message = $"An active category named '{catData.CategoryName}' already exists.",
                    Data = null
                });
            }
            
                catData.DeletedAt = null;
                context.Categories.Update(catData);
                await context.SaveChangesAsync();
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Status change successfully",
                    Data = catData
                });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [EndpointSummary("Delete Category By id")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var categoryData=await context.Categories.FindAsync(id);
            if (categoryData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data doesn't exist",
                    Data = null

                });
            }

            categoryData.DeletedAt= DateTime.UtcNow;
            context.Categories.Update(categoryData) ;
            return await context.SaveChangesAsync() > 0
                ? StatusCode(StatusCodes.Status201Created, new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Category deleted successfully",
                    Data = categoryData
                })
                : BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Category deleted failed",
                    Data = null
                });

        }
    }
}
