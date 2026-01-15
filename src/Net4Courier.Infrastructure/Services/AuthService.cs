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
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            
            if (adminUser == null)
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
                
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        Name = "Administrator",
                        Description = "Full system access",
                        IsActive = true
                    };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                adminUser = new User
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
                Console.WriteLine("Admin user created successfully");
            }
            else if (string.IsNullOrEmpty(adminUser.PasswordHash) || !adminUser.PasswordHash.StartsWith("$2"))
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                await _context.SaveChangesAsync();
                Console.WriteLine("Admin password reset");
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
