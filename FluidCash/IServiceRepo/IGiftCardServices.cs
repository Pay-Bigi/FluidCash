using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IGiftCardServices
{
    Task<decimal>
        GetGiftCardRateByIdAsync(string giftCardRateId);

    Task<bool>
        ConfirmCardExistsAsync(string? giftCardId);

    Task<StandardResponse<string>>
        UpdateGiftCardAsync(UpdateGiftCardDto updateGiftCardDto, string? userId);

    Task<StandardResponse<IEnumerable<GiftCardResponseDto>>>
        GetGiftCardAsync(GetGiftCardDto getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardAsync(string giftCardId, string? userId);

    Task<StandardResponse<string>>
        CreateGiftCardAndRateAsync(CreateGiftCardAndRateDto createGiftCardAndRateDto, string? userId);

    Task<StandardResponse<string>>
        UpdateGiftCardRateAsync(UpdateGiftCardRateDto updateGiftCardDto, string? userId);

    Task<StandardResponse<IEnumerable<GiftCardRateResponseDto>>>
        GetGiftCardRateAsync(GetGiftCardRateDto getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardRateAsync(string giftCardRateId, string? userId);
}
