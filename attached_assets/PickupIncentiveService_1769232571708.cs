using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using System.Text.Json;

namespace Server.Modules.Courier.Services;

public interface IPickupIncentiveService
{
    Task<List<PickupIncentiveSchedule>> GetAllSchedulesAsync(IncentiveType? incentiveType = null);
    Task<List<PickupIncentiveSchedule>> GetSchedulesByZoneAsync(Guid zoneId, IncentiveType? incentiveType = null);
    Task<PickupIncentiveSchedule?> GetScheduleByIdAsync(Guid id);
    Task<PickupIncentiveSchedule> CreateScheduleAsync(PickupIncentiveSchedule schedule);
    Task<PickupIncentiveSchedule> UpdateScheduleAsync(PickupIncentiveSchedule schedule);
    Task<bool> DeleteScheduleAsync(Guid id);
    Task<PickupIncentiveSchedule?> GetApplicableScheduleAsync(Guid? zoneId, Guid? customerId, IncentiveType incentiveType = IncentiveType.Pickup);
    Task<PickupIncentiveAward?> AwardIncentiveAsync(Guid commitmentId);
    Task<List<PickupIncentiveAward>> GetAwardsByCourierAsync(Guid courierAgentId);
    Task<decimal> GetTotalIncentivesForCourierAsync(Guid courierAgentId, DateTime? fromDate = null, DateTime? toDate = null);
}

public class PickupIncentiveService : IPickupIncentiveService
{
    private readonly AppDbContext _context;

    public PickupIncentiveService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PickupIncentiveSchedule>> GetAllSchedulesAsync(IncentiveType? incentiveType = null)
    {
        var query = _context.PickupIncentiveSchedules
            .Include(s => s.CourierZone)
            .Include(s => s.Customer)
            .Where(s => s.IsActive);

        if (incentiveType.HasValue)
        {
            query = query.Where(s => s.IncentiveType == incentiveType.Value);
        }

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<List<PickupIncentiveSchedule>> GetSchedulesByZoneAsync(Guid zoneId, IncentiveType? incentiveType = null)
    {
        var query = _context.PickupIncentiveSchedules
            .Include(s => s.CourierZone)
            .Where(s => s.CourierZoneId == zoneId && s.IsActive)
            .Where(s => s.ValidFrom <= DateTime.UtcNow)
            .Where(s => s.ValidTo == null || s.ValidTo >= DateTime.UtcNow);

        if (incentiveType.HasValue)
        {
            query = query.Where(s => s.IncentiveType == incentiveType.Value);
        }

        return await query.OrderByDescending(s => s.Priority).ToListAsync();
    }

    public async Task<PickupIncentiveSchedule?> GetScheduleByIdAsync(Guid id)
    {
        return await _context.PickupIncentiveSchedules
            .Include(s => s.CourierZone)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<PickupIncentiveSchedule> CreateScheduleAsync(PickupIncentiveSchedule schedule)
    {
        _context.PickupIncentiveSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<PickupIncentiveSchedule> UpdateScheduleAsync(PickupIncentiveSchedule schedule)
    {
        _context.PickupIncentiveSchedules.Update(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> DeleteScheduleAsync(Guid id)
    {
        var schedule = await _context.PickupIncentiveSchedules.FindAsync(id);
        if (schedule == null) return false;

        schedule.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PickupIncentiveSchedule?> GetApplicableScheduleAsync(Guid? zoneId, Guid? customerId, IncentiveType incentiveType = IncentiveType.Pickup)
    {
        var now = DateTime.UtcNow;
        
        var query = _context.PickupIncentiveSchedules
            .Where(s => s.IsActive)
            .Where(s => s.IncentiveType == incentiveType)
            .Where(s => s.ValidFrom <= now)
            .Where(s => s.ValidTo == null || s.ValidTo >= now);

        if (customerId.HasValue)
        {
            var customerSchedule = await query
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefaultAsync();
            
            if (customerSchedule != null) return customerSchedule;
        }

        if (zoneId.HasValue)
        {
            var zoneSchedule = await query
                .Where(s => s.CourierZoneId == zoneId && s.CustomerId == null)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefaultAsync();
            
            if (zoneSchedule != null) return zoneSchedule;
        }

        return await query
            .Where(s => s.CourierZoneId == null && s.CustomerId == null)
            .OrderByDescending(s => s.Priority)
            .FirstOrDefaultAsync();
    }

    public async Task<PickupIncentiveAward?> AwardIncentiveAsync(Guid commitmentId)
    {
        var commitment = await _context.PickupCommitments
            .Include(c => c.PickupRequest)
                .ThenInclude(p => p!.Customer)
            .FirstOrDefaultAsync(c => c.Id == commitmentId);

        if (commitment?.PickupRequest == null) return null;

        var schedule = await GetApplicableScheduleAsync(
            commitment.PickupRequest.CourierZoneId,
            commitment.PickupRequest.CustomerId);

        if (schedule == null) return null;

        var amount = CalculateIncentiveAmount(schedule, commitment.PickupRequest);

        var award = new PickupIncentiveAward
        {
            PickupCommitmentId = commitmentId,
            PickupIncentiveScheduleId = schedule.Id,
            Amount = amount,
            Currency = schedule.Currency,
            Status = IncentiveAwardStatus.Pending,
            AwardedAt = DateTime.UtcNow,
            CalculationSnapshot = JsonSerializer.Serialize(new
            {
                ScheduleCode = schedule.Code,
                CalculationMode = schedule.CalculationMode.ToString(),
                BaseAmount = schedule.Amount,
                Pieces = commitment.PickupRequest.ActualPieces,
                Weight = commitment.PickupRequest.ActualWeight,
                CalculatedAmount = amount
            })
        };

        _context.PickupIncentiveAwards.Add(award);
        await _context.SaveChangesAsync();

        return award;
    }

    private decimal CalculateIncentiveAmount(PickupIncentiveSchedule schedule, PickupRequest pickup)
    {
        decimal amount = schedule.CalculationMode switch
        {
            IncentiveCalculationMode.FlatAmount => schedule.Amount,
            IncentiveCalculationMode.PerPiece => schedule.Amount * pickup.ActualPieces,
            IncentiveCalculationMode.PerKg => schedule.Amount * (pickup.ActualWeight ?? 0),
            _ => schedule.Amount
        };

        if (schedule.MinimumAmount.HasValue && amount < schedule.MinimumAmount.Value)
        {
            amount = schedule.MinimumAmount.Value;
        }

        if (schedule.MaximumAmount.HasValue && amount > schedule.MaximumAmount.Value)
        {
            amount = schedule.MaximumAmount.Value;
        }

        return amount;
    }

    public async Task<List<PickupIncentiveAward>> GetAwardsByCourierAsync(Guid courierAgentId)
    {
        return await _context.PickupIncentiveAwards
            .Include(a => a.PickupCommitment)
                .ThenInclude(c => c!.PickupRequest)
            .Include(a => a.PickupIncentiveSchedule)
            .Where(a => a.PickupCommitment!.CourierAgentId == courierAgentId)
            .OrderByDescending(a => a.AwardedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalIncentivesForCourierAsync(
        Guid courierAgentId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.PickupIncentiveAwards
            .Where(a => a.PickupCommitment!.CourierAgentId == courierAgentId)
            .Where(a => a.Status != IncentiveAwardStatus.Cancelled);

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AwardedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AwardedAt <= toDate.Value);
        }

        return await query.SumAsync(a => a.Amount);
    }
}
