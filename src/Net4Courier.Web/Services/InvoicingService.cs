using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Finance.Entities;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class InvoicingService
{
    private readonly ApplicationDbContext _context;
    private readonly ShipmentStatusService _statusService;

    public InvoicingService(ApplicationDbContext context, ShipmentStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<List<InscanMaster>> GetUnbilledAWBs(long customerId, DateTime fromDate, DateTime toDate)
    {
        return await _context.InscanMasters
            .Include(i => i.OtherCharges)
                .ThenInclude(o => o.OtherChargeType)
            .Where(i => i.CustomerId == customerId &&
                        i.TransactionDate >= fromDate &&
                        i.TransactionDate <= toDate &&
                        i.InvoiceId == null &&
                        !i.IsDeleted)
            .OrderBy(i => i.TransactionDate)
            .ThenBy(i => i.AWBNo)
            .ToListAsync();
    }

    public async Task<List<SpecialCharge>> GetApplicableSpecialCharges(long customerId, DateTime invoiceDate)
    {
        return await _context.SpecialCharges
            .Where(c => c.Status == SpecialChargeStatus.Approved &&
                        c.IsActive && !c.IsDeleted &&
                        c.FromDate <= invoiceDate &&
                        c.ToDate >= invoiceDate &&
                        (c.CustomerId == null || c.CustomerId == customerId))
            .OrderByDescending(c => c.CustomerId)
            .ThenBy(c => c.ChargeName)
            .ToListAsync();
    }

    public InvoicePreviewModel GenerateInvoicePreview(
        long customerId,
        string customerName,
        string? customerAddress,
        string? customerTaxNo,
        DateTime invoiceDate,
        DateTime periodFrom,
        DateTime periodTo,
        List<InscanMaster> awbs,
        List<SpecialCharge> specialCharges,
        decimal taxPercent = 18)
    {
        var preview = new InvoicePreviewModel
        {
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerAddress = customerAddress,
            CustomerTaxNo = customerTaxNo,
            InvoiceDate = invoiceDate,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            TaxPercent = taxPercent
        };

        foreach (var awb in awbs)
        {
            var courierCharge = awb.CourierCharge ?? 0;
            var otherCharge = awb.OtherCharge ?? 0;

            preview.Details.Add(new InvoiceDetailPreview
            {
                InscanId = awb.Id,
                AWBNo = awb.AWBNo ?? "",
                AWBDate = awb.TransactionDate,
                Origin = awb.ConsignorCity ?? "",
                Destination = awb.ConsigneeCity ?? "",
                Pieces = awb.Pieces ?? 0,
                Weight = awb.ChargeableWeight ?? awb.Weight ?? 0,
                CourierCharge = courierCharge,
                OtherCharge = otherCharge,
                Total = courierCharge + otherCharge
            });
        }

        preview.SubTotal = preview.Details.Sum(d => d.Total);

        foreach (var charge in specialCharges)
        {
            decimal calculatedAmount;
            if (charge.ChargeType == ChargeType.Percentage)
            {
                calculatedAmount = preview.SubTotal * charge.ChargeValue / 100;
            }
            else
            {
                calculatedAmount = charge.ChargeValue;
            }

            decimal chargeTax = 0;
            if (charge.IsTaxApplicable && charge.TaxPercent.HasValue)
            {
                chargeTax = calculatedAmount * charge.TaxPercent.Value / 100;
            }

            preview.SpecialCharges.Add(new SpecialChargePreview
            {
                SpecialChargeId = charge.Id,
                ChargeName = charge.ChargeName,
                ChargeType = charge.ChargeType,
                ChargeValue = charge.ChargeValue,
                CalculatedAmount = calculatedAmount,
                IsTaxApplicable = charge.IsTaxApplicable,
                TaxPercent = charge.TaxPercent ?? 0,
                TaxAmount = chargeTax,
                TotalAmount = calculatedAmount + chargeTax
            });
        }

        preview.SpecialChargesTotal = preview.SpecialCharges.Sum(s => s.CalculatedAmount);
        preview.SpecialChargesTax = preview.SpecialCharges.Sum(s => s.TaxAmount);
        preview.TaxAmount = preview.SubTotal * taxPercent / 100;
        preview.NetTotal = preview.SubTotal + preview.TaxAmount + preview.SpecialChargesTotal + preview.SpecialChargesTax;

        return preview;
    }

    public async Task<Invoice> CreateInvoice(InvoicePreviewModel preview)
    {
        var invoiceNo = await GenerateInvoiceNumber();

        var invoice = new Invoice
        {
            InvoiceNo = invoiceNo,
            InvoiceDate = DateTime.SpecifyKind(preview.InvoiceDate, DateTimeKind.Utc),
            CompanyId = 1,
            CustomerId = preview.CustomerId,
            CustomerName = preview.CustomerName,
            CustomerAddress = preview.CustomerAddress,
            CustomerTaxNo = preview.CustomerTaxNo,
            PeriodFrom = DateTime.SpecifyKind(preview.PeriodFrom, DateTimeKind.Utc),
            PeriodTo = DateTime.SpecifyKind(preview.PeriodTo, DateTimeKind.Utc),
            TotalAWBs = preview.Details.Count,
            SubTotal = preview.SubTotal,
            TaxPercent = preview.TaxPercent,
            TaxAmount = preview.TaxAmount,
            SpecialChargesTotal = preview.SpecialChargesTotal,
            SpecialChargesTax = preview.SpecialChargesTax,
            NetTotal = preview.NetTotal,
            BalanceAmount = preview.NetTotal,
            Status = InvoiceStatus.Draft,
            DueDate = preview.InvoiceDate.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var detail in preview.Details)
        {
            invoice.Details.Add(new InvoiceDetail
            {
                InscanId = detail.InscanId,
                AWBNo = detail.AWBNo,
                AWBDate = detail.AWBDate,
                Origin = detail.Origin,
                Destination = detail.Destination,
                Pieces = detail.Pieces,
                Weight = detail.Weight,
                CourierCharge = detail.CourierCharge,
                OtherCharge = detail.OtherCharge,
                Total = detail.Total,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var sc in preview.SpecialCharges)
        {
            invoice.SpecialCharges.Add(new InvoiceSpecialCharge
            {
                SpecialChargeId = sc.SpecialChargeId,
                ChargeName = sc.ChargeName,
                ChargeType = sc.ChargeType,
                ChargeValue = sc.ChargeValue,
                CalculatedAmount = sc.CalculatedAmount,
                IsTaxApplicable = sc.IsTaxApplicable,
                TaxPercent = sc.TaxPercent,
                TaxAmount = sc.TaxAmount,
                TotalAmount = sc.TotalAmount,
                CreatedAt = DateTime.UtcNow
            });

            var specialCharge = await _context.SpecialCharges.FindAsync(sc.SpecialChargeId);
            if (specialCharge != null)
            {
                specialCharge.IsLocked = true;
            }
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        foreach (var detail in preview.Details)
        {
            var awb = await _context.InscanMasters.FindAsync(detail.InscanId);
            if (awb != null)
            {
                awb.InvoiceId = invoice.Id;
                awb.ModifiedAt = DateTime.UtcNow;
                
                await _statusService.SetStatus(
                    awb.Id, "INVOICED", "Invoice", invoice.Id, "Invoice",
                    null, null, null, null,
                    $"Invoice {invoice.InvoiceNo} generated", isAutomatic: true);
            }
        }

        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task<Invoice> PostInvoice(long invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Details)
            .Include(i => i.SpecialCharges)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            throw new Exception("Invoice not found");

        invoice.Status = InvoiceStatus.Generated;
        invoice.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return invoice;
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV{today:yyyyMM}";

        var count = await _context.Invoices
            .CountAsync(i => i.InvoiceNo.StartsWith(prefix));

        return $"{prefix}{(count + 1):D5}";
    }
}

public class InvoicePreviewModel
{
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string? CustomerAddress { get; set; }
    public string? CustomerTaxNo { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal SpecialChargesTotal { get; set; }
    public decimal SpecialChargesTax { get; set; }
    public decimal NetTotal { get; set; }

    public List<InvoiceDetailPreview> Details { get; set; } = new();
    public List<SpecialChargePreview> SpecialCharges { get; set; } = new();
}

public class InvoiceDetailPreview
{
    public long InscanId { get; set; }
    public string AWBNo { get; set; } = "";
    public DateTime AWBDate { get; set; }
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public int Pieces { get; set; }
    public decimal Weight { get; set; }
    public decimal CourierCharge { get; set; }
    public decimal OtherCharge { get; set; }
    public decimal Total { get; set; }
}

public class SpecialChargePreview
{
    public long SpecialChargeId { get; set; }
    public string ChargeName { get; set; } = "";
    public ChargeType ChargeType { get; set; }
    public decimal ChargeValue { get; set; }
    public decimal CalculatedAmount { get; set; }
    public bool IsTaxApplicable { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
