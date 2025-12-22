using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotels;

    public HotelsController(IHotelService hotels) => _hotels = hotels;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? city, CancellationToken ct)
    {
        var all = await _hotels.GetAllAsync(ct);
        if (!string.IsNullOrWhiteSpace(city))
            all = all.Where(h => string.Equals(h.City, city.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(all);
    }

    [HttpGet("{hotelId:guid}")]
    public async Task<IActionResult> GetById(Guid hotelId, CancellationToken ct)
    {
        var hotel = await _hotels.GetByIdAsync(hotelId, ct);
        return hotel is null ? NotFound() : Ok(hotel);
    }
}
