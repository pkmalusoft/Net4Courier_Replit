using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CODInvoiceVM:CODInvoice
    {
        public DateTime FromDate { get; set; }
        public string AWBNo { get; set; }
        public DateTime ToDate { get; set; }
        public int MovementTypeID { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
        public int TotalAWB { get; set; }
        public List<Net4Courier.Models.CODInvoiceDetailVM> Details { get; set; }
    }
    public class CODInvoiceDetailVM : CODInvoiceDetail
    {
        public DateTime AWBDateTime { get; set; }
        public string Origin { get; set; }
        public string Consignor { get; set; }
         
        public string ConsignorCountryName { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeCountryName { get; set; }
        public string ConsigneeCityName { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal? Weight { get; set; }
        public string Pieces { get; set; }
        public int? MovementId { get; set; }

        public int? ParcelTypeId { get; set; }
        public bool AWBChecked { get; set; }

    }
    public class CODInvoiceSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public List<CODInvoiceVM> Details { get; set; }
    }
}