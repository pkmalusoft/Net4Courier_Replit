using System.ComponentModel.DataAnnotations;
using Server.Core.Common;

namespace Server.Modules.Courier.Models;

public enum RemittanceStatus
{
    Pending,
    Approved,
    PaymentInitiated,
    Paid,
    PartiallyPaid,
    Cancelled,
    Disputed
}

public enum RemittancePaymentMode
{
    BankTransfer,
    Check,
    Cash,
    UPI,
    NEFT,
    RTGS,
    Other
}

public class CODRemittance : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string RemittanceNumber { get; set; } = string.Empty;
    
    public DateTime RemittanceDate { get; set; } = DateTime.UtcNow;
    
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    public RemittanceStatus Status { get; set; } = RemittanceStatus.Pending;
    
    public int ShipmentCount { get; set; }
    
    public decimal TotalCODAmount { get; set; }
    
    public decimal TotalMaterialCost { get; set; }
    
    public decimal DeductionAmount { get; set; }
    
    [MaxLength(500)]
    public string? DeductionReason { get; set; }
    
    public decimal NetPayableAmount { get; set; }
    
    public decimal PaidAmount { get; set; }
    
    public decimal BalanceAmount { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public RemittancePaymentMode? PaymentMode { get; set; }
    
    [MaxLength(100)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(100)]
    public string? PaymentReference { get; set; }
    
    [MaxLength(200)]
    public string? BankName { get; set; }
    
    [MaxLength(50)]
    public string? AccountNumber { get; set; }
    
    [MaxLength(20)]
    public string? IFSCCode { get; set; }
    
    public DateTime? PaymentDate { get; set; }
    
    public Guid? PaymentByUserId { get; set; }
    
    public Guid? ProcessedByUserId { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public Guid? ApprovedByUserId { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [MaxLength(1000)]
    public string? Remarks { get; set; }
    
    public Guid? JournalEntryId { get; set; }
    
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public ICollection<CODRemittanceItem> Items { get; set; } = new List<CODRemittanceItem>();
}

public class CODRemittanceItem : BaseEntity
{
    public Guid CODRemittanceId { get; set; }
    public CODRemittance CODRemittance { get; set; } = null!;
    
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;
    
    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;
    
    public DateTime DeliveryDate { get; set; }
    
    public decimal CODAmount { get; set; }
    
    public decimal MaterialCost { get; set; }
    
    public decimal MaterialCostAmount { get; set; }
    
    public decimal FreightDeduction { get; set; }
    
    public decimal OtherDeductions { get; set; }
    
    [MaxLength(500)]
    public string? DeductionNotes { get; set; }
    
    public decimal NetAmount { get; set; }
}
