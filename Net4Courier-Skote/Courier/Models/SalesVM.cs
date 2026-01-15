using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSV2.Models
{
    public class SalesInvoiceVM:SalesInvoice
    {
        public string  CustomerName { get; set; }

    }

    public class SalesInvoiceDetailVM:SalesInvoiceDetail
    {

    }

    public class SalesInvoiceSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public List<SalesInvoiceVM> Details { get; set; }
    }
}