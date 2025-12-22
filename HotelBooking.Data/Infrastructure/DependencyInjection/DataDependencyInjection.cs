using HotelBooking.Data.Application.Interfaces;
using HotelBooking.Data.Application.Services;
using HotelBooking.Data.Infrastructure.Dapper;
using HotelBooking.Data.Infrastructure.Identity;
using HotelBooking.Data.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Data.Infrastructure.DependencyInjection;

public static class DataDependencyInjection
{
    public static IServiceCollection AddHotelBookingData(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing connection string: MySql");

        services.AddDbContext<AppDbContext>(opt => opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

        services.AddIdentityCore<AppUser>(opt => { opt.User.RequireUniqueEmail = true; })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<DatabaseOptions>(o => o.ConnectionString = cs);
        services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IStatsService, StatsService>();

        return services;
    }
}
