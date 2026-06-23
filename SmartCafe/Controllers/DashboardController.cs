using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Models;

namespace SmartCafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(SmartCafeDbContext context):ControllerBase
    {
        [HttpGet("Summary")]
        [EndpointSummary("Dashboard Summary")]
        public async Task<IActionResult> DashboardSummary()
        {
            try
            {
                var menuCount =await context.Menus.CountAsync(m => m.IsAvailable == true);
                var categoryCount = await context.Categories.CountAsync(c => c.IsActive == true);
                var optionGroupCount = await context.OptionGroups.CountAsync();
                var optionItemCount =await context.OptionItems.CountAsync();
                var userCount =await context.UserInfos.CountAsync(u => u.Role == "Staff");

                //await Task.WhenAll(menuCount, categoryCount, optionGroupCount, optionItemCount, userCount);

                var summary = new ResponseDtos.SummaryDashboardDto
                {
                    TotalMenu = menuCount,
                    TotalCategory = categoryCount,
                    TotalOptionGroup = optionGroupCount,
                    TotalOptionItem =optionItemCount,
                    TotalStaff = userCount
                };
                return Ok(new DefaultResponseModel()
                {
                    Success = true,
                    Statuscode = StatusCodes.Status200OK,
                    Message = "all data",
                    Data = summary,
                });
            }
            catch(Exception )
            {
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
    }
}
