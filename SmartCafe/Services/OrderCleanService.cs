using Microsoft.EntityFrameworkCore;
using SmartCafe.Data;

namespace SmartCafe.Services
{
    public class OrderCleanService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCleanService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ညသန်းခေါင်အချိန် ရောက်မရောက်ကို ၁ နာရီတစ်ခါ နောက်ကွယ်ကနေ စစ်ပေးမယ့် Timer
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // သင့်ရဲ့ Database Context ကို Scope ထဲကနေ ဆွဲယူပါတယ်
                        var dbContext = scope.ServiceProvider.GetRequiredService<SmartCafeDbContext>();

                        // ဒီနေ့ ရက်စွဲရဲ့ အစ (ဥပမာ - 12/07/2026 00:00:00)
                        var todayStart = DateTime.Today;

                        // မနေ့ကအထိ မှာထားပြီး "Ready" သို့မဟုတ် "Cancelled" မဖြစ်သေးတဲ့ အော်ဒါတွေကို ရှာပါတယ်
                        var expiredOrders = await dbContext.Orders
                            .Where(o => o.CreatedAt < todayStart &&
                                        o.OrderStatus != "Ready" &&
                                        o.OrderStatus != "Cancelled")
                            .ToListAsync(stoppingToken);

                        if (expiredOrders.Any())
                        {       
                                dbContext.Orders.RemoveRange(expiredOrders);
                                await dbContext.SaveChangesAsync(stoppingToken);
                                Console.WriteLine($"[SmartCafe] Successfully permanent deleted {expiredOrders.Count} expired orders from yesterday.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Error တက်ရင် ဘာလို့တက်လဲ သိအောင် Log မှတ်ထားဖို့ပါ
                    Console.WriteLine($"Error cleaning expired orders: {ex.Message}");
                }
            }
        }
    }
}
