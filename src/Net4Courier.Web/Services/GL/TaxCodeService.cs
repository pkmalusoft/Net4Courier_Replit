using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;
using DtoTaxType = Truebooks.Platform.Contracts.DTOs.TaxType;

namespace Net4Courier.Web.Services.GL;

public class TaxCodeService : ITaxCodeService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public TaxCodeService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<TaxCodeDto>> GetAllAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        return await context.GLTaxCodes
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Code)
            .Select(t => new TaxCodeDto(
                LongToGuid(t.Id),
                tenantId,
                t.Code,
                t.Description ?? "",
                t.Rate,
                MapTaxType(t.TaxType),
                t.IsActive,
                t.CreatedAt,
                t.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<TaxCodeDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLTaxCodes
            .Where(t => t.Id == longId && !t.IsDeleted)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new TaxCodeDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Code,
            entity.Description ?? "",
            entity.Rate,
            MapTaxType(entity.TaxType),
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<TaxCodeDto> CreateAsync(Guid tenantId, CreateTaxCodeRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = new Net4Courier.Finance.Entities.GLTaxCode
        {
            Code = request.Code,
            Description = request.Description,
            Rate = request.Rate,
            TaxType = request.TaxType.ToString(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        context.GLTaxCodes.Add(entity);
        await context.SaveChangesAsync();

        return new TaxCodeDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Code,
            entity.Description ?? "",
            entity.Rate,
            MapTaxType(entity.TaxType),
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<TaxCodeDto> UpdateAsync(Guid tenantId, Guid id, UpdateTaxCodeRequest request)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLTaxCodes
            .FirstOrDefaultAsync(t => t.Id == longId && !t.IsDeleted);

        if (entity == null)
            throw new InvalidOperationException("Tax code not found");

        entity.Code = request.Code;
        entity.Description = request.Description;
        entity.Rate = request.Rate;
        entity.TaxType = request.TaxType.ToString();
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new TaxCodeDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Code,
            entity.Description ?? "",
            entity.Rate,
            MapTaxType(entity.TaxType),
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLTaxCodes
            .FirstOrDefaultAsync(t => t.Id == longId && !t.IsDeleted);

        if (entity == null)
            return;

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
    }

    private static DtoTaxType MapTaxType(string? taxType)
    {
        return taxType switch
        {
            "VAT" => DtoTaxType.VAT,
            "GST" => DtoTaxType.GST,
            "USSalesTax" => DtoTaxType.USSalesTax,
            _ => DtoTaxType.Simple
        };
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
