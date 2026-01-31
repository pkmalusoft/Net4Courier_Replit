using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services.GL;

public class AccountClassificationService : IAccountClassificationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public AccountClassificationService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<AccountClassificationDto>> GetAllAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        return await context.GLAccountClassifications
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new AccountClassificationDto(
                LongToGuid(a.Id),
                tenantId,
                a.Name,
                a.Description,
                a.IsActive,
                a.CreatedAt,
                a.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<AccountClassificationDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLAccountClassifications
            .Where(a => a.Id == longId && !a.IsDeleted)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new AccountClassificationDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<AccountClassificationDto> CreateAsync(Guid tenantId, CreateAccountClassificationRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = new Net4Courier.Finance.Entities.GLAccountClassification
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        context.GLAccountClassifications.Add(entity);
        await context.SaveChangesAsync();

        return new AccountClassificationDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<AccountClassificationDto> UpdateAsync(Guid tenantId, Guid id, UpdateAccountClassificationRequest request)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLAccountClassifications
            .FirstOrDefaultAsync(a => a.Id == longId && !a.IsDeleted);

        if (entity == null)
            throw new InvalidOperationException("Account classification not found");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new AccountClassificationDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var entity = await context.GLAccountClassifications
            .FirstOrDefaultAsync(a => a.Id == longId && !a.IsDeleted);

        if (entity == null)
            return;

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
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
