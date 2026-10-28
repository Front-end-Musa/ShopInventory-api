namespace ShopInventory.Api.Models.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal CostPrice { get; set; }

    // Category relationship
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Inventory
    public int QuantityInStock { get; set; }

    public int MinimumStockLevel { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}