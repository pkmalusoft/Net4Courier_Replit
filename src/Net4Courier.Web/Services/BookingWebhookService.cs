using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class BookingWebhookService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<BookingWebhookService> _logger;
    private readonly ISecureStorageService _secureStorage;

    public BookingWebhookService(ApplicationDbContext dbContext, ILogger<BookingWebhookService> logger, ISecureStorageService secureStorage)
    {
        _dbContext = dbContext;
        _logger = logger;
        _secureStorage = secureStorage;
    }

    public async Task<(bool Success, string Message, long? PickupRequestId)> ProcessBookingAsync(
        BookingWebhookPayload payload, 
        string integrationId)
    {
        try
        {
            if (!long.TryParse(integrationId, out var apiSettingId))
            {
                _logger.LogWarning("Invalid integration ID format: {IntegrationId}", integrationId);
                return (false, "Invalid integration ID", null);
            }

            var apiSetting = await _dbContext.ApiSettings
                .FirstOrDefaultAsync(s => 
                    s.Id == apiSettingId &&
                    s.IsEnabled && 
                    !s.IsDeleted &&
                    s.IntegrationType == ApiIntegrationType.BookingWebsite);

            if (apiSetting == null)
            {
                _logger.LogWarning("Invalid or disabled integration ID: {IntegrationId}", integrationId);
                return (false, "Invalid webhook configuration", null);
            }

            if (string.IsNullOrEmpty(payload.CustomerName) || string.IsNullOrEmpty(payload.PickupAddress))
            {
                return (false, "Customer name and pickup address are required", null);
            }

            var company = await _dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            var branch = await _dbContext.Branches.FirstOrDefaultAsync(b => !b.IsDeleted);

            var pickupNo = await GeneratePickupNumber(branch?.Id);

            var pickupRequest = new PickupRequest
            {
                PickupNo = pickupNo,
                RequestDate = DateTime.UtcNow,
                CompanyId = company?.Id,
                BranchId = apiSetting.BranchId ?? branch?.Id,
                CustomerName = payload.CustomerName,
                ContactPerson = payload.ContactPerson ?? payload.CustomerName,
                Phone = payload.Phone,
                Mobile = payload.Mobile ?? payload.Phone,
                Email = payload.Email,
                PickupAddress = payload.PickupAddress,
                City = payload.City,
                State = payload.State,
                Country = payload.Country ?? "India",
                PostalCode = payload.PostalCode,
                Landmark = payload.Landmark,
                EstimatedPieces = payload.EstimatedPieces,
                EstimatedWeight = payload.EstimatedWeight,
                PackageDescription = payload.PackageDescription,
                SpecialInstructions = payload.SpecialInstructions,
                ReferenceNo = payload.BookingId,
                Status = PickupStatus.PickupRequest,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(payload.ScheduledDate) && DateTime.TryParse(payload.ScheduledDate, out var scheduledDate))
            {
                pickupRequest.ScheduledDate = scheduledDate;
            }

            if (!string.IsNullOrEmpty(payload.ScheduledTimeFrom) && TimeSpan.TryParse(payload.ScheduledTimeFrom, out var timeFrom))
            {
                pickupRequest.ScheduledTimeFrom = (pickupRequest.ScheduledDate ?? DateTime.Today).Add(timeFrom);
            }

            if (!string.IsNullOrEmpty(payload.ScheduledTimeTo) && TimeSpan.TryParse(payload.ScheduledTimeTo, out var timeTo))
            {
                pickupRequest.ScheduledTimeTo = (pickupRequest.ScheduledDate ?? DateTime.Today).Add(timeTo);
            }

            _dbContext.PickupRequests.Add(pickupRequest);

            apiSetting.LastSyncAt = DateTime.UtcNow;
            apiSetting.LastSyncStatus = "Success";
            apiSetting.LastSyncError = null;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created pickup request {PickupNo} from booking {BookingId}", 
                pickupNo, payload.BookingId);

            return (true, $"Pickup request {pickupNo} created successfully", pickupRequest.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking webhook for {BookingId}", payload.BookingId);
            
            if (long.TryParse(integrationId, out var settingId))
            {
                var apiSetting = await _dbContext.ApiSettings.FindAsync(settingId);
                if (apiSetting != null)
                {
                    apiSetting.LastSyncAt = DateTime.UtcNow;
                    apiSetting.LastSyncStatus = "Failed";
                    apiSetting.LastSyncError = ex.Message;
                    await _dbContext.SaveChangesAsync();
                }
            }

            return (false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<bool> ValidateWebhookSecretAsync(string integrationId, string? webhookSecret = null)
    {
        if (!long.TryParse(integrationId, out var apiSettingId))
            return false;

        var apiSetting = await _dbContext.ApiSettings
            .FirstOrDefaultAsync(s => 
                s.Id == apiSettingId &&
                s.IsEnabled && 
                !s.IsDeleted &&
                s.IntegrationType == ApiIntegrationType.BookingWebsite);

        if (apiSetting == null)
            return false;

        if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(apiSetting.WebhookSecret))
        {
            var decryptedSecret = _secureStorage.Decrypt(apiSetting.WebhookSecret);
            return decryptedSecret == webhookSecret;
        }

        return false;
    }

    private async Task<string> GeneratePickupNumber(long? branchId)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _dbContext.PickupRequests
            .CountAsync(p => p.CreatedAt >= today && p.BranchId == branchId);
        
        return $"PKP{DateTime.UtcNow:yyyyMMdd}{(count + 1):D4}";
    }
}

public class BookingWebhookPayload
{
    public string? BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Landmark { get; set; }
    public int? EstimatedPieces { get; set; }
    public decimal? EstimatedWeight { get; set; }
    public string? PackageDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? ScheduledDate { get; set; }
    public string? ScheduledTimeFrom { get; set; }
    public string? ScheduledTimeTo { get; set; }
}
