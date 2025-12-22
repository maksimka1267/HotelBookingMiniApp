using System.ComponentModel.DataAnnotations;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Rooms;

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

    public List<HotelDto> Hotels { get; private set; } = new();

    [BindProperty, Required]
    public Guid HotelId { get; set; }

    [BindProperty, Range(1, 100)]
    public int Capacity { get; set; } = 2;

    [BindProperty, Range(0, 1_000_000)]
    public decimal PricePerNight { get; set; } = 1000;

    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Hotels = (await _gateway.GetHotelsAsync()).ToList();

        if (!IsEdit) return Page();

        var dto = await _gateway.GetRoomByIdAsync(Id!.Value);
        if (dto is null) return NotFound();

        HotelId = dto.HotelId;
        Capacity = dto.Capacity;
        PricePerNight = dto.PricePerNight;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] string handler)
    {
        handler = (handler ?? "").Trim().ToLowerInvariant();

        if (handler != "save")
            return BadRequest("Unknown handler");

        Hotels = (await _gateway.GetHotelsAsync()).ToList();

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
        var dto = new UpsertRoomDto(HotelId, PricePerNight, Capacity);

        if (IsEdit)
        {
            await _gateway.UpdateRoomAsync(Id!.Value, dto);
            Success = "Зміни збережено ✅";
            return Page();
        }

        await _gateway.CreateRoomAsync(dto);
        return RedirectToPage("/Admin/Rooms/Index");
    }
}
