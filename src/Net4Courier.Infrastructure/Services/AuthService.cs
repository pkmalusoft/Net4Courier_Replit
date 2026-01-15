using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Infrastructure.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task SeedAdminUserAsync()
    {
        try
        {
            if (!await _context.Users.AnyAsync())
            {
                var adminRole = new Role
                {
                    Name = "Administrator",
                    Description = "Full system access",
                    IsActive = true
                };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();

                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Email = "admin@net4courier.com",
                    FullName = "System Administrator",
                    RoleId = adminRole.Id,
                    IsActive = true
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not seed admin user: {ex.Message}");
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
