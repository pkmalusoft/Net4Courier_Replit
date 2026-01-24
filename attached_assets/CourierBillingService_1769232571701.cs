using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Core.Common;

namespace Server.Modules.Courier.Services;

public interface ICourierBillingService
{
    Task<FreightCalculation> CalculateFreightAsync(Guid serviceTypeId, Guid originZoneId, Guid destinationZoneId, decimal weight, decimal declaredValue, PaymentMode paymentMode);
    Task<List<CustomerBillingSummary>> GetPendingBillingByCustomerAsync();
    Task<List<Shipment>> GetUnbilledShipmentsAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<CustomerInvoiceBatch> GenerateCustomerInvoicesAsync(Guid customerId, List<Guid> shipmentIds, DateTime invoiceDate, string? remarks = null);
    Task<decimal> CalculateAgentCommissionAsync(Guid agentId, DateTime fromDate, DateTime toDate);
    Task<List<AgentCommissionSummary>> GetAgentCommissionSummaryAsync(DateTime fromDate, DateTime toDate);
    Task<BillingReport> GetBillingReportAsync(DateTime fromDate, DateTime toDate);
}

public class CourierBillingService : ICourierBillingService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IZoneRateService _zoneRateService;
    private readonly ILogger<CourierBillingService> _logger;

    public CourierBillingService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IZoneRateService zoneRateService,
        ILogger<CourierBillingService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _zoneRateService = zoneRateService;
        _logger = logger;
    }

    public async Task<FreightCalculation> CalculateFreightAsync(
        Guid serviceTypeId, 
        Guid originZoneId, 
        Guid destinationZoneId, 
        decimal weight, 
        decimal declaredValue, 
        PaymentMode paymentMode)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var calculation = new FreightCalculation
        {
            ServiceTypeId = serviceTypeId,
            OriginZoneId = originZoneId,
            DestinationZoneId = destinationZoneId,
            Weight = weight,
            DeclaredValue = declaredValue,
            PaymentMode = paymentMode
        };

        var rate = await _zoneRateService.GetRateAsync(destinationZoneId, serviceTypeId, weight);
        
        if (rate == null)
        {
            calculation.ErrorMessage = "No rate found for the specified route";
            return calculation;
        }

        calculation.BaseRate = rate.BaseRate;
        calculation.RatePerKg = rate.AdditionalRatePerKg;
        calculation.MinimumCharge = rate.MinCharge;

        decimal freightCharge = rate.BaseRate;
        if (weight > 0)
        {
            freightCharge += weight * rate.AdditionalRatePerKg;
        }

        if (freightCharge < rate.MinCharge)
        {
            freightCharge = rate.MinCharge;
        }

        calculation.FreightCharge = freightCharge;

        if (rate.FuelSurchargePercent > 0)
        {
            calculation.FuelSurcharge = freightCharge * (rate.FuelSurchargePercent / 100);
        }

        if (rate.InsurancePercent > 0 && declaredValue > 0)
        {
            calculation.InsuranceCharge = declaredValue * (rate.InsurancePercent / 100);
        }

        if (paymentMode == PaymentMode.COD && rate.CODChargePercent > 0)
        {
            calculation.CODCharge = declaredValue * (rate.CODChargePercent / 100);
            if (calculation.CODCharge < rate.CODMinCharge)
            {
                calculation.CODCharge = rate.CODMinCharge;
            }
        }

        calculation.SubTotal = calculation.FreightCharge + calculation.FuelSurcharge + calculation.InsuranceCharge + calculation.CODCharge;

        var serviceType = await _context.CourierServiceTypes
            .FirstOrDefaultAsync(s => s.Id == serviceTypeId && s.TenantId == tenantId.Value);

        if (serviceType?.TaxPercent > 0)
        {
            calculation.TaxPercent = serviceType.TaxPercent;
            calculation.TaxAmount = calculation.SubTotal * (serviceType.TaxPercent / 100);
        }

        calculation.TotalCharge = calculation.SubTotal + calculation.TaxAmount;
        calculation.IsValid = true;

        return calculation;
    }

    public async Task<List<CustomerBillingSummary>> GetPendingBillingByCustomerAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CustomerBillingSummary>();

        var unbilledShipments = await _context.Shipments
            .Include(s => s.Customer)
            .Where(s => s.TenantId == tenantId.Value
                && s.CustomerId.HasValue
                && s.Status == ShipmentStatus.Delivered
                && !s.IsBilled
                && !s.IsVoided
                && (s.PaymentMode == PaymentMode.Credit || s.PaymentMode == PaymentMode.Prepaid))
            .GroupBy(s => new { s.CustomerId, CustomerName = s.Customer!.Name })
            .Select(g => new CustomerBillingSummary
            {
                CustomerId = g.Key.CustomerId!.Value,
                CustomerName = g.Key.CustomerName,
                TotalShipments = g.Count(),
                TotalFreight = g.Sum(s => s.TotalCharge),
                OldestShipmentDate = g.Min(s => s.BookingDate)
            })
            .ToListAsync();

        return unbilledShipments.OrderByDescending(c => c.TotalFreight).ToList();
    }

    public async Task<List<Shipment>> GetUnbilledShipmentsAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        var query = _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.OriginZone)
            .Include(s => s.DestinationZone)
            .Where(s => s.TenantId == tenantId.Value
                && s.CustomerId == customerId
                && s.Status == ShipmentStatus.Delivered
                && !s.IsBilled
                && !s.IsVoided);

        if (fromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(s => s.BookingDate >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(s => s.BookingDate <= toDateUtc);
        }

        return await query.OrderBy(s => s.BookingDate).ToListAsync();
    }

    public async Task<CustomerInvoiceBatch> GenerateCustomerInvoicesAsync(
        Guid customerId, 
        List<Guid> shipmentIds, 
        DateTime invoiceDate,
        string? remarks = null)
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
                && !s.IsBilled)
            .ToListAsync();

        if (shipments.Count == 0)
            throw new ArgumentException("No valid shipments for invoicing");

        var batch = new CustomerInvoiceBatch
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            InvoiceDate = invoiceDate,
            ShipmentCount = shipments.Count,
            TotalFreight = shipments.Sum(s => s.FreightCharge),
            TotalFuelSurcharge = shipments.Sum(s => s.FuelSurcharge),
            TotalInsurance = shipments.Sum(s => s.InsuranceCharge),
            TotalOtherCharges = shipments.Sum(s => s.OtherCharges),
            TotalTax = shipments.Sum(s => s.TaxAmount),
            TotalAmount = shipments.Sum(s => s.TotalCharge),
            Remarks = remarks
        };

        foreach (var shipment in shipments)
        {
            shipment.IsBilled = true;
            shipment.BilledDate = invoiceDate;

            batch.ShipmentDetails.Add(new ShipmentBillingDetail
            {
                ShipmentId = shipment.Id,
                AWBNumber = shipment.AWBNumber,
                BookingDate = shipment.BookingDate,
                DeliveryDate = shipment.ActualDeliveryDate,
                Weight = shipment.ChargeableWeight,
                FreightCharge = shipment.FreightCharge,
                TotalCharge = shipment.TotalCharge
            });
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer invoice batch generated: {Customer}, Shipments: {Count}, Amount: {Amount}", 
            customer.Name, batch.ShipmentCount, batch.TotalAmount);

        return batch;
    }

    public async Task<decimal> CalculateAgentCommissionAsync(Guid agentId, DateTime fromDate, DateTime toDate)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return 0;

        var agent = await _context.CourierAgents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.TenantId == tenantId.Value);

        if (agent == null)
            return 0;

        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.AssignedAgentId == agentId
                && s.BookingDate >= fromDate
                && s.BookingDate <= toDate
                && s.Status == ShipmentStatus.Delivered
                && !s.IsVoided)
            .ToListAsync();

        decimal totalCommission = 0;

        foreach (var shipment in shipments)
        {
            if (agent.CommissionType == CommissionType.Percentage)
            {
                totalCommission += shipment.FreightCharge * (agent.CommissionRate / 100);
            }
            else
            {
                totalCommission += agent.CommissionRate;
            }
        }

        return totalCommission;
    }

    public async Task<List<AgentCommissionSummary>> GetAgentCommissionSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<AgentCommissionSummary>();

        var agents = await _context.CourierAgents
            .Where(a => a.TenantId == tenantId.Value && a.IsActive)
            .ToListAsync();

        var summaries = new List<AgentCommissionSummary>();

        foreach (var agent in agents)
        {
            var shipments = await _context.Shipments
                .Where(s => s.TenantId == tenantId.Value
                    && s.AssignedAgentId == agent.Id
                    && s.BookingDate >= fromDate
                    && s.BookingDate <= toDate
                    && s.Status == ShipmentStatus.Delivered
                    && !s.IsVoided)
                .ToListAsync();

            if (shipments.Count > 0)
            {
                decimal commission = 0;
                foreach (var shipment in shipments)
                {
                    if (agent.CommissionType == CommissionType.Percentage)
                    {
                        commission += shipment.FreightCharge * (agent.CommissionRate / 100);
                    }
                    else
                    {
                        commission += agent.CommissionRate;
                    }
                }

                summaries.Add(new AgentCommissionSummary
                {
                    AgentId = agent.Id,
                    AgentName = agent.Name,
                    AgentCode = agent.AgentCode,
                    TotalShipments = shipments.Count,
                    TotalFreight = shipments.Sum(s => s.FreightCharge),
                    CommissionRate = agent.CommissionRate,
                    CommissionType = agent.CommissionType,
                    TotalCommission = commission
                });
            }
        }

        return summaries.OrderByDescending(s => s.TotalCommission).ToList();
    }

    public async Task<BillingReport> GetBillingReportAsync(DateTime fromDate, DateTime toDate)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new BillingReport();

        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value
                && s.BookingDate >= fromDate
                && s.BookingDate <= toDate
                && !s.IsVoided)
            .ToListAsync();

        var report = new BillingReport
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalShipments = shipments.Count,
            TotalWeight = shipments.Sum(s => s.ChargeableWeight),
            TotalFreight = shipments.Sum(s => s.FreightCharge),
            TotalFuelSurcharge = shipments.Sum(s => s.FuelSurcharge),
            TotalInsurance = shipments.Sum(s => s.InsuranceCharge),
            TotalCODCharges = shipments.Where(s => s.PaymentMode == PaymentMode.COD).Sum(s => s.TotalCharge - s.FreightCharge - s.FuelSurcharge - s.InsuranceCharge),
            TotalTax = shipments.Sum(s => s.TaxAmount),
            TotalRevenue = shipments.Sum(s => s.TotalCharge),
            BilledShipments = shipments.Count(s => s.IsBilled),
            UnbilledShipments = shipments.Count(s => !s.IsBilled),
            BilledAmount = shipments.Where(s => s.IsBilled).Sum(s => s.TotalCharge),
            UnbilledAmount = shipments.Where(s => !s.IsBilled).Sum(s => s.TotalCharge)
        };

        report.ByPaymentMode = shipments
            .GroupBy(s => s.PaymentMode)
            .Select(g => new PaymentModeSummary
            {
                PaymentMode = g.Key,
                ShipmentCount = g.Count(),
                TotalAmount = g.Sum(s => s.TotalCharge)
            })
            .ToList();

        report.ByServiceType = await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Where(s => s.TenantId == tenantId.Value
                && s.BookingDate >= fromDate
                && s.BookingDate <= toDate
                && !s.IsVoided)
            .GroupBy(s => new { s.CourierServiceTypeId, ServiceTypeName = s.CourierServiceType.Name })
            .Select(g => new ServiceTypeSummary
            {
                ServiceTypeId = g.Key.CourierServiceTypeId,
                ServiceTypeName = g.Key.ServiceTypeName,
                ShipmentCount = g.Count(),
                TotalAmount = g.Sum(s => s.TotalCharge)
            })
            .ToListAsync();

        return report;
    }
}

public class FreightCalculation
{
    public Guid ServiceTypeId { get; set; }
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public decimal Weight { get; set; }
    public decimal DeclaredValue { get; set; }
    public PaymentMode PaymentMode { get; set; }
    
    public decimal BaseRate { get; set; }
    public decimal RatePerKg { get; set; }
    public decimal MinimumCharge { get; set; }
    
    public decimal FreightCharge { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal InsuranceCharge { get; set; }
    public decimal CODCharge { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalCharge { get; set; }
    
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CustomerBillingSummary
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalFreight { get; set; }
    public DateTime OldestShipmentDate { get; set; }
    public int DaysPending => (DateTime.UtcNow - OldestShipmentDate).Days;
}

public class CustomerInvoiceBatch
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int ShipmentCount { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal TotalFuelSurcharge { get; set; }
    public decimal TotalInsurance { get; set; }
    public decimal TotalOtherCharges { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public List<ShipmentBillingDetail> ShipmentDetails { get; set; } = new();
}

public class ShipmentBillingDetail
{
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal Weight { get; set; }
    public decimal FreightCharge { get; set; }
    public decimal TotalCharge { get; set; }
}

public class AgentCommissionSummary
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentCode { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal CommissionRate { get; set; }
    public CommissionType CommissionType { get; set; }
    public decimal TotalCommission { get; set; }
}

public class BillingReport
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalShipments { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal TotalFuelSurcharge { get; set; }
    public decimal TotalInsurance { get; set; }
    public decimal TotalCODCharges { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalRevenue { get; set; }
    public int BilledShipments { get; set; }
    public int UnbilledShipments { get; set; }
    public decimal BilledAmount { get; set; }
    public decimal UnbilledAmount { get; set; }
    public List<PaymentModeSummary> ByPaymentMode { get; set; } = new();
    public List<ServiceTypeSummary> ByServiceType { get; set; } = new();
}

public class PaymentModeSummary
{
    public PaymentMode PaymentMode { get; set; }
    public int ShipmentCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ServiceTypeSummary
{
    public Guid ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public int ShipmentCount { get; set; }
    public decimal TotalAmount { get; set; }
}
