// Decompiled with JetBrains decompiler
// Type: CMSV2.Models.FAgentAssignVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\CMSV2.dll

using System;
using System.Collections.Generic;

namespace CMSV2.Models
{
  public class FAgentAssignVM
  {
    public int CountryID { get; set; }

    public string Country { get; set; }

    public DateTime Date { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string FAgentAWB { get; set; }

    public int InScanInternationalID { get; set; }

    public int FAgentID { get; set; }

    public string ForwardingAWBNo { get; set; }

    public DateTime ForwardingDate { get; set; }

    public double VerifiedWeight { get; set; }

    public Decimal ForwardingCharge { get; set; }

    public bool StatusAssignment { get; set; }

    public string Flight { get; set; }

    public string BagNo { get; set; }

    public string RunNo { get; set; }

    public string CDNo { get; set; }

    public int? EmployeeID { get; set; }

    public string Remarks { get; set; }

    public string DocumentNo { get; set; }

    public string MAWBNo { get; set; }

    public DateTime? MAWBDate { get; set; }

    public DateTime? FlightDate { get; set; }

    public int? InScanID { get; set; }

    public int? ShippingCountryID { get; set; }

    public int? ShippingDepotID { get; set; }

    public int? ReceivingCountryID { get; set; }

    public int? ReceivingDepotID { get; set; }

    public Decimal? ForwardingAgentRate { get; set; }

    public Decimal? RevisedRate { get; set; }

    public int? Carrier { get; set; }

    public int? MAWB { get; set; }

    public double TotalWeight { get; set; }

    public double TotalCharge { get; set; }

    public List<AWBVM> lst { get; set; }
  }
}
