using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class AWBNumberService
{
    private readonly ApplicationDbContext _context;

    public AWBNumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAWBNumber(long branchId, MovementType movementType)
    {
        var config = await _context.BranchAWBConfigs
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.MovementType == movementType && !c.IsDeleted);

        if (config != null)
        {
            long nextNumber;
            if (config.LastUsedNumber == 0 || config.LastUsedNumber < config.StartingNumber)
            {
                nextNumber = config.StartingNumber;
            }
            else
            {
                nextNumber = config.LastUsedNumber + config.IncrementBy;
            }

            config.LastUsedNumber = nextNumber;
            config.ModifiedAt = DateTime.UtcNow;

            return $"{config.AWBPrefix}{nextNumber}";
        }

        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new InvalidOperationException($"Branch with ID {branchId} not found");
        }

        long fallbackNumber;
        if (branch.AWBLastUsedNumber == 0)
        {
            fallbackNumber = branch.AWBStartingNumber;
        }
        else
        {
            fallbackNumber = branch.AWBLastUsedNumber + branch.AWBIncrement;
        }

        branch.AWBLastUsedNumber = fallbackNumber;
        branch.ModifiedAt = DateTime.UtcNow;

        var prefix = branch.AWBPrefix ?? "";
        return $"{prefix}{fallbackNumber}";
    }

    public async Task<string> GenerateNextAWBNumber(long branchId)
    {
        return await GenerateNextAWBNumber(branchId, MovementType.Domestic);
    }

    public async Task<string> PreviewNextAWBNumber(long branchId, MovementType movementType)
    {
        var config = await _context.BranchAWBConfigs
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.MovementType == movementType && !c.IsDeleted);

        if (config != null)
        {
            long nextNumber;
            if (config.LastUsedNumber == 0 || config.LastUsedNumber < config.StartingNumber)
            {
                nextNumber = config.StartingNumber;
            }
            else
            {
                nextNumber = config.LastUsedNumber + config.IncrementBy;
            }

            return $"{config.AWBPrefix}{nextNumber}";
        }

        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            return "N/A";
        }

        long fallbackNumber;
        if (branch.AWBLastUsedNumber == 0)
        {
            fallbackNumber = branch.AWBStartingNumber;
        }
        else
        {
            fallbackNumber = branch.AWBLastUsedNumber + branch.AWBIncrement;
        }

        var prefix = branch.AWBPrefix ?? "";
        return $"{prefix}{fallbackNumber}";
    }

    public async Task<string> PreviewNextAWBNumber(long branchId)
    {
        return await PreviewNextAWBNumber(branchId, MovementType.Domestic);
    }

    public async Task<bool> IsAWBNumberUnique(string awbNumber)
    {
        return !await _context.InscanMasters.AnyAsync(i => i.AWBNo == awbNumber && !i.IsDeleted);
    }
}
