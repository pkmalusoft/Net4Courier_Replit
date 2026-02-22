namespace Net4Courier.Kernel.Enums;

public enum ApprovalWorkflowType
{
    CustomerInvoice = 1,
    VendorBill = 2,
    CourierExpense = 3,
    CreditNote = 4,
    DebitNote = 5,
    CustomerCredit = 6,
    Receipt = 7,
    VendorPayment = 8,
    JournalEntry = 9,
    CashVoucher = 10,
    BankVoucher = 11
}

public enum ApprovalStatus
{
    Draft = 0,
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Returned = 5,
    Cancelled = 6
}

public enum ApprovalActionType
{
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Returned = 4,
    Cancelled = 5,
    Reassigned = 6
}
