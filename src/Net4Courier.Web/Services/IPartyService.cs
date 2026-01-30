using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public interface IPartyService
{
    Task<List<Party>> GetAllAsync();
    Task<Party?> GetByIdAsync(long id);
    Task<List<Party>> GetCustomersAsync();
    Task<List<Party>> GetSuppliersAsync();
}
