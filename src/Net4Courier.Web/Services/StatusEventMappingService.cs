using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class StatusEventMappingService
{
    private readonly ApplicationDbContext _context;
    private readonly ShipmentStatusService _statusService;

    public StatusEventMappingService(ApplicationDbContext context, ShipmentStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<List<StatusEventMapping>> GetAllMappings()
    {
        return await _context.StatusEventMappings
            .Include(m => m.Status)
            .ThenInclude(s => s.StatusGroup)
            .Where(m => m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.SequenceNo)
            .ToListAsync();
    }

    public async Task<StatusEventMapping?> GetMappingByEventCode(string eventCode)
    {
        return await _context.StatusEventMappings
            .Include(m => m.Status)
            .ThenInclude(s => s.StatusGroup)
            .FirstOrDefaultAsync(m => m.EventCode == eventCode && m.IsActive && !m.IsDeleted);
    }

    public async Task<string?> GetStatusCodeForEvent(string eventCode)
    {
        var mapping = await GetMappingByEventCode(eventCode);
        return mapping?.Status?.Code;
    }

    public async Task<ShipmentStatusHistory?> ApplyEventStatus(
        string eventCode,
        long inscanMasterId,
        long? eventRefId = null,
        string? eventRefType = null,
        long? branchId = null,
        string? locationName = null,
        long? userId = null,
        string? userName = null,
        string? remarks = null,
        bool isAutomatic = true,
        decimal? latitude = null,
        decimal? longitude = null,
        string? deviceInfo = null)
    {
        var mapping = await GetMappingByEventCode(eventCode);
        if (mapping == null || !mapping.IsAutoApply)
            return null;

        return await _statusService.SetStatus(
            inscanMasterId,
            mapping.Status.Code,
            mapping.EventName,
            eventRefId,
            eventRefType,
            branchId,
            locationName,
            userId,
            userName,
            remarks,
            isAutomatic,
            latitude,
            longitude,
            deviceInfo);
    }

    public async Task<StatusEventMapping> CreateMapping(StatusEventMapping mapping)
    {
        mapping.CreatedAt = DateTime.UtcNow;
        _context.StatusEventMappings.Add(mapping);
        await _context.SaveChangesAsync();
        return mapping;
    }

    public async Task<StatusEventMapping?> UpdateMapping(StatusEventMapping mapping)
    {
        var existing = await _context.StatusEventMappings.FindAsync(mapping.Id);
        if (existing == null)
            return null;

        existing.EventCode = mapping.EventCode;
        existing.EventName = mapping.EventName;
        existing.Description = mapping.Description;
        existing.StatusId = mapping.StatusId;
        existing.SequenceNo = mapping.SequenceNo;
        existing.IsAutoApply = mapping.IsAutoApply;
        existing.IsActive = mapping.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMapping(long id)
    {
        var mapping = await _context.StatusEventMappings.FindAsync(id);
        if (mapping == null)
            return false;

        mapping.IsDeleted = true;
        mapping.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EventCodeExists(string eventCode, long? excludeId = null)
    {
        var query = _context.StatusEventMappings
            .Where(m => m.EventCode == eventCode && !m.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
