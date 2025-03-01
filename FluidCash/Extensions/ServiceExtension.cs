using FluidCash.DataAccess.DbContext;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;

namespace FluidCash.Extensions;

public static class ServiceExtension
{
    public static void
        ConfigureAspNetIdentity
        (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<NumericPasswordResetTokenProvider<AppUser>>("NumericPasswordReset");
    }
}
