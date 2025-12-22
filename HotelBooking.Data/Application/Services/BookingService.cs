using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using HotelBooking.Data.Domain.Entities;
using HotelBooking.Data.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Application.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    public BookingService(AppDbContext db) => _db = db;

    public async Task<Guid> CreateBookingAsync(Guid userId, CreateBookingRequest req, CancellationToken ct)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required");
        if (req.RoomId == Guid.Empty) throw new DomainException("RoomId is required");
        if (req.CheckIn >= req.CheckOut) throw new DomainException("CheckIn must be < CheckOut");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var roomExists = await _db.Rooms.AnyAsync(r => r.Id == req.RoomId, ct);
        if (!roomExists) throw new DomainException("Room not found");

        var hasConflict = await _db.Bookings.AnyAsync(b =>
            b.RoomId == req.RoomId &&
            req.CheckIn < b.CheckOut &&
            req.CheckOut > b.CheckIn
        , ct);

        if (hasConflict) throw new DomainException("Room is not available for selected dates");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoomId = req.RoomId,
            CheckIn = req.CheckIn,
            CheckOut = req.CheckOut,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return booking.Id;
    }

    public async Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required");

        return await _db.Bookings.AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BookingDto(
                b.Id,
                b.UserId,
                b.RoomId,
                b.Room.Hotel.Name,
                b.Room.Hotel.City,
                b.Room.PricePerNight,
                b.Room.Capacity,
                b.CheckIn,
                b.CheckOut,
                b.CreatedAtUtc
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct)
        => await _db.Bookings.AsNoTracking()
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BookingDto(
                b.Id,
                b.UserId,
                b.RoomId,
                b.Room.Hotel.Name,
                b.Room.Hotel.City,
                b.Room.PricePerNight,
                b.Room.Capacity,
                b.CheckIn,
                b.CheckOut,
                b.CreatedAtUtc
            ))
            .ToListAsync(ct);
}
