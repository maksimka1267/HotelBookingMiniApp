namespace HotelBooking.Data.Application.Dto;

public sealed record RoomDto(
    Guid Id,
    Guid HotelId,
    string HotelName,
    string City,
    decimal PricePerNight,
    int Capacity
);

public sealed record UpsertRoomDto(
    Guid HotelId,
    decimal PricePerNight,
    int Capacity
);
