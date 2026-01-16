using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class MAWBBag : AuditableEntity
{
    public long MAWBId { get; set; }
    public string BagNo { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    
    public string? SealNo { get; set; }
    public int PieceCount { get; set; }
    public decimal GrossWeight { get; set; }
    public decimal ChargeableWeight { get; set; }
    
    public string? BagType { get; set; }
    public string? Remarks { get; set; }
    
    public bool IsSealed { get; set; }
    public DateTime? SealedAt { get; set; }
    public long? SealedByUserId { get; set; }
    public string? SealedByUserName { get; set; }
    
    public virtual MasterAirwaybill? MAWB { get; set; }
    public virtual ICollection<InscanMaster> Shipments { get; set; } = new List<InscanMaster>();
}
