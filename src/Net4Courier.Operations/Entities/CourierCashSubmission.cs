using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class CourierCashSubmission : AuditableEntity
{
    public long DRSId { get; set; }
    public int CourierId { get; set; }
    public string? CourierName { get; set; }
    public DateTime SubmissionDate { get; set; }
    public decimal CashSubmittedAmount { get; set; }
    public DateTime SubmissionTime { get; set; }
    public int? ReceivedById { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public decimal? ReceivedAmount { get; set; }
    public string? ReceiptNo { get; set; }
    public long? ReceiptVoucherId { get; set; }
    public string? Remarks { get; set; }
    public bool IsAcknowledged { get; set; }
    
    public virtual DRS DRS { get; set; } = null!;
}
