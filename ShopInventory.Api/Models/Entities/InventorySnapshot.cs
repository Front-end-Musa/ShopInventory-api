namespace ShopInventory.Api.Models.Entities;

// A single closing aggregate per business day. Missing days remain missing.
public class InventorySnapshot
{
    public DateOnly Date { get; set; }
    public long Quantity { get; set; }
    public decimal InventoryValue { get; set; }
    public string Currency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
}
