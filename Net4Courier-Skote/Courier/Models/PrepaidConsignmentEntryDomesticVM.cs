// Decompiled with JetBrains decompiler
// Type: CMSV2.Models.PrepaidConsignmentEntryDomesticVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\CMSV2.dll

using System;

namespace CMSV2.Models
{
  public class PrepaidAWBVM:PrepaidAWB
  {
    public string AWBNO { get; set; }

    public string Shipper { get; set; }

    public string Address { get; set; }

    public int ShipperPhone { get; set; }

    public string City { get; set; }

    public string CityID { get; set; }

    public string Location { get; set; }

    public Decimal Weight { get; set; }

    public int pieces { get; set; }

    public int movementID { get; set; }

   
    public Decimal OtherCharge { get; set; }

     

    public string ProductType { get; set; }

    public int CollectedByID { get; set; }

    public int ReceivedByID { get; set; }

    public string CollectedBy { get; set; }

    public string ReceivedBy { get; set; }

    public string Remark { get; set; }

    public DateTime InscanTime { get; set; }

    public string FAgent { get; set; }

    public string FAWBNo { get; set; }

    public DateTime FDate { get; set; }

    public Decimal WeightVerified { get; set; }

    public Decimal ForwardingCharge { get; set; }

    public Decimal RevisedCharge { get; set; }

    public Decimal MaterialCost { get; set; }  

    public string Remarks { get; set; }
  }
    public class PrepaidAWBDetailVM:PrepaidAwbDetail
    {

    }
}
