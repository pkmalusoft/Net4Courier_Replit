using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class SlabRuleTemplate : AuditableEntity
{
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseWeight { get; set; } = 0.5m;
    public decimal BaseRate { get; set; }
    public long? CompanyId { get; set; }
    
    public virtual ICollection<SlabRuleTemplateDetail> Details { get; set; } = new List<SlabRuleTemplateDetail>();
}

public class SlabRuleTemplateDetail : AuditableEntity
{
    public long TemplateId { get; set; }
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }
    public decimal IncrementWeight { get; set; } = 0.5m;
    public decimal IncrementRate { get; set; }
    public decimal FlatRate { get; set; }
    public decimal CostFlatRate { get; set; }
    public decimal CostPerKgRate { get; set; }
    public SlabCalculationMode CalculationMode { get; set; } = SlabCalculationMode.PerStep;
    public int SortOrder { get; set; }
    
    public virtual SlabRuleTemplate? Template { get; set; }
}
