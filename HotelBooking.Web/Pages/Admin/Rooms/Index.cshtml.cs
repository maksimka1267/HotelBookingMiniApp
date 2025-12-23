using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Rooms;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public IndexModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    public List<HotelDto> Hotels { get; private set; } = new();
    public List<RoomDto> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid? HotelId { get; set; }

    [BindProperty]
    public Guid Id { get; set; }

    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Hotels = (await _gateway.GetHotelsAsync()).ToList();
            Items = (await _gateway.GetRoomsAsync(HotelId)).ToList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    public async Task<IActionResult> OnPostAsync([FromForm] string handler)
    {
        handler = (handler ?? "").Trim().ToLowerInvariant();

        try
        {
            if (handler == "delete")
            {
                if (Id == Guid.Empty)
                    return BadRequest("Invalid id");

                await _gateway.DeleteRoomAsync(Id);
            }

            return RedirectToPage(new { hotelId = HotelId });
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Hotels = (await _gateway.GetHotelsAsync()).ToList();
            Items = (await _gateway.GetRoomsAsync(HotelId)).ToList();
            return Page();
        }
    }
}
