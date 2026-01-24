using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICourierAgentService
{
    Task<List<CourierAgent>> GetAllAsync();
    Task<List<CourierAgent>> GetActiveAsync();
    Task<List<CourierAgent>> GetByTypeAsync(AgentType agentType);
    Task<CourierAgent?> GetByIdAsync(Guid id);
    Task<CourierAgent?> GetByCodeAsync(string agentCode);
    Task<CourierAgent> CreateAsync(CourierAgent agent);
    Task<CourierAgent> UpdateAsync(CourierAgent agent);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string agentCode, Guid? excludeId = null);
    Task<List<CourierAgent>> GetDeliveryAgentsAsync();
    Task<List<CourierAgent>> GetPickupAgentsAsync();
}

public class CourierAgentService : ICourierAgentService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CourierAgentService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<CourierAgent>> GetAllAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierAgent>();

        return await _context.CourierAgents
            .Include(a => a.Vendor)
            .Where(a => a.TenantId == tenantId.Value)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<List<CourierAgent>> GetActiveAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierAgent>();

        return await _context.CourierAgents
            .Include(a => a.Vendor)
            .Where(a => a.TenantId == tenantId.Value && a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<List<CourierAgent>> GetByTypeAsync(AgentType agentType)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierAgent>();

        return await _context.CourierAgents
            .Include(a => a.Vendor)
            .Where(a => a.TenantId == tenantId.Value && a.AgentType == agentType && a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<CourierAgent?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierAgents
            .Include(a => a.Vendor)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId.Value);
    }

    public async Task<CourierAgent?> GetByCodeAsync(string agentCode)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierAgents
            .Include(a => a.Vendor)
            .FirstOrDefaultAsync(a => a.AgentCode == agentCode && a.TenantId == tenantId.Value);
    }

    public async Task<CourierAgent> CreateAsync(CourierAgent agent)
    {
        _context.CourierAgents.Add(agent);
        await _context.SaveChangesAsync();
        return agent;
    }

    public async Task<CourierAgent> UpdateAsync(CourierAgent agent)
    {
        agent.UpdatedAt = DateTime.UtcNow;
        _context.CourierAgents.Update(agent);
        await _context.SaveChangesAsync();
        return agent;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var agent = await GetByIdAsync(id);
        if (agent == null)
            return false;

        _context.CourierAgents.Remove(agent);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string agentCode, Guid? excludeId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var query = _context.CourierAgents
            .Where(a => a.AgentCode == agentCode && a.TenantId == tenantId.Value);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<List<CourierAgent>> GetDeliveryAgentsAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierAgent>();

        return await _context.CourierAgents
            .Where(a => a.TenantId == tenantId.Value && 
                       a.IsActive && 
                       (a.AgentType == AgentType.DeliveryAgent || a.AgentType == AgentType.FranchisePartner))
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<List<CourierAgent>> GetPickupAgentsAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierAgent>();

        return await _context.CourierAgents
            .Where(a => a.TenantId == tenantId.Value && 
                       a.IsActive && 
                       (a.AgentType == AgentType.PickupAgent || a.AgentType == AgentType.FranchisePartner))
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
}
