namespace ShopInventory.Api.Models.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public PurchaseOrderStatus Status { get; set; }
        = PurchaseOrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    // Order items
    public ICollection<PurchaseOrderItem> Items { get; set; }
        = new List<PurchaseOrderItem>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

public enum PurchaseOrderStatus
{
    Pending = 1,
    Received = 2,
    Cancelled = 3
}