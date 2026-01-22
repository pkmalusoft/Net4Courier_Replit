using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Empost.Models;
using Server.Modules.GeneralLedger.Models;
using Shared.Enums;

namespace Server.Modules.Empost.Services;

public interface IEmpostReportingService
{
    Task<EmpostQuarterlyReport> GenerateQuarterlyReportAsync(Guid quarterId);
    Task<EmpostAnnualReport> GenerateAnnualReportAsync(Guid licenseId, int year);
    Task<List<EmpostShipmentFee>> GetQuarterShipmentDetailsAsync(Guid quarterId);
    Task<EmpostReconciliationResult> ReconcileWithGLAsync(Guid quarterId);
}

public class EmpostReportingService : IEmpostReportingService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostFeeCalculationService _feeCalculationService;
    private readonly IEmpostSettlementService _settlementService;
    private readonly ILogger<EmpostReportingService> _logger;

    public EmpostReportingService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostFeeCalculationService feeCalculationService,
        IEmpostSettlementService settlementService,
        ILogger<EmpostReportingService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _feeCalculationService = feeCalculationService;
        _settlementService = settlementService;
        _logger = logger;
    }

    public async Task<EmpostQuarterlyReport> GenerateQuarterlyReportAsync(Guid quarterId)
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
        var settlementCalc = await _settlementService.CalculateQuarterSettlementAsync(quarterId);

        var shipmentDetails = await GetQuarterShipmentDetailsAsync(quarterId);
        var adjustments = await _context.EmpostReturnAdjustments
            .Where(a => a.EmpostQuarterId == quarterId && a.TenantId == tenantId.Value)
            .ToListAsync();

        var report = new EmpostQuarterlyReport
        {
            QuarterId = quarterId,
            LicenseNumber = quarter.EmpostLicense.LicenseNumber,
            LicenseeName = quarter.EmpostLicense.LicenseeName ?? string.Empty,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            QuarterName = quarter.QuarterName,
            PeriodStart = quarter.PeriodStart,
            PeriodEnd = quarter.PeriodEnd,
            SubmissionDeadline = quarter.SubmissionDeadline,
            ReportGeneratedAt = DateTime.UtcNow,
            
            TotalShipments = breakdown.TotalShipments,
            TaxableShipments = breakdown.TaxableShipments,
            ExemptShipments = breakdown.ExemptShipments,
            
            TotalGrossRevenue = breakdown.TotalGrossRevenue,
            TaxableGrossRevenue = breakdown.TaxableGrossRevenue,
            ExemptGrossRevenue = breakdown.ExemptGrossRevenue,
            
            GrossEmpostFee = breakdown.TotalEmpostFee,
            ReturnAdjustments = breakdown.ReturnAdjustments,
            NetEmpostFee = breakdown.NetEmpostFee,
            
            CumulativeFeeToDate = settlementCalc.CumulativeFeeToDate,
            AdvancePaymentAmount = settlementCalc.AdvancePaymentAmount,
            ExcessOverAdvance = settlementCalc.ExcessOverAdvance,
            AmountPayableThisQuarter = settlementCalc.AmountPayable,
            VATOnFee = settlementCalc.VATOnFee,
            TotalPayable = settlementCalc.TotalPayable,
            
            ShipmentDetails = shipmentDetails.Select(f => new ShipmentFeeDetail
            {
                AWBNumber = f.AWBNumber,
                ShipmentDate = f.ShipmentDate,
                Classification = f.Classification,
                ShipmentClassification = f.ShipmentClassification,
                ShipmentMode = f.ShipmentMode,
                TaxabilityStatus = f.TaxabilityStatus,
                Weight = f.ChargeableWeight,
                FreightCharge = f.FreightCharge,
                FuelSurcharge = f.FuelSurcharge,
                InsuranceCharge = f.InsuranceCharge,
                CODCharge = f.CODCharge,
                OtherCharges = f.OtherCharges,
                GrossAmount = f.GrossAmount,
                EmpostFee = f.EmpostFeeAmount,
                IsAdjusted = f.IsReturnAdjusted
            }).ToList(),
            
            AdjustmentDetails = adjustments.Select(a => new AdjustmentDetail
            {
                AWBNumber = a.AWBNumber,
                OriginalShipmentDate = a.OriginalShipmentDate,
                ReturnDate = a.ReturnDate,
                OriginalFeeAmount = a.OriginalFeeAmount,
                AdjustmentAmount = a.AdjustmentAmount,
                Reason = a.Reason,
                Status = a.Status
            }).ToList(),
            
            ClassificationBreakdown = breakdown.ByClassification,
            ShipmentTypeBreakdown = breakdown.ByShipmentType,
            ShipmentModeBreakdown = breakdown.ByMode,
            TypeModeBreakdown = breakdown.ByTypeAndMode
        };

        quarter.TotalGrossRevenue = breakdown.TotalGrossRevenue;
        quarter.TotalTaxableRevenue = breakdown.TaxableGrossRevenue;
        quarter.TotalExemptRevenue = breakdown.ExemptGrossRevenue;
        quarter.TotalEmpostFee = breakdown.TotalEmpostFee;
        quarter.TotalReturnAdjustments = breakdown.ReturnAdjustments;
        quarter.NetEmpostFee = breakdown.NetEmpostFee;
        quarter.TotalShipments = breakdown.TotalShipments;
        quarter.TaxableShipments = breakdown.TaxableShipments;
        quarter.ExemptShipments = breakdown.ExemptShipments;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated quarterly report for {Quarter} {Year}", quarter.QuarterName, quarter.Year);

        return report;
    }

    public async Task<EmpostAnnualReport> GenerateAnnualReportAsync(Guid licenseId, int year)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);

        if (license == null)
            throw new ArgumentException("License not found");

        var quarters = await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == licenseId && q.Year == year && q.TenantId == tenantId.Value)
            .OrderBy(q => q.Quarter)
            .ToListAsync();

        var quarterReports = new List<EmpostQuarterlyReport>();
        foreach (var quarter in quarters)
        {
            var report = await GenerateQuarterlyReportAsync(quarter.Id);
            quarterReports.Add(report);
        }

        var annualReport = new EmpostAnnualReport
        {
            LicenseId = licenseId,
            LicenseNumber = license.LicenseNumber,
            Year = year,
            LicensePeriodStart = license.LicensePeriodStart,
            LicensePeriodEnd = license.LicensePeriodEnd,
            
            TotalShipments = quarterReports.Sum(r => r.TotalShipments),
            TaxableShipments = quarterReports.Sum(r => r.TaxableShipments),
            ExemptShipments = quarterReports.Sum(r => r.ExemptShipments),
            
            TotalGrossRevenue = quarterReports.Sum(r => r.TotalGrossRevenue),
            TaxableGrossRevenue = quarterReports.Sum(r => r.TaxableGrossRevenue),
            ExemptGrossRevenue = quarterReports.Sum(r => r.ExemptGrossRevenue),
            
            GrossEmpostFee = quarterReports.Sum(r => r.GrossEmpostFee),
            TotalReturnAdjustments = quarterReports.Sum(r => r.ReturnAdjustments),
            NetEmpostFee = quarterReports.Sum(r => r.NetEmpostFee),
            
            MinimumAdvanceAmount = license.MinimumAdvanceAmount,
            TotalPaid = quarterReports.Sum(r => r.TotalPayable),
            
            QuarterlyReports = quarterReports
        };

        return annualReport;
    }

    public async Task<List<EmpostShipmentFee>> GetQuarterShipmentDetailsAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostShipmentFee>();

        return await _context.EmpostShipmentFees
            .Include(f => f.Shipment)
            .Where(f => f.EmpostQuarterId == quarterId && f.TenantId == tenantId.Value)
            .OrderBy(f => f.ShipmentDate)
            .ToListAsync();
    }

    public async Task<EmpostReconciliationResult> ReconcileWithGLAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        var breakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(quarterId);

        var startDate = DateTime.SpecifyKind(quarter.PeriodStart, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(quarter.PeriodEnd, DateTimeKind.Utc);

        var revenueAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId.Value 
                && a.AccountType == AccountType.Revenue
                && a.IsActive)
            .ToListAsync();

        decimal glRevenue = 0;
        foreach (var account in revenueAccounts)
        {
            var journalLines = await _context.JournalEntryLines
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id 
                    && l.TenantId == tenantId.Value
                    && l.JournalEntry.EntryDate >= startDate
                    && l.JournalEntry.EntryDate <= endDate
                    && l.JournalEntry.Status == JournalEntryStatus.Posted)
                .ToListAsync();

            glRevenue += journalLines.Sum(l => l.Credit - l.Debit);
        }

        var courierRevenue = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.BookingDate >= startDate
                && s.BookingDate <= endDate
                && !s.IsVoided)
            .SumAsync(s => s.TotalCharge);

        var variance = Math.Abs(breakdown.TotalGrossRevenue - courierRevenue);
        var isReconciled = variance < 0.01m;

        var result = new EmpostReconciliationResult
        {
            QuarterId = quarterId,
            QuarterName = quarter.QuarterName,
            Year = quarter.Year,
            PeriodStart = quarter.PeriodStart,
            PeriodEnd = quarter.PeriodEnd,
            EmpostGrossRevenue = breakdown.TotalGrossRevenue,
            CourierModuleRevenue = courierRevenue,
            GLRevenue = glRevenue,
            VarianceWithCourier = breakdown.TotalGrossRevenue - courierRevenue,
            VarianceWithGL = breakdown.TotalGrossRevenue - glRevenue,
            IsReconciled = isReconciled,
            ReconciliationDate = DateTime.UtcNow,
            Notes = isReconciled 
                ? "Figures reconciled successfully" 
                : $"Variance detected: Empost={breakdown.TotalGrossRevenue:N2}, Courier={courierRevenue:N2}, GL={glRevenue:N2}"
        };

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.ReconciliationPerformed,
            ActionDescription = result.IsReconciled 
                ? $"Reconciliation passed for {quarter.QuarterName} {quarter.Year}" 
                : $"Reconciliation variance detected for {quarter.QuarterName} {quarter.Year}",
            EmpostQuarterId = quarterId,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            PerformedAt = DateTime.UtcNow,
            Notes = result.Notes
        });

        await _context.SaveChangesAsync();

        return result;
    }
}

public class EmpostQuarterlyReport
{
    public Guid QuarterId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public QuarterNumber Quarter { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public DateTime ReportGeneratedAt { get; set; }
    
    public int TotalShipments { get; set; }
    public int TaxableShipments { get; set; }
    public int ExemptShipments { get; set; }
    
    public decimal TotalGrossRevenue { get; set; }
    public decimal TaxableGrossRevenue { get; set; }
    public decimal ExemptGrossRevenue { get; set; }
    
    public decimal GrossEmpostFee { get; set; }
    public decimal ReturnAdjustments { get; set; }
    public decimal NetEmpostFee { get; set; }
    
    public decimal CumulativeFeeToDate { get; set; }
    public decimal AdvancePaymentAmount { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal AmountPayableThisQuarter { get; set; }
    public decimal VATOnFee { get; set; }
    public decimal TotalPayable { get; set; }
    
    public List<ShipmentFeeDetail> ShipmentDetails { get; set; } = new();
    public List<AdjustmentDetail> AdjustmentDetails { get; set; } = new();
    public List<ClassificationBreakdown> ClassificationBreakdown { get; set; } = new();
    public List<ShipmentTypeBreakdown> ShipmentTypeBreakdown { get; set; } = new();
    public List<ShipmentModeBreakdown> ShipmentModeBreakdown { get; set; } = new();
    public List<TypeModeBreakdown> TypeModeBreakdown { get; set; } = new();
}

public class ShipmentFeeDetail
{
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime ShipmentDate { get; set; }
    public EmpostClassification Classification { get; set; }
    public ShipmentClassificationType ShipmentClassification { get; set; }
    public ShipmentMode ShipmentMode { get; set; }
    public EmpostTaxabilityStatus TaxabilityStatus { get; set; }
    public decimal Weight { get; set; }
    public decimal FreightCharge { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal InsuranceCharge { get; set; }
    public decimal CODCharge { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal EmpostFee { get; set; }
    public bool IsAdjusted { get; set; }
}

public class AdjustmentDetail
{
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime OriginalShipmentDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal OriginalFeeAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AdjustmentStatus Status { get; set; }
}

public class EmpostAnnualReport
{
    public Guid LicenseId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime LicensePeriodStart { get; set; }
    public DateTime LicensePeriodEnd { get; set; }
    
    public int TotalShipments { get; set; }
    public int TaxableShipments { get; set; }
    public int ExemptShipments { get; set; }
    
    public decimal TotalGrossRevenue { get; set; }
    public decimal TaxableGrossRevenue { get; set; }
    public decimal ExemptGrossRevenue { get; set; }
    
    public decimal GrossEmpostFee { get; set; }
    public decimal TotalReturnAdjustments { get; set; }
    public decimal NetEmpostFee { get; set; }
    
    public decimal MinimumAdvanceAmount { get; set; }
    public decimal TotalPaid { get; set; }
    
    public List<EmpostQuarterlyReport> QuarterlyReports { get; set; } = new();
}

public class EmpostReconciliationResult
{
    public Guid QuarterId { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal EmpostGrossRevenue { get; set; }
    public decimal CourierModuleRevenue { get; set; }
    public decimal GLRevenue { get; set; }
    public decimal VarianceWithCourier { get; set; }
    public decimal VarianceWithGL { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
