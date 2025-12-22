using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Hotels;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public IndexModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    public List<HotelDto> Items { get; private set; } = new();
    public string? Error { get; set; }

    [BindProperty]
    public Guid Id { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Items = (await _gateway.GetHotelsAsync()).ToList();
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

                await _gateway.DeleteHotelAsync(Id);
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Items = (await _gateway.GetHotelsAsync()).ToList();
            return Page();
        }
    }
}
