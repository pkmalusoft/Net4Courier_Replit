using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CODReceiptVM : ManifestCODReceipt
    {
        
        public string AgentName {get;set;}
        public string AcHeadName { get; set; }
        public int[] SelectedValues { get; set; }
        public decimal allocatedtotalamount { get; set; }
        public string InvoiceNo { get; set; }
        
        public string CurrencyName { get; set; }
        public List<CODReceiptDetailVM> ReceiptDetails { get; set; }
    }
    public class CODReceiptDetailVM :ManifestCODReceiptDetail
    {
        public bool AWBChecked { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime AWBDate { get; set; }
        public string Shipper { get; set; }
        public string Receiver { get; set; }
    }


    public class CODReceiptSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public int ShipmentInvoiceID { get; set; }
        public List<CODReceiptVM> Details { get; set; }
    }

}