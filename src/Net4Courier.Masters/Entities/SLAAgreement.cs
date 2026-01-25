using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class SLAAgreement : BaseEntity
{
    public long CompanyId { get; set; }
    public long CustomerId { get; set; }
    public string AgreementNo { get; set; } = string.Empty;
    public string? Title { get; set; }
    
    public SLAAccountType AccountType { get; set; } = SLAAccountType.Credit;
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    
    public DateTime AgreementDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    public decimal MaxPackageWeight { get; set; } = 30;
    public int VolumetricDivisorAir { get; set; } = 5000;
    public int VolumetricDivisorRoad { get; set; } = 4000;
    public int VolumetricDivisorDomestic { get; set; } = 5000;
    
    public decimal LiabilityLimitUSD { get; set; } = 100;
    public string? LiabilityLimitCurrency { get; set; } = "USD";
    
    public string? SpecialTerms { get; set; }
    public string? Notes { get; set; }
    
    public SLAStatus Status { get; set; } = SLAStatus.Draft;
    
    public DateTime? ApprovedAt { get; set; }
    public long? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    
    public DateTime? TerminatedAt { get; set; }
    public long? TerminatedByUserId { get; set; }
    public string? TerminatedByUserName { get; set; }
    public string? TerminationReason { get; set; }
    
    public string? DocumentPath { get; set; }
    
    public virtual Company Company { get; set; } = null!;
    public virtual Party Customer { get; set; } = null!;
    public virtual ICollection<SLATransitRule> TransitRules { get; set; } = new List<SLATransitRule>();
}

public enum SLAAccountType
{
    Cash = 1,
    Credit = 2,
    Prepaid = 3
}

public enum SLAStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Expired = 3,
    Terminated = 4,
    Suspended = 5
}
