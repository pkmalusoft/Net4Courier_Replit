using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public interface IFinancialYearService
{
    Task<List<FinancialYear>> GetAllAsync();
    Task<FinancialYear?> GetByIdAsync(long id);
    Task<FinancialYear> CreateWithPeriodsAsync(string name, DateTime startDate, DateTime endDate);
    Task<bool> ClosePeriodAsync(long periodId);
    Task<bool> ReopenPeriodAsync(long periodId);
}
