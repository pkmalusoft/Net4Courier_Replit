using Net4Courier.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Net4Courier.Web.Services;

public interface IDateTimeService
{
    DateTime ToLocalTime(DateTime utcDateTime);
    DateTime ToLocalTime(DateTime? utcDateTime, DateTime defaultValue);
    string FormatDateTime(DateTime? utcDateTime, string format = "dd/MM/yyyy HH:mm");
    string FormatDate(DateTime? utcDateTime, string format = "dd/MM/yyyy");
    string FormatTime(DateTime? utcDateTime, string format = "HH:mm");
    TimeZoneInfo GetTimeZone();
    void SetTimeZone(string timeZoneId);
}

public class DateTimeService : IDateTimeService
{
    private TimeZoneInfo _timeZone;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _initialized = false;

    public DateTimeService(IDbContextFactory<ApplicationDbContext> dbFactory, IHttpContextAccessor httpContextAccessor)
    {
        _dbFactory = dbFactory;
        _httpContextAccessor = httpContextAccessor;
        _timeZone = GetSafeTimeZone("Asia/Dubai");
    }

    private static TimeZoneInfo GetSafeTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            try
            {
                return TimeZoneInfo.CreateCustomTimeZone(
                    "UTC+4",
                    TimeSpan.FromHours(4),
                    "Gulf Standard Time",
                    "Gulf Standard Time");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        
        try
        {
            await using var dbContext = await _dbFactory.CreateDbContextAsync();
            var branch = await dbContext.Branches
                .Where(b => !b.IsDeleted && b.IsActive)
                .OrderByDescending(b => b.IsHeadOffice)
                .FirstOrDefaultAsync();
            
            if (branch != null && !string.IsNullOrEmpty(branch.TimeZoneId))
            {
                _timeZone = GetSafeTimeZone(branch.TimeZoneId);
            }
            _initialized = true;
        }
        catch
        {
            _initialized = true;
        }
    }

    public DateTime ToLocalTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
            return utcDateTime;
            
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, _timeZone);
    }

    public DateTime ToLocalTime(DateTime? utcDateTime, DateTime defaultValue)
    {
        if (!utcDateTime.HasValue)
            return defaultValue;
        return ToLocalTime(utcDateTime.Value);
    }

    public string FormatDateTime(DateTime? utcDateTime, string format = "dd/MM/yyyy HH:mm")
    {
        if (!utcDateTime.HasValue)
            return "-";
        return ToLocalTime(utcDateTime.Value).ToString(format);
    }

    public string FormatDate(DateTime? utcDateTime, string format = "dd/MM/yyyy")
    {
        if (!utcDateTime.HasValue)
            return "-";
        return ToLocalTime(utcDateTime.Value).ToString(format);
    }

    public string FormatTime(DateTime? utcDateTime, string format = "HH:mm")
    {
        if (!utcDateTime.HasValue)
            return "-";
        return ToLocalTime(utcDateTime.Value).ToString(format);
    }

    public TimeZoneInfo GetTimeZone()
    {
        return _timeZone;
    }

    public void SetTimeZone(string timeZoneId)
    {
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            _initialized = true;
        }
        catch
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");
        }
    }
}
