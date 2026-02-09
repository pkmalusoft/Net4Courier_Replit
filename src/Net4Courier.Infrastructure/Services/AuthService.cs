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
            Console.WriteLine("SeedPlatformAdminAsync: Starting platform admin seeding...");
            
            var setupKey = Environment.GetEnvironmentVariable("SETUP_KEY");
            var platformAdminPassword = Environment.GetEnvironmentVariable("PLATFORMADMIN_PASSWORD");
            var effectivePassword = !string.IsNullOrEmpty(platformAdminPassword) ? platformAdminPassword : setupKey;
            var initialPassword = !string.IsNullOrEmpty(effectivePassword) ? effectivePassword : "Admin@123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword);
            
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            
            using var checkRoleCmd = conn.CreateCommand();
            checkRoleCmd.CommandText = "SELECT \"Id\" FROM \"Roles\" WHERE \"Name\" = 'PlatformAdmin' LIMIT 1";
            var roleIdObj = await checkRoleCmd.ExecuteScalarAsync();
            long roleId;
            
            if (roleIdObj == null)
            {
                using var createRoleCmd = conn.CreateCommand();
                createRoleCmd.CommandText = @"INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsActive"", ""CreatedAt"", ""IsDeleted"", ""IsDemo"") 
                    VALUES ('PlatformAdmin', 'Platform administrator with access to tenant management and demo data', TRUE, NOW(), FALSE, FALSE) 
                    RETURNING ""Id""";
                roleId = Convert.ToInt64(await createRoleCmd.ExecuteScalarAsync());
                Console.WriteLine($"SeedPlatformAdminAsync: PlatformAdmin role created with Id={roleId}");
            }
            else
            {
                roleId = Convert.ToInt64(roleIdObj);
                Console.WriteLine($"SeedPlatformAdminAsync: PlatformAdmin role already exists with Id={roleId}");
            }
            
            using var checkUserCmd = conn.CreateCommand();
            checkUserCmd.CommandText = "SELECT \"Id\" FROM \"Users\" WHERE \"Username\" = 'platformadmin' LIMIT 1";
            var userIdObj = await checkUserCmd.ExecuteScalarAsync();
            
            if (userIdObj == null)
            {
                using var createUserCmd = conn.CreateCommand();
                createUserCmd.CommandText = @"INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Email"", ""FullName"", ""Phone"", ""RoleId"", ""IsActive"", ""CreatedAt"", ""IsDeleted"", ""IsDemo"") 
                    VALUES ('platformadmin', @hash, 'platformadmin@net4courier.com', 'Platform Administrator', '+971-000-0000', @roleId, TRUE, NOW(), FALSE, FALSE) 
                    RETURNING ""Id""";
                var hashParam = createUserCmd.CreateParameter();
                hashParam.ParameterName = "@hash";
                hashParam.Value = passwordHash;
                createUserCmd.Parameters.Add(hashParam);
                var roleParam = createUserCmd.CreateParameter();
                roleParam.ParameterName = "@roleId";
                roleParam.Value = roleId;
                createUserCmd.Parameters.Add(roleParam);
                var userId = Convert.ToInt64(await createUserCmd.ExecuteScalarAsync());
                Console.WriteLine($"SeedPlatformAdminAsync: platformadmin user created with Id={userId}, password source: {(!string.IsNullOrEmpty(platformAdminPassword) ? "PLATFORMADMIN_PASSWORD" : !string.IsNullOrEmpty(setupKey) ? "SETUP_KEY" : "default")}");
            }
            else
            {
                var userId = Convert.ToInt64(userIdObj);
                Console.WriteLine($"SeedPlatformAdminAsync: platformadmin user already exists with Id={userId}");
                if (!string.IsNullOrEmpty(effectivePassword))
                {
                    using var updateCmd = conn.CreateCommand();
                    updateCmd.CommandText = @"UPDATE ""Users"" SET ""PasswordHash"" = @hash, ""IsActive"" = TRUE, ""RoleId"" = @roleId, ""ModifiedAt"" = NOW() WHERE ""Id"" = @userId";
                    var hashParam = updateCmd.CreateParameter();
                    hashParam.ParameterName = "@hash";
                    hashParam.Value = passwordHash;
                    updateCmd.Parameters.Add(hashParam);
                    var roleParam = updateCmd.CreateParameter();
                    roleParam.ParameterName = "@roleId";
                    roleParam.Value = roleId;
                    updateCmd.Parameters.Add(roleParam);
                    var userIdParam = updateCmd.CreateParameter();
                    userIdParam.ParameterName = "@userId";
                    userIdParam.Value = userId;
                    updateCmd.Parameters.Add(userIdParam);
                    await updateCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"SeedPlatformAdminAsync: platformadmin password synced from {(!string.IsNullOrEmpty(platformAdminPassword) ? "PLATFORMADMIN_PASSWORD" : "SETUP_KEY")}");
                }
            }
            
            Console.WriteLine("SeedPlatformAdminAsync: Completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR SeedPlatformAdminAsync: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"ERROR SeedPlatformAdminAsync StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Console.WriteLine($"ERROR SeedPlatformAdminAsync Inner: {ex.InnerException.Message}");
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
