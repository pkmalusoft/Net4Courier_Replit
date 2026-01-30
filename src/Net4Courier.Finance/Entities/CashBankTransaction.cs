using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Finance.Entities;

public enum TransactionType
{
    Bank,
    Cash
}

public enum RecPayType
{
    Receipt,
    Payment
}

public enum CashBankStatus
{
    Draft,
    Posted,
    Cancelled
}

public enum ReceiptType
{
    General,
    CustomerReceipt,
    VendorPayment
}

public enum TransactionCategory
{
    GL,
    PartyReceipt,
    PartyPayment
}

public enum DepositStatus
{
    NotApplicable,
    PendingDeposit,
    Deposited
}

public enum ClearanceStatus
{
    NotApplicable,
    PendingClearance,
    Cleared,
    Bounced
}

public class CashBankTransaction : BaseEntity
{
    private DateTime _voucherDate = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string VoucherNo { get; set; } = string.Empty;

    public DateTime VoucherDate 
    { 
        get => _voucherDate;
        set
        {
            _voucherDate = value;
            FiscalYear = value.Year;
        }
    }

    public TransactionType TransactionType { get; set; } = TransactionType.Bank;

    public RecPayType RecPayType { get; set; } = RecPayType.Receipt;

    public TransactionCategory TransactionCategory { get; set; } = TransactionCategory.GL;

    public long SourceAccountId { get; set; }
    public AccountHead? SourceAccount { get; set; }

    public long? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public decimal TotalAmount { get; set; }

    [MaxLength(50)]
    public string? ChequeNo { get; set; }

    public DateTime? ChequeDate { get; set; }

    public bool IsPDC { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(100)]
    public string? BranchName { get; set; }

    [MaxLength(100)]
    public string? ReferenceNo { get; set; }

    public CashBankStatus Status { get; set; } = CashBankStatus.Draft;

    public ReceiptType ReceiptType { get; set; } = ReceiptType.General;

    public long? PartyId { get; set; }
    public Party? Party { get; set; }

    public DepositStatus DepositStatus { get; set; } = DepositStatus.NotApplicable;
    public DateTime? ActualDepositDate { get; set; }

    public ClearanceStatus ClearanceStatus { get; set; } = ClearanceStatus.NotApplicable;
    public DateTime? ClearanceDate { get; set; }

    [MaxLength(500)]
    public string? BouncedReason { get; set; }

    public DateTime? PostedDate { get; set; }

    public long? PostedByUserId { get; set; }

    public bool IsVoided { get; set; } = false;

    public DateTime? VoidedDate { get; set; }

    public long? VoidedByUserId { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }

    public decimal TDSAmount { get; set; }

    public decimal TDSPercent { get; set; }

    [MaxLength(50)]
    public string? TDSCertificateNo { get; set; }

    public long? JournalId { get; set; }
    public Journal? Journal { get; set; }

    public long? CompanyBranchId { get; set; }
    public Branch? CompanyBranch { get; set; }

    public long? DepartmentId { get; set; }

    [MaxLength(1000)]
    public string? Narration { get; set; }

    public ICollection<CashBankTransactionLine> Lines { get; set; } = new List<CashBankTransactionLine>();

    public ICollection<VoucherAttachment> Attachments { get; set; } = new List<VoucherAttachment>();
    
    public int FiscalYear { get; set; } = DateTime.UtcNow.Year;
}
