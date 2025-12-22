using HotelBooking.Data.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Data.Infrastructure.Persistence;

public static class DbSeeder
{
    public const string RoleAdmin = "Admin";
    public const string RoleClient = "Client";

    public static async Task SeedAsync(IServiceProvider sp, string adminEmail, string adminPassword)
    {
        using var scope = sp.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (!await roleMgr.RoleExistsAsync(RoleAdmin))
            await roleMgr.CreateAsync(new IdentityRole<Guid>(RoleAdmin));

        if (!await roleMgr.RoleExistsAsync(RoleClient))
            await roleMgr.CreateAsync(new IdentityRole<Guid>(RoleClient));

        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var res = await userMgr.CreateAsync(admin, adminPassword);
            if (!res.Succeeded)
                throw new InvalidOperationException("Admin create failed: " + string.Join("; ", res.Errors.Select(e => e.Description)));
        }

        if (!await userMgr.IsInRoleAsync(admin, RoleAdmin))
            await userMgr.AddToRoleAsync(admin, RoleAdmin);
    }
}
