using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using HotelBooking.Data.Domain.Entities;
using HotelBooking.Data.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Application.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;
    public RoomService(AppDbContext db) => _db = db;

    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Rooms.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoomDto(r.Id, r.HotelId, r.Hotel.Name, r.Hotel.City, r.PricePerNight, r.Capacity))
            .FirstOrDefaultAsync(ct);

    public async Task<Guid> CreateAsync(UpsertRoomDto dto, CancellationToken ct)
    {
        Validate(dto);

        var hotelExists = await _db.Hotels.AnyAsync(h => h.Id == dto.HotelId, ct);
        if (!hotelExists) throw new DomainException("Hotel not found");

        var room = new Room
        {
            Id = Guid.NewGuid(),
            HotelId = dto.HotelId,
            PricePerNight = dto.PricePerNight,
            Capacity = dto.Capacity
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);
        return room.Id;
    }

    public async Task UpdateAsync(Guid id, UpsertRoomDto dto, CancellationToken ct)
    {
        Validate(dto);

        var room = await _db.Rooms.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new DomainException("Room not found");

        var hotelExists = await _db.Hotels.AnyAsync(h => h.Id == dto.HotelId, ct);
        if (!hotelExists) throw new DomainException("Hotel not found");

        room.HotelId = dto.HotelId;
        room.PricePerNight = dto.PricePerNight;
        room.Capacity = dto.Capacity;

        await _db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<RoomDto>> GetAllAsync(Guid? hotelId, CancellationToken ct)
    {
        var q = _db.Rooms
            .AsNoTracking()
            .AsQueryable();

        if (hotelId.HasValue)
            q = q.Where(r => r.HotelId == hotelId.Value);

        return await q
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.PricePerNight)
            .Select(r => new RoomDto(
                r.Id,
                r.HotelId,
                r.Hotel.Name,
                r.Hotel.City,
                r.PricePerNight,
                r.Capacity
            ))
            .ToListAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new DomainException("Room not found");

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RoomDto>> SearchAvailableAsync(SearchRoomsRequest req, CancellationToken ct)
    {
        Validate(req);

        // конфликт брони: req.CheckIn < b.CheckOut && req.CheckOut > b.CheckIn
        return await _db.Rooms.AsNoTracking()
            .Where(r => r.Hotel.City == req.City.Trim())
            .Where(r => req.MinCapacity == null || r.Capacity >= req.MinCapacity.Value)
            .Where(r => !r.Bookings.Any(b => req.CheckIn < b.CheckOut && req.CheckOut > b.CheckIn))
            .OrderBy(r => r.PricePerNight)
            .Select(r => new RoomDto(r.Id, r.HotelId, r.Hotel.Name, r.Hotel.City, r.PricePerNight, r.Capacity))
            .ToListAsync(ct);
    }

    private static void Validate(UpsertRoomDto dto)
    {
        if (dto.HotelId == Guid.Empty) throw new DomainException("HotelId is required");
        if (dto.PricePerNight <= 0) throw new DomainException("PricePerNight must be > 0");
        if (dto.Capacity <= 0) throw new DomainException("Capacity must be > 0");
    }

    private static void Validate(SearchRoomsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.City)) throw new DomainException("City is required");
        if (req.CheckIn >= req.CheckOut) throw new DomainException("CheckIn must be < CheckOut");
    }
}
