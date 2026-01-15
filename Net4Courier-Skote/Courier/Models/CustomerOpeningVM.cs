using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CustomerOpeningVM
    {
        public int AcOPInvoiceMasterId { get; set; }
        public int AcOPInviceDetailId { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public int AcJournalID { get; set; }
        public string CustomerType { get; set; }

    }
    public class CustomerOpeningSearch
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
       public List<CustomerOpeningVM> Details { get; set; }
    }

    public class SupplierOpeningVM
    {
        public int AcOPInvoiceMasterId { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public int AcJournalID { get; set; }

    }
    public class SupplierOpeningSearch
    {
        public int SupplierID { get; set; }
        public int SupplierTypeID { get; set; }
        public string SupplierName { get; set; }
        public List<SupplierOpeningVM> Details { get; set; }

    }
}