using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Infrastructure.Services;

public interface ISetupService
{
    Task<bool> IsSetupRequiredAsync();
    Task<bool> ValidateSetupKeyAsync(string setupKey);
    Task<(bool Success, string Message)> CreateInitialAdminAsync(string username, string email, string fullName, string password);
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

    public async Task<(bool Success, string Message)> CreateInitialAdminAsync(string username, string email, string fullName, string password)
    {
        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Username == username);
            if (existingUser)
            {
                return (false, "A user with this username already exists");
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
                var userBranch = new UserBranch
                {
                    UserId = adminUser.Id,
                    BranchId = defaultBranch.Id,
                    IsDefault = true
                };
                _context.UserBranches.Add(userBranch);
                await _context.SaveChangesAsync();
            }

            return (true, "Administrator account created successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating administrator: {ex.Message}");
        }
    }
}
