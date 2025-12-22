using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Data.Application.Interfaces;

public interface IRoomService
{
    Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Guid> CreateAsync(UpsertRoomDto dto, CancellationToken ct);
    Task UpdateAsync(Guid id, UpsertRoomDto dto, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<RoomDto>> SearchAvailableAsync(SearchRoomsRequest req, CancellationToken ct);
    Task<IReadOnlyList<RoomDto>> GetAllAsync(Guid? hotelId, CancellationToken ct);

}
