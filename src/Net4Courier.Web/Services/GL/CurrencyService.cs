using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services.GL;

public class CurrencyService : ICurrencyService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public CurrencyService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        return await context.Currencies
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto(
                LongToGuid(c.Id),
                tenantId,
                c.Code,
                c.Name,
                c.Symbol ?? "",
                c.IsBaseCurrency,
                c.IsActive
            ))
            .ToListAsync();
    }

    public async Task<CurrencyDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var longId = GuidToLong(id);
        
        var c = await context.Currencies
            .Where(c => c.Id == longId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (c == null) return null;

        return new CurrencyDto(
            LongToGuid(c.Id),
            tenantId,
            c.Code,
            c.Name,
            c.Symbol ?? "",
            c.IsBaseCurrency,
            c.IsActive
        );
    }

    public async Task<CurrencyDto?> GetByCodeAsync(Guid tenantId, string code)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        var c = await context.Currencies
            .Where(c => c.Code == code && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (c == null) return null;

        return new CurrencyDto(
            LongToGuid(c.Id),
            tenantId,
            c.Code,
            c.Name,
            c.Symbol ?? "",
            c.IsBaseCurrency,
            c.IsActive
        );
    }

    public async Task<CurrencyDto?> GetBaseCurrencyAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        var c = await context.Currencies
            .Where(c => c.IsBaseCurrency && c.IsActive && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (c == null) return null;

        return new CurrencyDto(
            LongToGuid(c.Id),
            tenantId,
            c.Code,
            c.Name,
            c.Symbol ?? "",
            c.IsBaseCurrency,
            c.IsActive
        );
    }

    public async Task<CurrencyDto> CreateAsync(Guid tenantId, CreateCurrencyRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        var entity = new Net4Courier.Masters.Entities.Currency
        {
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = request.IsActive,
            DecimalPlaces = 2,
            CreatedAt = DateTime.UtcNow
        };

        context.Currencies.Add(entity);
        await context.SaveChangesAsync();

        return (await GetByIdAsync(tenantId, LongToGuid(entity.Id)))!;
    }

    public async Task<CurrencyDto> UpdateAsync(Guid tenantId, Guid id, UpdateCurrencyRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var longId = GuidToLong(id);
        
        var entity = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == longId);

        if (entity == null)
            throw new InvalidOperationException("Currency not found");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Symbol = request.Symbol;
        entity.IsBaseCurrency = request.IsBaseCurrency;
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return (await GetByIdAsync(tenantId, LongToGuid(entity.Id)))!;
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid id)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var longId = GuidToLong(id);
        
        var entity = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == longId);

        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetAsBaseCurrencyAsync(Guid tenantId, Guid id)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var longId = GuidToLong(id);
        
        var currencies = await context.Currencies
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        var targetCurrency = currencies.FirstOrDefault(c => c.Id == longId);
        if (targetCurrency == null)
            return false;

        foreach (var currency in currencies)
        {
            currency.IsBaseCurrency = currency.Id == longId;
            currency.ModifiedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public Task<decimal> GetExchangeRateAsync(Guid tenantId, string fromCurrency, string toCurrency, DateTime? asOfDate = null)
    {
        if (fromCurrency == toCurrency)
            return Task.FromResult(1m);

        return Task.FromResult(1m);
    }

    private static Guid LongToGuid(long id)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private static long GuidToLong(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }
}
