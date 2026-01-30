using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Truebooks.Platform.Core.Infrastructure;
using CoreTaxType = Truebooks.Platform.Core.Infrastructure.TaxType;
using DtoTaxType = Truebooks.Platform.Contracts.DTOs.TaxType;

namespace Net4Courier.Web.Services.GL;

public class TaxCodeService : ITaxCodeService
{
    private readonly PlatformDbContext _context;

    public TaxCodeService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaxCodeDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<TaxCodeDto>();

        var taxCodes = await _context.TaxCodes
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Code)
            .Select(t => new TaxCodeDto(
                t.Id,
                t.TenantId,
                t.Code,
                t.Description ?? string.Empty,
                t.Rate,
                MapTaxType(t.TaxType),
                t.IsActive,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync();

        return taxCodes;
    }

    public async Task<TaxCodeDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return null;

        var taxCode = await _context.TaxCodes
            .Where(t => t.TenantId == tenantId && t.Id == id)
            .Select(t => new TaxCodeDto(
                t.Id,
                t.TenantId,
                t.Code,
                t.Description ?? string.Empty,
                t.Rate,
                MapTaxType(t.TaxType),
                t.IsActive,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        return taxCode;
    }

    public async Task<TaxCodeDto> CreateAsync(Guid tenantId, CreateTaxCodeRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = new TaxCode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Description = request.Description,
            Rate = request.Rate,
            TaxType = MapToCoreTaxType(request.TaxType),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaxCodes.Add(entity);
        await _context.SaveChangesAsync();

        return new TaxCodeDto(
            entity.Id,
            entity.TenantId,
            entity.Code,
            entity.Description ?? string.Empty,
            entity.Rate,
            MapTaxType(entity.TaxType),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<TaxCodeDto> UpdateAsync(Guid tenantId, Guid id, UpdateTaxCodeRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = await _context.TaxCodes
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (entity == null)
            throw new InvalidOperationException("Tax code not found");

        entity.Code = request.Code;
        entity.Description = request.Description;
        entity.Rate = request.Rate;
        entity.TaxType = MapToCoreTaxType(request.TaxType);
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TaxCodeDto(
            entity.Id,
            entity.TenantId,
            entity.Code,
            entity.Description ?? string.Empty,
            entity.Rate,
            MapTaxType(entity.TaxType),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return;

        var entity = await _context.TaxCodes
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (entity == null)
            return;

        _context.TaxCodes.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private static DtoTaxType MapTaxType(CoreTaxType taxType)
    {
        return taxType switch
        {
            CoreTaxType.Simple => DtoTaxType.Simple,
            CoreTaxType.VAT => DtoTaxType.VAT,
            CoreTaxType.GST => DtoTaxType.GST,
            _ => DtoTaxType.Simple
        };
    }

    private static CoreTaxType MapToCoreTaxType(DtoTaxType taxType)
    {
        return taxType switch
        {
            DtoTaxType.Simple => CoreTaxType.Simple,
            DtoTaxType.VAT => CoreTaxType.VAT,
            DtoTaxType.GST => CoreTaxType.GST,
            DtoTaxType.USSalesTax => CoreTaxType.Simple,
            _ => CoreTaxType.Simple
        };
    }
}
