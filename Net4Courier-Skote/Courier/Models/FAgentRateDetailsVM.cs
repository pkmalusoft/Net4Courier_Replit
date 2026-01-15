// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.FAgentRateDetailsVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class FAgentRateDetailsVM
  {
    public Decimal AddWtFrom { get; set; }

    public Decimal AddWtTo { get; set; }

    public Decimal IncrWt { get; set; }

    public Decimal ContractRate { get; set; }

    public Decimal AddRate { get; set; }

        public bool Deleted { get; set; }

        public int FAgentRateID { get; set; }
        public int FAgentRateDetID { get; set; }
    }

    public class FAgentCostDetailsVM
    {
        public string AWBNo { get; set; }
        public decimal Weight { get; set; }
        public Decimal TotalCost { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string ForwardingAWBNo   { get; set; }

        public Decimal OtherCharge { get; set; }

        public string Remarks { get; set; }
        
    }
}
