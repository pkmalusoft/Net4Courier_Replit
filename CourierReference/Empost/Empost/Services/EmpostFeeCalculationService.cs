using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostFeeCalculationService
{
    decimal CalculateGrossAmount(Shipment shipment);
    decimal CalculateFeeAmount(decimal grossAmount, decimal royaltyPercentage = 10.0m);
    EmpostClassification ClassifyShipment(Shipment shipment, decimal weightThresholdKg = 30.0m);
    Task<EmpostShipmentFee> CalculateAndRecordFeeAsync(Shipment shipment);
    Task<EmpostShipmentFee?> GetShipmentFeeAsync(Guid shipmentId);
    Task RecalculateShipmentFeeAsync(Guid shipmentId);
    Task<decimal> GetQuarterTotalFeeAsync(Guid quarterId);
    Task<QuarterFeeBreakdown> GetQuarterFeeBreakdownAsync(Guid quarterId);
    Task<QuarterCalculationResult> CalculateQuarterFeesAsync(Guid quarterId);
}

public class EmpostFeeCalculationService : IEmpostFeeCalculationService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostQuarterService _quarterService;
    private readonly ILogger<EmpostFeeCalculationService> _logger;

    private const decimal DEFAULT_ROYALTY_PERCENTAGE = 10.0m;
    private const decimal DEFAULT_WEIGHT_THRESHOLD_KG = 30.0m;

    public EmpostFeeCalculationService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostQuarterService quarterService,
        ILogger<EmpostFeeCalculationService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _quarterService = quarterService;
        _logger = logger;
    }

    public decimal CalculateGrossAmount(Shipment shipment)
    {
        var grossAmount = shipment.FreightCharge
            + shipment.FuelSurcharge
            + shipment.InsuranceCharge
            + shipment.CODCharge
            + shipment.OtherCharges;

        return grossAmount;
    }

    public decimal CalculateFeeAmount(decimal grossAmount, decimal royaltyPercentage = 10.0m)
    {
        return Math.Round(grossAmount * (royaltyPercentage / 100), 2);
    }

    public EmpostClassification ClassifyShipment(Shipment shipment, decimal weightThresholdKg = 30.0m)
    {
        var chargeableWeight = shipment.ChargeableWeight > 0 
            ? shipment.ChargeableWeight 
            : shipment.ActualWeight;

        if (chargeableWeight >= weightThresholdKg)
        {
            return EmpostClassification.FreightOver30Kg;
        }

        return EmpostClassification.Taxable;
    }

    public EmpostTaxabilityStatus DetermineTaxability(ShipmentClassificationType classification, ShipmentMode mode)
    {
        if (classification == ShipmentClassificationType.ParcelAbove30kg)
            return EmpostTaxabilityStatus.NonTaxable;

        if (mode == ShipmentMode.Import || mode == ShipmentMode.Transhipment)
            return EmpostTaxabilityStatus.NonTaxable;

        return EmpostTaxabilityStatus.Taxable;
    }

    public async Task<EmpostShipmentFee> CalculateAndRecordFeeAsync(Shipment shipment)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.TenantId == tenantId.Value && l.IsActive);

        if (license == null)
            throw new InvalidOperationException("No active Empost license found");

        var classification = ClassifyShipment(shipment, license.WeightThresholdKg);
        var grossAmount = CalculateGrossAmount(shipment);
        var taxabilityStatus = DetermineTaxability(shipment.ShipmentClassification, shipment.ShipmentMode);
        var feeAmount = taxabilityStatus == EmpostTaxabilityStatus.Taxable
            ? CalculateFeeAmount(grossAmount, license.RoyaltyPercentage)
            : 0;

        var quarter = await _quarterService.GetQuarterForDateAsync(shipment.BookingDate);
        if (quarter == null)
        {
            quarter = await _quarterService.CreateOrGetQuarterAsync(
                shipment.BookingDate.Year,
                _quarterService.GetQuarterInfo(shipment.BookingDate).Quarter,
                license.Id);
        }

        var existingFee = await _context.EmpostShipmentFees
            .FirstOrDefaultAsync(f => f.ShipmentId == shipment.Id && f.TenantId == tenantId.Value);

        if (existingFee != null)
        {
            existingFee.Classification = classification;
            existingFee.ShipmentClassification = shipment.ShipmentClassification;
            existingFee.ShipmentMode = shipment.ShipmentMode;
            existingFee.TaxabilityStatus = taxabilityStatus;
            existingFee.FreightCharge = shipment.FreightCharge;
            existingFee.FuelSurcharge = shipment.FuelSurcharge;
            existingFee.InsuranceCharge = shipment.InsuranceCharge;
            existingFee.CODCharge = shipment.CODCharge;
            existingFee.OtherCharges = shipment.OtherCharges;
            existingFee.DiscountAmount = shipment.DiscountAmount;
            existingFee.GrossAmount = grossAmount;
            existingFee.RoyaltyPercentage = license.RoyaltyPercentage;
            existingFee.EmpostFeeAmount = feeAmount;
            existingFee.ActualWeight = shipment.ActualWeight;
            existingFee.ChargeableWeight = shipment.ChargeableWeight;
            existingFee.EmpostQuarterId = quarter.Id;

            await _context.SaveChangesAsync();
            return existingFee;
        }

        var shipmentFee = new EmpostShipmentFee
        {
            ShipmentId = shipment.Id,
            EmpostQuarterId = quarter.Id,
            AWBNumber = shipment.AWBNumber,
            ShipmentDate = shipment.BookingDate,
            ActualWeight = shipment.ActualWeight,
            ChargeableWeight = shipment.ChargeableWeight,
            Classification = classification,
            ShipmentClassification = shipment.ShipmentClassification,
            ShipmentMode = shipment.ShipmentMode,
            TaxabilityStatus = taxabilityStatus,
            FreightCharge = shipment.FreightCharge,
            FuelSurcharge = shipment.FuelSurcharge,
            InsuranceCharge = shipment.InsuranceCharge,
            CODCharge = shipment.CODCharge,
            OtherCharges = shipment.OtherCharges,
            DiscountAmount = shipment.DiscountAmount,
            GrossAmount = grossAmount,
            RoyaltyPercentage = license.RoyaltyPercentage,
            EmpostFeeAmount = feeAmount,
            FeeStatus = EmpostFeeStatus.Pending
        };

        _context.EmpostShipmentFees.Add(shipmentFee);

        shipment.EmpostClassification = classification;
        shipment.EmpostGrossAmount = grossAmount;
        shipment.EmpostFeeAmount = feeAmount;
        shipment.EmpostFeeStatus = EmpostFeeStatus.Pending;
        shipment.EmpostQuarterId = quarter.Id;
        shipment.EmpostFeeCalculated = true;
        shipment.EmpostFeeCalculatedDate = DateTime.UtcNow;

        if (classification == EmpostClassification.FreightOver30Kg)
        {
            shipment.EmpostClassificationReason = $"Weight {shipment.ChargeableWeight}kg exceeds {license.WeightThresholdKg}kg threshold";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Calculated Empost fee for shipment {AWB}: Classification={Classification}, Gross={Gross}, Fee={Fee}",
            shipment.AWBNumber, classification, grossAmount, feeAmount);

        return shipmentFee;
    }

    public async Task<EmpostShipmentFee?> GetShipmentFeeAsync(Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.EmpostShipmentFees
            .Include(f => f.EmpostQuarter)
            .FirstOrDefaultAsync(f => f.ShipmentId == shipmentId && f.TenantId == tenantId.Value);
    }

    public async Task RecalculateShipmentFeeAsync(Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.TenantId == tenantId.Value);

        if (shipment == null)
            throw new ArgumentException("Shipment not found");

        await CalculateAndRecordFeeAsync(shipment);
    }

    public async Task<decimal> GetQuarterTotalFeeAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return 0;

        return await _context.EmpostShipmentFees
            .Where(f => f.EmpostQuarterId == quarterId 
                && f.TenantId == tenantId.Value
                && f.Classification == EmpostClassification.Taxable
                && !f.IsReturnAdjusted)
            .SumAsync(f => f.EmpostFeeAmount);
    }

    public async Task<QuarterFeeBreakdown> GetQuarterFeeBreakdownAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new QuarterFeeBreakdown();

        var fees = await _context.EmpostShipmentFees
            .Where(f => f.EmpostQuarterId == quarterId && f.TenantId == tenantId.Value)
            .ToListAsync();

        var adjustments = await _context.EmpostReturnAdjustments
            .Where(a => a.EmpostQuarterId == quarterId && a.TenantId == tenantId.Value)
            .ToListAsync();

        var breakdown = new QuarterFeeBreakdown
        {
            QuarterId = quarterId,
            TotalShipments = fees.Count,
            TaxableShipments = fees.Count(f => f.TaxabilityStatus == EmpostTaxabilityStatus.Taxable),
            ExemptShipments = fees.Count(f => f.TaxabilityStatus == EmpostTaxabilityStatus.NonTaxable),
            TotalGrossRevenue = fees.Sum(f => f.GrossAmount),
            TaxableGrossRevenue = fees.Where(f => f.TaxabilityStatus == EmpostTaxabilityStatus.Taxable).Sum(f => f.GrossAmount),
            ExemptGrossRevenue = fees.Where(f => f.TaxabilityStatus == EmpostTaxabilityStatus.NonTaxable).Sum(f => f.GrossAmount),
            TotalEmpostFee = fees.Where(f => f.TaxabilityStatus == EmpostTaxabilityStatus.Taxable).Sum(f => f.EmpostFeeAmount),
            ReturnAdjustments = adjustments.Where(a => a.Status == AdjustmentStatus.Applied).Sum(a => a.AdjustmentAmount),
            NetEmpostFee = fees.Where(f => f.TaxabilityStatus == EmpostTaxabilityStatus.Taxable && !f.IsReturnAdjusted).Sum(f => f.EmpostFeeAmount)
                - adjustments.Where(a => a.Status == AdjustmentStatus.Applied).Sum(a => a.AdjustmentAmount)
        };

        breakdown.ByClassification = fees
            .GroupBy(f => f.Classification)
            .Select(g => new ClassificationBreakdown
            {
                Classification = g.Key,
                ShipmentCount = g.Count(),
                GrossAmount = g.Sum(f => f.GrossAmount),
                FeeAmount = g.Sum(f => f.EmpostFeeAmount)
            })
            .ToList();

        breakdown.ByShipmentType = fees
            .GroupBy(f => f.ShipmentClassification)
            .Select(g => new ShipmentTypeBreakdown
            {
                ShipmentClassification = g.Key,
                ShipmentCount = g.Count(),
                GrossAmount = g.Sum(f => f.GrossAmount),
                FeeAmount = g.Sum(f => f.EmpostFeeAmount)
            })
            .ToList();

        breakdown.ByMode = fees
            .GroupBy(f => f.ShipmentMode)
            .Select(g => new ShipmentModeBreakdown
            {
                ShipmentMode = g.Key,
                ShipmentCount = g.Count(),
                GrossAmount = g.Sum(f => f.GrossAmount),
                FeeAmount = g.Sum(f => f.EmpostFeeAmount)
            })
            .ToList();

        breakdown.ByTypeAndMode = fees
            .GroupBy(f => new { f.ShipmentClassification, f.ShipmentMode })
            .Select(g => new TypeModeBreakdown
            {
                ShipmentClassification = g.Key.ShipmentClassification,
                ShipmentMode = g.Key.ShipmentMode,
                TaxabilityStatus = DetermineTaxability(g.Key.ShipmentClassification, g.Key.ShipmentMode),
                ShipmentCount = g.Count(),
                GrossAmount = g.Sum(f => f.GrossAmount),
                FeeAmount = g.Sum(f => f.EmpostFeeAmount)
            })
            .ToList();

        return breakdown;
    }

    public async Task<QuarterCalculationResult> CalculateQuarterFeesAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        if (quarter.IsLocked)
            throw new InvalidOperationException("Cannot calculate fees for a locked quarter");

        var license = quarter.EmpostLicense;
        if (license == null || !license.IsActive)
            throw new InvalidOperationException("No active license found for this quarter");

        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.BookingDate >= quarter.PeriodStart
                && s.BookingDate <= quarter.PeriodEnd
                && s.Status != ShipmentStatus.Cancelled)
            .ToListAsync();

        var result = new QuarterCalculationResult
        {
            QuarterId = quarterId,
            PeriodStart = quarter.PeriodStart,
            PeriodEnd = quarter.PeriodEnd
        };

        foreach (var shipment in shipments)
        {
            try
            {
                await CalculateAndRecordFeeAsync(shipment);
                result.ProcessedCount++;
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                _logger.LogWarning(ex, "Failed to calculate fee for shipment {AWB}", shipment.AWBNumber);
            }
        }

        var breakdown = await GetQuarterFeeBreakdownAsync(quarterId);
        quarter.TotalShipments = breakdown.TotalShipments;
        quarter.TaxableShipments = breakdown.TaxableShipments;
        quarter.ExemptShipments = breakdown.ExemptShipments;
        quarter.TotalGrossRevenue = breakdown.TotalGrossRevenue;
        quarter.TotalTaxableRevenue = breakdown.TaxableGrossRevenue;
        quarter.TotalExemptRevenue = breakdown.ExemptGrossRevenue;
        quarter.TotalEmpostFee = breakdown.TotalEmpostFee;
        quarter.NetEmpostFee = breakdown.NetEmpostFee;

        await _context.SaveChangesAsync();

        result.TotalShipments = breakdown.TotalShipments;
        result.TaxableShipments = breakdown.TaxableShipments;
        result.ExemptShipments = breakdown.ExemptShipments;
        result.TotalFeeAmount = breakdown.TotalEmpostFee;

        _logger.LogInformation("Calculated fees for quarter {QuarterName}: {Processed} processed, {Errors} errors, Total Fee: {Fee}",
            quarter.QuarterName, result.ProcessedCount, result.ErrorCount, result.TotalFeeAmount);

        return result;
    }
}

public class QuarterCalculationResult
{
    public Guid QuarterId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int ProcessedCount { get; set; }
    public int ErrorCount { get; set; }
    public int TotalShipments { get; set; }
    public int TaxableShipments { get; set; }
    public int ExemptShipments { get; set; }
    public decimal TotalFeeAmount { get; set; }
}

public class QuarterFeeBreakdown
{
    public Guid QuarterId { get; set; }
    public int TotalShipments { get; set; }
    public int TaxableShipments { get; set; }
    public int ExemptShipments { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TaxableGrossRevenue { get; set; }
    public decimal ExemptGrossRevenue { get; set; }
    public decimal TotalEmpostFee { get; set; }
    public decimal ReturnAdjustments { get; set; }
    public decimal NetEmpostFee { get; set; }
    public List<ClassificationBreakdown> ByClassification { get; set; } = new();
    public List<ShipmentTypeBreakdown> ByShipmentType { get; set; } = new();
    public List<ShipmentModeBreakdown> ByMode { get; set; } = new();
    public List<TypeModeBreakdown> ByTypeAndMode { get; set; } = new();
}

public class ClassificationBreakdown
{
    public EmpostClassification Classification { get; set; }
    public int ShipmentCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal FeeAmount { get; set; }
}

public class ShipmentTypeBreakdown
{
    public ShipmentClassificationType ShipmentClassification { get; set; }
    public int ShipmentCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal FeeAmount { get; set; }
}

public class ShipmentModeBreakdown
{
    public ShipmentMode ShipmentMode { get; set; }
    public int ShipmentCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal FeeAmount { get; set; }
}

public class TypeModeBreakdown
{
    public ShipmentClassificationType ShipmentClassification { get; set; }
    public ShipmentMode ShipmentMode { get; set; }
    public EmpostTaxabilityStatus TaxabilityStatus { get; set; }
    public int ShipmentCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal FeeAmount { get; set; }
}
