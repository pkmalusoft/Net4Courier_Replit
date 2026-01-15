namespace Net4Courier.Kernel.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedByName { get; set; }
    public string? ModifiedByName { get; set; }
}
