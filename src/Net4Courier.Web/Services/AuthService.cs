using Microsoft.EntityFrameworkCore;
using Net4Courier.Shared.Data;
using Net4Courier.Shared.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Net4Courier.Web.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.DefaultBranch)
            .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);

        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        user.IsLoggedIn = true;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsLoggedIn = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .Include(u => u.DefaultBranch)
            .ThenInclude(b => b!.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> CreateUserAsync(string username, string password, string? email, string? phone, int roleId, int? branchId)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == username))
            return null;

        var user = new User
        {
            UserName = username,
            PasswordHash = HashPassword(password),
            Email = email,
            Phone = phone,
            RoleId = roleId,
            DefaultBranchId = branchId,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task SeedAdminUserAsync()
    {
        if (!await _context.Users.AnyAsync())
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
            if (adminRole != null)
            {
                await CreateUserAsync("admin", "Admin@123", "admin@net4courier.com", null, adminRole.Id, null);
            }
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
