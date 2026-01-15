using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace  Net4Courier.Models
{
    public class CustomerRcieptVM
    {
        public CustomerRcieptVM() {

            CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
        }
        public int RecPayDetailID { get; set; }
        public int RecPayID { get; set; }

        public System.DateTime RecPayDate { get; set; }
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
        public decimal FMoney { get; set; }
        public Nullable<int> UserID { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string customerName { get; set; }
        public string JobID { get; set; } 
        public string Jobcode { get; set; }
        public decimal salesHome { get; set; }
        public decimal AmountToBeRecieved { get; set; }
        public decimal AmountToBePaid { get; set; }
        public int InvoiceID { get; set; }
        public Nullable<System.DateTime> AmtPaidTillDate { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set;}
        public decimal AllocatedAmount { get; set; }
        public int SupplierTypeId { get; set; }

        public int AcOPInvoiceDetailID { get; set; }
        public string ReceiptType { get; set; }
        public string OPRefNo { get; set; }
        public decimal OtherReceipt { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string EntryType { get; set; }
        public List<CustomerRcieptChildVM> CustomerRcieptChildVM { get; set; }
        public List<RecPayDetail> recPayDetail { get; set; }
        public List<ReceiptAllocationDetailVM> AWBAllocation { get; set; }
    }


    public class CustomerRcieptChildVM
    {
       // public CustomerRcieptChildVM() { }
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
        public string  JobCode { get; set; }
        public int InvoiceID { get; set; }
        public bool Allocated { get; set; }
        public int AcOPInvoiceDetailID { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceNo { get; set; }        
        public decimal salesHome { get; set; }
        public decimal AmountToBeRecieved { get; set; }
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



}