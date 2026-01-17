using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class ShipmentStatusService
{
    private readonly ApplicationDbContext _context;

    public ShipmentStatusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShipmentStatusHistory?> SetStatus(
        long inscanMasterId,
        string statusCode,
        string eventType,
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
        var status = await _context.ShipmentStatuses
            .Include(s => s.StatusGroup)
            .FirstOrDefaultAsync(s => s.Code == statusCode && s.IsActive);

        if (status == null)
            return null;

        var inscan = await _context.InscanMasters.FindAsync(inscanMasterId);
        if (inscan == null)
            return null;

        var history = new ShipmentStatusHistory
        {
            InscanMasterId = inscanMasterId,
            StatusId = status.Id,
            StatusGroupId = status.StatusGroupId,
            EventType = eventType,
            EventRefId = eventRefId,
            EventRefType = eventRefType,
            BranchId = branchId,
            LocationName = locationName,
            UserId = userId,
            UserName = userName,
            Remarks = remarks,
            ChangedAt = DateTime.UtcNow,
            IsAutomatic = isAutomatic,
            Latitude = latitude,
            Longitude = longitude,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        _context.ShipmentStatusHistories.Add(history);

        if (status.MapsToCourierStatus.HasValue)
        {
            inscan.CourierStatusId = status.MapsToCourierStatus.Value;
            inscan.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return history;
    }

    public async Task<List<ShipmentStatusHistory>> GetTimeline(long inscanMasterId)
    {
        return await _context.ShipmentStatusHistories
            .Include(h => h.Status)
            .Include(h => h.StatusGroup)
            .Where(h => h.InscanMasterId == inscanMasterId)
            .OrderBy(h => h.ChangedAt)
            .ThenBy(h => h.Id)
            .ToListAsync();
    }

    public async Task<List<ShipmentStatusHistory>> GetTimelineByAWB(string awbNo)
    {
        var inscan = await _context.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
        if (inscan == null)
            return new List<ShipmentStatusHistory>();

        return await GetTimeline(inscan.Id);
    }

    public async Task<ShipmentStatus?> GetStatus(string statusCode)
    {
        return await _context.ShipmentStatuses
            .Include(s => s.StatusGroup)
            .FirstOrDefaultAsync(s => s.Code == statusCode && s.IsActive);
    }

    public async Task<List<ShipmentStatusGroup>> GetAllGroups()
    {
        return await _context.ShipmentStatusGroups
            .Include(g => g.Statuses.Where(s => s.IsActive).OrderBy(s => s.SequenceNo))
            .Where(g => g.IsActive)
            .OrderBy(g => g.SequenceNo)
            .ToListAsync();
    }

    public async Task<List<ShipmentStatus>> GetStatusesByGroup(string groupCode)
    {
        return await _context.ShipmentStatuses
            .Include(s => s.StatusGroup)
            .Where(s => s.StatusGroup.Code == groupCode && s.IsActive)
            .OrderBy(s => s.SequenceNo)
            .ToListAsync();
    }

    public async Task SeedDefaultStatuses()
    {
        if (await _context.ShipmentStatusGroups.AnyAsync())
            return;

        var groups = new List<ShipmentStatusGroup>
        {
            new() { Code = "PRE_PICKUP", Name = "Pre-Pickup", Description = "Request and planning", SequenceNo = 1, IconName = "Schedule", ColorCode = "#9E9E9E", CreatedAt = DateTime.UtcNow },
            new() { Code = "COLLECTION", Name = "Collection", Description = "Physical pickup", SequenceNo = 2, IconName = "LocalShipping", ColorCode = "#2196F3", CreatedAt = DateTime.UtcNow },
            new() { Code = "ORIGIN_WH", Name = "Origin Warehouse", Description = "Origin processing", SequenceNo = 3, IconName = "Warehouse", ColorCode = "#FF9800", CreatedAt = DateTime.UtcNow },
            new() { Code = "TRANSIT", Name = "Transit", Description = "Inter-hub movement", SequenceNo = 4, IconName = "Flight", ColorCode = "#673AB7", CreatedAt = DateTime.UtcNow },
            new() { Code = "DEST_WH", Name = "Destination Warehouse", Description = "Destination processing", SequenceNo = 5, IconName = "Hub", ColorCode = "#00BCD4", CreatedAt = DateTime.UtcNow },
            new() { Code = "DELIVERY", Name = "Delivery", Description = "Last-mile delivery", SequenceNo = 6, IconName = "DeliveryDining", ColorCode = "#4CAF50", CreatedAt = DateTime.UtcNow },
            new() { Code = "EXCEPTION", Name = "Exception / Return", Description = "Failed or returned shipments", SequenceNo = 7, IconName = "Warning", ColorCode = "#F44336", CreatedAt = DateTime.UtcNow },
            new() { Code = "FINANCE", Name = "Billing & Payment", Description = "Invoicing and settlement", SequenceNo = 8, IconName = "AttachMoney", ColorCode = "#795548", CreatedAt = DateTime.UtcNow },
            new() { Code = "CLOSED", Name = "Closed", Description = "Finalized shipments", SequenceNo = 9, IconName = "CheckCircle", ColorCode = "#607D8B", CreatedAt = DateTime.UtcNow }
        };

        _context.ShipmentStatusGroups.AddRange(groups);
        await _context.SaveChangesAsync();

        var statuses = new List<ShipmentStatus>
        {
            new() { StatusGroupId = groups[0].Id, Code = "PICKUP_REQUESTED", Name = "Pickup Requested", TimelineDescription = "Pickup request created", SequenceNo = 1, MapsToCourierStatus = CourierStatus.Pending, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[0].Id, Code = "PICKUP_SCHEDULED", Name = "Pickup Scheduled", TimelineDescription = "Courier assigned", SequenceNo = 2, MapsToCourierStatus = CourierStatus.Pending, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[0].Id, Code = "PICKUP_ATTEMPTED", Name = "Pickup Attempted", TimelineDescription = "Pickup attempt made", SequenceNo = 3, IsException = true, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[1].Id, Code = "ASSIGNED_FOR_COLLECTION", Name = "Assigned for Collection", TimelineDescription = "Assigned to pickup courier", SequenceNo = 1, MapsToCourierStatus = CourierStatus.AssignedToPickup, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[1].Id, Code = "SHIPMENT_COLLECTED", Name = "Shipment Collected", TimelineDescription = "Shipment collected from customer", SequenceNo = 2, MapsToCourierStatus = CourierStatus.PickedUp, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[1].Id, Code = "PICKUP_MANIFESTED", Name = "Pickup Manifested", TimelineDescription = "Added to pickup manifest", SequenceNo = 3, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[2].Id, Code = "INSCAN_ORIGIN", Name = "Inscan – Origin Warehouse", TimelineDescription = "Received at origin warehouse", SequenceNo = 1, MapsToCourierStatus = CourierStatus.InscanAtOrigin, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[2].Id, Code = "QC_COMPLETED", Name = "QC Completed", TimelineDescription = "Weight / dimension verified", SequenceNo = 2, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[2].Id, Code = "BAGGED", Name = "Bagged for Manifest", TimelineDescription = "Added to MAWB bag", SequenceNo = 3, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[2].Id, Code = "UNBAGGED", Name = "Removed from Bag", TimelineDescription = "Removed from MAWB bag", SequenceNo = 4, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[2].Id, Code = "MANIFESTED", Name = "Manifested", TimelineDescription = "MAWB finalized", SequenceNo = 5, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[2].Id, Code = "ORIGIN_PROCESSED", Name = "Origin Processing Completed", TimelineDescription = "Sorted and routed", SequenceNo = 6, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[3].Id, Code = "DISPATCHED_TO_HUB", Name = "Dispatched to Hub", TimelineDescription = "Line-haul dispatched", SequenceNo = 1, MapsToCourierStatus = CourierStatus.InTransit, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[3].Id, Code = "IN_TRANSIT", Name = "In Transit", TimelineDescription = "Moving between hubs", SequenceNo = 2, MapsToCourierStatus = CourierStatus.InTransit, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[4].Id, Code = "INSCAN_DESTINATION", Name = "Inscan – Destination Hub", TimelineDescription = "Received at destination hub", SequenceNo = 1, MapsToCourierStatus = CourierStatus.InscanAtDestination, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[4].Id, Code = "DESTINATION_PROCESSED", Name = "Destination Processing Completed", TimelineDescription = "Sorted for delivery", SequenceNo = 2, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[5].Id, Code = "OUT_FOR_DELIVERY", Name = "Out for Delivery", TimelineDescription = "Assigned to delivery courier", SequenceNo = 1, MapsToCourierStatus = CourierStatus.OutForDelivery, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[5].Id, Code = "DELIVERY_ATTEMPTED", Name = "Delivery Attempted", TimelineDescription = "Attempt made", SequenceNo = 2, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[5].Id, Code = "DELIVERED", Name = "Delivered", TimelineDescription = "Successfully delivered", SequenceNo = 3, MapsToCourierStatus = CourierStatus.Delivered, RequiresPOD = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[5].Id, Code = "POD_CAPTURED", Name = "Proof of Delivery Captured", TimelineDescription = "POD uploaded", SequenceNo = 4, IsTerminal = true, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[6].Id, Code = "DELIVERY_FAILED", Name = "Delivery Failed", TimelineDescription = "Failed attempt", SequenceNo = 1, IsException = true, MapsToCourierStatus = CourierStatus.NotDelivered, RequiresRemarks = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[6].Id, Code = "RETURN_TO_ORIGIN", Name = "Return to Origin Initiated", TimelineDescription = "RTO started", SequenceNo = 2, IsException = true, MapsToCourierStatus = CourierStatus.ReturnToOrigin, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[6].Id, Code = "RETURN_INSCAN", Name = "Return Inscan", TimelineDescription = "Received back at origin", SequenceNo = 3, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[6].Id, Code = "RETURN_COMPLETED", Name = "Return Completed", TimelineDescription = "Return closed", SequenceNo = 4, IsException = true, IsTerminal = true, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[7].Id, Code = "READY_FOR_INVOICE", Name = "Ready for Invoice", TimelineDescription = "Eligible for billing", SequenceNo = 1, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[7].Id, Code = "INVOICED", Name = "Customer Invoiced", TimelineDescription = "Invoice generated", SequenceNo = 2, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[7].Id, Code = "COD_RECONCILED", Name = "COD Reconciled", TimelineDescription = "COD settled", SequenceNo = 3, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = groups[7].Id, Code = "PAYMENT_RECEIVED", Name = "Payment Received", TimelineDescription = "Payment completed", SequenceNo = 4, CreatedAt = DateTime.UtcNow },
            
            new() { StatusGroupId = groups[8].Id, Code = "SHIPMENT_CLOSED", Name = "Shipment Closed", TimelineDescription = "Lifecycle completed", SequenceNo = 1, IsTerminal = true, CreatedAt = DateTime.UtcNow }
        };

        _context.ShipmentStatuses.AddRange(statuses);
        await _context.SaveChangesAsync();
    }
    
    public async Task SeedRTSStatuses()
    {
        var exceptionGroup = await _context.ShipmentStatusGroups
            .FirstOrDefaultAsync(g => g.Code == "EXCEPTION");
        
        if (exceptionGroup == null)
            return;
            
        var existingRTS = await _context.ShipmentStatuses
            .AnyAsync(s => s.Code == "RTS_REQUESTED");
        
        if (existingRTS)
            return;
        
        var maxSequence = await _context.ShipmentStatuses
            .Where(s => s.StatusGroupId == exceptionGroup.Id)
            .MaxAsync(s => (int?)s.SequenceNo) ?? 0;
        
        var rtsStatuses = new List<ShipmentStatus>
        {
            new() { StatusGroupId = exceptionGroup.Id, Code = "RTS_REQUESTED", Name = "RTS Requested", TimelineDescription = "Return to shipper requested", SequenceNo = maxSequence + 1, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = exceptionGroup.Id, Code = "RTS_COLLECTED", Name = "RTS Collected", TimelineDescription = "Return shipment collected from receiver", SequenceNo = maxSequence + 2, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = exceptionGroup.Id, Code = "RTS_INSCANNED", Name = "RTS Inscanned", TimelineDescription = "Return shipment inscanned at store", SequenceNo = maxSequence + 3, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = exceptionGroup.Id, Code = "RTS_IN_TRANSIT", Name = "RTS In Transit", TimelineDescription = "Return shipment in transit to shipper", SequenceNo = maxSequence + 4, IsException = true, CreatedAt = DateTime.UtcNow },
            new() { StatusGroupId = exceptionGroup.Id, Code = "RTS_DELIVERED", Name = "RTS Delivered", TimelineDescription = "Return shipment delivered to original shipper", SequenceNo = maxSequence + 5, IsException = true, IsTerminal = true, CreatedAt = DateTime.UtcNow }
        };
        
        _context.ShipmentStatuses.AddRange(rtsStatuses);
        await _context.SaveChangesAsync();
    }
}
