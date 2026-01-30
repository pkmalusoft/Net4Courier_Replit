using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class TicketComment : BaseEntity
{
    public long TicketId { get; set; }
    public virtual Ticket? Ticket { get; set; }
    
    public string Comment { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; }
    
    public bool IsFromCustomer { get; set; }
    
    public string? AttachmentPath { get; set; }
    public string? AttachmentName { get; set; }
    
    public long? UserId { get; set; }
    public string? UserName { get; set; }
}
