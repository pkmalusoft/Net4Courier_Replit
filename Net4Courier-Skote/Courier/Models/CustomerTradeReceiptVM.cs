using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CustomerTradeReceiptVM
    {
        public int RecPayDetailID { get; set; }
        public string InvoiceNo { get; set; }
        public int SalesInvoiceDetailID { get; set; }
        public Nullable<int> SalesInvoiceID { get; set; }
        public Nullable<int> ProductID { get; set; }
        public int AcOPInvoiceDetailID { get; set; }
        public string InvoiceType { get; set; }
        public string ProductName { get; set; }       
        public Nullable<decimal> NetValue { get; set; }
        public Nullable<int> JobID { get; set; }
        public string JobCode { get; set; }
        public string Description { get; set; }
        public DateTime? date { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? AmountReceived { get; set; }
        public decimal? Balance { get; set; }
        public string DateTime { get; set; }
        public decimal? AdjustmentAmount { get; set; }
        public bool Allocated { get; set; }
        public decimal? Amount { get; set; }

        public string Remarks { get; set; }


        public List<ReceiptAllocationDetailVM> AWBAllocation { get; set; }
    }

    public class ReceiptAllocationDetailVM
    {
        public int ID { get; set; }
        public int RecPayID { get; set; }
        public int RecPayDetailID { get; set; }
        public int CustomerInvoiceId { get; set; }
        public int CustomerInvoiceDetailID { get; set; }
        
        public int InScanID { get; set; }
        public string AWBNo { get; set; }
        public string AWBDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public bool Allocated { get; set; }

    }
}
