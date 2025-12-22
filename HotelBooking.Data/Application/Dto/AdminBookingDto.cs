namespace HotelBooking.Data.Application.Dto;

public sealed record AdminBookingDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    Guid RoomId,
    string HotelName,
    string City,
    decimal PricePerNight,
    int Capacity,
    DateOnly CheckIn,
    DateOnly CheckOut,
    DateTime CreatedAtUtc
);
