using FluidCash.Helpers.ObjectFormatters.DTOs.CustomErrors;
using Microsoft.AspNetCore.Diagnostics;
using NLog;
using ILogger = NLog.ILogger;

namespace FluidCash.Middlewares;

public static class ExceptionMiddleware
{
    private static ILogger logger = LogManager.GetCurrentClassLogger();
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(
            appError => appError.Run(
                async context =>
                {
                    context.Response.ContentType = "application/json";
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionFeature is not null)
                    {
                        var error = exceptionFeature.Error;
                        var endPoint = exceptionFeature.Endpoint;
                        logger.Error(error, $"An error occurred processing request {endPoint}.");
                        await context.Response.WriteAsJsonAsync(new GlobalErrorDetails
                        {
                            Status = false,
                            StatusCode = 500,
                            Message = $"Server error. Details: {error.Message}"
                        });
                    }
                })
            );
    }
}
