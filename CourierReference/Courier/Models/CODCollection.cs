using System.ComponentModel.DataAnnotations;
using Server.Core.Common;

namespace Server.Modules.Courier.Models;

public enum CODStatus
{
    Pending,
    Collected,
    RemittedToCustomer,
    RemittedToBranch,
    Reconciled,
    Disputed
}

public enum RemittanceMode
{
    Cash,
    BankTransfer,
    Cheque,
    OnlinePayment
}

public class CODCollection : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CollectionNumber { get; set; } = string.Empty;

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public DateTime CollectionDate { get; set; } = DateTime.UtcNow;

    public decimal CODAmount { get; set; }

    public decimal CollectedAmount { get; set; }

    public decimal? ShortageAmount { get; set; }

    public Guid? CollectedByAgentId { get; set; }
    public CourierAgent? CollectedByAgent { get; set; }

    public CODStatus Status { get; set; } = CODStatus.Pending;

    public DateTime? RemittedDate { get; set; }

    public RemittanceMode? RemittanceMode { get; set; }

    [MaxLength(100)]
    public string? RemittanceReference { get; set; }

    public decimal? RemittanceAmount { get; set; }

    public decimal? DeductedCommission { get; set; }

    public decimal? DeductedCharges { get; set; }

    public Guid? CustomerRemittanceInvoiceId { get; set; }

    public Guid? CashBankTransactionId { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
