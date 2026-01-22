using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Port : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PortType PortType { get; set; }
    public string? IATACode { get; set; }
    public string? ICAOCode { get; set; }
    public string? UNLocode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? TimeZone { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public enum PortType
{
    Airport = 1,
    Seaport = 2,
    LandBorder = 3
}
