using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(long? userId, string featureCode);
    Task<HashSet<string>> GetUserPermissionsAsync(long? userId);
    Task<List<FeaturePermission>> GetRolePermissionsAsync(long roleId);
    Task SetRolePermissionAsync(long roleId, string featureCode, bool isGranted);
}

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<long, HashSet<string>> _permissionCache = new();

    public PermissionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasPermissionAsync(long? userId, string featureCode)
    {
        if (userId == null) return false;
        
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(featureCode);
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(long? userId)
    {
        if (userId == null) return new HashSet<string>();

        var user = await _dbContext.Users
            .Include(u => u.Role)
            .ThenInclude(r => r!.FeaturePermissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Role == null) return new HashSet<string>();

        var permissions = new HashSet<string>();
        
        if (user.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            user.Role.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var code in FeatureCodes.All)
            {
                permissions.Add(code);
            }
        }
        else
        {
            foreach (var fp in user.Role.FeaturePermissions.Where(p => p.IsGranted))
            {
                permissions.Add(fp.FeatureCode);
            }
        }

        return permissions;
    }

    public async Task<List<FeaturePermission>> GetRolePermissionsAsync(long roleId)
    {
        return await _dbContext.FeaturePermissions
            .Where(fp => fp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task SetRolePermissionAsync(long roleId, string featureCode, bool isGranted)
    {
        var existing = await _dbContext.FeaturePermissions
            .FirstOrDefaultAsync(fp => fp.RoleId == roleId && fp.FeatureCode == featureCode);

        if (existing != null)
        {
            existing.IsGranted = isGranted;
            existing.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            _dbContext.FeaturePermissions.Add(new FeaturePermission
            {
                RoleId = roleId,
                FeatureCode = featureCode,
                IsGranted = isGranted,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
