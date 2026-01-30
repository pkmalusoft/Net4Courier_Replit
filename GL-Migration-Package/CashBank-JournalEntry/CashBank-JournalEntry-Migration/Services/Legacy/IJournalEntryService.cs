using Truebooks.Platform.Contracts.Legacy.DTOs;

namespace Truebooks.Shared.UI.Services;

public interface IJournalEntryService
{
    Task<List<JournalEntryDto>> GetAllAsync();
    Task<JournalEntryDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(JournalEntryDto entry);
    Task UpdateAsync(Guid id, JournalEntryDto entry);
    Task DeleteAsync(Guid id);
    Task<string> GetNextEntryNumberAsync();
    Task PostAsync(Guid id);
    Task ReverseAsync(Guid id);
}
