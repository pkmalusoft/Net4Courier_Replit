using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Dynamic;
using System.Data;
//using Microsoft.Reporting.WebForms;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Net4Courier.DAL;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    //   [Authorize]
    public class ReceiptPaymentAllocationController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 Context1 = new Entities1();

        EditCommanFu editfu = new EditCommanFu();

        #region "Customer Receipt Allocation"
        public ActionResult Index(CustomerReceiptSearch obj)
        {

            //CustomerReceiptSearch obj = (CustomerReceiptSearch)Session["CustomerReceiptSearch"];
            CustomerReceiptSearch model = new CustomerReceiptSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofYear().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;

                obj = new CustomerReceiptSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.ReceiptNo = "";
                Session["CustomerReceiptSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.ReceiptNo = "";
                model.EntryType = "Receipt";
                model.Details = new List<ReceiptVM>();
            }
            else
            {
                model = obj;
                var data = ReceiptDAO.GetAllCustomerReceipts(obj.EntryType, obj.FromDate, obj.ToDate, yearid, obj.ReceiptNo, obj.CustomerType, obj.CustomerID, obj.AllocationPending, obj.InvoiceNo);
                model.Details = data;
            }

            return View(model);

        }

       [HttpGet]
       public ActionResult Index1(int id)
        {
            CustomerReceiptSearch model = new CustomerReceiptSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofYear().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;

                model = new CustomerReceiptSearch();
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.ReceiptNo = "";
                model.CustomerID = id;
            var customer = Context1.CustomerMasters.Find(id);
            model.CustomerName = customer.CustomerName;
                Session["CustomerReceiptSearch"] = model;                
                model.EntryType = "Receipt";
            model.CustomerType = customer.CustomerType;
                model.Details = new List<ReceiptVM>();
                var data = ReceiptDAO.GetAllCustomerReceipts(model.EntryType, model.FromDate, model.ToDate, yearid, model.ReceiptNo, "CR", model.CustomerID, model.AllocationPending, model.InvoiceNo);
                model.Details = data;
                return RedirectToAction("Index", model);
            
        }
        
        [HttpPost]
        public ActionResult ShowReceiptAllocation(int id=0,string EntryType="Receipt")
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();
            decimal Advance = 0;
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                if (EntryType == "Receipt")
                {
                    RecPay recpay = Context1.RecPays.Find(id);
                    cust.RecPayID = recpay.RecPayID; // = RP.GetRecPayByRecpayID(id);
                    cust.FMoney = recpay.FMoney;
                    cust.DocumentNo = recpay.DocumentNo;
                    cust.CustomerID = recpay.CustomerID;
                    if (recpay.OtherReceipt != null)
                    {
                        cust.OtherReceipt = Convert.ToDecimal(recpay.OtherReceipt);
                    }
                    cust.EntryType = EntryType;
                   
                    Advance = ReceiptDAO.SP_GetCustomerAdvance(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(id), FyearId,"Receipt");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;

                }
                else if (EntryType=="CN")
                {
                    CreditNote cn = Context1.CreditNotes.Find(id);
                    cust.RecPayID = cn.CreditNoteID;
                    cust.FMoney = Convert.ToDecimal(cn.Amount);
                    cust.DocumentNo = cn.CreditNoteNo;
                    cust.CustomerID = cn.CustomerID;
                    cust.EntryType = EntryType;
                    Advance = ReceiptDAO.SP_GetCustomerAdvance(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(id), FyearId, "CN");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;

                }
                else if (EntryType == "OP")
                {
                    AcOPInvoiceDetail cn = Context1.AcOPInvoiceDetails.Find(id);
                    cust.RecPayID = cn.AcOPInvoiceDetailID;
                    cust.FMoney =Math.Abs(Convert.ToDecimal(cn.Amount));
                    cust.DocumentNo = cn.InvoiceNo;
                    var acopinvoicemaster = Context1.AcOPInvoiceMasters.Find(cn.AcOPInvoiceMasterID);
                    cust.CustomerID = acopinvoicemaster.PartyID;
                    cust.EntryType = EntryType;
                    Advance = ReceiptDAO.SP_GetCustomerAdvance(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(id), FyearId, "OP");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;

                }
            }                 
          
           
           
       
         
            return PartialView("ReceiptAllocation", cust);

        }

        [HttpPost]
        public JsonResult SaveReceiptAllocation(int RecPayID,decimal OtherReceipt, string EntryType,string CustomerType, string Details)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            var CustomerRcieptChildVM = JsonConvert.DeserializeObject<List<CustomerRcieptChildVM>>(Details);
            int i = 0;
            decimal TotalAmount = 0;
            RecPay recpay = new RecPay();
            int UserID = Convert.ToInt32(Session["UserID"]);
        
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
           
            if (CustomerRcieptChildVM == null)
            {
                CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            }
            
            if (RecPayID>0 && EntryType=="Receipt") //edit mode
            {
                decimal Fmoney = 0;
             
          
                recpay = Context1.RecPays.Find(RecPayID);
                recpay.OtherReceipt = OtherReceipt;
               
                recpay.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                recpay.ModifiedBy = UserID;
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                //var consignmentdetails = (from d in Context1.RecPayAllocationDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                //Context1.RecPayAllocationDetails.RemoveRange(consignmentdetails);
                //Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "C";
                        if (CustomerType == "CL")
                            invoicetype = "L";
                        if (item.InvoiceType=="OP" )
                         {
                            if (CustomerType == "CL")
                                invoicetype = "LOP";
                            else
                               invoicetype = "COP";

                            if (item.AcOPInvoiceDetailID >0)
                                {
                           ReceiptDAO.InsertRecpayDetailsForCust(RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), 0, 3, item.AdjustmentAmount);
                            }
                            else
                            {
                                ReceiptDAO.InsertRecpayDetailsForCust(RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), 0, 3, item.AdjustmentAmount);
                            }
                            
                        }
                    else if(item.InvoiceType=="CJ")
                        {
                            invoicetype = "CJ";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(),0, 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), 0, 3, item.AdjustmentAmount);
                        }
 
                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }

                if (recpay.StatusEntry !="DR" && recpay.FMoney>0)
                {
                    ReceiptDAO.InsertJournalOfCustomer(recpay.RecPayID, fyearid);
                }else if (recpay.StatusEntry == "DR")
                {
                    ReceiptDAO.InsertJournalOfCustomerDRR(recpay.RecPayID, fyearid);
                }

            }
            else if (RecPayID >0 && EntryType == "CN")
            {
                var cn = Context1.CreditNotes.Find(RecPayID);

                
                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.ForCreditNoteID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                    RecPayDetail detail = new RecPayDetail();
                    detail.RecPayDetailID = maxrecpaydetailid + 1;
                    detail.ForCreditNoteID = RecPayID;
                    if (item.InvoiceType == "OP")
                    {
                        detail.AcOPInvoiceDetailID = item.InvoiceID;
                    }
                    else if (item.InvoiceType=="CN")
                    {
                        detail.CreditNoteId = item.InvoiceID;

                    }
                    else
                    {
                        detail.InvoiceID = item.InvoiceID;
                    }
                    detail.InvNo = item.InvoiceNo;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    detail.InvDate = vInvoiceDate1;
                    detail.Amount = item.Amount;
                    if (CustomerType=="CR")
                    detail.StatusInvoice = "C";
                    else
                        detail.StatusInvoice = "L";
                    Context1.RecPayDetails.Add(detail);
                    Context1.SaveChanges();

                }


                }
            else if (RecPayID > 0 && EntryType == "OP")
            {
                var cn = Context1.AcOPInvoiceDetails.Find(RecPayID);


                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.ForAcOPInvoiceDetailID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                    RecPayDetail detail = new RecPayDetail();
                    detail.RecPayDetailID = maxrecpaydetailid + 1;
                    detail.ForAcOPInvoiceDetailID = RecPayID;
                    if (item.InvoiceType == "OP")
                    {
                        detail.AcOPInvoiceDetailID = item.InvoiceID;
                    }
                    else if (item.InvoiceType == "CN")
                    {
                        detail.CreditNoteId = item.InvoiceID;

                    }else if(item.InvoiceType=="DN")
                    {
                        detail.DebitNoteId = item.InvoiceID;
                    }
                    else
                    {
                        detail.InvoiceID = item.InvoiceID;
                    }
                    detail.InvNo = item.InvoiceNo;
                    if (CustomerType == "CR")
                        detail.StatusInvoice = "C";
                    else
                        detail.StatusInvoice = "L";
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    detail.InvDate = vInvoiceDate1;
                    detail.Amount = item.Amount;
                    Context1.RecPayDetails.Add(detail);
                    Context1.SaveChanges();

                }


            }
            else if (RecPayID > 0 && EntryType == "Payment") //edit mode
            {
                decimal Fmoney = 0;


                recpay = Context1.RecPays.Find(RecPayID);

              

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

             

                foreach (var item in CustomerRcieptChildVM)
                {
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        if (maxrecpaydetailid == null)
                            maxrecpaydetailid = 0;
                        RecPayDetail detail = new RecPayDetail();
                        detail.RecPayDetailID = maxrecpaydetailid+1;
                        detail.RecPayID = RecPayID;
                        detail.InvNo = item.InvoiceNo;
                        detail.InvDate = vInvoiceDate1;
                        if (item.InvoiceType == "OP" && item.AcOPInvoiceDetailID > 0)
                            detail.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
                        else if (item.InvoiceType == "OP" && item.InvoiceID > 0)
                            detail.AcOPInvoiceDetailID = item.InvoiceID;
                        else
                            detail.InvoiceID = item.InvoiceID;

                        detail.StatusInvoice = "S";
                        detail.Amount = item.Amount;
                        detail.AdjustmentAmount = item.AdjustmentAmount;

                        Context1.RecPayDetails.Add(detail);
                        Context1.SaveChanges();

                    }
                }
                     
            }
            else if (RecPayID > 0 && EntryType == "DN")
            {
                var cn = Context1.DebitNotes.Find(RecPayID);


                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.ForCreditNoteID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                    RecPayDetail detail = new RecPayDetail();
                    detail.RecPayDetailID = maxrecpaydetailid + 1;
                    detail.ForDebitNoteId = RecPayID;
                    detail.StatusInvoice = "S";
                    if (item.InvoiceType == "OP")
                    {
                        detail.AcOPInvoiceDetailID = item.InvoiceID;
                    }
                    else if (item.InvoiceType == "CN")
                    {
                        detail.CreditNoteId = item.InvoiceID;

                    }
                    else
                    {
                        detail.InvoiceID = item.InvoiceID;
                    }
                    detail.InvNo = item.InvoiceNo;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    detail.InvDate = vInvoiceDate1;
                    detail.Amount = item.Amount;
                    Context1.RecPayDetails.Add(detail);
                    Context1.SaveChanges();

                }


            }





            return Json(new { Status="OK"  }, JsonRequestBehavior.AllowGet);
        }

         
        [HttpPost]
        public JsonResult GetTradeInvoiceOfCustomer(int? ID, decimal? amountreceived, int? RecPayId, string RecPayType = "CR",string EntryType="Receipt")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0, Convert.ToInt32(RecPayId), 0, "B", RecPayType);
            int rowindex = 0;
            for (rowindex = 0; rowindex < Allinvoices.Count; rowindex++) // each (var Invoice in Allinvoices)
            {
                if (amountreceived != null)
                {
                    if (amountreceived == 0)
                    {
                        return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Allinvoices[rowindex].Balance > 0)
                {
                    if (amountreceived != null)
                    {
                        if (amountreceived >= Allinvoices[rowindex].Balance)
                        {
                            Allinvoices[rowindex].Allocated = true;
                            Allinvoices[rowindex].Amount = Allinvoices[rowindex].Balance;
                            amountreceived = amountreceived - Allinvoices[rowindex].Amount;
                        }
                        else if (amountreceived > 0)
                        {
                            Allinvoices[rowindex].Allocated = true;
                            Allinvoices[rowindex].Amount = amountreceived;
                            amountreceived = amountreceived - Allinvoices[rowindex].Amount;
                        }
                        else
                        {
                            Allinvoices[rowindex].Amount = 0;
                        }
                    }
                    // salesinvoice.Add(Invoice);
                    if (Allinvoices[rowindex].InvoiceType == "TR")
                    {
                        if (RecPayId == null)
                        {
                            AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Allinvoices[rowindex].SalesInvoiceID), Convert.ToDecimal(Allinvoices[rowindex].Amount), 0); //customer invoiceid,amount
                        }
                        else
                        {
                            AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Allinvoices[rowindex].SalesInvoiceID), Convert.ToDecimal(Allinvoices[rowindex].Amount), Convert.ToInt32(RecPayId)); //customer invoiceid,amount
                        }
                    }
                }
            }



            Session["AWBAllocation"] = AWBAllocation;
            return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetReceiptAllocated(int? ID, decimal? amountreceived, int? RecPayId, string RecPayType = "CR",string EntryType="Receipt")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.GetCustomerReceiptAllocated(Convert.ToInt32(ID), 0, Convert.ToInt32(RecPayId), 0, "B", RecPayType,EntryType);

         
            return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
        }
        //GetInvoiceReceipts
        [HttpPost]
        public JsonResult GetInvoiceReceipts(int CustomerID, string InvoiceNo)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<ReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();

            var recpaydetails = (from c in Context1.RecPayDetails where c.InvNo == InvoiceNo select c).ToList();
            foreach(var item in recpaydetails)
            {
                ReceiptVM obj = new ReceiptVM();
                obj.Amount = Math.Abs(Convert.ToDecimal(item.Amount));
                if (item.AdjustmentAmount==null)
                {
                    obj.AdjustmentAmount = 0;
                }
                else
                {
                    obj.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                }
                
                obj.RecPayDetailID = item.RecPayDetailID;
                
                if (item.RecPayID!=null && item.RecPayID>0)
                {
                    var recpay = Context1.RecPays.Find(item.RecPayID);
                    obj.ReceiptAmount = recpay.FMoney;
                    obj.RecPayDate = recpay.RecPayDate;
                    obj.RecPayDate1 = recpay.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.DocumentNo;
                }

                else if (item.ForCreditNoteID != null && item.ForCreditNoteID > 0)
                {
                    var recpay = Context1.CreditNotes.Find(item.ForCreditNoteID);
                    obj.ReceiptAmount = Convert.ToDecimal(recpay.Amount);
                    obj.RecPayDate = Convert.ToDateTime(recpay.CreditNoteDate);
                    obj.RecPayDate1 = obj.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.CreditNoteNo;
                }
                else if (item.ForAcOPInvoiceDetailID != null && item.ForAcOPInvoiceDetailID > 0)
                {
                    var recpay = Context1.AcOPInvoiceDetails.Find(item.ForAcOPInvoiceDetailID);
                    obj.ReceiptAmount = Convert.ToDecimal(recpay.Amount);
                    obj.RecPayDate = Convert.ToDateTime(recpay.InvoiceDate);
                    obj.RecPayDate1 = obj.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.InvoiceNo;
                }
                salesinvoice.Add(obj);
            }
            return Json(new { Status="OK", salesinvoice = salesinvoice }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAllocation(int id)
        {
            if (id > 0)
            {
                try
                {
                    var recpaydetail = Context1.RecPayDetails.Find(id);
                    if (recpaydetail != null)
                    {
                        Context1.RecPayDetails.Remove(recpaydetail);
                        Context1.SaveChanges();
                        return Json(new { Status = "OK", Message = "Allocation Deleted" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Status = "Failed", Message = "Allocation Delete Failed" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    return Json(new { Status = "Failed", Message = "Allocation Delete Failed" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Status = "Failed", Message = "Allocation Delete Failed" }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult GetTradeInvoiceOfCustomerDRS(int? ID, decimal? amountreceived, int? RecPayId, string RecPayType = "CR")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.SP_GetCustomerInvoicePendingDRS(Convert.ToInt32(ID), 0, Convert.ToInt32(RecPayId), 0, "OP", RecPayType);

            foreach (var Invoice in Allinvoices)
            {
                if (Invoice.Balance > 0)
                {
                    if (amountreceived != null)
                    {
                        if (amountreceived >= Invoice.Balance)
                        {
                            Invoice.Allocated = true;
                            Invoice.Amount = Invoice.Balance;
                            amountreceived = amountreceived - Invoice.Amount;
                        }
                        else if (amountreceived > 0)
                        {
                            Invoice.Allocated = true;
                            Invoice.Amount = amountreceived;
                            amountreceived = amountreceived - Invoice.Amount;
                        }
                        else
                        {
                            Invoice.Amount = 0;
                        }
                    }
                    salesinvoice.Add(Invoice);
                    if (Invoice.InvoiceType == "TR")
                    {
                        if (RecPayId == null)
                        {
                            AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Invoice.SalesInvoiceID), Convert.ToDecimal(Invoice.Amount), 0); //customer invoiceid,amount
                        }
                        else
                        {
                            AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Invoice.SalesInvoiceID), Convert.ToDecimal(Invoice.Amount), Convert.ToInt32(RecPayId)); //customer invoiceid,amount
                        }
                    }
                }
            }



            Session["AWBAllocation"] = AWBAllocation;
            return Json(new { advance = 0, salesinvoice = salesinvoice }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetAWBAllocation(int InvoiceId)
        {
            List<ReceiptAllocationDetailVM> AWBAllocationall = new List<ReceiptAllocationDetailVM>();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            AWBAllocationall = (List<ReceiptAllocationDetailVM>)Session["AWBAllocation"];
            AWBAllocation = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == InvoiceId).ToList();
            return Json(AWBAllocation, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult SaveAWBAllocation(List<ReceiptAllocationDetailVM> RecP)
        {
            var dd = RecP;
            List<ReceiptAllocationDetailVM> AWBAllocationall = new List<ReceiptAllocationDetailVM>();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            AWBAllocationall = (List<ReceiptAllocationDetailVM>)Session["AWBAllocation"];
            foreach (var item in AWBAllocationall)
            {
                foreach (var item2 in RecP)
                {
                    if (item.CustomerInvoiceDetailID == item2.CustomerInvoiceDetailID)
                    {
                        item.AllocatedAmount = item2.AllocatedAmount;
                        break;
                    }

                }
                AWBAllocation.Add(item);
            }
            Session["AWBAllocation"] = AWBAllocation;
            //AWBAllocation = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == InvoiceId).ToList();
            //   AWBAllocation = updatelist;
            return Json(AWBAllocation, JsonRequestBehavior.AllowGet);

        }
         
        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in Context1.CustomerMasters
                                    where (c1.CustomerType == "CR" || c1.CustomerType == "CL") && c1.CustomerName.ToLower().Contains(term.ToLower())
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in Context1.CustomerMasters
                                    where (c1.CustomerType == "CR" || c1.CustomerType == "CL") 
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetCustomerNameByType(string term,string CustomerType="CR")
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in Context1.CustomerMasters
                                    where (c1.CustomerType == CustomerType ) && c1.CustomerName.ToLower().Contains(term.ToLower())
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in Context1.CustomerMasters
                                    where (c1.CustomerType == CustomerType)
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public JsonResult GetCreditCustomerName(string term)
        {
            var customerlist = (from c1 in Context1.CustomerMasters
                                where c1.CustomerType == "CR" && c1.CustomerName.ToLower().Contains(term.ToLower())
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).ToList();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region "Supplier payment allocation"
        public ActionResult PaymentIndex(CustomerReceiptSearch obj)
        {
            var supplierMasterTypes = (from d in Context1.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            //CustomerReceiptSearch obj = (CustomerReceiptSearch)Session["CustomerReceiptSearch"];
            CustomerReceiptSearch model = new CustomerReceiptSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofYear().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;

                obj = new CustomerReceiptSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.ReceiptNo = "";
                Session["SupplierPaymentSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.ReceiptNo = "";
                model.EntryType = "Payment";
                model.Details = new List<ReceiptVM>();
            }
            else
            {
                model = obj;
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
                var data = ReceiptDAO.GetAllSupplierPayments(obj.EntryType, model.FromDate, model.ToDate, yearid, obj.ReceiptNo,obj.SupplierTypeId, obj.SupplierID, obj.AllocationPending, obj.InvoiceNo);
                model.Details = data;
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult ShowPaymentAllocation(int id = 0, string EntryType = "Payment")
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();
            decimal Advance = 0;
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

            if (id > 0)
            {
                if (EntryType == "Payment")
                {
                    RecPay recpay = Context1.RecPays.Find(id);
                    cust.RecPayID = recpay.RecPayID; // = RP.GetRecPayByRecpayID(id);
                    cust.FMoney = recpay.FMoney;
                    cust.DocumentNo = recpay.DocumentNo;
                    cust.SupplierID = recpay.SupplierID;
                    cust.EntryType = EntryType;
                    Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), FyearId, "Payment");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;
                    cust.EntryType = EntryType;

                }
                else if (EntryType == "DN")
                {
                    DebitNote cn = Context1.DebitNotes.Find(id);
                    cust.RecPayID = cn.DebitNoteID;
                    cust.FMoney = Convert.ToDecimal(cn.Amount);
                    cust.DocumentNo = cn.DebitNoteNo;
                    cust.SupplierID = cn.SupplierID;
                    cust.EntryType = EntryType;
                    Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), FyearId, "DN");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;
                    cust.EntryType = EntryType;

                }
                else if (EntryType == "OP")
                {
                    AcOPInvoiceDetail cn = Context1.AcOPInvoiceDetails.Find(id);
                    cust.RecPayID = cn.AcOPInvoiceDetailID;
                    cust.FMoney = Math.Abs(Convert.ToDecimal(cn.Amount));
                    cust.DocumentNo = cn.InvoiceNo;
                    var acopinvoicemaster = Context1.AcOPInvoiceMasters.Find(cn.AcOPInvoiceMasterID);
                    cust.CustomerID = acopinvoicemaster.PartyID;
                    cust.EntryType = EntryType;
                    Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), FyearId, "OP");
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;
                    cust.EntryType = EntryType;

                }
            }





            return PartialView("PaymentAllocation", cust);

        }


        [HttpPost]
        public JsonResult GetTradeInvoiceOfSupplier(int? ID, decimal? amountreceived, int? RecPayId, int SupplierTypeId= 1, string EntryType = "Payment")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            
            Allinvoices = ReceiptDAO.SP_GetSupplierInvoicePending(Convert.ToInt32(ID), Convert.ToInt32(RecPayId), SupplierTypeId,EntryType);
            
            int rowindex = 0;
            for (rowindex = 0; rowindex < Allinvoices.Count; rowindex++) // each (var Invoice in Allinvoices)
            {
                if (amountreceived != null)
                {
                    if (amountreceived == 0)
                    {
                        return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Allinvoices[rowindex].Balance > 0)
                {
                    if (amountreceived != null)
                    {
                        if (amountreceived >= Allinvoices[rowindex].Balance)
                        {
                            Allinvoices[rowindex].Allocated = true;
                            Allinvoices[rowindex].Amount = Allinvoices[rowindex].Balance;
                            amountreceived = amountreceived - Allinvoices[rowindex].Amount;
                        }
                        else if (amountreceived > 0)
                        {
                            Allinvoices[rowindex].Allocated = true;
                            Allinvoices[rowindex].Amount = amountreceived;
                            amountreceived = amountreceived - Allinvoices[rowindex].Amount;
                        }
                        else
                        {
                            Allinvoices[rowindex].Amount = 0;
                        }
                    }
                    //// salesinvoice.Add(Invoice);
                    //if (Allinvoices[rowindex].InvoiceType == "TR")
                    //{
                    //    if (RecPayId == null)
                    //    {
                    //        AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Allinvoices[rowindex].SalesInvoiceID), Convert.ToDecimal(Allinvoices[rowindex].Amount), 0); //customer invoiceid,amount
                    //    }
                    //    else
                    //    {
                    //        AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Allinvoices[rowindex].SalesInvoiceID), Convert.ToDecimal(Allinvoices[rowindex].Amount), Convert.ToInt32(RecPayId)); //customer invoiceid,amount
                    //    }
                    //}
                }
            }



            Session["AWBAllocation"] = AWBAllocation;
            return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetSupplierAllocated(int? ID, decimal? amountreceived, int? RecPayId, int SupplierTypeId=1, string EntryType = "Payment")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.GetSupplierPaymentAllocated(Convert.ToInt32(ID),  Convert.ToInt32(RecPayId),SupplierTypeId, EntryType);


            return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePaymentAllocation(int RecPayID, string EntryType, string Details)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            var CustomerRcieptChildVM = JsonConvert.DeserializeObject<List<CustomerRcieptChildVM>>(Details);
            int i = 0;
            decimal TotalAmount = 0;
            RecPay recpay = new RecPay();
            int UserID = Convert.ToInt32(Session["UserID"]);

            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

            if (CustomerRcieptChildVM == null)
            {
                CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            }

         
             if (RecPayID > 0 && EntryType == "OP")
            {
                var cn = Context1.AcOPInvoiceDetails.Find(RecPayID);


                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.ForAcOPInvoiceDetailID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                    RecPayDetail detail = new RecPayDetail();
                    detail.RecPayDetailID = maxrecpaydetailid + 1;
                    detail.ForAcOPInvoiceDetailID = RecPayID;
                    if (item.InvoiceType == "OP")
                    {
                        detail.AcOPInvoiceDetailID = item.InvoiceID;
                    }
                    else if (item.InvoiceType == "SJ")
                    {
                        detail.DebitNoteId = item.InvoiceID;

                    }                    
                    else
                    {
                        detail.InvoiceID = item.InvoiceID;
                    }
                    detail.InvNo = item.InvoiceNo;
                    detail.StatusInvoice = "S";
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    detail.InvDate = vInvoiceDate1;
                    detail.Amount = item.Amount;
                    detail.AdjustmentAmount = item.AdjustmentAmount;
                    Context1.RecPayDetails.Add(detail);
                    Context1.SaveChanges();

                }


            }
            else if (RecPayID > 0 && EntryType == "Payment") //edit mode
            {
                decimal Fmoney = 0;


                recpay = Context1.RecPays.Find(RecPayID);



                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();



                foreach (var item in CustomerRcieptChildVM)
                {
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        if (maxrecpaydetailid == null)
                            maxrecpaydetailid = 0;
                        RecPayDetail detail = new RecPayDetail();
                        detail.RecPayDetailID = maxrecpaydetailid + 1;
                        detail.RecPayID = RecPayID;
                        detail.InvNo = item.InvoiceNo;
                        detail.InvDate = vInvoiceDate1;
                        if (item.InvoiceType == "OP" && item.AcOPInvoiceDetailID > 0)
                            detail.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
                        else if (item.InvoiceType == "OP" && item.InvoiceID > 0)
                            detail.AcOPInvoiceDetailID = item.InvoiceID;
                        else if (item.InvoiceType == "SJ" && item.InvoiceID > 0)
                            detail.DebitNoteId = item.InvoiceID;
                        else
                            detail.InvoiceID = item.InvoiceID;

                        detail.StatusInvoice = "S";
                        detail.CurrencyID = Convert.ToInt32(Session["CurrencyId"].ToString());
                        
                        detail.Amount = item.Amount;
                        detail.AdjustmentAmount = item.AdjustmentAmount;

                        Context1.RecPayDetails.Add(detail);
                        Context1.SaveChanges();

                    }
                }

            }
            else if (RecPayID > 0 && EntryType == "DN")
            {
                var cn = Context1.DebitNotes.Find(RecPayID);


                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.ForDebitNoteId == RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                foreach (var item in CustomerRcieptChildVM)
                {
                    var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                    RecPayDetail detail = new RecPayDetail();
                    detail.RecPayDetailID = maxrecpaydetailid + 1;
                    detail.ForDebitNoteId = RecPayID;
                    detail.StatusInvoice = "S";
                    if (item.InvoiceType == "OP")
                    {
                        detail.AcOPInvoiceDetailID = item.InvoiceID;
                    }
                    else if (item.InvoiceType == "DN")
                    {
                        detail.DebitNoteId = item.InvoiceID;

                    }
                    else
                    {
                        detail.InvoiceID = item.InvoiceID;
                    }
                    detail.InvNo = item.InvoiceNo;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    detail.InvDate = vInvoiceDate1;
                    detail.Amount = item.Amount;
                    detail.AdjustmentAmount = item.AdjustmentAmount;
                    Context1.RecPayDetails.Add(detail);
                    Context1.SaveChanges();

                }


            }





            return Json(new { Status = "OK" }, JsonRequestBehavior.AllowGet);
        }

        //GetInvoiceReceipts
        [HttpPost]
        public JsonResult GetSupplierInvoiceReceipts(int SupplierID, string InvoiceNo)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<ReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();

            var recpaydetails = (from c in Context1.RecPayDetails where c.InvNo == InvoiceNo select c).ToList();
            foreach (var item in recpaydetails)
            {
                ReceiptVM obj = new ReceiptVM();
                obj.Amount = Math.Abs(Convert.ToDecimal(item.Amount));
                if (item.AdjustmentAmount == null)
                {
                    obj.AdjustmentAmount = 0;
                }
                else
                {
                    obj.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                }

                obj.RecPayDetailID = item.RecPayDetailID;

                if (item.RecPayID != null && item.RecPayID > 0)
                {
                    var recpay = Context1.RecPays.Find(item.RecPayID);
                    obj.ReceiptAmount = recpay.FMoney;
                    obj.RecPayDate = recpay.RecPayDate;
                    obj.RecPayDate1 = recpay.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.DocumentNo;
                }

                else if (item.ForDebitNoteId != null && item.ForDebitNoteId > 0)
                {
                    var recpay = Context1.DebitNotes.Find(item.ForDebitNoteId);
                    obj.ReceiptAmount = Convert.ToDecimal(recpay.Amount);
                    obj.RecPayDate = Convert.ToDateTime(recpay.DebitNoteDate);
                    obj.RecPayDate1 = obj.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.DebitNoteNo;
                }
                else if (item.ForAcOPInvoiceDetailID != null && item.ForAcOPInvoiceDetailID > 0)
                {
                    var recpay = Context1.AcOPInvoiceDetails.Find(item.ForAcOPInvoiceDetailID);
                    obj.ReceiptAmount = Convert.ToDecimal(recpay.Amount);
                    obj.RecPayDate = Convert.ToDateTime(recpay.InvoiceDate);
                    obj.RecPayDate1 = obj.RecPayDate.ToString("dd-MM-yyyy");
                    obj.DocumentNo = recpay.InvoiceNo;
                }
                salesinvoice.Add(obj);
            }
            return Json(new { Status = "OK", salesinvoice = salesinvoice }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
