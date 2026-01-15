// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.TaxConfiqrationVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class TaxConfigurationVM
  {
    public int TaxConfigurationID { get; set; }

    public int MoveMentID { get; set; }

    public int ParcelTypeId { get; set; }

    public Decimal TaxPercentage { get; set; }

    public int SaleHeadID { get; set; }

    public Decimal MinimumRate { get; set; }

    public int AcCompanyID { get; set; }

    public DateTime EffectFromDate { get; set; }

    public string Movement { get; set; }

    public string CourierType { get; set; } //Parceltype

    public string AcHead { get; set; }
  }
}
