using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class SupplierInvoiceVM:SupplierInvoice
    {
        public string SupplierName { get; set; }
        public string SupplierType { get; set; }
        public int SupplierTypeId { get; set; }
        public decimal Amount { get; set; }
        public int ItemTypeId { get; set; }        
        public int ItemTypeId1 { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }        
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
        public decimal AdditionalCharges { get; set; }
        public decimal FuelPercentage { get; set; }
        public List<SupplierInvoiceDetailVM> Details { get; set; }
    }
    public class SupplierInvoiceDetailVM
    {
        public int? AcHeadId { get; set; }
        public string AcHeadName { get; set; }
        public int SupplierInvoiceDetailID { get; set; }
        public int SupplierInvoiceID { get; set; }
        public string Particulars { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public int CurrencyID { get; set; }
        public decimal CurrencyAmount { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Value { get; set; }
        public string InvNo { get; set; }
        public string Currency { get; set; }

        public string BookNo { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemId { get; set; }
        public int AWBStart { get; set; }
        public int AWBEnd { get; set; }
        public int AWBCount { get; set; }
        public int ItemQty { get; set; }



        public string ItemSize { get; set; }
    }

    public class SupplierInvoiceAWBVM : SupplierInvoiceAWB
    {
        public int TruckDetailID { get; set; }
        public string ConsignmentNo { get; set; }
        public DateTime ConsignmentDate { get; set; }
    }

    public class SupplierInvoiceSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int SupplierId { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
        public string SupplierName { get; set; }
        public int SupplierTypeId { get; set; }
        public string InvoiceNo { get; set; }
        public List<SupplierInvoiceVM> Details { get; set; }
    }
}
