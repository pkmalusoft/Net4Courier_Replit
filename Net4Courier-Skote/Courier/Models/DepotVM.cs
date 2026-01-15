// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.DepotVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class DepotVM
  {
    public int ID { get; set; }

    public int CountryID { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public int CityID { get; set; }

    public string Depot { get; set; }

    public bool IsOwn { get; set; }

    public int? AgentID { get; set; }

    public int? CompanyID { get; set; }

    public string Own { get; set; }

        public string CountryName { get; set; }
        public string CityName { get; set; }

        public string BranchName { get; set; }
        public int?  BranchID { get; set; }
    }
}
