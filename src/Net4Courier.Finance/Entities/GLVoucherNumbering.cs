using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class GLVoucherNumbering : BaseEntity
{
    public long? CompanyId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? Separator { get; set; } = "-";
    public int NextNumber { get; set; } = 1;
    public int NumberLength { get; set; } = 6;
    public bool IsLocked { get; set; } = false;
    public long? FinancialYearId { get; set; }
}
