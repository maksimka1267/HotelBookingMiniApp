using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Bookings;

public class MyModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public MyModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    public List<BookingDto> Items { get; private set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Items = (await _gateway.GetMyBookingsAsync()).ToList();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
