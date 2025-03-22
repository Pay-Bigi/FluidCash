using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IExternalServicesRepo;

public interface IEmailSender
{
    Task<StandardResponse<string>> ProcessAndSendEmailAsync(SendEmailParams sendEmailRequestDto);
}
