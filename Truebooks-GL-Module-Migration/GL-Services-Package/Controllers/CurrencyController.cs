using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;

namespace Truebooks.Platform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly PlatformDbContext _db;

    public CurrencyController(PlatformDbContext db)
    {
        _db = db;
    }

    private Guid GetTenantId()
    {
        var tenantHeader = Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return Guid.TryParse(tenantHeader, out var tenantId) ? tenantId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<CurrencySettingsDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var tenantId = GetTenantId();
        var query = _db.Currencies.Where(c => c.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var currencies = await query
            .OrderByDescending(c => c.IsBaseCurrency)
            .ThenBy(c => c.Code)
            .Select(c => new CurrencySettingsDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                ExchangeRate = c.ExchangeRate,
                DecimalPlaces = c.DecimalPlaces,
                IsBaseCurrency = c.IsBaseCurrency,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(currencies);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<CurrencySettingsDto>>> GetActive()
    {
        var tenantId = GetTenantId();
        var currencies = await _db.Currencies
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderByDescending(c => c.IsBaseCurrency)
            .ThenBy(c => c.Code)
            .Select(c => new CurrencySettingsDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                ExchangeRate = c.ExchangeRate,
                DecimalPlaces = c.DecimalPlaces,
                IsBaseCurrency = c.IsBaseCurrency,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(currencies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencySettingsDto>> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (currency == null)
            return NotFound();

        return Ok(new CurrencySettingsDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            ExchangeRate = currency.ExchangeRate,
            DecimalPlaces = currency.DecimalPlaces,
            IsBaseCurrency = currency.IsBaseCurrency,
            IsActive = currency.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<CurrencySettingsDto>> Create([FromBody] CreateCurrencyRequest request)
    {
        var tenantId = GetTenantId();
        
        if (await _db.Currencies.AnyAsync(c => c.TenantId == tenantId && c.Code == request.Code))
            return BadRequest("Currency code already exists");

        if (request.IsBaseCurrency)
        {
            var existingBase = await _db.Currencies.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.IsBaseCurrency);
            if (existingBase != null)
            {
                existingBase.IsBaseCurrency = false;
            }
        }

        var currency = new Currency
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol ?? "",
            ExchangeRate = request.ExchangeRate,
            DecimalPlaces = request.DecimalPlaces,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = true
        };

        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = currency.Id }, new CurrencySettingsDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            ExchangeRate = currency.ExchangeRate,
            DecimalPlaces = currency.DecimalPlaces,
            IsBaseCurrency = currency.IsBaseCurrency,
            IsActive = currency.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyRequest request)
    {
        var tenantId = GetTenantId();
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (currency == null)
            return NotFound();

        if (await _db.Currencies.AnyAsync(c => c.TenantId == tenantId && c.Code == request.Code && c.Id != id))
            return BadRequest("Currency code already exists");

        if (request.IsBaseCurrency && !currency.IsBaseCurrency)
        {
            var existingBase = await _db.Currencies.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.IsBaseCurrency && c.Id != id);
            if (existingBase != null)
            {
                existingBase.IsBaseCurrency = false;
            }
        }

        currency.Code = request.Code;
        currency.Name = request.Name;
        currency.Symbol = request.Symbol ?? "";
        currency.ExchangeRate = request.ExchangeRate;
        currency.DecimalPlaces = request.DecimalPlaces;
        currency.IsBaseCurrency = request.IsBaseCurrency;
        currency.IsActive = request.IsActive;
        currency.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (currency == null)
            return NotFound();

        if (currency.IsBaseCurrency)
            return BadRequest("Cannot delete base currency");

        currency.IsActive = false;
        currency.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var tenantId = GetTenantId();
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (currency == null)
            return NotFound();

        currency.IsActive = true;
        currency.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/exchange-rate")]
    public async Task<ActionResult<ExchangeRateDto>> GetExchangeRate(Guid id)
    {
        var tenantId = GetTenantId();
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (currency == null)
            return NotFound();

        return Ok(new ExchangeRateDto
        {
            CurrencyId = currency.Id,
            CurrencyCode = currency.Code,
            ExchangeRate = currency.ExchangeRate,
            IsBaseCurrency = currency.IsBaseCurrency
        });
    }
}

public class ExchangeRateDto
{
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = "";
    public decimal ExchangeRate { get; set; }
    public bool IsBaseCurrency { get; set; }
}

public class CurrencySettingsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "";
    public decimal ExchangeRate { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCurrencyRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Symbol { get; set; }
    public decimal ExchangeRate { get; set; } = 1.0m;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; }
}

public class UpdateCurrencyRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Symbol { get; set; }
    public decimal ExchangeRate { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
}
