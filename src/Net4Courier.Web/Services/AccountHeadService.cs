using Microsoft.EntityFrameworkCore;
using Net4Courier.Finance.Entities;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services;

public class AccountHeadService : IAccountHeadService
{
    private readonly ApplicationDbContext _context;

    public AccountHeadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountHead>> GetAllAsync()
    {
        return await _context.AccountHeads
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task<AccountHead?> GetByIdAsync(long id)
    {
        return await _context.AccountHeads
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<AccountHead> CreateAsync(AccountHead accountHead)
    {
        _context.AccountHeads.Add(accountHead);
        await _context.SaveChangesAsync();
        return accountHead;
    }

    public async Task<bool> UpdateAsync(AccountHead accountHead)
    {
        _context.AccountHeads.Update(accountHead);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var accountHead = await _context.AccountHeads.FindAsync(id);
        if (accountHead == null) return false;

        accountHead.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
