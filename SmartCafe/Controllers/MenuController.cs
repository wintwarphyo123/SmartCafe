using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Hubs;
using SmartCafe.Interfaces;
using SmartCafe.Models;
using static SmartCafe.DTOs.ResponseDtos;

namespace SmartCafe.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController(SmartCafeDbContext context,
        IConvertion convertion,
        IFileService FileService,
        IHubContext<NotificationHubs> hubContext) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [EndpointSummary("Get all Menu Data")]
        public async Task<IActionResult> GetMenuData()
        {
            var menuList = await context.Menus.AsNoTracking()
                .Where(m => m.DeletedAt == null)
                .Select(m => new ResponseDtos.AllMenu()
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage = m.MenuImage,
                    Description = m.Description,
                    Price = m.Price,
                    Is_available = m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = (m.Category != null && m.Category.DeletedAt == null)
                            ? m.Category.CategoryName
                            : "Deleted Category",
                    IsSpecial= m.IsSpecial,
                    Archived=m.Archived

                }).ToListAsync();
            if (!menuList.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Menu Data not found",
                    Data = null
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

        [AllowAnonymous]
        [HttpGet("Kiosk-menus")]
        [EndpointSummary("Get all Menu Data for Customer")]
        public async Task<IActionResult> GetMenuforCustomer()
        {

            
            var menuList = await context.Menus
                .Where(m => m.DeletedAt == null && m.IsSpecial==false && m.Archived==false
                && (m.Category == null || (m.Category.DeletedAt == null)))
                .Select(m => new ResponseDtos.AllMenu()
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage = m.MenuImage,
                    Description = m.Description,
                    Price = m.Price,
                    Is_available = (m.Category != null && m.Category.IsActive == false) ? false : m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category != null ? m.Category.CategoryName : "No Category",
                    IsSpecial=m.IsSpecial,
                    Archived=m.Archived

                }).ToListAsync();
            if (!menuList.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Menu Data not found",
                    Data = null
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

        [Authorize(Roles = "Admin,KitchenStaff")]
        [HttpGet("Deleted")]
        [EndpointSummary("Get Deleted Data")]
        public async Task<IActionResult> GetDeletedData()
        {
            var menuData = await context.Menus.AsNoTracking()
                .Where(m=> m.DeletedAt != null)
                .Select(m => new ResponseDtos.AllMenu()
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage = m.MenuImage,
                    Description = m.Description,
                    Price = m.Price,
                    Is_available = m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category != null ? m.Category.CategoryName : "No Category",
                    IsSpecial=m.IsSpecial,
                    Archived=m.Archived

                }).ToListAsync();
            if (menuData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = menuData
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

        [AllowAnonymous]
        [HttpGet("Special")]
        [EndpointSummary("Get Special Data")]
        public async Task<IActionResult> GetSpecialData()
        {
            var menuData = await context.Menus.AsNoTracking()
                .Where(m => m.DeletedAt == null && m.IsSpecial==true && m.Archived==false)
                .Select(m => new ResponseDtos.AllMenu()
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage = m.MenuImage,
                    Description = m.Description,
                    Price = m.Price,
                    Is_available = m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category != null ? m.Category.CategoryName : "No Category",
                    IsSpecial = m.IsSpecial,
                    Archived = m.Archived

                }).ToListAsync();
            if (menuData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = menuData
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

        [AllowAnonymous]
        [HttpGet("IsSpecial")]
        [EndpointSummary("Get Special Data for Admin")]
        public async Task<IActionResult> GetSpecialDataForAdmin()
        {
            var menuData = await context.Menus.AsNoTracking()
                .Where(m => m.DeletedAt == null && m.IsSpecial == true)
                .Select(m => new ResponseDtos.AllMenu
                {
                    Id = m.MenuId,
                    MenuName = m.MenuName,
                    MenuImage = m.MenuImage,
                    Description = m.Description,
                    Price = m.Price,
                    Is_available = m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category != null ? m.Category.CategoryName : "No Category",
                    IsSpecial = m.IsSpecial,
                    Archived = m.Archived

                }).ToListAsync();
            if (menuData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = menuData
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

        [Authorize(Roles = "Admin,KitchenStaff")]
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
                    Is_available = menuData.IsAvailable,
                    IsSpecial = menuData.IsSpecial,
                    Archived= menuData.Archived,
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
        [Authorize(Roles = "Admin,KitchenStaff")]
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
        [AllowAnonymous]
        [HttpGet("all_categories")]
        [EndpointSummary("Get all categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categoryList = await context.Categories.AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .Select(c=>new AllCategoryForDropDown
                {
                    CategoriesId=c.CategoryId,
                    CategoriesName=c.CategoryName,
                })
                .ToListAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode=StatusCodes.Status200OK,
                Message="All categories",
                Data=categoryList
            });
        }


        [Authorize(Roles = "Admin,KitchenStaff")]
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
        [AllowAnonymous]
        [HttpGet("all-option-groups")]
        [EndpointSummary("Get All Option Groups for Dropdown Master List")]
        public async Task<IActionResult> GetAllOptionGroups()
        {
            
            var optionGroups = await context.OptionGroups.AsNoTracking()
                .Where(og => og.DeletedAt == null) 
                .Select(og => new OptionGroupDto
                {
                    GroupId = og.Id,
                    GroupName = og.GroupName,
                    OptionItems = context.OptionItems
                        .Where(oi => oi.OptionGroupId == og.Id && oi.DeletedAt==null)
                        .Select(oi => new OptionItemDto
                        {
                            ItemId = oi.Id,
                            ItemName = oi.ItemName,
                            ExtraPrice = oi.ExtraPrice
                        }).ToList()
                }).ToListAsync();

            return Ok(new DefaultResponseModel { Success = true, Data = optionGroups });
        }

        [AllowAnonymous]
        [HttpGet("detail/{id}")]
        [EndpointSummary("Get Menu Detail")]
        public async Task<IActionResult> GetMenuDetail(int id)
        {
            
            var menu=await context.ViewMenuDetailOptions.AsNoTracking().Where(m=>m.MenuId==id).ToListAsync();
            if (!menu.Any())
            {
                var menuExist = await context.Menus.AnyAsync(m => m.MenuId == id && m.DeletedAt == null);
                if (!menuExist)
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
            var firstRow = menu.FirstOrDefault();

            var result = new MenuDetailResponseDto
            {
                MenuId = id,
                MenuName = firstRow?.MenuName ?? string.Empty,
                Price = firstRow?.MenuPrice ?? 0,
                Description = firstRow?.MenuDescription ?? string.Empty,

                OptionGroups = menu
            .Where(g => g.GroupId.HasValue) // Filter out NULL group rows
            .GroupBy(g => new { GroupId = g.GroupId!.Value, g.GroupName })
            .Select(mog => new OptionGroupDto
            {
                GroupId = mog.Key.GroupId,
                GroupName = mog.Key.GroupName ?? string.Empty,

                OptionItems = mog
                    .Where(oi => oi.ItemId.HasValue) // Filter out NULL item rows
                    .Select(oi => new OptionItemDto
                    {
                        ItemId = oi.ItemId!.Value,
                        ItemName = oi.ItemName ?? string.Empty,
                        ExtraPrice = oi.ExtraPrice ?? 0,
                        IsAvailable = oi.IsAvailable ?? false
                    }).ToList()
            }).ToList()
            };

            return Ok(new DefaultResponseModel { Success = true, Data = result });
        }

        [AllowAnonymous]
        [HttpGet("Recommend/{menuId}")]
        [EndpointSummary("GetRecommendationMenu")]
        public async Task<ActionResult<IEnumerable<ResponseRecommendation>>> GetRecommendationsByMenuId(int menuId)
        {
            var recommendations = await context.MenuRecommendations
                .Where(r => r.MainMenuId == menuId)
                .Include(r => r.RecommendedMenu) // Navigation property ဖြင့် Menu အချက်အလက်များ ဆွဲယူခြင်း
                .OrderByDescending(r => r.SupportScore)
                .Select(r => new ResponseRecommendation
                {
                    MainMenuId = r.MainMenuId,
                    RecommendedMenuId = r.RecommendedMenuId,
                    RecommendedMenuName = r.RecommendedMenu.MenuName,
                    RecommendedMenuPrice = (decimal)r.RecommendedMenu.Price,
                    RecommendedMenuImageUrl = r.RecommendedMenu.MenuImage,
                    PairingCount = r.PairingCount,
                    SupportScore = r.SupportScore
                })
                .ToListAsync();

            if (!recommendations.Any())
            {
                return NotFound(new { Message = "No recommendations found for this item." });
            }

            return Ok(recommendations);
        }
    
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [EndpointSummary("Create new Menu")]
        public async Task<IActionResult> CreateMenu(RequestDtos.RequestMenu menuDto)
        {//description,categoryId,price,isAvailable,categoryId,
            Menu menuData = new()
            {
                MenuName = menuDto.MenuName,
                Description = menuDto.Description,
                CategoryId = menuDto.CategoryId,
                Price = menuDto.Price,
                IsAvailable = menuDto.Is_available,
                CreatedAt = DateTime.UtcNow,
                IsSpecial = menuDto.IsSpecial,
                Archived = false
            };
            context.Menus.Add(menuData);
            bool isSaved = await context.SaveChangesAsync() > 0;
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

                    // လက်ရှိအချိန် (yyyyMMddHHmmssfff) ပါဝင်သော ဖိုင်အမည်သတ်မှတ်ခြင်း
                    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    string fileName = $"{menuData.MenuId}_{timestamp}{extension}";

                    menuData.MenuImage = $"images/menu/{fileName}";
                    //string extension = convertion.GetFileExtension(base64Data);
                    //string fileName = $"{menuData.MenuId}{extension}";

                   // menuData.MenuImage = $"images/menu/{fileName}";

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
                        bool imageSavedResult = await FileService.WriteImageDocker(formFile, $"{menuData.MenuId}_{timestamp}", "menu");
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
                        CategoryId = menuData.CategoryId,
                        IsSpecial= menuData.IsSpecial,
                        Archived= menuData.Archived,
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
        [Authorize(Roles = "Admin")]
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
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var alreadyLink = await context.ProductOptionGroups
                .Where(pod => pod.MenuId == dto.MenuId).ToListAsync();
                if (alreadyLink.Any())
                {
                    context.ProductOptionGroups.RemoveRange(alreadyLink);
                }
                if (dto.OptionGroupIds != null && dto.OptionGroupIds.Any())
                {
                    var newLink = dto.OptionGroupIds.Select(id => new ProductOptionGroup
                    {
                        MenuId = dto.MenuId,
                        OptionGroupId = id,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                    await context.ProductOptionGroups.AddRangeAsync(newLink);
                }
                var currentDisabledItems = await context.MenuDisabledOptions
            .Where(mdo => mdo.MenuId == dto.MenuId).ToListAsync();

                if (currentDisabledItems.Any())
                {
                    context.MenuDisabledOptions.RemoveRange(currentDisabledItems);
                }
                if (dto.OptionsAvailability != null && dto.OptionsAvailability.Any())
                {
                    var outOfStockRecords = dto.OptionsAvailability
                        .Where(item => item.IsAvailable == false)
                        .Select(item => new MenuDisabledOption // Change this mapping object matching your entity architecture
                        {
                            MenuId = dto.MenuId,
                            OptionItemId = item.OptionItemId,
                            CreatedAt = DateTime.UtcNow
                        }).ToList();

                    if (outOfStockRecords.Any())
                    {
                        await context.MenuDisabledOptions.AddRangeAsync(outOfStockRecords);
                    }
                }
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Successfully",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Message = $"Failed to save updates: {ex.Message}",
                    Data = null
                });
            }

        }
        //to join Menu and Option_Group table
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [EndpointSummary("Update Menu Data")]
        public async Task<IActionResult> UpdateMenu(int id, RequestDtos.RequestMenu menuDto)
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
            existingMenu.Price = menuDto.Price;
            existingMenu.CategoryId = menuDto.CategoryId;
            existingMenu.Description = menuDto.Description;
            existingMenu.UpdatedAt = DateTime.UtcNow;
            existingMenu.IsAvailable = true;

            // ပုံအသစ် (Base64) ပါလာမှသာ စစ်ဆေးပြီး သိမ်းဆည်းရန်
            if (!string.IsNullOrWhiteSpace(menuDto.MenuImage) &&
                !menuDto.MenuImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !menuDto.MenuImage.StartsWith("images/", StringComparison.OrdinalIgnoreCase) &&
                !menuDto.MenuImage.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                string base64Data = menuDto.MenuImage;
                
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                //string extension = convertion.GetFileExtension(base64Data);
                //string fileName = $"{existingMenu.UpdatedAt}{extension}";
                string extension = convertion.GetFileExtension(base64Data);

                // လက်ရှိအချိန် (yyyyMMddHHmmssfff) ပါဝင်သော ဖိုင်အမည်သတ်မှတ်ခြင်း
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                string fileName = $"{existingMenu.MenuId}_{timestamp}{extension}";

                existingMenu.MenuImage = $"images/menu/{fileName}";

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                using (MemoryStream memoryStream = new(imageBytes))
                {
                    memoryStream.Position = 0;
                    IFormFile formFile = new FormFile(memoryStream, 0, imageBytes.Length, "fileUpload", fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = $"image/{extension.TrimStart('.')}"
                    };

                    bool imageSavedResult = await FileService.WriteImageDocker(formFile, $"{existingMenu.MenuId}_{timestamp}", "menu");
                    if (!imageSavedResult)
                    {
                        return BadRequest(new DefaultResponseModel()
                        {
                            Success = false,
                            Statuscode = StatusCodes.Status400BadRequest,
                            Message = "Failed to save menu image.",
                            Data = null
                        });
                    }
                }

                existingMenu.MenuImage = $"images/menu/{fileName}";
            }

            try
            {
                await context.SaveChangesAsync();

                var responseData = new ResponseDtos.AllMenu
                {
                    Id = existingMenu.MenuId,
                    MenuName = existingMenu.MenuName,
                    MenuImage = existingMenu.MenuImage,
                    Price = existingMenu.Price,
                    Description = existingMenu.Description,
                    CategoryId = existingMenu.CategoryId,
                    IsSpecial = existingMenu.IsSpecial,
                    Archived = existingMenu.Archived
                };

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Menu updated successfully",
                    Data = responseData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = $"Menu update failed: {ex.Message}",
                    Data = null
                });
            }
        }
        //[Authorize(Roles = "Admin")]
        //[HttpPut("{id}")]
        //[EndpointSummary("Update Menu Data")]
        //public async Task<IActionResult> UpdateMenu(int id, RequestDtos.RequestMenu menuDto)
        //{
        //    Menu? existingMenu = await context.Menus.FirstOrDefaultAsync(c => c.MenuId == id);
        //    if (existingMenu == null)
        //    {
        //        return NotFound(new DefaultResponseModel()
        //        {
        //            Success = false,
        //            Statuscode = StatusCodes.Status404NotFound,
        //            Message = "Data doesn't exist",
        //            Data = null
        //        });
        //    }

        //    existingMenu.MenuName = menuDto.MenuName;
        //    existingMenu.Price = menuDto.Price;
        //    existingMenu.CategoryId = menuDto.CategoryId;
        //    existingMenu.Description = menuDto.Description;
        //    existingMenu.UpdatedAt = DateTime.UtcNow;
        //    existingMenu.IsAvailable = true;

            
        //    if(!string.IsNullOrEmpty(menuDto.MenuImage) && !menuDto.MenuImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        //    {
        //        string extension = convertion.GetFileExtension(menuDto.MenuImage);
        //        string fileName = $"{existingMenu.MenuId}{extension}";
        //        byte[] imageBytes = Convert.FromBase64String(menuDto.MenuImage);
        //        using MemoryStream memoryStream = new(imageBytes);
        //        IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "fileUpload", fileName);
        //        _ = await FileService.WriteImageDocker(formFile, existingMenu.MenuId.ToString(), "menu");
        //        existingMenu.MenuImage = $"images/menu/{fileName}";

        //    }
        //    else
        //    {
        //        existingMenu.MenuImage = existingMenu.MenuImage;
        //    }
          
        //   bool isSaved= await context.SaveChangesAsync()>0;
        //    if (isSaved)
        //    {
        //        var responseData = new ResponseDtos.AllMenu
        //        {
        //            Id = existingMenu.MenuId,
        //            MenuName = existingMenu.MenuName,
        //            MenuImage = existingMenu.MenuImage,
        //            Price = existingMenu.Price,
        //            Description = existingMenu.Description,
        //            CategoryId = existingMenu.CategoryId,
        //            IsSpecial = existingMenu.IsSpecial,
        //            Archived = existingMenu.Archived
        //        };

        //        return Ok(new DefaultResponseModel()
        //        {
        //            Success = true,
        //            Statuscode = StatusCodes.Status200OK, // HTTP 200 OK for Update
        //            Message = "Menu updated successfully",
        //            Data = responseData
        //        });
        //    }
        //    else
        //    {
        //        return BadRequest(new DefaultResponseModel()
        //        {
        //            Success = false,
        //            Statuscode = StatusCodes.Status400BadRequest,
        //            Message = "menu updated failed",
        //            Data = null
        //        });
        //    }
        //}

        [Authorize(Roles = "Admin,KitchenStaff")]
        [HttpPut("{menuId}/Available")]
        [EndpointSummary("Change Menu Status")]
        public async Task<IActionResult> ChangeStatus(int menuId)
        {
            var menuData=await context.Menus.FirstOrDefaultAsync(m=>m.MenuId== menuId);
            if (menuData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode=StatusCodes.Status400BadRequest,
                    Message="Data not exist",
                    Data=null
                });
            }
            else
            {
                menuData.IsAvailable = !menuData.IsAvailable;
                context.Menus.Update(menuData);
                await context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("ReceiveMenuUpdate", new
                {
                    menuId = menuData.MenuId,
                    isAvailable = menuData.IsAvailable,
                    action = "status_change" 
                });
                
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Menu data status is changed",
                    Data = menuData
                });
            }
        }
        [Authorize(Roles = "Admin,KitchenStaff")]
        [HttpPut("{menuId}/IsSpecial")]
        [EndpointSummary("Change Menu Special Status")]
        public async Task<IActionResult> ChangeSpeial(int menuId)
        {
            var menuData = await context.Menus.FirstOrDefaultAsync(m => m.MenuId == menuId);
            if (menuData == null)
            {
                return BadRequest(new DefaultResponseModel()
                { 
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data not exist",
                    Data = null
                });
            }
            else
            {
                menuData.IsSpecial = !menuData.IsSpecial;
                context.Menus.Update(menuData);
                await context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("ReceiveMenuSpecial", new
                {
                    menuId = menuData.MenuId,
                    isSpecial=menuData.IsSpecial,
                    action = "status_change"
                });

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Menu data status is changed",
                    Data = menuData
                });
            }
        }

        [Authorize(Roles = "Admin,KitchenStaff")]
        [HttpPut("{menuId}/Archived")]
        [EndpointSummary("Change Menu Archived Status")]
        public async Task<IActionResult> ChangeArchived(int menuId)
        {
            var menuData = await context.Menus.FirstOrDefaultAsync(m => m.MenuId == menuId);
            if (menuData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data not exist",
                    Data = null
                });
            }
            else
            {
                menuData.Archived = !menuData.Archived;
                context.Menus.Update(menuData);
                await context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("RecevieMenuArchived", new
                {
                    menuId = menuData.MenuId,
                    archived = menuData.Archived,
                    action = "status_change"
                });

                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Menu data status is changed",
                    Data = menuData
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Restore")]
        [EndpointSummary("Restore Deleted Data")]
        public async Task<IActionResult> RestoreData(int id)
        {
            var menuData = await context.Menus.FirstOrDefaultAsync(m =>m.MenuId == id);
            if (menuData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data is missed",
                    Data = null
                });
            }
            bool isNameConflict = await context.Menus.AnyAsync(m => m.MenuName == menuData.MenuName && m.DeletedAt == null);
            if (isNameConflict)
            {
                return Conflict(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status409Conflict,
                    Message = $"An active menu named '{menuData.MenuName}' already exists.",
                    Data = null
                });
            }

            menuData.DeletedAt = null;
            context.Menus.Update(menuData);
            await context.SaveChangesAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Status change successfully",
                Data = menuData
            });
        }

        [Authorize(Roles = "Admin")]
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
