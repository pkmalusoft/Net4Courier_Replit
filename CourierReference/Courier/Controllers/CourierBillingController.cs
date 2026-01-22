using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/billing")]
[Authorize]
public class CourierBillingController : ControllerBase
{
    private readonly ICourierBillingService _billingService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CourierBillingController> _logger;

    public CourierBillingController(
        ICourierBillingService billingService,
        ITenantProvider tenantProvider,
        ILogger<CourierBillingController> logger)
    {
        _billingService = billingService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpPost("calculate-freight")]
    public async Task<ActionResult<FreightCalculationResultDto>> CalculateFreight(
        [FromBody] CalculateFreightRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (!Enum.TryParse<PaymentMode>(request.PaymentMode, out var paymentMode))
                paymentMode = PaymentMode.Prepaid;

            var result = await _billingService.CalculateFreightAsync(
                request.ServiceTypeId,
                request.OriginZoneId,
                request.DestinationZoneId,
                request.Weight,
                request.DeclaredValue,
                paymentMode);

            return Ok(new FreightCalculationResultDto
            {
                Weight = result.Weight,
                DeclaredValue = result.DeclaredValue,
                BaseRate = result.BaseRate,
                RatePerKg = result.RatePerKg,
                MinimumCharge = result.MinimumCharge,
                FreightCharge = result.FreightCharge,
                FuelSurcharge = result.FuelSurcharge,
                InsuranceCharge = result.InsuranceCharge,
                CODCharge = result.CODCharge,
                SubTotal = result.SubTotal,
                TaxPercent = result.TaxPercent,
                TaxAmount = result.TaxAmount,
                TotalCharge = result.TotalCharge
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating freight");
            return StatusCode(500, "An error occurred while calculating freight.");
        }
    }

    [HttpPost("calculate-batch")]
    public async Task<ActionResult<List<BatchFreightCalculationResultDto>>> CalculateFreightBatch(
        [FromBody] CalculateFreightBatchRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var results = new List<BatchFreightCalculationResultDto>();

            foreach (var item in request.Items)
            {
                try
                {
                    if (!Enum.TryParse<PaymentMode>(item.PaymentMode, out var paymentMode))
                        paymentMode = PaymentMode.Prepaid;

                    var result = await _billingService.CalculateFreightAsync(
                        item.ServiceTypeId,
                        item.OriginZoneId,
                        item.DestinationZoneId,
                        item.Weight,
                        item.DeclaredValue,
                        paymentMode);

                    results.Add(new BatchFreightCalculationResultDto
                    {
                        ReferenceId = item.ReferenceId,
                        Success = true,
                        Weight = result.Weight,
                        TotalCharge = result.TotalCharge,
                        FreightCharge = result.FreightCharge,
                        FuelSurcharge = result.FuelSurcharge,
                        InsuranceCharge = result.InsuranceCharge,
                        CODCharge = result.CODCharge,
                        TaxAmount = result.TaxAmount
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new BatchFreightCalculationResultDto
                    {
                        ReferenceId = item.ReferenceId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating batch freight");
            return StatusCode(500, "An error occurred while calculating batch freight.");
        }
    }

    [HttpGet("pending-by-customer")]
    public async Task<ActionResult<List<CustomerBillingSummaryDto>>> GetPendingBillingByCustomer()
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var summaries = await _billingService.GetPendingBillingByCustomerAsync();

            return Ok(summaries.Select(s => new CustomerBillingSummaryDto
            {
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                TotalShipments = s.TotalShipments,
                TotalFreight = s.TotalFreight,
                OldestShipmentDate = s.OldestShipmentDate
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending billing by customer");
            return StatusCode(500, "An error occurred while fetching pending billing.");
        }
    }

    [HttpGet("customer/{customerId}/unbilled")]
    public async Task<ActionResult<List<UnbilledShipmentDto>>> GetUnbilledShipments(
        Guid customerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var shipments = await _billingService.GetUnbilledShipmentsAsync(customerId, fromDate, toDate);

            return Ok(shipments.Select(s => new UnbilledShipmentDto
            {
                ShipmentId = s.Id,
                AWBNumber = s.AWBNumber,
                BookingDate = s.BookingDate,
                DeliveryDate = s.ActualDeliveryDate,
                ServiceTypeName = s.CourierServiceType?.Name,
                OriginCity = s.SenderCity,
                DestinationCity = s.ReceiverCity,
                Pieces = s.NumberOfPieces,
                ChargeableWeight = s.ChargeableWeight,
                FreightCharge = s.FreightCharge,
                TotalCharge = s.TotalCharge,
                PaymentMode = s.PaymentMode.ToString()
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching unbilled shipments for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while fetching unbilled shipments.");
        }
    }

    [HttpPost("generate-invoices")]
    public async Task<ActionResult<InvoiceGenerationResultDto>> GenerateInvoices(
        [FromBody] GenerateInvoicesRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (request.ShipmentIds == null || request.ShipmentIds.Count == 0)
                return BadRequest("At least one shipment is required.");

            var result = await _billingService.GenerateCustomerInvoicesAsync(
                request.CustomerId,
                request.ShipmentIds,
                request.InvoiceDate,
                request.Remarks);

            return Ok(new InvoiceGenerationResultDto
            {
                Success = true,
                Message = "Invoice generated successfully",
                CustomerId = result.CustomerId,
                CustomerName = result.CustomerName,
                InvoiceDate = result.InvoiceDate,
                ShipmentCount = result.ShipmentCount,
                TotalFreight = result.TotalFreight,
                TotalFuelSurcharge = result.TotalFuelSurcharge,
                TotalInsurance = result.TotalInsurance,
                TotalOtherCharges = result.TotalOtherCharges,
                TotalTax = result.TotalTax,
                TotalAmount = result.TotalAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoices");
            return StatusCode(500, "An error occurred while generating invoices.");
        }
    }

    [HttpGet("agent/{agentId}/commission")]
    public async Task<ActionResult<decimal>> CalculateAgentCommission(
        Guid agentId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var commission = await _billingService.CalculateAgentCommissionAsync(agentId, fromDate, toDate);
            return Ok(commission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating commission for agent {AgentId}", agentId);
            return StatusCode(500, "An error occurred while calculating commission.");
        }
    }

    [HttpGet("agent-commission-summary")]
    public async Task<ActionResult<List<AgentCommissionSummaryDto>>> GetAgentCommissionSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var summaries = await _billingService.GetAgentCommissionSummaryAsync(fromDate, toDate);

            return Ok(summaries.Select(s => new AgentCommissionSummaryDto
            {
                AgentId = s.AgentId,
                AgentName = s.AgentName,
                AgentCode = s.AgentCode,
                TotalShipments = s.TotalShipments,
                TotalFreight = s.TotalFreight,
                CommissionRate = s.CommissionRate,
                CommissionType = s.CommissionType.ToString(),
                TotalCommission = s.TotalCommission
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching agent commission summary");
            return StatusCode(500, "An error occurred while fetching commission summary.");
        }
    }

    [HttpGet("report")]
    public async Task<ActionResult<BillingReportDto>> GetBillingReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var report = await _billingService.GetBillingReportAsync(fromDate, toDate);

            return Ok(new BillingReportDto
            {
                FromDate = report.FromDate,
                ToDate = report.ToDate,
                TotalShipments = report.TotalShipments,
                TotalWeight = report.TotalWeight,
                TotalFreight = report.TotalFreight,
                TotalFuelSurcharge = report.TotalFuelSurcharge,
                TotalInsurance = report.TotalInsurance,
                TotalCODCharges = report.TotalCODCharges,
                TotalTax = report.TotalTax,
                TotalRevenue = report.TotalRevenue,
                BilledShipments = report.BilledShipments,
                UnbilledShipments = report.UnbilledShipments,
                BilledAmount = report.BilledAmount,
                UnbilledAmount = report.UnbilledAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing report");
            return StatusCode(500, "An error occurred while generating billing report.");
        }
    }
}

public class CalculateFreightRequest
{
    public Guid ServiceTypeId { get; set; }
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public decimal Weight { get; set; }
    public decimal DeclaredValue { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
}

public class CalculateFreightBatchRequest
{
    public List<FreightCalculationItem> Items { get; set; } = new();
}

public class FreightCalculationItem
{
    public string? ReferenceId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public decimal Weight { get; set; }
    public decimal DeclaredValue { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
}

public class FreightCalculationResultDto
{
    public decimal Weight { get; set; }
    public decimal DeclaredValue { get; set; }
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
}

public class BatchFreightCalculationResultDto
{
    public string? ReferenceId { get; set; }
    public bool Success { get; set; }
    public decimal Weight { get; set; }
    public decimal TotalCharge { get; set; }
    public decimal FreightCharge { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal InsuranceCharge { get; set; }
    public decimal CODCharge { get; set; }
    public decimal TaxAmount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CustomerBillingSummaryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalFreight { get; set; }
    public DateTime OldestShipmentDate { get; set; }
}

public class UnbilledShipmentDto
{
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? OriginCity { get; set; }
    public string? DestinationCity { get; set; }
    public int Pieces { get; set; }
    public decimal ChargeableWeight { get; set; }
    public decimal FreightCharge { get; set; }
    public decimal TotalCharge { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
}

public class GenerateInvoicesRequest
{
    public Guid CustomerId { get; set; }
    public List<Guid> ShipmentIds { get; set; } = new();
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
}

public class InvoiceGenerationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
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
}

public class AgentCommissionSummaryDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentCode { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalFreight { get; set; }
    public decimal CommissionRate { get; set; }
    public string CommissionType { get; set; } = string.Empty;
    public decimal TotalCommission { get; set; }
}

public class BillingReportDto
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
}
