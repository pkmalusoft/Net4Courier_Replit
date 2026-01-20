using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class PODUpdateResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string AwbNo { get; set; } = "";
}

public class PODUpdateRequest
{
    public string AwbNo { get; set; } = "";
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Delivered;
    public DateTime? DeliveryDate { get; set; }
    public string? ReceivedBy { get; set; }
    public RecipientRelation? Relation { get; set; }
    public NonDeliveryReason? NonDeliveryReason { get; set; }
    public string? Remarks { get; set; }
    public string? Photo1 { get; set; }
    public string? Photo2 { get; set; }
    public string? Photo3 { get; set; }
    public string? SignatureImage { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class PODUpdateService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ShipmentStatusService _shipmentStatusService;

    public PODUpdateService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ShipmentStatusService shipmentStatusService)
    {
        _dbContextFactory = dbContextFactory;
        _shipmentStatusService = shipmentStatusService;
    }

    public PODUpdateResult ValidateRequest(PODUpdateRequest request)
    {
        var result = new PODUpdateResult { AwbNo = request.AwbNo };

        if (string.IsNullOrWhiteSpace(request.AwbNo))
        {
            result.ErrorMessage = "AWB number is required";
            return result;
        }

        if (!request.DeliveryDate.HasValue)
        {
            result.ErrorMessage = "Delivery date is required";
            return result;
        }

        if (request.DeliveryStatus == DeliveryStatus.Delivered || request.DeliveryStatus == DeliveryStatus.PartialDelivery)
        {
            if (string.IsNullOrWhiteSpace(request.ReceivedBy))
            {
                result.ErrorMessage = "Received By is required for delivered status";
                return result;
            }
        }

        if (request.DeliveryStatus == DeliveryStatus.NotDelivered || request.DeliveryStatus == DeliveryStatus.Refused)
        {
            if (request.NonDeliveryReason == null)
            {
                result.ErrorMessage = "Reason is required for non-delivery status";
                return result;
            }
        }

        result.Success = true;
        return result;
    }

    public async Task<PODUpdateResult> UpdateSinglePODAsync(PODUpdateRequest request, long? userId = null, long? branchId = null)
    {
        var validation = ValidateRequest(request);
        if (!validation.Success)
        {
            return validation;
        }

        var result = new PODUpdateResult { AwbNo = request.AwbNo };

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var awb = await dbContext.InscanMasters
            .FirstOrDefaultAsync(i => i.AWBNo == request.AwbNo.Trim() && !i.IsDeleted);

        if (awb == null)
        {
            result.ErrorMessage = $"AWB {request.AwbNo} not found";
            return result;
        }

        if (awb.CourierStatusId == CourierStatus.Delivered)
        {
            result.ErrorMessage = $"AWB {request.AwbNo} is already delivered";
            return result;
        }

        if (awb.CourierStatusId == CourierStatus.Cancelled)
        {
            result.ErrorMessage = $"AWB {request.AwbNo} is cancelled";
            return result;
        }

        try
        {
            var courierStatus = request.DeliveryStatus switch
            {
                DeliveryStatus.Delivered => CourierStatus.Delivered,
                DeliveryStatus.PartialDelivery => CourierStatus.OnHold,
                DeliveryStatus.Refused => CourierStatus.Returned,
                DeliveryStatus.NotDelivered => CourierStatus.OnHold,
                _ => CourierStatus.OnHold
            };

            var tracking = new AWBTracking
            {
                InscanId = awb.Id,
                EventDateTime = DateTime.UtcNow,
                StatusId = courierStatus,
                DeliveryStatusId = request.DeliveryStatus,
                DeliveryDateTime = request.DeliveryDate?.ToUniversalTime(),
                ReceivedBy = request.ReceivedBy,
                RelationType = request.Relation,
                NonDeliveryReasonId = request.NonDeliveryReason,
                PODImage = request.Photo1,
                PODImage2 = request.Photo2,
                PODImage3 = request.Photo3,
                SignatureImage = request.SignatureImage,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Remarks = request.Remarks,
                IsPODCaptured = request.DeliveryStatus == DeliveryStatus.Delivered || request.DeliveryStatus == DeliveryStatus.PartialDelivery,
                IsPublic = true,
                CreatedBy = (int?)userId
            };

            dbContext.AWBTrackings.Add(tracking);

            awb.CourierStatusId = courierStatus;
            awb.ModifiedAt = DateTime.UtcNow;
            awb.ModifiedBy = (int?)userId;

            await dbContext.SaveChangesAsync();

            await RecordStatusHistoryAsync(awb, request, userId, branchId);

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error updating POD: {ex.Message}";
        }

        return result;
    }

    public async Task<List<PODUpdateResult>> UpdateBatchPODAsync(List<PODUpdateRequest> requests, long? userId = null, long? branchId = null)
    {
        var results = new List<PODUpdateResult>();

        foreach (var request in requests)
        {
            var result = await UpdateSinglePODAsync(request, userId, branchId);
            results.Add(result);
        }

        return results;
    }

    private async Task RecordStatusHistoryAsync(InscanMaster awb, PODUpdateRequest request, long? userId, long? branchId)
    {
        try
        {
            if (request.DeliveryStatus == DeliveryStatus.Delivered || request.DeliveryStatus == DeliveryStatus.PartialDelivery)
            {
                await _shipmentStatusService.SetStatus(
                    awb.Id, "DELIVERED", "POD", null, null,
                    branchId, awb.ConsigneeCity, userId, null,
                    $"Received by {request.ReceivedBy} ({request.Relation})", isAutomatic: false);

                await _shipmentStatusService.SetStatus(
                    awb.Id, "POD_CAPTURED", "POD", null, null,
                    branchId, awb.ConsigneeCity, userId, null,
                    $"POD captured via batch update", isAutomatic: true);
            }
            else if (request.DeliveryStatus == DeliveryStatus.Refused)
            {
                await _shipmentStatusService.SetStatus(
                    awb.Id, "RETURN_TO_ORIGIN", "POD", null, null,
                    branchId, awb.ConsigneeCity, userId, null,
                    $"Receiver rejected: {request.NonDeliveryReason}", isAutomatic: false);
            }
            else if (request.DeliveryStatus == DeliveryStatus.NotDelivered)
            {
                await _shipmentStatusService.SetStatus(
                    awb.Id, "DELIVERY_FAILED", "POD", null, null,
                    branchId, awb.ConsigneeCity, userId, null,
                    $"Unable to deliver: {request.NonDeliveryReason}", isAutomatic: false);
            }
        }
        catch
        {
        }
    }

    public static DeliveryStatus? ParseDeliveryStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.Trim().ToUpperInvariant() switch
        {
            "DELIVERED" => DeliveryStatus.Delivered,
            "PARTIAL" or "PARTIAL DELIVERY" or "PARTIALDELIVERY" => DeliveryStatus.PartialDelivery,
            "REFUSED" => DeliveryStatus.Refused,
            "NOT DELIVERED" or "NOTDELIVERED" or "FAILED" => DeliveryStatus.NotDelivered,
            _ => null
        };
    }

    public static RecipientRelation? ParseRelation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.Trim().ToUpperInvariant() switch
        {
            "SELF" => RecipientRelation.Self,
            "FAMILY" => RecipientRelation.Family,
            "COLLEAGUE" => RecipientRelation.Colleague,
            "SECURITY" => RecipientRelation.Security,
            "RECEPTION" => RecipientRelation.Reception,
            "NEIGHBOR" or "NEIGHBOUR" => RecipientRelation.Neighbor,
            "OTHER" => RecipientRelation.Other,
            _ => null
        };
    }

    public static NonDeliveryReason? ParseNonDeliveryReason(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.Trim().ToUpperInvariant() switch
        {
            "ADDRESS NOT FOUND" or "ADDRESSNOTFOUND" => NonDeliveryReason.AddressNotFound,
            "CUSTOMER NOT AVAILABLE" or "CUSTOMERNOTAVAILABLE" or "NOT AVAILABLE" => NonDeliveryReason.CustomerNotAvailable,
            "REFUSED" or "REFUSED BY CUSTOMER" => NonDeliveryReason.Refused,
            "PREMISES CLOSED" or "PREMISESCLOSED" or "CLOSED" => NonDeliveryReason.PremisesClosed,
            "INCORRECT ADDRESS" or "INCORRECTADDRESS" or "WRONG ADDRESS" => NonDeliveryReason.IncorrectAddress,
            "RESCHEDULE" or "CUSTOMER REQUESTED RESCHEDULE" => NonDeliveryReason.CustomerRequestedReschedule,
            "WEATHER" or "WEATHER CONDITIONS" => NonDeliveryReason.WeatherConditions,
            "ACCESS RESTRICTED" or "ACCESSRESTRICTED" => NonDeliveryReason.AccessRestricted,
            "OTHER" => NonDeliveryReason.Other,
            _ => null
        };
    }
}
