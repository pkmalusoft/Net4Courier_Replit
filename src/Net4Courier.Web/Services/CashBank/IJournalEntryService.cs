using Net4Courier.Web.DTOs;

namespace Net4Courier.Web.Services.CashBank;

public interface IJournalEntryService
{
    Task<IEnumerable<JournalEntryListDto>> GetAllAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null, string? status = null);
    Task<JournalEntryDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<JournalEntryDto?> CreateAsync(Guid tenantId, JournalEntryDto entry);
    Task<JournalEntryDto?> UpdateAsync(Guid tenantId, Guid id, JournalEntryDto entry);
    Task<bool> DeleteAsync(Guid tenantId, Guid id);
    Task<bool> PostAsync(Guid tenantId, Guid id);
    Task<bool> VoidAsync(Guid tenantId, Guid id, string reason);
    Task<bool> ReverseAsync(Guid tenantId, Guid id);
    Task<string> GetNextEntryNumberAsync(Guid tenantId);
}
