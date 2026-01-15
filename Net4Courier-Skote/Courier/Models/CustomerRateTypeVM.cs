// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CustomerRateTypeVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class CustomerRateTypeVM
  {
    public int CustomerRateTypeID { get; set; }

    public string CustomerRateType { get; set; }

    public int ZoneCategoryID { get; set; }

    public bool StatusDefault { get; set; }

    public string ZoneCategory { get; set; }

    public decimal CourierCharge { get; set; }
        public int RateBasedType { get; set; }
  }


    public class CustomerRateVM
    {
        public int CourierServiceID { get; set; }

        public int MovementID { get; set; }

        public int FAgentID { get; set; }

        public int CustomerRateID { get; set; }

        public string CustomerRateType { get; set; }

        public string CountryName { get; set; }

        public string CityName { get; set; }
    }
}
