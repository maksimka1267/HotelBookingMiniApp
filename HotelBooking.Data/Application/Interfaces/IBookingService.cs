using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Data.Application.Interfaces;

public interface IBookingService
{
    Task<Guid> CreateBookingAsync(Guid userId, CreateBookingRequest req, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct);

    // admin
    Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct);
}
