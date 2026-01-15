// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.FAgentRateVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class FAgentRateVM
  {
    public int FAgentRateID { get; set; }

    public int FAgentID { get; set; }

    public int CountryID { get; set; }

    public string Fname { get; set; }

    public int? ProductTypeID { get; set; }

    public int ZoneID { get; set; }

    public Decimal BaseWeight { get; set; }

    public Decimal BaseRate { get; set; }
        public decimal hfAddCustomerRate { get; set; }
        public int? ZoneCategoryID { get; set; }

        public string ZoneChartName { get; set; }
        public int ZoneChartID { get; set; }

        public string CourierService { get; set; }

    public int FAgentRateDetID { get; set; }
      
    public List<FAgentRateDetailsVM> FAgentRateDetails { get; set; }
  }

    public class FwdAgentRateDetail
    {

        public decimal WeightFrom { get; set; }
        public decimal WeightTo { get; set; }
        public decimal IncrementWeight { get; set; }
        public decimal Rate { get; set; }
        public decimal AdditionalRate { get; set; }
        public decimal BaseRate { get; set; }
    }
}
