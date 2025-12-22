using System.ComponentModel.DataAnnotations;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Hotels;

[Authorize(Policy = "AdminOnly")]
public class UpsertModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public UpsertModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;

    [BindProperty, Required, MinLength(2)]
    public string Name { get; set; } = "";

    [BindProperty, Required, MinLength(2)]
    public string City { get; set; } = "";

    [BindProperty, Required, MinLength(3)]
    public string Address { get; set; } = "";

    [BindProperty]
    public string? Description { get; set; }

    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsEdit) return Page();

        var dto = await _gateway.GetHotelByIdAsync(Id!.Value);
        if (dto is null) return NotFound();

        Name = dto.Name;
        City = dto.City;
        Address = dto.Address;
        Description = dto.Description;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] string handler)
    {
        handler = (handler ?? "").Trim().ToLowerInvariant();

        if (handler != "save")
            return BadRequest("Unknown handler");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            return await DoSaveAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }
    }

    private async Task<IActionResult> DoSaveAsync()
    {
        var dto = new UpsertHotelDto(Name.Trim(), City.Trim(), Address.Trim(), string.IsNullOrWhiteSpace(Description) ? null : Description.Trim());

        if (IsEdit)
        {
            await _gateway.UpdateHotelAsync(Id!.Value, dto);
            Success = "Зміни збережено ✅";
            return Page();
        }

        await _gateway.CreateHotelAsync(dto);
        return RedirectToPage("/Admin/Hotels/Index");
    }
}
