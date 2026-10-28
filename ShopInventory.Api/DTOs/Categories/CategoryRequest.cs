using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Api.DTOs.Categories;

public class CategoryRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
