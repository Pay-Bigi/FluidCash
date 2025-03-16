namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateAccountParams
(
    IFormFile? dpImage,
    string displayName,
    string? userEmail,
    string? password
);