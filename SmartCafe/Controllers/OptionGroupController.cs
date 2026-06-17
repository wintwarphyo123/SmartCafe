using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Models;
using SmartCafe.Entities;
using Microsoft.OpenApi;

namespace SmartCafe.Controllers
{
    [Route("api/OptionGroup")]
    [ApiController]
    public class OptionGroupController(SmartCafeDbContext context):ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Get Option Groups")]
        public async Task<IActionResult> GetOptions()
        {
            var optionList=await context.OptionGroups
                .Where(o=>o.DeletedAt==null)
                .Select(o=> new ResponseDtos.ResponseOptionGroup()
                {
                    Id=o.Id,
                    GroupName=o.GroupName
                }).ToListAsync();
            if (!optionList.Any())
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Option Data Not found",
                    Data = null
                });
            }
            else
            {
                return Ok(new DefaultResponseModel()
                {
                    Success=true,
                    Statuscode=StatusCodes.Status200OK,
                    Message="Option Data exist",
                    Data=optionList
                });
            }
        }

        [HttpPost]
        [EndpointSummary("Create new Option Group")]
        public async Task<IActionResult> CreateOption([FromBody]RequestDtos.RequestOptionGroup optiondto)
        {
            if (!ModelState.IsValid)//test validation model
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Invalid data provided",
                    Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }
            bool hasOption=await context.OptionGroups.AnyAsync(o=>o.GroupName==optiondto.GroupName);
            if (hasOption)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status400BadRequest,
                    Message="Data already exist",
                    Data=null
                });
            }
            else
            {
                var optionData = new OptionGroup()
                {
                    GroupName = optiondto.GroupName,
                    CreatedAt = DateTime.UtcNow
                };
                await context.OptionGroups.AddAsync(optionData);
                bool isSaved=await context.SaveChangesAsync()>0;
                if (isSaved)
                {
                    var responseData = new ResponseDtos.ResponseOptionGroup()
                    {
                        Id = optionData.Id,
                        GroupName = optionData.GroupName

                    };
                    return Ok(new DefaultResponseModel()
                    {
                        Success = true,
                        Statuscode = StatusCodes.Status200OK,
                        Message = "Option Group create successfully",
                        Data = responseData
                    });
                }
                else
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success=false,
                        Statuscode=StatusCodes.Status400BadRequest,
                        Message="create failed",
                        Data=null
                    });
                }
            }
        }

        [HttpPut("{id}")]
        [EndpointSummary("Update option group")]
        public async Task<IActionResult> UpdateOption(int id,RequestDtos.RequestOptionGroup optiondto)
        {
            var optionData=await context.OptionGroups.FirstOrDefaultAsync(o=>o.Id==id);
            if (optionData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status404NotFound,
                    Message="Option Data not found",
                    Data=null
                });
            }
            else
            {
                optionData.GroupName = optiondto.GroupName;
                optionData.UpdatedAt = DateTime.UtcNow;
                context.OptionGroups.Update(optionData);
                bool isSaved=await context.SaveChangesAsync()>0;
                if (isSaved)
                {
                    var optionList = new ResponseDtos.ResponseOptionGroup()
                    {
                        Id = optionData.Id,
                        GroupName=optionData.GroupName,

                    };
                    return Ok(new DefaultResponseModel()
                    {
                        Success = true,
                        Statuscode = StatusCodes.Status200OK,
                        Message = "Update Successfully",
                        Data = optionList
                    });

                }
                else
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success=false,
                        Statuscode=StatusCodes.Status400BadRequest,
                        Message="Update failed",
                        Data=null
                    }); 
                }
            }
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Delete Option Group")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var optionData = await context.OptionGroups.FindAsync(id);
            if(optionData == null)
            {
                return NotFound(new DefaultResponseModel()
                {
                    Success=false,
                    Statuscode=StatusCodes.Status404NotFound,
                    Message="Data Not Found",
                    Data=null
                });
            }
            else
            {
                optionData.DeletedAt = DateTime.UtcNow;
                context.OptionGroups.Update(optionData);
                bool isSaved=await  context.SaveChangesAsync()>0;
                if (isSaved)
                {
                    return Ok(new DefaultResponseModel()
                    {
                        Success = true,
                        Statuscode = StatusCodes.Status200OK,
                        Message = "Deleted Successfully",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new DefaultResponseModel()
                    {
                        Success = false,
                        Statuscode = StatusCodes.Status400BadRequest,
                        Message = "Deleted failed",
                        Data = null
                    });
                }
            }
        }

    }
}
