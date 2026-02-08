using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Infrastructure.Services;

public interface IAdminEmailNotifier
{
    Task<bool> SendAdminCredentialsEmailAsync(string toEmail, string fullName, string username, string password, string loginUrl);
}

public interface ISetupService
{
    Task<bool> IsSetupRequiredAsync();
    Task<bool> IsSetupEnabledAsync();
    Task<bool> ValidateSetupKeyAsync(string setupKey);
    Task<(bool Success, string Message, bool IsUpdate)> CreateOrUpdateAdminAsync(string setupKey, string username, string email, string fullName, string password);
    Task<(bool Success, string Message)> CreateInitialAdminAsync(string setupKey, string username, string email, string fullName, string password);
    Task<(bool Success, string Message)> ResetPasswordAsync(string setupKey, string username, string newPassword);
    Task<List<(long Id, string Username, string FullName, string? RoleName, bool IsActive)>> GetUsersAsync(string setupKey);
}

public class SetupService : ISetupService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SetupService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<bool> IsSetupRequiredAsync()
    {
        try
        {
            var hasAnyUser = await _context.Users.AnyAsync(u => u.IsActive && !u.IsDeleted);
            return !hasAnyUser;
        }
        catch
        {
            return true;
        }
    }

    public Task<bool> ValidateSetupKeyAsync(string setupKey)
    {
        var configuredKey = _configuration["SETUP_KEY"] ?? Environment.GetEnvironmentVariable("SETUP_KEY");
        
        if (string.IsNullOrEmpty(configuredKey))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(setupKey == configuredKey);
    }

    public async Task<(bool Success, string Message, bool IsUpdate)> CreateOrUpdateAdminAsync(string setupKey, string username, string email, string fullName, string password)
    {
        try
        {
            var isValidKey = await ValidateSetupKeyAsync(setupKey);
            if (!isValidKey)
            {
                return (false, "Invalid setup key. Administrator creation denied.", false);
            }

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

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
            
            if (existingUser != null)
            {
                existingUser.Email = email;
                existingUser.FullName = fullName;
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                existingUser.RoleId = adminRole.Id;
                existingUser.IsActive = true;
                existingUser.ModifiedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return (true, "Admin user have been successfully updated and an email has been sent to your admin mail id", true);
            }
            else
            {
                var adminUser = new User
                {
                    Username = username,
                    Email = email,
                    FullName = fullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                var defaultBranch = await _context.Branches.FirstOrDefaultAsync(b => b.IsActive);
                if (defaultBranch != null)
                {
                    var existingUserBranch = await _context.UserBranches.AnyAsync(ub => ub.UserId == adminUser.Id && ub.BranchId == defaultBranch.Id);
                    if (!existingUserBranch)
                    {
                        var userBranch = new UserBranch
                        {
                            UserId = adminUser.Id,
                            BranchId = defaultBranch.Id,
                            IsDefault = true
                        };
                        _context.UserBranches.Add(userBranch);
                        await _context.SaveChangesAsync();
                    }
                }

                return (true, "Admin user have been successfully created and an email has been sent to your admin mail id", false);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error creating/updating administrator: {ex.Message}", false);
        }
    }

    public async Task<(bool Success, string Message)> CreateInitialAdminAsync(string setupKey, string username, string email, string fullName, string password)
    {
        var result = await CreateOrUpdateAdminAsync(setupKey, username, email, fullName, password);
        return (result.Success, result.Message);
    }

    public Task<bool> IsSetupEnabledAsync()
    {
        var configuredKey = _configuration["SETUP_KEY"] ?? Environment.GetEnvironmentVariable("SETUP_KEY");
        return Task.FromResult(!string.IsNullOrEmpty(configuredKey));
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string setupKey, string username, string newPassword)
    {
        try
        {
            var isValidKey = await ValidateSetupKeyAsync(setupKey);
            if (!isValidKey)
            {
                return (false, "Invalid setup key. Password reset denied.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
            if (user == null)
            {
                return (false, $"User '{username}' not found or is deleted");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, $"Password reset successfully for user '{username}'");
        }
        catch (Exception ex)
        {
            return (false, $"Error resetting password: {ex.Message}");
        }
    }

    public async Task<List<(long Id, string Username, string FullName, string? RoleName, bool IsActive)>> GetUsersAsync(string setupKey)
    {
        var isValidKey = await ValidateSetupKeyAsync(setupKey);
        if (!isValidKey)
        {
            return new List<(long, string, string, string?, bool)>();
        }

        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .Select(u => new { u.Id, u.Username, u.FullName, RoleName = u.Role != null ? u.Role.Name : null, u.IsActive })
            .ToListAsync();

        return users.Select(u => (u.Id, u.Username, u.FullName ?? "", u.RoleName, u.IsActive)).ToList();
    }
}
