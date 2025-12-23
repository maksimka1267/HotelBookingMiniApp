using HotelBooking.Data.Domain.Entities;
using HotelBooking.Data.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Hotel>(e =>
        {
            e.ToTable("Hotels");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnType("char(36)");

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.Address).HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);

            e.HasIndex(x => x.City);
        });

        b.Entity<Room>(e =>
        {
            e.ToTable("Rooms");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnType("char(36)");
            e.Property(x => x.HotelId).HasColumnType("char(36)");

            e.Property(x => x.PricePerNight).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Capacity).IsRequired();

            e.HasOne(x => x.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.HotelId, x.Capacity });
        });

        b.Entity<Booking>(e =>
        {
            e.ToTable("Bookings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnType("char(36)");
            e.Property(x => x.UserId).HasColumnType("char(36)");
            e.Property(x => x.RoomId).HasColumnType("char(36)");

            e.Property(x => x.CheckIn)
                .HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v))
                .HasColumnType("date")
                .IsRequired();

            e.Property(x => x.CheckOut)
                .HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v))
                .HasColumnType("date")
                .IsRequired();

            e.Property(x => x.CreatedAtUtc).IsRequired();

            e.HasOne(x => x.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.RoomId, x.CheckIn, x.CheckOut });
            e.HasIndex(x => x.UserId);
        });
    }
}
