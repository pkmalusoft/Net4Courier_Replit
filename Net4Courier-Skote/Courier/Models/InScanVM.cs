// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.InScanVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
    public class InScanVM
    {
        public InScanVM()
        {
            lst = new List<AWBDetailsVM>();
        }
        public int QuickInscanID { get; set; }
        public int DepotID { get; set; }
        public string DepotName { get; set; }
        public string InScanSheetNo { get; set; }
        public int VehicleId { get; set; }
        public string DriverName { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public int CollectedByID { get; set; }
        public int AgentID { get; set; }
        public int ReceivedByID { get; set; }
        public DateTime QuickInscanDateTime { get; set; }
        public string SelectedInScanId { get; set; }
        public string RemovedInScanId { get; set; }
        public DateTime CollectedDate { get; set; }
        public string CollectedBy { get; set; }
        public string AgentName { get; set; }
        public string ReceivedBy { get; set; }

        public string MAWB { get; set; } //Shipment MAWB
        public DateTime ImportDate { get; set; } //shipment IMport Date
        public List<AWBDetailsVM> lst { get; set; }
        public string Details { get; set; }

        public List<InBoundAWBList>  AWBDetail {get; set; }
        public string Source { get; set; }
        public bool OutScanReturned { get; set; }
  }

public class InBoundAWBList
{
    public int Sno { get; set; }
    public int ShipmentDetailID { get; set; }
    public string ConsignorName { get; set; }
    public string AWB { get; set; }
    public string OriginCountry { get; set; }
    public string OriginCity { get; set; }

    public string DestinationCountry { get; set; }
    public string DestinationCity { get; set; }

    public string Receiver { get; set; }
    public string Shipper { get; set; }
    public bool AWBChecked { get; set; }

    public int? CourierStatusId { get; set; }
    public int? StatusTypeId { get; set; }
    public string CourierStatus { get; set; }
    public string StatusType { get; set; }
        public string MAWB { get; set; }
        public DateTime ImportDate { get; set; }
    public bool RemoveAllowed { get; set; }
        
}
    public class AWBList
    {
        public int InScanId { get; set; }
        public string AWB { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public bool AWBChecked { get; set; }
        public int ShipmentDetailId { get; set; }

        public int? CourierStatusId { get; set; }
        public int? DRSID { get; set; }
    }

    public class InScanSearch 
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
               
        public List<InScanVM> Details { get; set; }

    }

    public class CourierCollectionVM
    {
        public int CollectionId { get; set; }
        public string AWBNo { get; set; }
        public string Remarks { get; set; }
        public decimal PickupCash { get; set; }
        public decimal COD { get; set; }
        public decimal OtherAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal MaterialCost { get; set; }
        public string CollectionType { get; set; }
        public int DRSReceiptID { get; set; }
        public bool Deleted { get; set; }
        public bool ChangeCOD { get; set; }
        public bool CODPending { get; set; }
        public DateTime CollectedDate { get; set; }
        public string CourierName { get; set; }
        public int CourierID { get; set; }
        public int EmployeeID { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public int PaymentModeId { get; set; }
        public int DRSRecpayID { get; set; }
        public string LogDetail { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
