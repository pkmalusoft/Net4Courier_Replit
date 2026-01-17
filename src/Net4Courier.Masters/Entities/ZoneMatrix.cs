using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class ZoneMatrix : AuditableEntity
{
    public long? ZoneCategoryId { get; set; }
    public string ZoneCode { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public long? CountryId { get; set; }
    public long? CityId { get; set; }
    public long? CompanyId { get; set; }
    public int SortOrder { get; set; }
    
    public virtual ZoneCategory? ZoneCategory { get; set; }
    public virtual Country? Country { get; set; }
    public virtual City? City { get; set; }
    public virtual ICollection<ZoneMatrixDetail> Details { get; set; } = new List<ZoneMatrixDetail>();
    public virtual ICollection<RateCardZone> RateCardZones { get; set; } = new List<RateCardZone>();
}
