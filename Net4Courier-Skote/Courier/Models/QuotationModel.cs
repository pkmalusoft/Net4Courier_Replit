
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class QuotationVM : Quotation
    {
        public string CurrencyName { get; set; }
        public int SelectedQuotaionId { get; set; }
        public string NewVersion { get; set; }
        public int StatusTypeId { get; set; }
        public string CustomerName { get; set; }
        public List<QuotationDetailVM> QuotationDetails { get; set; }
    }
    public class QuotationDetailVM : QuotationDetail
    {
        public bool Deleted { get; set; }

    }

    public class QuotationSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string QuotationNo { get; set; }
        public List<QuotationVM> Details { get; set; }
    }

}