using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostLicenseService
{
    Task<EmpostLicense?> GetActiveLicenseAsync();
    Task<EmpostLicense?> GetLicenseAsync(Guid licenseId);
    Task<List<EmpostLicense>> GetAllLicensesAsync();
    Task<EmpostLicense> CreateLicenseAsync(CreateLicenseRequest request, Guid userId);
    Task<EmpostLicense> UpdateLicenseAsync(Guid licenseId, UpdateLicenseRequest request);
    Task<EmpostLicense> RenewLicenseAsync(Guid licenseId, DateTime newPeriodEnd, Guid userId);
    Task<EmpostAdvancePayment> RecordAdvancePaymentAsync(Guid licenseId, RecordAdvancePaymentRequest request, Guid userId);
    Task<List<EmpostAdvancePayment>> GetAdvancePaymentsAsync(Guid licenseId);
    Task InitializeQuartersForLicenseAsync(Guid licenseId);
}

public class EmpostLicenseService : IEmpostLicenseService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostQuarterService _quarterService;
    private readonly ILogger<EmpostLicenseService> _logger;

    public EmpostLicenseService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostQuarterService quarterService,
        ILogger<EmpostLicenseService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _quarterService = quarterService;
        _logger = logger;
    }

    public async Task<EmpostLicense?> GetActiveLicenseAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.EmpostLicenses
            .Include(l => l.AdvancePayments)
            .FirstOrDefaultAsync(l => l.TenantId == tenantId.Value && l.IsActive);
    }

    public async Task<EmpostLicense?> GetLicenseAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.EmpostLicenses
            .Include(l => l.AdvancePayments)
            .Include(l => l.Quarters)
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);
    }

    public async Task<List<EmpostLicense>> GetAllLicensesAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostLicense>();

        return await _context.EmpostLicenses
            .Where(l => l.TenantId == tenantId.Value)
            .OrderByDescending(l => l.LicensePeriodStart)
            .ToListAsync();
    }

    public async Task<EmpostLicense> CreateLicenseAsync(CreateLicenseRequest request, Guid userId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var existingActive = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.TenantId == tenantId.Value && l.IsActive);

        if (existingActive != null)
        {
            existingActive.IsActive = false;
            existingActive.Status = EmpostLicenseStatus.Expired;
        }

        var licensePeriodStart = new DateTime(request.LicenseDate.Year, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        if (request.LicenseDate.Month < 10)
        {
            licensePeriodStart = new DateTime(request.LicenseDate.Year - 1, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        var licensePeriodEnd = licensePeriodStart.AddYears(1).AddDays(-1);

        var advancePaymentDueDate = licensePeriodStart.AddDays(-1);

        var license = new EmpostLicense
        {
            LicenseNumber = request.LicenseNumber,
            LicenseeName = request.LicenseeName,
            LicenseDate = DateTime.SpecifyKind(request.LicenseDate, DateTimeKind.Utc),
            LicensePeriodStart = DateTime.SpecifyKind(licensePeriodStart, DateTimeKind.Utc),
            LicensePeriodEnd = DateTime.SpecifyKind(licensePeriodEnd, DateTimeKind.Utc),
            AdvancePaymentDueDate = DateTime.SpecifyKind(advancePaymentDueDate, DateTimeKind.Utc),
            MinimumAdvanceAmount = request.MinimumAdvanceAmount ?? 100000.00m,
            RoyaltyPercentage = request.RoyaltyPercentage ?? 10.00m,
            WeightThresholdKg = request.WeightThresholdKg ?? 30.00m,
            Status = EmpostLicenseStatus.Active,
            IsActive = true,
            Notes = request.Notes
        };

        _context.EmpostLicenses.Add(license);

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.LicenseCreated,
            ActionDescription = $"License {license.LicenseNumber} created for period {licensePeriodStart:dd/MM/yyyy} to {licensePeriodEnd:dd/MM/yyyy}",
            EntityType = nameof(EmpostLicense),
            EmpostLicenseId = license.Id,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await InitializeQuartersForLicenseAsync(license.Id);

        _logger.LogInformation("Created Empost license {LicenseNumber} for tenant", license.LicenseNumber);

        return license;
    }

    public async Task<EmpostLicense> UpdateLicenseAsync(Guid licenseId, UpdateLicenseRequest request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);

        if (license == null)
            throw new ArgumentException("License not found");

        if (!string.IsNullOrEmpty(request.LicenseNumber))
            license.LicenseNumber = request.LicenseNumber;

        if (!string.IsNullOrEmpty(request.LicenseeName))
            license.LicenseeName = request.LicenseeName;

        if (request.MinimumAdvanceAmount.HasValue)
            license.MinimumAdvanceAmount = request.MinimumAdvanceAmount.Value;

        if (request.RoyaltyPercentage.HasValue)
            license.RoyaltyPercentage = request.RoyaltyPercentage.Value;

        if (request.WeightThresholdKg.HasValue)
            license.WeightThresholdKg = request.WeightThresholdKg.Value;

        if (!string.IsNullOrEmpty(request.Notes))
            license.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return license;
    }

    public async Task<EmpostLicense> RenewLicenseAsync(Guid licenseId, DateTime newPeriodEnd, Guid userId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);

        if (license == null)
            throw new ArgumentException("License not found");

        var oldPeriodEnd = license.LicensePeriodEnd;
        license.LicensePeriodStart = license.LicensePeriodEnd.AddDays(1);
        license.LicensePeriodEnd = DateTime.SpecifyKind(newPeriodEnd, DateTimeKind.Utc);
        license.AdvancePaymentDueDate = license.LicensePeriodStart.AddDays(-1);
        license.RenewalDate = DateTime.UtcNow;
        license.Status = EmpostLicenseStatus.Active;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.LicenseRenewed,
            ActionDescription = $"License {license.LicenseNumber} renewed until {newPeriodEnd:dd/MM/yyyy}",
            EntityType = nameof(EmpostLicense),
            EntityId = licenseId,
            EmpostLicenseId = licenseId,
            OldData = $"Period end: {oldPeriodEnd:dd/MM/yyyy}",
            NewData = $"Period end: {newPeriodEnd:dd/MM/yyyy}",
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await InitializeQuartersForLicenseAsync(licenseId);

        _logger.LogInformation("Renewed Empost license {LicenseNumber} until {NewPeriodEnd}",
            license.LicenseNumber, newPeriodEnd);

        return license;
    }

    public async Task<EmpostAdvancePayment> RecordAdvancePaymentAsync(
        Guid licenseId, 
        RecordAdvancePaymentRequest request, 
        Guid userId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var license = await _context.EmpostLicenses
            .FirstOrDefaultAsync(l => l.Id == licenseId && l.TenantId == tenantId.Value);

        if (license == null)
            throw new ArgumentException("License not found");

        var payment = new EmpostAdvancePayment
        {
            EmpostLicenseId = licenseId,
            PaymentReference = request.PaymentReference,
            DueDate = license.AdvancePaymentDueDate,
            PaymentDate = DateTime.SpecifyKind(request.PaymentDate, DateTimeKind.Utc),
            AmountDue = license.MinimumAdvanceAmount,
            AmountPaid = request.AmountPaid,
            Status = request.AmountPaid >= license.MinimumAdvanceAmount 
                ? AdvancePaymentStatus.Paid 
                : AdvancePaymentStatus.PartiallyPaid,
            ForLicenseYear = license.LicensePeriodStart.Year,
            LicensePeriodStart = license.LicensePeriodStart,
            LicensePeriodEnd = license.LicensePeriodEnd,
            PaymentMethod = request.PaymentMethod,
            BankReference = request.BankReference,
            Notes = request.Notes,
            RecordedBy = userId
        };

        _context.EmpostAdvancePayments.Add(payment);

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.AdvancePaymentRecorded,
            ActionDescription = $"Advance payment of AED {request.AmountPaid:N2} recorded for license {license.LicenseNumber}",
            EntityType = nameof(EmpostAdvancePayment),
            EmpostLicenseId = licenseId,
            NewValue = request.AmountPaid,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            Notes = $"Reference: {request.PaymentReference}"
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded advance payment of AED {Amount} for license {LicenseNumber}",
            request.AmountPaid, license.LicenseNumber);

        return payment;
    }

    public async Task<List<EmpostAdvancePayment>> GetAdvancePaymentsAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostAdvancePayment>();

        return await _context.EmpostAdvancePayments
            .Where(p => p.EmpostLicenseId == licenseId && p.TenantId == tenantId.Value)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task InitializeQuartersForLicenseAsync(Guid licenseId)
    {
        var license = await GetLicenseAsync(licenseId);
        if (license == null)
            throw new ArgumentException("License not found");

        await _quarterService.EnsureQuartersExistAsync(
            licenseId,
            license.LicensePeriodStart,
            license.LicensePeriodEnd);

        _logger.LogInformation("Initialized quarters for license {LicenseNumber}", license.LicenseNumber);
    }
}

public class CreateLicenseRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string? LicenseeName { get; set; }
    public DateTime LicenseDate { get; set; }
    public decimal? MinimumAdvanceAmount { get; set; }
    public decimal? RoyaltyPercentage { get; set; }
    public decimal? WeightThresholdKg { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLicenseRequest
{
    public string? LicenseNumber { get; set; }
    public string? LicenseeName { get; set; }
    public decimal? MinimumAdvanceAmount { get; set; }
    public decimal? RoyaltyPercentage { get; set; }
    public decimal? WeightThresholdKg { get; set; }
    public string? Notes { get; set; }
}

public class RecordAdvancePaymentRequest
{
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BankReference { get; set; }
    public string? Notes { get; set; }
}
