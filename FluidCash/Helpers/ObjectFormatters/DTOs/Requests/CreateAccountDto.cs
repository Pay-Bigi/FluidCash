namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateAccountDto
(
    IFormFile? dpImage,
    string displayName,
    string? userEmail,
    string? password
);