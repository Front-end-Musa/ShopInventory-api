namespace ShopInventory.Api.Models.Entities;

public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        = new List<PurchaseOrder>();

    public ICollection<Product> Products { get; set; }
        = new List<Product>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}