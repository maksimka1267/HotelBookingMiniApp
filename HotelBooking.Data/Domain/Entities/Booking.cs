using HotelBooking.Data.Infrastructure.Identity;

namespace HotelBooking.Data.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
