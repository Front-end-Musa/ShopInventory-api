using Microsoft.AspNetCore.Mvc;
using ShopInventory.Api.Services;
namespace ShopInventory.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(AnalyticsService service) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        if (days is not (7 or 30 or 90)) return BadRequest(new { message = "Days must be 7, 30, or 90." });
        return Ok(await service.Overview(days, cancellationToken));
    }
}
