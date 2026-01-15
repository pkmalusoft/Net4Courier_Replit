using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity;
using Net4Courier.DAL;
namespace Net4Courier.Models
{
    public class CustomerInvoiceDetailForReceipt
        {
        public string InvoiceNo { get; set; }
        public  int CustomerInvoiceID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal AmountToBeReceived { get; set; }
        public decimal AmtPaidTillDate { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public decimal Advance { get; set; }
        public string CurrencyName { get; set; }
        }
    public class ReceiptVM
        {
        public int RecPayID { get; set; }
        public DateTime RecPayDate { get; set; }
        public string DocumentNo { get; set; }
        public string PartyName { get; set; }
        public string Remarks { get; set; }
        public string BankName { get; set; }
        public decimal? Amount { get; set; }

        public string PaymentMode { get; set; }

        public string ChequeNo { get; set; }
        public string ChequeDate { get; set; }
        public decimal Currency { get; set; }

      
        public int CustomerID { get; set; }
        public string RecPayDate1 { get; set; }
        
        
        public decimal AllocatedAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal UnAllocatedAmount { get; set; }
        public decimal OtherReceipt { get; set; }
        public int RecPayDetailID { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
    }

    public class CustomerReceiptSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string ReceiptNo { get; set; }
        public bool AllocationPending { get; set; }
        public string InvoiceNo { get; set; }
        public string EntryType { get; set; }
        public int SupplierID { get; set; }
        public int SupplierTypeId { get; set; }
        public string SupplierName { get; set; }
        public List<ReceiptVM> Details { get; set; }
    }

  
    public class OpeningInvoiceVM
    {
        public int ACOPInvoiceDetailId { get; set; }
        public int CustomerId { get; set; }
        public int SupplierId { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string RefNo { get; set; }

        public decimal Amount { get; set; }
    }

    public class CustomerReceivable
    {
        public int InvoiceId { get; set; }
        public decimal Receivable{ get; set; }
    }
    public class RecieptPaymentModel
    {
        Entities1 Context1 = new Entities1();

        //public List<SP_GetAllRecieptsDetails_Result> GetAllReciepts()
        //{
        //    return Context1.SP_GetAllRecieptsDetails().ToList();
        //}

        //public List<SP_GetAllPaymentsDetails_Result> GetAllPayments()
        //{
        //    return Context1.SP_GetAllPaymentsDetails().ToList();
        //}

        public string GetMaxRecieptDocumentNo()
        {
            string Docno = "";

            var quary = Context1.SP_GetMaxRVID();

            foreach (var item in quary)
            {

                Docno = item.ToString();
            }

            return Docno;
        }

        public string GetMaxPaymentDocumentNo()
        {
            string Docno = "";

            Docno=ReceiptDAO.SP_GetMaxPVID();
            
            return Docno;
        }


        public CurrencyMaster GetExchgeRateByCurID(int CurID)
        {
            // string ExRate = "";

            return Context1.CurrencyMasters.Find(CurID);

            //foreach (var item in quary)
            //{
            //    ExRate = item.ToString();
            //}

            // return ExRate;
        }

        //public List<SP_GetCustomerInvoiceDetailsForReciept_Result> GetCustomerInvoiceDetails(int CustomerID,DateTime fromdate,DateTime todate)
        //{

        //    //todo:fix to run by sethu
        //    //  return Context1.SP_GetCustomerInvoiceDetailsForReciept(CustomerID, fromdate, todate).ToList();
        //    return Context1.SP_GetCustomerInvoiceDetailsForReciept(CustomerID).ToList();
        //}

        //public List<SP_GetSupplierCostDetailsForPayment_Result> GetSupplierCostDetails(int SupplierID)
        //{
        //    return Context1.SP_GetSupplierCostDetailsForPayment(SupplierID).ToList();
        //}

        //public int AddCustomerRecieptPayment(CustomerRcieptVM RecPy, string UserID)
        //{
        //    int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));

        //    return query;
        //}

        public decimal GetAdvanceAmount(int CustomerID)
        {
            decimal AdvanceAmt = 0;

            var query = (from r in Context1.RecPays
                         join r1 in Context1.RecPayDetails on r.RecPayID equals r1.RecPayID
                         where r1.StatusAdvance == true && r1.StatusInvoice == "C" && r.CustomerID == CustomerID
                         select r1.Amount).FirstOrDefault();

                         // Context1.SP_GetAdvanceAmountOfCustomer(CustomerID);

            //foreach (var item in query)
            //{
            //    AdvanceAmt = Convert.ToDecimal(item);
            //}

            AdvanceAmt = Convert.ToDecimal(query);

            return AdvanceAmt;
        }

        //public int InsertRecpayDetailsForCust(int RecPayID, int InvoiceID,int JInvoiceID, decimal Amount, string Remarks, string StatusInvoice, bool StatusAdvance, string statusReceip, string InvDate, string InvNo, int CurrencyID, int invoiceStatus,int JobID)
        //{
        //    int query = Context1.SP_InsertRecPayDetailsForCustomer(RecPayID, JInvoiceID
        //        , Amount, Remarks, StatusInvoice
        //        , StatusAdvance, statusReceip, InvDate
        //        , InvNo, CurrencyID, invoiceStatus, JobID);

        //    return query;
        //}

        //public int InsertRecpayDetailsForSup(int RecPayID, int InvoiceID,int JInvoiceID, decimal Amount, string Remarks, string StatusInvoice, bool StatusAdvance, string statusReceip, string InvDate, string InvNo, int CurrencyID, int invoiceStatus, int JobID)
        //{
        //    //todo:fix to run by sethu
        //    int query = Context1.SP_InsertRecPayDetailsForSupplier(RecPayID, InvoiceID, Amount, Remarks, StatusInvoice, StatusAdvance, statusReceip, InvDate, InvNo, CurrencyID, invoiceStatus, JobID);

        //    return query;
        //}

        public CustomerRcieptVM GetRecPayByRecpayID(int RecpayID)
        {
            CustomerRcieptVM cust = new CustomerRcieptVM();

            if (RecpayID <= 0)
                return new CustomerRcieptVM();
            var query = Context1.RecPays.Find(RecpayID); // .SP_GetCustomerRecieptByRecPayID(RecpayID);

            if (query != null)
            {
                var item = query; //.FirstOrDefault();
                cust.RecPayDate = item.RecPayDate;
                cust.DocumentNo = item.DocumentNo;
                cust.CustomerID = item.CustomerID;
                cust.customerName = Context1.CustomerMasters.Find(item.CustomerID).CustomerName;
                var cashOrBankID = (from t in Context1.AcHeads where t.AcHead1 == item.BankName select t.AcHeadID).FirstOrDefault();
                cust.CashBank = (cashOrBankID).ToString();
                cust.ChequeBank = (cashOrBankID).ToString();
                cust.ChequeNo = item.ChequeNo;
                cust.ChequeDate = item.ChequeDate;
                cust.Remarks = item.Remarks;
                cust.EXRate = item.EXRate;
                cust.FMoney = item.FMoney;
                cust.RecPayID = item.RecPayID;
                cust.SupplierID = item.SupplierID;
                cust.AcJournalID = item.AcJournalID;
                cust.StatusEntry = item.StatusEntry;
                cust.StatusOrigin = item.StatusOrigin;
                cust.AcOPInvoiceDetailID = 0;
                if (cust.StatusEntry == "CS")
                    cust.CashBank = (cashOrBankID).ToString();
                else if (cust.StatusEntry == "BK")
                    cust.ChequeBank = (cashOrBankID).ToString();
                else
                {
                    cust.OPRefNo = item.BankName;
                    if (item.AcOPInvoiceDetailID!=null)
                    cust.AcOPInvoiceDetailID =Convert.ToInt32(item.AcOPInvoiceDetailID);

                }

                cust.BankName = item.BankName;
                var a = (from t in Context1.RecPayDetails where t.RecPayID == RecpayID select t.CurrencyID).FirstOrDefault();
                cust.CurrencyId = Convert.ToInt32(a.HasValue ? a.Value : 0);

                if (item.OtherReceipt == null)
                {
                    cust.OtherReceipt = 0;
                }
                else
                {
                    cust.OtherReceipt = Convert.ToDecimal(item.OtherReceipt);
                }

            }

            else
            {
                return new CustomerRcieptVM();
            }

            return cust;
        }

        public CustomerRcieptVM GetSupplierRecPayByRecpayID(int RecpayID)
        {
            CustomerRcieptVM cust = new CustomerRcieptVM();
            if (RecpayID <= 0)
                return new CustomerRcieptVM();
            var query = Context1.RecPays.Find(RecpayID);

            if (query != null)
            {
                var item = query;
                cust.RecPayDate = item.RecPayDate;
                cust.DocumentNo = item.DocumentNo;
                cust.SupplierID = item.SupplierID;
                var supplier = Context1.SupplierMasters.Find(item.SupplierID);
                cust.customerName = supplier.SupplierName;
                cust.SupplierTypeId =Convert.ToInt32(supplier.SupplierTypeID);
                var cashOrBankID = (from t in Context1.AcHeads where t.AcHead1 == item.BankName select t.AcHeadID).FirstOrDefault();
                cust.CashBank = (cashOrBankID).ToString();
                cust.ChequeBank = (cashOrBankID).ToString();
                cust.ChequeNo = item.ChequeNo;
                cust.ChequeDate = item.ChequeDate;
                cust.Remarks = item.Remarks;
                cust.EXRate = item.EXRate;
                cust.FMoney = item.FMoney;
                cust.RecPayID = item.RecPayID;
                cust.AcJournalID = item.AcJournalID;
                cust.StatusEntry = item.StatusEntry;
                cust.AcOPInvoiceDetailID = 0;
                if (cust.StatusEntry == "CS")
                    cust.CashBank = (cashOrBankID).ToString();
                else if (cust.StatusEntry == "BK")
                    cust.ChequeBank = (cashOrBankID).ToString();
                else
                {
                    cust.OPRefNo = item.BankName;
                    if (item.AcOPInvoiceDetailID != null)
                        cust.AcOPInvoiceDetailID = Convert.ToInt32(item.AcOPInvoiceDetailID);
                }
                var a = (from t in Context1.RecPayDetails where t.RecPayID == RecpayID select t.CurrencyID).FirstOrDefault();
                if (a.HasValue)
                    cust.CurrencyId = Convert.ToInt32(a.Value);


            }

            return cust;
        }

        public int GetMaxRecPayID()
        {
            int RecPayID = 0; 
            
            var x= Context1.RecPays.OrderByDescending(item => item.RecPayID).FirstOrDefault();
            if (x == null)
            {

                RecPayID= 1;
            }
            else
            {
                RecPayID = x.RecPayID+ 1;
            }
            
            return RecPayID;
        }

        //public void InsertJournalOfCustomer(int RecpayID, int fyaerId)
        //{
        //    Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        //}

        //public void InsertJournalOfSupplier(int RecpayID, int fyaerId)
        //{
        //    Context1.SP_InsertJournalEntryForRecPay_SupplierPayment(RecpayID, fyaerId);
        //}

        //public int UpdateCostStatus(int CostUpdationID)
        //{
        //    int i = Context1.SP_UpdateCostUpdatonStatus(CostUpdationID);

        //    return i;
        //}

        //public List<CustomerInvoice> InvDtls(int InvoiceID)
        //{

        //    return Context1.CustomerInvoices.Find(InvoiceID); // .SP_GetJInvoiceDetailsByInvoiceID(InvoiceID).ToList();
        //}

        //public int DeleteCustomerDetails(int RecPayID)
        //{
        //    int i = Context1.SP_DeleteCustomerReciepts(RecPayID);

        //    return i;
        //}

        //public int DeleteSupplierDetails(int RecPayID)
        //{
        //    int i = Context1.SP_DeleteSupplierPayment(RecPayID);

        //    return i;
        //}

        public int EditCustomerRecieptDetails(List<RecPayDetail> rpayDetails, int recpayID)
        {
            //code for edit
            try
            {
                foreach (var CU in rpayDetails)
                {
                    if (recpayID > 0)
                    {
                        RecPayDetail objrpayDetails = Context1.RecPayDetails.Where(item => item.RecPayDetailID == CU.RecPayDetailID).FirstOrDefault();
                        objrpayDetails.Amount = CU.Amount;
                        objrpayDetails.CurrencyID = CU.CurrencyID;
                        objrpayDetails.InvDate = CU.InvDate;

                        objrpayDetails.InvNo = CU.InvNo;

                        objrpayDetails.InvoiceID = CU.InvoiceID;

                        objrpayDetails.Remarks = CU.Remarks;
                        Context1.Entry(objrpayDetails).State = EntityState.Modified;
                    }
                    Context1.SaveChanges();
                }
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }


        }


        public int EditSupplierRecieptDetails(List<RecPayDetail> rpayDetails, int recpayID)
        {
            //code for edit
            try
            {
                foreach (var CU in rpayDetails)
                {
                    if (recpayID > 0)
                    {
                        RecPayDetail objrpayDetails = Context1.RecPayDetails.Where(item => item.RecPayDetailID == CU.RecPayDetailID).FirstOrDefault();
                        objrpayDetails.Amount = CU.Amount;
                        objrpayDetails.CurrencyID = CU.CurrencyID;
                        objrpayDetails.InvDate = CU.InvDate;

                        objrpayDetails.InvNo = CU.InvNo;

                        objrpayDetails.InvoiceID = CU.InvoiceID;

                        objrpayDetails.Remarks = CU.Remarks;
                        Context1.Entry(objrpayDetails).State = EntityState.Modified;
                    }
                    Context1.SaveChanges();
                }
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }


        }

        public int EditCustomerRecPay(CustomerRcieptVM RecPy, string UserID)
        {
            if (RecPy.RecPayID > 0)
            {
                try
                {
                    //Edit Code
                    RecPay objRecPay = Context1.RecPays.Where(item => item.RecPayID == RecPy.RecPayID).FirstOrDefault();

                    objRecPay.BankName = RecPy.BankName;
                    objRecPay.ChequeDate = RecPy.ChequeDate;
                    objRecPay.ChequeNo = RecPy.ChequeNo;
                    objRecPay.EXRate = RecPy.EXRate;
                    objRecPay.FMoney = RecPy.FMoney;
                    objRecPay.RecPayDate = RecPy.RecPayDate;
                    objRecPay.Remarks = RecPy.Remarks;
                    objRecPay.FYearID = RecPy.FYearID;
                    //objRecPay.AcJournalID = RecPy.AcJournalID;
                    objRecPay.CustomerID = RecPy.CustomerID;
                    objRecPay.UserID = Convert.ToInt32(UserID);
                    if (RecPy.CashBank != null)
                    {
                        objRecPay.BankName = RecPy.CashBank;
                    }
                    else
                    {
                        objRecPay.BankName = RecPy.ChequeBank;
                    }

                    Context1.Entry(objRecPay).State = EntityState.Modified;
                    Context1.SaveChanges();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return 1;
        }



        public int EditSupplierRecPay(CustomerRcieptVM RecPy, string UserID)
        {
            if (RecPy.RecPayID > 0)
            {
                try
                {
                    //Edit Code
                    RecPay objRecPay = Context1.RecPays.Where(item => item.RecPayID == RecPy.RecPayID).FirstOrDefault();

                    objRecPay.BankName = RecPy.BankName;
                    objRecPay.ChequeDate = RecPy.ChequeDate;
                    objRecPay.ChequeNo = RecPy.ChequeNo;
                    objRecPay.EXRate = RecPy.EXRate;
                    objRecPay.FMoney = RecPy.FMoney;
                    objRecPay.RecPayDate = RecPy.RecPayDate;
                    objRecPay.Remarks = RecPy.Remarks;
                    objRecPay.FYearID = RecPy.FYearID;
                    objRecPay.AcJournalID = RecPy.AcJournalID;
                    objRecPay.SupplierID = RecPy.SupplierID;
                    objRecPay.UserID = Convert.ToInt32(UserID);
                    if (RecPy.CashBank != null)
                    {
                        objRecPay.BankName = RecPy.CashBank;
                    }
                    else
                    {
                        objRecPay.BankName = RecPy.ChequeBank;
                    }

                    Context1.Entry(objRecPay).State = EntityState.Modified;
                    Context1.SaveChanges();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return 1;
        }

       

    }
}