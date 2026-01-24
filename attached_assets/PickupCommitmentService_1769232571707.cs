using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IPickupCommitmentService
{
    Task<PickupCommitment?> GetActiveCommitmentAsync(Guid pickupRequestId);
    Task<PickupCommitment?> GetByIdAsync(Guid id);
    Task<List<PickupCommitment>> GetByCourierAsync(Guid courierAgentId);
    Task<(bool Success, string Message, PickupCommitment? Commitment)> CommitAsync(Guid pickupRequestId, Guid courierAgentId, int? expiryMinutes = null);
    Task<(bool Success, string Message)> ReleaseAsync(Guid commitmentId, Guid courierAgentId, string? reason = null);
    Task<(bool Success, string Message)> CompleteAsync(Guid commitmentId, Guid courierAgentId);
    Task<bool> HasActiveCommitmentAsync(Guid pickupRequestId);
    Task<CourierAgent?> GetCommittedCourierAsync(Guid pickupRequestId);
}

public class PickupCommitmentService : IPickupCommitmentService
{
    private readonly AppDbContext _context;
    private readonly IPickupIncentiveService _incentiveService;

    public PickupCommitmentService(AppDbContext context, IPickupIncentiveService incentiveService)
    {
        _context = context;
        _incentiveService = incentiveService;
    }

    public async Task<PickupCommitment?> GetActiveCommitmentAsync(Guid pickupRequestId)
    {
        return await _context.PickupCommitments
            .Include(c => c.CourierAgent)
            .Include(c => c.PickupRequest)
            .Where(c => c.PickupRequestId == pickupRequestId)
            .Where(c => c.Status == CommitmentStatus.Committed)
            .FirstOrDefaultAsync();
    }

    public async Task<PickupCommitment?> GetByIdAsync(Guid id)
    {
        return await _context.PickupCommitments
            .Include(c => c.CourierAgent)
            .Include(c => c.PickupRequest)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<PickupCommitment>> GetByCourierAsync(Guid courierAgentId)
    {
        return await _context.PickupCommitments
            .Include(c => c.PickupRequest)
            .Where(c => c.CourierAgentId == courierAgentId)
            .OrderByDescending(c => c.CommittedAt)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, PickupCommitment? Commitment)> CommitAsync(
        Guid pickupRequestId, Guid courierAgentId, int? expiryMinutes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var existingCommitment = await _context.PickupCommitments
                .Where(c => c.PickupRequestId == pickupRequestId)
                .Where(c => c.Status == CommitmentStatus.Committed)
                .FirstOrDefaultAsync();

            if (existingCommitment != null)
            {
                return (false, "This pickup is already committed by another courier", null);
            }

            var pickupRequest = await _context.PickupRequests.FindAsync(pickupRequestId);
            if (pickupRequest == null)
            {
                return (false, "Pickup request not found", null);
            }

            if (pickupRequest.Status == PickupStatus.Completed || 
                pickupRequest.Status == PickupStatus.Cancelled)
            {
                return (false, "This pickup request is no longer available", null);
            }

            var courierAgent = await _context.CourierAgents.FindAsync(courierAgentId);
            if (courierAgent == null || !courierAgent.IsActive)
            {
                return (false, "Courier agent not found or inactive", null);
            }

            var commitment = new PickupCommitment
            {
                PickupRequestId = pickupRequestId,
                CourierAgentId = courierAgentId,
                Status = CommitmentStatus.Committed,
                CommittedAt = DateTime.UtcNow,
                ExpiresAt = expiryMinutes.HasValue ? DateTime.UtcNow.AddMinutes(expiryMinutes.Value) : null
            };

            _context.PickupCommitments.Add(commitment);

            pickupRequest.AssignedAgentId = courierAgentId;
            pickupRequest.Status = PickupStatus.Assigned;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Successfully committed to pickup", commitment);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error committing to pickup: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> ReleaseAsync(
        Guid commitmentId, Guid courierAgentId, string? reason = null)
    {
        var commitment = await _context.PickupCommitments
            .Include(c => c.PickupRequest)
            .FirstOrDefaultAsync(c => c.Id == commitmentId);

        if (commitment == null)
        {
            return (false, "Commitment not found");
        }

        if (commitment.CourierAgentId != courierAgentId)
        {
            return (false, "You can only release your own commitments");
        }

        if (commitment.Status != CommitmentStatus.Committed)
        {
            return (false, "This commitment is not active");
        }

        commitment.Status = CommitmentStatus.Released;
        commitment.ReleasedAt = DateTime.UtcNow;
        commitment.ReleaseReason = reason;

        if (commitment.PickupRequest != null)
        {
            commitment.PickupRequest.AssignedAgentId = null;
            commitment.PickupRequest.Status = PickupStatus.Requested;
        }

        await _context.SaveChangesAsync();
        return (true, "Commitment released successfully");
    }

    public async Task<(bool Success, string Message)> CompleteAsync(Guid commitmentId, Guid courierAgentId)
    {
        var commitment = await _context.PickupCommitments
            .Include(c => c.PickupRequest)
            .FirstOrDefaultAsync(c => c.Id == commitmentId);

        if (commitment == null)
        {
            return (false, "Commitment not found");
        }

        if (commitment.CourierAgentId != courierAgentId)
        {
            return (false, "You can only complete your own commitments");
        }

        if (commitment.Status != CommitmentStatus.Committed)
        {
            return (false, "This commitment is not active");
        }

        commitment.Status = CommitmentStatus.Completed;
        commitment.CompletedAt = DateTime.UtcNow;

        await _incentiveService.AwardIncentiveAsync(commitment.Id);

        await _context.SaveChangesAsync();
        return (true, "Commitment completed successfully");
    }

    public async Task<bool> HasActiveCommitmentAsync(Guid pickupRequestId)
    {
        return await _context.PickupCommitments
            .AnyAsync(c => c.PickupRequestId == pickupRequestId && c.Status == CommitmentStatus.Committed);
    }

    public async Task<CourierAgent?> GetCommittedCourierAsync(Guid pickupRequestId)
    {
        var commitment = await GetActiveCommitmentAsync(pickupRequestId);
        return commitment?.CourierAgent;
    }
}
