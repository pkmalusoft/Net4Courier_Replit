namespace Net4Courier.Web.DTOs;

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
