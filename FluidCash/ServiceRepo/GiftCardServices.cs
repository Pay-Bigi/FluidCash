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
        (CreateGiftCardAndRateParams createGiftCardAndRateDto, string? userId)
    {
        string? lowerCategory = createGiftCardAndRateDto.category.ToLower();
        string? lowerSubCategory = createGiftCardAndRateDto.subCategory.ToLower();
        bool cardExists = await _giftCardRepo.ExistsByConditionAsync
            (crd =>
                crd.Category!.ToLower() == lowerCategory &&
                crd.SubCategory!.ToLower() == lowerSubCategory
            );
        if (cardExists)
        {
            string? errorMsg = "Not created. Card with category and sub category already exists";
            return StandardResponse<string>.Failed(data: null, errorMsg);
        }
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
        var giftCard = _giftCardRepo.GetNonDeletedByCondition(x => x.Id == giftCardId)
            .Include(x => x.GiftCardRates)
            .FirstOrDefault();
        if (giftCard is null)
        {
            string? errorMsg = "Gift Card not found";
            return StandardResponse<string>.Failed(errorMsg);
        }
        giftCard.UpdatedAt = DateTime.UtcNow;
        giftCard.UpdatedBy = userId;
        foreach (var rate in giftCard.GiftCardRates!)
        {
            rate.UpdatedAt = DateTime.UtcNow;
            rate.UpdatedBy = userId;
        }
        _giftCardRepo.SoftDelete(giftCard);
        _giftCardRateRepo.SoftDeleteRange(giftCard.GiftCardRates!);
        _giftCardRateRepo.UpdateRange(giftCard.GiftCardRates!);
        _giftCardRepo.Update(giftCard);
        await _giftCardRepo.SaveChangesAsync();

        string? successMsg = "Gift Card deleted successfully";
        return StandardResponse<string>.Pending(successMsg);
    }

    public async Task<StandardResponse<string>>
        DeleteGiftCardRateAsync
        (string giftCardRateId, string? userId)
    {
        var giftCardRate = _giftCardRateRepo
            .GetNonDeletedByCondition(x => x.Id == giftCardRateId)
            .FirstOrDefault();
        if (giftCardRate is null)
        {
            string? errorMsg = "Gift card rate not found";
            return StandardResponse<string>.Failed(errorMsg);
        }
        giftCardRate.UpdatedAt = DateTime.UtcNow;
        giftCardRate.UpdatedBy = userId;
        _giftCardRateRepo.SoftDelete(giftCardRate);
        _giftCardRateRepo.Update(giftCardRate);
        await _giftCardRateRepo.SaveChangesAsync();

        string? successMsg = "Gift card rate deleted successfully";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<StandardResponse<IEnumerable<GiftCardResponseDto>>>
        GetGiftCardAsync
        (GetGiftCardFilterParams getGiftCardDto)
    {
        var query = _giftCardRepo.GetAllNonDeleted();
        if(!query.Any())
        {
            string? errorMsg = "No gift cards found";
            return StandardResponse<IEnumerable<GiftCardResponseDto>>.Failed(data: null, errorMsg);
        }

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(getGiftCardDto.giftCardId))
        {
            var giftCardId = getGiftCardDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Id!.ToLower(), $"%{giftCardId}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto.category))
        {
            var category = getGiftCardDto.category.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Category!.ToLower(), $"%{category}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto.subCategory))
        {
            var subCategory = getGiftCardDto.subCategory.ToLower();
            query = query.Where(x => EF.Functions.Like(x.SubCategory!.ToLower(), $"%{subCategory}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardDto?.giftCardRateId))
        {
            var giftCardRateId = getGiftCardDto.giftCardRateId.Trim();
            query = query.Where(x => x.GiftCardRates!.Any(rate => rate.Id == giftCardRateId));
        }

        // Execute query
        var giftCards = await query.Include(crd => crd.GiftCardRates)
            .Select(x => new GiftCardResponseDto(
                x.Category,
                x.SubCategory,
                x.GiftCardRates!.Select(y => new GiftCardRateResponseDto(
                    y.CountryCode,
                    y.Currency,
                    y.Rate,
                    x.Id,
                    y.Id
                ))
            ))
            .ToListAsync();

        // Return result
        return giftCards.Any()
            ? StandardResponse<IEnumerable<GiftCardResponseDto>>.Success(giftCards)
            : StandardResponse<IEnumerable<GiftCardResponseDto>>.Failed(data: null, errorMessage: "No gift cards found.");
    }

    public async Task<GiftCardResponseDto?>
        GetGiftCardByIdAsync
        (string cardId)
    {
        // Base query: Get non-deleted gift cards by ID
        var query = _giftCardRepo.GetByCondition(crd => crd.Id == cardId)
            .Include(crd=>crd.GiftCardRates);

        // Execute query
        var giftCard = await query
            .Select(x => new GiftCardResponseDto(
                x.Category,
                x.SubCategory,
                x.GiftCardRates!.Any() ? x.GiftCardRates!.Select(y => new GiftCardRateResponseDto(
                    y.CountryCode,
                    y.Currency,
                    y.Rate,
                    x.Id,
                    y.Id
                )) : null
            ))
            .FirstOrDefaultAsync();

        // Return result
        return giftCard;
    }

    public async Task<StandardResponse<GiftCardResponseDto>?>
        GetGiftCardByIdApiAsync
        (string cardId)
    {
        // Base query: Get non-deleted gift cards by ID
        var query = _giftCardRepo.GetByCondition(crd => crd.Id == cardId)
            .Include(crd=>crd.GiftCardRates);

        // Execute query
        var giftCard = await query
            .Select(x => new GiftCardResponseDto(
                x.Category,
                x.SubCategory,
                x.GiftCardRates!.Any() ? x.GiftCardRates!.Select(y => new GiftCardRateResponseDto(
                    y.CountryCode,
                    y.Currency,
                    y.Rate,
                    x.Id,
                    y.Id
                )) : null
            ))
            .FirstOrDefaultAsync();

        // Return result
        return StandardResponse<GiftCardResponseDto>.Success(giftCard);
    }

    public async Task<decimal>
        GetGiftCardRateByIdAsync(string giftCardRateId)
    {
        var rate = await _giftCardRateRepo.GetNonDeletedByCondition(crd => crd.Id == giftCardRateId)
            .Select(crd => crd.Rate)
            .FirstOrDefaultAsync();

        // Return result
        return rate;
    }

    public async Task<StandardResponse<IEnumerable<GiftCardRateResponseDto>>>
        GetGiftCardRatesAsync
        (GetGiftCardRateFilterParams getGiftCardRateDto)
    {
        // Base query: Get non-deleted gift cards rates
        var query = _giftCardRateRepo.GetAllNonDeleted();

        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.giftCardId))
        {
            var cardId = getGiftCardRateDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.GiftCardId!.ToLower(), $"%{cardId}%"));
        }
        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.giftCardRateId))
        {
            string? cardRateId = getGiftCardRateDto.giftCardRateId.ToLower();
            query = query.Where(crd => EF.Functions.Like(crd.Id, $"%{cardRateId}%"));
        }
        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.countryCode))
        {
            var countryCode = getGiftCardRateDto.countryCode.ToLower();
            query = query.Where(x => EF.Functions.Like(x.CountryCode!.ToLower(), $"%{countryCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(getGiftCardRateDto.currency))
        {
            var currency = getGiftCardRateDto.currency.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Currency!.ToLower(), $"%{currency}%"));
        }

        if (getGiftCardRateDto.rate.HasValue)
        {
            var rate = getGiftCardRateDto.rate;
            query = query.Where(x => x.Rate == rate);
        }

        // Execute query
        var giftCardRates = await query
            .Select(x =>
                new GiftCardRateResponseDto(
                    x.CountryCode,
                    x.Currency,
                    x.Rate,
                    x.GiftCardId,
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
        (UpdateGiftCardParams updateGiftCardDto, string? userId)
    {
        // Base query: Get non-deleted gift cards by ID
        var cardToUpdate = _giftCardRepo.GetNonDeletedByCondition(crd => crd.Id == updateGiftCardDto.giftCardId)
            .FirstOrDefault();

        if (cardToUpdate is null)
        {
            string? errorMsg = "Gift Card not found";
            return StandardResponse<string>.Failed(errorMsg);
        }

            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(updateGiftCardDto.category))
        {
            cardToUpdate.Category = updateGiftCardDto.category;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardDto.subCategory))
        {
            cardToUpdate.SubCategory = updateGiftCardDto.subCategory;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardDto?.giftCardRateId))
        {
            var giftCardRate = _giftCardRateRepo.GetNonDeletedByCondition(x => x.Id == updateGiftCardDto.giftCardRateId)
                .FirstOrDefault();
            cardToUpdate.GiftCardRates!.Add(giftCardRate!);
        }
        cardToUpdate.UpdatedBy = userId;
        cardToUpdate.UpdatedAt = DateTime.UtcNow;
        _giftCardRepo.Update(cardToUpdate);
        await _giftCardRepo.SaveChangesAsync();

        string successMsg = "Giftcard update successful";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<StandardResponse<string>>
        UpdateGiftCardRateAsync
        (UpdateGiftCardRateParams updateGiftCardRateDto, string? userId)
    {
        // Base query: Get non-deleted gift card rate by ID
        var cardRateToUpdate = _giftCardRateRepo.GetNonDeletedByCondition(crd => crd.Id == updateGiftCardRateDto.giftCardRateId)
            .FirstOrDefault();
        if (cardRateToUpdate is null)
        {
            string? errorMsg = "Gift Card rate not found";
            return StandardResponse<string>.Failed(errorMsg);
        }

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(updateGiftCardRateDto.countryCode))
        {
            cardRateToUpdate.CountryCode = updateGiftCardRateDto.countryCode;
        }

        if (!string.IsNullOrWhiteSpace(updateGiftCardRateDto.currency))
        {
            cardRateToUpdate.Currency = updateGiftCardRateDto.currency;
        }

        if (updateGiftCardRateDto.rate.HasValue)
        {
            cardRateToUpdate.Rate = updateGiftCardRateDto.rate.Value;
        }
        cardRateToUpdate.UpdatedBy = userId;
        cardRateToUpdate.UpdatedAt = DateTime.UtcNow;
        await _giftCardRateRepo.SaveChangesAsync();

        string successMsg = "Giftcard rate update successful";
        return StandardResponse<string>.Success(successMsg);
    }

    public async Task<bool>
        ConfirmCardExistsAsync(string? giftCardId)
    {
        var cardExists = await _giftCardRepo.ExistsByConditionAsync(giftCrd => giftCrd.Id == giftCardId);
        return cardExists;
    }
}