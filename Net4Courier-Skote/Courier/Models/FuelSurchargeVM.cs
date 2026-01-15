// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.FuelSurchargeVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class FuelSurchargeVM
  {
    public int ID { get; set; }

    public int FAgentID { get; set; }

    public string Month { get; set; }

    public double CostRate { get; set; }

    public double SaleRate { get; set; }
  }
}
