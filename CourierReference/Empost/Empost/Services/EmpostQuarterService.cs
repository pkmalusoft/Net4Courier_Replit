using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostQuarterService
{
    Task<EmpostQuarter?> GetCurrentQuarterAsync();
    Task<EmpostQuarter?> GetQuarterForDateAsync(DateTime date);
    Task<List<EmpostQuarter>> GetQuartersForLicensePeriodAsync(Guid licenseId);
    Task<List<EmpostQuarter>> GetQuartersForYearAsync(int year);
    Task<EmpostQuarter> CreateOrGetQuarterAsync(int year, QuarterNumber quarter, Guid licenseId);
    Task<EmpostQuarter> EnsureQuartersExistAsync(Guid licenseId, DateTime fromDate, DateTime toDate);
    QuarterInfo GetQuarterInfo(DateTime date);
    DateTime GetQuarterStartDate(int year, QuarterNumber quarter);
    DateTime GetQuarterEndDate(int year, QuarterNumber quarter);
    DateTime GetSubmissionDeadline(int year, QuarterNumber quarter);
    bool IsQuarterLocked(EmpostQuarter quarter);
    Task<bool> IsDateInLockedQuarterAsync(DateTime date);
    int GetDaysUntilSubmissionDeadline(int year, QuarterNumber quarter);
}

public class EmpostQuarterService : IEmpostQuarterService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<EmpostQuarterService> _logger;

    public EmpostQuarterService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        ILogger<EmpostQuarterService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<EmpostQuarter?> GetCurrentQuarterAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var now = DateTime.UtcNow;
        var quarterInfo = GetQuarterInfo(now);

        return await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.TenantId == tenantId.Value
                && q.Year == quarterInfo.Year
                && q.Quarter == quarterInfo.Quarter);
    }

    public async Task<EmpostQuarter?> GetQuarterForDateAsync(DateTime date)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var quarterInfo = GetQuarterInfo(date);

        return await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.TenantId == tenantId.Value
                && q.Year == quarterInfo.Year
                && q.Quarter == quarterInfo.Quarter);
    }

    public async Task<List<EmpostQuarter>> GetQuartersForLicensePeriodAsync(Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostQuarter>();

        return await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId.Value && q.EmpostLicenseId == licenseId)
            .OrderBy(q => q.Year)
            .ThenBy(q => q.Quarter)
            .ToListAsync();
    }

    public async Task<List<EmpostQuarter>> GetQuartersForYearAsync(int year)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostQuarter>();

        return await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId.Value && q.Year == year)
            .OrderBy(q => q.Quarter)
            .ToListAsync();
    }

    public async Task<EmpostQuarter> CreateOrGetQuarterAsync(int year, QuarterNumber quarter, Guid licenseId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var existing = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.TenantId == tenantId.Value
                && q.Year == year
                && q.Quarter == quarter);

        if (existing != null)
            return existing;

        var startDate = GetQuarterStartDate(year, quarter);
        var endDate = GetQuarterEndDate(year, quarter);
        var submissionDeadline = GetSubmissionDeadline(year, quarter);

        var newQuarter = new EmpostQuarter
        {
            EmpostLicenseId = licenseId,
            Year = year,
            Quarter = quarter,
            QuarterName = GetQuarterName(quarter),
            PeriodStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            PeriodEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
            SubmissionDeadline = DateTime.SpecifyKind(submissionDeadline, DateTimeKind.Utc),
            Status = QuarterStatus.Open,
            IsLocked = false
        };

        _context.EmpostQuarters.Add(newQuarter);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created Empost quarter {Quarter} {Year} for license {LicenseId}",
            quarter, year, licenseId);

        return newQuarter;
    }

    public async Task<EmpostQuarter> EnsureQuartersExistAsync(Guid licenseId, DateTime fromDate, DateTime toDate)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var currentDate = fromDate;
        EmpostQuarter? lastQuarter = null;

        while (currentDate <= toDate)
        {
            var quarterInfo = GetQuarterInfo(currentDate);
            lastQuarter = await CreateOrGetQuarterAsync(quarterInfo.Year, quarterInfo.Quarter, licenseId);
            
            currentDate = GetQuarterEndDate(quarterInfo.Year, quarterInfo.Quarter).AddDays(1);
        }

        return lastQuarter!;
    }

    public QuarterInfo GetQuarterInfo(DateTime date)
    {
        var month = date.Month;
        var year = date.Year;

        QuarterNumber quarter = month switch
        {
            >= 1 and <= 3 => QuarterNumber.Q1,
            >= 4 and <= 6 => QuarterNumber.Q2,
            >= 7 and <= 9 => QuarterNumber.Q3,
            _ => QuarterNumber.Q4
        };

        return new QuarterInfo
        {
            Year = year,
            Quarter = quarter,
            QuarterName = GetQuarterName(quarter),
            StartDate = GetQuarterStartDate(year, quarter),
            EndDate = GetQuarterEndDate(year, quarter),
            SubmissionDeadline = GetSubmissionDeadline(year, quarter)
        };
    }

    public DateTime GetQuarterStartDate(int year, QuarterNumber quarter)
    {
        return quarter switch
        {
            QuarterNumber.Q1 => new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q2 => new DateTime(year, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q3 => new DateTime(year, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q4 => new DateTime(year, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(quarter))
        };
    }

    public DateTime GetQuarterEndDate(int year, QuarterNumber quarter)
    {
        return quarter switch
        {
            QuarterNumber.Q1 => new DateTime(year, 3, 31, 23, 59, 59, DateTimeKind.Utc),
            QuarterNumber.Q2 => new DateTime(year, 6, 30, 23, 59, 59, DateTimeKind.Utc),
            QuarterNumber.Q3 => new DateTime(year, 9, 30, 23, 59, 59, DateTimeKind.Utc),
            QuarterNumber.Q4 => new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(quarter))
        };
    }

    public DateTime GetSubmissionDeadline(int year, QuarterNumber quarter)
    {
        return quarter switch
        {
            QuarterNumber.Q1 => new DateTime(year, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q2 => new DateTime(year, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q3 => new DateTime(year, 10, 15, 0, 0, 0, DateTimeKind.Utc),
            QuarterNumber.Q4 => new DateTime(year + 1, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(quarter))
        };
    }

    public bool IsQuarterLocked(EmpostQuarter quarter)
    {
        return quarter.IsLocked || quarter.Status == QuarterStatus.Locked;
    }

    public async Task<bool> IsDateInLockedQuarterAsync(DateTime date)
    {
        var quarter = await GetQuarterForDateAsync(date);
        if (quarter == null)
            return false;

        return IsQuarterLocked(quarter);
    }

    public int GetDaysUntilSubmissionDeadline(int year, QuarterNumber quarter)
    {
        var deadline = GetSubmissionDeadline(year, quarter);
        var today = DateTime.UtcNow.Date;
        return (deadline.Date - today).Days;
    }

    private string GetQuarterName(QuarterNumber quarter)
    {
        return quarter switch
        {
            QuarterNumber.Q1 => "Jan-Mar",
            QuarterNumber.Q2 => "Apr-Jun",
            QuarterNumber.Q3 => "Jul-Sep",
            QuarterNumber.Q4 => "Oct-Dec",
            _ => "Unknown"
        };
    }
}

public class QuarterInfo
{
    public int Year { get; set; }
    public QuarterNumber Quarter { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmissionDeadline { get; set; }
}
