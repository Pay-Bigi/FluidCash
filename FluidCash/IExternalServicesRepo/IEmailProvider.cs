using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using Microsoft.AspNetCore.Identity;

namespace FluidCash.IExternalServicesRepo;

public interface IEmailProvider
{
    IdentityResult SendEmail(EmailParams emailDTO);
}
