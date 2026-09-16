using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Entities;
using SmartCafe.Models;
using SmartCafe.Services;

namespace SmartCafe.Controllers
{

    [Authorize]
    [Route("api/OptionGroup")]
    [ApiController]
    public class OptionGroupController(SmartCafeDbContext context):ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [EndpointSummary("Get Option Groups")]
        public async Task<IActionResult> GetOptions()
        {
            var optionList=await context.OptionGroups
                .Where(o=>o.DeletedAt==null)
                .Select(o=> new ResponseDtos.ResponseOptionGroup()
                {
                    Id=o.Id,
                    GroupName=o.GroupName,
                    Status=o.Status,
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
        [Authorize(Roles = "Admin")]
        [HttpGet("Deleted")]
        [EndpointSummary("Get Deleted Data")]
        public async Task<IActionResult> GetDeletedData()
        {
            var groupData = await context.OptionGroups
                .Where(o => o.DeletedAt != null)
                .Select(o => new ResponseDtos.ResponseOptionGroup()
                {
                    Id = o.Id,
                    GroupName = o.GroupName,
                    Status = o.Status,
                })
                .ToListAsync();
            if (groupData.Any())
            {
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "Data exist",
                    Data = groupData
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
                    CreatedAt = DateTime.UtcNow,
                    Status=true
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

        [Authorize(Roles = "Admin")]
        [HttpPost("import-optionGroups")]
        [EndpointSummary("Import option groups from Excel file")]
        public async Task<IActionResult> ImportOptionGroup(
     IFormFile file,
     [FromServices] ImportService importService)
        {
            // 1. File Validation
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are allowed.");

            using var stream = file.OpenReadStream();

            // 2. Excel Header to Model Property Mapping
            var mappings = new KeyValuePair<string, string>[]
            {
        new("Option Group Name", nameof(OptionGroup.GroupName)),
        new("GroupName", nameof(OptionGroup.GroupName))
            };

            // 3. Parse Excel Stream
            List<OptionGroup> importedOptionGroups = importService.ImportFromExcelStream<OptionGroup>(stream, mappings);

            if (!importedOptionGroups.Any())
            {
                return BadRequest("No valid data found in the uploaded file.");
            }

            // 4. DB ထဲတွင် ရှိပြီးသား Group Name များကို HashSet ဖြင့် ဆွဲထုတ်ခြင်း
            var existingNames = (await context.OptionGroups
                .Where(g => !string.IsNullOrEmpty(g.GroupName))
                .Select(g => g.GroupName.Trim().ToLower())
                .ToListAsync())
                .ToHashSet();

            var uniqueNewGroups = new List<OptionGroup>();
            var processedNamesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 5. DB ရော Excel ထဲတွင်ပါ ထပ်နေသော Duplicate များကို Filter ပြုလုပ်ခြင်း
            foreach (var item in importedOptionGroups)
            {
                if (string.IsNullOrWhiteSpace(item.GroupName)) continue;

                string cleanName = item.GroupName.Trim();
                string lookupName = cleanName.ToLower();

                if (!existingNames.Contains(lookupName) && !processedNamesInFile.Contains(lookupName))
                {
                    item.GroupName = cleanName;
                    item.CreatedAt = DateTime.UtcNow;
                    item.Status = true;

                    uniqueNewGroups.Add(item);
                    processedNamesInFile.Add(lookupName);
                }
            }

            // 6. Data အားလုံး Duplicate ဖြစ်နေပါက Skip မည်
            if (!uniqueNewGroups.Any())
            {
                return Ok(new
                {
                    Success = true,
                    Message = "All option groups from the file already exist in the database or are duplicates. Nothing imported."
                });
            }

            // 7. Distinct/Unique Record များကိုသာ Bulk Insert ပြုလုပ်မည်
            await context.OptionGroups.AddRangeAsync(uniqueNewGroups);
            await context.SaveChangesAsync();

            int skippedCount = importedOptionGroups.Count - uniqueNewGroups.Count;

            return Ok(new
            {
                Success = true,
                Message = $"Successfully imported {uniqueNewGroups.Count} option group(s)." +
                          (skippedCount > 0 ? $" ({skippedCount} duplicate(s) skipped)." : "")
            });
        }

        [Authorize(Roles = "Admin")]
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
                optionData.Status = true;
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Restore")]
        [EndpointSummary("Restore Deleted Data")]
        public async Task<IActionResult> RestoreData(int id)
        {
            var groupData = await context.OptionGroups.FirstOrDefaultAsync(o => o.Id == id);
            if (groupData == null)
            {
                return BadRequest(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status400BadRequest,
                    Message = "Data is missed",
                    Data = null
                });
            }
            bool isNameConflict = await context.OptionGroups.AnyAsync(o => o.GroupName== groupData.GroupName && o.DeletedAt == null);
            if (isNameConflict)
            {
                return Conflict(new DefaultResponseModel()
                {
                    Success = false,
                    Statuscode = StatusCodes.Status409Conflict,
                    Message = $"An active option group named '{groupData.GroupName}' already exists.",
                    Data = null
                });
            }

            groupData.DeletedAt = null;
            context.OptionGroups.Update(groupData);
            await context.SaveChangesAsync();
            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Status change successfully",
                Data = groupData
            });
        }
        [Authorize(Roles = "Admin")]
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
