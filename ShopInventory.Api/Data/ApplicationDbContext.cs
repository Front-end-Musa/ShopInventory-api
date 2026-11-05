using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Models.Entities;
namespace ShopInventory.Api.Data;

public class ShopInventoryContext: DbContext
{
    public ShopInventoryContext(DbContextOptions<ShopInventoryContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<Product> Products { get; set; } = null;
    public DbSet<Category> Category { get; set; } = null;
}