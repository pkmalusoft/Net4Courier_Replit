using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class ImportBag : AuditableEntity
{
    public long ImportMasterId { get; set; }
    public string BagNumber { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    
    public BagType BagType { get; set; } = BagType.CourierBag;
    public int ShipmentCount { get; set; }
    public int InscannedCount { get; set; }
    
    public decimal? GrossWeight { get; set; }
    public decimal? ChargeableWeight { get; set; }
    public int? PieceCount { get; set; }
    
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    
    public string? SealNumber { get; set; }
    public bool IsSealed { get; set; }
    public string? HandlingCode { get; set; }
    
    public ImportBagStatus Status { get; set; } = ImportBagStatus.Expected;
    public string? Remarks { get; set; }
    
    public DateTime? ArrivedAt { get; set; }
    public DateTime? InscannedAt { get; set; }
    public long? InscannedByUserId { get; set; }
    public string? InscannedByUserName { get; set; }
    
    public virtual ImportMaster? ImportMaster { get; set; }
    public virtual ICollection<ImportShipment> Shipments { get; set; } = new List<ImportShipment>();
}
