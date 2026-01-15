// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CustRateVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class CustRateVM
  {
    public int CustomerRateID { get; set; }

    public int ContractRateID { get; set; }

    public int ContractRateTypeID { get; set; }
        public int CustomerRateTypeID { get; set; }

        public int ZoneChartID { get; set; }
        public string ZoneChartName { get; set; }
        public string ZoneType { get; set; }
        public int ProductTypeID { get; set; }
        public int MovementID { get; set; }
        public int PaymentModeID { get; set; }
        public int FAgentID { get; set; }

    public int CountryID { get; set; }

    public Decimal BaseWt { get; set; }

    public Decimal BaseRate { get; set; }

    public Decimal hfAddCustomerRate { get; set; }

    public int CustomerRateDetID { get; set; }

    public bool withtax { get; set; }

    public bool withouttax { get; set; }

    public bool AdditionalCharges { get; set; }
        public string CustomerRateType { get; set; }
        public string ZoneCategory { get; set; }
        public string ZoneName { get; set; }

        public string ProductName { get; set; }
        public string FAgentName { get; set; }
        public string CitiesCountries { get; set; }
        public decimal BaseMargin { get; set; }
        public int BranchCustomerRateType { get; set; }
        public List<CustRateDetailsVM> CustRateDetails { get; set; }
  }
}
