using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Models;

namespace SmartCafe.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OptionItemController(SmartCafeDbContext context):ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [EndpointSummary("Get Option Items")]
        public async Task<IActionResult> GetOptionItems()
        {
            var ItemList = await context.OptionItems
                .Where(i => i.DeletedAt == null)
                .Select(i => new ResponseDtos.ResponseOptionItem()
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    ExtraPrice = i.ExtraPrice,
                    OptionGroupId = i.OptionGroupId,
                    Status= i.Status,
                    GroupName=i.OptionGroup.GroupName?? "No Category"
                }).ToListAsync();
            if (!ItemList.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Data Not exist",
                    Data = null
                });
            }
            else
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = ItemList
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Deleted")]
        [EndpointSummary("Get Deleted Data")]
        public async Task<IActionResult> GetDeletedData()
        {
            var itemData = await context.OptionItems
                .Where(i => i.DeletedAt != null)
                .Select(i => new ResponseDtos.ResponseOptionItem()
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    ExtraPrice = i.ExtraPrice,
                    OptionGroupId = i.OptionGroupId,
                    Status = i.Status,
                    GroupName = i.OptionGroup.GroupName ?? "No Category"
                }).ToListAsync();
            if (itemData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = itemData
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
        [HttpPost]
        [EndpointSummary("Create New Option Item")]
        public async Task<IActionResult> CreateOptionItem(RequestDtos.RequestOptionItem itemdto)
        {
            bool hasItem=await context.OptionItems.AnyAsync(i=>i.ItemName==itemdto.ItemName);
            if (hasItem)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "item already exist",
                    Data = null
                });
            }
            bool hasOption = await context.OptionGroups.AnyAsync(o => o.Id == itemdto.OptionGroupId);
            if (!hasOption)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Option Group doesn't exist",
                    Data = null
                });
            }
            var itemData = new OptionItem()
            {
                ItemName = itemdto.ItemName,
                ExtraPrice = itemdto.ExtraPrice,
                OptionGroupId = itemdto.OptionGroupId,
                CreatedAt = DateTime.UtcNow,
                Status = true
            };
            await context.OptionItems.AddAsync(itemData);
            bool isSaved = await context.SaveChangesAsync() > 0;
            if (isSaved)
            {
                var responseData = new ResponseDtos.ResponseOptionItem()
                {
                    Id = itemData.Id,
                    ItemName=itemData.ItemName,
                    ExtraPrice=itemData.ExtraPrice,
                    OptionGroupId=itemData.OptionGroupId
                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Option Item created successfully",
                    Data = responseData
                });

            }
            else
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Option Item created failed",
                    Data = null
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [EndpointSummary("Update Option Item")]
        public async Task<IActionResult> UpdateItem(int id,RequestDtos.RequestOptionItem itemDto)
        {
            var itemData = await context.OptionItems.FirstOrDefaultAsync(i => i.Id == id);
            if(itemData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status404NotFound,
                    Message="Data Not found",
                    Data=null
                });
            }
            bool hasOption = await context.OptionGroups.AnyAsync(o => o.Id == itemDto.OptionGroupId);
            if (!hasOption)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Option Group doesn't exist",
                    Data = null
                });
            }
            itemData.ItemName= itemDto.ItemName;
            itemData.ExtraPrice= itemDto.ExtraPrice;
            itemData.OptionGroupId= itemDto.OptionGroupId;
            itemData.UpdatedAt = DateTime.UtcNow;
            itemData.Status = true;
            context.OptionItems.Update(itemData);
            bool isSaved=await context.SaveChangesAsync()>0;
            
            if (isSaved)
            {
                var responseData = new ResponseDtos.ResponseOptionItem()
                {
                    Id = itemData.Id,
                    ItemName = itemData.ItemName,
                    ExtraPrice = itemData.ExtraPrice,
                    OptionGroupId = itemData.OptionGroupId
                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Option Item updated successfully",
                    Data = responseData
                });

            }
            else
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Option Item updated failed",
                    Data = null
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Restore")]
        [EndpointSummary("Restore Deleted Data")]
        public async Task<IActionResult> RestoreData(int id)
        {
            var itemData = await context.OptionItems.FirstOrDefaultAsync(i => i.Id == id);
            if (itemData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data is missed",
                    Data = null
                });
            }
            bool isNameConflict = await context.OptionItems.AnyAsync(i => i.ItemName == itemData.ItemName && i.DeletedAt == null);
            if (isNameConflict)
            {
                return Conflict(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status409Conflict,
                    Message = $"An active option item named '{itemData.ItemName}' already exists.",
                    Data = null
                });
            }

            itemData.DeletedAt = null;
            context.OptionItems.Update(itemData);
            await context.SaveChangesAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Status change successfully",
                Data = itemData
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [EndpointSummary("Delete Option Item")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var itemData = await context.OptionItems.FindAsync(id);
            if (itemData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Item Data not found",
                    Data = null
                });
            }
            itemData.DeletedAt = DateTime.UtcNow;
            context.OptionItems.Update(itemData);
            return await context.SaveChangesAsync() > 0
                ? StatusCode(StatusCodes.Status201Created, new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status201Created,
                    Message = "Item deleted successfully",
                    Data = null
                })
                : BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Item deleted failed",
                    Data = null
                });
        }
    }
}
