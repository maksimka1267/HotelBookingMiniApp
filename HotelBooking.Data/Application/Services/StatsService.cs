using Dapper;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using HotelBooking.Data.Infrastructure.Dapper;

namespace HotelBooking.Data.Application.Services;

public class StatsService : IStatsService
{
    private readonly IDbConnectionFactory _factory;
    public StatsService(IDbConnectionFactory factory) => _factory = factory;

    public async Task<AdminStatsDto> GetHotelStatsAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        if (from >= to) throw new ArgumentException("from must be < to");

        const string sql = @"
SELECT
    h.Id AS HotelId,
    h.Name AS HotelName,
    h.City AS City,
    CAST(COUNT(DISTINCT b.Id) AS SIGNED) AS BookingsCount,
    CAST(COALESCE(SUM(DATEDIFF(b.CheckOut, b.CheckIn)), 0) AS SIGNED) AS TotalNights,
    CAST(COALESCE(SUM(DATEDIFF(b.CheckOut, b.CheckIn) * r.PricePerNight), 0) AS DECIMAL(18,2)) AS Revenue
FROM Hotels h
LEFT JOIN Rooms r ON r.HotelId = h.Id
LEFT JOIN Bookings b ON b.RoomId = r.Id
    AND b.CheckIn >= @FromDate
    AND b.CheckOut <= @ToDate
GROUP BY h.Id, h.Name, h.City
ORDER BY Revenue DESC, BookingsCount DESC;";

        using var conn = _factory.Create();
        var rows = (await conn.QueryAsync<AdminHotelStatsRow>(
            new CommandDefinition(sql, new
            {
                FromDate = from.ToDateTime(TimeOnly.MinValue),
                ToDate = to.ToDateTime(TimeOnly.MinValue)
            }, cancellationToken: ct)
        )).ToList().AsReadOnly();

        return new AdminStatsDto(from, to, rows);

    }
}
