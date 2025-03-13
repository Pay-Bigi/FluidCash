using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<StandardResponse<string>> 
        DeleteGiftCardAsync
        (string giftCardId, string? userId)
    {
        var giftCard = _giftCardRepo.GetNonDeletedByCondition(x => x.Id == giftCardId).FirstOrDefault();
        if(giftCard is not null)
       { 
            _giftCardRepo.SoftDelete(giftCard);
            await _giftCardRepo.SaveChangesAsync();
        }
        string? successMsg = "Gift Card deleted successfully";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<StandardResponse<string>> 
        DeleteGiftCardRateAsync
        (string giftCardRateId, string? userId)
    {
        var giftCardRate = _giftCardRateRepo
            .GetNonDeletedByCondition(x => x.Id == giftCardRateId)
            .FirstOrDefault();
        if (giftCardRate is not null)
        {
            _giftCardRateRepo.SoftDelete(giftCardRate);
            await _giftCardRateRepo.SaveChangesAsync();
        }
        string? successMsg = "Gift card rate deleted successfully";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<StandardResponse<IEnumerable<GiftCardResponseDto>>> 
        GetGiftCardAsync
        (GetGiftCardDto getGiftCardDto)
    {
        // Base query: Get non-deleted gift cards by ID
        var query = _giftCardRepo.GetAllNonDeleted();

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(getGiftCardDto.giftCardId))
        {
            var giftCardId = getGiftCardDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Id.ToLower(), $"%{giftCardId}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto.category))
        {
            var category = getGiftCardDto.category.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Category.ToLower(), $"%{category}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto.subCategory))
        {
            var subCategory = getGiftCardDto.subCategory.ToLower();
            query = query.Where(x => EF.Functions.Like(x.SubCategory.ToLower(), $"%{subCategory}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto?.giftCardRateId))
        {
            var giftCardRateId = getGiftCardDto.giftCardRateId.Trim();
            query = query.Where(x => x.GiftCardRates.Any(rate => rate.Id == giftCardRateId));
        }

        // Execute query
        var giftCards = await query
            .Select(x => new GiftCardResponseDto(
                x.Category,
                x.SubCategory,
                x.GiftCardRates.Select(y => new GiftCardRateResponseDto(
                    y.CountryCode,
                    y.Currency,
                    y.Rate,
                    getGiftCardDto.giftCardId,
                    y.Id
                ))
            ))
            .ToListAsync();

        // Return result
        return giftCards.Any()
            ? StandardResponse<IEnumerable<GiftCardResponseDto>>.Success(giftCards)
            : StandardResponse<IEnumerable<GiftCardResponseDto>>.Failed(data: null, errorMessage: "No gift cards found.");
    }

    public async Task<decimal>
        GetGiftCardRateByIdAsync(string giftCardRateId)
    {
        var rate = await _giftCardRateRepo.GetNonDeletedByCondition(crd => crd.Id == giftCardRateId)
            .Select(crd=>crd.Rate)
            .FirstOrDefaultAsync();

        // Return result
        return rate;
    }

    public async Task<StandardResponse<IEnumerable<GiftCardRateResponseDto>>> 
        GetGiftCardRateAsync
        (GetGiftCardRateDto getGiftCardRateDto)
    {// Base query: Get non-deleted gift cards by ID
        string? cardRateId = getGiftCardRateDto.giftCardRateId;
        var query = _giftCardRateRepo.GetNonDeletedByCondition(crd => 
        EF.Functions.Like( crd.Id,  $"%{cardRateId}%" ));

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.countryCode))
        {
            var countryCode = getGiftCardRateDto.countryCode.ToLower();
            query = query.Where(x => EF.Functions.Like(x.CountryCode.ToLower(), $"%{countryCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.currency))
        {
            var currency = getGiftCardRateDto.currency.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Currency.ToLower(), $"%{currency}%"));
        }

        if (getGiftCardRateDto.rate.HasValue)
        {
            var rate = getGiftCardRateDto.rate;
            query = query.Where(x => x.Rate == rate);
        }


        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.giftCardId))
        {
            var cardId = getGiftCardRateDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.GiftCardId.ToLower(), $"%{cardId}%"));
        }

        // Execute query
        var giftCardRates = await query
            .Select(x => 
                new GiftCardRateResponseDto(
                    x.CountryCode,
                    x.Currency,
                    x.Rate,
                    getGiftCardRateDto.giftCardId,
                    x.Id
                )
            ).ToListAsync();

        // Return result
        return giftCardRates.Any()
            ? StandardResponse<IEnumerable<GiftCardRateResponseDto>>.Success(giftCardRates)
            : StandardResponse<IEnumerable<GiftCardRateResponseDto>>.Failed(data: null, errorMessage: "No gift cards found.");
    }

    public async Task<StandardResponse<string>> 
        UpdateGiftCardAsync
        (UpdateGiftCardDto updateGiftCardDto, string? userId)
    {
        // Base query: Get non-deleted gift cards by ID
        var query = _giftCardRepo.GetNonDeletedByCondition(crd => crd.Id == updateGiftCardDto.giftCardId).FirstOrDefault();

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(updateGiftCardDto.category))
        {
            query.Category = updateGiftCardDto.category;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardDto.subCategory))
        {
            query.SubCategory = updateGiftCardDto.subCategory;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardDto?.giftCardRateId))
        {
            var giftCardRate = _giftCardRateRepo.GetNonDeletedByCondition(x => x.Id == updateGiftCardDto.giftCardRateId).FirstOrDefault();
            query.GiftCardRates.Add(giftCardRate);
        }
        query.UpdatedBy = userId;
        query.UpdatedAt = DateTime.UtcNow;
        await _giftCardRepo.SaveChangesAsync();

        string successMsg = "Giftcard update successful";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<StandardResponse<string>> 
        UpdateGiftCardRateAsync
        (UpdateGiftCardRateDto updateGiftCardRateDto, string? userId)
    {
        // Base query: Get non-deleted gift card rate by ID
        var query = _giftCardRateRepo.GetNonDeletedByCondition(crd => crd.Id == updateGiftCardRateDto.giftCardRateId).FirstOrDefault();

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(updateGiftCardRateDto.countryCode))
        {
            query.CountryCode = updateGiftCardRateDto.countryCode;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardRateDto.currency))
        {
            query.Currency = updateGiftCardRateDto.currency;
        }

        if (updateGiftCardRateDto.rate.HasValue)
        {
            query.Rate = updateGiftCardRateDto.rate.Value;
        }
        query.UpdatedBy = userId;
        query.UpdatedAt = DateTime.UtcNow;
        await _giftCardRateRepo.SaveChangesAsync();

        string successMsg = "Giftcard rate update successful";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<bool>
        ConfirmCardExistsAsync(string? giftCardId)
    {
        var cardExists = await _giftCardRepo.ExistsByCondition(giftCrd => giftCrd.Id == giftCardId);
        return cardExists;
    }
}