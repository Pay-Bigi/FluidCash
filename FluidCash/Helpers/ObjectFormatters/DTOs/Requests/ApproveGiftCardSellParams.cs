namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record ApproveGiftCardSellParams
(
    string tradeId,
    bool isApproved
);