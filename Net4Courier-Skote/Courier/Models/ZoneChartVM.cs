// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.ZoneChartVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class ZoneChartVM
  {
    public int ZoneChartID { get; set; }

    public int ZoneCategoryID { get; set; }

    public int ZoneID { get; set; }

        public string ZoneType { get; set; }

        public string ZoneCategory { get; set; }
        public string Cities { get; set; }
        public string Countries { get; set; }
        public string ZoneName { get; set; }

    public int depotcountry { get; set; }

    public List<int> country { get; set; }

    public List<int> city { get; set; }

    public string StatusZone { get; set; }

    public string countrylist { get; set; }

    public string citylist { get; set; }

        public string LocationName { get; set; }
        public string CountryName { get; set; }
        public string PlaceID { get; set; }
        public string CityName { get; set; }
        public int CityID { get; set; }
        public int CountryID { get; set; }
        public List<ZoneChartDetailsVM> Details { get; set; }

        public bool IsCountryChange { get; set; }
    }

    public class ZoneChartDetailsVM
    {
        public int ZoneChartDetailID { get; set; }
        public int ZoneChartID { get; set; }
        public int LocationID{ get; set; }
        public string LocationName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string PlaceID { get; set; }
        public string SubLocality { get; set; }
        public bool  Deleted { get; set; }
        public int CountryID { get; set; }
        public int CityID { get; set; }
        public bool IsCountryChange { get; set; }
    }

    public class ZoneEmpVM : EmpZoneAllocation
    {
        public string EmployeeName { get; set; }
        public List<ZomeEmpDetailsVM> Details { get; set; }

    }

    public class ZomeEmpDetailsVM : EmpZoneAllocationDetail
    {
        public string ZoneName { get; set; }
        public string LocationName { get; set; }
        public string LocationId { get; set; }
        public string PlaceID { get; set; }
    }
}
