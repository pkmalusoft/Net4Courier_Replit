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
        var stepOrder = 1;

        try
        {
            var (rateCard, rateCardSource) = await FindApplicableRateCardWithTrace(request);
            if (rateCard == null)
            {
                result.ErrorMessage = "No applicable rate card found for this shipment";
                return result;
            }

            result.RateCardId = rateCard.Id;
            result.RateCardName = rateCard.RateCardName;
            result.RateCardSource = rateCardSource;

            result.FormulaTrace.Add(new FormulaTraceStep
            {
                Order = stepOrder++,
                Category = "Rate Card Selection",
                Description = $"Selected: {rateCard.RateCardName}",
                Formula = rateCardSource
            });

            var (zone, zonePath) = await ResolveZoneWithTrace(rateCard.Id, request.DestinationCountryId, request.DestinationCityId);
            if (zone == null)
            {
                result.ErrorMessage = "No zone found for destination";
                return result;
            }

            result.ZoneCode = zone.ZoneMatrix?.ZoneCode ?? "";
            result.ZoneName = zone.ZoneMatrix?.ZoneName ?? "";
            result.ZoneResolutionPath = zonePath;

            result.FormulaTrace.Add(new FormulaTraceStep
            {
                Order = stepOrder++,
                Category = "Zone Resolution",
                Description = $"Zone: {result.ZoneCode} - {result.ZoneName}",
                Formula = zonePath
            });

            var (chargeableWeight, volumetricWeight, weightFormula) = CalculateChargeableWeightWithTrace(request);
            result.ChargeableWeight = chargeableWeight;
            result.VolumetricWeight = volumetricWeight;

            result.FormulaTrace.Add(new FormulaTraceStep
            {
                Order = stepOrder++,
                Category = "Weight Calculation",
                Description = "Chargeable Weight",
                Formula = weightFormula,
                Value = chargeableWeight
            });

            var (baseCharge, slabCharge, rules) = CalculateCharges(zone, chargeableWeight);
            result.BaseCharge = baseCharge;
            result.SlabCharge = slabCharge;
            result.AppliedRules = rules;

            result.FormulaTrace.Add(new FormulaTraceStep
            {
                Order = stepOrder++,
                Category = "Base Charge",
                Description = $"Base Weight: {zone.BaseWeight:N3}kg @ {zone.BaseRate:N2}",
                Formula = $"Base Rate = {zone.BaseRate:N2}",
                Value = baseCharge
            });

            if (slabCharge > 0)
            {
                result.FormulaTrace.Add(new FormulaTraceStep
                {
                    Order = stepOrder++,
                    Category = "Slab Charges",
                    Description = string.Join("; ", rules.Skip(1)),
                    Formula = $"Total Slab = {slabCharge:N2}",
                    Value = slabCharge
                });
            }

            var originalSubTotal = baseCharge + slabCharge;
            result.SubTotal = originalSubTotal;

            if (zone.MinCharge.HasValue && result.SubTotal < zone.MinCharge.Value)
            {
                result.MinChargeApplied = zone.MinCharge.Value;
                result.FormulaTrace.Add(new FormulaTraceStep
                {
                    Order = stepOrder++,
                    Category = "Min Charge Adjustment",
                    Description = $"Subtotal {originalSubTotal:N2} < Min {zone.MinCharge.Value:N2}",
                    Formula = $"Applied Min Charge = {zone.MinCharge.Value:N2}",
                    Value = zone.MinCharge.Value
                });
                result.SubTotal = zone.MinCharge.Value;
            }
            if (zone.MaxCharge.HasValue && result.SubTotal > zone.MaxCharge.Value)
            {
                result.MaxChargeApplied = zone.MaxCharge.Value;
                result.FormulaTrace.Add(new FormulaTraceStep
                {
                    Order = stepOrder++,
                    Category = "Max Charge Adjustment",
                    Description = $"Subtotal {result.SubTotal:N2} > Max {zone.MaxCharge.Value:N2}",
                    Formula = $"Applied Max Charge = {zone.MaxCharge.Value:N2}",
                    Value = zone.MaxCharge.Value
                });
                result.SubTotal = zone.MaxCharge.Value;
            }

            if (zone.MarginPercentage.HasValue && zone.MarginPercentage > 0)
            {
                result.MarginPercentage = zone.MarginPercentage;
                result.Margin = result.SubTotal * (zone.MarginPercentage.Value / 100m);
                result.FormulaTrace.Add(new FormulaTraceStep
                {
                    Order = stepOrder++,
                    Category = "Margin",
                    Description = $"{zone.MarginPercentage.Value:N1}% of {result.SubTotal:N2}",
                    Formula = $"{result.SubTotal:N2} x {zone.MarginPercentage.Value:N1}% = {result.Margin:N2}",
                    Value = result.Margin
                });
            }

            result.TotalCharge = result.SubTotal + result.Margin;

            result.FormulaTrace.Add(new FormulaTraceStep
            {
                Order = stepOrder++,
                Category = "Final Total",
                Description = result.Margin > 0 ? $"{result.SubTotal:N2} + {result.Margin:N2}" : $"Total",
                Formula = $"Total Charge = {result.TotalCharge:N2}",
                Value = result.TotalCharge
            });

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Rating calculation error: {ex.Message}";
        }

        return result;
    }

    private async Task<(RateCard? rateCard, string source)> FindApplicableRateCardWithTrace(RatingRequest request)
    {
        var today = DateTime.UtcNow;

        if (request.CustomerId.HasValue)
        {
            var assignment = await _dbContext.CustomerRateAssignments
                .Include(a => a.RateCard)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == request.CustomerId.Value
                         && a.IsActive && !a.IsDeleted
                         && a.EffectiveFrom <= today
                         && (!a.EffectiveTo.HasValue || a.EffectiveTo >= today))
                .OrderBy(a => a.Priority)
                .FirstOrDefaultAsync();

            if (assignment?.RateCard != null && assignment.RateCard.Status == RateCardStatus.Active)
            {
                var customerName = assignment.Customer?.Name ?? "Customer";
                return (assignment.RateCard, $"Customer Assignment: {customerName} (Priority {assignment.Priority})");
            }
        }

        var baseQuery = _dbContext.RateCards
            .Where(r => r.Status == RateCardStatus.Active
                     && !r.IsDeleted
                     && r.MovementTypeId == request.MovementType
                     && r.PaymentModeId == request.PaymentMode
                     && r.ValidFrom <= today
                     && (!r.ValidTo.HasValue || r.ValidTo >= today));

        if (request.ServiceTypeId.HasValue)
            baseQuery = baseQuery.Where(r => r.ServiceTypeId == request.ServiceTypeId || r.ServiceTypeId == null);
        if (request.ShipmentModeId.HasValue)
            baseQuery = baseQuery.Where(r => r.ShipmentModeId == request.ShipmentModeId || r.ShipmentModeId == null);

        var defaultCard = await baseQuery
            .Where(r => r.IsDefault)
            .OrderByDescending(r => r.ServiceTypeId.HasValue)
            .ThenByDescending(r => r.ShipmentModeId.HasValue)
            .FirstOrDefaultAsync();

        if (defaultCard != null) return (defaultCard, "Default Rate Card (IsDefault=true)");

        var fallbackCard = await baseQuery
            .OrderByDescending(r => r.ServiceTypeId.HasValue)
            .ThenByDescending(r => r.ShipmentModeId.HasValue)
            .ThenByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return (fallbackCard, fallbackCard != null ? "Fallback: Most Recent Active Rate Card" : "No matching rate card found");
    }

    private async Task<(RateCardZone? zone, string resolutionPath)> ResolveZoneWithTrace(long rateCardId, long? countryId, long? cityId)
    {
        var zones = await _dbContext.RateCardZones
            .Include(z => z.ZoneMatrix)
                .ThenInclude(zm => zm!.Details)
            .Include(z => z.SlabRules.Where(s => !s.IsDeleted))
            .Where(z => z.RateCardId == rateCardId && !z.IsDeleted && z.IsActive)
            .ToListAsync();

        if (!zones.Any()) return (null, "No zones configured for rate card");

        if (cityId.HasValue)
        {
            var cityDetailMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.Details.Any(d => d.CityId == cityId && !d.IsDeleted) == true);
            if (cityDetailMatch != null) 
                return (cityDetailMatch, "Matched by City (zone detail)");

            var zoneCityMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.CityId == cityId);
            if (zoneCityMatch != null) 
                return (zoneCityMatch, "Matched by City (zone default)");
        }

        if (countryId.HasValue)
        {
            var countryDetailMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.Details.Any(d => d.CountryId == countryId && d.CityId == null && !d.IsDeleted) == true);
            if (countryDetailMatch != null) 
                return (countryDetailMatch, "Matched by Country (zone detail)");

            var zoneCountryMatch = zones.FirstOrDefault(z =>
                z.ZoneMatrix?.CountryId == countryId && z.ZoneMatrix?.CityId == null);
            if (zoneCountryMatch != null) 
                return (zoneCountryMatch, "Matched by Country (zone default)");
        }

        var defaultZone = zones.OrderBy(z => z.ZoneMatrix?.SortOrder ?? 999).FirstOrDefault();
        return (defaultZone, $"Default Zone (SortOrder {defaultZone?.ZoneMatrix?.SortOrder ?? 0})");
    }

    private (decimal chargeableWeight, decimal volumetricWeight, string formula) CalculateChargeableWeightWithTrace(RatingRequest request)
    {
        var volumetricWeight = 0m;
        var formula = "";
        
        if (request.Length > 0 && request.Width > 0 && request.Height > 0)
        {
            var volumetricDivisor = request.VolumetricDivisor > 0 ? request.VolumetricDivisor : 5000m;
            volumetricWeight = (request.Length * request.Width * request.Height) / volumetricDivisor;
            
            formula = $"Actual: {request.ActualWeight:N3}kg, Volumetric: ({request.Length:N1} x {request.Width:N1} x {request.Height:N1}) / {volumetricDivisor:N0} = {volumetricWeight:N3}kg";
            
            if (volumetricWeight > request.ActualWeight)
            {
                formula += " (Volumetric used)";
            }
            else
            {
                formula += " (Actual used)";
            }
        }
        else
        {
            formula = $"Actual Weight: {request.ActualWeight:N3}kg (no dimensions)";
        }

        return (Math.Max(request.ActualWeight, volumetricWeight), volumetricWeight, formula);
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
                case SlabCalculationMode.FlatForSlab:
                    slabCharge = slab.FlatRate ?? 0;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: Flat for Slab = {slabCharge:N2}");
                    break;

                case SlabCalculationMode.PerKg:
                    slabCharge = slabWeight * slab.IncrementRate;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: {slabWeight:N3}kg @ {slab.IncrementRate:N2}/kg = {slabCharge:N2}");
                    break;

                case SlabCalculationMode.FlatAfter:
                    slabCharge = slab.FlatRate ?? slab.IncrementRate;
                    rules.Add($"Slab {slab.FromWeight:N1}-{slab.ToWeight:N1}kg: Flat After = {slabCharge:N2}");
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
    public long? ServiceTypeId { get; set; }
    public long? ShipmentModeId { get; set; }
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
    public List<FormulaTraceStep> FormulaTrace { get; set; } = new();
    public decimal? MinChargeApplied { get; set; }
    public decimal? MaxChargeApplied { get; set; }
    public decimal? MarginPercentage { get; set; }
    public decimal VolumetricWeight { get; set; }
    public string RateCardSource { get; set; } = "";
    public string ZoneResolutionPath { get; set; } = "";
}

public class FormulaTraceStep
{
    public int Order { get; set; }
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Formula { get; set; } = "";
    public decimal? Value { get; set; }
}
