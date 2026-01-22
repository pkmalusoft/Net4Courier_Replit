using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/quick-booking")]
[Authorize]
public class QuickBookingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IZoneRateService _zoneRateService;
    private readonly IShipmentService _shipmentService;

    public QuickBookingController(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IZoneRateService zoneRateService,
        IShipmentService shipmentService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _zoneRateService = zoneRateService;
        _shipmentService = shipmentService;
    }

    [HttpPost("rates")]
    public async Task<ActionResult<List<RateResultDto>>> GetRates([FromBody] RateRequestDto request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return Unauthorized("No tenant context");

        var serviceTypes = await _context.CourierServiceTypes
            .Where(s => s.TenantId == tenantId.Value && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();

        var results = new List<RateResultDto>();

        foreach (var serviceType in serviceTypes)
        {
            var freight = await _zoneRateService.CalculateFreightAsync(
                request.DestinationZoneId,
                serviceType.Id,
                request.Weight,
                request.DeclaredValue,
                false);

            if (freight > 0)
            {
                var rate = await _zoneRateService.GetRateAsync(
                    request.DestinationZoneId,
                    serviceType.Id,
                    request.Weight);

                var baseFreight = freight;
                var fuelSurcharge = 0m;
                
                if (rate != null && rate.FuelSurchargePercent > 0)
                {
                    var rawFreight = freight / (1 + rate.FuelSurchargePercent / 100);
                    fuelSurcharge = freight - rawFreight;
                    baseFreight = rawFreight;
                }

                results.Add(new RateResultDto
                {
                    ServiceTypeId = serviceType.Id,
                    ServiceTypeName = serviceType.Name,
                    ServiceTypeCode = serviceType.Code,
                    IsExpress = serviceType.IsExpress,
                    TransitDays = serviceType.DeliveryDays,
                    BaseFreight = Math.Round(baseFreight, 2),
                    FuelSurcharge = Math.Round(fuelSurcharge, 2),
                    TotalPrice = freight,
                    Currency = "AED"
                });
            }
        }

        return Ok(results.OrderBy(r => r.TransitDays).ThenBy(r => r.TotalPrice).ToList());
    }

    [HttpPost("book")]
    public async Task<ActionResult<QuickBookingResultDto>> BookShipment([FromBody] QuickBookingRequestDto request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return Unauthorized("No tenant context");

        var originZone = await _context.CourierZones
            .FirstOrDefaultAsync(z => z.Id == request.OriginZoneId && z.TenantId == tenantId.Value);
        
        var destinationZone = await _context.CourierZones
            .FirstOrDefaultAsync(z => z.Id == request.DestinationZoneId && z.TenantId == tenantId.Value);

        if (originZone == null || destinationZone == null)
            return BadRequest("Invalid origin or destination zone");

        var serviceType = await _context.CourierServiceTypes
            .FirstOrDefaultAsync(s => s.Id == request.CourierServiceTypeId && s.TenantId == tenantId.Value);

        if (serviceType == null)
            return BadRequest("Invalid service type");

        var awbNumber = await GenerateAwbNumber(tenantId.Value);

        var volumetricWeight = (request.Length * request.Width * request.Height) / 5000m;
        var chargeableWeight = Math.Max(request.Weight, volumetricWeight);
        
        var shipment = new Shipment
        {
            TenantId = tenantId.Value,
            AWBNumber = awbNumber,
            BookingDate = DateTime.UtcNow,
            CourierServiceTypeId = request.CourierServiceTypeId,
            OriginZoneId = request.OriginZoneId,
            DestinationZoneId = request.DestinationZoneId,
            SenderName = "Quick Booking",
            SenderCity = originZone.City ?? originZone.ZoneName,
            SenderCountry = originZone.Country,
            ReceiverName = "Quick Booking",
            ReceiverCity = destinationZone.City ?? destinationZone.ZoneName,
            ReceiverCountry = destinationZone.Country,
            ContentType = Enum.TryParse<ContentType>(request.PackageType, out var ct) ? ct : ContentType.Parcel,
            NumberOfPieces = 1,
            ActualWeight = request.Weight,
            VolumetricWeight = volumetricWeight,
            ChargeableWeight = chargeableWeight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            PaymentMode = PaymentMode.Prepaid,
            Status = ShipmentStatus.Draft,
            FreightCharge = request.TotalPrice,
            TotalCharge = request.TotalPrice,
            NetAmount = request.TotalPrice,
            SpecialInstructions = request.IsResidential ? "Residential delivery" : null,
            CreatedBy = User.Identity?.Name ?? "System",
            UpdatedBy = User.Identity?.Name ?? "System"
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        return Ok(new QuickBookingResultDto
        {
            ShipmentId = shipment.Id,
            AwbNumber = shipment.AWBNumber
        });
    }

    private async Task<string> GenerateAwbNumber(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"AWB{today:yyMMdd}";
        
        var lastShipment = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.AWBNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AWBNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastShipment != null)
        {
            var lastSeq = lastShipment.AWBNumber.Replace(prefix, "");
            if (int.TryParse(lastSeq, out var parsed))
                sequence = parsed + 1;
        }

        return $"{prefix}{sequence:D4}";
    }
}

public class RateRequestDto
{
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public decimal Weight { get; set; }
    public string PackageType { get; set; } = "Parcel";
    public bool IsResidential { get; set; }
    public decimal DeclaredValue { get; set; }
}

public class RateResultDto
{
    public Guid ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceTypeCode { get; set; } = string.Empty;
    public bool IsExpress { get; set; }
    public int TransitDays { get; set; }
    public decimal BaseFreight { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "AED";
}

public class QuickBookingRequestDto
{
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public Guid CourierServiceTypeId { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string PackageType { get; set; } = "Parcel";
    public bool IsResidential { get; set; }
    public decimal TotalPrice { get; set; }
}

public class QuickBookingResultDto
{
    public Guid ShipmentId { get; set; }
    public string AwbNumber { get; set; } = string.Empty;
}
