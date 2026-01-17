using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class ShipmentNote : AuditableEntity
{
    public long ShipmentId { get; set; }
    public long AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    
    public virtual InscanMaster Shipment { get; set; } = null!;
    public virtual ICollection<ShipmentNoteMention> Mentions { get; set; } = new List<ShipmentNoteMention>();
}

public class ShipmentNoteMention : BaseEntity
{
    public long NoteId { get; set; }
    public long MentionedUserId { get; set; }
    public string MentionedUserName { get; set; } = string.Empty;
    public bool IsNotified { get; set; }
    
    public virtual ShipmentNote Note { get; set; } = null!;
}
