using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IGiftCardServices
{
    Task<StandardResponse<GiftCardResponseDto>>
        GetGiftCardByIdAsync(string cardId);

    Task<decimal>
        GetGiftCardRateByIdAsync(string giftCardRateId);

    Task<bool>
        ConfirmCardExistsAsync(string? giftCardId);

    Task<StandardResponse<string>>
        UpdateGiftCardAsync(UpdateGiftCardParams updateGiftCardDto, string? userId);

    Task<StandardResponse<IEnumerable<GiftCardResponseDto>>>
        GetGiftCardAsync(GetGiftCardFilterParams getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardAsync(string giftCardId, string? userId);

    Task<StandardResponse<string>>
        CreateGiftCardAndRateAsync(CreateGiftCardAndRateParams createGiftCardAndRateDto, string? userId);

    Task<StandardResponse<string>>
        UpdateGiftCardRateAsync(UpdateGiftCardRateParams updateGiftCardDto, string? userId);

    Task<StandardResponse<IEnumerable<GiftCardRateResponseDto>>>
        GetGiftCardRateAsync(GetGiftCardRateFilterParams getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardRateAsync(string giftCardRateId, string? userId);
}
