using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomsController(IRoomService rooms) => _rooms = rooms;

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string city,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int? minCapacity,
        CancellationToken ct)
    {
        try
        {
            var req = new SearchRoomsRequest(city, checkIn, checkOut, minCapacity);
            var result = await _rooms.SearchAvailableAsync(req, ct);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetById(Guid roomId, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        return room is null ? NotFound() : Ok(room);
    }
}
