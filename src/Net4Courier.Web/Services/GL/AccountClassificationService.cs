using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Truebooks.Platform.Core.Infrastructure;

namespace Net4Courier.Web.Services.GL;

public class AccountClassificationService : IAccountClassificationService
{
    private readonly PlatformDbContext _context;

    public AccountClassificationService(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AccountClassificationDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<AccountClassificationDto>();

        return await _context.AccountClassifications
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountClassificationDto(
                a.Id,
                a.TenantId,
                a.Name,
                a.Description,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<AccountClassificationDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return null;

        var entity = await _context.AccountClassifications
            .Where(a => a.TenantId == tenantId && a.Id == id)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        return new AccountClassificationDto(
            entity.Id,
            entity.TenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<AccountClassificationDto> CreateAsync(Guid tenantId, CreateAccountClassificationRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = new AccountClassification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountClassifications.Add(entity);
        await _context.SaveChangesAsync();

        return new AccountClassificationDto(
            entity.Id,
            entity.TenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<AccountClassificationDto> UpdateAsync(Guid tenantId, Guid id, UpdateAccountClassificationRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        var entity = await _context.AccountClassifications
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (entity == null)
            throw new InvalidOperationException("Account classification not found");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AccountClassificationDto(
            entity.Id,
            entity.TenantId,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return;

        var entity = await _context.AccountClassifications
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (entity == null)
            return;

        _context.AccountClassifications.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
