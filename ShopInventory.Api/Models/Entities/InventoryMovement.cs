namespace ShopInventory.Api.Models.Entities;

public class InventoryMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    // Positive = stock added; negative = stock removed
    public int QuantityChange { get; set; }

    public MovementType MovementType { get; set; }

    public string? Reason { get; set; }

    // Optional reference to a related transaction
    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Notes { get; set; }

    // User who performed the operation
    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum MovementType
{
    Purchase = 1,
    Sale = 2,
    Adjustment = 3,
    Transfer = 4,
    Return = 5,
    Damaged = 6
}