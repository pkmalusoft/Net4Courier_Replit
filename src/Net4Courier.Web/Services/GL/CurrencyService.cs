using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.Services;
using Truebooks.Platform.Core.Infrastructure;

namespace Truebooks.Platform.Host.Services;

public class CurrencyService : ICurrencyService
{
    private readonly PlatformDbContext _context;

    public CurrencyService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<CurrencyDto>();

        return await _context.Currencies
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto(
                c.Id,
                c.TenantId,
                c.Code,
                c.Name,
                c.Symbol,
                c.IsBaseCurrency,
                c.IsActive
            ))
            .ToListAsync();
    }

    public async Task<CurrencyDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return null;

        return await _context.Currencies
            .Where(c => c.TenantId == tenantId && c.Id == id)
            .Select(c => new CurrencyDto(
                c.Id,
                c.TenantId,
                c.Code,
                c.Name,
                c.Symbol,
                c.IsBaseCurrency,
                c.IsActive
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyDto?> GetByCodeAsync(Guid tenantId, string code)
    {
        if (tenantId == Guid.Empty)
            return null;

        return await _context.Currencies
            .Where(c => c.TenantId == tenantId && c.Code == code)
            .Select(c => new CurrencyDto(
                c.Id,
                c.TenantId,
                c.Code,
                c.Name,
                c.Symbol,
                c.IsBaseCurrency,
                c.IsActive
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyDto?> GetBaseCurrencyAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        return await _context.Currencies
            .Where(c => c.TenantId == tenantId && c.IsBaseCurrency)
            .Select(c => new CurrencyDto(
                c.Id,
                c.TenantId,
                c.Code,
                c.Name,
                c.Symbol,
                c.IsBaseCurrency,
                c.IsActive
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyDto> CreateAsync(Guid tenantId, CreateCurrencyRequest request)
    {
        var entity = new Currency
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Currencies.Add(entity);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(tenantId, entity.Id))!;
    }

    public async Task<CurrencyDto> UpdateAsync(Guid tenantId, Guid id, UpdateCurrencyRequest request)
    {
        var entity = await _context.Currencies
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (entity == null)
            throw new InvalidOperationException("Currency not found");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Symbol = request.Symbol;
        entity.IsBaseCurrency = request.IsBaseCurrency;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(tenantId, entity.Id))!;
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid id)
    {
        var entity = await _context.Currencies
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (entity == null)
            return false;

        _context.Currencies.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetAsBaseCurrencyAsync(Guid tenantId, Guid id)
    {
        var currencies = await _context.Currencies
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var targetCurrency = currencies.FirstOrDefault(c => c.Id == id);
        if (targetCurrency == null)
            return false;

        foreach (var currency in currencies)
        {
            currency.IsBaseCurrency = currency.Id == id;
            currency.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetExchangeRateAsync(Guid tenantId, string fromCurrency, string toCurrency, DateTime? asOfDate = null)
    {
        if (fromCurrency == toCurrency)
            return 1m;

        var rate = await _context.ExchangeRates
            .Where(r => r.TenantId == tenantId && 
                        r.FromCurrency == fromCurrency && 
                        r.ToCurrency == toCurrency &&
                        (!asOfDate.HasValue || r.EffectiveDate <= asOfDate.Value))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        return rate?.Rate ?? 1m;
    }
}
