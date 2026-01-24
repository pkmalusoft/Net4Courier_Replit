using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class PickupIncentiveService
{
    private readonly ApplicationDbContext _context;

    public PickupIncentiveService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IncentiveSchedule>> GetSchedulesAsync(long? companyId = null, bool activeOnly = true)
    {
        var query = _context.IncentiveSchedules.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(s => s.CompanyId == companyId);
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IncentiveSchedule?> GetScheduleAsync(long id)
    {
        return await _context.IncentiveSchedules.FindAsync(id);
    }

    public async Task<IncentiveSchedule> CreateScheduleAsync(IncentiveSchedule schedule)
    {
        _context.IncentiveSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<IncentiveSchedule> UpdateScheduleAsync(IncentiveSchedule schedule)
    {
        _context.Entry(schedule).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> DeleteScheduleAsync(long id)
    {
        var schedule = await _context.IncentiveSchedules.FindAsync(id);
        if (schedule == null) return false;

        _context.IncentiveSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IncentiveSchedule?> FindApplicableScheduleAsync(
        long courierId,
        long? customerId,
        long? zoneId,
        DateTime transactionDate,
        decimal weight,
        int pieces)
    {
        var utcDate = DateTime.SpecifyKind(transactionDate.Date, DateTimeKind.Utc);
        
        var schedules = await _context.IncentiveSchedules
            .Where(s => s.IsActive)
            .Where(s => s.EffectiveFrom <= utcDate)
            .Where(s => s.EffectiveTo == null || s.EffectiveTo >= utcDate)
            .OrderByDescending(s => s.CustomerId.HasValue)
            .ThenByDescending(s => s.ZoneId.HasValue)
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            if (schedule.CustomerId.HasValue && schedule.CustomerId != customerId)
                continue;
            if (schedule.ZoneId.HasValue && schedule.ZoneId != zoneId)
                continue;
            if (schedule.MinWeight.HasValue && weight < schedule.MinWeight)
                continue;
            if (schedule.MaxWeight.HasValue && weight > schedule.MaxWeight)
                continue;
            if (schedule.MinPieces.HasValue && pieces < schedule.MinPieces)
                continue;
            if (schedule.MaxPieces.HasValue && pieces > schedule.MaxPieces)
                continue;

            return schedule;
        }

        return null;
    }

    public async Task<IncentiveAward?> CalculateAndAwardIncentiveAsync(
        long courierId,
        string courierName,
        long? pickupRequestId,
        string? pickupNo,
        long? customerId,
        string? customerName,
        int pieces,
        decimal weight,
        long? branchId = null)
    {
        var schedule = await FindApplicableScheduleAsync(courierId, customerId, null, DateTime.UtcNow, weight, pieces);
        if (schedule == null)
            return null;

        decimal incentiveAmount = 0;
        switch (schedule.CalculationType)
        {
            case IncentiveCalculationType.PerPiece:
                incentiveAmount = pieces * schedule.IncentiveRate;
                break;
            case IncentiveCalculationType.PerKg:
                incentiveAmount = weight * schedule.IncentiveRate;
                break;
            case IncentiveCalculationType.FlatRate:
                incentiveAmount = schedule.IncentiveRate;
                break;
            case IncentiveCalculationType.Percentage:
                break;
        }

        decimal bonusAmount = 0;
        if (schedule.BonusThreshold.HasValue && schedule.BonusAmount.HasValue)
        {
            var todayPickups = await _context.IncentiveAwards
                .Where(a => a.CourierId == courierId)
                .Where(a => a.AwardDate.Date == DateTime.UtcNow.Date)
                .CountAsync();

            if (todayPickups + 1 >= schedule.BonusThreshold.Value)
            {
                bonusAmount = schedule.BonusAmount.Value;
            }
        }

        var award = new IncentiveAward
        {
            IncentiveScheduleId = schedule.Id,
            CourierId = courierId,
            CourierName = courierName,
            PickupRequestId = pickupRequestId,
            PickupNo = pickupNo,
            CustomerId = customerId,
            CustomerName = customerName,
            AwardDate = DateTime.UtcNow,
            Pieces = pieces,
            Weight = weight,
            IncentiveAmount = Math.Round(incentiveAmount, 2),
            BonusAmount = Math.Round(bonusAmount, 2),
            TotalAmount = Math.Round(incentiveAmount + bonusAmount, 2),
            Status = IncentiveAwardStatus.Pending,
            BranchId = branchId
        };

        _context.IncentiveAwards.Add(award);
        await _context.SaveChangesAsync();

        return award;
    }

    public async Task<List<IncentiveAward>> GetAwardsAsync(
        long? courierId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        IncentiveAwardStatus? status = null,
        long? branchId = null)
    {
        var query = _context.IncentiveAwards
            .Include(a => a.IncentiveSchedule)
            .AsQueryable();

        if (courierId.HasValue)
            query = query.Where(a => a.CourierId == courierId);
        if (fromDate.HasValue)
            query = query.Where(a => a.AwardDate >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc));
        if (toDate.HasValue)
            query = query.Where(a => a.AwardDate <= DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc));
        if (status.HasValue)
            query = query.Where(a => a.Status == status);
        if (branchId.HasValue)
            query = query.Where(a => a.BranchId == branchId);

        return await query
            .OrderByDescending(a => a.AwardDate)
            .ToListAsync();
    }

    public async Task<bool> ApproveAwardsAsync(List<long> awardIds, long userId, string userName)
    {
        var awards = await _context.IncentiveAwards
            .Where(a => awardIds.Contains(a.Id) && a.Status == IncentiveAwardStatus.Pending)
            .ToListAsync();

        foreach (var award in awards)
        {
            award.Status = IncentiveAwardStatus.Approved;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAwardsPaidAsync(List<long> awardIds, string paymentReference)
    {
        var awards = await _context.IncentiveAwards
            .Where(a => awardIds.Contains(a.Id) && a.Status == IncentiveAwardStatus.Approved)
            .ToListAsync();

        foreach (var award in awards)
        {
            award.Status = IncentiveAwardStatus.Paid;
            award.PaidAt = DateTime.UtcNow;
            award.PaymentReference = paymentReference;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CourierIncentiveSummary> GetCourierSummaryAsync(long courierId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.IncentiveAwards.Where(a => a.CourierId == courierId);

        if (fromDate.HasValue)
            query = query.Where(a => a.AwardDate >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc));
        if (toDate.HasValue)
            query = query.Where(a => a.AwardDate <= DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc));

        var awards = await query.ToListAsync();

        return new CourierIncentiveSummary
        {
            CourierId = courierId,
            TotalPickups = awards.Count,
            TotalPieces = awards.Sum(a => a.Pieces),
            TotalWeight = awards.Sum(a => a.Weight),
            TotalIncentive = awards.Sum(a => a.IncentiveAmount),
            TotalBonus = awards.Sum(a => a.BonusAmount ?? 0),
            TotalEarnings = awards.Sum(a => a.TotalAmount),
            PendingAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Pending).Sum(a => a.TotalAmount),
            ApprovedAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Approved).Sum(a => a.TotalAmount),
            PaidAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Paid).Sum(a => a.TotalAmount)
        };
    }
}

public class CourierIncentiveSummary
{
    public long CourierId { get; set; }
    public int TotalPickups { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalIncentive { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
}
