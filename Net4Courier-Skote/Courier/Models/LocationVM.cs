// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.LocationVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class LocationVM
  {
    public int LocationID { get; set; }

    public string Location { get; set; }
        public string LocationName { get; set; }

        public int CityID { get; set; }

    public int CountryID { get; set; }

    public string CountryName{ get; set; }
    public string CityName { get; set; }
        public string PlaceID { get; set; }
        public string CountryCode { get; set; }
    }
}
