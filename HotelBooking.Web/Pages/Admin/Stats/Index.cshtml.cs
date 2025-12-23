using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Stats;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public IndexModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    [BindProperty(SupportsGet = true)]
    public DateOnly From { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

    [BindProperty(SupportsGet = true)]
    public DateOnly To { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public AdminStatsDto? Stats { get; private set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            if (To < From)
            {
                Error = "Дата 'По' має бути не раніше дати 'З'.";
                return;
            }

            Stats = await _gateway.GetAdminStatsAsync(From, To);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
