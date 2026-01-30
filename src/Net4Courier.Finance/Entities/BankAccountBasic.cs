namespace Net4Courier.Finance.Entities;

public class BankAccountBasic
{
    public long Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public bool IsActive { get; set; } = true;
}
