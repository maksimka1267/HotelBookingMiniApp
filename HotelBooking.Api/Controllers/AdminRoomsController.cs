using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/admin/rooms")]
[Authorize(Roles = "Admin")]
public class AdminRoomsController : ControllerBase
{
    private readonly IRoomService _rooms;

    public AdminRoomsController(IRoomService rooms) => _rooms = rooms;

    [HttpPost]
    public async Task<IActionResult> Create(UpsertRoomDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _rooms.CreateAsync(dto, ct);
            return Ok(new { roomId = id });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{roomId:guid}")]
    public async Task<IActionResult> Update(Guid roomId, UpsertRoomDto dto, CancellationToken ct)
    {
        try
        {
            await _rooms.UpdateAsync(roomId, dto, ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> Delete(Guid roomId, CancellationToken ct)
    {
        try
        {
            await _rooms.DeleteAsync(roomId, ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
