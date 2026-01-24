using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class CODRemittanceService
{
    private readonly ApplicationDbContext _context;

    public CODRemittanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CODRemittance>> GetRemittancesAsync(
        long? branchId = null,
        long? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CODRemittanceStatus? status = null)
    {
        var query = _context.CODRemittances.AsQueryable();

        if (branchId.HasValue)
            query = query.Where(r => r.BranchId == branchId);
        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId);
        if (fromDate.HasValue)
            query = query.Where(r => r.RemittanceDate >= DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc));
        if (toDate.HasValue)
            query = query.Where(r => r.RemittanceDate <= DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc));
        if (status.HasValue)
            query = query.Where(r => r.Status == status);

        return await query
            .OrderByDescending(r => r.RemittanceDate)
            .ThenByDescending(r => r.Id)
            .ToListAsync();
    }

    public async Task<CODRemittance?> GetRemittanceWithDetailsAsync(long id)
    {
        return await _context.CODRemittances
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<PendingCODSummary>> GetPendingCODByCustomerAsync(long? branchId = null)
    {
        var query = _context.InscanMasters
            .Where(i => i.IsCOD && i.CODCollected && i.CODAmount > 0)
            .Where(i => !_context.CODRemittanceDetails.Any(d => d.InscanMasterId == i.Id));

        if (branchId.HasValue)
            query = query.Where(i => i.BranchId == branchId);

        var pendingCOD = await query
            .GroupBy(i => new { i.CustomerId, CustomerName = i.Consignor ?? "Unknown" })
            .Select(g => new PendingCODSummary
            {
                CustomerId = g.Key.CustomerId ?? 0,
                CustomerName = g.Key.CustomerName,
                ShipmentCount = g.Count(),
                TotalCODAmount = g.Sum(i => i.CODAmount ?? 0)
            })
            .OrderByDescending(s => s.TotalCODAmount)
            .ToListAsync();

        return pendingCOD;
    }

    public async Task<List<InscanMaster>> GetPendingCODShipmentsAsync(long customerId, long? branchId = null)
    {
        var query = _context.InscanMasters
            .Where(i => i.CustomerId == customerId)
            .Where(i => i.IsCOD && i.CODCollected && i.CODAmount > 0)
            .Where(i => !_context.CODRemittanceDetails.Any(d => d.InscanMasterId == i.Id));

        if (branchId.HasValue)
            query = query.Where(i => i.BranchId == branchId);

        return await query
            .OrderBy(i => i.DeliveredDate)
            .ToListAsync();
    }

    public async Task<CODRemittance> CreateRemittanceAsync(
        long customerId,
        List<long> shipmentIds,
        decimal serviceChargePercent,
        long? companyId,
        long? branchId,
        long? financialYearId,
        long userId,
        string userName)
    {
        var customer = await _context.Parties.FindAsync(customerId);
        var shipments = await _context.InscanMasters
            .Where(i => shipmentIds.Contains(i.Id))
            .ToListAsync();

        var remittanceNo = await GenerateRemittanceNoAsync(branchId);
        var totalCOD = shipments.Sum(s => s.CODAmount ?? 0);
        var serviceCharge = Math.Round(totalCOD * (serviceChargePercent / 100), 2);
        var netPayable = totalCOD - serviceCharge;

        var remittance = new CODRemittance
        {
            RemittanceNo = remittanceNo,
            RemittanceDate = DateTime.UtcNow,
            CompanyId = companyId,
            BranchId = branchId,
            FinancialYearId = financialYearId,
            CustomerId = customerId,
            CustomerName = customer?.Name,
            CustomerCode = customer?.Code,
            TotalCODAmount = totalCOD,
            ServiceChargePercent = serviceChargePercent,
            ServiceCharge = serviceCharge,
            NetPayable = netPayable,
            BalanceAmount = netPayable,
            Status = CODRemittanceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = (int)userId,
            CreatedByName = userName
        };

        foreach (var shipment in shipments)
        {
            var itemServiceCharge = Math.Round((shipment.CODAmount ?? 0) * (serviceChargePercent / 100), 2);
            remittance.Details.Add(new CODRemittanceDetail
            {
                InscanMasterId = shipment.Id,
                AWBNo = shipment.AWBNo,
                DeliveredDate = shipment.DeliveredDate,
                ConsigneeName = shipment.Consignee,
                CODAmount = shipment.CODAmount ?? 0,
                CollectedAmount = shipment.CODAmount ?? 0,
                ServiceCharge = itemServiceCharge,
                NetPayable = (shipment.CODAmount ?? 0) - itemServiceCharge
            });
        }

        _context.CODRemittances.Add(remittance);
        await _context.SaveChangesAsync();

        return remittance;
    }

    public async Task<bool> ApproveRemittanceAsync(long id, long userId, string userName)
    {
        var remittance = await _context.CODRemittances.FindAsync(id);
        if (remittance == null || remittance.Status != CODRemittanceStatus.Draft)
            return false;

        remittance.Status = CODRemittanceStatus.Approved;
        remittance.ApprovedAt = DateTime.UtcNow;
        remittance.ApprovedByUserId = userId;
        remittance.ApprovedByUserName = userName;
        remittance.ModifiedAt = DateTime.UtcNow;
        remittance.ModifiedBy = (int)userId;
        remittance.ModifiedByName = userName;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(
        long id,
        decimal paidAmount,
        string paymentMode,
        string? paymentReference,
        string? bankName,
        string? chequeNo,
        DateTime? chequeDate,
        string? transactionId,
        long userId,
        string userName)
    {
        var remittance = await _context.CODRemittances.FindAsync(id);
        if (remittance == null || remittance.Status == CODRemittanceStatus.Cancelled)
            return false;

        remittance.PaidAmount += paidAmount;
        remittance.BalanceAmount = remittance.NetPayable - remittance.PaidAmount;
        remittance.PaymentMode = paymentMode;
        remittance.PaymentReference = paymentReference;
        remittance.BankName = bankName;
        remittance.ChequeNo = chequeNo;
        remittance.ChequeDate = chequeDate;
        remittance.TransactionId = transactionId;
        remittance.PaidAt = DateTime.UtcNow;
        remittance.PaidByUserId = userId;
        remittance.PaidByUserName = userName;
        remittance.ModifiedAt = DateTime.UtcNow;
        remittance.ModifiedBy = (int)userId;
        remittance.ModifiedByName = userName;

        if (remittance.BalanceAmount <= 0)
            remittance.Status = CODRemittanceStatus.Paid;
        else if (remittance.PaidAmount > 0)
            remittance.Status = CODRemittanceStatus.PartiallyPaid;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelRemittanceAsync(long id, string reason, long userId, string userName)
    {
        var remittance = await _context.CODRemittances.FindAsync(id);
        if (remittance == null || remittance.Status == CODRemittanceStatus.Paid)
            return false;

        remittance.Status = CODRemittanceStatus.Cancelled;
        remittance.Remarks = reason;
        remittance.ModifiedAt = DateTime.UtcNow;
        remittance.ModifiedBy = (int)userId;
        remittance.ModifiedByName = userName;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateRemittanceNoAsync(long? branchId)
    {
        var prefix = "COD";
        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        var lastRemittance = await _context.CODRemittances
            .Where(r => r.RemittanceNo.StartsWith($"{prefix}-{datePart}"))
            .OrderByDescending(r => r.RemittanceNo)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastRemittance != null)
        {
            var parts = lastRemittance.RemittanceNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}-{datePart}-{sequence:D5}";
    }
}

public class PendingCODSummary
{
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ShipmentCount { get; set; }
    public decimal TotalCODAmount { get; set; }
}
