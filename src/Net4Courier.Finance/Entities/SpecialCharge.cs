using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class SpecialCharge : AuditableEntity
{
    public long CompanyId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public string? ChargeCode { get; set; }
    public ChargeType ChargeType { get; set; } = ChargeType.Flat;
    public decimal ChargeValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsTaxApplicable { get; set; }
    public decimal? TaxPercent { get; set; }
    public SpecialChargeStatus Status { get; set; } = SpecialChargeStatus.Draft;
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }
    public bool IsLocked { get; set; }
}

public enum ChargeType
{
    Flat = 0,
    Percentage = 1
}

public enum SpecialChargeStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2
}
