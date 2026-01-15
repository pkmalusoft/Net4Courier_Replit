// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.ForwardingAgentInvoiceVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class ForwardingAgentInvoiceVM
  {
    public int FAInvoiceID { get; set; }

    public string FAInvoiceNo { get; set; }

    public DateTime FAInvoiceDate { get; set; }

    public int FAgentID { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string Column { get; set; }

    public string value { get; set; }

    public double Fuel { get; set; }

    public double FuelSurchargePer { get; set; }

    public List<Net4Courier.Models.ForwardingAgentInvoiceDetailsVM> ForwardingAgentInvoiceDetailsVM { get; set; }

    public List<Net4Courier.Models.FuelSurchargeVM> FuelSurchargeVM { get; set; }
  }
}
