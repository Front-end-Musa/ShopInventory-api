using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Api.DTOs.Products;

public class ProductRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SKU { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CostPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStockLevel { get; set; }

    public bool IsActive { get; set; } = true;
}
