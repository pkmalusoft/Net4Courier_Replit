// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.DRSVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class DRSVM
  {
    public int DRSID { get; set; }

    public string DRSNo { get; set; }

    public DateTime DRSDate { get; set; }

    public int? DeliveredBy { get; set; }

    public int CheckedBy { get; set; }

    public Decimal? TotalCourierCharge { get; set; }
        public Decimal? TotalMaterialCost{ get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalAWB { get; set; }
        public decimal TotalVAT { get; set; }
        public int APISuccess { get; set; }
        public int APIFailed { get; set; }
        public int? VehicleID { get; set; }
        public bool Pending { get; set; }
        public bool Delivered { get; set; }
        public string StatusDRS { get; set; }

    public int? AcCompanyID { get; set; }

    public bool? StatusInbound { get; set; }

    public string DrsType { get; set; }

    public string Depot { get; set; }
    public List<DRSDet> lst { get; set; }
    public string Deliver { get; set; }
        public string CourierName { get; set; }
    public string vehicle { get; set; }

    public Decimal Cash { get; set; }

    public Decimal MaterialCost { get; set; }
    public List<DRSDet> Details { get; set; }
  }
    public class DRSDet
    {
        public int SNo { get; set; }
        public int InScanID { get; set; }
        public bool deleted { get; set; }
        public string deletedclass { get; set; }
        public int ShipmentDetailID { get; set; }
        public string AWB { get; set; }
        public string consignor { get; set; }
        public string consignee { get; set; }
        public string city { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public decimal COD { get; set; }
        public decimal MaterialCost { get; set; }

        public decimal CustomValue { get; set; }

        public DateTime AWBDate { get; set; }
        public string CourierStatus { get; set; }
        public string PayMode { get; set; }
        public string Remarks { get; set; }

    }
    public class OutScanSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<DRSVM> DRSDetails { get; set; }

    }

    public class DRSAWBITEM
    {
        public int InscanId { get; set; }
        public int ShipmentDetailID { get; set; }
        public string AWBNo { get; set; }
        public string CustomerName { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public DateTime AWBDate { get; set; }
        public decimal CourierCharge { get; set; }
        public decimal CourierChargePending { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal  TotalCharge { get; set; }
        public decimal CODVatPending { get; set; }
        public decimal CODVAT { get; set; }
        public decimal  MaterialCost { get; set; }
        public bool IsNCND { get; set; }
        public bool IsCashOnly { get; set; }
        public bool IsCollectMaterial { get; set; }
        public bool IsCheque { get; set; }
        public bool IsDoCopyBack { get; set; }
        public string Pieces { get; set; }
        public decimal Weight { get; set; }
        public string PaymentMode { get; set; }
        public int PaymentModeId { get; set; }
        public string LockedStatus { get; set; } //--date  lock
        public string CollectedBy { get; set; }
    }
}
