using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;

namespace SmartCafe.Services
{
    public class OrderCleanService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCleanService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 💡 1. App တက်တက်ချင်း တန်းအလုပ်လုပ်ရန် CleanExpiredOrders ကို ပထမဆုံးအကြိမ် တိုက်ရိုက် ခေါ်ယူပါသည်
            await CleanExpiredOrdersAsync(stoppingToken);

            // 💡 2. ထို့နောက်မှ ၁ နာရီလျှင် တစ်ကြိမ် ပုံမှန် စစ်ဆေးပေးပါမည်
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanExpiredOrdersAsync(stoppingToken);
            }
        }

        private async Task CleanExpiredOrdersAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartCafeDbContext>();

                var todayStart = DateTime.Today;

                // မနေ့ကအထိ အော်ဒါအဟောင်းများထဲမှ Ready သို့မဟုတ် Cancelled မဟုတ်သော အော်ဒါများကို ရှာပါမည်
                var expiredOrders = await dbContext.Orders
                    .Include(o => o.OrderItems) // 💡 Foreign Key Error မတက်အောင် OrderItems များကိုပါ ပူးတွဲ Fetch လုပ်ပါသည်
                    .Where(o => o.CreatedAt < todayStart &&
                                o.OrderStatus != "Ready" &&
                                o.OrderStatus != "Cancelled")
                    .ToListAsync(stoppingToken);

                if (expiredOrders.Any())
                {
                    // Foreign Key ကြောင့် Error မတက်စေရန် OrderItems များကိုပါ ရောပြီး ဖျက်ပေးပါသည်
                    var expiredOrderItems = expiredOrders.SelectMany(o => o.OrderItems).ToList();
                    if (expiredOrderItems.Any())
                    {
                        dbContext.OrderItems.RemoveRange(expiredOrderItems);
                    }

                    dbContext.Orders.RemoveRange(expiredOrders);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    Console.WriteLine($"[SmartCafe] Successfully permanently deleted {expiredOrders.Count} expired orders from database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartCafe Error] Cleaning expired orders failed: {ex.Message}");
            }
        }
    }
}