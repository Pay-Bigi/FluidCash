namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record ApproveGiftCardSellDto
(
    string tradeId,
    bool isApproved
);