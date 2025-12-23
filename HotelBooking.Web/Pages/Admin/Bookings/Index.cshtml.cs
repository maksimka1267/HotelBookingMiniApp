using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Bookings;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public IndexModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    public List<BookingDto> Items { get; private set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Items = (await _gateway.GetAllBookingsAsync()).ToList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
