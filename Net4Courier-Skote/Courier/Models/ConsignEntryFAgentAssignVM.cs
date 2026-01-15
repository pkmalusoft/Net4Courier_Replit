// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.ConsignEntryFAgentAssignVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class ConsignEntryFAgentAssignVM
  {
    public int FAgentID { get; set; }

    public string FAgentAWB { get; set; }

    public DateTime InscanTime { get; set; }

    public DateTime FDate { get; set; }

    public double VerifiedWeight { get; set; }

    public Decimal ForwardingCharge { get; set; }

    public Decimal? RevisedRate { get; set; }

    public Decimal MaterialCost { get; set; }

    public string Reference { get; set; }

    public string Remarks { get; set; }

    public bool NCND { get; set; }

    public bool IsCheque { get; set; }

    public bool DOCopyBack { get; set; }

    public bool CashOnly { get; set; }

    public bool CollectMaterial { get; set; }
  }
}
