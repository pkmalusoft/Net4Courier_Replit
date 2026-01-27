using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Infrastructure.Services;

public class AWBStockService
{
    private readonly ApplicationDbContext _context;

    public AWBStockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AWBStock>> GetAllStockAsync(long companyId, long? branchId = null)
    {
        var query = _context.AWBStocks
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query.OrderByDescending(s => s.StockDate).ToListAsync();
    }

    public async Task<List<AWBStock>> GetAvailableStockAsync(long companyId, long branchId)
    {
        return await _context.AWBStocks
            .Where(s => s.CompanyId == companyId 
                && s.BranchId == branchId 
                && !s.IsDeleted 
                && s.IsActive
                && s.AvailableCount > 0)
            .OrderBy(s => s.AWBNoFrom)
            .ToListAsync();
    }

    public async Task<AWBStock?> GetByIdAsync(long id)
    {
        return await _context.AWBStocks
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<AWBStock> CreateStockAsync(AWBStock stock)
    {
        stock.CreatedAt = DateTime.UtcNow;
        stock.AvailableCount = stock.AWBCount;
        stock.AllocatedCount = 0;
        stock.Status = StockStatus.Available;
        stock.Amount = stock.Qty * stock.Rate;

        _context.AWBStocks.Add(stock);
        await _context.SaveChangesAsync();
        return stock;
    }

    public async Task<AWBStock> UpdateStockAsync(AWBStock stock)
    {
        stock.ModifiedAt = DateTime.UtcNow;
        stock.Amount = stock.Qty * stock.Rate;
        
        _context.AWBStocks.Update(stock);
        await _context.SaveChangesAsync();
        return stock;
    }

    public async Task<bool> DeleteStockAsync(long id, int userId)
    {
        var stock = await GetByIdAsync(id);
        if (stock == null) return false;

        if (stock.AllocatedCount > 0)
            throw new InvalidOperationException("Cannot delete stock that has allocated AWBs.");

        stock.IsDeleted = true;
        stock.ModifiedAt = DateTime.UtcNow;
        stock.ModifiedBy = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AllocateAWBsAsync(long stockId, int count)
    {
        var stock = await GetByIdAsync(stockId);
        if (stock == null || stock.AvailableCount < count)
            return false;

        stock.AvailableCount -= count;
        stock.AllocatedCount += count;
        stock.ModifiedAt = DateTime.UtcNow;

        if (stock.AvailableCount == 0)
            stock.Status = StockStatus.FullyAllocated;
        else if (stock.AllocatedCount > 0)
            stock.Status = StockStatus.PartiallyAllocated;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsAWBInStockAsync(string awbNo, long companyId)
    {
        var stocks = await _context.AWBStocks
            .Where(s => s.CompanyId == companyId && !s.IsDeleted && s.IsActive)
            .ToListAsync();

        foreach (var stock in stocks)
        {
            if (IsAWBInRange(awbNo, stock.AWBNoFrom, stock.AWBNoTo))
                return true;
        }
        return false;
    }

    public async Task<AWBStock?> GetStockContainingAWBAsync(string awbNo, long companyId)
    {
        var stocks = await _context.AWBStocks
            .Where(s => s.CompanyId == companyId && !s.IsDeleted && s.IsActive && s.AvailableCount > 0)
            .ToListAsync();

        foreach (var stock in stocks)
        {
            if (IsAWBInRange(awbNo, stock.AWBNoFrom, stock.AWBNoTo))
                return stock;
        }
        return null;
    }

    public async Task<List<string>> GetAvailableAWBNumbersAsync(long stockId, int count)
    {
        var stock = await GetByIdAsync(stockId);
        if (stock == null || stock.AvailableCount < count)
            return new List<string>();

        var allocatedAWBs = await _context.PrepaidAWBs
            .Where(p => p.AWBStockId == stockId)
            .Select(p => p.AWBNo)
            .ToListAsync();

        var availableAWBs = new List<string>();
        
        if (long.TryParse(stock.AWBNoFrom, out var fromNum) && long.TryParse(stock.AWBNoTo, out var toNum))
        {
            for (var i = fromNum; i <= toNum && availableAWBs.Count < count; i++)
            {
                var awbNo = i.ToString();
                if (!allocatedAWBs.Contains(awbNo))
                    availableAWBs.Add(awbNo);
            }
        }

        return availableAWBs;
    }

    private bool IsAWBInRange(string awbNo, string rangeFrom, string rangeTo)
    {
        if (long.TryParse(awbNo, out var awbNum) &&
            long.TryParse(rangeFrom, out var fromNum) &&
            long.TryParse(rangeTo, out var toNum))
        {
            return awbNum >= fromNum && awbNum <= toNum;
        }

        return string.Compare(awbNo, rangeFrom, StringComparison.Ordinal) >= 0 &&
               string.Compare(awbNo, rangeTo, StringComparison.Ordinal) <= 0;
    }

    public async Task<string> GenerateNextReferenceNoAsync(long companyId, long branchId)
    {
        var lastStock = await _context.AWBStocks
            .Where(s => s.CompanyId == companyId && s.BranchId == branchId)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastStock?.ReferenceNo != null)
        {
            var lastNum = lastStock.ReferenceNo.Split('-').LastOrDefault();
            if (int.TryParse(lastNum, out var num))
                nextNumber = num + 1;
        }

        return $"STK-{DateTime.UtcNow:yyyyMM}-{nextNumber:D4}";
    }
}
