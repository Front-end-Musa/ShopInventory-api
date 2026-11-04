using Microsoft.EntityFrameworkCore;
using ShopInventory.Api.Data;
using ShopInventory.Api.DTOs.Categories;
using ShopInventory.Api.Models.Entities;

namespace ShopInventory.Api.Services;

public class CategoriesService
{
    private readonly ShopInventoryContext _context;

    public CategoriesService(ShopInventoryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetCategories()
    {
        var categories = await _context.Category
            .AsNoTracking()
            .Select(category => ToResponse(category))
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryResponseDto?> GetCategory(int id)
    {
        var category = await _context.Category
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

        return category is null ? null : ToResponse(category);
    }

    public async Task<CategoryResponseDto> CreateCategory(CategoryRequestDto request)
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
        return response;
    }

    public async Task<bool> UpdateCategory(int id, CategoryRequestDto request)
    {
        var category = await _context.Category.FindAsync(id);
        if (category is null)
        {
            return false;
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategory(int id)
    {
        var category = await _context.Category.FindAsync(id);
        if (category is null)
        {
            return false;
        }

        _context.Category.Remove(category);
        await _context.SaveChangesAsync();
        return true;
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
