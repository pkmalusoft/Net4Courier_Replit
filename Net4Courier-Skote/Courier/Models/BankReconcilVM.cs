using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class BankReconcilVM
    {
        public int AcBankDetailID { get; set; }
        public int AcJournalID { get; set; }
        public string BankName { get; set; }
        public string ChequeNo { get; set; }
        public DateTime ChequeDate { get; set; }
        public string PartyName { get; set; }
        public string StatusTrans { get; set; }
        public bool StatusReconciled { get; set; }
        public DateTime ValueDate { get; set; }
        public bool IsSelected { get; set; }
        public string VoucherNo { get; set; }
        public DateTime TransDate { get; set; }
        public string AcHead { get; set; }
        public string Remarks { get; set; }

      
        public DateTime VoucherDate { get; set; }
        public decimal Amount { get; set; }

        List<BankReconcilVM> list1 { get; set; }
    }
}