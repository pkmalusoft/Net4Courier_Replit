using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class ShipmentInvoiceVM:ShipmentInvoice
    {
        public string EnteredBy { get; set; }
    //    public string MAWB { get; set; }
        public decimal TaxPercent { get; set; }
        public DateTime ImportDate { get; set; }
        public string AWBDetails { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal VatTotal { get; set; }
        public decimal AdminCharges { get; set; }
        public List<ShipmentInvoiceDetailVM> Details { get; set; }

    }

    public class ShipmentInvoiceDetailVM : ShipmentInvoiceDetail
    {
        public DateTime AWBDate { get; set; }
        public decimal CustomValue { get; set; }
        public bool AWBChecked { get; set; }
        public bool ZeroTax { get; set; }
        public string AWBNo { get; set;}
        public string Shipper { get; set; }
        public string Receiver { get; set; }
        public string BagNo { get; set; }
       
        public string CurrencyName { get; set; }
    }

    public class ShipmentInvoiceSearch
    {
        public string AWBNo { get; set; }
        public int ShipmentInvoiceDetailId { get; set; }
        public int ShipmentInvoiceId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ShipmentInvoiceVM> Details { get; set; }
    }
}