namespace HotelBooking.Data.Domain.Entities;

public class Hotel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }

    public List<Room> Rooms { get; set; } = new();
}
