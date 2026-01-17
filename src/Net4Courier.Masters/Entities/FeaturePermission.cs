using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class FeaturePermission : BaseEntity
{
    public long RoleId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public bool IsGranted { get; set; } = true;
    
    public virtual Role Role { get; set; } = null!;
}

public static class FeatureCodes
{
    public const string PricingView = "Pricing.View";
    public const string MarginView = "Margin.View";
    public const string FinancialsView = "Financials.View";
    public const string CostView = "Cost.View";
    public const string NotesAdd = "Notes.Add";
    public const string NotesDelete = "Notes.Delete";
    
    public static readonly string[] All = new[]
    {
        PricingView,
        MarginView,
        FinancialsView,
        CostView,
        NotesAdd,
        NotesDelete
    };
    
    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { PricingView, "View pricing information on shipments" },
        { MarginView, "View margin calculations on shipments" },
        { FinancialsView, "View financial snapshot (charges, COD, invoice status)" },
        { CostView, "View cost rates from carriers" },
        { NotesAdd, "Add notes/comments to shipments" },
        { NotesDelete, "Delete notes/comments from shipments" }
    };
}
