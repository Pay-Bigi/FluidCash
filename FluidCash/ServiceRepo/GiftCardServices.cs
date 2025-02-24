﻿using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;

namespace FluidCash.ServiceRepo;

public class GiftCardServices : IGiftCardServices
{
    public Task<StandardResponse<string>> CreateGiftCardAndRateAsync(CreateGiftCardAndRateDto createGiftCardAndRateDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> CreateGiftCardAsync(CreateGiftCardDto createGiftCardDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> DeleteGiftCardAsync(string giftCardId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<GiftCardResponseDto>> GetGiftCardAsync(GetGiftCardDto getGiftCardDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> UpdateGiftCardAsync(UpdateGiftCardDto updateGiftCardDto)
    {
        throw new NotImplementedException();
    }
}
