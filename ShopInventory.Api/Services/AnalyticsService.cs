using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopInventory.Api.Data;

namespace ShopInventory.Api.Services;

public class AnalyticsService(ShopInventoryContext context, SalesAnalyticsSource salesSource, IOptions<AnalyticsOptions> options)
{
    public async Task<object> Overview(int days, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var now = DateTimeOffset.UtcNow;
        var end = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, TimeZoneInfo.FindSystemTimeZoneById(settings.Timezone)).DateTime).AddDays(-1);
        var start = end.AddDays(1 - days);
        var products = await context.Products.AsNoTracking().Include(p => p.Category).ToListAsync(cancellationToken);
        var categories = await context.Category.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        var snapshots = await context.InventorySnapshots.AsNoTracking()
            .Where(s => s.Date >= start && s.Date <= end && s.Currency == settings.Currency && s.Timezone == settings.Timezone)
            .ToDictionaryAsync(s => s.Date, cancellationToken);
        var source = await salesSource.Read(cancellationToken);
        var sales = source?.Daily.ToDictionary(day => day.Date);
        var daily = Enumerable.Range(0, days).Select(index =>
        {
            var date = start.AddDays(index);
            snapshots.TryGetValue(date, out var snapshot);
            var covered = source is not null && date >= source.StartDate && date <= source.EndDate;
            SalesDay? sale = null;
            sales?.TryGetValue(date, out sale);
            return new { Date = date, Quantity = (long?)snapshot?.Quantity, InventoryValue = (decimal?)snapshot?.InventoryValue,
                Revenue = covered ? (decimal?)(sale?.Revenue ?? 0) : null,
                UnitsSold = covered ? (long?)(sale?.UnitsSold ?? 0) : null };
        }).ToArray();
        var allSales = daily.All(day => day.Revenue.HasValue);
        return new
        {
            settings.Currency, settings.Timezone, GeneratedAt = now, StartDate = start, EndDate = end,
            Availability = new { Inventory = true, History = snapshots.Count == days, Sales = allSales,
                SalesReason = allSales ? null : "Sales data is not connected or does not cover the complete reporting period." },
            Inventory = new { ProductCount = products.Count, CategoryCount = categories.Count,
                InventoryValue = products.Sum(p => p.Price * p.QuantityInStock),
                InStockCount = products.Count(p => p.QuantityInStock > 0 && p.QuantityInStock >= p.MinimumStockLevel),
                LowStockCount = products.Count(p => p.QuantityInStock > 0 && p.QuantityInStock < p.MinimumStockLevel),
                OutOfStockCount = products.Count(p => p.QuantityInStock == 0) },
            Sales = new { Revenue = allSales ? (decimal?)daily.Sum(day => day.Revenue ?? 0) : null,
                UnitsSold = allSales ? (long?)daily.Sum(day => day.UnitsSold ?? 0) : null },
            Categories = categories.Select(c => new { c.Id, c.Name, ProductCount = products.Count(p => p.CategoryId == c.Id),
                Quantity = products.Where(p => p.CategoryId == c.Id).Sum(p => (long)p.QuantityInStock),
                InventoryValue = products.Where(p => p.CategoryId == c.Id).Sum(p => p.Price * p.QuantityInStock) }),
            LowStockProducts = products.Where(p => p.QuantityInStock == 0 || p.QuantityInStock < p.MinimumStockLevel)
                .OrderBy(p => p.QuantityInStock).ThenBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.QuantityInStock, p.MinimumStockLevel }),
            Daily = daily
        };
    }
}
