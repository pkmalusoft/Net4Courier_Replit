using System.ComponentModel.DataAnnotations;

namespace Net4Courier.Kernel.Entities;

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3
}

public class AuditLog
{
    public long Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    public long EntityId { get; set; }
    
    public AuditAction Action { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    public long? UserId { get; set; }
    
    [MaxLength(100)]
    public string? UserName { get; set; }
    
    public long? BranchId { get; set; }
    
    [MaxLength(100)]
    public string? BranchName { get; set; }
    
    [MaxLength(50)]
    public string? IPAddress { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? AdditionalInfo { get; set; }
}
