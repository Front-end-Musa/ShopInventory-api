namespace ShopInventory.Api.Models.Entities;

public class Sale
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }

    // Sale items
    public ICollection<SaleItem> Items { get; set; }
        = new List<SaleItem>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    MobilePayment = 3,
    BankTransfer = 4
}