using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Data.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public List<Domain.Entities.Booking> Bookings { get; set; } = new();
}
