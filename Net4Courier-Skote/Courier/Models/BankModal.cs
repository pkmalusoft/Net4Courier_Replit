using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
   [Serializable]
    public class BankModal
    {
        public HttpPostedFile CsvDoc { get; set; }
        public string cboProduct { get; set; }
        public DateTime dtpFrom { get; set; }
        public DateTime dtpTo { get; set; }
        public string cboBranch { get; set; }
        public bool chkSection { get; set; }
        public bool chkProduct { get; set; }
        public bool chkInvoice { get; set; }
        public List<CsvRecordsViewModel> ResultViewModel { get; set; }
        public BankModal()
        {
            ResultViewModel = new List<CsvRecordsViewModel>();
        }
    }
    public class BankReconcSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string  FilterStatus { get; set; }
        public  string ChangeStatus { get; set; }
        public DateTime ReconcDate { get; set; }
        public int BankHeadID { get; set; }
        public int DepositHeadID { get; set; }
        public string BankName { get; set; }
        public bool AllSelected { get; set; }
        public decimal LedgerBalance { get; set; }
        public List<BankDetails> Details { get; set; }
        public List<BankDetails> CSVDetails { get; set; }
    }

    /// <summary>
    /// this class used for bank reconciliation and bankdeposit
    /// </summary>
    public class BankDetails
    {
        public int AcJournalID { get; set; }
        public string  VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime? ReconcDate { get; set; }
        public string TransType { get; set; }
        public string VoucherType { get; set; }
        public string VoucherType1 { get; set; }
        public int AcheadId  { get; set; }
        public string ChequeNo { get; set; }
        public DateTime ChequeDate { get; set; }
        public int AcBankDetailID { get; set; }
        public string PartyName { get; set; }
        public bool StatusReconciled { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; }
        public string Remarks { get; set; }
        public decimal BankCharges { get; set; }
       public int PDCOpeningID { get; set; }
        public string ChangeStatus { get; set; }
        public string TransactionPage { get; set; }
        public int ReferenceId { get; set; }
        public string DepositHeadName { get; set; }
    }

}