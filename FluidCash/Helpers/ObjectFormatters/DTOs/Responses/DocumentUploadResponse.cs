namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record DocumentUploadResponse
(
    string? fileUrlPath,
    string? filePublicId
);