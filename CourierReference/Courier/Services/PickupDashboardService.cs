using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IPickupDashboardService
{
    Task<PickupDashboardSummaryDto> GetSummaryAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null);
    Task<List<CustomerPickupGroupDto>> GetPickupsByCustomerAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null);
    Task<List<PickupAwbDto>> GetAwbsByPickupRequestAsync(Guid pickupRequestId);
    Task<List<PickupAwbDto>> GetAwbsByCustomerAsync(Guid customerId, DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null);
    Task<List<CourierPerformanceDto>> GetCourierPerformanceAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null);
    Task<bool> AssignShipmentsToAgentAsync(List<Guid> shipmentIds, Guid agentId);
    Task<bool> AssignPickupRequestToAgentAsync(Guid pickupRequestId, Guid agentId);
}

public enum DateRangeType
{
    Today,
    Week,
    Month,
    Year,
    Custom
}

public class PickupDashboardService : IPickupDashboardService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public PickupDashboardService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    private Guid GetTenantId() => _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");

    private (DateTime from, DateTime to) GetDateRange(DateRangeType rangeType, DateTime? customFromDate, DateTime? customToDate)
    {
        var now = DateTime.UtcNow;
        return rangeType switch
        {
            DateRangeType.Today => (DateTime.SpecifyKind(now.Date, DateTimeKind.Utc), DateTime.SpecifyKind(now.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),
            DateRangeType.Week => (DateTime.SpecifyKind(now.Date.AddDays(-(int)now.DayOfWeek), DateTimeKind.Utc), DateTime.SpecifyKind(now.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),
            DateRangeType.Month => (DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc), DateTime.SpecifyKind(now.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),
            DateRangeType.Year => (DateTime.SpecifyKind(new DateTime(now.Year, 1, 1), DateTimeKind.Utc), DateTime.SpecifyKind(now.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),
            DateRangeType.Custom when customFromDate.HasValue && customToDate.HasValue => 
                (DateTime.SpecifyKind(customFromDate.Value.Date, DateTimeKind.Utc), DateTime.SpecifyKind(customToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),
            _ => (DateTime.SpecifyKind(now.Date, DateTimeKind.Utc), DateTime.SpecifyKind(now.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc))
        };
    }

    public async Task<PickupDashboardSummaryDto> GetSummaryAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null)
    {
        var tenantId = GetTenantId();
        var (fromDate, toDate) = GetDateRange(rangeType, customFromDate, customToDate);

        var query = _context.PickupRequests
            .Where(p => p.TenantId == tenantId && p.ScheduledDate >= fromDate && p.ScheduledDate <= toDate && !p.IsVoided);

        var totalRequests = await query.CountAsync();
        var requestedCount = await query.CountAsync(p => p.Status == PickupStatus.Requested || p.Status == PickupStatus.Confirmed);
        var assignedCount = await query.CountAsync(p => p.Status == PickupStatus.Assigned || p.Status == PickupStatus.InProgress);
        var collectedCount = await query.CountAsync(p => p.Status == PickupStatus.Completed);
        var cancelledCount = await query.CountAsync(p => p.Status == PickupStatus.Cancelled);
        var failedCount = await query.CountAsync(p => p.Status == PickupStatus.Failed);

        var customerCount = await query
            .Where(p => p.CustomerId.HasValue)
            .Select(p => p.CustomerId)
            .Distinct()
            .CountAsync();

        var pendingAssignmentCount = await query.CountAsync(p => 
            (p.Status == PickupStatus.Requested || p.Status == PickupStatus.Confirmed) && 
            !p.AssignedAgentId.HasValue);

        return new PickupDashboardSummaryDto
        {
            TotalRequests = totalRequests,
            RequestedCount = requestedCount,
            AssignedCount = assignedCount,
            CollectedCount = collectedCount,
            CancelledCount = cancelledCount,
            FailedCount = failedCount,
            CustomerCount = customerCount,
            PendingAssignmentCount = pendingAssignmentCount,
            FromDate = fromDate,
            ToDate = toDate,
            RangeType = rangeType.ToString()
        };
    }

    public async Task<List<CustomerPickupGroupDto>> GetPickupsByCustomerAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null)
    {
        var tenantId = GetTenantId();
        var (fromDate, toDate) = GetDateRange(rangeType, customFromDate, customToDate);

        var pickups = await _context.PickupRequests
            .Include(p => p.Customer)
            .Include(p => p.AssignedAgent)
            .Include(p => p.Shipments)
            .Where(p => p.TenantId == tenantId && p.ScheduledDate >= fromDate && p.ScheduledDate <= toDate && !p.IsVoided)
            .OrderByDescending(p => p.ScheduledDate)
            .ToListAsync();

        var grouped = pickups
            .GroupBy(p => new { p.CustomerId, CustomerName = p.Customer?.Name ?? p.ContactName })
            .Select(g => new CustomerPickupGroupDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                TotalRequests = g.Count(),
                RequestedCount = g.Count(p => p.Status == PickupStatus.Requested || p.Status == PickupStatus.Confirmed),
                AssignedCount = g.Count(p => p.Status == PickupStatus.Assigned || p.Status == PickupStatus.InProgress),
                CollectedCount = g.Count(p => p.Status == PickupStatus.Completed),
                TotalAwbs = g.Sum(p => p.Shipments.Count),
                PickupRequests = g.Select(p => new PickupRequestSummaryDto
                {
                    Id = p.Id,
                    RequestNumber = p.RequestNumber,
                    RequestDate = p.RequestDate,
                    ScheduledDate = p.ScheduledDate,
                    Status = p.Status.ToString(),
                    ContactName = p.ContactName,
                    ContactPhone = p.ContactPhone,
                    PickupAddress = p.PickupAddress,
                    City = p.City,
                    ExpectedPieces = p.ExpectedPieces,
                    ExpectedWeight = p.ExpectedWeight,
                    ActualPieces = p.ActualPieces,
                    ActualWeight = p.ActualWeight,
                    AssignedAgentId = p.AssignedAgentId,
                    AssignedAgentName = p.AssignedAgent?.Name,
                    AwbCount = p.Shipments.Count,
                    SpecialInstructions = p.SpecialInstructions
                }).OrderByDescending(p => p.ScheduledDate).ToList()
            })
            .OrderByDescending(g => g.TotalRequests)
            .ToList();

        return grouped;
    }

    public async Task<List<PickupAwbDto>> GetAwbsByPickupRequestAsync(Guid pickupRequestId)
    {
        var tenantId = GetTenantId();

        var pickupRequest = await _context.PickupRequests
            .Include(p => p.Shipments)
                .ThenInclude(s => s.CourierServiceType)
            .Include(p => p.Shipments)
                .ThenInclude(s => s.AssignedAgent)
            .FirstOrDefaultAsync(p => p.Id == pickupRequestId && p.TenantId == tenantId);

        if (pickupRequest == null)
            return new List<PickupAwbDto>();

        return pickupRequest.Shipments.Select(s => new PickupAwbDto
        {
            Id = s.Id,
            AWBNumber = s.AWBNumber,
            BookingDate = s.BookingDate,
            SenderName = s.SenderName,
            ReceiverName = s.ReceiverName,
            ReceiverCity = s.ReceiverCity,
            ReceiverCountry = s.ReceiverCountry,
            ServiceType = s.CourierServiceType?.Name,
            Status = s.Status.ToString(),
            Pieces = s.NumberOfPieces,
            Weight = s.ActualWeight,
            AssignedAgentId = s.AssignedAgentId,
            AssignedAgentName = s.AssignedAgent?.Name,
            PaymentMode = s.PaymentMode.ToString(),
            TotalCharge = s.TotalCharge
        }).OrderByDescending(s => s.BookingDate).ToList();
    }

    public async Task<List<PickupAwbDto>> GetAwbsByCustomerAsync(Guid customerId, DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null)
    {
        var tenantId = GetTenantId();
        var (fromDate, toDate) = GetDateRange(rangeType, customFromDate, customToDate);

        var pickupRequests = await _context.PickupRequests
            .Include(p => p.Shipments)
                .ThenInclude(s => s.CourierServiceType)
            .Include(p => p.Shipments)
                .ThenInclude(s => s.AssignedAgent)
            .Where(p => p.TenantId == tenantId && p.CustomerId == customerId && 
                        p.ScheduledDate >= fromDate && p.ScheduledDate <= toDate && !p.IsVoided)
            .ToListAsync();

        var allShipments = pickupRequests.SelectMany(p => p.Shipments);

        return allShipments.Select(s => new PickupAwbDto
        {
            Id = s.Id,
            AWBNumber = s.AWBNumber,
            BookingDate = s.BookingDate,
            SenderName = s.SenderName,
            ReceiverName = s.ReceiverName,
            ReceiverCity = s.ReceiverCity,
            ReceiverCountry = s.ReceiverCountry,
            ServiceType = s.CourierServiceType?.Name,
            Status = s.Status.ToString(),
            Pieces = s.NumberOfPieces,
            Weight = s.ActualWeight,
            AssignedAgentId = s.AssignedAgentId,
            AssignedAgentName = s.AssignedAgent?.Name,
            PaymentMode = s.PaymentMode.ToString(),
            TotalCharge = s.TotalCharge
        }).OrderByDescending(s => s.BookingDate).ToList();
    }

    public async Task<List<CourierPerformanceDto>> GetCourierPerformanceAsync(DateRangeType rangeType, DateTime? customFromDate = null, DateTime? customToDate = null)
    {
        var tenantId = GetTenantId();
        var (fromDate, toDate) = GetDateRange(rangeType, customFromDate, customToDate);

        var agents = await _context.CourierAgents
            .Where(a => a.TenantId == tenantId && a.IsActive && 
                       (a.AgentType == AgentType.PickupAgent || a.AgentType == AgentType.DeliveryAgent))
            .ToListAsync();

        var pickupRequests = await _context.PickupRequests
            .Where(p => p.TenantId == tenantId && p.ScheduledDate >= fromDate && p.ScheduledDate <= toDate && 
                        !p.IsVoided && p.AssignedAgentId.HasValue)
            .ToListAsync();

        var result = new List<CourierPerformanceDto>();

        foreach (var agent in agents)
        {
            var agentPickups = pickupRequests.Where(p => p.AssignedAgentId == agent.Id).ToList();
            
            var totalAssigned = agentPickups.Count;
            var collected = agentPickups.Count(p => p.Status == PickupStatus.Completed);
            
            result.Add(new CourierPerformanceDto
            {
                AgentId = agent.Id,
                AgentCode = agent.AgentCode,
                AgentName = agent.Name,
                AgentType = agent.AgentType.ToString(),
                TotalAssigned = totalAssigned,
                Collected = collected,
                Pending = agentPickups.Count(p => p.Status == PickupStatus.Assigned || p.Status == PickupStatus.InProgress),
                Failed = agentPickups.Count(p => p.Status == PickupStatus.Failed),
                TotalPieces = agentPickups.Sum(p => p.ActualPieces),
                TotalWeight = agentPickups.Sum(p => p.ActualWeight ?? 0),
                CollectionRate = totalAssigned > 0 
                    ? Math.Round((double)collected / totalAssigned * 100, 1) 
                    : 0
            });
        }

        return result.OrderByDescending(a => a.TotalAssigned).ToList();
    }

    public async Task<bool> AssignShipmentsToAgentAsync(List<Guid> shipmentIds, Guid agentId)
    {
        var tenantId = GetTenantId();

        var agent = await _context.CourierAgents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.TenantId == tenantId && a.IsActive);

        if (agent == null)
            return false;

        var shipments = await _context.Shipments
            .Where(s => shipmentIds.Contains(s.Id) && s.TenantId == tenantId)
            .ToListAsync();

        foreach (var shipment in shipments)
        {
            shipment.AssignedAgentId = agentId;
            if (shipment.Status == ShipmentStatus.Booked)
            {
                shipment.Status = ShipmentStatus.PickedUp;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignPickupRequestToAgentAsync(Guid pickupRequestId, Guid agentId)
    {
        var tenantId = GetTenantId();

        var agent = await _context.CourierAgents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.TenantId == tenantId && a.IsActive);

        if (agent == null)
            return false;

        var pickupRequest = await _context.PickupRequests
            .FirstOrDefaultAsync(p => p.Id == pickupRequestId && p.TenantId == tenantId);

        if (pickupRequest == null)
            return false;

        pickupRequest.AssignedAgentId = agentId;
        if (pickupRequest.Status == PickupStatus.Requested || pickupRequest.Status == PickupStatus.Confirmed)
        {
            pickupRequest.Status = PickupStatus.Assigned;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

public class PickupDashboardSummaryDto
{
    public int TotalRequests { get; set; }
    public int RequestedCount { get; set; }
    public int AssignedCount { get; set; }
    public int CollectedCount { get; set; }
    public int CancelledCount { get; set; }
    public int FailedCount { get; set; }
    public int CustomerCount { get; set; }
    public int PendingAssignmentCount { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string RangeType { get; set; } = string.Empty;
}

public class CustomerPickupGroupDto
{
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int RequestedCount { get; set; }
    public int AssignedCount { get; set; }
    public int CollectedCount { get; set; }
    public int TotalAwbs { get; set; }
    public List<PickupRequestSummaryDto> PickupRequests { get; set; } = new();
}

public class PickupRequestSummaryDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public int ExpectedPieces { get; set; }
    public decimal? ExpectedWeight { get; set; }
    public int ActualPieces { get; set; }
    public decimal? ActualWeight { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public int AwbCount { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class PickupAwbDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverCity { get; set; }
    public string? ReceiverCountry { get; set; }
    public string? ServiceType { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Pieces { get; set; }
    public decimal Weight { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
}

public class CourierPerformanceDto
{
    public Guid AgentId { get; set; }
    public string AgentCode { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Collected { get; set; }
    public int Pending { get; set; }
    public int Failed { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalWeight { get; set; }
    public double CollectionRate { get; set; }
}

public class AssignShipmentsRequest
{
    public List<Guid> ShipmentIds { get; set; } = new();
    public Guid AgentId { get; set; }
}

public class AssignPickupRequest
{
    public Guid PickupRequestId { get; set; }
    public Guid AgentId { get; set; }
}
