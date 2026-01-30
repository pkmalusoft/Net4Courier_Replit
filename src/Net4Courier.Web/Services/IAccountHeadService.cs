using Net4Courier.Finance.Entities;

namespace Net4Courier.Web.Services;

public interface IAccountHeadService
{
    Task<List<AccountHead>> GetAllAsync();
    Task<AccountHead?> GetByIdAsync(long id);
    Task<AccountHead> CreateAsync(AccountHead accountHead);
    Task<bool> UpdateAsync(AccountHead accountHead);
    Task<bool> DeleteAsync(long id);
}
