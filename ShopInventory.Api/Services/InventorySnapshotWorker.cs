using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopInventory.Api.Data;
using ShopInventory.Api.Models.Entities;
namespace ShopInventory.Api.Services;

public class InventorySnapshotWorker(IServiceScopeFactory scopes, IOptions<AnalyticsOptions> options,
    ILogger<InventorySnapshotWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.Timezone);
        while (!stoppingToken.IsCancellationRequested)
        {
            var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
            var midnight = DateTime.SpecifyKind(localNow.Date.AddDays(1), DateTimeKind.Unspecified);
            // Advance through a possible DST gap at midnight.
            while (zone.IsInvalidTime(midnight)) midnight = midnight.AddMinutes(1);
            var next = TimeZoneInfo.ConvertTimeToUtc(midnight, zone);
            await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(1, (next - DateTime.UtcNow).TotalMilliseconds)), stoppingToken);
            // Never label a late capture as a past closing balance after suspension.
            if (DateTime.UtcNow - next > TimeSpan.FromMinutes(1)) continue;
            try
            {
                using var scope = scopes.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ShopInventoryContext>();
                var date = DateOnly.FromDateTime(localNow.Date);
                if (await context.InventorySnapshots.AnyAsync(s => s.Date == date, stoppingToken)) continue;
                var totals = await context.Products.AsNoTracking().GroupBy(p => 1)
                    .Select(group => new { Quantity = group.Sum(p => (long)p.QuantityInStock), Value = group.Sum(p => p.Price * p.QuantityInStock) })
                    .SingleOrDefaultAsync(stoppingToken);
                context.InventorySnapshots.Add(new InventorySnapshot { Date = date,
                    Quantity = totals?.Quantity ?? 0, InventoryValue = totals?.Value ?? 0,
                    Currency = options.Value.Currency, Timezone = options.Value.Timezone });
                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Unable to capture closing inventory snapshot; history for this day may be unavailable");
            }
        }
    }
}
