using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  Net4Courier.Models
{
    public class AcJournalDetailsList
    {

        public int acHeadID { get; set; }
        public decimal Amount { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public short IsDebit { get; set; }
        public string AcRemark { get; set; }
        public string AcHead { get; set; }
        public int AcJournalDetID { get; set; }
        public bool IsDeleted { get; set; }
        public string drcr { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }

        public int ID { get; set; }

    }
}