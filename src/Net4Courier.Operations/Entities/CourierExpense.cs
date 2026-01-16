using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class CourierExpense : AuditableEntity
{
    public long DRSId { get; set; }
    public int CourierId { get; set; }
    public string? CourierName { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? BillImagePath { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalRemarks { get; set; }
    
    public virtual DRS DRS { get; set; } = null!;
}
