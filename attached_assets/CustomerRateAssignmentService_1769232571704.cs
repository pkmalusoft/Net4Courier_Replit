using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICustomerRateAssignmentService
{
    Task<List<CustomerRateAssignment>> GetAllAsync();
    Task<CustomerRateAssignment?> GetByIdAsync(Guid id);
    Task<List<CustomerRateAssignment>> GetByCustomerIdAsync(Guid customerId);
    Task<List<CustomerRateAssignment>> GetByRateNameAsync(string rateName);
    Task<string?> GetActiveRateNameForCustomerAsync(Guid customerId, DateTime? asOfDate = null);
    Task<List<Customer>> GetCustomersByRateNameAsync(string rateName, DateTime? asOfDate = null);
    Task<CustomerRateAssignment> CreateAsync(CustomerRateAssignment assignment);
    Task<List<CustomerRateAssignment>> BulkAssignAsync(List<Guid> customerIds, string rateName, DateTime effectiveFrom, DateTime? effectiveTo = null, string? notes = null);
    Task<CustomerRateAssignment> UpdateAsync(CustomerRateAssignment assignment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<List<string>> GetDistinctRateNamesAsync();
    Task<bool> HasActiveAssignmentAsync(Guid customerId, Guid? excludeId = null);
}

public class CustomerRateAssignmentService : ICustomerRateAssignmentService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CustomerRateAssignmentService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<CustomerRateAssignment>> GetAllAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CustomerRateAssignment>();

        return await _context.CustomerRateAssignments
            .Include(a => a.Customer)
            .Where(a => a.TenantId == tenantId.Value)
            .OrderBy(a => a.Customer.Name)
            .ThenByDescending(a => a.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<CustomerRateAssignment?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CustomerRateAssignments
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId.Value);
    }

    public async Task<List<CustomerRateAssignment>> GetByCustomerIdAsync(Guid customerId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CustomerRateAssignment>();

        return await _context.CustomerRateAssignments
            .Where(a => a.CustomerId == customerId && a.TenantId == tenantId.Value)
            .OrderByDescending(a => a.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<List<CustomerRateAssignment>> GetByRateNameAsync(string rateName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CustomerRateAssignment>();

        return await _context.CustomerRateAssignments
            .Include(a => a.Customer)
            .Where(a => a.RateName == rateName && a.TenantId == tenantId.Value)
            .OrderBy(a => a.Customer.Name)
            .ToListAsync();
    }

    public async Task<string?> GetActiveRateNameForCustomerAsync(Guid customerId, DateTime? asOfDate = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var checkDate = asOfDate ?? DateTime.UtcNow;

        var assignment = await _context.CustomerRateAssignments
            .Where(a => a.CustomerId == customerId 
                && a.TenantId == tenantId.Value
                && a.IsActive
                && a.EffectiveFrom <= checkDate
                && (a.EffectiveTo == null || a.EffectiveTo >= checkDate))
            .OrderByDescending(a => a.EffectiveFrom)
            .FirstOrDefaultAsync();

        return assignment?.RateName;
    }

    public async Task<List<Customer>> GetCustomersByRateNameAsync(string rateName, DateTime? asOfDate = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Customer>();

        var checkDate = asOfDate ?? DateTime.UtcNow;

        var customerIds = await _context.CustomerRateAssignments
            .Where(a => a.RateName == rateName 
                && a.TenantId == tenantId.Value
                && a.IsActive
                && a.EffectiveFrom <= checkDate
                && (a.EffectiveTo == null || a.EffectiveTo >= checkDate))
            .Select(a => a.CustomerId)
            .Distinct()
            .ToListAsync();

        return await _context.Customers
            .Where(c => customerIds.Contains(c.Id) && c.TenantId == tenantId.Value)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CustomerRateAssignment> CreateAsync(CustomerRateAssignment assignment)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required");

        assignment.TenantId = tenantId.Value;
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.EffectiveFrom = DateTime.SpecifyKind(assignment.EffectiveFrom, DateTimeKind.Utc);
        if (assignment.EffectiveTo.HasValue)
            assignment.EffectiveTo = DateTime.SpecifyKind(assignment.EffectiveTo.Value, DateTimeKind.Utc);

        _context.CustomerRateAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<List<CustomerRateAssignment>> BulkAssignAsync(
        List<Guid> customerIds, 
        string rateName, 
        DateTime effectiveFrom, 
        DateTime? effectiveTo = null, 
        string? notes = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required");

        var assignments = new List<CustomerRateAssignment>();
        var utcEffectiveFrom = DateTime.SpecifyKind(effectiveFrom, DateTimeKind.Utc);
        var utcEffectiveTo = effectiveTo.HasValue 
            ? DateTime.SpecifyKind(effectiveTo.Value, DateTimeKind.Utc) 
            : (DateTime?)null;

        foreach (var customerId in customerIds)
        {
            var existing = await _context.CustomerRateAssignments
                .FirstOrDefaultAsync(a => a.CustomerId == customerId 
                    && a.RateName == rateName 
                    && a.TenantId == tenantId.Value
                    && a.IsActive);

            if (existing != null)
            {
                existing.EffectiveFrom = utcEffectiveFrom;
                existing.EffectiveTo = utcEffectiveTo;
                existing.Notes = notes;
                existing.UpdatedAt = DateTime.UtcNow;
                assignments.Add(existing);
            }
            else
            {
                var assignment = new CustomerRateAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.Value,
                    CustomerId = customerId,
                    RateName = rateName,
                    EffectiveFrom = utcEffectiveFrom,
                    EffectiveTo = utcEffectiveTo,
                    IsActive = true,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CustomerRateAssignments.Add(assignment);
                assignments.Add(assignment);
            }
        }

        await _context.SaveChangesAsync();
        return assignments;
    }

    public async Task<CustomerRateAssignment> UpdateAsync(CustomerRateAssignment assignment)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context is required");

        if (assignment.TenantId != tenantId.Value)
            throw new InvalidOperationException("Cannot update assignment from another tenant");

        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.EffectiveFrom = DateTime.SpecifyKind(assignment.EffectiveFrom, DateTimeKind.Utc);
        if (assignment.EffectiveTo.HasValue)
            assignment.EffectiveTo = DateTime.SpecifyKind(assignment.EffectiveTo.Value, DateTimeKind.Utc);

        _context.CustomerRateAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await GetByIdAsync(id);
        if (assignment == null)
            return false;

        _context.CustomerRateAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var assignment = await GetByIdAsync(id);
        if (assignment == null)
            return false;

        assignment.IsActive = false;
        assignment.EffectiveTo = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetDistinctRateNamesAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<string>();

        var assignmentNames = await _context.CustomerRateAssignments
            .Where(a => a.TenantId == tenantId.Value && !string.IsNullOrEmpty(a.RateName))
            .Select(a => a.RateName)
            .Distinct()
            .ToListAsync();

        var zoneRateNames = await _context.ZoneRates
            .Where(r => r.TenantId == tenantId.Value && !string.IsNullOrEmpty(r.RateName))
            .Select(r => r.RateName!)
            .Distinct()
            .ToListAsync();

        return assignmentNames
            .Union(zoneRateNames)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    public async Task<bool> HasActiveAssignmentAsync(Guid customerId, Guid? excludeId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var now = DateTime.UtcNow;
        var query = _context.CustomerRateAssignments
            .Where(a => a.CustomerId == customerId 
                && a.TenantId == tenantId.Value
                && a.IsActive
                && a.EffectiveFrom <= now
                && (a.EffectiveTo == null || a.EffectiveTo >= now));

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
