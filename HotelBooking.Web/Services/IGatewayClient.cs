using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Web.Services;

public interface IGatewayClient
{
    Task<string> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<string> RegisterAsync(string email, string password, CancellationToken ct = default);

    // Hotels
    Task<IReadOnlyList<HotelDto>> GetHotelsAsync(CancellationToken ct = default);
    Task<HotelDto?> GetHotelByIdAsync(Guid id, CancellationToken ct = default);

    // Rooms
    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(Guid? hotelId = null, CancellationToken ct = default);
    Task<RoomDto?> GetRoomByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<RoomDto>> SearchRoomsAsync(SearchRoomsRequest request, CancellationToken ct = default);

    // Bookings
    Task CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(CancellationToken ct = default);

    // Admin
    Task CreateHotelAsync(UpsertHotelDto dto, CancellationToken ct = default);
    Task UpdateHotelAsync(Guid id, UpsertHotelDto dto, CancellationToken ct = default);
    Task DeleteHotelAsync(Guid id, CancellationToken ct = default);

    Task CreateRoomAsync(UpsertRoomDto dto, CancellationToken ct = default);
    Task UpdateRoomAsync(Guid id, UpsertRoomDto dto, CancellationToken ct = default);
    Task DeleteRoomAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct = default); // если у тебя пока нет AdminBookingDto
    Task<AdminStatsDto> GetAdminStatsAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
