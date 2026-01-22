using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Data;
using Server.Modules.Courier.Models;
using System.Security.Cryptography;
using System.Text;

namespace Server.Modules.Courier.Services;

public interface IIntegrationHubService
{
    Task<List<IntegrationProvider>> GetAllProvidersAsync();
    Task<IntegrationProvider?> GetProviderByIdAsync(Guid id);
    Task<IntegrationProvider?> GetProviderByCodeAsync(string code);
    Task<IntegrationProvider> CreateProviderAsync(IntegrationProvider provider);
    Task<IntegrationProvider> UpdateProviderAsync(IntegrationProvider provider);
    Task<bool> DeleteProviderAsync(Guid id);

    Task<List<TenantIntegration>> GetTenantIntegrationsAsync(Guid tenantId);
    Task<TenantIntegration?> GetTenantIntegrationByIdAsync(Guid id);
    Task<TenantIntegration> CreateTenantIntegrationAsync(TenantIntegration integration);
    Task<TenantIntegration> UpdateTenantIntegrationAsync(TenantIntegration integration);
    Task<bool> DeleteTenantIntegrationAsync(Guid id);
    Task<TenantIntegration> EnableIntegrationAsync(Guid id, bool enable);

    Task<TenantIntegrationSecret> SetSecretAsync(Guid tenantIntegrationId, string key, string value, string? label = null);
    Task<string?> GetSecretValueAsync(Guid tenantIntegrationId, string key);
    Task<bool> DeleteSecretAsync(Guid secretId);

    Task<List<IntegrationJob>> GetJobsAsync(Guid tenantIntegrationId);
    Task<IntegrationJob?> GetJobByIdAsync(Guid id);
    Task<IntegrationJob> CreateJobAsync(IntegrationJob job);
    Task<IntegrationJob> UpdateJobAsync(IntegrationJob job);
    Task<bool> DeleteJobAsync(Guid id);

    Task<IntegrationRun> StartRunAsync(Guid jobId, string triggeredBy, bool isManual = false);
    Task<IntegrationRun> CompleteRunAsync(Guid runId, RunStatus status, int successCount, int failureCount, string? errorSummary = null);
    Task<IntegrationRunEvent> LogRunEventAsync(IntegrationRunEvent runEvent);
    Task<List<IntegrationRun>> GetRunHistoryAsync(Guid jobId, int limit = 50);

    Task LogAuditAsync(Guid? tenantIntegrationId, string entityType, Guid entityId, string action, string? before, string? after, string? actorId, string? actorName);
}

public class IntegrationHubService : IIntegrationHubService
{
    private readonly AppDbContext _context;
    private readonly byte[] _encryptionKey;

    public IntegrationHubService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        var keyString = configuration["Integration:EncryptionKey"] 
            ?? Environment.GetEnvironmentVariable("INTEGRATION_ENCRYPTION_KEY")
            ?? throw new InvalidOperationException("Integration encryption key not configured. Set INTEGRATION_ENCRYPTION_KEY environment variable.");
        
        if (keyString.Length < 32)
        {
            throw new InvalidOperationException($"Integration encryption key must be at least 32 characters. Current length: {keyString.Length}. Set a secure 32+ character key in INTEGRATION_ENCRYPTION_KEY.");
        }
        _encryptionKey = Encoding.UTF8.GetBytes(keyString[..32]);
    }

    public async Task<List<IntegrationProvider>> GetAllProvidersAsync()
    {
        return await _context.IntegrationProviders
            .Include(p => p.Endpoints)
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProviderName)
            .ToListAsync();
    }

    public async Task<IntegrationProvider?> GetProviderByIdAsync(Guid id)
    {
        return await _context.IntegrationProviders
            .Include(p => p.Endpoints)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IntegrationProvider?> GetProviderByCodeAsync(string code)
    {
        return await _context.IntegrationProviders
            .Include(p => p.Endpoints)
            .FirstOrDefaultAsync(p => p.ProviderCode == code);
    }

    public async Task<IntegrationProvider> CreateProviderAsync(IntegrationProvider provider)
    {
        _context.IntegrationProviders.Add(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<IntegrationProvider> UpdateProviderAsync(IntegrationProvider provider)
    {
        provider.UpdatedAt = DateTime.UtcNow;
        _context.IntegrationProviders.Update(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<bool> DeleteProviderAsync(Guid id)
    {
        var provider = await _context.IntegrationProviders.FindAsync(id);
        if (provider == null) return false;

        provider.IsActive = false;
        provider.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TenantIntegration>> GetTenantIntegrationsAsync(Guid tenantId)
    {
        return await _context.TenantIntegrations
            .Include(ti => ti.IntegrationProvider)
            .Include(ti => ti.Jobs)
            .Where(ti => ti.TenantId == tenantId)
            .OrderBy(ti => ti.IntegrationProvider.ProviderName)
            .ToListAsync();
    }

    public async Task<TenantIntegration?> GetTenantIntegrationByIdAsync(Guid id)
    {
        return await _context.TenantIntegrations
            .Include(ti => ti.IntegrationProvider)
                .ThenInclude(p => p.Endpoints)
            .Include(ti => ti.Secrets)
            .Include(ti => ti.Jobs)
            .FirstOrDefaultAsync(ti => ti.Id == id);
    }

    public async Task<TenantIntegration> CreateTenantIntegrationAsync(TenantIntegration integration)
    {
        _context.TenantIntegrations.Add(integration);
        await _context.SaveChangesAsync();

        await LogAuditAsync(integration.Id, "TenantIntegration", integration.Id, "Created", null, 
            System.Text.Json.JsonSerializer.Serialize(new { integration.IntegrationProviderId, integration.Status }), 
            integration.CreatedBy, integration.CreatedBy);

        return integration;
    }

    public async Task<TenantIntegration> UpdateTenantIntegrationAsync(TenantIntegration integration)
    {
        integration.UpdatedAt = DateTime.UtcNow;
        _context.TenantIntegrations.Update(integration);
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task<bool> DeleteTenantIntegrationAsync(Guid id)
    {
        var integration = await _context.TenantIntegrations.FindAsync(id);
        if (integration == null) return false;

        integration.IsEnabled = false;
        integration.Status = IntegrationStatus.Inactive;
        integration.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TenantIntegration> EnableIntegrationAsync(Guid id, bool enable)
    {
        var integration = await _context.TenantIntegrations.FindAsync(id);
        if (integration == null)
            throw new InvalidOperationException("Integration not found");

        integration.IsEnabled = enable;
        integration.Status = enable ? IntegrationStatus.Active : IntegrationStatus.Inactive;
        integration.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAuditAsync(integration.Id, "TenantIntegration", integration.Id, 
            enable ? "Enabled" : "Disabled", null, null, null, null);

        return integration;
    }

    public async Task<TenantIntegrationSecret> SetSecretAsync(Guid tenantIntegrationId, string key, string value, string? label = null)
    {
        var integration = await _context.TenantIntegrations.FindAsync(tenantIntegrationId);
        if (integration == null)
            throw new InvalidOperationException("Integration not found");

        var existing = await _context.TenantIntegrationSecrets
            .FirstOrDefaultAsync(s => s.TenantIntegrationId == tenantIntegrationId && s.SecretKey == key);

        var encryptedValue = EncryptValue(value);

        if (existing != null)
        {
            existing.EncryptedValue = encryptedValue;
            existing.SecretLabel = label ?? existing.SecretLabel;
            existing.LastRotatedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var secret = new TenantIntegrationSecret
        {
            TenantIntegrationId = tenantIntegrationId,
            TenantId = integration.TenantId,
            SecretKey = key,
            SecretLabel = label,
            EncryptedValue = encryptedValue,
            LastRotatedAt = DateTime.UtcNow
        };

        _context.TenantIntegrationSecrets.Add(secret);
        await _context.SaveChangesAsync();

        await LogAuditAsync(tenantIntegrationId, "TenantIntegrationSecret", secret.Id, 
            "SecretSet", null, $"Key: {key}", null, null);

        return secret;
    }

    public async Task<string?> GetSecretValueAsync(Guid tenantIntegrationId, string key)
    {
        var secret = await _context.TenantIntegrationSecrets
            .FirstOrDefaultAsync(s => s.TenantIntegrationId == tenantIntegrationId && s.SecretKey == key);

        if (secret == null) return null;

        return DecryptValue(secret.EncryptedValue);
    }

    public async Task<bool> DeleteSecretAsync(Guid secretId)
    {
        var secret = await _context.TenantIntegrationSecrets.FindAsync(secretId);
        if (secret == null) return false;

        _context.TenantIntegrationSecrets.Remove(secret);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<IntegrationJob>> GetJobsAsync(Guid tenantIntegrationId)
    {
        return await _context.IntegrationJobs
            .Where(j => j.TenantIntegrationId == tenantIntegrationId)
            .OrderBy(j => j.JobName)
            .ToListAsync();
    }

    public async Task<IntegrationJob?> GetJobByIdAsync(Guid id)
    {
        return await _context.IntegrationJobs
            .Include(j => j.TenantIntegration)
                .ThenInclude(ti => ti.IntegrationProvider)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IntegrationJob> CreateJobAsync(IntegrationJob job)
    {
        _context.IntegrationJobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<IntegrationJob> UpdateJobAsync(IntegrationJob job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.IntegrationJobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteJobAsync(Guid id)
    {
        var job = await _context.IntegrationJobs.FindAsync(id);
        if (job == null) return false;

        job.IsEnabled = false;
        job.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IntegrationRun> StartRunAsync(Guid jobId, string triggeredBy, bool isManual = false)
    {
        var job = await _context.IntegrationJobs
            .Include(j => j.TenantIntegration)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
            throw new InvalidOperationException("Job not found");

        var run = new IntegrationRun
        {
            IntegrationJobId = jobId,
            TenantId = job.TenantId,
            StartedAt = DateTime.UtcNow,
            Status = RunStatus.Running,
            TriggeredBy = triggeredBy,
            IsManual = isManual
        };

        _context.IntegrationRuns.Add(run);

        job.LastRunAt = DateTime.UtcNow;
        job.LastRunStatus = JobStatus.Running;

        job.TenantIntegration.LastSyncAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<IntegrationRun> CompleteRunAsync(Guid runId, RunStatus status, int successCount, int failureCount, string? errorSummary = null)
    {
        var run = await _context.IntegrationRuns
            .Include(r => r.IntegrationJob)
                .ThenInclude(j => j.TenantIntegration)
            .FirstOrDefaultAsync(r => r.Id == runId);

        if (run == null)
            throw new InvalidOperationException("Run not found");

        run.CompletedAt = DateTime.UtcNow;
        run.Status = status;
        run.SuccessCount = successCount;
        run.FailureCount = failureCount;
        run.ErrorSummary = errorSummary;
        run.DurationMs = (long)(run.CompletedAt.Value - run.StartedAt).TotalMilliseconds;

        var job = run.IntegrationJob;
        job.LastRunStatus = status == RunStatus.Completed ? JobStatus.Completed : JobStatus.Failed;

        var integration = job.TenantIntegration;
        if (status == RunStatus.Completed || status == RunStatus.PartialSuccess)
        {
            integration.LastSuccessAt = DateTime.UtcNow;
            integration.ConsecutiveFailures = 0;
        }
        else
        {
            integration.LastErrorAt = DateTime.UtcNow;
            integration.LastErrorMessage = errorSummary;
            integration.ConsecutiveFailures++;
        }

        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<IntegrationRunEvent> LogRunEventAsync(IntegrationRunEvent runEvent)
    {
        runEvent.EventTimestamp = DateTime.UtcNow;
        _context.IntegrationRunEvents.Add(runEvent);
        await _context.SaveChangesAsync();
        return runEvent;
    }

    public async Task<List<IntegrationRun>> GetRunHistoryAsync(Guid jobId, int limit = 50)
    {
        return await _context.IntegrationRuns
            .Where(r => r.IntegrationJobId == jobId)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task LogAuditAsync(Guid? tenantIntegrationId, string entityType, Guid entityId, string action, 
        string? before, string? after, string? actorId, string? actorName)
    {
        var tenantId = Guid.Empty;
        if (tenantIntegrationId.HasValue)
        {
            var integration = await _context.TenantIntegrations.FindAsync(tenantIntegrationId.Value);
            if (integration != null) tenantId = integration.TenantId;
        }

        var audit = new IntegrationAuditLog
        {
            TenantId = tenantId,
            TenantIntegrationId = tenantIntegrationId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            BeforeJson = before,
            AfterJson = after,
            ActorId = actorId,
            ActorName = actorName,
            ActionTimestamp = DateTime.UtcNow
        };

        _context.IntegrationAuditLogs.Add(audit);
        await _context.SaveChangesAsync();
    }

    private string EncryptValue(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    private string DecryptValue(string encryptedText)
    {
        var fullBytes = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        var iv = new byte[16];
        var encrypted = new byte[fullBytes.Length - 16];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(fullBytes, 16, encrypted, 0, encrypted.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
