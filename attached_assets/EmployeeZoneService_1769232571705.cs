using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IEmployeeZoneService
{
    Task<List<EmployeeZoneAssignment>> GetAllAsync();
    Task<List<EmployeeZoneAssignment>> GetByZoneAsync(Guid zoneId);
    Task<List<EmployeeZoneAssignment>> GetByCourierAsync(Guid courierAgentId);
    Task<EmployeeZoneAssignment?> GetByIdAsync(Guid id);
    Task<EmployeeZoneAssignment> CreateAsync(EmployeeZoneAssignment assignment);
    Task<EmployeeZoneAssignment> UpdateAsync(EmployeeZoneAssignment assignment);
    Task<bool> DeleteAsync(Guid id);
    Task<List<CourierAgent>> GetCouriersForZoneAsync(Guid zoneId);
    Task<List<CourierZone>> GetZonesForCourierAsync(Guid courierAgentId);
}

public class EmployeeZoneService : IEmployeeZoneService
{
    private readonly AppDbContext _context;

    public EmployeeZoneService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeZoneAssignment>> GetAllAsync()
    {
        return await _context.EmployeeZoneAssignments
            .Include(e => e.CourierAgent)
            .Include(e => e.CourierZone)
            .Where(e => e.IsActive)
            .OrderBy(e => e.CourierZone!.ZoneName)
            .ThenBy(e => e.PriorityRank)
            .ToListAsync();
    }

    public async Task<List<EmployeeZoneAssignment>> GetByZoneAsync(Guid zoneId)
    {
        return await _context.EmployeeZoneAssignments
            .Include(e => e.CourierAgent)
            .Include(e => e.CourierZone)
            .Where(e => e.CourierZoneId == zoneId && e.IsActive)
            .Where(e => e.EffectiveFrom <= DateTime.UtcNow)
            .Where(e => e.EffectiveTo == null || e.EffectiveTo >= DateTime.UtcNow)
            .OrderBy(e => e.PriorityRank)
            .ToListAsync();
    }

    public async Task<List<EmployeeZoneAssignment>> GetByCourierAsync(Guid courierAgentId)
    {
        return await _context.EmployeeZoneAssignments
            .Include(e => e.CourierAgent)
            .Include(e => e.CourierZone)
            .Where(e => e.CourierAgentId == courierAgentId && e.IsActive)
            .Where(e => e.EffectiveFrom <= DateTime.UtcNow)
            .Where(e => e.EffectiveTo == null || e.EffectiveTo >= DateTime.UtcNow)
            .OrderBy(e => e.CourierZone!.ZoneName)
            .ToListAsync();
    }

    public async Task<EmployeeZoneAssignment?> GetByIdAsync(Guid id)
    {
        return await _context.EmployeeZoneAssignments
            .Include(e => e.CourierAgent)
            .Include(e => e.CourierZone)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EmployeeZoneAssignment> CreateAsync(EmployeeZoneAssignment assignment)
    {
        if (assignment.IsPrimary)
        {
            var existingPrimary = await _context.EmployeeZoneAssignments
                .Where(e => e.CourierAgentId == assignment.CourierAgentId && e.IsPrimary && e.IsActive)
                .ToListAsync();
            
            foreach (var ep in existingPrimary)
            {
                ep.IsPrimary = false;
            }
        }

        _context.EmployeeZoneAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<EmployeeZoneAssignment> UpdateAsync(EmployeeZoneAssignment assignment)
    {
        if (assignment.IsPrimary)
        {
            var existingPrimary = await _context.EmployeeZoneAssignments
                .Where(e => e.CourierAgentId == assignment.CourierAgentId && e.IsPrimary && e.IsActive && e.Id != assignment.Id)
                .ToListAsync();
            
            foreach (var ep in existingPrimary)
            {
                ep.IsPrimary = false;
            }
        }

        _context.EmployeeZoneAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await _context.EmployeeZoneAssignments.FindAsync(id);
        if (assignment == null) return false;

        assignment.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CourierAgent>> GetCouriersForZoneAsync(Guid zoneId)
    {
        var now = DateTime.UtcNow;
        return await _context.EmployeeZoneAssignments
            .Where(e => e.CourierZoneId == zoneId && e.IsActive)
            .Where(e => e.EffectiveFrom <= now)
            .Where(e => e.EffectiveTo == null || e.EffectiveTo >= now)
            .Select(e => e.CourierAgent!)
            .Where(c => c.IsActive)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<CourierZone>> GetZonesForCourierAsync(Guid courierAgentId)
    {
        var now = DateTime.UtcNow;
        return await _context.EmployeeZoneAssignments
            .Where(e => e.CourierAgentId == courierAgentId && e.IsActive)
            .Where(e => e.EffectiveFrom <= now)
            .Where(e => e.EffectiveTo == null || e.EffectiveTo >= now)
            .Select(e => e.CourierZone!)
            .Where(z => z.IsActive)
            .Distinct()
            .ToListAsync();
    }
}
