// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.PrintAWBVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class PrintAWBVM
  {
    public string HAWBNo { get; set; }

    public int InScanID { get; set; }

    public DateTime InScanDate { get; set; }

    public int customerid { get; set; }

    public string shipper { get; set; }

    public string shippercontact { get; set; }

    public string shipperaddres { get; set; }

    public int origincountry { get; set; }

    public int origincity { get; set; }

    public int originlocation { get; set; }

    public string ConsignorPhone { get; set; }

    public string EnquiryNo { get; set; }

    public string paymentmode { get; set; }

    public string code { get; set; }

    public string consignee { get; set; }

    public string consigneecontact { get; set; }

    public string ConsigneePhone { get; set; }

    public string Consigneeaddress { get; set; }

    public int destinationCountry { get; set; }

    public int destinationCity { get; set; }

    public int destinationLocation { get; set; }

    public string orilocation { get; set; }

    public string destlocation { get; set; }

    public Decimal CourierCharge { get; set; }

    public Decimal OtherCharge { get; set; }

    public Decimal PackingCharge { get; set; }

    public Decimal CustomCharge { get; set; }

    public Decimal totalCharge { get; set; }

    public Decimal ForwardingCharge { get; set; }

    public string remarks { get; set; }

    public Decimal materialcost { get; set; }

    public string Description { get; set; }

    public int Pieces { get; set; }

    public Decimal Weight { get; set; }

    public int CourierType { get; set; }

    public int CourierMode { get; set; }

    public int ProductType { get; set; }

    public int PickedBy { get; set; }

    public int ReceivedBy { get; set; }

    public int FagentID { get; set; }

    public string FAWBNo { get; set; }

    public Decimal VerifiedWeight { get; set; }

    public DateTime ForwardingDate { get; set; }

    public bool StatusAssignment { get; set; }

    public int TaxconfigurationID { get; set; }

    public string customer { get; set; }

    public string shippername { get; set; }

    public string consigneename { get; set; }

    public string origin { get; set; }

    public string destination { get; set; }

    public int AcJournalID { get; set; }
  }
}
