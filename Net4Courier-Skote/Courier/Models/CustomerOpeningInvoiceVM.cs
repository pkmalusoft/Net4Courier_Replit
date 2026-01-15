// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CustomerOpeningInvoiceVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class CustomerOpeningInvoiceVM
  {
    public string CustomerID { get; set; }

    public string Customer { get; set; }

    public string Type { get; set; }

    public int CustomerInvoiceNo { get; set; }

    public DateTime InvoiceDate { get; set; }

    public double OriginalAmt { get; set; }

    public double BalancedAmt { get; set; }

    public int AcHeadID { get; set; }

    public string Remarks { get; set; }
  }
}
