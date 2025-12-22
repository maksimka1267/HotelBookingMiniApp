namespace HotelBooking.Data.Application.Dto;

public sealed record AdminHotelStatsRow(
    Guid HotelId,
    string HotelName,
    string City,
    long BookingsCount,
    long TotalNights,
    decimal Revenue
);
public sealed record AdminStatsDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<AdminHotelStatsRow> ByHotels
);
