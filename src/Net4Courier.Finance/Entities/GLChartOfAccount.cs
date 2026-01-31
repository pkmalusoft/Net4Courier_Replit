using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class GLChartOfAccount : BaseEntity
{
    public long? CompanyId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountType { get; set; }
    public long? ParentId { get; set; }
    public bool IsSystemAccount { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? Description { get; set; }
    public bool AllowPosting { get; set; } = true;
    public int Level { get; set; } = 0;
    public long? AccountClassificationId { get; set; }
    public int? ControlAccountType { get; set; }
    public DateTime? DeactivatedDate { get; set; }
    public string? DeactivationReason { get; set; }
    public string? CreatedByUser { get; set; }
    public string? UpdatedByUser { get; set; }
    public string? DeactivatedByUserId { get; set; }
    
    public virtual GLChartOfAccount? Parent { get; set; }
    public virtual GLAccountClassification? AccountClassification { get; set; }
    public virtual ICollection<GLChartOfAccount> Children { get; set; } = new List<GLChartOfAccount>();
}
