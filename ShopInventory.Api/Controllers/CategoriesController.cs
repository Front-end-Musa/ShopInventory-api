using Microsoft.AspNetCore.Mvc;
using ShopInventory.Api.DTOs.Categories;
using ShopInventory.Api.Services;

namespace ShopInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoriesService _service;

    public CategoriesController(CategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        return Ok(await _service.GetCategories());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
    {
        var response = await _service.GetCategory(id);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryRequestDto request)
    {
        var response = await _service.CreateCategory(request);
        return CreatedAtAction(nameof(GetCategory), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequestDto request)
    {
        return await _service.UpdateCategory(id, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _service.DeleteCategory(id);
        return result switch
        {
            CategoryDeletionResult.Deleted => NoContent(),
            CategoryDeletionResult.NotFound => NotFound(),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Category contains products",
                detail: "Reassign or remove all products before deleting this category.")
        };
    }
}
