using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    PendingCustomer = 2,
    Resolved = 3,
    Closed = 4,
    Cancelled = 5
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

public class Ticket : BaseEntity
{
    public string TicketNo { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    
    public long PartyId { get; set; }
    public string? PartyName { get; set; }
    
    public long? AWBId { get; set; }
    public string? AWBNo { get; set; }
    
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    public long? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    public string? ResolutionNotes { get; set; }
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }
    
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
}
