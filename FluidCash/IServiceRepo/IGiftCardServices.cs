using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IGiftCardServices
{
    Task<StandardResponse<string>> 
        CreateGiftCardAsync(CreateGiftCardDto createGiftCardDto);

    Task<StandardResponse<string>>
        UpdateGiftCardAsync(UpdateGiftCardDto updateGiftCardDto);

    Task<StandardResponse<GiftCardResponseDto>>
        GetGiftCardAsync(GetGiftCardDto getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardAsync(string giftCardId);

    Task<StandardResponse<string>>
        CreateGiftCardAndRateAsync(CreateGiftCardAndRateDto createGiftCardAndRateDto);

    Task<StandardResponse<string>>
        UpdateGiftCardRateAsync(UpdateGiftCardRateDto updateGiftCardDto);

    Task<StandardResponse<GiftCardRateResponseDto>>
        GetGiftCardRateAsync(GetGiftCardRateDto getGiftCardDto);

    Task<StandardResponse<string>>
        DeleteGiftCardRateAsync(string giftCardId);
}
