using System.Security.Claims;
using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Client/Admin — достаточно
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest req, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        try
        {
            var id = await _bookings.CreateBookingAsync(userId, req, ct);
            return Ok(new { bookingId = id });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var list = await _bookings.GetMyBookingsAsync(userId, ct);
        return Ok(list);
    }
}
