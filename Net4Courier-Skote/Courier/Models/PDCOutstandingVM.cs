using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  Net4Courier.Models
{
    public class PDCOutstandingVM
    {

        public int AcMemoJournalID { get; set; }
        public int AcJournalID { get; set; }
        public string VoucherNo { get; set; }
        public DateTime TransDate { get; set; }
        public string AcHead { get; set; }
        public decimal Amount { get; set; }
        public string ChequeNo { get; set; }
        public DateTime ChequeDate { get; set; }
        public DateTime VoucherDate { get; set; }
        public bool IsSelected { get; set; }

    }
}