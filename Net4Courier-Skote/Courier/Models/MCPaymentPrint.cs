using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{

    public class MCPaymentPrintSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AWBNo { get; set; }
        public bool Download { get; set; }
        public List<MCPaymentAWB> Details { get; set; }
    }
    public class MCPaymentMultiplePrint
    {
        public int MCPaymentVoucherId { get; set; }
        public string InscanID { get; set; }
        public bool MultiplePrint { get; set; }
    }
    public class MCPaymentPrint
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime PrintDate { get; set; }
        public string Shipper { get; set; }
        public string Receiver { get; set; }
        public string AWB { get; set; }     
        
        public int UserID { get; set; }

        public bool Printed { get; set; }

        public int DocumentNo { get; set; }
        public bool ConsignorAllSelected { get; set; }
        public bool AWBAllSelected { get; set; }
        public List<MCPaymentAWB> Details { get; set; }
        public List<MCPaymentConsignor> ConsignorDetails { get; set; }
    }


    public class MCPaymentConsignor
    {
        public string Consignor { get; set; }
        public bool ConsignorChecked { get; set; }
        
    }

    public class MCPaymentInScan
    {
        public int InScanID { get; set; }
        public string AWBNo { get; set; }
        public int DRRDetailID { get; set; }
        public string Consignor { get; set; }
        public bool AWBChecked { get; set; }
      
    }
    public class MCPaymentAWB
    {
        public int MCPaymentVoucherID { get; set; }

        public int MCPaymentVoucherDetailID { get; set; }
        public int DRRDetailID { get; set; }
        public DateTime DRRDate { get; set; }
        public int InScanID { get; set; }
        public string AWBNo { get; set; }

        public string VoucherNo { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string ConsigneeCountry { get; set; }
        public DateTime AWBDate { get; set; }
        public DateTime PrintDate { get; set; }
        public decimal MCAmount { get; set; }
        public decimal Receivedmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string DocumentNo { get; set; }

        public bool Printed { get; set; }
        public int TotalAWB { get; set; }

        public decimal TotalAmount { get; set; }
        public bool AWBChecked { get; set; }
        public bool Closed { get; set; }
        public DateTime? ClosedDate {get;set;}
        public string EmployeeName { get; set; }

    }
    public class MCPaymentVoucherVM
    {

        public int MCPaymentVoucherID { get; set; }
        public string VoucherNo { get; set; }
        public DateTime PrintDate { get; set; }

        public int DocumentNo { get; set; }
        public string Consignor { get; set; }
        public int TotalAWB { get; set; }
        public decimal TotalAmount { get; set; }

        public List<MCPaymentAWB> Details { get; set; }
    }
}