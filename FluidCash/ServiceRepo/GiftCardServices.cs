using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;

namespace FluidCash.ServiceRepo;

public class GiftCardServices : IGiftCardServices
{
    private readonly IBaseRepo<GiftCard> _giftCardRepo;
    private readonly IBaseRepo<GiftCardRate> _giftCardRateRepo;

    public GiftCardServices(IBaseRepo<GiftCard> giftCardRepo, 
        IBaseRepo<GiftCardRate> giftCardRateRepo)
    {
        _giftCardRepo = giftCardRepo;
        _giftCardRateRepo = giftCardRateRepo;
    }

    public async Task<StandardResponse<string>> 
        CreateGiftCardAndRateAsync
        (CreateGiftCardAndRateDto createGiftCardAndRateDto, string? userId)
    {
        var giftCard = new GiftCard
        {
            Category = createGiftCardAndRateDto.category,
            SubCategory = createGiftCardAndRateDto.subCategory,
            CreatedBy = userId
        };
        var giftCardRate = new GiftCardRate
        {
            Rate = createGiftCardAndRateDto.rate,
            CountryCode = createGiftCardAndRateDto.countryCode,
            Currency = createGiftCardAndRateDto.currency,
            GiftCardId = giftCard.Id,
            CreatedBy = userId
        };

        await _giftCardRepo.AddAsync(giftCard);
        await _giftCardRateRepo.AddAsync(giftCardRate);

        await _giftCardRepo.SaveChangesAsync();

        string? message = "Gift Card and Rate created successfully";

        return StandardResponse<string>.Success(message);
    }

    public Task<StandardResponse<string>> CreateGiftCardAsync(CreateGiftCardDto createGiftCardDto, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> DeleteGiftCardAsync(string giftCardId, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> DeleteGiftCardRateAsync(string giftCardId, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<IEnumerable<GiftCardResponseDto>>> GetGiftCardAsync(GetGiftCardDto getGiftCardDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<IEnumerable<GiftCardRateResponseDto>>> GetGiftCardRateAsync(GetGiftCardRateDto getGiftCardDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> UpdateGiftCardAsync(UpdateGiftCardDto updateGiftCardDto, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> UpdateGiftCardRateAsync(UpdateGiftCardRateDto updateGiftCardDto, string? userId)
    {
        throw new NotImplementedException();
    }
}
