using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Data;
using ShopInventory.Api.DTOs.Categories;
using ShopInventory.Api.Models.Entities;

namespace ShopInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ShopInventoryContext _context;

    public CategoriesController(ShopInventoryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _context.Category
            .AsNoTracking()
            .Select(category => ToResponse(category))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
    {
        var category = await _context.Category
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

        return category is null ? NotFound() : Ok(ToResponse(category));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryRequestDto request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Category.Add(category);
        await _context.SaveChangesAsync();

        var response = ToResponse(category);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequestDto request)
    {
        var category = await _context.Category.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Category.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        _context.Category.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static CategoryResponseDto ToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        CreatedAt = category.CreatedAt,
        UpdatedAt = category.UpdatedAt
    };
}
