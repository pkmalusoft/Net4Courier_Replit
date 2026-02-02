using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class FavoriteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public FavoriteService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<UserFavorite>> GetUserFavoritesAsync(long userId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.UserFavorites
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderBy(f => f.Category)
            .ThenBy(f => f.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> IsFavoriteAsync(long userId, string menuCode)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.UserFavorites
            .AnyAsync(f => f.UserId == userId && f.MenuCode == menuCode && !f.IsDeleted);
    }

    public async Task<UserFavorite> AddFavoriteAsync(long userId, string menuCode, string menuName, string? menuIcon, string? route, string? category)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        
        var existing = await db.UserFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.MenuCode == menuCode);
        
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.ModifiedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return existing;
        }

        var maxOrder = await db.UserFavorites
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .MaxAsync(f => (int?)f.DisplayOrder) ?? 0;

        var favorite = new UserFavorite
        {
            UserId = userId,
            MenuCode = menuCode,
            MenuName = menuName,
            MenuIcon = menuIcon,
            Route = route,
            Category = category,
            DisplayOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        db.UserFavorites.Add(favorite);
        await db.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> RemoveFavoriteAsync(long userId, string menuCode)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var favorite = await db.UserFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.MenuCode == menuCode && !f.IsDeleted);
        
        if (favorite != null)
        {
            favorite.IsDeleted = true;
            favorite.ModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> RemoveFavoriteByIdAsync(long favoriteId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var favorite = await db.UserFavorites.FindAsync(favoriteId);
        
        if (favorite != null)
        {
            favorite.IsDeleted = true;
            favorite.ModifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
