using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class PickupCommitmentService
{
    private readonly ApplicationDbContext _context;
    private readonly int _defaultExpirationMinutes = 30;

    public PickupCommitmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PickupRequest>> GetAvailablePickupsAsync(long? branchId = null)
    {
        var query = _context.PickupRequests
            .Where(p => p.Status == PickupStatus.PickupRequest)
            .Where(p => !_context.PickupCommitments.Any(c => 
                c.PickupRequestId == p.Id && 
                c.Status == PickupCommitmentStatus.Active && 
                c.ExpiresAt > DateTime.UtcNow));

        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId);

        return await query
            .OrderBy(p => p.ScheduledDate)
            .ThenBy(p => p.ScheduledTimeFrom)
            .ToListAsync();
    }

    public async Task<List<PickupCommitment>> GetCourierCommitmentsAsync(long courierId, bool activeOnly = true)
    {
        var query = _context.PickupCommitments
            .Include(c => c.PickupRequest)
            .Where(c => c.CourierId == courierId);

        if (activeOnly)
            query = query.Where(c => c.Status == PickupCommitmentStatus.Active && c.ExpiresAt > DateTime.UtcNow);

        return await query
            .OrderByDescending(c => c.CommittedAt)
            .ToListAsync();
    }

    public async Task<PickupCommitment?> CommitToPickupAsync(
        long pickupRequestId,
        long courierId,
        string courierName,
        int? expirationMinutes = null,
        long? branchId = null)
    {
        var activeCommitment = await _context.PickupCommitments
            .Where(c => c.PickupRequestId == pickupRequestId)
            .Where(c => c.Status == PickupCommitmentStatus.Active && c.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (activeCommitment != null)
            return null;

        var pickup = await _context.PickupRequests.FindAsync(pickupRequestId);
        if (pickup == null || pickup.Status != PickupStatus.PickupRequest)
            return null;

        var expMins = expirationMinutes ?? _defaultExpirationMinutes;
        var commitment = new PickupCommitment
        {
            PickupRequestId = pickupRequestId,
            CourierId = courierId,
            CourierName = courierName,
            CommittedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expMins),
            Status = PickupCommitmentStatus.Active,
            BranchId = branchId
        };

        _context.PickupCommitments.Add(commitment);
        await _context.SaveChangesAsync();

        return commitment;
    }

    public async Task<bool> ConfirmCommitmentAsync(long commitmentId)
    {
        var commitment = await _context.PickupCommitments
            .Include(c => c.PickupRequest)
            .FirstOrDefaultAsync(c => c.Id == commitmentId);

        if (commitment == null || commitment.Status != PickupCommitmentStatus.Active)
            return false;

        if (commitment.ExpiresAt < DateTime.UtcNow)
        {
            commitment.Status = PickupCommitmentStatus.Expired;
            await _context.SaveChangesAsync();
            return false;
        }

        commitment.Status = PickupCommitmentStatus.Confirmed;
        commitment.ConfirmedAt = DateTime.UtcNow;

        if (commitment.PickupRequest != null)
        {
            commitment.PickupRequest.Status = PickupStatus.AssignedForCollection;
            commitment.PickupRequest.CourierId = commitment.CourierId;
            commitment.PickupRequest.CourierName = commitment.CourierName;
            commitment.PickupRequest.AssignedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseCommitmentAsync(
        long commitmentId,
        string reason,
        long? userId = null,
        string? userName = null)
    {
        var commitment = await _context.PickupCommitments.FindAsync(commitmentId);
        if (commitment == null || commitment.Status != PickupCommitmentStatus.Active)
            return false;

        commitment.Status = PickupCommitmentStatus.Released;
        commitment.ReleasedAt = DateTime.UtcNow;
        commitment.ReleaseReason = reason;
        commitment.ReleasedByUserId = userId;
        commitment.ReleasedByUserName = userName;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> ExpireOldCommitmentsAsync()
    {
        var expiredCommitments = await _context.PickupCommitments
            .Where(c => c.Status == PickupCommitmentStatus.Active && c.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var commitment in expiredCommitments)
        {
            commitment.Status = PickupCommitmentStatus.Expired;
        }

        await _context.SaveChangesAsync();
        return expiredCommitments.Count;
    }

    public async Task<bool> ExtendCommitmentAsync(long commitmentId, int additionalMinutes)
    {
        var commitment = await _context.PickupCommitments.FindAsync(commitmentId);
        if (commitment == null || commitment.Status != PickupCommitmentStatus.Active)
            return false;

        commitment.ExpiresAt = commitment.ExpiresAt.AddMinutes(additionalMinutes);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteCommitmentAsync(long commitmentId)
    {
        var commitment = await _context.PickupCommitments.FindAsync(commitmentId);
        if (commitment == null || commitment.Status != PickupCommitmentStatus.Confirmed)
            return false;

        commitment.Status = PickupCommitmentStatus.Completed;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CourierCommitmentStats> GetCourierStatsAsync(long courierId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.PickupCommitments.Where(c => c.CourierId == courierId);

        if (fromDate.HasValue)
            query = query.Where(c => c.CommittedAt >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc));
        if (toDate.HasValue)
            query = query.Where(c => c.CommittedAt <= DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc));

        var commitments = await query.ToListAsync();

        return new CourierCommitmentStats
        {
            CourierId = courierId,
            TotalCommitments = commitments.Count,
            Completed = commitments.Count(c => c.Status == PickupCommitmentStatus.Completed),
            Confirmed = commitments.Count(c => c.Status == PickupCommitmentStatus.Confirmed),
            Released = commitments.Count(c => c.Status == PickupCommitmentStatus.Released),
            Expired = commitments.Count(c => c.Status == PickupCommitmentStatus.Expired),
            Active = commitments.Count(c => c.Status == PickupCommitmentStatus.Active)
        };
    }
}

public class CourierCommitmentStats
{
    public long CourierId { get; set; }
    public int TotalCommitments { get; set; }
    public int Completed { get; set; }
    public int Confirmed { get; set; }
    public int Released { get; set; }
    public int Expired { get; set; }
    public int Active { get; set; }
    public decimal CompletionRate => TotalCommitments > 0 ? (decimal)Completed / TotalCommitments * 100 : 0;
}
