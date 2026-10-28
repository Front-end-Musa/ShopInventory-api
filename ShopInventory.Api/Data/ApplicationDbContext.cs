using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Models.Entities;
namespace ShopInventory.Api.Data;

public class ShopInventoryContext: DbContext
{
    public ShopInventoryContext(DbContextOptions<ShopInventoryContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null;
    public DbSet<Category> Category { get; set; } = null;
}