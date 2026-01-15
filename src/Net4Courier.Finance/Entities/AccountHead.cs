using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class AccountHead : BaseEntity
{
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public AccountType AccountType { get; set; }
    public AccountGroup AccountGroup { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? OpeningBalance { get; set; }
    public string? OpeningBalanceType { get; set; }
    
    public virtual AccountHead? Parent { get; set; }
    public virtual ICollection<AccountHead> Children { get; set; } = new List<AccountHead>();
}

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Income = 3,
    Expense = 4,
    Equity = 5
}

public enum AccountGroup
{
    CurrentAsset = 1,
    FixedAsset = 2,
    CurrentLiability = 3,
    LongTermLiability = 4,
    DirectIncome = 5,
    IndirectIncome = 6,
    DirectExpense = 7,
    IndirectExpense = 8,
    Capital = 9,
    BankAccount = 10,
    CashAccount = 11,
    Receivable = 12,
    Payable = 13
}
