using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class ApiSetting : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ApiIntegrationType IntegrationType { get; set; } = ApiIntegrationType.BookingWebsite;
    
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public string? WebhookEndpoint { get; set; }
    
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? BearerToken { get; set; }
    public DateTime? TokenExpiry { get; set; }
    
    public AuthenticationType AuthType { get; set; } = AuthenticationType.ApiKey;
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public string? LastSyncError { get; set; }
    public int? SyncIntervalMinutes { get; set; }
    
    public long? BranchId { get; set; }
    public string? Headers { get; set; }
    public string? CustomFields { get; set; }
}

public enum ApiIntegrationType
{
    BookingWebsite = 1,
    CarrierTracking = 2,
    AddressValidation = 3,
    CurrencyExchange = 4,
    SMSNotification = 5,
    EmailService = 6,
    PaymentGateway = 7
}

public enum AuthenticationType
{
    None = 0,
    ApiKey = 1,
    BasicAuth = 2,
    BearerToken = 3,
    OAuth2 = 4
}
