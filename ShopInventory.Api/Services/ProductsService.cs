using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Data;
using ShopInventory.Api.DTOs.Products;
using ShopInventory.Api.Models.Entities;

namespace ShopInventory.Api.Services;

public class ProductsService
{
    private readonly ShopInventoryContext _context;

    public ProductsService(ShopInventoryContext context)
    {
        _context = context;
    }

    // GET: api/products
    public async Task<ProductPageResponseDto> GetProducts(
        int page = 1,
        int pageSize = 10,
        string sortBy = "name",
        string sortOrder = "asc",
        string searchInput = "")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        var totalCount = await _context.Products.CountAsync();
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchInput))
        {
            var term = searchInput.Trim();

            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                EF.Functions.ILike(p.SKU, $"%{term}%") ||
                EF.Functions.ILike(p.Description!, $"%{term}%"));
        }

        query = sortBy.ToLowerInvariant() switch
        {
            "price" => descending
                ? query.OrderByDescending(product => product.Price).ThenBy(product => product.Id)
                : query.OrderBy(product => product.Price).ThenBy(product => product.Id),
            "stock" => descending
                ? query.OrderByDescending(product => product.QuantityInStock).ThenBy(product => product.Id)
                : query.OrderBy(product => product.QuantityInStock).ThenBy(product => product.Id),
            _ => descending
                ? query.OrderByDescending(product => product.Name).ThenBy(product => product.Id)
                : query.OrderBy(product => product.Name).ThenBy(product => product.Id)
        };

        var products = await query
            .Include(product => product.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductPageResponseDto
        {
            Products = products.Select(ToResponseDto).ToList(),
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Page = page,
            PageSize = pageSize,
            SearchInput = searchInput
        };
    }

    // GET: api/products/5
    public async Task<ProductResponseDto?> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return null;
        }

        return ToResponseDto(product);
    }

    // POST: api/products
    public async Task<ProductResponseDto> CreateProduct(
        ProductRequestDto request)
    {
        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            Barcode = request.Barcode,
            Description = request.Description,
            Price = request.Price,
            CostPrice = request.CostPrice,
            CategoryId = request.CategoryId,
            QuantityInStock = request.QuantityInStock,
            MinimumStockLevel = request.MinimumStockLevel,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(product => product.Category).LoadAsync();

        return ToResponseDto(product);
    }

    // PUT: api/products/5
    public async Task<bool> UpdateProduct(int id, ProductRequestDto request)
    {
        var existingProduct = await _context.Products.FindAsync(id);

        if (existingProduct is null)
        {
            return false;
        }

        existingProduct.Name = request.Name;
        existingProduct.SKU = request.SKU;
        existingProduct.Barcode = request.Barcode;
        existingProduct.Description = request.Description;
        existingProduct.Price = request.Price;
        existingProduct.CostPrice = request.CostPrice;
        existingProduct.CategoryId = request.CategoryId;
        existingProduct.QuantityInStock = request.QuantityInStock;
        existingProduct.MinimumStockLevel = request.MinimumStockLevel;
        existingProduct.IsActive = request.IsActive;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductResponseDto ToResponseDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        SKU = product.SKU,
        Barcode = product.Barcode,
        Description = product.Description,
        Price = product.Price,
        CostPrice = product.CostPrice,
        CategoryId = product.CategoryId,
        Category = new()
        {
            Id = product.Category.Id,
            Name = product.Category.Name,
            Description = product.Category.Description,
            CreatedAt = product.Category.CreatedAt,
            UpdatedAt = product.Category.UpdatedAt
        },
        QuantityInStock = product.QuantityInStock,
        MinimumStockLevel = product.MinimumStockLevel,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };

    // DELETE: api/products/5
    public async Task<bool> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }
}
