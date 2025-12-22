using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Data.Application.Interfaces;

public interface IStatsService
{
    Task<AdminStatsDto> GetHotelStatsAsync(DateOnly from, DateOnly to, CancellationToken ct);
}
