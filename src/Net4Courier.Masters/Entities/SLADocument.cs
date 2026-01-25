using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class SLADocument : BaseEntity
{
    public long SLAAgreementId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    
    public virtual SLAAgreement SLAAgreement { get; set; } = null!;
}
