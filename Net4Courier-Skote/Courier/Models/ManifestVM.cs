// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.ManifestVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class ManifestVM
  {
    public string Shipper { get; set; }

    public string courier { get; set; }

    public DateTime date { get; set; }

    public string awbno { get; set; }

    public string consignee { get; set; }

    public string destination { get; set; }

    public int noofpcs { get; set; }

    public double weight { get; set; }

    public string type { get; set; }

    public string contents { get; set; }

    public string remark { get; set; }

    public string prepby { get; set; }

    public string checkedby { get; set; }

    public string pickupcourier { get; set; }
  }
}
