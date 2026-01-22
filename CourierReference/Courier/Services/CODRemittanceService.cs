using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICODRemittanceService
{
    Task<List<CODRemittance>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, RemittanceStatus? status = null);
    Task<CODRemittance?> GetByIdAsync(Guid id);
    Task<CODRemittance?> GetByNumberAsync(string remittanceNumber);
    Task<List<CustomerCODSummary>> GetPendingRemittancesByCustomerAsync();
    Task<List<Shipment>> GetPendingCODShipmentsAsync(Guid customerId);
    Task<CODRemittance> CreateRemittanceAsync(Guid customerId, List<Guid> shipmentIds, string? remarks = null);
    Task<CODRemittance?> ProcessPaymentAsync(Guid remittanceId, string paymentReference, string paymentMethod, DateTime paymentDate, Guid? userId = null);
    Task<CODRemittance?> VoidRemittanceAsync(Guid remittanceId, string reason, Guid? userId = null);
    Task<decimal> CalculatePendingCODAsync(Guid customerId);
    Task<CODRemittanceSummary> GetRemittanceSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<string> GenerateRemittanceNumberAsync();
}

public class CODRemittanceService : ICODRemittanceService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CODRemittanceService> _logger;

    public CODRemittanceService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        ILogger<CODRemittanceService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<List<CODRemittance>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, RemittanceStatus? status = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CODRemittance>();

        var query = _context.CODRemittances
            .Include(r => r.Customer)
            .Include(r => r.Items)
            .Where(r => r.TenantId == tenantId.Value);

        if (fromDate.HasValue)
        {
            var utcFromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.RemittanceDate >= utcFromDate);
        }

        if (toDate.HasValue)
        {
            var utcToDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.RemittanceDate <= utcToDate);
        }

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.RemittanceDate)
            .ToListAsync();
    }

    public async Task<CODRemittance?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CODRemittances
            .Include(r => r.Customer)
            .Include(r => r.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId.Value);
    }

    public async Task<CODRemittance?> GetByNumberAsync(string remittanceNumber)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CODRemittances
            .Include(r => r.Customer)
            .Include(r => r.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(r => r.RemittanceNumber == remittanceNumber && r.TenantId == tenantId.Value);
    }

    public async Task<List<CustomerCODSummary>> GetPendingRemittancesByCustomerAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CustomerCODSummary>();

        var pendingShipments = await _context.Shipments
            .Include(s => s.Customer)
            .Where(s => s.TenantId == tenantId.Value
                && s.PaymentMode == PaymentMode.COD
                && s.Status == ShipmentStatus.Delivered
                && !s.CODCollected
                && s.CustomerId.HasValue)
            .GroupBy(s => new { s.CustomerId, CustomerName = s.Customer!.Name })
            .Select(g => new CustomerCODSummary
            {
                CustomerId = g.Key.CustomerId!.Value,
                CustomerName = g.Key.CustomerName,
                TotalShipments = g.Count(),
                TotalCODAmount = g.Sum(s => s.CODAmount),
                OldestDeliveryDate = g.Min(s => s.ActualDeliveryDate)
            })
            .ToListAsync();

        return pendingShipments.OrderByDescending(c => c.TotalCODAmount).ToList();
    }

    public async Task<List<Shipment>> GetPendingCODShipmentsAsync(Guid customerId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.DestinationZone)
            .Where(s => s.TenantId == tenantId.Value
                && s.CustomerId == customerId
                && s.PaymentMode == PaymentMode.COD
                && s.Status == ShipmentStatus.Delivered
                && !s.CODCollected)
            .OrderBy(s => s.ActualDeliveryDate)
            .ToListAsync();
    }

    public async Task<CODRemittance> CreateRemittanceAsync(Guid customerId, List<Guid> shipmentIds, string? remarks = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.TenantId == tenantId.Value);

        if (customer == null)
            throw new ArgumentException("Customer not found");

        var shipments = await _context.Shipments
            .Where(s => shipmentIds.Contains(s.Id) 
                && s.TenantId == tenantId.Value
                && s.CustomerId == customerId
                && s.PaymentMode == PaymentMode.COD
                && s.Status == ShipmentStatus.Delivered
                && !s.CODCollected)
            .ToListAsync();

        if (shipments.Count == 0)
            throw new ArgumentException("No valid shipments for remittance");

        var totalCOD = shipments.Sum(s => s.CODAmount);
        var totalMaterialCost = shipments.Sum(s => s.MaterialCostAmount);

        var remittance = new CODRemittance
        {
            RemittanceNumber = await GenerateRemittanceNumberAsync(),
            RemittanceDate = DateTime.UtcNow,
            CustomerId = customerId,
            TotalCODAmount = totalCOD,
            TotalMaterialCost = totalMaterialCost,
            DeductionAmount = 0,
            NetPayableAmount = totalCOD - totalMaterialCost,
            Status = RemittanceStatus.Pending,
            Remarks = remarks
        };

        _context.CODRemittances.Add(remittance);
        await _context.SaveChangesAsync();

        foreach (var shipment in shipments)
        {
            var item = new CODRemittanceItem
            {
                CODRemittanceId = remittance.Id,
                ShipmentId = shipment.Id,
                AWBNumber = shipment.AWBNumber,
                CODAmount = shipment.CODAmount,
                MaterialCostAmount = shipment.MaterialCostAmount,
                DeliveryDate = shipment.ActualDeliveryDate ?? shipment.CreatedAt
            };

            _context.CODRemittanceItems.Add(item);

            shipment.CODCollected = true;
            shipment.RemittanceId = remittance.Id;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("COD Remittance created: {RemittanceNumber}, Customer: {Customer}, Amount: {Amount}", 
            remittance.RemittanceNumber, customer.Name, remittance.NetPayableAmount);

        return remittance;
    }

    public async Task<CODRemittance?> ProcessPaymentAsync(
        Guid remittanceId, 
        string paymentReference, 
        string paymentMethod, 
        DateTime paymentDate, 
        Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var remittance = await _context.CODRemittances
            .Include(r => r.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(r => r.Id == remittanceId && r.TenantId == tenantId.Value);

        if (remittance == null)
        {
            _logger.LogWarning("Remittance not found: {RemittanceId}", remittanceId);
            return null;
        }

        if (remittance.Status != RemittanceStatus.Pending)
        {
            _logger.LogWarning("Remittance not in pending status: {RemittanceId}, Status: {Status}", 
                remittanceId, remittance.Status);
            return null;
        }

        remittance.Status = RemittanceStatus.Paid;
        remittance.PaymentReference = paymentReference;
        remittance.PaymentMethod = paymentMethod;
        remittance.PaymentDate = paymentDate;
        remittance.ProcessedByUserId = userId;
        remittance.ProcessedAt = DateTime.UtcNow;

        foreach (var item in remittance.Items)
        {
            if (item.Shipment != null)
            {
                item.Shipment.MaterialCostRemitted = true;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("COD Remittance paid: {RemittanceNumber}, Reference: {Reference}", 
            remittance.RemittanceNumber, paymentReference);

        return remittance;
    }

    public async Task<CODRemittance?> VoidRemittanceAsync(Guid remittanceId, string reason, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var remittance = await _context.CODRemittances
            .Include(r => r.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(r => r.Id == remittanceId && r.TenantId == tenantId.Value);

        if (remittance == null)
            return null;

        if (remittance.Status == RemittanceStatus.Paid)
        {
            _logger.LogWarning("Cannot void paid remittance: {RemittanceId}", remittanceId);
            return null;
        }

        remittance.Status = RemittanceStatus.Cancelled;
        remittance.Remarks = $"Voided: {reason}";

        foreach (var item in remittance.Items)
        {
            if (item.Shipment != null)
            {
                item.Shipment.CODCollected = false;
                item.Shipment.RemittanceId = null;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("COD Remittance voided: {RemittanceNumber}, Reason: {Reason}", 
            remittance.RemittanceNumber, reason);

        return remittance;
    }

    public async Task<decimal> CalculatePendingCODAsync(Guid customerId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return 0;

        return await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.CustomerId == customerId
                && s.PaymentMode == PaymentMode.COD
                && s.Status == ShipmentStatus.Delivered
                && !s.CODCollected)
            .SumAsync(s => s.CODAmount);
    }

    public async Task<CODRemittanceSummary> GetRemittanceSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new CODRemittanceSummary();

        var query = _context.CODRemittances
            .Where(r => r.TenantId == tenantId.Value);

        if (fromDate.HasValue)
        {
            var utcFromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.RemittanceDate >= utcFromDate);
        }

        if (toDate.HasValue)
        {
            var utcToDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.RemittanceDate <= utcToDate);
        }

        var remittances = await query.ToListAsync();

        var pendingCOD = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.PaymentMode == PaymentMode.COD
                && s.Status == ShipmentStatus.Delivered
                && !s.CODCollected)
            .SumAsync(s => s.CODAmount);

        return new CODRemittanceSummary
        {
            TotalRemittances = remittances.Count,
            PendingRemittances = remittances.Count(r => r.Status == RemittanceStatus.Pending),
            PaidRemittances = remittances.Count(r => r.Status == RemittanceStatus.Paid),
            TotalCODAmount = remittances.Sum(r => r.TotalCODAmount),
            TotalPaidAmount = remittances.Where(r => r.Status == RemittanceStatus.Paid).Sum(r => r.NetPayableAmount),
            TotalPendingCOD = pendingCOD
        };
    }

    public async Task<string> GenerateRemittanceNumberAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var today = DateTime.UtcNow;
        var prefix = $"REM{today:yyyyMMdd}";

        var lastRemittance = await _context.CODRemittances
            .Where(r => r.TenantId == tenantId.Value && r.RemittanceNumber.StartsWith(prefix))
            .OrderByDescending(r => r.RemittanceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastRemittance != null)
        {
            var lastNumberStr = lastRemittance.RemittanceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}

public class CustomerCODSummary
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalCODAmount { get; set; }
    public DateTime? OldestDeliveryDate { get; set; }
    public int DaysPending => OldestDeliveryDate.HasValue ? (DateTime.UtcNow - OldestDeliveryDate.Value).Days : 0;
}

public class CODRemittanceSummary
{
    public int TotalRemittances { get; set; }
    public int PendingRemittances { get; set; }
    public int PaidRemittances { get; set; }
    public decimal TotalCODAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalPendingCOD { get; set; }
}
