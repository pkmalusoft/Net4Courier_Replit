using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcInvoiceOpeningVM :AcOPInvoiceMaster
    {
        public string PartyName { get; set; }
        public string PartyType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public int AcOPInvoiceDetailId { get; set; }
        public int AcHeadId { get; set; }
        public string AccountName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerType { get; set; }
        

        public List<AcInvoiceOpeningDetailVM> InvoiceDetailVM { get; set; }
    }
    public class AcInvoiceOpeningDetailVM : AcOPInvoiceDetail
    {
        public bool IsDeleted { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class CustomerInvoiceOpeningVM
    {
        public int SupplierTypeID { get; set; }
        public int PartyID { get; set; }
        public string PartyName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string StatusSDSC { get; set; }
        public int AcOPInvoiceDetailId { get; set; }
        public int AcOPInvoiceMasterID { get; set; }
        public int AcHeadID { get; set; }
        public string AccountName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate
        {
            get; set;
        }
    }


}