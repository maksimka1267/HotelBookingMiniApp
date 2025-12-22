using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/admin/hotels")]
[Authorize(Roles = "Admin")]
public class AdminHotelsController : ControllerBase
{
    private readonly IHotelService _hotels;

    public AdminHotelsController(IHotelService hotels) => _hotels = hotels;

    [HttpPost]
    public async Task<IActionResult> Create(UpsertHotelDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _hotels.CreateAsync(dto, ct);
            return Ok(new { hotelId = id });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{hotelId:guid}")]
    public async Task<IActionResult> Update(Guid hotelId, UpsertHotelDto dto, CancellationToken ct)
    {
        try
        {
            await _hotels.UpdateAsync(hotelId, dto, ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{hotelId:guid}")]
    public async Task<IActionResult> Delete(Guid hotelId, CancellationToken ct)
    {
        try
        {
            await _hotels.DeleteAsync(hotelId, ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
