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
        Console.WriteLine($"[AUTH DEBUG] Attempting login for username: '{username}'");
        
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Include(u => u.UserBranches)
                .ThenInclude(ub => ub.Branch)
                    .ThenInclude(b => b.Company)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
        {
            Console.WriteLine($"[AUTH DEBUG] User not found or not active for username: '{username}'");
            return null;
        }

        Console.WriteLine($"[AUTH DEBUG] User found: Id={user.Id}, Username='{user.Username}', IsActive={user.IsActive}");
        Console.WriteLine($"[AUTH DEBUG] PasswordHash length: {user.PasswordHash?.Length ?? 0}");
        Console.WriteLine($"[AUTH DEBUG] PasswordHash starts with: {(user.PasswordHash?.Length > 10 ? user.PasswordHash.Substring(0, 10) : "null/short")}");
        
        try
        {
            var verifyResult = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            Console.WriteLine($"[AUTH DEBUG] BCrypt.Verify result: {verifyResult}");
            
            if (!verifyResult)
                return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH DEBUG] BCrypt.Verify EXCEPTION: {ex.Message}");
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        Console.WriteLine($"[AUTH DEBUG] Login successful for user: {user.Username}");

        return user;
    }
    
    public async Task<List<Branch>> GetUserBranchesAsync(long userId)
    {
        var userBranches = await _context.UserBranches
            .Include(ub => ub.Branch)
                .ThenInclude(b => b.Company)
            .Include(ub => ub.Branch)
                .ThenInclude(b => b.Currency)
            .Where(ub => ub.UserId == userId && ub.Branch.IsActive && !ub.IsDeleted)
            .Select(ub => ub.Branch)
            .ToListAsync();
            
        if (!userBranches.Any())
        {
            var user = await _context.Users
                .Include(u => u.Branch)
                    .ThenInclude(b => b!.Company)
                .Include(u => u.Branch)
                    .ThenInclude(b => b!.Currency)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Branch != null)
            {
                userBranches.Add(user.Branch);
            }
        }
        
        return userBranches;
    }
    
    public async Task<Branch?> GetBranchWithCompanyAsync(long branchId)
    {
        return await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId);
    }

    public async Task SeedAdminUserAsync()
    {
        try
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
                Console.WriteLine("Administrator role created");
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin" || u.RoleId == adminRole.Id);
            
            if (adminUser != null)
            {
                var hasUserBranch = await _context.UserBranches.AnyAsync(ub => ub.UserId == adminUser.Id);
                if (!hasUserBranch)
                {
                    var defaultBranch = await _context.Branches.FirstOrDefaultAsync(b => b.IsActive);
                    if (defaultBranch != null)
                    {
                        var userBranch = new UserBranch
                        {
                            UserId = adminUser.Id,
                            BranchId = defaultBranch.Id,
                            IsDefault = true
                        };
                        _context.UserBranches.Add(userBranch);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Admin assigned to branch: {defaultBranch.Name}");
                    }
                }
            }
            
            await SeedPlatformAdminAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not seed admin role: {ex.Message}");
        }
    }
    
    public async Task SeedPlatformAdminAsync()
    {
        try
        {
            var platformAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "PlatformAdmin");
            
            if (platformAdminRole == null)
            {
                platformAdminRole = new Role
                {
                    Name = "PlatformAdmin",
                    Description = "Platform administrator with access to tenant management and demo data",
                    IsActive = true
                };
                _context.Roles.Add(platformAdminRole);
                await _context.SaveChangesAsync();
                Console.WriteLine("PlatformAdmin role created");
            }
            
            var platformAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "platformadmin");
            
            if (platformAdminUser == null)
            {
                var defaultBranch = await _context.Branches.FirstOrDefaultAsync(b => b.IsActive);
                
                // Use SETUP_KEY environment variable for initial password, fallback to a generated secure password
                var setupKey = Environment.GetEnvironmentVariable("SETUP_KEY");
                var initialPassword = !string.IsNullOrEmpty(setupKey) ? setupKey : Guid.NewGuid().ToString("N")[..12];
                
                platformAdminUser = new User
                {
                    Username = "platformadmin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
                    Email = "platformadmin@net4courier.com",
                    FullName = "Platform Administrator",
                    Phone = "+971-000-0000",
                    RoleId = platformAdminRole.Id,
                    BranchId = defaultBranch?.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(platformAdminUser);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Platform admin user created (username: platformadmin, password: {(string.IsNullOrEmpty(setupKey) ? "check SETUP_KEY or use generated password" : "uses SETUP_KEY")})");
                
                if (defaultBranch != null)
                {
                    var userBranch = new UserBranch
                    {
                        UserId = platformAdminUser.Id,
                        BranchId = defaultBranch.Id,
                        IsDefault = true
                    };
                    _context.UserBranches.Add(userBranch);
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not seed platform admin: {ex.Message}");
        }
    }
    
    public async Task<bool> IsPlatformAdminAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        return user?.Role?.Name == "PlatformAdmin";
    }
    
    public async Task<bool> IsPlatformAdminByUsernameAsync(string username)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
        
        return user?.Role?.Name == "PlatformAdmin";
    }
    
    public async Task<string?> GetUserRoleByUsernameAsync(string username)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
        
        return user?.Role?.Name;
    }
    
    public async Task<bool> IsAdministratorByUsernameAsync(string username)
    {
        var roleName = await GetUserRoleByUsernameAsync(username);
        return roleName == "Administrator" || roleName == "PlatformAdmin";
    }
    
    public async Task<bool> IsCourierByUsernameAsync(string username)
    {
        var roleName = await GetUserRoleByUsernameAsync(username);
        return roleName == "Courier";
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
