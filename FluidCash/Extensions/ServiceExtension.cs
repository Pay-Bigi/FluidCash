using FluidCash.DataAccess.DbContext;
using FluidCash.ExternalServicesRepo;
using FluidCash.Helpers.ServiceConfigs.External;
using FluidCash.Helpers.ServiceConfigs.Internal;
using FluidCash.IExternalServicesRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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
            .AddTokenProvider<NumericPasswordResetTokenProvider<AppUser>>(providerName: "NumericPasswordReset");

        //Set lifespan for token validity
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(5); // Set token expiration time
        });
    }

    public static void
        ConfigureRedisCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
    }


    public static void
        ConfigureEmailService
        (this IServiceCollection services, IConfigurationBuilder configurationBuilder)
    {
        IConfiguration configuration = configurationBuilder
            .AddEnvironmentVariables("PAYBIGISMTP__")
            .Build();
        services.Configure<EmailConfig>(configuration);
        services.AddScoped<IEmailProvider, EmailProvider>();
        services.AddScoped<IEmailSender, EmailSender>();
    }

    public static void
        ConfigureAuthServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        var validIssuer = configuration.GetValue<string>(key: "ValidIssuer");
        var validAudience = configuration.GetValue<string>(key: "ValidAudience");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = validIssuer;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.Audience = validAudience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = validIssuer
                };
            });
    }

    public static void
     ConfigureTokenService
     (this IServiceCollection services, IConfigurationBuilder configurationBuilder)
    {
        IConfiguration configuration = configurationBuilder
            .AddEnvironmentVariables("JwtSettings_")
            .Build();
        services.Configure<JwtSettings>(configuration);
    }
}
