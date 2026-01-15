// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CityVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class CityVM
  {
    public int CityID { get; set; }

    public string CityCode { get; set; }

    public string City { get; set; }

    public int CountryID { get; set; }

        public string CountryName{ get; set; }

        public bool IsHub { get; set; }

    public string Country { get; set; }
        public string CountryCode { get; set; }
  }

    public class CitySearch
    {
        public string CountryName { get; set; }
        public int CountryID { get; set; }
        public List<CityVM> Details { get; set; }
    }

    public class LocationSearch
    {
        public string CountryName { get; set; }
        public int CountryID { get; set; }
        public List<LocationVM> Details { get; set; }
    }
}
