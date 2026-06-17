using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Interfaces;
using SmartCafe.Models;
using static SmartCafe.DTOs.ResponseDtos;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController(SmartCafeDbContext context,
        IConvertion convertion,
        IFileService FileService) :ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Get all Menu Data")]
        public async Task<IActionResult> GetMenuData()
        {
            var menuList = await context.Menus
                .Where(m=>m.DeletedAt==null && m.IsAvailable==true)
                .Select(m => new ResponseDtos.AllMenu()
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage= m.MenuImage,
                    Description=m.Description,
                    Price =m.Price,
                    Is_available=m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName=m.Category !=null ? m.Category.CategoryName:"No Category"
                    
                }).ToListAsync();
            if(!menuList.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status404NotFound,
                    Message="Menu Data not found",
                    Data=null
                });
            }
            else
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Menu data exist",
                    Data = menuList
                });
            }
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get Menu By Id")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            var menuData=await context.Menus.FindAsync(id);
            
            if (menuData == null || menuData.DeletedAt!=null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data not found",
                    Data = null

                });
            }
            else
            {
                var menuDto = new ResponseDtos.AllMenu()
                {
                    Id = menuData.MenuId,
                    MenuName = menuData.MenuName,
                    Description=menuData.Description,
                    Price = menuData.Price,
                    CategoryId = menuData.CategoryId,
                    Is_available = menuData.IsAvailable
                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = menuDto
                });
            }
        }

        [HttpGet("Category/{categoryId}")]
        [EndpointSummary("Get Menu By CategoryId")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            IQueryable<Menu> menuInfo=from m in context.Menus join c in context.Categories
                                      on m.CategoryId equals c.CategoryId
                                      where m.CategoryId == categoryId
                                      select m;
            var menuList = await menuInfo.ToListAsync();
            if(menuList==null || !menuInfo.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "No menu found for the specified State Id",
                    Data = null
                });
            }
            else
            {
                
                return Ok(new DefaultResponseModel()
                {
                    Success= true,
                    Statuscode= StatusCodes.Status200OK,
                    Message="Menu Data exist",
                    Data= menuList
                });
            }
        }

        [HttpGet("search/{MenuName}")]
        [EndpointSummary("Get Menu by name")]
        public async Task<IActionResult> GetByName(string MenuName)
        {
            if (string.IsNullOrEmpty(MenuName))
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Search keyword cannot be empty",
                    Data = null
                });
            }
            var menuData = await context.Menus
                .Where(m => m.MenuName != null && m.MenuName.Contains(MenuName))
                .ToListAsync();

            if (menuData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "menu exist",
                    Data = menuData
                });
            }
            else
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "menu doesn't exist",
                    Data = null
                });
            }
        }

        [HttpGet("detail/{id}")]
        [EndpointSummary("Get Menu Detail")]
        public async Task<IActionResult> GetMenuDetail(int id)
        {
            // ၁။ Include နှင့် ThenInclude ကို သုံးပြီး Multi-level Data များကို ဆွဲထုတ်ခြင်း
            var menu = await context.Menus
                .Include(m => m.ProductOptionGroups) 
                    .ThenInclude(mog => mog.OptionGroup) 
                .Where(m => m.MenuId == id && m.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (menu == null)
            {
                return NotFound(new DefaultResponseModel { Success = false, Message = "Menu Data Not Found" });
            }

            var result = new MenuDetailResponseDto
            {
                MenuId = menu.MenuId,
                MenuName = menu.MenuName,
                Price = menu.Price,
                Description = menu.Description,

                OptionGroups = menu.ProductOptionGroups// .OrderBy(mog => mog)
                    .Select(mog => new OptionGroupDto
                    {
                        GroupId = mog.OptionGroup!.Id,
                        GroupName = mog.OptionGroup.GroupName,

                        OptionItems = context.OptionItems
                            .Where(oi => oi.OptionGroupId == mog.OptionGroupId)
                            .Select(oi => new OptionItemDto
                            {
                                ItemId = oi.Id,
                                ItemName = oi.ItemName,
                                ExtraPrice = oi.ExtraPrice
                            }).ToList()
                    }).ToList()
            };

            return Ok(new DefaultResponseModel { Success = true, Data = result });
        }

        

        [HttpPost]
        [EndpointSummary("Create new Menu")]
        public async Task<IActionResult> CreateMenu(RequestDtos.RequestMenu menuDto)
        {//description,categoryId,price,isAvailable,categoryId,
            Menu menuData = new()
            {
                MenuName = menuDto.MenuName,
                Description= menuDto.Description,
                CategoryId= menuDto.CategoryId,
                Price = menuDto.Price,
                IsAvailable=true,
                CreatedAt= DateTime.UtcNow,
            };
            context.Menus.Add(menuData);
            bool isSaved=await context.SaveChangesAsync()>0;
            if (isSaved)
            {
                if (!string.IsNullOrEmpty(menuDto.MenuImage))
                {
                    string base64Data = menuDto.MenuImage;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }

                    string extension = convertion.GetFileExtension(base64Data);
                    string fileName = $"{menuData.MenuId}{extension}";

                    menuData.MenuImage = $"images/menu/{fileName}";

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
                        bool imageSavedResult = await FileService.WriteImageDocker(formFile, $"{menuData.MenuId}", "menu");
                        if (!imageSavedResult)
                        {
                            return BadRequest(new DefaultResponseModel()
                            {
                                Success = false,
                                Statuscode = StatusCodes.Status400BadRequest,
                                Message = $"Failed to save menu image. Error: {fileServiceError}",
                                Data = null
                            });
                        }
                    }

                    await context.SaveChangesAsync();
                    //Id,menuName,menuImage,price,description,is_available,categoryId,cateogryName
                    var responseData = new ResponseDtos.AllMenu()
                    {
                        Id = menuData.MenuId,
                        MenuName = menuData.MenuName,
                        MenuImage = menuData.MenuImage,
                        Description = menuData.Description,
                        Price = menuData.Price,
                        Is_available = menuData.IsAvailable,
                        CategoryId = menuData.CategoryId
                    };

                    return Ok(new DefaultResponseModel()
                    {
                        Success = true,
                        Statuscode = StatusCodes.Status201Created,
                        Message = "Menu created successfully",
                        Data = responseData // Returning the newly created data object is usually safer
                    });
                }
            }

                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Menu creation failed",
                    Data = null
                });
            }
            
        

        //for MenuOptionGroup tale , join_table
        [HttpPost("link-option-group")]
        [EndpointSummary("Link an Option Group to a Menu Product")]
        public async Task<IActionResult> LinkOptionGroup(RequestDtos.RequestMenuOptionGroupDto dto)
        {
            // 1. Check if the Product (Menu item) exists and is not soft-deleted
            bool hasMenu = await context.Menus.AnyAsync(m => m.MenuId == dto.MenuId && m.DeletedAt == null);
            if (!hasMenu)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Menu item (Product) does not exist",
                    Data = null
                });
            }

            // 2. Check if the Option Group exists and is not soft-deleted
            bool hasOptionGroup = await context.OptionGroups.AnyAsync(og => og.Id == dto.OptionGroupId && og.DeletedAt == null);
            if (!hasOptionGroup)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Option Group does not exist",
                    Data = null
                });
            }

            // 3. Check if this specific link already exists to avoid composite primary key duplication errors
            bool alreadyLinked = await context.ProductOptionGroups
                .AnyAsync(pog => pog.MenuId == dto.MenuId && pog.OptionGroupId == dto.OptionGroupId);

            if (alreadyLinked)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "This Option Group is already linked to this product",
                    Data = null
                });
            }

            // 4. Create the mapping row
            var productOptionGroupData = new ProductOptionGroup()
            {
                MenuId = dto.MenuId,
                OptionGroupId = dto.OptionGroupId,
                CreatedAt = DateTime.UtcNow
            };

            await context.ProductOptionGroups.AddAsync(productOptionGroupData);
            bool isSaved = await context.SaveChangesAsync() > 0;

            if (isSaved)
            {
                return StatusCode(StatusCodes.Status201Created, new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Option Group successfully linked to the product",
                    Data = new { dto.MenuId, dto.OptionGroupId }
                });
            }
            else
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Failed to link Option Group to the product",
                    Data = null
                });
            }
        }
        //to join Menu and Option_Group table

        [HttpPut("{id}")]
        [EndpointSummary("Update Menu Data")]
        public async Task<IActionResult> UpdateMenu(int id,RequestDtos.RequestMenu menuDto)
        {
            Menu? existingMenu = await context.Menus.FirstOrDefaultAsync(c => c.MenuId == id);
            if (existingMenu == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data doesn't exist",
                    Data = null

                });
            }
            existingMenu.MenuName = menuDto.MenuName;
            existingMenu.Price= menuDto.Price;
            existingMenu.CategoryId = menuDto.CategoryId;
            existingMenu.Description= menuDto.Description;
            existingMenu.UpdatedAt = DateTime.UtcNow;
            existingMenu.IsAvailable= true;

            if (!string.IsNullOrEmpty(menuDto.MenuImage) &&
               !menuDto.MenuImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string extension = convertion.GetFileExtension(menuDto.MenuImage);
                string fileName = $"{existingMenu.MenuId}{extension}";

                byte[] imageBytes = Convert.FromBase64String(menuDto.MenuImage);
                using MemoryStream memoryStream = new(imageBytes);
                IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "fileUpload", fileName);

                // Save image
                _ = await FileService.WriteImageDocker(formFile, existingMenu.MenuId.ToString(), "menu");

                // Set new image path
                existingMenu.MenuImage = $"images/menu/{fileName}";
            }
            else
            {
                // retain existing image
                existingMenu.MenuImage = existingMenu.MenuImage;
            }

            //context.Attach(categorydto).State = EntityState.Modified;
            bool isSaved = await context.SaveChangesAsync() > 0;

            if (isSaved)
            {
                var responseData = new ResponseDtos.AllMenu
                {
                    Id = existingMenu.MenuId,
                    MenuName = existingMenu.MenuName,
                    MenuImage= existingMenu.MenuImage,
                    Price=existingMenu.Price,
                    Description= existingMenu.Description,
                    CategoryId=existingMenu.CategoryId,

                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Menu updated successfully",
                    Data = responseData
                });
            }
            else
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Menu updated failed",
                    Data = null
                });
            }

        }
        

        [HttpDelete("{id}")]
        [EndpointSummary("Delete menu Data")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menuData=await context.Menus.FindAsync(id);
            if(menuData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Menu Data not found",
                    Data = null
                });
            }
            menuData.DeletedAt=DateTime.UtcNow;
            context.Menus.Update(menuData);
            return await context.SaveChangesAsync() > 0
                ? StatusCode(StatusCodes.Status201Created, new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Menu deleted successfully",
                    Data = null
                })
                : BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Menu deleted failed",
                    Data = null
                });


        }
    }
}
