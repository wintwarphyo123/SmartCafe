using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.DTOs;
using SmartCafe.Models;
using static SmartCafe.DTOs.ResponseDtos;

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
                var today = DateTime.Today;
                var menuCount =await context.Menus.CountAsync(m => m.IsAvailable == true);
                var categoryCount = await context.Categories.CountAsync(c => c.IsActive == true);
                var orderCount = await context.Orders.CountAsync(o=>o.OrderStatus=="Paid" && o.CreatedAt >= today);
                var dailyRevenue = await context.Orders
                    .Where(o => o.OrderStatus == "Paid" && o.CreatedAt >= today)
                    .SumAsync(o => o.TotalAmount);
                var userCount =await context.UserInfos.CountAsync(u => u.Role == "Staff");

                var summary = new ResponseDtos.SummaryDashboardDto
                {
                    TotalMenu = menuCount,
                    TotalCategory = categoryCount,
                    TotalOrders=orderCount,
                    TotalRevenue=dailyRevenue,
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

        [HttpGet("trending_item")]
        [EndpointSummary("Get trending items")]
        [ProducesResponseType(typeof(List<TrendingItemResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrendingItem([FromQuery] int limit=5) 
        {
            DateTime startDate= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime endDate = DateTime.Today.AddDays(1);
            
            var saleItem=context.OrderItems
                .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt < endDate && oi.Order.OrderStatus != "Cancelled")
                .GroupBy(oi => new 
                { 
                    oi.MenuId,
                    oi.Menu.MenuName,
                    oi.Menu.Category.CategoryName
                })
                .Select(g => new
                {
                    g.Key.MenuId,
                    g.Key.MenuName,
                    g.Key.CategoryName,
                    TotalQuantity = g.Sum(oi => oi.Quantity) 
                });
            int allItemTotalSale = await saleItem.SumAsync(x => x.TotalQuantity);
            var trendingItems = await saleItem
                .OrderByDescending(x => x.TotalQuantity)
                .Take(limit)
                .ToListAsync();
            var result = trendingItems.Select(item => new TrendingItemResponseModel
            {
                MenuId = item.MenuId,
                MenuName = item.MenuName,
                CategoryName = item.CategoryName,
                TotalSales = item.TotalQuantity,
                Percentage = allItemTotalSale > 0
            ? Math.Round((double)item.TotalQuantity / allItemTotalSale * 100, 1)
            : 0
            }).ToList();

            return Ok(new DefaultResponseModel()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message="All Trending items",
                Data=result
            });
        }

        [HttpGet("Revenue_Overview")]
        [EndpointSummary("Get Revenue Overview")]
        public async Task<IActionResult> GetRevenue([FromQuery] string period="month")
        {
            var labels = new List<string>();
            var values = new List<decimal>();
            DateTime today = DateTime.Today;
            if (period.ToLower() == "day")
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now.AddDays(1);

                var hourlySales = await context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.OrderStatus != "Cancelled")
            .Select(o => new { o.CreatedAt.Hour, o.TotalAmount })
            .ToListAsync();

                for (int hour = 8; hour <= 20; hour += 2)
                {
                    string ampm = hour >= 12 ? "PM" : "AM";
                    int displayHour = hour > 12 ? hour - 12 : hour;
                    labels.Add($"{displayHour:D2}:00 {ampm}");

                    var sum = hourlySales.Where(h => h.Hour >= hour && h.Hour < hour + 2).Sum(h => h.TotalAmount);
                    values.Add(sum);
                }
            }
            else if (period.ToLower() == "year")
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime endDate = startDate.AddYears(1);

                var monthlySales = await context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.OrderStatus != "Cancelled")
                    .GroupBy(o => o.CreatedAt.Month)
                    .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
                    .ToListAsync();

                string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                for (int m = 1; m <= 12; m++)
                {
                    labels.Add(monthNames[m - 1]);
                    var salesForMonth = monthlySales.FirstOrDefault(ms => ms.Month == m)?.Total ?? 0;
                    values.Add(salesForMonth);
                }
            }
            else
            {
                DateTime startDate = new DateTime(DateTime.Now.Year,DateTime.Now.Month, 1);
                DateTime endDate = startDate.AddMonths(1);

                var ordersInMonth = await context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate && o.OrderStatus != "Cancelled")
                    .Select(o => new { o.CreatedAt.Day, o.TotalAmount })
                    .ToListAsync();

                for (int week = 1; week <= 4; week++)
                {
                    labels.Add($"Week {week}");
                    int startDay = (week - 1) * 7 + 1;
                    int endDay = week == 4 ? 31 : week * 7;

                    var sum = ordersInMonth.Where(o => o.Day >= startDay && o.Day <= endDay).Sum(o => o.TotalAmount);
                    values.Add(sum);
                }
            }
            return Ok(new
            {
                success = true,
                data = new
                {
                    labels = labels,
                    values = values
                }
            });
        }
    }
}
