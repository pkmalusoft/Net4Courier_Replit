// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CustRateDetailsVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class CustRateDetailsVM
  {
        public int CustomerRateDetID { get; set; }
        public bool Deleted { get; set; }
    public Decimal AddWtFrom { get; set; }

    public Decimal AddWtTo { get; set; }

    public Decimal IncrWt { get; set; }

    public Decimal ContractRate { get; set; }

    public Decimal AddRate { get; set; }
  }
}
