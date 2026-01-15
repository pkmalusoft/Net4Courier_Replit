// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.ImportVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class ImportVM
  {
    public int ImportID { get; set; }

    public DateTime? ImportDate { get; set; }

    public string Agent { get; set; }

    public string View { get; set; }

    public string FlightNo { get; set; }

    public string MAWBNo { get; set; }

    public int Package { get; set; }

    public double Weight { get; set; }

    public string Pieces { get; set; }

    public Decimal ClearingCharge { get; set; }

    public int? ImportNo { get; set; }
  }
}
