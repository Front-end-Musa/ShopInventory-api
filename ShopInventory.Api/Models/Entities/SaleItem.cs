namespace ShopInventory.Api.Models.Entities;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    // Snapshot of the selling price at the time of sale
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}