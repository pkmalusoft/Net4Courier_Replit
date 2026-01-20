using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services;

public class AWBNumberService
{
    private readonly ApplicationDbContext _context;

    public AWBNumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAWBNumber(long branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new InvalidOperationException($"Branch with ID {branchId} not found");
        }

        long nextNumber;
        if (branch.AWBLastUsedNumber == 0)
        {
            nextNumber = branch.AWBStartingNumber;
        }
        else
        {
            nextNumber = branch.AWBLastUsedNumber + branch.AWBIncrement;
        }

        branch.AWBLastUsedNumber = nextNumber;
        branch.ModifiedAt = DateTime.UtcNow;

        var prefix = branch.AWBPrefix ?? "";
        return $"{prefix}{nextNumber}";
    }

    public async Task<string> PreviewNextAWBNumber(long branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            return "N/A";
        }

        long nextNumber;
        if (branch.AWBLastUsedNumber == 0)
        {
            nextNumber = branch.AWBStartingNumber;
        }
        else
        {
            nextNumber = branch.AWBLastUsedNumber + branch.AWBIncrement;
        }

        var prefix = branch.AWBPrefix ?? "";
        return $"{prefix}{nextNumber}";
    }

    public async Task<bool> IsAWBNumberUnique(string awbNumber)
    {
        return !await _context.InscanMasters.AnyAsync(i => i.AWBNo == awbNumber && !i.IsDeleted);
    }
}
