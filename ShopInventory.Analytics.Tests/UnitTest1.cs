using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using ShopInventory.Api.Data;
using ShopInventory.Api.Services;
using ShopInventory.Api.Models.Entities;
using ShopInventory.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ShopInventory.Analytics.Tests;
public class AnalyticsTests
{
    [Fact]
    public async Task InventoryTotalsAreGlobalAndStockGroupsDoNotOverlap()
    {
        await using var context = new ShopInventoryContext(new DbContextOptionsBuilder<ShopInventoryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var category = new Category { Id = 1, Name = "Food" };
        context.Category.Add(category);
        context.Products.AddRange(
            new Product { Id = 1, Name = "Bread", Category = category, Price = 2, QuantityInStock = 3, MinimumStockLevel = 5 },
            new Product { Id = 2, Name = "Milk", Category = category, Price = 3, QuantityInStock = 0, MinimumStockLevel = 2 });
        await context.SaveChangesAsync();
        var options = Options.Create(new AnalyticsOptions());
        var service = new AnalyticsService(context, new SalesAnalyticsSource(options, NullLogger<SalesAnalyticsSource>.Instance), options);
        var result = JsonSerializer.SerializeToElement(await service.Overview(7, default), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(6, result.GetProperty("inventory").GetProperty("inventoryValue").GetDecimal());
        Assert.Equal(1, result.GetProperty("inventory").GetProperty("lowStockCount").GetInt32());
        Assert.Equal(1, result.GetProperty("inventory").GetProperty("outOfStockCount").GetInt32());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("sales").GetProperty("revenue").ValueKind);
        Assert.All(result.GetProperty("daily").EnumerateArray(), day => Assert.Equal(JsonValueKind.Null, day.GetProperty("inventoryValue").ValueKind));
        Assert.IsType<BadRequestObjectResult>(await new AnalyticsController(service).Overview(8));
    }

    [Fact]
    public async Task SalesSourceDistinguishesUnavailableFromZeroAndRejectsDuplicates()
    {
        var file = Path.GetTempFileName();
        try
        {
            var options = Options.Create(new AnalyticsOptions { SalesFile = file });
            var source = new SalesAnalyticsSource(options, NullLogger<SalesAnalyticsSource>.Instance);
            Assert.Null(await source.Read(default)); // empty JSON is unavailable
            var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            var export = new SalesExport("USD", "UTC", yesterday.AddDays(-6), yesterday, []);
            await File.WriteAllTextAsync(file, JsonSerializer.Serialize(export, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            Assert.NotNull(await source.Read(default)); // valid empty covered period is zero activity
            export.Daily.Add(new SalesDay(yesterday, -5, -1)); // normalized return
            export.Daily.Add(new SalesDay(yesterday, 10, 2));
            await File.WriteAllTextAsync(file, JsonSerializer.Serialize(export, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            Assert.Null(await source.Read(default));
        }
        finally { File.Delete(file); }
    }
    [Fact]
    public async Task CompleteSalesCoverageReportsZerosAndSnapshotsRemainSparse()
    {
        var file = Path.GetTempFileName();
        try
        {
            var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            var export = new SalesExport("USD", "UTC", yesterday.AddDays(-6), yesterday, []);
            await File.WriteAllTextAsync(file, JsonSerializer.Serialize(export, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            await using var context = new ShopInventoryContext(new DbContextOptionsBuilder<ShopInventoryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            context.InventorySnapshots.Add(new InventorySnapshot { Date = yesterday, Quantity = 3, InventoryValue = 6 });
            await context.SaveChangesAsync();
            var options = Options.Create(new AnalyticsOptions { SalesFile = file });
            var service = new AnalyticsService(context, new SalesAnalyticsSource(options, NullLogger<SalesAnalyticsSource>.Instance), options);
            var result = JsonSerializer.SerializeToElement(await service.Overview(7, default), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.True(result.GetProperty("availability").GetProperty("sales").GetBoolean());
            Assert.Equal(0, result.GetProperty("sales").GetProperty("revenue").GetDecimal());
            var days = result.GetProperty("daily").EnumerateArray().ToArray();
            Assert.Equal(JsonValueKind.Null, days[0].GetProperty("inventoryValue").ValueKind);
            Assert.Equal(6, days[^1].GetProperty("inventoryValue").GetDecimal());
        }
        finally { File.Delete(file); }
    }

}
