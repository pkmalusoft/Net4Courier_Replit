namespace Truebooks.Platform.Contracts.Legacy.DTOs;

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

public enum TransactionCategory
{
    GL = 0,
    PartyReceipt = 1,
    PartyPayment = 2
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

public class CashBankTransaction
{
    public Guid Id { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; } = DateTime.Today;
    public TransactionType TransactionType { get; set; } = TransactionType.Bank;
    public RecPayType RecPayType { get; set; } = RecPayType.Receipt;
    public TransactionCategory TransactionCategory { get; set; } = TransactionCategory.GL;
    
    public Guid SourceAccountId { get; set; }
    public ChartOfAccount? SourceAccount { get; set; }
    
    public Guid? BankAccountId { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public bool IsPDC { get; set; }
    
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? ReferenceNo { get; set; }
    
    public CashBankStatus Status { get; set; } = CashBankStatus.Draft;
    
    public ReceiptType ReceiptType { get; set; } = ReceiptType.General;
    
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    
    public DepositStatus DepositStatus { get; set; } = DepositStatus.NotApplicable;
    public DateTime? ActualDepositDate { get; set; }
    
    public ClearanceStatus ClearanceStatus { get; set; } = ClearanceStatus.NotApplicable;
    public DateTime? ClearanceDate { get; set; }
    public string? BouncedReason { get; set; }
    public DateTime? PostedDate { get; set; }
    public Guid? PostedByUserId { get; set; }
    
    public bool IsVoided { get; set; } = false;
    public DateTime? VoidedDate { get; set; }
    public Guid? VoidedByUserId { get; set; }
    public string? VoidReason { get; set; }
    
    public Guid? JournalEntryId { get; set; }
    
    public Guid? BranchId { get; set; }
    public string? OrganizationBranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? OrganizationDepartmentName { get; set; }
    
    public List<CashBankTransactionLine> Lines { get; set; } = new();
}

public class CashBankTransactionLine
{
    public Guid Id { get; set; }
    public Guid CashBankTransactionId { get; set; }
    
    public Guid DestinationAccountId { get; set; }
    public ChartOfAccount? DestinationAccount { get; set; }
    
    public decimal GrossAmount { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public List<CashBankAllocation> Allocations { get; set; } = new();
}

public class CashBankAllocation
{
    public Guid Id { get; set; }
    public Guid CashBankTransactionLineId { get; set; }
    
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class CashBankInvoiceAllocation
{
    public Guid Id { get; set; }
    public Guid CashBankTransactionId { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal InvoiceTotal { get; set; }
    public decimal InvoiceOutstanding { get; set; }
    public decimal AllocationAmount { get; set; }
    public DateTime AllocationDate { get; set; } = DateTime.Today;
}

public class InvoiceForAllocation
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
}

public class CashBankBillAllocation
{
    public Guid Id { get; set; }
    public Guid CashBankTransactionId { get; set; }
    public Guid PurchaseBillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal BillTotal { get; set; }
    public decimal BillOutstanding { get; set; }
    public decimal AllocationAmount { get; set; }
    public DateTime AllocationDate { get; set; } = DateTime.Today;
}

public class BillForAllocation
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
}
