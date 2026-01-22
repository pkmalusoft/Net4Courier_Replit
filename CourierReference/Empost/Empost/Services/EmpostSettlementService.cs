using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostSettlementService
{
    Task<LicenseFeeStatus> GetLicenseFeeStatusAsync(Guid licenseId);
    Task<QuarterSettlementCalculation> CalculateQuarterSettlementAsync(Guid quarterId);
    Task<EmpostQuarterlySettlement> CreateSettlementAsync(Guid quarterId, Guid userId);
    Task<EmpostQuarterlySettlement> RecordPaymentAsync(Guid settlementId, decimal amountPaid, string paymentMethod, string paymentReference, Guid userId);
    Task<List<EmpostQuarterlySettlement>> GetSettlementsForLicensePeriodAsync(Guid licenseId);
    Task<decimal> GetCumulativeFeeForLicensePeriodAsync(Guid licenseId);
    Task<decimal> GetAdvanceUtilizationAsync(Guid licenseId);
}

public class EmpostSettlementService : IEmpostSettlementService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostFeeCalculationService _feeCalculationService;
    private readonly ILogger<EmpostSettlementService> _logger;

    private const decimal MINIMUM_ADVANCE_AMOUNT = 100000.00m;
    private const decimal VAT_PERCENTAGE = 5.0m;

    public EmpostSettlementService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostFeeCalculationService feeCalculationService,
        ILogger<EmpostSettlementService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _feeCalculationService = feeCalculationService;
        _logger = logger;
    }

    public async Task<LicenseFeeStatus> GetLicenseFeeStatusAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .Include(l => l.AdvancePayments)
            .Include(l => l.Quarters)
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);

        if (license == null)
            throw new ArgumentException("License not found");

        var advancePayments = license.AdvancePayments
            .Where(p => p.LicensePeriodStart >= license.LicensePeriodStart 
                && p.LicensePeriodEnd <= license.LicensePeriodEnd)
            .Sum(p => p.AmountPaid);

        var quarters = await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == licenseId && q.TenantId == tenantId.Value)
            .ToListAsync();

        decimal cumulativeFee = 0;
        decimal settledAmount = 0;

        foreach (var quarter in quarters)
        {
            var breakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(quarter.Id);
            cumulativeFee += breakdown.NetEmpostFee;
        }

        var settlements = await _context.EmpostQuarterlySettlements
            .Where(s => s.EmpostLicenseId == licenseId && s.TenantId == tenantId.Value)
            .ToListAsync();

        settledAmount = settlements.Sum(s => s.AmountPaid);

        var status = new LicenseFeeStatus
        {
            LicenseId = licenseId,
            LicensePeriodStart = license.LicensePeriodStart,
            LicensePeriodEnd = license.LicensePeriodEnd,
            MinimumAdvanceAmount = license.MinimumAdvanceAmount,
            AdvancePaymentsPaid = advancePayments,
            CumulativeFeeToDate = cumulativeFee,
            SettledAmount = settledAmount,
            AdvanceUtilized = Math.Min(cumulativeFee, advancePayments),
            ExcessOverAdvance = Math.Max(0, cumulativeFee - advancePayments),
            BalanceDue = Math.Max(0, cumulativeFee - advancePayments - settledAmount),
            EffectiveTaxRate = cumulativeFee > 0 ? (cumulativeFee / (cumulativeFee / 0.10m)) * 100 : 0
        };

        return status;
    }

    public async Task<QuarterSettlementCalculation> CalculateQuarterSettlementAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        var breakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(quarterId);
        var licenseStatus = await GetLicenseFeeStatusAsync(quarter.EmpostLicenseId);

        var previousQuarters = await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == quarter.EmpostLicenseId 
                && q.TenantId == tenantId.Value
                && (q.Year < quarter.Year || (q.Year == quarter.Year && q.Quarter < quarter.Quarter)))
            .ToListAsync();

        decimal previousFees = 0;
        foreach (var pq in previousQuarters)
        {
            var pqBreakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(pq.Id);
            previousFees += pqBreakdown.NetEmpostFee;
        }

        var previousSettlements = await _context.EmpostQuarterlySettlements
            .Where(s => s.EmpostLicenseId == quarter.EmpostLicenseId 
                && s.TenantId == tenantId.Value
                && s.EmpostQuarterId != quarterId)
            .SumAsync(s => s.AmountPaid);

        var cumulativeFeeToDate = previousFees + breakdown.NetEmpostFee;
        var advanceAmount = quarter.EmpostLicense.MinimumAdvanceAmount;
        var excessOverAdvance = Math.Max(0, cumulativeFeeToDate - advanceAmount);
        var amountPayable = Math.Max(0, excessOverAdvance - previousSettlements);
        var vatOnFee = amountPayable * (VAT_PERCENTAGE / 100);
        var totalPayable = amountPayable + vatOnFee;

        var calculation = new QuarterSettlementCalculation
        {
            QuarterId = quarterId,
            QuarterName = quarter.QuarterName,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            QuarterFeeAmount = breakdown.NetEmpostFee,
            ReturnAdjustments = breakdown.ReturnAdjustments,
            NetQuarterFee = breakdown.NetEmpostFee,
            CumulativeFeeToDate = cumulativeFeeToDate,
            AdvancePaymentAmount = advanceAmount,
            PreviousSettlements = previousSettlements,
            ExcessOverAdvance = excessOverAdvance,
            AmountPayable = amountPayable,
            VATPercentage = VAT_PERCENTAGE,
            VATOnFee = vatOnFee,
            TotalPayable = totalPayable,
            SettlementDueDate = quarter.SubmissionDeadline
        };

        return calculation;
    }

    public async Task<EmpostQuarterlySettlement> CreateSettlementAsync(Guid quarterId, Guid userId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        var existingSettlement = await _context.EmpostQuarterlySettlements
            .FirstOrDefaultAsync(s => s.EmpostQuarterId == quarterId && s.TenantId == tenantId.Value);

        if (existingSettlement != null)
            throw new InvalidOperationException("Settlement already exists for this quarter");

        var calculation = await CalculateQuarterSettlementAsync(quarterId);

        var settlementRef = $"SET-{quarter.Year}-Q{(int)quarter.Quarter}-{DateTime.UtcNow:yyyyMMdd}";

        var settlement = new EmpostQuarterlySettlement
        {
            EmpostQuarterId = quarterId,
            EmpostLicenseId = quarter.EmpostLicenseId,
            SettlementReference = settlementRef,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            CumulativeFeeToDate = calculation.CumulativeFeeToDate,
            AdvancePaymentAmount = calculation.AdvancePaymentAmount,
            PreviousSettlements = calculation.PreviousSettlements,
            QuarterFeeAmount = calculation.QuarterFeeAmount,
            ReturnAdjustments = calculation.ReturnAdjustments,
            NetQuarterFee = calculation.NetQuarterFee,
            ExcessOverAdvance = calculation.ExcessOverAdvance,
            AmountPayable = calculation.AmountPayable,
            VATOnFee = calculation.VATOnFee,
            TotalPayable = calculation.TotalPayable,
            BalanceDue = calculation.TotalPayable,
            Status = EmpostSettlementStatus.Pending,
            SettlementDueDate = calculation.SettlementDueDate,
            RecordedBy = userId,
            RecordedDate = DateTime.UtcNow
        };

        _context.EmpostQuarterlySettlements.Add(settlement);

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.SettlementCreated,
            ActionDescription = $"Settlement created for {quarter.QuarterName} {quarter.Year}: Total Payable AED {calculation.TotalPayable:N2}",
            EntityType = nameof(EmpostQuarterlySettlement),
            EmpostQuarterId = quarterId,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            NewValue = calculation.TotalPayable,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created settlement {Reference} for quarter {Quarter} {Year}: AED {Amount}",
            settlementRef, quarter.QuarterName, quarter.Year, calculation.TotalPayable);

        return settlement;
    }

    public async Task<EmpostQuarterlySettlement> RecordPaymentAsync(
        Guid settlementId, 
        decimal amountPaid, 
        string paymentMethod, 
        string paymentReference, 
        Guid userId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var settlement = await _context.EmpostQuarterlySettlements
            .FirstOrDefaultAsync(s => s.Id == settlementId && s.TenantId == tenantId.Value);

        if (settlement == null)
            throw new ArgumentException("Settlement not found");

        var previouslyPaid = settlement.AmountPaid;
        settlement.AmountPaid += amountPaid;
        settlement.BalanceDue = settlement.TotalPayable - settlement.AmountPaid;
        settlement.PaymentDate = DateTime.UtcNow;
        settlement.PaymentMethod = paymentMethod;
        settlement.PaymentReference = paymentReference;

        if (settlement.BalanceDue <= 0)
        {
            settlement.Status = EmpostSettlementStatus.Paid;
            settlement.BalanceDue = 0;
        }
        else
        {
            settlement.Status = EmpostSettlementStatus.PartiallyPaid;
        }

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.SettlementPaid,
            ActionDescription = $"Payment recorded: AED {amountPaid:N2} via {paymentMethod}",
            EntityType = nameof(EmpostQuarterlySettlement),
            EntityId = settlementId,
            EmpostQuarterId = settlement.EmpostQuarterId,
            Year = settlement.Year,
            Quarter = settlement.Quarter,
            OldValue = previouslyPaid,
            NewValue = settlement.AmountPaid,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            Notes = $"Reference: {paymentReference}"
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded payment of AED {Amount} for settlement {Reference}",
            amountPaid, settlement.SettlementReference);

        return settlement;
    }

    public async Task<List<EmpostQuarterlySettlement>> GetSettlementsForLicensePeriodAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostQuarterlySettlement>();

        return await _context.EmpostQuarterlySettlements
            .Include(s => s.EmpostQuarter)
            .Where(s => s.EmpostLicenseId == licenseId && s.TenantId == tenantId.Value)
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Quarter)
            .ToListAsync();
    }

    public async Task<decimal> GetCumulativeFeeForLicensePeriodAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return 0;

        var quarters = await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == licenseId && q.TenantId == tenantId.Value)
            .ToListAsync();

        decimal cumulativeFee = 0;
        foreach (var quarter in quarters)
        {
            var breakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(quarter.Id);
            cumulativeFee += breakdown.NetEmpostFee;
        }

        return cumulativeFee;
    }

    public async Task<decimal> GetAdvanceUtilizationAsync(Guid licenseId)
    {
        var status = await GetLicenseFeeStatusAsync(licenseId);
        return status.AdvanceUtilized;
    }
}

public class LicenseFeeStatus
{
    public Guid LicenseId { get; set; }
    public DateTime LicensePeriodStart { get; set; }
    public DateTime LicensePeriodEnd { get; set; }
    public decimal MinimumAdvanceAmount { get; set; }
    public decimal AdvancePaymentsPaid { get; set; }
    public decimal CumulativeFeeToDate { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal AdvanceUtilized { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal BalanceDue { get; set; }
    public decimal EffectiveTaxRate { get; set; }
    public decimal AdvanceUtilizationPercentage => MinimumAdvanceAmount > 0 
        ? (AdvanceUtilized / MinimumAdvanceAmount) * 100 
        : 0;
}

public class QuarterSettlementCalculation
{
    public Guid QuarterId { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public int Year { get; set; }
    public QuarterNumber Quarter { get; set; }
    public decimal QuarterFeeAmount { get; set; }
    public decimal ReturnAdjustments { get; set; }
    public decimal NetQuarterFee { get; set; }
    public decimal CumulativeFeeToDate { get; set; }
    public decimal AdvancePaymentAmount { get; set; }
    public decimal PreviousSettlements { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal AmountPayable { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATOnFee { get; set; }
    public decimal TotalPayable { get; set; }
    public DateTime SettlementDueDate { get; set; }
}
