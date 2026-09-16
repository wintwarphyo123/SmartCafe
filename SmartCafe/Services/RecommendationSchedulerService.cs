using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;
using SmartCafe.Entities;

namespace SmartCafe.Services
{
    public class RecommendationSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecommendationSchedulerService> _logger;

        public RecommendationSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<RecommendationSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial delay before first run
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartCafeDbContext>();
                    await GenerateRecommendationsLogic(dbContext, minSupportPercentage: 3.0);
                }

                // Production: Delay until midnight (12:00 AM)
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1);
                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task GenerateRecommendationsLogic(SmartCafeDbContext context, double minSupportPercentage = 3.0)
        {
            try
            {
                _logger.LogInformation("=== STARTING RECOMMENDATION GENERATION ===");

                var totalOrders = await context.Orders.CountAsync();
                _logger.LogInformation($"Total Orders Count: {totalOrders}");

                if (totalOrders == 0)
                {
                    _logger.LogWarning("No orders found in database. Exiting.");
                    return;
                }

                // FIXED: Formula corrected to divide by 100.0
                int minSupportCount = (int)Math.Ceiling(totalOrders * (minSupportPercentage / 100.0));
                _logger.LogInformation($"Min Support Threshold Count: {minSupportCount}");

                //var itemPairings = await context.OrderItems
                //    .GroupBy(o => o.OrderId)
                //    .SelectMany(g => g.SelectMany(
                //        i1 => g.Where(i2 => i1.MenuId != i2.MenuId),
                //        (i1, i2) => new { MainMenuId = i1.MenuId, RecommendedMenuId = i2.MenuId }
                //    ))
                //    .GroupBy(pair => new { pair.MainMenuId, pair.RecommendedMenuId })
                //    .Select(g => new
                //    {
                //        g.Key.MainMenuId,
                //        g.Key.RecommendedMenuId,
                //        PairCount = g.Count()
                //    })
                //    .Where(p => p.PairCount >= minSupportCount)
                //    .ToListAsync();
                // DTO သို့မဟုတ် Anonymous Object သို့ တိုက်ရိုက် Map လုပ်ခြင်း
                var itemPairings = await (
                    from i1 in context.OrderItems
                    join i2 in context.OrderItems on i1.OrderId equals i2.OrderId
                    where i1.MenuId != i2.MenuId
                    group i2 by new { MainMenuId = i1.MenuId, RecommendedMenuId = i2.MenuId } into g
                    select new
                    {
                        MainMenuId = g.Key.MainMenuId,
                        RecommendedMenuId = g.Key.RecommendedMenuId,
                        PairCount = g.Count()
                    }
                )
                .Where(p => p.PairCount >= minSupportCount)
                .ToListAsync();

                _logger.LogInformation($"Found Pairs Count: {itemPairings.Count}");

                var oldData = await context.MenuRecommendations.ToListAsync();
                context.MenuRecommendations.RemoveRange(oldData);

                foreach (var pair in itemPairings)
                {
                    _logger.LogInformation($"--> Pair Found: Drink ID {pair.MainMenuId} + Snack ID {pair.RecommendedMenuId} (Count: {pair.PairCount})");

                    var newRec = new MenuRecommendation
                    {
                        MainMenuId = pair.MainMenuId,
                        RecommendedMenuId = pair.RecommendedMenuId,
                        PairingCount = pair.PairCount,
                        SupportScore = Math.Round(((double)pair.PairCount / totalOrders) * 100, 2),
                        LastUpdated = DateTime.UtcNow
                    };
                    await context.MenuRecommendations.AddAsync(newRec);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("=== RECOMMENDATIONS SAVED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GenerateRecommendationsLogic: {ex.Message}");
            }
        }
    }
}