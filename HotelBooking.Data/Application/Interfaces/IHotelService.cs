using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Data.Application.Interfaces;

public interface IHotelService
{
    Task<IReadOnlyList<HotelDto>> GetAllAsync(CancellationToken ct);
    Task<HotelDto?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Guid> CreateAsync(UpsertHotelDto dto, CancellationToken ct);
    Task UpdateAsync(Guid id, UpsertHotelDto dto, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
