using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.Services;
using Truebooks.Platform.Core.Infrastructure;
using CoreVoucherTransactionType = Truebooks.Platform.Core.Infrastructure.VoucherTransactionType;
using DtoVoucherTransactionType = Truebooks.Platform.Contracts.DTOs.VoucherTransactionType;

namespace Truebooks.Platform.Host.Services;

public class VoucherNumberingService : IVoucherNumberingService
{
    private readonly PlatformDbContext _context;

    public VoucherNumberingService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VoucherNumberingDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<VoucherNumberingDto>();

        var vouchers = await _context.VoucherNumberings
            .Where(v => v.TenantId == tenantId)
            .OrderBy(v => v.TransactionType)
            .Select(v => new VoucherNumberingDto(
                v.Id,
                v.TenantId,
                MapTransactionType(v.TransactionType),
                v.Prefix,
                v.NextNumber,
                v.NumberLength,
                v.Separator,
                v.IsLocked,
                v.IsActive,
                v.CreatedAt,
                v.UpdatedAt
            ))
            .ToListAsync();

        return vouchers;
    }

    public async Task<VoucherNumberingDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return null;

        var voucher = await _context.VoucherNumberings
            .Where(v => v.TenantId == tenantId && v.Id == id)
            .Select(v => new VoucherNumberingDto(
                v.Id,
                v.TenantId,
                MapTransactionType(v.TransactionType),
                v.Prefix,
                v.NextNumber,
                v.NumberLength,
                v.Separator,
                v.IsLocked,
                v.IsActive,
                v.CreatedAt,
                v.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        return voucher;
    }

    public async Task<VoucherNumberingDto> CreateAsync(Guid tenantId, CreateVoucherNumberingRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = new VoucherNumbering
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransactionType = MapToCoreTransactionType(request.TransactionType),
            Prefix = request.Prefix,
            NextNumber = request.NextNumber,
            NumberLength = request.NumberLength,
            Separator = request.Separator,
            IsLocked = false,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.VoucherNumberings.Add(entity);
        await _context.SaveChangesAsync();

        return new VoucherNumberingDto(
            entity.Id,
            entity.TenantId,
            MapTransactionType(entity.TransactionType),
            entity.Prefix,
            entity.NextNumber,
            entity.NumberLength,
            entity.Separator,
            entity.IsLocked,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<VoucherNumberingDto> UpdateAsync(Guid tenantId, Guid id, UpdateVoucherNumberingRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = await _context.VoucherNumberings
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.Id == id);

        if (entity == null)
            throw new InvalidOperationException("Voucher numbering not found");

        if (entity.IsLocked)
            throw new InvalidOperationException("This voucher numbering configuration is locked and cannot be modified");

        entity.Prefix = request.Prefix;
        entity.NextNumber = request.NextNumber;
        entity.NumberLength = request.NumberLength;
        entity.Separator = request.Separator;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new VoucherNumberingDto(
            entity.Id,
            entity.TenantId,
            MapTransactionType(entity.TransactionType),
            entity.Prefix,
            entity.NextNumber,
            entity.NumberLength,
            entity.Separator,
            entity.IsLocked,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return;

        var entity = await _context.VoucherNumberings
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.Id == id);

        if (entity == null)
            return;

        if (entity.IsLocked)
            throw new InvalidOperationException("This voucher numbering configuration is locked and cannot be deleted");

        _context.VoucherNumberings.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private static DtoVoucherTransactionType MapTransactionType(CoreVoucherTransactionType type)
    {
        return (DtoVoucherTransactionType)(int)type;
    }

    private static CoreVoucherTransactionType MapToCoreTransactionType(DtoVoucherTransactionType type)
    {
        return (CoreVoucherTransactionType)(int)type;
    }
}
