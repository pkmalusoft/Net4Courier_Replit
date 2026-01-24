using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using CourierTransferStatus = Server.Modules.Courier.Models.TransferStatus;

namespace Server.Modules.Courier.Services;

public interface ITransferOrderService
{
    Task<List<TransferOrder>> GetAllTransfersAsync(TransferType? transferType = null, CourierTransferStatus? status = null);
    Task<TransferOrder?> GetTransferByIdAsync(Guid id);
    Task<TransferOrder?> GetTransferByNumberAsync(string transferNumber);
    Task<TransferOrder> CreateTransferAsync(TransferOrder transfer);
    Task<TransferOrder> UpdateTransferAsync(TransferOrder transfer);
    Task<bool> DeleteTransferAsync(Guid id);
    Task<TransferOrderItem> AddItemToTransferAsync(Guid transferId, Guid shipmentId);
    Task<bool> RemoveItemFromTransferAsync(Guid transferId, Guid itemId);
    Task<TransferOrderItem?> ScanItemAsync(Guid transferId, string awbNumber, string scannedBy);
    Task<TransferOrder> StartTransferAsync(Guid transferId, string executedBy);
    Task<TransferOrder> CompleteTransferAsync(Guid transferId, string completedBy);
    Task<TransferOrder> RejectTransferAsync(Guid transferId, string rejectedBy, string reason);
    Task<TransferScanEvent> LogScanEventAsync(TransferScanEvent scanEvent);
    Task<List<TransferScanEvent>> GetScanEventsAsync(Guid transferId);
    Task<string> GenerateTransferNumberAsync(TransferType transferType);
}

public class TransferOrderService : ITransferOrderService
{
    private readonly AppDbContext _context;

    public TransferOrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferOrder>> GetAllTransfersAsync(TransferType? transferType = null, CourierTransferStatus? status = null)
    {
        var query = _context.TransferOrders
            .Include(t => t.Items)
            .AsQueryable();

        if (transferType.HasValue)
        {
            query = query.Where(t => t.TransferType == transferType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TransferOrder?> GetTransferByIdAsync(Guid id)
    {
        return await _context.TransferOrders
            .Include(t => t.Items)
                .ThenInclude(i => i.Shipment)
            .Include(t => t.ScanEvents)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TransferOrder?> GetTransferByNumberAsync(string transferNumber)
    {
        return await _context.TransferOrders
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.TransferNumber == transferNumber);
    }

    public async Task<TransferOrder> CreateTransferAsync(TransferOrder transfer)
    {
        if (string.IsNullOrEmpty(transfer.TransferNumber))
        {
            transfer.TransferNumber = await GenerateTransferNumberAsync(transfer.TransferType);
        }

        _context.TransferOrders.Add(transfer);
        await _context.SaveChangesAsync();

        await LogScanEventAsync(new TransferScanEvent
        {
            TransferOrderId = transfer.Id,
            TenantId = transfer.TenantId,
            ScanType = ScanEventType.OutscanStart,
            ScannedByName = transfer.CreatedBy,
            Notes = $"Transfer {transfer.TransferNumber} created"
        });

        return transfer;
    }

    public async Task<TransferOrder> UpdateTransferAsync(TransferOrder transfer)
    {
        transfer.UpdatedAt = DateTime.UtcNow;
        _context.TransferOrders.Update(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task<bool> DeleteTransferAsync(Guid id)
    {
        var transfer = await _context.TransferOrders.FindAsync(id);
        if (transfer == null) return false;

        if (transfer.Status != CourierTransferStatus.Draft)
        {
            return false;
        }

        transfer.Status = CourierTransferStatus.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TransferOrderItem> AddItemToTransferAsync(Guid transferId, Guid shipmentId)
    {
        var transfer = await _context.TransferOrders.FindAsync(transferId);
        if (transfer == null)
            throw new InvalidOperationException("Transfer not found");

        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null)
            throw new InvalidOperationException("Shipment not found");

        var existingItem = await _context.TransferOrderItems
            .FirstOrDefaultAsync(i => i.TransferOrderId == transferId && i.ShipmentId == shipmentId);

        if (existingItem != null)
            throw new InvalidOperationException("Shipment already in transfer");

        var item = new TransferOrderItem
        {
            TransferOrderId = transferId,
            TenantId = transfer.TenantId,
            ShipmentId = shipmentId,
            AWBNumber = shipment.AWBNumber,
            Status = TransferItemStatus.Pending
        };

        _context.TransferOrderItems.Add(item);
        transfer.TotalItems++;
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<bool> RemoveItemFromTransferAsync(Guid transferId, Guid itemId)
    {
        var item = await _context.TransferOrderItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TransferOrderId == transferId);

        if (item == null) return false;

        var transfer = await _context.TransferOrders.FindAsync(transferId);
        if (transfer == null) return false;

        if (transfer.Status != CourierTransferStatus.Draft)
            return false;

        _context.TransferOrderItems.Remove(item);
        transfer.TotalItems--;
        if (item.Status == TransferItemStatus.Scanned)
            transfer.ScannedItems--;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TransferOrderItem?> ScanItemAsync(Guid transferId, string awbNumber, string scannedBy)
    {
        var transfer = await _context.TransferOrders.FindAsync(transferId);
        if (transfer == null) return null;

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.AWBNumber == awbNumber && s.TenantId == transfer.TenantId);

        if (shipment == null) return null;

        var existingItem = await _context.TransferOrderItems
            .FirstOrDefaultAsync(i => i.TransferOrderId == transferId && i.ShipmentId == shipment.Id);

        TransferOrderItem item;
        if (existingItem != null)
        {
            existingItem.Status = TransferItemStatus.Scanned;
            existingItem.ScannedAt = DateTime.UtcNow;
            existingItem.ScannedBy = scannedBy;
            item = existingItem;
        }
        else
        {
            item = new TransferOrderItem
            {
                TransferOrderId = transferId,
                TenantId = transfer.TenantId,
                ShipmentId = shipment.Id,
                AWBNumber = awbNumber,
                Status = TransferItemStatus.Scanned,
                ScannedAt = DateTime.UtcNow,
                ScannedBy = scannedBy
            };
            _context.TransferOrderItems.Add(item);
            transfer.TotalItems++;
        }

        transfer.ScannedItems++;
        await _context.SaveChangesAsync();

        await LogScanEventAsync(new TransferScanEvent
        {
            TransferOrderId = transferId,
            TenantId = transfer.TenantId,
            TransferOrderItemId = item.Id,
            ShipmentId = shipment.Id,
            AWBNumber = awbNumber,
            ScanType = ScanEventType.ItemScanned,
            ScannedByName = scannedBy
        });

        return item;
    }

    public async Task<TransferOrder> StartTransferAsync(Guid transferId, string executedBy)
    {
        var transfer = await GetTransferByIdAsync(transferId);
        if (transfer == null)
            throw new InvalidOperationException("Transfer not found");

        if (transfer.Status != CourierTransferStatus.Draft)
            throw new InvalidOperationException("Transfer is not in draft status");

        transfer.Status = CourierTransferStatus.InProgress;
        transfer.ExecutedAt = DateTime.UtcNow;
        transfer.UpdatedBy = executedBy;
        transfer.UpdatedAt = DateTime.UtcNow;

        foreach (var item in transfer.Items.Where(i => i.Status == TransferItemStatus.Scanned))
        {
            item.Status = TransferItemStatus.Loaded;
            item.LoadedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await LogScanEventAsync(new TransferScanEvent
        {
            TransferOrderId = transferId,
            TenantId = transfer.TenantId,
            ScanType = ScanEventType.DepartWarehouse,
            ScannedByName = executedBy,
            Notes = $"Transfer started with {transfer.ScannedItems} items"
        });

        return transfer;
    }

    public async Task<TransferOrder> CompleteTransferAsync(Guid transferId, string completedBy)
    {
        var transfer = await GetTransferByIdAsync(transferId);
        if (transfer == null)
            throw new InvalidOperationException("Transfer not found");

        if (transfer.Status != CourierTransferStatus.InProgress)
            throw new InvalidOperationException("Transfer must be in progress to complete");

        if (transfer.ScannedItems == 0)
            throw new InvalidOperationException("Cannot complete transfer with no scanned items");

        transfer.Status = CourierTransferStatus.Completed;
        transfer.CompletedAt = DateTime.UtcNow;
        transfer.UpdatedBy = completedBy;
        transfer.UpdatedAt = DateTime.UtcNow;

        foreach (var item in transfer.Items.Where(i => i.Status == TransferItemStatus.Loaded || i.Status == TransferItemStatus.InTransit))
        {
            item.Status = TransferItemStatus.Received;
            item.ReceivedAt = DateTime.UtcNow;
            item.ReceivedBy = completedBy;
        }

        await _context.SaveChangesAsync();

        await LogScanEventAsync(new TransferScanEvent
        {
            TransferOrderId = transferId,
            TenantId = transfer.TenantId,
            ScanType = ScanEventType.TransferComplete,
            ScannedByName = completedBy,
            Notes = $"Transfer completed with {transfer.ScannedItems} items received"
        });

        return transfer;
    }

    public async Task<TransferOrder> RejectTransferAsync(Guid transferId, string rejectedBy, string reason)
    {
        var transfer = await GetTransferByIdAsync(transferId);
        if (transfer == null)
            throw new InvalidOperationException("Transfer not found");

        if (transfer.Status == CourierTransferStatus.Completed || transfer.Status == CourierTransferStatus.Cancelled)
            throw new InvalidOperationException("Cannot reject a completed or cancelled transfer");

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Rejection reason is required");

        foreach (var item in transfer.Items)
        {
            if (item.Status != TransferItemStatus.Received)
            {
                item.Status = TransferItemStatus.Exception;
                item.ExceptionReason = reason;
            }
        }
        transfer.ExceptionItems = transfer.Items.Count(i => i.Status == TransferItemStatus.Exception);

        transfer.Status = CourierTransferStatus.Rejected;
        transfer.Remarks = reason;
        transfer.UpdatedBy = rejectedBy;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await LogScanEventAsync(new TransferScanEvent
        {
            TransferOrderId = transferId,
            TenantId = transfer.TenantId,
            ScanType = ScanEventType.Exception,
            ScannedByName = rejectedBy,
            IsException = true,
            ExceptionDetails = reason,
            Notes = $"Transfer rejected: {reason}"
        });

        return transfer;
    }

    public async Task<TransferScanEvent> LogScanEventAsync(TransferScanEvent scanEvent)
    {
        scanEvent.ScanTimestamp = DateTime.UtcNow;
        _context.TransferScanEvents.Add(scanEvent);
        await _context.SaveChangesAsync();
        return scanEvent;
    }

    public async Task<List<TransferScanEvent>> GetScanEventsAsync(Guid transferId)
    {
        return await _context.TransferScanEvents
            .Where(e => e.TransferOrderId == transferId)
            .OrderByDescending(e => e.ScanTimestamp)
            .ToListAsync();
    }

    public async Task<string> GenerateTransferNumberAsync(TransferType transferType)
    {
        var prefix = transferType switch
        {
            TransferType.DeliveryOutscan => "DO",
            TransferType.WarehouseTransfer => "WT",
            TransferType.CourierReassignment => "CR",
            _ => "TR"
        };

        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.TransferOrders
            .Where(t => t.TransferNumber.StartsWith($"{prefix}{today}"))
            .CountAsync();

        return $"{prefix}{today}{(count + 1):D4}";
    }
}
