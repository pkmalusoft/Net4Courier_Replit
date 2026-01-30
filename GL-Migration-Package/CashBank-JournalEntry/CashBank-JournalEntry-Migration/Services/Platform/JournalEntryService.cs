using Truebooks.Platform.Contracts.DTOs.Finance;
using Truebooks.Platform.Contracts.Services;

namespace Truebooks.Platform.Host.Services;

public class JournalEntryService : IJournalEntryService
{
    public Task<IEnumerable<JournalEntryListDto>> GetAllAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
    {
        return Task.FromResult<IEnumerable<JournalEntryListDto>>(new List<JournalEntryListDto>());
    }

    public Task<JournalEntryDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        return Task.FromResult<JournalEntryDto?>(null);
    }

    public Task<JournalEntryDto?> CreateAsync(Guid tenantId, JournalEntryDto entry)
    {
        return Task.FromResult<JournalEntryDto?>(null);
    }

    public Task<JournalEntryDto?> UpdateAsync(Guid tenantId, Guid id, JournalEntryDto entry)
    {
        return Task.FromResult<JournalEntryDto?>(null);
    }

    public Task<bool> DeleteAsync(Guid tenantId, Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<bool> PostAsync(Guid tenantId, Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<bool> VoidAsync(Guid tenantId, Guid id, string reason)
    {
        return Task.FromResult(false);
    }

    public Task<bool> ReverseAsync(Guid tenantId, Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<string> GetNextEntryNumberAsync(Guid tenantId)
    {
        return Task.FromResult($"JV-{DateTime.Now:yyyyMMdd}-001");
    }
}
