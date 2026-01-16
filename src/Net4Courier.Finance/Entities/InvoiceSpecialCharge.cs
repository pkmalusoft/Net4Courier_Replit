using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class InvoiceSpecialCharge : BaseEntity
{
    public long InvoiceId { get; set; }
    public long SpecialChargeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public ChargeType ChargeType { get; set; }
    public decimal ChargeValue { get; set; }
    public decimal CalculatedAmount { get; set; }
    public bool IsTaxApplicable { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public virtual Invoice? Invoice { get; set; }
    public virtual SpecialCharge? SpecialCharge { get; set; }
}
