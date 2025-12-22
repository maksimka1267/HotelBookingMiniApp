namespace HotelBooking.Data.Application.Dto;

public sealed record BookingDto(
    Guid Id,
    Guid UserId,
    Guid RoomId,
    string HotelName,
    string City,
    decimal PricePerNight,
    int Capacity,
    DateOnly CheckIn,
    DateOnly CheckOut,
    DateTime CreatedAtUtc
);

public sealed record CreateBookingRequest(
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut
);
