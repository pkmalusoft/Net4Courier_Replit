// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.AWBillVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class AWBillVM
  {
    public string AWBNO { get; set; }

    public int InScanID { get; set; }

    public DateTime InScanDate { get; set; }

    public int? depotId { get; set; }

    public int AWBStatusID { get; set; }

    public int StatusDescriptionID { get; set; }

    public DateTime? Date { get; set; }

    public int statusUpdationID { get; set; }

    public string Remarks { get; set; }

    public string FormID { get; set; }

    public string Status { get; set; }

    public bool statusUpdate { get; set; }

    public int ReceivedBy { get; set; }

    public string Depot { get; set; }

    public string Customer { get; set; }

    public int? CustomerID { get; set; }

    public string code { get; set; }

    public string CustomerRateType { get; set; }

    public int CustomerRateTypeID { get; set; }

    public string paymentmode { get; set; }

    public int paymentmodeID { get; set; }

    public string shipper { get; set; }

    public string shipperphone { get; set; }

    public string shippercontact { get; set; }

    public string shipperaddress { get; set; }

    public int origincountry { get; set; }

    public int origincity { get; set; }

    public string originlocation { get; set; }

    public double Weight { get; set; }

    public string Pieces { get; set; }

    public double StatedWeight { get; set; }

    public string consignee { get; set; }

    public string consigneecontact { get; set; }

    public string ConsigneePhone { get; set; }

    public string Consigneeaddress { get; set; }

    public int destinationCountry { get; set; }

    public int destinationCity { get; set; }

    public string destinationLocation { get; set; }

    public string CargoDescription { get; set; }

    public string HandlingInstructions { get; set; }

    public int CourierType { get; set; }

    public string MaterialDescription { get; set; }

    public int CourierMode { get; set; }

    public int ProductType { get; set; }

    public Decimal MaterialCost { get; set; }

    public string ReferenceNumber { get; set; }

    public string movement { get; set; }

    public int movementID { get; set; }

    public string InvoiceValue { get; set; }

    public string SpecialInstructions { get; set; }

    public Decimal CourierCharges { get; set; }

    public Decimal ServiceCharges { get; set; }

    public Decimal OtherCharges { get; set; }

    public Decimal PackingCharges { get; set; }

    public Decimal Tax { get; set; }

    public Decimal txrate { get; set; }

    public Decimal TotalCharges { get; set; }

    public string ForwardingAgentName { get; set; }

    public int ForwardingAgentID { get; set; }

    public string ForwardingAWB { get; set; }

    public DateTime? ForwardingDate { get; set; }

    public Decimal ForwardingAgentRate { get; set; }

    public double VerifiedWeight { get; set; }

    public Decimal RevisedRate { get; set; }

    public Decimal ForwardingCharge { get; set; }

    public int Carrier { get; set; }

    public int CollectedByDetails { get; set; }

    public int Agent { get; set; }

    public Decimal collectedamt { get; set; }

    public Decimal pickupcharge { get; set; }

    public bool Courier { get; set; }

    public int? CourierMovement { get; set; }

    public int EmployeeID { get; set; }
  }
}
