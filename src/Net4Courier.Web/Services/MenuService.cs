using Microsoft.EntityFrameworkCore;
using Net4Courier.Shared.Data;
using Net4Courier.Shared.Entities;

namespace Net4Courier.Web.Services;

public class MenuService
{
    private readonly ApplicationDbContext _context;

    public MenuService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Menu>> GetMenuTreeAsync()
    {
        return await _context.Menus
            .Where(m => m.IsActive && m.ParentId == null)
            .Include(m => m.Children.Where(c => c.IsActive))
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<List<Menu>> GetUserMenuAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<Menu>();

        var permissions = user.Role.Permissions.Where(p => p.CanView).Select(p => p.ModuleName).ToList();

        if (user.Role.Name == "Administrator")
        {
            return await GetMenuTreeAsync();
        }

        return await _context.Menus
            .Where(m => m.IsActive && m.ParentId == null && permissions.Contains(m.ModuleName))
            .Include(m => m.Children.Where(c => c.IsActive && permissions.Contains(c.ModuleName)))
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }
}
