using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class RatingEngineService
{
    private readonly ApplicationDbContext _dbContext;

    public RatingEngineService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RatingResult> CalculateRate(RatingRequest request)
    {
        var result = new RatingResult { Request = request };

        try
        {
            var rateCard = await FindApplicableRateCard(request);
            if (rateCard == null)
            {
                result.ErrorMessage = "No applicable rate card found for this shipment";
                return result;
            }

            result.RateCardId = rateCard.Id;
            result.RateCardName = rateCard.RateCardName;

            var zone = await ResolveZone(rateCard.Id, request.DestinationCountryId, request.DestinationCityId);
            if (zone == null)
            {
                result.ErrorMessage = "No zone found for destination";
                return result;
            }

            result.ZoneCode = zone.ZoneMatrix?.ZoneCode ?? "";
            result.ZoneName = zone.ZoneMatrix?.ZoneName ?? "";

            var chargeableWeight = CalculateChargeableWeight(request);
            result.ChargeableWeight = chargeableWeight;

            var (baseCharge, slabCharge, rules) = CalculateCharges(zone, chargeableWeight);
            result.BaseCharge = baseCharge;
            result.SlabCharge = slabCharge;
            result.AppliedRules = rules;

            result.SubTotal = baseCharge + slabCharge;

            if (zone.MinCharge.HasValue && result.SubTotal < zone.MinCharge.Value)
            {
                result.SubTotal = zone.MinCharge.Value;
            }
            if (zone.MaxCharge.HasValue && result.SubTotal > zone.MaxCharge.Value)
            {
                result.SubTotal = zone.MaxCharge.Value;
            }

            if (zone.MarginPercentage.HasValue && zone.MarginPercentage > 0)
            {
                result.Margin = result.SubTotal * (zone.MarginPercentage.Value / 100m);
            }

            result.TotalCharge = result.SubTotal + result.Margin;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Rating calculation error: {ex.Message}";
        }

        return result;
    }

    private async Task<RateCard?> FindApplicableRateCard(RatingRequest request)
    {
        var today = DateTime.UtcNow;

        if (request.CustomerId.HasValue)
        {
            var assignment = await _dbContext.CustomerRateAssignments
                .Include(a => a.RateCard)
                .Where(a => a.CustomerId == request.CustomerId.Value
                         && a.IsActive && !a.IsDeleted
                         && a.EffectiveFrom <= today
                         && (!a.EffectiveTo.HasValue || a.EffectiveTo >= today))
                .OrderBy(a => a.Priority)
                .FirstOrDefaultAsync();

            if (assignment?.RateCard != null && assignment.RateCard.Status == RateCardStatus.Active)
            {
                return assignment.RateCard;
            }
        }

        var defaultCard = await _dbContext.RateCards
            .Where(r => r.Status == RateCardStatus.Active
                     && !r.IsDeleted
                     && r.MovementTypeId == request.MovementType
                     && r.PaymentModeId == request.PaymentMode
                     && r.ValidFrom <= today
                     && (!r.ValidTo.HasValue || r.ValidTo >= today)
                     && r.IsDefault)
            .FirstOrDefaultAsync();

        if (defaultCard != null) return defaultCard;

        return await _dbContext.RateCards
            .Where(r => r.Status == RateCardStatus.Active
                     && !r.IsDeleted
                     && r.MovementTypeId == request.MovementType
                     && r.PaymentModeId == request.PaymentMode
                     && r.ValidFrom <= today
                     && (!r.ValidTo.HasValue || r.ValidTo >= today))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<RateCardZone?> ResolveZone(long rateCardId, long? countryId, long? cityId)
    {
        var zones = await _dbContext.RateCardZones
            .Include(z => z.ZoneMatrix)
                .ThenInclude(zm => zm!.Details)
            .Include(z => z.SlabRules.Where(s => !s.IsDeleted))
            .Where(z => z.RateCardId == rateCardId && !z.IsDeleted && z.IsActive)
            .ToListAsync();

        if (!zones.Any()) return null;

        if (cityId.HasValue)
        {
            var cityDetailMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.Details.Any(d => d.CityId == cityId && !d.IsDeleted) == true);
            if (cityDetailMatch != null) return cityDetailMatch;

            var zoneCityMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.CityId == cityId);
            if (zoneCityMatch != null) return zoneCityMatch;
        }

        if (countryId.HasValue)
        {
            var countryDetailMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.Details.Any(d => d.CountryId == countryId && d.CityId == null && !d.IsDeleted) == true);
            if (countryDetailMatch != null) return countryDetailMatch;

            var zoneCountryMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.CountryId == countryId && z.ZoneMatrix?.CityId == null);
            if (zoneCountryMatch != null) return zoneCountryMatch;
        }

        var defaultZone = zones.OrderBy(z => z.ZoneMatrix?.SortOrder ?? 999).FirstOrDefault();
        return defaultZone;
    }

    private decimal CalculateChargeableWeight(RatingRequest request)
    {
        var volumetricWeight = 0m;
        if (request.Length > 0 && request.Width > 0 && request.Height > 0)
        {
            var volumetricDivisor = request.VolumetricDivisor > 0 ? request.VolumetricDivisor : 5000m;
            volumetricWeight = (request.Length * request.Width * request.Height) / volumetricDivisor;
        }

        return Math.Max(request.ActualWeight, volumetricWeight);
    }

    private (decimal baseCharge, decimal slabCharge, List<string> rules) CalculateCharges(RateCardZone zone, decimal weight)
    {
        var rules = new List<string>();
        var baseWeight = zone.BaseWeight;
        var baseRate = zone.BaseRate;

        rules.Add($"Base: {baseWeight:N3}kg @ {baseRate:N2}");

        if (weight <= baseWeight)
        {
            return (baseRate, 0m, rules);
        }

        decimal slabTotal = 0m;
        var remainingWeight = weight;

        var slabRules = zone.SlabRules
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.FromWeight)
            .ToList();

        foreach (var slab in slabRules)
        {
            if (remainingWeight <= slab.FromWeight) break;

            var effectiveFrom = Math.Max(slab.FromWeight, baseWeight);
            var effectiveTo = Math.Min(slab.ToWeight, remainingWeight);

            if (effectiveTo <= effectiveFrom) continue;

            var slabWeight = effectiveTo - effectiveFrom;
            decimal slabCharge;

            switch (slab.CalculationMode)
            {
                case SlabCalculationMode.PerKg:
                    slabCharge = slabWeight * slab.IncrementRate;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: {slabWeight:N3}kg @ {slab.IncrementRate:N2}/kg = {slabCharge:N2}");
                    break;

                case SlabCalculationMode.FlatAfter:
                    slabCharge = slab.IncrementRate;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: Flat {slabCharge:N2}");
                    break;

                case SlabCalculationMode.PerStep:
                default:
                    var steps = Math.Ceiling(slabWeight / slab.IncrementWeight);
                    slabCharge = steps * slab.IncrementRate;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: {steps:N0} x {slab.IncrementWeight:N3}kg @ {slab.IncrementRate:N2} = {slabCharge:N2}");
                    break;
            }

            slabTotal += slabCharge;
        }

        return (baseRate, slabTotal, rules);
    }
}

public class RatingRequest
{
    public long? CustomerId { get; set; }
    public MovementType MovementType { get; set; } = MovementType.Domestic;
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Prepaid;
    public long? OriginCountryId { get; set; }
    public long? OriginCityId { get; set; }
    public long? DestinationCountryId { get; set; }
    public long? DestinationCityId { get; set; }
    public decimal ActualWeight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public int Pieces { get; set; } = 1;
    public decimal VolumetricDivisor { get; set; } = 5000m;
}

public class RatingResult
{
    public RatingRequest Request { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? RateCardId { get; set; }
    public string RateCardName { get; set; } = "";
    public string ZoneCode { get; set; } = "";
    public string ZoneName { get; set; } = "";
    public decimal ChargeableWeight { get; set; }
    public decimal BaseCharge { get; set; }
    public decimal SlabCharge { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Margin { get; set; }
    public decimal TotalCharge { get; set; }
    public List<string> AppliedRules { get; set; } = new();
}
