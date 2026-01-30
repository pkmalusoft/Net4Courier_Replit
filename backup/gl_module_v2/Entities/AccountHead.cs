using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class AccountHead : BaseEntity
{
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public AccountClassification Classification { get; set; }
    public AccountGroup AccountGroup { get; set; }
    public AccountNature AccountNature { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? OpeningBalance { get; set; }
    public string? OpeningBalanceType { get; set; }
    
    public virtual AccountHead? Parent { get; set; }
    public virtual ICollection<AccountHead> Children { get; set; } = new List<AccountHead>();
}

public enum AccountClassification
{
    Assets = 1,
    Liabilities = 2,
    Income = 3,
    Expenditure = 4,
    Equity = 5
}

public enum AccountGroup
{
    CurrentAssets = 1,
    FixedAssets = 2,
    Investments = 3,
    CurrentLiabilities = 4,
    LongTermLiabilities = 5,
    DirectIncome = 6,
    IndirectIncome = 7,
    DirectExpenses = 8,
    IndirectExpenses = 9,
    Capital = 10,
    Reserves = 11
}

public enum AccountNature
{
    General = 0,
    CashAccount = 1,
    BankAccount = 2,
    ControlAccount = 3,
    Receivable = 4,
    Payable = 5,
    TaxAccount = 6,
    DiscountAccount = 7,
    RoundOffAccount = 8
}
