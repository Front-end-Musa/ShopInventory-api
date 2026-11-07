namespace ShopInventory.Api.Services;
public class AnalyticsOptions
{
    public string Currency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
    // Optional normalized export supplied by the shop's sales integration.
    public string? SalesFile { get; set; }
}
