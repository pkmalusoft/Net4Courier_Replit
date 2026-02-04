using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum FinanceExpenseStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Paid = 5
}

public enum ExpenseCategory
{
    Travel = 1,
    Fuel = 2,
    Maintenance = 3,
    Office = 4,
    Communication = 5,
    Utilities = 6,
    Insurance = 7,
    Customs = 8,
    Freight = 9,
    Handling = 10,
    Other = 99
}

public class Expense : AuditableEntity
{
    public string ExpenseNo { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
    public long? FinancialYearId { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;
    public string? Description { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public long? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public decimal Amount { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? AttachmentPath { get; set; }
    public FinanceExpenseStatus Status { get; set; } = FinanceExpenseStatus.Draft;
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaidBy { get; set; }
}
