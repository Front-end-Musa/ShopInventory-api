using ShopInventory.Api.DTOs.Categories;

namespace ShopInventory.Api.DTOs.Products;

public class ProductResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal CostPrice { get; set; }

    public int CategoryId { get; set; }

    public CategoryResponseDto Category { get; set; } = null!;

    public int QuantityInStock { get; set; }

    public int MinimumStockLevel { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
