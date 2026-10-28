using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Data;
using ShopInventory.Api.DTOs.Products;
using ShopInventory.Api.Models.Entities;

namespace ShopInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ShopInventoryContext _context;

    public ProductsController(ShopInventoryContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<ProductPageResponseDto>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        var totalCount = await _context.Products.CountAsync();
        var query = _context.Products.AsNoTracking();

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

        return Ok(new ProductPageResponseDto
        {
            Products = products.Select(ToResponseDto).ToList(),
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Page = page,
            PageSize = pageSize
        });
    }

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(ToResponseDto(product));
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(
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

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            ToResponseDto(product));
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductRequestDto request)
    {
        var existingProduct = await _context.Products.FindAsync(id);

        if (existingProduct is null)
        {
            return NotFound();
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

        return NoContent();
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
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
