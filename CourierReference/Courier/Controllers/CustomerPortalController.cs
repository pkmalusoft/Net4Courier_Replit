using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Core.MultiTenancy;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/customer-portal")]
[Authorize]
public class CustomerPortalController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CustomerPortalController> _logger;

    public CustomerPortalController(
        AppDbContext context,
        ITenantProvider tenantProvider,
        ILogger<CustomerPortalController> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<CustomerPortalDashboardResponse>> GetDashboard()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return Unauthorized("No tenant context");

        var customerName = User.Identity?.Name ?? "Customer";
        var customerUsername = User.Identity?.Name;
        
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeStatuses = new[] { 
            ShipmentStatus.Draft, 
            ShipmentStatus.Booked, 
            ShipmentStatus.PickedUp, 
            ShipmentStatus.InTransit, 
            ShipmentStatus.OutForDelivery 
        };

        var activeShipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value 
                && s.CreatedBy == customerUsername 
                && activeStatuses.Contains(s.Status))
            .CountAsync();

        var deliveredThisMonth = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value 
                && s.CreatedBy == customerUsername
                && s.Status == ShipmentStatus.Delivered 
                && s.ActualDeliveryDate >= startOfMonth)
            .CountAsync();

        var pendingPayments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value 
                && s.CreatedBy == customerUsername
                && !s.IsBilled 
                && s.Status != ShipmentStatus.Draft
                && s.Status != ShipmentStatus.Cancelled)
            .CountAsync();

        var outstandingBalance = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value 
                && s.CreatedBy == customerUsername
                && !s.IsBilled 
                && s.Status != ShipmentStatus.Draft
                && s.Status != ShipmentStatus.Cancelled)
            .SumAsync(s => s.TotalCharge);

        var recentShipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value && s.CreatedBy == customerUsername)
            .OrderByDescending(s => s.BookingDate)
            .Take(5)
            .Select(s => new RecentShipmentDto
            {
                Id = s.Id,
                AWBNumber = s.AWBNumber,
                SenderCity = s.SenderCity,
                ReceiverCity = s.ReceiverCity,
                Status = s.Status.ToString(),
                BookingDate = s.BookingDate
            })
            .ToListAsync();

        return Ok(new CustomerPortalDashboardResponse
        {
            CustomerName = customerName,
            Dashboard = new CustomerDashboardData
            {
                ActiveShipments = activeShipments,
                DeliveredThisMonth = deliveredThisMonth,
                PendingPayments = pendingPayments,
                OutstandingBalance = outstandingBalance
            },
            RecentShipments = recentShipments
        });
    }

    [HttpGet("shipments")]
    public async Task<ActionResult<List<CustomerShipmentDto>>> GetMyShipments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return Unauthorized("No tenant context");

        var customerUsername = User.Identity?.Name;

        var query = _context.Shipments
            .Where(s => s.TenantId == tenantId.Value && s.CreatedBy == customerUsername);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ShipmentStatus>(status, out var statusEnum))
        {
            query = query.Where(s => s.Status == statusEnum);
        }

        var shipments = await query
            .OrderByDescending(s => s.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new CustomerShipmentDto
            {
                Id = s.Id,
                AWBNumber = s.AWBNumber,
                BookingDate = s.BookingDate,
                SenderName = s.SenderName,
                SenderCity = s.SenderCity,
                ReceiverName = s.ReceiverName,
                ReceiverCity = s.ReceiverCity,
                Status = s.Status.ToString(),
                TotalCharge = s.TotalCharge,
                IsBilled = s.IsBilled
            })
            .ToListAsync();

        return Ok(shipments);
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<List<CustomerInvoiceDto>>> GetMyInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return Unauthorized("No tenant context");

        var customerUsername = User.Identity?.Name;

        var invoices = await _context.SalesInvoices
            .Where(i => i.TenantId == tenantId.Value && i.CreatedBy == customerUsername)
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new CustomerInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                BalanceAmount = i.BalanceAmount,
                Status = i.Status.ToString()
            })
            .ToListAsync();

        return Ok(invoices);
    }
}

public class CustomerPortalDashboardResponse
{
    public string? CustomerName { get; set; }
    public CustomerDashboardData? Dashboard { get; set; }
    public List<RecentShipmentDto>? RecentShipments { get; set; }
}

public class CustomerDashboardData
{
    public int ActiveShipments { get; set; }
    public int DeliveredThisMonth { get; set; }
    public int PendingPayments { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public class RecentShipmentDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public string? SenderCity { get; set; }
    public string? ReceiverCity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
}

public class CustomerShipmentDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderCity { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverCity { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
    public bool IsBilled { get; set; }
}

public class CustomerInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
