using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CourierDashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CourierDashboardController> _logger;

    public CourierDashboardController(AppDbContext context, ITenantProvider tenantProvider, ILogger<CourierDashboardController> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<CourierDashboardDto>> GetDashboard(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
        
            var fromDateUtc = fromDate.HasValue 
                ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc) 
                : DateTime.UtcNow.Date;
            var toDateUtc = toDate.HasValue 
                ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) 
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var shipmentsQuery = _context.Shipments
                .Where(s => s.TenantId == tenantId && s.BookingDate >= fromDateUtc && s.BookingDate <= toDateUtc);

            var totalShipments = await shipmentsQuery.CountAsync();
        var deliveredCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.Delivered);
        var inTransitCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.InTransit);
        var outForDeliveryCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.OutForDelivery);
        var rtoCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.ReturnedToOrigin);
        var pickedUpCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.PickedUp);
        var bookedCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.Booked);
        var inScanCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.InScan);

        var codShipments = await shipmentsQuery
            .Where(s => s.PaymentMode == PaymentMode.COD && s.Status == ShipmentStatus.Delivered)
            .SumAsync(s => (decimal?)s.CODAmount) ?? 0m;

        var totalRevenue = await shipmentsQuery.SumAsync(s => (decimal?)s.TotalCharge) ?? 0m;

        var deliveryRate = totalShipments > 0 ? (double)deliveredCount / totalShipments * 100 : 0;
        var rtoRate = totalShipments > 0 ? (double)rtoCount / totalShipments * 100 : 0;

        var hubCount = await _context.Branches
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .CountAsync();

        var drsQuery = _context.DeliveryRunSheets
            .Where(d => d.TenantId == tenantId && d.DRSDate >= fromDateUtc && d.DRSDate <= toDateUtc);
        var drsCount = await drsQuery.CountAsync();

        var agentsQuery = _context.CourierAgents.Where(a => a.TenantId == tenantId);
        var totalAgents = await agentsQuery.CountAsync();
        var activeAgents = await agentsQuery.CountAsync(a => a.IsActive);

        var codPendingCount = await _context.CODCollections
            .Where(c => c.TenantId == tenantId && c.RemittedDate == null && c.CollectionDate >= fromDateUtc && c.CollectionDate <= toDateUtc)
            .CountAsync();

        var pendingPickupCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.Booked);
        var pendingInscanCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.PickedUp);
        var unassignedDrsCount = await shipmentsQuery.CountAsync(s => s.Status == ShipmentStatus.InScan && s.DeliveryRunSheetId == null);
        
        var pendingManifestCount = await _context.Manifests
            .Where(m => m.TenantId == tenantId && m.Status == ManifestStatus.Open && m.ManifestDate >= fromDateUtc && m.ManifestDate <= toDateUtc)
            .CountAsync();

        var statusDistribution = new List<StatusDistributionItem>
        {
            new() { Status = "Delivered", Count = deliveredCount },
            new() { Status = "In Transit", Count = inTransitCount },
            new() { Status = "Out for Delivery", Count = outForDeliveryCount },
            new() { Status = "Picked Up", Count = pickedUpCount },
            new() { Status = "Booked", Count = bookedCount },
            new() { Status = "RTO", Count = rtoCount }
        };

        List<ZoneDistributionItem> zoneDistribution = new();
        try
        {
            var shipmentsWithZones = await shipmentsQuery
                .Where(s => s.DestinationZoneId.HasValue)
                .Include(s => s.DestinationZone)
                .ToListAsync();
            
            zoneDistribution = shipmentsWithZones
                .GroupBy(s => s.DestinationZone != null ? s.DestinationZone.ZoneName : "Unknown")
                .Select(g => new ZoneDistributionItem { ZoneName = g.Key ?? "Unknown", Count = g.Count() })
                .OrderByDescending(z => z.Count)
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading zone distribution");
        }

        List<ServiceTypeDistributionItem> serviceTypeDistribution = new();
        try
        {
            var shipmentsWithServiceTypes = await shipmentsQuery
                .Where(s => s.CourierServiceTypeId != Guid.Empty)
                .Include(s => s.CourierServiceType)
                .ToListAsync();
            
            serviceTypeDistribution = shipmentsWithServiceTypes
                .GroupBy(s => s.CourierServiceType != null ? s.CourierServiceType.Name : "Unknown")
                .Select(g => new ServiceTypeDistributionItem { ServiceType = g.Key ?? "Unknown", Count = g.Count() })
                .OrderByDescending(s => s.Count)
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading service type distribution");
        }

        ShipmentTrendData trendData = new();
        try
        {
            trendData = await GetShipmentTrendAsync(tenantId, fromDateUtc, toDateUtc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading trend data");
        }

        List<RecentShipmentItem> recentShipments = new();
        try
        {
            var recentShipmentsData = await shipmentsQuery
                .OrderByDescending(s => s.BookingDate)
                .Take(10)
                .ToListAsync();
            
            if (recentShipmentsData.Any())
            {
                var serviceTypeIds = recentShipmentsData
                    .Where(s => s.CourierServiceTypeId != Guid.Empty)
                    .Select(s => s.CourierServiceTypeId)
                    .Distinct()
                    .ToList();
                
                var serviceTypes = serviceTypeIds.Any()
                    ? await _context.CourierServiceTypes
                        .Where(st => serviceTypeIds.Contains(st.Id))
                        .ToDictionaryAsync(st => st.Id, st => st.Name)
                    : new Dictionary<Guid, string>();
                
                recentShipments = recentShipmentsData
                    .Select(s => new RecentShipmentItem
                    {
                        AWBNumber = s.AWBNumber,
                        SenderName = s.SenderName,
                        ReceiverName = s.ReceiverName,
                        DestinationCity = s.ReceiverCity ?? "",
                        ServiceType = serviceTypes.TryGetValue(s.CourierServiceTypeId, out var name) ? name : "Unknown",
                        Status = s.Status.ToString(),
                        BookingDate = s.BookingDate
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading recent shipments");
        }

        List<TopAgentItem> topAgents = new();
        try
        {
            var shipmentsWithCustomers = await shipmentsQuery
                .Where(s => s.CustomerId.HasValue)
                .Include(s => s.Customer)
                .ToListAsync();
            
            topAgents = shipmentsWithCustomers
                .GroupBy(s => new { s.CustomerId, CustomerName = s.Customer != null ? s.Customer.Name : "Unknown" })
                .Select(g => new TopAgentItem
                {
                    Name = g.Key.CustomerName,
                    ShipmentCount = g.Count(),
                    Revenue = g.Sum(s => s.TotalCharge)
                })
                .OrderByDescending(a => a.ShipmentCount)
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading top agents");
        }

        return Ok(new CourierDashboardDto
        {
            TotalShipments = totalShipments,
            DeliveredCount = deliveredCount,
            InTransitCount = inTransitCount,
            OutForDeliveryCount = outForDeliveryCount,
            RtoCount = rtoCount,
            HubCount = hubCount,
            DrsCount = drsCount,
            ActiveAgents = activeAgents,
            TotalAgents = totalAgents,
            CodCollected = codShipments,
            TotalRevenue = totalRevenue,
            CodPendingCount = codPendingCount,
            DeliveryRate = deliveryRate,
            RtoRate = rtoRate,
            PendingPickupCount = pendingPickupCount,
            PendingInscanCount = pendingInscanCount,
            UnassignedDrsCount = unassignedDrsCount,
            PendingManifestCount = pendingManifestCount,
            StatusDistribution = statusDistribution,
            ZoneDistribution = zoneDistribution,
            ServiceTypeDistribution = serviceTypeDistribution,
            TrendData = trendData,
            RecentShipments = recentShipments,
            TopAgents = topAgents
        });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courier dashboard data for tenant {TenantId}", _tenantProvider.CurrentTenantId);
            return StatusCode(500, new { 
                error = "An error occurred while loading dashboard data", 
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace 
            });
        }
    }

    private async Task<ShipmentTrendData> GetShipmentTrendAsync(Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var days = (toDate - fromDate).Days + 1;
        var labels = new List<string>();
        var bookedData = new List<int>();
        var deliveredData = new List<int>();

        var bookedByDate = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.BookingDate >= fromDate && s.BookingDate <= toDate)
            .GroupBy(s => s.BookingDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var deliveredByDate = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.ActualDeliveryDate >= fromDate && s.ActualDeliveryDate <= toDate)
            .GroupBy(s => s.ActualDeliveryDate!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        if (days <= 7)
        {
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                labels.Add(date.ToString("ddd"));
                bookedData.Add(bookedByDate.GetValueOrDefault(date, 0));
                deliveredData.Add(deliveredByDate.GetValueOrDefault(date, 0));
            }
        }
        else
        {
            var weeks = (days + 6) / 7;
            for (var i = 0; i < Math.Min(weeks, 8); i++)
            {
                var weekStart = fromDate.Date.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(6);
                if (weekEnd > toDate.Date) weekEnd = toDate.Date;
                
                labels.Add($"W{i + 1}");
                
                var weekBooked = 0;
                var weekDelivered = 0;
                for (var d = weekStart; d <= weekEnd; d = d.AddDays(1))
                {
                    weekBooked += bookedByDate.GetValueOrDefault(d, 0);
                    weekDelivered += deliveredByDate.GetValueOrDefault(d, 0);
                }
                
                bookedData.Add(weekBooked);
                deliveredData.Add(weekDelivered);
            }
        }

        return new ShipmentTrendData
        {
            Labels = labels,
            BookedData = bookedData,
            DeliveredData = deliveredData
        };
    }
}

public class CourierDashboardDto
{
    public int TotalShipments { get; set; }
    public int DeliveredCount { get; set; }
    public int InTransitCount { get; set; }
    public int OutForDeliveryCount { get; set; }
    public int RtoCount { get; set; }
    public int HubCount { get; set; }
    public int DrsCount { get; set; }
    public int ActiveAgents { get; set; }
    public int TotalAgents { get; set; }
    public decimal CodCollected { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CodPendingCount { get; set; }
    public double DeliveryRate { get; set; }
    public double RtoRate { get; set; }
    public int PendingPickupCount { get; set; }
    public int PendingInscanCount { get; set; }
    public int UnassignedDrsCount { get; set; }
    public int PendingManifestCount { get; set; }
    public List<StatusDistributionItem> StatusDistribution { get; set; } = new();
    public List<ZoneDistributionItem> ZoneDistribution { get; set; } = new();
    public List<ServiceTypeDistributionItem> ServiceTypeDistribution { get; set; } = new();
    public ShipmentTrendData TrendData { get; set; } = new();
    public List<RecentShipmentItem> RecentShipments { get; set; } = new();
    public List<TopAgentItem> TopAgents { get; set; } = new();
}

public class StatusDistributionItem
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
}

public class ZoneDistributionItem
{
    public string ZoneName { get; set; } = "";
    public int Count { get; set; }
}

public class ServiceTypeDistributionItem
{
    public string ServiceType { get; set; } = "";
    public int Count { get; set; }
}

public class ShipmentTrendData
{
    public List<string> Labels { get; set; } = new();
    public List<int> BookedData { get; set; } = new();
    public List<int> DeliveredData { get; set; } = new();
}

public class RecentShipmentItem
{
    public string AWBNumber { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string ReceiverName { get; set; } = "";
    public string DestinationCity { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime BookingDate { get; set; }
}

public class TopAgentItem
{
    public string Name { get; set; } = "";
    public int ShipmentCount { get; set; }
    public decimal Revenue { get; set; }
}
