using HotelBooking.Data.Application.Common;
using HotelBooking.Data.Application.Dto;
using HotelBooking.Data.Application.Interfaces;
using HotelBooking.Data.Domain.Entities;
using HotelBooking.Data.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Application.Services;

public class HotelService : IHotelService
{
    private readonly AppDbContext _db;
    public HotelService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<HotelDto>> GetAllAsync(CancellationToken ct)
        => await _db.Hotels.AsNoTracking()
            .OrderBy(h => h.City).ThenBy(h => h.Name)
            .Select(h => new HotelDto(h.Id, h.Name, h.City, h.Address, h.Description))
            .ToListAsync(ct);

    public async Task<HotelDto?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Hotels.AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new HotelDto(h.Id, h.Name, h.City, h.Address, h.Description))
            .FirstOrDefaultAsync(ct);

    public async Task<Guid> CreateAsync(UpsertHotelDto dto, CancellationToken ct)
    {
        Validate(dto);

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            City = dto.City.Trim(),
            Address = dto.Address.Trim(),
            Description = dto.Description?.Trim()
        };

        _db.Hotels.Add(hotel);
        await _db.SaveChangesAsync(ct);
        return hotel.Id;
    }

    public async Task UpdateAsync(Guid id, UpsertHotelDto dto, CancellationToken ct)
    {
        Validate(dto);

        var hotel = await _db.Hotels.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new DomainException("Hotel not found");

        hotel.Name = dto.Name.Trim();
        hotel.City = dto.City.Trim();
        hotel.Address = dto.Address.Trim();
        hotel.Description = dto.Description?.Trim();

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var hotel = await _db.Hotels.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new DomainException("Hotel not found");

        _db.Hotels.Remove(hotel);
        await _db.SaveChangesAsync(ct);
    }

    private static void Validate(UpsertHotelDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new DomainException("Hotel name is required");
        if (string.IsNullOrWhiteSpace(dto.City)) throw new DomainException("City is required");
        if (string.IsNullOrWhiteSpace(dto.Address)) throw new DomainException("Address is required");
    }
}
