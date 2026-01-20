using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class ImportShipmentNote : AuditableEntity
{
    public long ImportShipmentId { get; set; }
    
    public NoteCategory Category { get; set; } = NoteCategory.General;
    public string NoteText { get; set; } = string.Empty;
    
    public long? AddedByUserId { get; set; }
    public string? AddedByUserName { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsInternal { get; set; } = true;
    
    public virtual ImportShipment? ImportShipment { get; set; }
}
