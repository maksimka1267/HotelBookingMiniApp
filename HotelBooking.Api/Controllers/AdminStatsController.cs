using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IStatsService _stats;

    public AdminStatsController(IStatsService stats) => _stats = stats;

    [HttpGet("hotels")]
    public async Task<IActionResult> Hotels([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        var res = await _stats.GetHotelStatsAsync(from, to, ct);
        return Ok(res);
    }
}
