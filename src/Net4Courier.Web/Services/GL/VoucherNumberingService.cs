using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;
using DtoVoucherTransactionType = Truebooks.Platform.Contracts.DTOs.VoucherTransactionType;

namespace Net4Courier.Web.Services.GL;

public class VoucherNumberingService : IVoucherNumberingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public VoucherNumberingService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<VoucherNumberingDto>> GetAllAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        return await context.GLVoucherNumberings
            .Where(v => !v.IsDeleted)
            .OrderBy(v => v.TransactionType)
            .Select(v => new VoucherNumberingDto(
                LongToGuid(v.Id),
                tenantId,
                MapTransactionType(v.TransactionType),
                v.Prefix ?? "",
                v.NextNumber,
                v.NumberLength,
                v.Separator ?? "-",
                v.IsLocked,
                v.IsActive,
                v.CreatedAt,
                v.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<VoucherNumberingDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLVoucherNumberings
            .Where(v => v.Id == longId && !v.IsDeleted)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new VoucherNumberingDto(
            LongToGuid(entity.Id),
            tenantId,
            MapTransactionType(entity.TransactionType),
            entity.Prefix ?? "",
            entity.NextNumber,
            entity.NumberLength,
            entity.Separator ?? "-",
            entity.IsLocked,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<VoucherNumberingDto> CreateAsync(Guid tenantId, CreateVoucherNumberingRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = new Net4Courier.Finance.Entities.GLVoucherNumbering
        {
            TransactionType = request.TransactionType.ToString(),
            Prefix = request.Prefix,
            NextNumber = request.NextNumber,
            NumberLength = request.NumberLength,
            Separator = request.Separator,
            IsLocked = false,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        context.GLVoucherNumberings.Add(entity);
        await context.SaveChangesAsync();

        return new VoucherNumberingDto(
            LongToGuid(entity.Id),
            tenantId,
            MapTransactionType(entity.TransactionType),
            entity.Prefix ?? "",
            entity.NextNumber,
            entity.NumberLength,
            entity.Separator ?? "-",
            entity.IsLocked,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<VoucherNumberingDto> UpdateAsync(Guid tenantId, Guid id, UpdateVoucherNumberingRequest request)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLVoucherNumberings
            .FirstOrDefaultAsync(v => v.Id == longId && !v.IsDeleted);

        if (entity == null)
            throw new InvalidOperationException("Voucher numbering not found");

        if (entity.IsLocked)
            throw new InvalidOperationException("This voucher numbering configuration is locked and cannot be modified");

        entity.Prefix = request.Prefix;
        entity.NextNumber = request.NextNumber;
        entity.NumberLength = request.NumberLength;
        entity.Separator = request.Separator;
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new VoucherNumberingDto(
            LongToGuid(entity.Id),
            tenantId,
            MapTransactionType(entity.TransactionType),
            entity.Prefix ?? "",
            entity.NextNumber,
            entity.NumberLength,
            entity.Separator ?? "-",
            entity.IsLocked,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLVoucherNumberings
            .FirstOrDefaultAsync(v => v.Id == longId && !v.IsDeleted);

        if (entity == null)
            return;

        if (entity.IsLocked)
            throw new InvalidOperationException("This voucher numbering configuration is locked and cannot be deleted");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
    }

    private static DtoVoucherTransactionType MapTransactionType(string? type)
    {
        if (Enum.TryParse<DtoVoucherTransactionType>(type, true, out var result))
            return result;
        return 0;
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
