using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class PartyService : IPartyService
{
    private readonly ApplicationDbContext _context;

    public PartyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Party>> GetAllAsync()
    {
        return await _context.Parties
            .Where(p => !p.IsDeleted)
            .Include(p => p.AccountType)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Party?> GetByIdAsync(long id)
    {
        return await _context.Parties
            .Include(p => p.AccountType)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<List<Party>> GetCustomersAsync()
    {
        return await _context.Parties
            .Where(p => !p.IsDeleted && p.PartyType == PartyType.Customer)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Party>> GetSuppliersAsync()
    {
        return await _context.Parties
            .Where(p => !p.IsDeleted && p.PartyType == PartyType.Supplier)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
