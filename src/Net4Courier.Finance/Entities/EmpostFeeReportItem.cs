namespace Net4Courier.Finance.Entities;

public class EmpostFeeReportItem
{
    public int ParcelTypeId { get; set; }
    public string ParcelType { get; set; } = string.Empty;
    public int MovementId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal CourierCharge { get; set; }
    public decimal OtherCharge { get; set; }
    public decimal NetTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal VatPercent { get; set; }
    public decimal VatAmount { get; set; }
}
