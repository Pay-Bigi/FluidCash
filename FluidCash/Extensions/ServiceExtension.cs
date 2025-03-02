using FluidCash.DataAccess.DbContext;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;

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

        //Set lifespan for token validity
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(10); // Set token expiration time
        });
    }

    public static void
        ConfigureRedisCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
    }

}
