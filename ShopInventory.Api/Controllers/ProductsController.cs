using Microsoft.AspNetCore.Mvc;
using ShopInventory.Api.DTOs.Products;
using ShopInventory.Api.Services;

namespace ShopInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _service;

    public ProductsController(ProductsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ProductPageResponseDto>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string searchInput = "")
    {
        return Ok(await _service.GetProducts(page, pageSize, sortBy, sortOrder, searchInput));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var response = await _service.GetProduct(id);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductRequestDto request)
    {
        var response = await _service.CreateProduct(request);
        return CreatedAtAction(nameof(GetProduct), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductRequestDto request)
    {
        return await _service.UpdateProduct(id, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        return await _service.DeleteProduct(id) ? NoContent() : NotFound();
    }
}
