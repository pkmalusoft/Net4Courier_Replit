using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CreditNoteVM
    {
        public string VoucherNo { get; set; }
        public int CreditNoteID { get; set; }
        public string CreditNoteNo { get; set; }
        public DateTime Date { get; set; }
        public int CustomerID { get; set; }
        public  bool ForInvoice { get; set; }
        public int AcHeadID { get; set; }
        public int AcDetailHeadID { get; set; }
        public int AcDetailAmount { get; set; }
        public string AcDetailAmountType { get; set; }
        public string AcDetailRemarks { get; set; }

        public int AcJournalID { get; set; }
        public string AcHeadName { get; set; }
        public string ReferenceType { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string JobNO { get; set; }
        public string InvoiceNo { get; set; }
        public decimal HDebitAmount { get; set; }
        public decimal HCreditAmount { get; set; }
        public int InvoiceID { get; set; }
        public int RecPayID { get; set; }

        public string RefType { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public bool TradingInvoice { get; set; }
        public string CustomerType { get; set; }
        public string AmountType { get; set; }
        public string TransType { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public List<CustomerRcieptChildVM> CustomerRcieptChildVM { get; set; }
        public List<CreditNoteDetailVM> Details { get; set; }
    }

    public class CreditNoteDetailVM :CreditNoteDetail
    {
        public string AcHeadName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public bool deleted { get; set; }
    }

    public class CustomerReceiptChildVM
    {
        // public CustomerReceiptChildVM() { }
        public int RecPayID { get; set; }
        public int RecPayDetailID { get; set; }
        public Nullable<System.DateTime> RecPayDate { get; set; }
        public string DocumentNo { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<int> SupplierID { get; set; }
        public Nullable<int> BusinessCentreID { get; set; }
        public string CashBank { get; set; }
        public string BankName { get; set; }
        public string ChequeBank { get; set; }
        public string ChequeNo { get; set; }
        public Nullable<System.DateTime> ChequeDate { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> AcJournalID { get; set; }
        public Nullable<bool> StatusRec { get; set; }
        public string StatusEntry { get; set; }
        public string StatusOrigin { get; set; }
        public Nullable<int> FYearID { get; set; }
        public Nullable<int> AcCompanyID { get; set; }
        public Nullable<decimal> EXRate { get; set; }
        public Nullable<decimal> FMoney { get; set; }
        public Nullable<int> UserID { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CustomerName { get; set; }
        public int JobID { get; set; }
        public string JobCode { get; set; }
        public int InvoiceID { get; set; }
        public bool Allocated { get; set; }
        public int AcOPInvoiceDetailID { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal salesHome { get; set; }
        public decimal AmountToBeRecieved { get; set; }
        public decimal ReceivedAmount { get; set; }
        public Nullable<decimal> AmtPaidTillDate { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountToBePaid { get; set; }
        public string strDate { get; set; }
        public decimal? Amount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string SInvoiceNo { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }

        public bool? IsTradingReceipt { get; set; }

    }
    public class CreditNoteSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public string CreditNoteNo { get; set; }
        public string InvoiceNo { get; set; }
        public List<CreditNoteVM> Details { get; set; }
    }
}