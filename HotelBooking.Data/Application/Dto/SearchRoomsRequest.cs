namespace HotelBooking.Data.Application.Dto;

public sealed record SearchRoomsRequest(
    string City,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int? MinCapacity = null
);
