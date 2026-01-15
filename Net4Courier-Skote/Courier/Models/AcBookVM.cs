using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcBookVM
    {
        public string TransactionNo { get; set; }
        public int transtype { get; set; }
        public string TransactionType { get; set; }
        public short paytype { get; set; }
        public DateTime transdate { get; set; }
        public string AcHead { get; set; }
        public Nullable<int> SelectedAcHead { get; set; }
        public string reference { get; set; }
        public string remarks { get; set; }
        public string bankname { get; set; }
        public string chequeno { get; set; }
        public DateTime chequedate { get; set; }
        public string partyname { get; set; }

        public string ReceivedFrom { get; set; }
        public int SelectedReceivedFrom { get; set; }
        public decimal amount { get; set; }
        public string remark1 { get; set; }
        public int TotalAmt { get; set; }

        public int SupplierId { get; set; }
        public int SupplierName { get; set; }
        public int SupplierTRNNo { get; set; }

        public string AcJournalDetail { get; set; }
        public string AcHeadAllocation { get; set; }

        public int AcBankDetailID { get; set; }
        public int AcJournalID { get; set; }
        public string VoucherType { get; set; }
        public string VoucherNo { get; set; }
        public decimal TaxPercent { get; set; }
        public int TaxAccountId { get; set; }
        public string TaxAccountName { get; set; }
        public decimal TaxAmount { get; set; }
        public string VoucherStatus { get; set; }
        public bool UpdateEnable { get; set; }
        public string BankReconc { get; set; }
        public List<AcJournalDetailVM> AcJDetailVM { get; set; }
        
    }

    public class AcJournalSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public List<AcJournalMasterVM> Details { get; set; }
    }
}