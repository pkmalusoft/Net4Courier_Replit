using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.DTOs.Finance;

namespace Net4Courier.Web.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetAllAsync(Guid tenantId);
    Task<CurrencyDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<CurrencyDto?> GetByCodeAsync(Guid tenantId, string code);
    Task<CurrencyDto?> GetBaseCurrencyAsync(Guid tenantId);
    Task<CurrencyDto> CreateAsync(Guid tenantId, CreateCurrencyRequest request);
    Task<CurrencyDto> UpdateAsync(Guid tenantId, Guid id, UpdateCurrencyRequest request);
    Task<bool> DeleteAsync(Guid tenantId, Guid id);
    Task<bool> SetAsBaseCurrencyAsync(Guid tenantId, Guid id);
    Task<decimal> GetExchangeRateAsync(Guid tenantId, string fromCurrency, string toCurrency, DateTime? asOfDate = null);
}
