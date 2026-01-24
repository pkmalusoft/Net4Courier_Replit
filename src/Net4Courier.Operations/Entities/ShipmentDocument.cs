using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class ShipmentDocument : AuditableEntity
{
    public long InscanMasterId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public long FileSize { get; set; }
    public string? Description { get; set; }
    
    public virtual InscanMaster InscanMaster { get; set; } = null!;
}
