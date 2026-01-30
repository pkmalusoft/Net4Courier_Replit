using System.ComponentModel.DataAnnotations;
using Server.Modules.GeneralLedger.Models;
using Server.Modules.AR.Models;
using Server.Modules.AP.Models;
using Server.Core.Common;
using Server.Modules.BankReconciliation.Models;

namespace Server.Modules.CashBank.Models;

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

    public Guid SourceAccountId { get; set; }
    public ChartOfAccount SourceAccount { get; set; } = null!;

    // Link to Bank Account for reconciliation (nullable for cash transactions)
    public Guid? BankAccountId { get; set; }
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

    // Receipt Type for Customer/Vendor tracking
    public ReceiptType ReceiptType { get; set; } = ReceiptType.General;

    // Customer/Vendor linking
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    // PDC Deposit Tracking
    public DepositStatus DepositStatus { get; set; } = DepositStatus.NotApplicable;
    public DateTime? ActualDepositDate { get; set; }

    // Cheque Clearance Tracking
    public ClearanceStatus ClearanceStatus { get; set; } = ClearanceStatus.NotApplicable;
    public DateTime? ClearanceDate { get; set; }

    [MaxLength(500)]
    public string? BouncedReason { get; set; }

    public DateTime? PostedDate { get; set; }

    public Guid? PostedByUserId { get; set; }

    // Soft Delete / Void fields
    public bool IsVoided { get; set; } = false;

    public DateTime? VoidedDate { get; set; }

    public Guid? VoidedByUserId { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }

    // TDS (Tax Deducted at Source) for B2B service receipts
    public decimal TDSAmount { get; set; }

    public decimal TDSPercent { get; set; }

    [MaxLength(50)]
    public string? TDSCertificateNo { get; set; }

    // Service Contract linking for automated billing
    public Guid? ServiceContractId { get; set; }

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    // Branch and Department for transaction stamping
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<CashBankTransactionLine> Lines { get; set; } = new List<CashBankTransactionLine>();

    public ICollection<VoucherAttachment> Attachments { get; set; } = new List<VoucherAttachment>();
    
    public int FiscalYear { get; set; } = DateTime.UtcNow.Year;
}
