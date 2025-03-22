namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UploadDpParams
(
    IFormFile? dp,
    string? accountId
);