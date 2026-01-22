using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/shipments")]
[Authorize]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentService _shipmentService;
    private readonly ICourierZoneService _zoneService;
    private readonly ICourierServiceTypeService _serviceTypeService;
    private readonly ITenantProvider _tenantProvider;

    public ShipmentController(
        IShipmentService shipmentService,
        ICourierZoneService zoneService,
        ICourierServiceTypeService serviceTypeService,
        ITenantProvider tenantProvider)
    {
        _shipmentService = shipmentService;
        _zoneService = zoneService;
        _serviceTypeService = serviceTypeService;
        _tenantProvider = tenantProvider;
    }

    [HttpGet]
    public async Task<ActionResult<List<ShipmentListDto>>> GetShipments(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status)
    {
        ShipmentStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ShipmentStatus>(status, out var s))
            statusEnum = s;

        var items = await _shipmentService.GetAllAsync(fromDate, toDate, statusEnum);
        return Ok(items.Select(MapToListDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShipmentDetailDto>> GetShipment(Guid id)
    {
        var item = await _shipmentService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(MapToDetailDto(item));
    }

    [HttpGet("awb/{awbNumber}")]
    public async Task<ActionResult<ShipmentDetailDto>> GetShipmentByAWB(string awbNumber)
    {
        var item = await _shipmentService.GetByAWBAsync(awbNumber);
        if (item == null)
            return NotFound();
        return Ok(MapToDetailDto(item));
    }

    [HttpGet("track/{awbNumber}")]
    public async Task<ActionResult<ShipmentTrackingInfoDto>> TrackShipment(string awbNumber)
    {
        var item = await _shipmentService.GetByAWBForTrackingAsync(awbNumber);
        if (item == null)
            return NotFound();
        return Ok(MapToTrackingInfoDto(item));
    }

    [HttpGet("pending-deliveries")]
    public async Task<ActionResult<List<ShipmentListDto>>> GetPendingDeliveries()
    {
        var items = await _shipmentService.GetPendingDeliveriesAsync();
        return Ok(items.Select(MapToListDto));
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<ShipmentListDto>>> GetByStatus(string status)
    {
        if (!Enum.TryParse<ShipmentStatus>(status, out var statusEnum))
            return BadRequest("Invalid status");

        var items = await _shipmentService.GetByStatusAsync(statusEnum);
        return Ok(items.Select(MapToListDto));
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentDetailDto>> CreateShipment([FromBody] CreateShipmentRequest request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        var serviceType = await _serviceTypeService.GetByIdAsync(request.CourierServiceTypeId);
        if (serviceType == null)
            return BadRequest("Invalid service type.");

        var shipment = new Shipment
        {
            TenantId = tenantId.Value,
            BookingDate = request.BookingDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.BookingDate, DateTimeKind.Utc) 
                : request.BookingDate.ToUniversalTime(),
            CourierServiceTypeId = request.CourierServiceTypeId,
            CustomerId = request.CustomerId,
            SenderName = request.SenderName,
            SenderCompany = request.SenderCompany,
            SenderPhone = request.SenderPhone,
            SenderEmail = request.SenderEmail,
            SenderAddress = request.SenderAddress,
            SenderCity = request.SenderCity,
            SenderState = request.SenderState,
            SenderPostalCode = request.SenderPostalCode,
            SenderCountry = request.SenderCountry,
            ReceiverName = request.ReceiverName,
            ReceiverCompany = request.ReceiverCompany,
            ReceiverPhone = request.ReceiverPhone,
            ReceiverEmail = request.ReceiverEmail,
            ReceiverAddress = request.ReceiverAddress,
            ReceiverCity = request.ReceiverCity,
            ReceiverState = request.ReceiverState,
            ReceiverPostalCode = request.ReceiverPostalCode,
            ReceiverCountry = request.ReceiverCountry,
            OriginZoneId = request.OriginZoneId,
            DestinationZoneId = request.DestinationZoneId,
            ShipmentClassification = Enum.TryParse<ShipmentClassificationType>(request.ShipmentClassification, out var sc) ? sc : ShipmentClassificationType.ParcelUpto30kg,
            NumberOfPieces = request.Pieces,
            ActualWeight = request.ActualWeight,
            VolumetricWeight = request.VolumetricWeight,
            DeclaredValue = request.DeclaredValue,
            PaymentMode = Enum.TryParse<PaymentMode>(request.PaymentMode, out var pm) ? pm : PaymentMode.Prepaid,
            CODAmount = request.CODAmount,
            SpecialInstructions = request.SpecialInstructions,
            InternalNotes = request.InternalNotes,
            Status = ShipmentStatus.Booked,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(serviceType.DefaultTransitDays)
        };

        if (request.DestinationZoneId.HasValue)
        {
            var isCOD = shipment.PaymentMode == PaymentMode.COD;
            shipment.FreightCharge = await _shipmentService.CalculateChargesAsync(
                request.CourierServiceTypeId,
                request.OriginZoneId ?? Guid.Empty,
                request.DestinationZoneId.Value,
                request.ActualWeight,
                request.DeclaredValue,
                shipment.PaymentMode);
        }

        shipment.TotalCharge = shipment.FreightCharge + shipment.InsuranceCharge + shipment.OtherCharges;

        foreach (var itemDto in request.Items)
        {
            shipment.Items.Add(new ShipmentItem
            {
                TenantId = tenantId.Value,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                Weight = itemDto.Weight,
                Length = itemDto.Length,
                Width = itemDto.Width,
                Height = itemDto.Height,
                DeclaredValue = itemDto.DeclaredValue
            });
        }

        foreach (var chargeDto in request.Charges ?? Enumerable.Empty<CreateChargeRequest>())
        {
            shipment.Charges.Add(new ShipmentCharge
            {
                TenantId = tenantId.Value,
                ChargeTypeId = chargeDto.ChargeTypeId,
                ChargeName = chargeDto.ChargeName,
                Amount = chargeDto.Amount,
                Notes = chargeDto.Notes
            });
        }

        var created = await _shipmentService.CreateAsync(shipment);
        return CreatedAtAction(nameof(GetShipment), new { id = created.Id }, MapToDetailDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShipmentDetailDto>> UpdateShipment(Guid id, [FromBody] UpdateShipmentRequest request)
    {
        var existing = await _shipmentService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        if (existing.Status != ShipmentStatus.Booked && existing.Status != ShipmentStatus.Draft)
            return BadRequest("Only booked or draft shipments can be edited.");

        existing.BookingDate = request.BookingDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.BookingDate, DateTimeKind.Utc) 
            : request.BookingDate.ToUniversalTime();
        existing.CourierServiceTypeId = request.CourierServiceTypeId;
        existing.CustomerId = request.CustomerId;
        existing.SenderName = request.SenderName;
        existing.SenderCompany = request.SenderCompany;
        existing.SenderPhone = request.SenderPhone;
        existing.SenderEmail = request.SenderEmail;
        existing.SenderAddress = request.SenderAddress;
        existing.SenderCity = request.SenderCity;
        existing.SenderState = request.SenderState;
        existing.SenderPostalCode = request.SenderPostalCode;
        existing.SenderCountry = request.SenderCountry;
        existing.ReceiverName = request.ReceiverName;
        existing.ReceiverCompany = request.ReceiverCompany;
        existing.ReceiverPhone = request.ReceiverPhone;
        existing.ReceiverEmail = request.ReceiverEmail;
        existing.ReceiverAddress = request.ReceiverAddress;
        existing.ReceiverCity = request.ReceiverCity;
        existing.ReceiverState = request.ReceiverState;
        existing.ReceiverPostalCode = request.ReceiverPostalCode;
        existing.ReceiverCountry = request.ReceiverCountry;
        existing.OriginZoneId = request.OriginZoneId;
        existing.DestinationZoneId = request.DestinationZoneId;
        existing.ShipmentClassification = Enum.TryParse<ShipmentClassificationType>(request.ShipmentClassification, out var sc) ? sc : ShipmentClassificationType.ParcelUpto30kg;
        existing.NumberOfPieces = request.Pieces;
        existing.ActualWeight = request.ActualWeight;
        existing.VolumetricWeight = request.VolumetricWeight;
        existing.DeclaredValue = request.DeclaredValue;
        existing.PaymentMode = Enum.TryParse<PaymentMode>(request.PaymentMode, out var pm) ? pm : PaymentMode.Prepaid;
        existing.CODAmount = request.CODAmount;
        existing.SpecialInstructions = request.SpecialInstructions;
        existing.InternalNotes = request.InternalNotes;

        if (request.FreightChargeModified && request.FreightCharges != existing.FreightCharge)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uid))
            {
                return BadRequest("User identity required for freight charge modification audit");
            }

            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _shipmentService.LogFreightChargeChangeAsync(
                existing.Id,
                existing.AWBNumber,
                "FreightCharge",
                existing.FreightCharge,
                request.FreightCharges,
                request.FreightChangeReason,
                uid,
                userName,
                userEmail,
                ipAddress);

            existing.FreightCharge = request.FreightCharges;
        }
        else if (request.FreightChargeModified)
        {
            existing.FreightCharge = request.FreightCharges;
        }
        else if (request.DestinationZoneId.HasValue)
        {
            existing.FreightCharge = await _shipmentService.CalculateChargesAsync(
                request.CourierServiceTypeId,
                request.OriginZoneId ?? Guid.Empty,
                request.DestinationZoneId.Value,
                request.ActualWeight,
                request.DeclaredValue,
                existing.PaymentMode);
        }

        existing.TotalCharge = existing.FreightCharge + existing.InsuranceCharge + existing.OtherCharges;

        if (request.Charges != null)
        {
            existing.Charges.Clear();
            foreach (var chargeDto in request.Charges)
            {
                existing.Charges.Add(new ShipmentCharge
                {
                    TenantId = existing.TenantId,
                    ShipmentId = existing.Id,
                    ChargeTypeId = chargeDto.ChargeTypeId,
                    ChargeName = chargeDto.ChargeName,
                    Amount = chargeDto.Amount,
                    Notes = chargeDto.Notes
                });
            }
        }

        var updated = await _shipmentService.UpdateAsync(existing);
        return Ok(MapToDetailDto(updated));
    }

    [HttpPost("{id}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (!Enum.TryParse<ShipmentStatus>(request.Status, out var statusEnum))
            return BadRequest("Invalid status");

        var success = await _shipmentService.UpdateStatusAsync(id, statusEnum, request.Remarks, request.AgentId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/void")]
    public async Task<ActionResult> VoidShipment(Guid id, [FromBody] VoidShipmentRequest request)
    {
        var success = await _shipmentService.VoidAsync(id, request.Reason, request.UserId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/pod")]
    public async Task<ActionResult> RecordProofOfDelivery(Guid id, [FromBody] RecordPODRequest request)
    {
        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null)
            return NotFound();

        if (shipment.Status == ShipmentStatus.Delivered)
            return BadRequest("Shipment already delivered");

        shipment.ReceivedBy = request.ReceivedBy;
        shipment.Relationship = request.ReceiverRelation;
        shipment.DeliveryNotes = request.DeliveryNotes;
        shipment.ActualDeliveryDate = DateTime.UtcNow;
        shipment.DeliveryLatitude = request.Latitude;
        shipment.DeliveryLongitude = request.Longitude;

        if (!string.IsNullOrEmpty(request.SignatureBase64))
            shipment.SignatureImage = Convert.FromBase64String(request.SignatureBase64);

        if (!string.IsNullOrEmpty(request.PhotoBase64))
            shipment.PODImage = Convert.FromBase64String(request.PhotoBase64);

        if (request.CODCollected.HasValue)
            shipment.CODCollectedAmount = request.CODCollected.Value;

        await _shipmentService.UpdateAsync(shipment);
        await _shipmentService.UpdateStatusAsync(id, ShipmentStatus.Delivered, 
            $"Delivered to {request.ReceivedBy}");

        return NoContent();
    }

    [HttpPost("{id}/tracking")]
    public async Task<ActionResult<ShipmentTrackingDto>> AddTrackingEvent(Guid id, [FromBody] AddTrackingRequest request)
    {
        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null)
            return NotFound();

        if (!Enum.TryParse<ShipmentStatus>(request.Status, out var statusEnum))
            return BadRequest("Invalid status");

        var tracking = await _shipmentService.AddTrackingEventAsync(id, statusEnum, 
            request.StatusDescription, request.Location, request.AgentId);

        return Ok(new ShipmentTrackingDto
        {
            Id = tracking.Id,
            Status = tracking.Status.ToString(),
            StatusDescription = tracking.StatusDescription,
            Location = tracking.Location,
            EventDateTime = tracking.EventDateTime,
            AgentId = tracking.AgentId,
            IsPublic = tracking.IsPublic
        });
    }

    [HttpGet("{id}/tracking")]
    public async Task<ActionResult<List<ShipmentTrackingDto>>> GetTrackingHistory(Guid id)
    {
        var items = await _shipmentService.GetTrackingHistoryAsync(id);
        return Ok(items.Select(t => new ShipmentTrackingDto
        {
            Id = t.Id,
            Status = t.Status.ToString(),
            StatusDescription = t.StatusDescription,
            Location = t.Location,
            EventDateTime = t.EventDateTime,
            AgentId = t.AgentId,
            AgentName = t.Agent?.Name,
            IsPublic = t.IsPublic
        }));
    }

    [HttpPost("calculate-charges")]
    public async Task<ActionResult<CalculateChargesResponse>> CalculateCharges([FromBody] CalculateChargesRequest request)
    {
        if (!Enum.TryParse<PaymentMode>(request.PaymentMode, out var paymentMode))
            paymentMode = PaymentMode.Prepaid;

        var charges = await _shipmentService.CalculateChargesAsync(
            request.ServiceTypeId,
            request.OriginZoneId,
            request.DestinationZoneId,
            request.Weight,
            request.DeclaredValue,
            paymentMode);

        return Ok(new CalculateChargesResponse { FreightCharges = charges });
    }

    private static ShipmentListDto MapToListDto(Shipment entity) => new()
    {
        Id = entity.Id,
        AWBNumber = entity.AWBNumber,
        BookingDate = entity.BookingDate,
        ServiceTypeName = entity.CourierServiceType?.Name,
        CustomerName = entity.Customer?.Name,
        SenderName = entity.SenderName,
        SenderCity = entity.SenderCity ?? string.Empty,
        ReceiverName = entity.ReceiverName,
        ReceiverCity = entity.ReceiverCity ?? string.Empty,
        Pieces = entity.NumberOfPieces,
        ChargeableWeight = entity.ChargeableWeight,
        TotalCharges = entity.TotalCharge,
        Status = entity.Status.ToString(),
        PaymentMode = entity.PaymentMode.ToString(),
        AssignedAgentName = entity.AssignedAgent?.Name,
        ExpectedDeliveryDate = entity.ExpectedDeliveryDate
    };

    private static ShipmentTrackingInfoDto MapToTrackingInfoDto(Shipment entity)
    {
        var trackingHistory = new List<TrackingHistoryItemDto>();
        
        if (entity.PickupRequest != null)
        {
            trackingHistory.Add(new TrackingHistoryItemDto
            {
                Timestamp = entity.PickupRequest.RequestDate,
                Status = "PickupRequested",
                StatusDescription = "Pickup request created",
                Location = entity.PickupRequest.City ?? "",
                Remarks = $"Request #{entity.PickupRequest.RequestNumber}"
            });
            
            if (entity.PickupRequest.ScheduledDate > entity.PickupRequest.RequestDate)
            {
                trackingHistory.Add(new TrackingHistoryItemDto
                {
                    Timestamp = entity.PickupRequest.ScheduledDate,
                    Status = "PickupScheduled",
                    StatusDescription = "Pickup scheduled",
                    Location = entity.PickupRequest.City ?? "",
                    Remarks = null
                });
            }
            
            if (entity.PickupRequest.ActualPickupTime.HasValue)
            {
                trackingHistory.Add(new TrackingHistoryItemDto
                {
                    Timestamp = entity.PickupRequest.ActualPickupTime.Value,
                    Status = "PickupCollected",
                    StatusDescription = $"Shipment collected - {entity.PickupRequest.ActualPieces} pcs, {entity.PickupRequest.ActualWeight?.ToString("N2") ?? "0"} kg",
                    Location = entity.PickupRequest.City ?? "",
                    Remarks = null
                });
            }
        }
        
        if (entity.TrackingHistory != null)
        {
            trackingHistory.AddRange(entity.TrackingHistory
                .Where(t => t.IsPublic)
                .Select(t => new TrackingHistoryItemDto
                {
                    Timestamp = t.EventDateTime,
                    Status = t.Status.ToString(),
                    StatusDescription = t.StatusDescription ?? "",
                    Location = t.Location ?? "",
                    Remarks = t.Remarks
                }));
        }
        
        return new ShipmentTrackingInfoDto
        {
            Id = entity.Id,
            AWBNumber = entity.AWBNumber,
            BookingDate = entity.BookingDate,
            Status = entity.Status.ToString(),
            ServiceType = entity.CourierServiceType?.Name ?? "",
            PaymentMode = entity.PaymentMode.ToString(),
            SenderName = entity.SenderName,
            SenderAddress = entity.SenderAddress ?? "",
            SenderCity = entity.SenderCity ?? "",
            SenderPincode = entity.SenderPostalCode ?? "",
            SenderPhone = entity.SenderPhone ?? "",
            ReceiverName = entity.ReceiverName,
            ReceiverAddress = entity.ReceiverAddress ?? "",
            ReceiverCity = entity.ReceiverCity ?? "",
            ReceiverPincode = entity.ReceiverPostalCode ?? "",
            ReceiverPhone = entity.ReceiverPhone ?? "",
            Pieces = entity.NumberOfPieces,
            ActualWeight = entity.ActualWeight,
            ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
            FreightCharges = entity.FreightCharge,
            FuelSurcharge = entity.FuelSurcharge,
            CODCharges = entity.CODCharge,
            OtherCharges = entity.OtherCharges,
            TotalCharges = entity.TotalCharge,
            CODAmount = entity.CODAmount,
            CODCollected = entity.CODCollected,
            POD = entity.Status == ShipmentStatus.Delivered ? new PODInfoDto
            {
                ReceivedBy = entity.ReceivedBy ?? "",
                Relationship = entity.Relationship ?? "",
                DeliveryTime = entity.ActualDeliveryDate ?? DateTime.UtcNow,
                HasSignature = entity.SignatureImage != null && entity.SignatureImage.Length > 0,
                HasPhoto = entity.PODImage != null && entity.PODImage.Length > 0
            } : null,
            PickupRequest = entity.PickupRequest != null ? new PickupRequestInfoDto
            {
                Id = entity.PickupRequest.Id,
                RequestNumber = entity.PickupRequest.RequestNumber,
                RequestDate = entity.PickupRequest.RequestDate,
                ScheduledDate = entity.PickupRequest.ScheduledDate,
                Status = entity.PickupRequest.Status.ToString(),
                ContactName = entity.PickupRequest.ContactName,
                ContactPhone = entity.PickupRequest.ContactPhone,
                PickupAddress = entity.PickupRequest.PickupAddress,
                ExpectedPieces = entity.PickupRequest.ExpectedPieces,
                ExpectedWeight = entity.PickupRequest.ExpectedWeight,
                ActualPieces = entity.PickupRequest.ActualPieces,
                ActualWeight = entity.PickupRequest.ActualWeight,
                ActualPickupTime = entity.PickupRequest.ActualPickupTime
            } : null,
            TrackingHistory = trackingHistory.OrderByDescending(t => t.Timestamp).ToList()
        };
    }

    private static ShipmentDetailDto MapToDetailDto(Shipment entity) => new()
    {
        Id = entity.Id,
        AWBNumber = entity.AWBNumber,
        BookingDate = entity.BookingDate,
        CourierServiceTypeId = entity.CourierServiceTypeId,
        ServiceTypeName = entity.CourierServiceType?.Name,
        CustomerId = entity.CustomerId,
        CustomerName = entity.Customer?.Name,
        SenderName = entity.SenderName,
        SenderCompany = entity.SenderCompany,
        SenderPhone = entity.SenderPhone ?? string.Empty,
        SenderEmail = entity.SenderEmail,
        SenderAddress = entity.SenderAddress ?? string.Empty,
        SenderCity = entity.SenderCity ?? string.Empty,
        SenderState = entity.SenderState,
        SenderPostalCode = entity.SenderPostalCode ?? string.Empty,
        SenderCountry = entity.SenderCountry ?? string.Empty,
        ReceiverName = entity.ReceiverName,
        ReceiverCompany = entity.ReceiverCompany,
        ReceiverPhone = entity.ReceiverPhone ?? string.Empty,
        ReceiverEmail = entity.ReceiverEmail,
        ReceiverAddress = entity.ReceiverAddress ?? string.Empty,
        ReceiverCity = entity.ReceiverCity ?? string.Empty,
        ReceiverState = entity.ReceiverState,
        ReceiverPostalCode = entity.ReceiverPostalCode ?? string.Empty,
        ReceiverCountry = entity.ReceiverCountry ?? string.Empty,
        OriginZoneId = entity.OriginZoneId,
        OriginZoneName = entity.OriginZone?.ZoneName,
        DestinationZoneId = entity.DestinationZoneId,
        DestinationZoneName = entity.DestinationZone?.ZoneName,
        ShipmentClassification = entity.ShipmentClassification.ToString(),
        ShipmentMode = entity.ShipmentMode.ToString(),
        Pieces = entity.NumberOfPieces,
        ActualWeight = entity.ActualWeight,
        VolumetricWeight = entity.VolumetricWeight,
        ChargeableWeight = entity.ChargeableWeight,
        DeclaredValue = entity.DeclaredValue,
        PaymentMode = entity.PaymentMode.ToString(),
        FreightCharges = entity.FreightCharge,
        CODAmount = entity.CODAmount,
        InsuranceCharges = entity.InsuranceCharge,
        OtherCharges = entity.OtherCharges,
        TotalCharges = entity.TotalCharge,
        Status = entity.Status.ToString(),
        ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
        ActualDeliveryDate = entity.ActualDeliveryDate,
        AssignedAgentId = entity.AssignedAgentId,
        AssignedAgentName = entity.AssignedAgent?.Name,
        SpecialInstructions = entity.SpecialInstructions,
        InternalNotes = entity.InternalNotes,
        IsVoided = entity.IsVoided,
        VoidReason = entity.VoidReason,
        Items = entity.Items?.Select(i => new ShipmentItemDto
        {
            Id = i.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            Weight = i.Weight,
            Length = i.Length,
            Width = i.Width,
            Height = i.Height,
            DeclaredValue = i.DeclaredValue
        }).ToList() ?? new(),
        TrackingHistory = entity.TrackingHistory?.Select(t => new ShipmentTrackingDto
        {
            Id = t.Id,
            Status = t.Status.ToString(),
            StatusDescription = t.StatusDescription,
            Location = t.Location,
            EventDateTime = t.EventDateTime,
            AgentId = t.AgentId,
            IsPublic = t.IsPublic
        }).ToList() ?? new(),
        Charges = entity.Charges?.Select(c => new ShipmentChargeResponseDto
        {
            Id = c.Id,
            ChargeTypeId = c.ChargeTypeId,
            ChargeName = c.ChargeName,
            Amount = c.Amount,
            Notes = c.Notes
        }).ToList() ?? new()
    };
}

public class ShipmentListDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? CustomerName { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderCity { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverCity { get; set; } = string.Empty;
    public int Pieces { get; set; }
    public decimal ChargeableWeight { get; set; }
    public decimal TotalCharges { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string? AssignedAgentName { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}

public class ShipmentDetailDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public Guid CourierServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderCompany { get; set; }
    public string SenderPhone { get; set; } = string.Empty;
    public string? SenderEmail { get; set; }
    public string SenderAddress { get; set; } = string.Empty;
    public string SenderCity { get; set; } = string.Empty;
    public string? SenderState { get; set; }
    public string SenderPostalCode { get; set; } = string.Empty;
    public string SenderCountry { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverCompany { get; set; }
    public string ReceiverPhone { get; set; } = string.Empty;
    public string? ReceiverEmail { get; set; }
    public string ReceiverAddress { get; set; } = string.Empty;
    public string ReceiverCity { get; set; } = string.Empty;
    public string? ReceiverState { get; set; }
    public string ReceiverPostalCode { get; set; } = string.Empty;
    public string ReceiverCountry { get; set; } = string.Empty;
    public Guid? OriginZoneId { get; set; }
    public string? OriginZoneName { get; set; }
    public Guid? DestinationZoneId { get; set; }
    public string? DestinationZoneName { get; set; }
    public string ShipmentClassification { get; set; } = "ParcelUpto30kg";
    public string ShipmentMode { get; set; } = "Domestic";
    public int Pieces { get; set; }
    public decimal ActualWeight { get; set; }
    public decimal VolumetricWeight { get; set; }
    public decimal ChargeableWeight { get; set; }
    public decimal DeclaredValue { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
    public decimal FreightCharges { get; set; }
    public decimal CODAmount { get; set; }
    public decimal InsuranceCharges { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal TotalCharges { get; set; }
    public string Status { get; set; } = "Booked";
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? InternalNotes { get; set; }
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }
    public List<ShipmentItemDto> Items { get; set; } = new();
    public List<ShipmentTrackingDto> TrackingHistory { get; set; } = new();
    public List<ShipmentChargeResponseDto> Charges { get; set; } = new();
}

public class ShipmentChargeResponseDto
{
    public Guid Id { get; set; }
    public Guid? ChargeTypeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class ShipmentItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal DeclaredValue { get; set; }
}

public class ShipmentTrackingDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime EventDateTime { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public bool IsPublic { get; set; }
}

public class CreateShipmentRequest
{
    public DateTime BookingDate { get; set; } = DateTime.UtcNow.Date;
    public Guid CourierServiceTypeId { get; set; }
    public Guid? CustomerId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderCompany { get; set; }
    public string SenderPhone { get; set; } = string.Empty;
    public string? SenderEmail { get; set; }
    public string SenderAddress { get; set; } = string.Empty;
    public string SenderCity { get; set; } = string.Empty;
    public string? SenderState { get; set; }
    public string SenderPostalCode { get; set; } = string.Empty;
    public string SenderCountry { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverCompany { get; set; }
    public string ReceiverPhone { get; set; } = string.Empty;
    public string? ReceiverEmail { get; set; }
    public string ReceiverAddress { get; set; } = string.Empty;
    public string ReceiverCity { get; set; } = string.Empty;
    public string? ReceiverState { get; set; }
    public string ReceiverPostalCode { get; set; } = string.Empty;
    public string ReceiverCountry { get; set; } = string.Empty;
    public Guid? OriginZoneId { get; set; }
    public Guid? DestinationZoneId { get; set; }
    public string ShipmentClassification { get; set; } = "ParcelUpto30kg";
    public int Pieces { get; set; } = 1;
    public decimal ActualWeight { get; set; }
    public decimal VolumetricWeight { get; set; }
    public decimal DeclaredValue { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
    public decimal CODAmount { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? InternalNotes { get; set; }
    public List<ShipmentItemDto> Items { get; set; } = new();
    public List<CreateChargeRequest>? Charges { get; set; }
}

public class CreateChargeRequest
{
    public Guid? ChargeTypeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateShipmentRequest : CreateShipmentRequest
{
    public decimal FreightCharges { get; set; }
    public bool FreightChargeModified { get; set; }
    public decimal OriginalFreightCharges { get; set; }
    public string? FreightChangeReason { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? AgentId { get; set; }
}

public class VoidShipmentRequest
{
    public string Reason { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class CalculateChargesRequest
{
    public Guid ServiceTypeId { get; set; }
    public Guid OriginZoneId { get; set; }
    public Guid DestinationZoneId { get; set; }
    public decimal Weight { get; set; }
    public decimal DeclaredValue { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
}

public class CalculateChargesResponse
{
    public decimal FreightCharges { get; set; }
}

public class RecordPODRequest
{
    public string ReceivedBy { get; set; } = string.Empty;
    public string? ReceiverRelation { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? SignatureBase64 { get; set; }
    public string? PhotoBase64 { get; set; }
    public decimal? CODCollected { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class AddTrackingRequest
{
    public string Status { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Remarks { get; set; }
    public Guid? AgentId { get; set; }
}

public class ShipmentTrackingInfoDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = "";
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string PaymentMode { get; set; } = "";
    
    public string SenderName { get; set; } = "";
    public string SenderAddress { get; set; } = "";
    public string SenderCity { get; set; } = "";
    public string SenderPincode { get; set; } = "";
    public string SenderPhone { get; set; } = "";
    
    public string ReceiverName { get; set; } = "";
    public string ReceiverAddress { get; set; } = "";
    public string ReceiverCity { get; set; } = "";
    public string ReceiverPincode { get; set; } = "";
    public string ReceiverPhone { get; set; } = "";
    
    public int Pieces { get; set; }
    public decimal ActualWeight { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    public decimal FreightCharges { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal CODCharges { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal TotalCharges { get; set; }
    
    public decimal? CODAmount { get; set; }
    public bool CODCollected { get; set; }
    
    public PODInfoDto? POD { get; set; }
    public PickupRequestInfoDto? PickupRequest { get; set; }
    public List<TrackingHistoryItemDto> TrackingHistory { get; set; } = new();
}

public class PickupRequestInfoDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = "";
    public DateTime RequestDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = "";
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? PickupAddress { get; set; }
    public int ExpectedPieces { get; set; }
    public decimal? ExpectedWeight { get; set; }
    public int ActualPieces { get; set; }
    public decimal? ActualWeight { get; set; }
    public DateTime? ActualPickupTime { get; set; }
}

public class PODInfoDto
{
    public string ReceivedBy { get; set; } = "";
    public string Relationship { get; set; } = "";
    public DateTime DeliveryTime { get; set; }
    public bool HasSignature { get; set; }
    public bool HasPhoto { get; set; }
}

public class TrackingHistoryItemDto
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = "";
    public string StatusDescription { get; set; } = "";
    public string Location { get; set; } = "";
    public string? Remarks { get; set; }
}
