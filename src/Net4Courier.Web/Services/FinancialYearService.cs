using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class FinancialYearService : IFinancialYearService
{
    private readonly ApplicationDbContext _context;

    public FinancialYearService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FinancialYear>> GetAllAsync()
    {
        return await _context.FinancialYears
            .Include(fy => fy.Periods)
            .OrderByDescending(fy => fy.StartDate)
            .ToListAsync();
    }

    public async Task<FinancialYear?> GetByIdAsync(long id)
    {
        return await _context.FinancialYears
            .Include(fy => fy.Periods)
            .FirstOrDefaultAsync(fy => fy.Id == id);
    }

    public async Task<FinancialYear> CreateWithPeriodsAsync(string name, DateTime startDate, DateTime endDate)
    {
        var financialYear = new FinancialYear
        {
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            IsClosed = false,
            IsActive = true
        };

        _context.FinancialYears.Add(financialYear);
        await _context.SaveChangesAsync();

        var currentDate = startDate;
        int periodNumber = 1;
        while (currentDate <= endDate)
        {
            var periodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            if (periodEnd > endDate) periodEnd = endDate;

            var period = new FinancialPeriod
            {
                FinancialYearId = financialYear.Id,
                PeriodNumber = periodNumber,
                PeriodName = currentDate.ToString("MMM yyyy"),
                StartDate = currentDate,
                EndDate = periodEnd,
                Status = PeriodStatus.Open,
                IsActive = true
            };

            _context.FinancialPeriods.Add(period);
            currentDate = periodEnd.AddDays(1);
            periodNumber++;
        }

        await _context.SaveChangesAsync();
        return financialYear;
    }

    public async Task<bool> ClosePeriodAsync(long periodId)
    {
        var period = await _context.FinancialPeriods.FindAsync(periodId);
        if (period == null) return false;

        period.Status = PeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReopenPeriodAsync(long periodId)
    {
        var period = await _context.FinancialPeriods.FindAsync(periodId);
        if (period == null) return false;

        period.Status = PeriodStatus.Open;
        period.ReopenedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
