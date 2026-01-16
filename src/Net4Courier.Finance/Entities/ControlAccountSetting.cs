using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class ControlAccountSetting : BaseEntity
{
    public long CompanyId { get; set; }
    public ControlAccountType AccountType { get; set; }
    public long AccountHeadId { get; set; }
    public string? Description { get; set; }
    
    public virtual AccountHead? AccountHead { get; set; }
}

public enum ControlAccountType
{
    CashAccount = 1,
    CODControl = 2,
    PrepaidControl = 3,
    CADControl = 4,
    FreightReceivable = 5,
    FreightPayable = 6,
    TDSPayable = 7,
    GSTPayable = 8,
    GSTReceivable = 9,
    RoundOff = 10,
    Discount = 11,
    BankAccount = 12
}
