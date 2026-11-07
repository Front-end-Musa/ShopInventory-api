using System.Text.Json;
using Microsoft.Extensions.Options;
namespace ShopInventory.Api.Services;

public record SalesDay([property: System.Text.Json.Serialization.JsonRequired] DateOnly Date,
    [property: System.Text.Json.Serialization.JsonRequired] decimal Revenue,
    [property: System.Text.Json.Serialization.JsonRequired] long UnitsSold);
public record SalesExport([property: System.Text.Json.Serialization.JsonRequired] string Currency,
    [property: System.Text.Json.Serialization.JsonRequired] string Timezone,
    [property: System.Text.Json.Serialization.JsonRequired] DateOnly StartDate,
    [property: System.Text.Json.Serialization.JsonRequired] DateOnly EndDate,
    [property: System.Text.Json.Serialization.JsonRequired] List<SalesDay> Daily);

// Source adapters must normalize net revenue (excluding tax/shipping, including returns)
// before publishing this atomic daily export. Coverage means absent days are real zeros.
public class SalesAnalyticsSource(IOptions<AnalyticsOptions> options, ILogger<SalesAnalyticsSource> logger)
{
    public async Task<SalesExport?> Read(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SalesFile)) return null;
        try
        {
            await using var stream = File.OpenRead(settings.SalesFile);
            var export = await JsonSerializer.DeserializeAsync<SalesExport>(stream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(settings.Timezone)).DateTime);
            if (export is null || export.Currency != settings.Currency || export.Timezone != settings.Timezone
                || export.StartDate > export.EndDate || export.EndDate >= today || export.Daily is null
                || export.Daily.Any(day => day.Date < export.StartDate || day.Date > export.EndDate)
                || export.Daily.Select(day => day.Date).Distinct().Count() != export.Daily.Count)
                return null;
            return export;
        }
        catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException)
        {
            logger.LogWarning(error, "Sales analytics source is unavailable");
            return null;
        }
    }
}
