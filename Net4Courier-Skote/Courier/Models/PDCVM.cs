using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  Net4Courier.Models
{
    public class PDCVM
    {
        public int transtype { get; set; }

        public string TransactionType { get; set; }

    
        public DateTime transdate { get; set; }
        public int AcHead { get; set; }
       
        public string remarks { get; set; }
        public string bankname { get; set; }
        public string chequeno { get; set; }
        public DateTime chequedate { get; set; }
        public string partyname { get; set; }

        public int ReceivedFrom { get; set; }
        public decimal amount { get; set; }
        public string remark1 { get; set; }
        public int TotalAmt { get; set; }

        public int AcBankDetailID { get; set; }
        public int AcJournalID { get; set; }
        public string VoucherType { get; set; }
        public string VoucherNo { get; set; }
        //public string AcHeadAllocation { get; set; }

        public List<AcMemoJournalDetailVM> AcJMDetailVM { get; set; }
    }
}