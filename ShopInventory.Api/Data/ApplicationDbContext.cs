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
        modelBuilder.Entity<InventorySnapshot>().HasKey(snapshot => snapshot.Date);
        modelBuilder.Entity<InventorySnapshot>().Property(snapshot => snapshot.InventoryValue).HasPrecision(20, 2);
        modelBuilder.Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<InventorySnapshot> InventorySnapshots { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null;
    public DbSet<Category> Category { get; set; } = null;
}