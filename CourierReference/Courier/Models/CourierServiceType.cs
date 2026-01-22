using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class CourierServiceType : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DeliveryDays { get; set; } = 1;
    
    public int DefaultTransitDays 
    { 
        get => DeliveryDays; 
        set => DeliveryDays = value; 
    }

    public bool IsExpress { get; set; } = false;

    public bool RequiresSignature { get; set; } = true;

    public bool AllowsCOD { get; set; } = true;
    
    public bool AllowCOD 
    { 
        get => AllowsCOD; 
        set => AllowsCOD = value; 
    }
    
    public bool AllowInsurance { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    
    public decimal TaxPercent { get; set; } = 0;

    public ICollection<ZoneRate> ZoneRates { get; set; } = new List<ZoneRate>();
}
