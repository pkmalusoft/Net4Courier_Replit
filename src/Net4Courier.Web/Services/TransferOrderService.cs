using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class TransferOrderService
{
    private readonly ApplicationDbContext _context;

    public TransferOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferOrder>> GetTransferOrdersAsync(
        long? sourceBranchId = null,
        long? destinationBranchId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        TransferOrderStatus? status = null)
    {
        var query = _context.TransferOrders.AsQueryable();

        if (sourceBranchId.HasValue)
            query = query.Where(t => t.SourceBranchId == sourceBranchId);
        if (destinationBranchId.HasValue)
            query = query.Where(t => t.DestinationBranchId == destinationBranchId);
        if (fromDate.HasValue)
            query = query.Where(t => t.TransferDate >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc));
        if (toDate.HasValue)
            query = query.Where(t => t.TransferDate <= DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc));
        if (status.HasValue)
            query = query.Where(t => t.Status == status);

        return await query
            .OrderByDescending(t => t.TransferDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync();
    }

    public async Task<TransferOrder?> GetTransferOrderWithDetailsAsync(long id)
    {
        return await _context.TransferOrders
            .Include(t => t.Items)
            .Include(t => t.Events)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TransferOrder> CreateTransferOrderAsync(
        long sourceBranchId,
        string sourceBranchName,
        long destinationBranchId,
        string destinationBranchName,
        TransferOrderType transferType,
        List<long> shipmentIds,
        long? companyId,
        long? financialYearId,
        long userId,
        string userName,
        string? remarks = null)
    {
        var transferNo = await GenerateTransferNoAsync(sourceBranchId);
        var shipments = await _context.InscanMasters
            .Where(i => shipmentIds.Contains(i.Id))
            .ToListAsync();

        var transfer = new TransferOrder
        {
            TransferNo = transferNo,
            TransferDate = DateTime.UtcNow,
            CompanyId = companyId,
            FinancialYearId = financialYearId,
            SourceBranchId = sourceBranchId,
            SourceBranchName = sourceBranchName,
            DestinationBranchId = destinationBranchId,
            DestinationBranchName = destinationBranchName,
            TransferType = transferType,
            Status = TransferOrderStatus.Draft,
            TotalItems = shipments.Count,
            TotalPieces = shipments.Sum(s => s.Pieces ?? 0),
            TotalWeight = shipments.Sum(s => s.Weight ?? 0),
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = (int)userId,
            CreatedByName = userName
        };

        foreach (var shipment in shipments)
        {
            transfer.Items.Add(new TransferOrderItem
            {
                InscanMasterId = shipment.Id,
                AWBNo = shipment.AWBNo,
                Description = shipment.CargoDescription,
                Pieces = shipment.Pieces ?? 1,
                Weight = shipment.Weight ?? 0,
                Status = TransferItemStatus.Pending
            });
        }

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Created",
            Description = $"Transfer order created",
            Location = sourceBranchName,
            BranchId = sourceBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        _context.TransferOrders.Add(transfer);
        await _context.SaveChangesAsync();

        return transfer;
    }

    public async Task<bool> ConfirmTransferOrderAsync(long id, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Draft)
            return false;

        transfer.Status = TransferOrderStatus.Confirmed;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Confirmed",
            Description = "Transfer order confirmed and ready for loading",
            BranchId = transfer.SourceBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow,
            OldValue = TransferOrderStatus.Draft.ToString(),
            NewValue = TransferOrderStatus.Confirmed.ToString()
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ScanItemAsync(long transferOrderId, long itemId, long userId, string userName)
    {
        var item = await _context.TransferOrderItems
            .Include(i => i.TransferOrder)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TransferOrderId == transferOrderId);

        if (item == null || item.Status != TransferItemStatus.Pending)
            return false;

        item.Status = TransferItemStatus.Scanned;
        item.ScannedAt = DateTime.UtcNow;
        item.ScannedByUserId = userId;
        item.ScannedByUserName = userName;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartLoadingAsync(long id, string? vehicleNo, string? driverName, string? driverPhone, string? sealNo, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Confirmed)
            return false;

        transfer.Status = TransferOrderStatus.Loading;
        transfer.VehicleNo = vehicleNo;
        transfer.DriverName = driverName;
        transfer.DriverPhone = driverPhone;
        transfer.SealNo = sealNo;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Loading",
            Description = $"Loading started. Vehicle: {vehicleNo}",
            BranchId = transfer.SourceBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LoadItemAsync(long transferOrderId, long itemId)
    {
        var item = await _context.TransferOrderItems.FindAsync(itemId);
        if (item == null || item.TransferOrderId != transferOrderId || item.Status != TransferItemStatus.Scanned)
            return false;

        item.Status = TransferItemStatus.Loaded;
        item.LoadedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DispatchTransferAsync(long id, DateTime? expectedArrival, string? remarks, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Loading)
            return false;

        var loadedCount = transfer.Items.Count(i => i.Status == TransferItemStatus.Loaded);
        if (loadedCount == 0)
            return false;

        transfer.Status = TransferOrderStatus.InTransit;
        transfer.DispatchedAt = DateTime.UtcNow;
        transfer.DispatchedByUserId = userId;
        transfer.DispatchedByUserName = userName;
        transfer.ExpectedArrival = expectedArrival;
        transfer.DispatchRemarks = remarks;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        foreach (var item in transfer.Items.Where(i => i.Status == TransferItemStatus.Loaded))
        {
            item.Status = TransferItemStatus.InTransit;
        }

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Dispatched",
            Description = $"Transfer dispatched to {transfer.DestinationBranchName}",
            BranchId = transfer.SourceBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkArrivedAsync(long id, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.InTransit)
            return false;

        transfer.Status = TransferOrderStatus.Arrived;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Arrived",
            Description = "Transfer arrived at destination",
            Location = transfer.DestinationBranchName,
            BranchId = transfer.DestinationBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartUnloadingAsync(long id, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Arrived)
            return false;

        transfer.Status = TransferOrderStatus.Unloading;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Unloading",
            Description = "Unloading started",
            BranchId = transfer.DestinationBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReceiveItemAsync(
        long transferOrderId,
        long itemId,
        bool isShort,
        bool isDamaged,
        bool isExcess,
        string? damageDescription,
        string? remarks,
        long userId,
        string userName)
    {
        var item = await _context.TransferOrderItems.FindAsync(itemId);
        if (item == null || item.TransferOrderId != transferOrderId)
            return false;

        item.ReceivedAt = DateTime.UtcNow;
        item.IsShort = isShort;
        item.IsDamaged = isDamaged;
        item.IsExcess = isExcess;
        item.DamageDescription = damageDescription;
        item.ReceivedRemarks = remarks;

        if (isShort)
            item.Status = TransferItemStatus.Short;
        else if (isDamaged)
            item.Status = TransferItemStatus.Damaged;
        else if (isExcess)
            item.Status = TransferItemStatus.Excess;
        else
        {
            item.Status = TransferItemStatus.Received;
            item.UnloadedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteReceivingAsync(long id, string? remarks, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Unloading)
            return false;

        transfer.Status = TransferOrderStatus.Received;
        transfer.ReceivedAt = DateTime.UtcNow;
        transfer.ReceivedByUserId = userId;
        transfer.ReceivedByUserName = userName;
        transfer.ReceiptRemarks = remarks;
        transfer.ReceivedCount = transfer.Items.Count(i => i.Status == TransferItemStatus.Received);
        transfer.ShortCount = transfer.Items.Count(i => i.IsShort);
        transfer.DamagedCount = transfer.Items.Count(i => i.IsDamaged);
        transfer.ExcessCount = transfer.Items.Count(i => i.IsExcess);
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Received",
            Description = $"Receiving completed. Received: {transfer.ReceivedCount}, Short: {transfer.ShortCount}, Damaged: {transfer.DamagedCount}",
            BranchId = transfer.DestinationBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteTransferAsync(long id, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status != TransferOrderStatus.Received)
            return false;

        transfer.Status = TransferOrderStatus.Completed;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Completed",
            Description = "Transfer order completed",
            BranchId = transfer.DestinationBranchId,
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelTransferAsync(long id, string reason, long userId, string userName)
    {
        var transfer = await GetTransferOrderWithDetailsAsync(id);
        if (transfer == null || transfer.Status == TransferOrderStatus.Completed || transfer.Status == TransferOrderStatus.InTransit)
            return false;

        transfer.Status = TransferOrderStatus.Cancelled;
        transfer.Remarks = reason;
        transfer.ModifiedAt = DateTime.UtcNow;
        transfer.ModifiedBy = (int)userId;
        transfer.ModifiedByName = userName;

        transfer.Events.Add(new TransferOrderEvent
        {
            EventType = "Cancelled",
            Description = $"Transfer cancelled. Reason: {reason}",
            UserId = userId,
            UserName = userName,
            EventTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateTransferNoAsync(long sourceBranchId)
    {
        var prefix = "TRF";
        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        var lastTransfer = await _context.TransferOrders
            .Where(t => t.TransferNo.StartsWith($"{prefix}-{datePart}"))
            .OrderByDescending(t => t.TransferNo)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastTransfer != null)
        {
            var parts = lastTransfer.TransferNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}-{datePart}-{sequence:D5}";
    }
}
