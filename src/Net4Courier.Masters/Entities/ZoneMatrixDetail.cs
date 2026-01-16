using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class ZoneMatrixDetail : AuditableEntity
{
    public long ZoneMatrixId { get; set; }
    public long? CountryId { get; set; }
    public long? CityId { get; set; }
    public long? StateId { get; set; }
    public string? PostalCodeFrom { get; set; }
    public string? PostalCodeTo { get; set; }
    
    public virtual ZoneMatrix? ZoneMatrix { get; set; }
}
