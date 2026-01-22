using System.ComponentModel.DataAnnotations;
using Server.Data;

namespace Server.Modules.Courier.Models;

public enum IntegrationDirection
{
    Inbound,
    Outbound,
    Bidirectional
}

public enum IntegrationCategory
{
    Coloader,
    Government,
    Tracking,
    Customs,
    Payment,
    Other
}

public enum AuthSchemeType
{
    None,
    ApiKey,
    BasicAuth,
    OAuth2Client,
    OAuth2Password,
    BearerToken,
    Custom
}

public enum IntegrationStatus
{
    Active,
    Inactive,
    Testing,
    Error,
    Suspended
}

public class IntegrationProvider : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string ProviderCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ProviderName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public IntegrationCategory Category { get; set; } = IntegrationCategory.Coloader;

    public IntegrationDirection Direction { get; set; } = IntegrationDirection.Inbound;

    [MaxLength(200)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? DocumentationUrl { get; set; }

    public AuthSchemeType DefaultAuthScheme { get; set; } = AuthSchemeType.ApiKey;

    public string? CapabilitiesJson { get; set; }

    public string? DefaultHeadersJson { get; set; }

    public string? RequestMappingTemplate { get; set; }

    public string? ResponseMappingTemplate { get; set; }

    public bool IsSystemProvider { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public ICollection<ProviderEndpoint> Endpoints { get; set; } = new List<ProviderEndpoint>();

    public ICollection<TenantIntegration> TenantIntegrations { get; set; } = new List<TenantIntegration>();
}

public enum EndpointEnvironment
{
    Production,
    Sandbox,
    Staging
}

public class ProviderEndpoint : BaseEntity
{
    public Guid IntegrationProviderId { get; set; }
    public IntegrationProvider IntegrationProvider { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string EndpointName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public EndpointEnvironment Environment { get; set; } = EndpointEnvironment.Production;

    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Path { get; set; }

    [MaxLength(10)]
    public string HttpMethod { get; set; } = "GET";

    public string? DefaultHeadersJson { get; set; }

    public string? QueryParametersJson { get; set; }

    public int? RateLimitPerMinute { get; set; }

    public int? TimeoutSeconds { get; set; } = 30;

    public int? RetryCount { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}

public class TenantIntegration : BaseEntity
{
    public Guid IntegrationProviderId { get; set; }
    public IntegrationProvider IntegrationProvider { get; set; } = null!;

    public IntegrationStatus Status { get; set; } = IntegrationStatus.Inactive;

    public EndpointEnvironment ActiveEnvironment { get; set; } = EndpointEnvironment.Sandbox;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public bool IsEnabled { get; set; } = false;

    public DateTime? LastSyncAt { get; set; }

    public DateTime? LastSuccessAt { get; set; }

    public DateTime? LastErrorAt { get; set; }

    [MaxLength(500)]
    public string? LastErrorMessage { get; set; }

    public int ConsecutiveFailures { get; set; } = 0;

    public int? ThrottleDelayMs { get; set; }

    public string? CustomSettingsJson { get; set; }

    public ICollection<TenantIntegrationSecret> Secrets { get; set; } = new List<TenantIntegrationSecret>();

    public ICollection<IntegrationJob> Jobs { get; set; } = new List<IntegrationJob>();
}

public class TenantIntegrationSecret : BaseEntity
{
    public Guid TenantIntegrationId { get; set; }
    public TenantIntegration TenantIntegration { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string SecretKey { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SecretLabel { get; set; }

    public string EncryptedValue { get; set; } = string.Empty;

    public string? EncryptionKeyReference { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastRotatedAt { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsMasked { get; set; } = true;
}

public enum JobFrequency
{
    Manual,
    Hourly,
    Daily,
    Weekly,
    Custom
}

public enum JobStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Paused,
    Cancelled
}

public class IntegrationJob : BaseEntity
{
    public Guid TenantIntegrationId { get; set; }
    public TenantIntegration TenantIntegration { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string JobName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public IntegrationDirection Direction { get; set; } = IntegrationDirection.Inbound;

    public JobFrequency Frequency { get; set; } = JobFrequency.Manual;

    [MaxLength(100)]
    public string? CronExpression { get; set; }

    public DateTime? NextRunAt { get; set; }

    public DateTime? LastRunAt { get; set; }

    public JobStatus LastRunStatus { get; set; } = JobStatus.Scheduled;

    public bool IsEnabled { get; set; } = true;

    public int? BatchSize { get; set; } = 100;

    public string? FilterCriteriaJson { get; set; }

    public string? PayloadTemplateJson { get; set; }

    public ICollection<IntegrationRun> Runs { get; set; } = new List<IntegrationRun>();
}

public enum RunStatus
{
    Pending,
    Running,
    Completed,
    PartialSuccess,
    Failed,
    Cancelled
}

public class IntegrationRun : BaseEntity
{
    public Guid IntegrationJobId { get; set; }
    public IntegrationJob IntegrationJob { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public RunStatus Status { get; set; } = RunStatus.Pending;

    public int TotalRecords { get; set; } = 0;

    public int SuccessCount { get; set; } = 0;

    public int FailureCount { get; set; } = 0;

    public int SkippedCount { get; set; } = 0;

    [MaxLength(500)]
    public string? ErrorSummary { get; set; }

    public string? ErrorDetailsJson { get; set; }

    public string? RequestPayloadSample { get; set; }

    public string? ResponsePayloadSample { get; set; }

    public long? DurationMs { get; set; }

    [MaxLength(200)]
    public string? TriggeredBy { get; set; }

    public bool IsManual { get; set; } = false;

    public ICollection<IntegrationRunEvent> Events { get; set; } = new List<IntegrationRunEvent>();
}

public enum RunEventType
{
    RecordProcessed,
    RecordCreated,
    RecordUpdated,
    RecordSkipped,
    RecordFailed,
    ValidationError,
    ApiError,
    TransformError
}

public class IntegrationRunEvent : BaseEntity
{
    public Guid IntegrationRunId { get; set; }
    public IntegrationRun IntegrationRun { get; set; } = null!;

    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    [MaxLength(50)]
    public string? AWBNumber { get; set; }

    [MaxLength(100)]
    public string? ExternalReference { get; set; }

    public RunEventType EventType { get; set; }

    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;

    public bool IsSuccess { get; set; } = true;

    [MaxLength(500)]
    public string? Message { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public int? HttpStatusCode { get; set; }
}

public class IntegrationAuditLog : BaseEntity
{
    public Guid? TenantIntegrationId { get; set; }
    public TenantIntegration? TenantIntegration { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    [MaxLength(200)]
    public string? ActorId { get; set; }

    [MaxLength(200)]
    public string? ActorName { get; set; }

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
}
