namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record ApproveGiftCardDto
(
    string tradeId,
    bool isApproved
);