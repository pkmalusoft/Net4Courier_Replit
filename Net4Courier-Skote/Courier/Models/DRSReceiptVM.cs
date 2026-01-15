// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.DRSReceiptVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class DRSReceiptVM :DRSRecPay
  {
    public string DRSNo { get; set; }        
        public string CashBank { get; set; }
        //public string BankName { get; set; }
        //public int DeliveredBy { get; set; }
        public DateTime? DRSDate { get; set; }
        public decimal DRSAmount { get; set; }
        public string ChequeBank { get; set; }
        public string CourierEmpName { get; set; }
        public string DRRNo { get; set; }
   
        //public Nullable<System.DateTime> ChequeDate { get; set; }
        public List<DRSReceiptDetailVM> Details { get; set; }
        public List<CourierCollectionVM> AWBDetails { get; set; }
    }

    public class DRSReceiptDetailVM : DRSRecPayDetail
    {
                public string AWBDate { get; set; }
        public decimal TotalAmount { get; set; } 
        public decimal Balance { get; set; }
    }

    public class DRSReceiptSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string DocumentNo { get; set; }
        public List<DRSReceiptVM> Details { get; set; }
    }
    public class DRRSearchVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string DRRNo { get; set; }
        public List<DRRVM>  Details { get; set; }
    }
    public class DRRVM :DRR
    {
        public string DRSNo { get; set; }
        public string DRRNo { get; set; } //journal voucherno
        public DateTime DRSDate { get; set; }
        public string DRSReceiptNo { get; set; }
        public string DRSReceiptDate { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal ReceivedAmount { get; set; }
        public int DeliveredBy { get; set; }
        public string DeliveredByName { get; set; }
        public string UserName { get; set; }
        public List<DRRDetailVM> Details { get; set; }

    }

    public class DRRDetailVM
    {
        public int DRRDetailID { get; set; }
        public int? DRRID { get; set; }
        public int? ReferenceId { get; set; }
        public string Type { get; set; }
        public string Reference { get; set; }
        public decimal? COD { get; set; }
        public decimal? ChequeAmount { get; set; }
        public decimal? Discount { get; set; }
        public decimal? MCReceived { get; set; }
        public decimal? CODVat { get; set; }
        public decimal? PKPCash { get; set; }
        public decimal? Receipt { get; set; }
        public decimal? Expense { get; set; }
        public decimal? MCPayment { get; set; }
        public decimal? Total { get; set; }
        public string Confirmation { get; set; }

        public bool PODStatus { get; set; }

        public bool ChequeStatus { get; set; }
        public string AWBReceivedBy { get; set; }

        public decimal? SettlementAmount { get; set; }
        public string ChequeNo { get; set; }

        public bool TaxInclude { get; set; }
        public bool ChangeAWB { get; set; }
        public int PaymentModeId { get; set; }
        public int? RecPayID { get; set; }

        public int ChangePaymentModeId { get; set; }
        public decimal ChangeCourierCharge { get; set; }
        public decimal ChangeMaterialCost { get; set; }
        public bool ChangeMC { get; set; }
        public int ChangeCustomerId { get; set; }
        public string AWBDate1 { get; set; }
        public DateTime AWBDate { get; set; }
        public string TransactionType { get; set; }
        public decimal? MCPaid { get; set; }
        public int InScanID { get; set; }
        public int ShipmentDetailID { get; set; }
        public int CustomerId { get; set; }
        public int AcHeadID { get; set; }
        public string CustomerName { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Pieces { get; set; }
        public decimal Weight { get; set; }
        public string PaymentMode { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal TotalCharge { get; set; }
        public bool IsNCND { get; set; }
        public bool IsCashOnly { get; set; }
        public bool IsCollectMaterial { get; set; }
        public bool IsCheque { get; set; }
        public bool IsDoCopyBack { get; set; }
        public string CollectedBy { get; set; }
        public string LockedStatus { get; set; }
        public int TotalAWB {get;set;}
    }


    public class CourierExpenseSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int EmployeeID { get; set; }
        public List<CourierExpensesVM> Details { get; set; }
    }
    public class CourierExpensesVM : CourierExpens
    {
        public DateTime DRSDate { get; set; }
        public string DRSNo { get; set; }
        public decimal DRSAmount { get; set; }
        public string RevenueType { get; set; }
        public string CourierName { get; set; }
    }
    public class CourierCollectionSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int EmployeeID { get; set; }
        public List<CourierCollectionVM> Details { get; set; }
    }

    

}
