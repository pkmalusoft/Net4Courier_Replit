using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Finance.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public interface IEmpostService
{
    Task<EmpostLicense?> GetActiveLicenseAsync();
    Task<List<EmpostLicense>> GetAllLicensesAsync();
    Task<EmpostLicense> CreateLicenseAsync(EmpostLicense license, int userId, string userName);
    Task<EmpostLicense> UpdateLicenseAsync(EmpostLicense license, int userId, string userName);
    Task<bool> DeleteLicenseAsync(long id);
    
    Task<List<EmpostAdvancePayment>> GetAdvancePaymentsAsync(long licenseId);
    Task<EmpostAdvancePayment> RecordAdvancePaymentAsync(EmpostAdvancePayment payment, int userId, string userName);
    
    Task<List<EmpostQuarter>> GetQuartersAsync(long licenseId, int? year = null);
    Task<EmpostQuarter?> GetCurrentQuarterAsync(long licenseId);
    Task<EmpostQuarter> CreateQuarterAsync(EmpostQuarter quarter, int userId);
    Task<EmpostQuarter> LockQuarterAsync(long quarterId, int userId, string userName);
    Task<EmpostQuarter> UnlockQuarterAsync(long quarterId, int userId, string userName, string reason);
    Task<EmpostQuarter> SubmitQuarterAsync(long quarterId, int userId, string userName);
    Task<QuarterCalculationResult> CalculateQuarterFeesAsync(long quarterId);
    
    Task<List<EmpostShipmentFee>> GetQuarterShipmentFeesAsync(long quarterId);
    Task<EmpostShipmentFee> CalculateShipmentFeeAsync(long inscanMasterId, long quarterId, decimal royaltyPercentage);
    
    Task<List<EmpostQuarterlySettlement>> GetSettlementsAsync(long licenseId);
    Task<EmpostQuarterlySettlement> CreateSettlementAsync(long quarterId, int userId, string userName);
    Task<EmpostQuarterlySettlement> RecordSettlementPaymentAsync(long settlementId, decimal amountPaid, string paymentMethod, string paymentReference, int userId, string userName);
    
    Task<List<EmpostReturnAdjustment>> GetReturnAdjustmentsAsync(long? quarterId = null);
    Task<EmpostReturnAdjustment> CreateReturnAdjustmentAsync(EmpostReturnAdjustment adjustment, int userId, string userName);
    Task<int> ProcessPendingAdjustmentsAsync(int userId, string userName);
    
    Task<List<EmpostAuditLog>> GetAuditLogsAsync(long? licenseId = null, long? quarterId = null, int limit = 100);
    Task LogAuditAsync(EmpostAuditAction action, string description, long? licenseId, long? quarterId, int? userId, string? userName, string? awbNumber = null, decimal? oldValue = null, decimal? newValue = null);
    
    Task<EmpostDashboardData> GetDashboardDataAsync(long licenseId);
    Task<LicenseFeeStatus> GetLicenseFeeStatusAsync(long licenseId);
}

public class EmpostService : IEmpostService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmpostService> _logger;

    public EmpostService(ApplicationDbContext context, ILogger<EmpostService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmpostLicense?> GetActiveLicenseAsync()
    {
        return await _context.EmpostLicenses
            .Where(l => l.IsActive && !l.IsDeleted)
            .OrderByDescending(l => l.LicensePeriodEnd)
            .FirstOrDefaultAsync();
    }

    public async Task<List<EmpostLicense>> GetAllLicensesAsync()
    {
        return await _context.EmpostLicenses
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.LicensePeriodEnd)
            .ToListAsync();
    }

    public async Task<EmpostLicense> CreateLicenseAsync(EmpostLicense license, int userId, string userName)
    {
        license.CreatedAt = DateTime.UtcNow;
        license.CreatedBy = userId;
        _context.EmpostLicenses.Add(license);
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.LicenseCreated, $"License {license.LicenseNumber} created", license.Id, null, userId, userName);
        
        await GenerateQuartersForLicenseAsync(license, userId);
        
        return license;
    }

    public async Task<EmpostLicense> UpdateLicenseAsync(EmpostLicense license, int userId, string userName)
    {
        license.ModifiedAt = DateTime.UtcNow;
        license.ModifiedBy = userId;
        _context.EmpostLicenses.Update(license);
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.LicenseUpdated, $"License {license.LicenseNumber} updated", license.Id, null, userId, userName);
        
        return license;
    }

    public async Task<bool> DeleteLicenseAsync(long id)
    {
        var license = await _context.EmpostLicenses.FindAsync(id);
        if (license == null) return false;
        
        license.IsDeleted = true;
        license.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task GenerateQuartersForLicenseAsync(EmpostLicense license, int userId)
    {
        var startYear = license.LicensePeriodStart.Year;
        var endYear = license.LicensePeriodEnd.Year;
        
        for (int year = startYear; year <= endYear; year++)
        {
            for (int q = 1; q <= 4; q++)
            {
                var quarterStart = new DateTime(year, (q - 1) * 3 + 1, 1);
                var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
                
                if (quarterEnd < license.LicensePeriodStart || quarterStart > license.LicensePeriodEnd)
                    continue;
                
                var quarter = new EmpostQuarter
                {
                    EmpostLicenseId = license.Id,
                    Year = year,
                    Quarter = (QuarterNumber)q,
                    QuarterName = $"Q{q}",
                    PeriodStart = quarterStart,
                    PeriodEnd = quarterEnd,
                    SubmissionDeadline = quarterEnd.AddDays(45),
                    Status = QuarterStatus.Open,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                
                _context.EmpostQuarters.Add(quarter);
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmpostAdvancePayment>> GetAdvancePaymentsAsync(long licenseId)
    {
        return await _context.EmpostAdvancePayments
            .Where(p => p.EmpostLicenseId == licenseId && !p.IsDeleted)
            .OrderByDescending(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<EmpostAdvancePayment> RecordAdvancePaymentAsync(EmpostAdvancePayment payment, int userId, string userName)
    {
        payment.RecordedBy = userId;
        payment.CreatedAt = DateTime.UtcNow;
        payment.CreatedBy = userId;
        
        if (payment.AmountPaid >= payment.AmountDue)
            payment.Status = AdvancePaymentStatus.Paid;
        else if (payment.AmountPaid > 0)
            payment.Status = AdvancePaymentStatus.PartiallyPaid;
        
        _context.EmpostAdvancePayments.Add(payment);
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.AdvancePaymentRecorded, 
            $"Advance payment of {payment.AmountPaid:N2} recorded", 
            payment.EmpostLicenseId, null, userId, userName, 
            newValue: payment.AmountPaid);
        
        return payment;
    }

    public async Task<List<EmpostQuarter>> GetQuartersAsync(long licenseId, int? year = null)
    {
        var query = _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .Where(q => q.EmpostLicenseId == licenseId && !q.IsDeleted);
        
        if (year.HasValue)
            query = query.Where(q => q.Year == year.Value);
        
        return await query
            .OrderByDescending(q => q.Year)
            .ThenByDescending(q => q.Quarter)
            .ToListAsync();
    }

    public async Task<EmpostQuarter?> GetCurrentQuarterAsync(long licenseId)
    {
        var now = DateTime.UtcNow;
        return await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == licenseId && 
                       q.PeriodStart <= now && q.PeriodEnd >= now && 
                       !q.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<EmpostQuarter> CreateQuarterAsync(EmpostQuarter quarter, int userId)
    {
        quarter.CreatedAt = DateTime.UtcNow;
        quarter.CreatedBy = userId;
        _context.EmpostQuarters.Add(quarter);
        await _context.SaveChangesAsync();
        return quarter;
    }

    public async Task<EmpostQuarter> LockQuarterAsync(long quarterId, int userId, string userName)
    {
        var quarter = await _context.EmpostQuarters.FindAsync(quarterId);
        if (quarter == null)
            throw new ArgumentException("Quarter not found");
        
        if (quarter.IsLocked)
            throw new InvalidOperationException("Quarter is already locked");
        
        quarter.IsLocked = true;
        quarter.LockedDate = DateTime.UtcNow;
        quarter.LockedBy = userId;
        quarter.LockedByName = userName;
        quarter.Status = QuarterStatus.PendingSubmission;
        quarter.ModifiedAt = DateTime.UtcNow;
        quarter.ModifiedBy = userId;
        
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.QuarterLocked, 
            $"Quarter {quarter.QuarterName} {quarter.Year} locked", 
            quarter.EmpostLicenseId, quarter.Id, userId, userName);
        
        return quarter;
    }

    public async Task<EmpostQuarter> UnlockQuarterAsync(long quarterId, int userId, string userName, string reason)
    {
        var quarter = await _context.EmpostQuarters.FindAsync(quarterId);
        if (quarter == null)
            throw new ArgumentException("Quarter not found");
        
        if (!quarter.IsLocked)
            throw new InvalidOperationException("Quarter is not locked");
        
        quarter.IsLocked = false;
        quarter.Status = QuarterStatus.Open;
        quarter.ModifiedAt = DateTime.UtcNow;
        quarter.ModifiedBy = userId;
        
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.QuarterUnlocked, 
            $"Quarter {quarter.QuarterName} {quarter.Year} unlocked. Reason: {reason}", 
            quarter.EmpostLicenseId, quarter.Id, userId, userName);
        
        return quarter;
    }

    public async Task<EmpostQuarter> SubmitQuarterAsync(long quarterId, int userId, string userName)
    {
        var quarter = await _context.EmpostQuarters.FindAsync(quarterId);
        if (quarter == null)
            throw new ArgumentException("Quarter not found");
        
        if (!quarter.IsLocked)
            throw new InvalidOperationException("Quarter must be locked before submission");
        
        if (quarter.Status == QuarterStatus.Submitted)
            throw new InvalidOperationException("Quarter is already submitted");
        
        quarter.Status = QuarterStatus.Submitted;
        quarter.SubmittedDate = DateTime.UtcNow;
        quarter.SubmittedBy = userId;
        quarter.SubmittedByName = userName;
        quarter.ModifiedAt = DateTime.UtcNow;
        quarter.ModifiedBy = userId;
        
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.QuarterSubmitted, 
            $"Quarter {quarter.QuarterName} {quarter.Year} submitted to Empost", 
            quarter.EmpostLicenseId, quarter.Id, userId, userName);
        
        return quarter;
    }

    public async Task<QuarterCalculationResult> CalculateQuarterFeesAsync(long quarterId)
    {
        var quarter = await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.Id == quarterId);
        
        if (quarter == null)
            throw new ArgumentException("Quarter not found");
        
        if (quarter.IsLocked)
            throw new InvalidOperationException("Cannot calculate fees for locked quarter");
        
        var existingFees = await _context.EmpostShipmentFees
            .Where(f => f.EmpostQuarterId == quarterId)
            .ToListAsync();
        _context.EmpostShipmentFees.RemoveRange(existingFees);
        
        var shipments = await _context.InscanMasters
            .Where(s => s.TransactionDate >= quarter.PeriodStart && 
                       s.TransactionDate <= quarter.PeriodEnd &&
                       !s.IsDeleted)
            .ToListAsync();
        
        var royaltyPct = quarter.EmpostLicense.RoyaltyPercentage;
        var weightThreshold = quarter.EmpostLicense.WeightThresholdKg;
        
        decimal totalGross = 0;
        decimal totalTaxable = 0;
        decimal totalExempt = 0;
        decimal totalFee = 0;
        int taxableCount = 0;
        int exemptCount = 0;
        
        foreach (var shipment in shipments)
        {
            var courierCharge = shipment.CourierCharge ?? 0m;
            var fuelSurcharge = shipment.FuelSurcharge ?? 0m;
            var otherCharge = shipment.OtherCharge ?? 0m;
            var codAmount = shipment.CODAmount ?? 0m;
            var grossAmount = courierCharge + fuelSurcharge + otherCharge + codAmount;
            var chargeableWeight = shipment.ChargeableWeight ?? 0m;
            var actualWeight = shipment.Weight ?? shipment.StatedWeight ?? 0m;
            
            var classification = EmpostClassification.Taxable;
            var taxability = EmpostTaxabilityStatus.Taxable;
            
            if (chargeableWeight >= weightThreshold)
            {
                classification = EmpostClassification.FreightOver30Kg;
                taxability = EmpostTaxabilityStatus.NonTaxable;
            }
            else if (shipment.MovementTypeId == MovementType.InternationalImport)
            {
                classification = EmpostClassification.Exempt;
                taxability = EmpostTaxabilityStatus.NonTaxable;
            }
            else if (shipment.MovementTypeId == MovementType.Transhipment)
            {
                classification = EmpostClassification.Exempt;
                taxability = EmpostTaxabilityStatus.NonTaxable;
            }
            
            var feeAmount = taxability == EmpostTaxabilityStatus.Taxable
                ? Math.Round(grossAmount * (royaltyPct / 100), 2)
                : 0;
            
            var shipmentFee = new EmpostShipmentFee
            {
                EmpostQuarterId = quarterId,
                InscanMasterId = shipment.Id,
                AWBNumber = shipment.AWBNo,
                ShipmentDate = shipment.TransactionDate,
                ActualWeight = actualWeight,
                ChargeableWeight = chargeableWeight,
                Classification = classification,
                TaxabilityStatus = taxability,
                FreightCharge = courierCharge,
                FuelSurcharge = fuelSurcharge,
                InsuranceCharge = 0m,
                CODCharge = codAmount,
                OtherCharges = otherCharge,
                GrossAmount = grossAmount,
                RoyaltyPercentage = royaltyPct,
                EmpostFeeAmount = feeAmount,
                FeeStatus = EmpostFeeStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.EmpostShipmentFees.Add(shipmentFee);
            
            totalGross += grossAmount;
            if (taxability == EmpostTaxabilityStatus.Taxable)
            {
                totalTaxable += grossAmount;
                totalFee += feeAmount;
                taxableCount++;
            }
            else
            {
                totalExempt += grossAmount;
                exemptCount++;
            }
        }
        
        var returnAdjustments = await _context.EmpostReturnAdjustments
            .Where(a => a.EmpostQuarterId == quarterId && a.Status == AdjustmentStatus.Applied)
            .SumAsync(a => a.AdjustmentAmount);
        
        quarter.TotalShipments = shipments.Count;
        quarter.TaxableShipments = taxableCount;
        quarter.ExemptShipments = exemptCount;
        quarter.TotalGrossRevenue = totalGross;
        quarter.TotalTaxableRevenue = totalTaxable;
        quarter.TotalExemptRevenue = totalExempt;
        quarter.TotalEmpostFee = totalFee;
        quarter.TotalReturnAdjustments = returnAdjustments;
        quarter.NetEmpostFee = totalFee - returnAdjustments;
        quarter.ModifiedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return new QuarterCalculationResult
        {
            QuarterId = quarterId,
            TotalShipments = shipments.Count,
            TaxableShipments = taxableCount,
            ExemptShipments = exemptCount,
            TotalGrossRevenue = totalGross,
            TotalTaxableRevenue = totalTaxable,
            TotalExemptRevenue = totalExempt,
            TotalEmpostFee = totalFee,
            ReturnAdjustments = returnAdjustments,
            NetEmpostFee = totalFee - returnAdjustments
        };
    }

    public async Task<List<EmpostShipmentFee>> GetQuarterShipmentFeesAsync(long quarterId)
    {
        return await _context.EmpostShipmentFees
            .Where(f => f.EmpostQuarterId == quarterId && !f.IsDeleted)
            .OrderBy(f => f.ShipmentDate)
            .ToListAsync();
    }

    public async Task<EmpostShipmentFee> CalculateShipmentFeeAsync(long inscanMasterId, long quarterId, decimal royaltyPercentage)
    {
        var shipment = await _context.InscanMasters.FindAsync(inscanMasterId);
        if (shipment == null)
            throw new ArgumentException("Shipment not found");
        
        var courierCharge = shipment.CourierCharge ?? 0m;
        var fuelSurcharge = shipment.FuelSurcharge ?? 0m;
        var otherCharge = shipment.OtherCharge ?? 0m;
        var codAmount = shipment.CODAmount ?? 0m;
        var grossAmount = courierCharge + fuelSurcharge + otherCharge + codAmount;
        var chargeableWeight = shipment.ChargeableWeight ?? 0m;
        var actualWeight = shipment.Weight ?? shipment.StatedWeight ?? 0m;
        
        var feeAmount = Math.Round(grossAmount * (royaltyPercentage / 100), 2);
        
        var fee = new EmpostShipmentFee
        {
            EmpostQuarterId = quarterId,
            InscanMasterId = inscanMasterId,
            AWBNumber = shipment.AWBNo,
            ShipmentDate = shipment.TransactionDate,
            ActualWeight = actualWeight,
            ChargeableWeight = chargeableWeight,
            GrossAmount = grossAmount,
            RoyaltyPercentage = royaltyPercentage,
            EmpostFeeAmount = feeAmount,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.EmpostShipmentFees.Add(fee);
        await _context.SaveChangesAsync();
        
        return fee;
    }

    public async Task<List<EmpostQuarterlySettlement>> GetSettlementsAsync(long licenseId)
    {
        return await _context.EmpostQuarterlySettlements
            .Include(s => s.EmpostQuarter)
            .Where(s => s.EmpostLicenseId == licenseId && !s.IsDeleted)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Quarter)
            .ToListAsync();
    }

    public async Task<EmpostQuarterlySettlement> CreateSettlementAsync(long quarterId, int userId, string userName)
    {
        var quarter = await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.Id == quarterId);
        
        if (quarter == null)
            throw new ArgumentException("Quarter not found");
        
        if (quarter.Status != QuarterStatus.Submitted)
            throw new InvalidOperationException("Quarter must be submitted before creating settlement");
        
        var feeStatus = await GetLicenseFeeStatusAsync(quarter.EmpostLicenseId);
        
        var settlement = new EmpostQuarterlySettlement
        {
            EmpostQuarterId = quarterId,
            EmpostLicenseId = quarter.EmpostLicenseId,
            SettlementReference = $"SET-{quarter.Year}-{quarter.QuarterName}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            CumulativeFeeToDate = feeStatus.CumulativeFeeToDate,
            AdvancePaymentAmount = feeStatus.TotalAdvancePayments,
            PreviousSettlements = feeStatus.TotalSettlementsPaid,
            QuarterFeeAmount = quarter.TotalEmpostFee,
            ReturnAdjustments = quarter.TotalReturnAdjustments,
            NetQuarterFee = quarter.NetEmpostFee,
            ExcessOverAdvance = feeStatus.ExcessOverAdvance,
            AmountPayable = Math.Max(0, feeStatus.ExcessOverAdvance),
            VATOnFee = 0,
            TotalPayable = Math.Max(0, feeStatus.ExcessOverAdvance),
            AmountPaid = 0,
            BalanceDue = Math.Max(0, feeStatus.ExcessOverAdvance),
            Status = EmpostSettlementStatus.Pending,
            SettlementDueDate = quarter.SubmissionDeadline.AddDays(30),
            RecordedBy = userId,
            RecordedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        
        _context.EmpostQuarterlySettlements.Add(settlement);
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.SettlementCreated, 
            $"Settlement created for {quarter.QuarterName} {quarter.Year}", 
            quarter.EmpostLicenseId, quarter.Id, userId, userName,
            newValue: settlement.TotalPayable);
        
        return settlement;
    }

    public async Task<EmpostQuarterlySettlement> RecordSettlementPaymentAsync(long settlementId, decimal amountPaid, string paymentMethod, string paymentReference, int userId, string userName)
    {
        var settlement = await _context.EmpostQuarterlySettlements.FindAsync(settlementId);
        if (settlement == null)
            throw new ArgumentException("Settlement not found");
        
        settlement.AmountPaid += amountPaid;
        settlement.BalanceDue = settlement.TotalPayable - settlement.AmountPaid;
        settlement.PaymentDate = DateTime.UtcNow;
        settlement.PaymentMethod = paymentMethod;
        settlement.PaymentReference = paymentReference;
        settlement.ModifiedAt = DateTime.UtcNow;
        settlement.ModifiedBy = userId;
        
        if (settlement.BalanceDue <= 0)
            settlement.Status = EmpostSettlementStatus.Paid;
        else if (settlement.AmountPaid > 0)
            settlement.Status = EmpostSettlementStatus.PartiallyPaid;
        
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.SettlementPaid, 
            $"Payment of {amountPaid:N2} recorded for settlement {settlement.SettlementReference}", 
            settlement.EmpostLicenseId, settlement.EmpostQuarterId, userId, userName,
            newValue: amountPaid);
        
        return settlement;
    }

    public async Task<List<EmpostReturnAdjustment>> GetReturnAdjustmentsAsync(long? quarterId = null)
    {
        var query = _context.EmpostReturnAdjustments
            .Include(a => a.EmpostQuarter)
            .Where(a => !a.IsDeleted);
        
        if (quarterId.HasValue)
            query = query.Where(a => a.EmpostQuarterId == quarterId.Value);
        
        return await query.OrderByDescending(a => a.ReturnDate).ToListAsync();
    }

    public async Task<EmpostReturnAdjustment> CreateReturnAdjustmentAsync(EmpostReturnAdjustment adjustment, int userId, string userName)
    {
        adjustment.CreatedAt = DateTime.UtcNow;
        adjustment.CreatedBy = userId;
        _context.EmpostReturnAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();
        
        await LogAuditAsync(EmpostAuditAction.ReturnAdjustmentCreated, 
            $"Return adjustment created for AWB {adjustment.AWBNumber}", 
            null, adjustment.EmpostQuarterId, userId, userName,
            awbNumber: adjustment.AWBNumber, newValue: adjustment.AdjustmentAmount);
        
        return adjustment;
    }

    public async Task<int> ProcessPendingAdjustmentsAsync(int userId, string userName)
    {
        var pendingAdjustments = await _context.EmpostReturnAdjustments
            .Where(a => a.Status == AdjustmentStatus.Pending && !a.IsDeleted)
            .ToListAsync();
        
        int count = 0;
        foreach (var adjustment in pendingAdjustments)
        {
            var fee = await _context.EmpostShipmentFees.FindAsync(adjustment.EmpostShipmentFeeId);
            if (fee != null)
            {
                fee.IsReturnAdjusted = true;
                fee.AdjustedDate = DateTime.UtcNow;
                fee.FeeStatus = EmpostFeeStatus.Adjusted;
            }
            
            adjustment.Status = AdjustmentStatus.Applied;
            adjustment.AppliedDate = DateTime.UtcNow;
            adjustment.AppliedBy = userId;
            adjustment.AppliedByName = userName;
            count++;
        }
        
        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<List<EmpostAuditLog>> GetAuditLogsAsync(long? licenseId = null, long? quarterId = null, int limit = 100)
    {
        var query = _context.EmpostAuditLogs
            .Where(l => !l.IsDeleted)
            .AsQueryable();
        
        if (licenseId.HasValue)
            query = query.Where(l => l.EmpostLicenseId == licenseId.Value);
        
        if (quarterId.HasValue)
            query = query.Where(l => l.EmpostQuarterId == quarterId.Value);
        
        return await query
            .OrderByDescending(l => l.PerformedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task LogAuditAsync(EmpostAuditAction action, string description, long? licenseId, long? quarterId, int? userId, string? userName, string? awbNumber = null, decimal? oldValue = null, decimal? newValue = null)
    {
        var log = new EmpostAuditLog
        {
            Action = action,
            ActionDescription = description,
            EmpostLicenseId = licenseId,
            EmpostQuarterId = quarterId,
            AWBNumber = awbNumber,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        
        _context.EmpostAuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<EmpostDashboardData> GetDashboardDataAsync(long licenseId)
    {
        var license = await _context.EmpostLicenses.FindAsync(licenseId);
        if (license == null)
            return new EmpostDashboardData { HasActiveLicense = false };
        
        var currentQuarter = await GetCurrentQuarterAsync(licenseId);
        var feeStatus = await GetLicenseFeeStatusAsync(licenseId);
        var pendingSubmissions = await _context.EmpostQuarters
            .CountAsync(q => q.EmpostLicenseId == licenseId && q.Status == QuarterStatus.PendingSubmission);
        
        return new EmpostDashboardData
        {
            HasActiveLicense = true,
            LicenseNumber = license.LicenseNumber,
            LicensePeriodStart = license.LicensePeriodStart,
            LicensePeriodEnd = license.LicensePeriodEnd,
            CurrentQuarterName = currentQuarter?.QuarterName ?? "N/A",
            CurrentYear = currentQuarter?.Year ?? DateTime.UtcNow.Year,
            DaysUntilSubmission = currentQuarter != null 
                ? (int)(currentQuarter.SubmissionDeadline - DateTime.UtcNow).TotalDays 
                : 0,
            SubmissionDeadline = currentQuarter?.SubmissionDeadline ?? DateTime.UtcNow,
            IsCurrentQuarterLocked = currentQuarter?.IsLocked ?? false,
            MinimumAdvanceAmount = license.MinimumAdvanceAmount,
            RoyaltyPercentage = license.RoyaltyPercentage,
            CumulativeFeeToDate = feeStatus.CumulativeFeeToDate,
            AdvanceUtilized = feeStatus.AdvanceUtilized,
            ExcessOverAdvance = feeStatus.ExcessOverAdvance,
            BalanceDue = feeStatus.BalanceDue,
            CurrentQuarterShipments = currentQuarter?.TotalShipments ?? 0,
            CurrentQuarterTaxableShipments = currentQuarter?.TaxableShipments ?? 0,
            CurrentQuarterGrossRevenue = currentQuarter?.TotalGrossRevenue ?? 0,
            CurrentQuarterFee = currentQuarter?.NetEmpostFee ?? 0,
            QuartersPendingSubmission = pendingSubmissions
        };
    }

    public async Task<LicenseFeeStatus> GetLicenseFeeStatusAsync(long licenseId)
    {
        var totalAdvance = await _context.EmpostAdvancePayments
            .Where(p => p.EmpostLicenseId == licenseId && p.Status == AdvancePaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;
        
        var cumulativeFee = await _context.EmpostQuarters
            .Where(q => q.EmpostLicenseId == licenseId && q.Status == QuarterStatus.Submitted)
            .SumAsync(q => (decimal?)q.NetEmpostFee) ?? 0;
        
        var totalPaid = await _context.EmpostQuarterlySettlements
            .Where(s => s.EmpostLicenseId == licenseId)
            .SumAsync(s => (decimal?)s.AmountPaid) ?? 0;
        
        var advanceUtilized = Math.Min(totalAdvance, cumulativeFee);
        var excessOverAdvance = Math.Max(0, cumulativeFee - totalAdvance);
        var balanceDue = Math.Max(0, excessOverAdvance - totalPaid);
        
        return new LicenseFeeStatus
        {
            TotalAdvancePayments = totalAdvance,
            CumulativeFeeToDate = cumulativeFee,
            AdvanceUtilized = advanceUtilized,
            ExcessOverAdvance = excessOverAdvance,
            TotalSettlementsPaid = totalPaid,
            BalanceDue = balanceDue
        };
    }
}

public class QuarterCalculationResult
{
    public long QuarterId { get; set; }
    public int TotalShipments { get; set; }
    public int TaxableShipments { get; set; }
    public int ExemptShipments { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalTaxableRevenue { get; set; }
    public decimal TotalExemptRevenue { get; set; }
    public decimal TotalEmpostFee { get; set; }
    public decimal ReturnAdjustments { get; set; }
    public decimal NetEmpostFee { get; set; }
}

public class EmpostDashboardData
{
    public bool HasActiveLicense { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicensePeriodStart { get; set; }
    public DateTime LicensePeriodEnd { get; set; }
    public string CurrentQuarterName { get; set; } = string.Empty;
    public int CurrentYear { get; set; }
    public int DaysUntilSubmission { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public bool IsCurrentQuarterLocked { get; set; }
    public decimal MinimumAdvanceAmount { get; set; }
    public decimal RoyaltyPercentage { get; set; }
    public decimal CumulativeFeeToDate { get; set; }
    public decimal AdvanceUtilized { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal BalanceDue { get; set; }
    public int CurrentQuarterShipments { get; set; }
    public int CurrentQuarterTaxableShipments { get; set; }
    public decimal CurrentQuarterGrossRevenue { get; set; }
    public decimal CurrentQuarterFee { get; set; }
    public int QuartersPendingSubmission { get; set; }
}

public class LicenseFeeStatus
{
    public decimal TotalAdvancePayments { get; set; }
    public decimal CumulativeFeeToDate { get; set; }
    public decimal AdvanceUtilized { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal TotalSettlementsPaid { get; set; }
    public decimal BalanceDue { get; set; }
}
