namespace HotelBooking.Data.Application.Dto;

public sealed record HotelDto(
    Guid Id,
    string Name,
    string City,
    string Address,
    string? Description
);

public sealed record UpsertHotelDto(
    string Name,
    string City,
    string Address,
    string? Description
);
