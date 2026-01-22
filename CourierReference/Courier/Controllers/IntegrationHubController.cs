using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntegrationHubController : ControllerBase
{
    private readonly IIntegrationHubService _integrationService;

    public IntegrationHubController(IIntegrationHubService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpGet("providers")]
    public async Task<ActionResult<List<IntegrationProviderDto>>> GetProviders()
    {
        var providers = await _integrationService.GetAllProvidersAsync();
        return Ok(providers.Select(MapProviderToDto));
    }

    [HttpGet("providers/{id}")]
    public async Task<ActionResult<IntegrationProviderDto>> GetProvider(Guid id)
    {
        var provider = await _integrationService.GetProviderByIdAsync(id);
        if (provider == null) return NotFound();
        return Ok(MapProviderToDto(provider));
    }

    [HttpPost("providers")]
    public async Task<ActionResult<IntegrationProviderDto>> CreateProvider([FromBody] CreateProviderDto dto)
    {
        var category = IntegrationCategory.Coloader;
        if (!string.IsNullOrEmpty(dto.Category) && Enum.TryParse<IntegrationCategory>(dto.Category, true, out var parsedCat))
            category = parsedCat;

        var direction = IntegrationDirection.Inbound;
        if (!string.IsNullOrEmpty(dto.Direction) && Enum.TryParse<IntegrationDirection>(dto.Direction, true, out var parsedDir))
            direction = parsedDir;

        var authScheme = AuthSchemeType.ApiKey;
        if (!string.IsNullOrEmpty(dto.DefaultAuthScheme) && Enum.TryParse<AuthSchemeType>(dto.DefaultAuthScheme, true, out var parsedAuth))
            authScheme = parsedAuth;

        var provider = new IntegrationProvider
        {
            ProviderCode = dto.ProviderCode,
            ProviderName = dto.ProviderName,
            Description = dto.Description,
            Category = category,
            Direction = direction,
            DefaultAuthScheme = authScheme,
            LogoUrl = dto.LogoUrl,
            DocumentationUrl = dto.DocumentationUrl,
            CreatedBy = User.Identity?.Name
        };

        var result = await _integrationService.CreateProviderAsync(provider);
        return CreatedAtAction(nameof(GetProvider), new { id = result.Id }, MapProviderToDto(result));
    }

    [HttpGet("tenant-integrations")]
    public async Task<ActionResult<List<TenantIntegrationDto>>> GetTenantIntegrations([FromQuery] Guid tenantId)
    {
        var integrations = await _integrationService.GetTenantIntegrationsAsync(tenantId);
        return Ok(integrations.Select(MapTenantIntegrationToDto));
    }

    [HttpGet("tenant-integrations/{id}")]
    public async Task<ActionResult<TenantIntegrationDto>> GetTenantIntegration(Guid id)
    {
        var integration = await _integrationService.GetTenantIntegrationByIdAsync(id);
        if (integration == null) return NotFound();
        return Ok(MapTenantIntegrationToDto(integration));
    }

    [HttpPost("tenant-integrations")]
    public async Task<ActionResult<TenantIntegrationDto>> CreateTenantIntegration([FromBody] CreateTenantIntegrationDto dto)
    {
        var environment = EndpointEnvironment.Sandbox;
        if (!string.IsNullOrEmpty(dto.ActiveEnvironment) && Enum.TryParse<EndpointEnvironment>(dto.ActiveEnvironment, true, out var parsedEnv))
            environment = parsedEnv;

        var integration = new TenantIntegration
        {
            TenantId = dto.TenantId,
            IntegrationProviderId = dto.IntegrationProviderId,
            DisplayName = dto.DisplayName,
            ActiveEnvironment = environment,
            Status = IntegrationStatus.Inactive,
            CreatedBy = User.Identity?.Name
        };

        var result = await _integrationService.CreateTenantIntegrationAsync(integration);
        return CreatedAtAction(nameof(GetTenantIntegration), new { id = result.Id }, MapTenantIntegrationToDto(result));
    }

    [HttpPut("tenant-integrations/{id}/enable")]
    public async Task<ActionResult<TenantIntegrationDto>> EnableIntegration(Guid id, [FromQuery] bool enable = true)
    {
        try
        {
            var result = await _integrationService.EnableIntegrationAsync(id, enable);
            return Ok(MapTenantIntegrationToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("tenant-integrations/{id}/secrets")]
    public async Task<IActionResult> SetSecret(Guid id, [FromBody] SetSecretDto dto)
    {
        try
        {
            await _integrationService.SetSecretAsync(id, dto.Key, dto.Value, dto.Label);
            return Ok(new { message = "Secret saved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("tenant-integrations/{id}/jobs")]
    public async Task<ActionResult<List<IntegrationJobDto>>> GetJobs(Guid id)
    {
        var jobs = await _integrationService.GetJobsAsync(id);
        return Ok(jobs.Select(MapJobToDto));
    }

    [HttpPost("jobs")]
    public async Task<ActionResult<IntegrationJobDto>> CreateJob([FromBody] CreateJobDto dto)
    {
        var direction = IntegrationDirection.Inbound;
        if (!string.IsNullOrEmpty(dto.Direction) && Enum.TryParse<IntegrationDirection>(dto.Direction, true, out var parsedDir))
            direction = parsedDir;

        var frequency = JobFrequency.Manual;
        if (!string.IsNullOrEmpty(dto.Frequency) && Enum.TryParse<JobFrequency>(dto.Frequency, true, out var parsedFreq))
            frequency = parsedFreq;

        var job = new IntegrationJob
        {
            TenantIntegrationId = dto.TenantIntegrationId,
            JobName = dto.JobName,
            Description = dto.Description,
            Direction = direction,
            Frequency = frequency,
            CronExpression = dto.CronExpression,
            BatchSize = dto.BatchSize ?? 100,
            IsEnabled = true,
            CreatedBy = User.Identity?.Name
        };

        var result = await _integrationService.CreateJobAsync(job);
        return Ok(MapJobToDto(result));
    }

    [HttpPost("jobs/{id}/run")]
    public async Task<ActionResult<IntegrationRunDto>> RunJob(Guid id)
    {
        try
        {
            var run = await _integrationService.StartRunAsync(id, User.Identity?.Name ?? "System", isManual: true);
            return Ok(MapRunToDto(run));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("jobs/{id}/history")]
    public async Task<ActionResult<List<IntegrationRunDto>>> GetRunHistory(Guid id, [FromQuery] int limit = 50)
    {
        var runs = await _integrationService.GetRunHistoryAsync(id, limit);
        return Ok(runs.Select(MapRunToDto));
    }

    private static IntegrationProviderDto MapProviderToDto(IntegrationProvider p) => new()
    {
        Id = p.Id,
        ProviderCode = p.ProviderCode,
        ProviderName = p.ProviderName,
        Description = p.Description,
        Category = p.Category.ToString(),
        Direction = p.Direction.ToString(),
        DefaultAuthScheme = p.DefaultAuthScheme.ToString(),
        LogoUrl = p.LogoUrl,
        DocumentationUrl = p.DocumentationUrl,
        IsActive = p.IsActive,
        Endpoints = p.Endpoints?.Select(e => new ProviderEndpointDto
        {
            Id = e.Id,
            EndpointName = e.EndpointName,
            Environment = e.Environment.ToString(),
            BaseUrl = e.BaseUrl,
            Path = e.Path,
            HttpMethod = e.HttpMethod
        }).ToList() ?? new()
    };

    private static TenantIntegrationDto MapTenantIntegrationToDto(TenantIntegration ti) => new()
    {
        Id = ti.Id,
        IntegrationProviderId = ti.IntegrationProviderId,
        ProviderName = ti.IntegrationProvider?.ProviderName,
        ProviderCode = ti.IntegrationProvider?.ProviderCode,
        DisplayName = ti.DisplayName,
        Status = ti.Status.ToString(),
        ActiveEnvironment = ti.ActiveEnvironment.ToString(),
        IsEnabled = ti.IsEnabled,
        LastSyncAt = ti.LastSyncAt,
        LastSuccessAt = ti.LastSuccessAt,
        LastErrorAt = ti.LastErrorAt,
        LastErrorMessage = ti.LastErrorMessage,
        ConsecutiveFailures = ti.ConsecutiveFailures,
        SecretKeys = ti.Secrets?.Select(s => s.SecretKey).ToList() ?? new(),
        JobCount = ti.Jobs?.Count ?? 0
    };

    private static IntegrationJobDto MapJobToDto(IntegrationJob j) => new()
    {
        Id = j.Id,
        JobName = j.JobName,
        Description = j.Description,
        Direction = j.Direction.ToString(),
        Frequency = j.Frequency.ToString(),
        CronExpression = j.CronExpression,
        NextRunAt = j.NextRunAt,
        LastRunAt = j.LastRunAt,
        LastRunStatus = j.LastRunStatus.ToString(),
        IsEnabled = j.IsEnabled,
        BatchSize = j.BatchSize
    };

    private static IntegrationRunDto MapRunToDto(IntegrationRun r) => new()
    {
        Id = r.Id,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt,
        Status = r.Status.ToString(),
        TotalRecords = r.TotalRecords,
        SuccessCount = r.SuccessCount,
        FailureCount = r.FailureCount,
        SkippedCount = r.SkippedCount,
        ErrorSummary = r.ErrorSummary,
        DurationMs = r.DurationMs,
        TriggeredBy = r.TriggeredBy,
        IsManual = r.IsManual
    };
}

public class IntegrationProviderDto
{
    public Guid Id { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string DefaultAuthScheme { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? DocumentationUrl { get; set; }
    public bool IsActive { get; set; }
    public List<ProviderEndpointDto> Endpoints { get; set; } = new();
}

public class ProviderEndpointDto
{
    public Guid Id { get; set; }
    public string EndpointName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
}

public class TenantIntegrationDto
{
    public Guid Id { get; set; }
    public Guid IntegrationProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderCode { get; set; }
    public string? DisplayName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ActiveEnvironment { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public DateTime? LastSuccessAt { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public string? LastErrorMessage { get; set; }
    public int ConsecutiveFailures { get; set; }
    public List<string> SecretKeys { get; set; } = new();
    public int JobCount { get; set; }
}

public class IntegrationJobDto
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string LastRunStatus { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int? BatchSize { get; set; }
}

public class IntegrationRunDto
{
    public Guid Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SkippedCount { get; set; }
    public string? ErrorSummary { get; set; }
    public long? DurationMs { get; set; }
    public string? TriggeredBy { get; set; }
    public bool IsManual { get; set; }
}

public class CreateProviderDto
{
    public string ProviderCode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Coloader";
    public string Direction { get; set; } = "Inbound";
    public string DefaultAuthScheme { get; set; } = "ApiKey";
    public string? LogoUrl { get; set; }
    public string? DocumentationUrl { get; set; }
}

public class CreateTenantIntegrationDto
{
    public Guid TenantId { get; set; }
    public Guid IntegrationProviderId { get; set; }
    public string? DisplayName { get; set; }
    public string ActiveEnvironment { get; set; } = "Sandbox";
}

public class SetSecretDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public class CreateJobDto
{
    public Guid TenantIntegrationId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Direction { get; set; } = "Inbound";
    public string Frequency { get; set; } = "Manual";
    public string? CronExpression { get; set; }
    public int? BatchSize { get; set; }
}
