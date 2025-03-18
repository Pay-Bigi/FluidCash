namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record AccountsFilterParams
(
    string? accountId,
    string? displayName,
    string? userMail
);
