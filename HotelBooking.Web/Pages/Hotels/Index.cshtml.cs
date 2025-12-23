using System.ComponentModel.DataAnnotations;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Hotels;

public class IndexModel : PageModel
{
    private readonly IGatewayClient _gateway;

    public IndexModel(IGatewayClient gateway)
    {
        _gateway = gateway;
    }

    [BindProperty]
    public string? City { get; set; }

    [BindProperty, DataType(DataType.Date)]
    public DateOnly CheckIn { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [BindProperty, DataType(DataType.Date)]
    public DateOnly CheckOut { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

    [BindProperty]
    public int? MinCapacity { get; set; }

    [BindProperty]
    public Guid RoomId { get; set; }

    public List<RoomDto> Results { get; private set; } = new();

    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync()
    {
        // можно не грузить ничего по умолчанию
        await Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync([FromForm] string handler)
    {
        handler = (handler ?? "").Trim().ToLowerInvariant();

        try
        {
            return handler switch
            {
                "search" => await DoSearchAsync(),
                "book" => await DoBookAsync(),
                _ => BadRequest("Unknown handler")
            };
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            // сохраняем результаты, если поиск уже был
            if (CheckOut > CheckIn)
                Results = (await _gateway.SearchRoomsAsync(new SearchRoomsRequest(City, CheckIn, CheckOut, MinCapacity))).ToList();
            return Page();
        }
    }

    private async Task<IActionResult> DoSearchAsync()
    {
        if (CheckOut <= CheckIn)
        {
            Error = "Дата виїзду має бути пізніше дати заїзду.";
            return Page();
        }

        var req = new SearchRoomsRequest(City, CheckIn, CheckOut, MinCapacity);
        Results = (await _gateway.SearchRoomsAsync(req)).ToList();
        return Page();
    }

    private async Task<IActionResult> DoBookAsync()
    {
        if (RoomId == Guid.Empty)
        {
            Error = "Некоректний номер.";
            return await DoSearchAsync();
        }

        if (CheckOut <= CheckIn)
        {
            Error = "Дата виїзду має бути пізніше дати заїзду.";
            return Page();
        }

        await _gateway.CreateBookingAsync(new CreateBookingRequest(RoomId, CheckIn, CheckOut));

        Success = "Бронювання створено ✅";
        // обновим выдачу
        return await DoSearchAsync();
    }
}
