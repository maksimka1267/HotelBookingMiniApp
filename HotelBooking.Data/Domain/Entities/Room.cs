namespace HotelBooking.Data.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }

    public Guid HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;

    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }

    public List<Booking> Bookings { get; set; } = new();
}
