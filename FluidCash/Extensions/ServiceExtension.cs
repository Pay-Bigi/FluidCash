﻿using Asp.Versioning;
using CloudinaryDotNet;
using FluidCash.DataAccess.DbContext;
using FluidCash.DataAccess.Repo;
using FluidCash.ExternalServicesRepo;
using FluidCash.Helpers.ObjectFormatters.DTOs.CustomErrors;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.Helpers.ServiceConfigs.External;
using FluidCash.Helpers.ServiceConfigs.Internal;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using FluidCash.ServiceRepo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PayStack.Net;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using Account = CloudinaryDotNet.Account;
using ValidationError = FluidCash.Helpers.ObjectFormatters.DTOs.CustomErrors.ValidationError;

namespace FluidCash.Extensions;

public static class ServiceExtension
{

    public static void
    RegisterDbContext
        (this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = Environment.GetEnvironmentVariable("FluidCashDB");
        services.AddDbContext<DataContext>(options =>
        options.UseSqlServer(connectionString)
        );
    }

    public static void
        RegisterBaseRpositories
        (this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
    }

    public static void
        ConfigureAspNetIdentity
        (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, AppRole>(o =>
        {
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength = 8;
            o.Password.RequiredUniqueChars = 1;
            o.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<NumericPasswordResetTokenProvider<AppUser>>(providerName: "NumericPasswordReset");

        //Set lifespan for token validity
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(5); // Set token expiration time
        });

        services.AddScoped<UserManager<AppUser>>();
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
        services.AddScoped<ITokenService, TokenService>();
    }

    public static void
        ConfigureControllers
        (this IServiceCollection services)
    {
        services.AddControllers
            (
            config =>
            {
                config.RespectBrowserAcceptHeader = true;
            })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddXmlDataContractSerializerFormatters().ConfigureApiBehaviorOptions(opts =>
            opts.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .SelectMany(ms => ms.Value.Errors
                    .Select(e => new ValidationError
                    {
                        FieldName = ms.Key,
                        ErrorMessage = e.ErrorMessage
                    }))
                    .ToList();

                return new BadRequestObjectResult
                (
                    StandardResponse<IEnumerable<ValidationError>>.Failed
                    (data: errors, errorMessage: "One or more validation errors occurred", 400)
                );
            });
    }

    public static void
        ConfigureApiVersioning
        (this IServiceCollection services)
    {
        services.AddApiVersioning
        (opts =>
        {
            opts.DefaultApiVersion = new ApiVersion(1.0);
            opts.ReportApiVersions = true;
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ApiVersionReader = ApiVersionReader.Combine
                (
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
        }
        ).AddApiExplorer
        (
            opts =>
            {
                opts.GroupNameFormat = "'v'V";
                opts.SubstituteApiVersionInUrl = true;
            }
        );
    }

    public static void
        RegisterContainerServices
        (this IServiceCollection services)
    {
        services.AddScoped<IAccountMgtServices, AccountMgtServices>();
        services.AddScoped<IAuthServices, AuthServices>();
        services.AddScoped<IGiftCardServices, GiftCardServices>();
        services.AddScoped<ITradingServices, TradingServices>();
        services.AddScoped<IWalletServices, WalletServices>();
        //services.AddScoped<ITransactionServices, TransactionServices>();
    }

    public static void
        ConfigureRedisCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
        services.AddScoped<IRedisCacheService, RedisCacheService>();
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
        ConfigurePayStackServices
        (this IServiceCollection services)
    {
        services.AddSingleton<PayStackApi>(provder =>
        {
            var testOrLiveSecret = Environment.GetEnvironmentVariable("PayStackKey");
            var payStackApi = new PayStackApi(testOrLiveSecret);
            return payStackApi;
        });
        services.AddScoped<IPaystackServices, PaystackServices>();
    }

    public static void
        ConfigureCloudinary
        (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Cloudinary>(provider =>
        {
            /*var cloudinaryConfig = configuration.GetSection("CloudinaryConfig")
                                                  .Get<CloudinaryConfig>();  //Used for reading config from file like appsettings.json*/
            var cloudinaryConfig = new CloudinaryConfig
            {
                CLOUDNAME = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUDNAME"),
                APIKEY = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY"),
                APISECRET = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET")
            };

            var cloudinary = new Cloudinary(new Account(
                cloudinaryConfig.CLOUDNAME,
                cloudinaryConfig.APIKEY,
                cloudinaryConfig.APISECRET));
            cloudinary.Api.Secure = cloudinaryConfig.APISECURE;
            return cloudinary;
        });

        services.AddScoped<ICloudinaryServices, CloudinaryServices>();
    }

    public static void
        ConfigureSwaggerGen
        (this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            options.AddSecurityDefinition("Bearer", securityScheme);
            // Make sure Swagger UI requires a Bearer token to be passed for authentication
            var securityRequirement = new OpenApiSecurityRequirement
            {
                { securityScheme, new List<string>() }
            };
            options.AddSecurityRequirement(securityRequirement);
        });

    }



    //public static void
    //   ConfigureHangfire
    //   (this IServiceCollection services)
    //{
    //    string connectionString = Environment.GetEnvironmentVariable("PayBigiDB");
    //    services.AddHangfire(config => config
    //        .UseSimpleAssemblyNameTypeSerializer()
    //        .UseRecommendedSerializerSettings()
    //        .UseSqlServerStorage(connectionString));
    //    services.AddHangfireServer();
    //}
}
