using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class SLATransitRule : BaseEntity
{
    public long SLAAgreementId { get; set; }
    public long? ServiceTypeId { get; set; }
    
    public string? OriginZone { get; set; }
    public string? DestinationZone { get; set; }
    public long? OriginCountryId { get; set; }
    public long? DestinationCountryId { get; set; }
    public long? OriginCityId { get; set; }
    public long? DestinationCityId { get; set; }
    
    public int TransitDays { get; set; }
    public int? PickupSLAHours { get; set; }
    public int? DeliverySLAHours { get; set; }
    
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    
    public virtual SLAAgreement SLAAgreement { get; set; } = null!;
    public virtual ServiceType? ServiceType { get; set; }
    public virtual Country? OriginCountry { get; set; }
    public virtual Country? DestinationCountry { get; set; }
    public virtual City? OriginCity { get; set; }
    public virtual City? DestinationCity { get; set; }
}
