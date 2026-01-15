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
    public class CustomerReceiptController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 Context1 = new Entities1();

        EditCommanFu editfu = new EditCommanFu();


        public ActionResult Index()
        {
            
               CustomerReceiptSearch obj = (CustomerReceiptSearch)Session["CustomerReceiptSearch"];
            CustomerReceiptSearch model = new CustomerReceiptSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                obj = new CustomerReceiptSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.ReceiptNo = "";                
                Session["CustomerReceiptSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.ReceiptNo = "";
                
                model.Details = new List<ReceiptVM>();
            }
            else
            {
                model = obj;
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
                Session["CustomerReceiptSearch"] = model;
                   
            }
            var data = ReceiptDAO.GetCustomerReceiptsByDate(model.FromDate, model.ToDate, yearid, model.ReceiptNo, "CR");
            model.Details = data;
            return View(model);

        }

        [HttpPost]
        public ActionResult Index(CustomerReceiptSearch obj)
        {
            Session["CustomerReceiptSearch"] = obj;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create(int id=0)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();

            

            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    cust = RP.GetRecPayByRecpayID(id);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Cash") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Bank") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    cust.recPayDetail = Context1.RecPayDetails.Where(item => item.RecPayID == id).ToList();

                    decimal Advance = 0;
                    //Advance = ReceiptDAO.SP_GetCustomerAdvance(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(id), FyearId);
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;
                    foreach (var item in cust.recPayDetail)
                    {
                        if (item.AcOPInvoiceDetailID > 0)
                        {
                            var sInvoiceDetail = (from d in Context1.AcOPInvoiceDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                if (sInvoiceDetail.Count > 0)
                                {
                                    var invoicetotal = sInvoiceDetail.Sum(d => d.Amount); //  sInvoiceDetail.Sum(d=>d.OtherCharge);                                                                                              
                                    var totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(item.AcOPInvoiceDetailID), Convert.ToInt32(id), 0, "OP");
                                    var totamt = totamtpaid;// + totadjust;// + CreditAmount;
                                    var customerinvoice = new CustomerRcieptChildVM();
                                    customerinvoice.InvoiceID = 0;
                                    customerinvoice.AcOPInvoiceDetailID = sInvoiceDetail[0].AcOPInvoiceDetailID;
                                    customerinvoice.InvoiceType = "OP";
                                    customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.AcOPInvoiceDetailID;
                                    customerinvoice.SInvoiceNo = sInvoiceDetail[0].InvoiceNo;
                                    customerinvoice.InvoiceNo = sInvoiceDetail[0].InvoiceNo;
                                    customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                    customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                    customerinvoice.AmountToBeRecieved = Convert.ToDecimal(invoicetotal);// - Convert.ToDecimal(totamt) - Convert.ToDecimal(item.Amount);
                                    customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid); // Convert.ToDecimal(totamtpaid) - Convert.ToDecimal(totadjust);// ;// customerinvoice.AmountToBeRecieved;
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                    customerinvoice.Balance = customerinvoice.AmountToBeRecieved - totamtpaid;// customerinvoice.AmountToBePaid; // (Convert.ToDecimal(invoicetotal) - Convert.ToDecimal(totamt)) - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                    customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                    customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                    customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                    cust.CustomerRcieptChildVM.Add(customerinvoice);
                                }
                            }
                        }
                        else if (item.CreditNoteId > 0)
                        {
                            var sInvoiceDetail = (from d in Context1.CreditNotes where d.CreditNoteID == item.CreditNoteId select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                if (sInvoiceDetail.Count > 0)
                                {
                                    var invoicetotal = sInvoiceDetail.Sum(d => d.Amount); //  sInvoiceDetail.Sum(d=>d.OtherCharge);                                                                                                
                                    var totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(item.CreditNoteId), Convert.ToInt32(id), 0, "CP");
                                    var totamt = totamtpaid;// + totadjust;// + CreditAmount;
                                    var customerinvoice = new CustomerRcieptChildVM();
                                    customerinvoice.InvoiceID = Convert.ToInt32(item.CreditNoteId);
                                    customerinvoice.AcOPInvoiceDetailID = 0;
                                    customerinvoice.InvoiceType = "CJ";
                                    customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                    customerinvoice.InvoiceNo = sInvoiceDetail[0].CreditNoteNo;
                                    customerinvoice.SInvoiceNo = sInvoiceDetail[0].CreditNoteNo;
                                    customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                    customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                    customerinvoice.AmountToBeRecieved = Convert.ToDecimal(invoicetotal);// - Convert.ToDecimal(totamt) - Convert.ToDecimal(item.Amount);
                                    customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid); // Convert.ToDecimal(totamtpaid) - Convert.ToDecimal(totadjust);// ;// customerinvoice.AmountToBeRecieved;
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                    customerinvoice.Balance = customerinvoice.AmountToBeRecieved - totamtpaid;// customerinvoice.AmountToBePaid; // (Convert.ToDecimal(invoicetotal) - Convert.ToDecimal(totamt)) - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                    customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                    customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                    customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                    cust.CustomerRcieptChildVM.Add(customerinvoice);
                                }
                            }
                        }
                        else if (item.InvoiceID > 0 && (item.AcOPInvoiceDetailID == 0 || item.AcOPInvoiceDetailID == null))
                        {
                            cust.AWBAllocation = ReceiptDAO.GetAWBAllocation(cust.AWBAllocation, Convert.ToInt32(item.InvoiceID), Convert.ToDecimal(item.Amount), cust.RecPayID); //customer invoiceid,amount
                            var sInvoiceDetail = (from d in Context1.CustomerInvoiceDetails where d.CustomerInvoiceID == item.InvoiceID select d).ToList();
                            var awbDetail = (from d in Context1.RecPayAllocationDetails where d.CustomerInvoiceID == item.InvoiceID && d.RecPayDetailID == item.RecPayDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                if (sInvoiceDetail.Count > 0)
                                {
                                    var invoicetotal = sInvoiceDetail.Sum(d => d.NetValue); //  sInvoiceDetail.Sum(d=>d.OtherCharge);
                                    var awbtotal = awbDetail.Sum(d => d.AllocatedAmount);
                                    var Sinvoice = (from d in Context1.CustomerInvoices where d.CustomerInvoiceID == item.InvoiceID select d).FirstOrDefault();

                                    var totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(item.InvoiceID), Convert.ToInt32(id), 0, "TR");

                                    var customerinvoice = new CustomerRcieptChildVM();
                                    customerinvoice.InvoiceID = Convert.ToInt32(item.InvoiceID);
                                    customerinvoice.AcOPInvoiceDetailID = 0;
                                    customerinvoice.InvoiceType = "TR";
                                    customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                    customerinvoice.SInvoiceNo = item.InvNo;
                                    customerinvoice.InvoiceNo = item.InvNo;
                                    customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                    customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                    customerinvoice.AmountToBeRecieved = Convert.ToDecimal(invoicetotal);// - Convert.ToDecimal(totamt) - Convert.ToDecimal(item.Amount);
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                    customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid); // - customerinvoice.AmountToBeRecieved;
                                    customerinvoice.Balance = (Convert.ToDecimal(invoicetotal) - totamtpaid);// customerinvoice.AmountToBePaid); // ; // - Convert.ToDecimal(totamt)) - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                    customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                    customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                    customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                    cust.CustomerRcieptChildVM.Add(customerinvoice);
                                }
                            }
                        }
                    }
                    Session["AWBAllocation"] = cust.AWBAllocation;
                    StatusModel result = AccountsDAO.CheckDateValidate(cust.RecPayDate.ToString(), FyearId);
                    if (result.Status == "YearClose") //Period locked
                    {

                        ViewBag.Message = result.Message;
                        ViewBag.SaveEnable = false;
                    }
                    BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "Create";
                    BindAllMasters(2);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Cash") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Bank") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.RecPayDate = pFromDate;
                    cust.StatusOrigin = "CR";
                    cust.RecPayID = 0;
                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                    Session["AWBAllocation"] = cust.AWBAllocation;
                    StatusModel result = AccountsDAO.CheckDateValidate(cust.RecPayDate.ToString(), FyearId);
                    if (result.Status == "YearClose") //Period locked
                    {

                        ViewBag.Message = result.Message;
                        ViewBag.SaveEnable = false;
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
            var StaffNotes = (from d in Context1.StaffNotes where d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var users = (from d in Context1.UserRegistrations select d).ToList();

            var staffnotemodel = new List<StaffNoteModel>();
            foreach (var item in StaffNotes)
            {
                var model = new StaffNoteModel();
                model.id = item.NotesId;
                model.employeeid = item.EmployeeId;
                //model.jobid = item.JobId;
                model.TaskDetails = item.Notes;
                model.Datetime = item.EntryDate;
                model.EmpName = users.Where(d => d.UserID == item.EmployeeId).FirstOrDefault().UserName;
                staffnotemodel.Add(model);
            }
            ViewBag.StaffNoteModel = staffnotemodel;
            var customerdetails = (from d in Context1.CustomerMasters where d.CustomerID == cust.CustomerID && d.CustomerType == "CS" select d).FirstOrDefault();
            if (customerdetails == null)
            {
                customerdetails = new CustomerMaster();
            }
            ViewBag.CustomerDetail = customerdetails;
            var CustomerNotification = (from d in Context1.CustomerNotifications where d.RecPayID == id && d.PageTypeId == 2 orderby d.NotificationId descending select d).ToList();

            var customernotification = new List<CustomerNotificationModel>();
            foreach (var item in CustomerNotification)
            {
                var model = new CustomerNotificationModel();
                model.id = item.NotificationId;
                model.employeeid = item.UserId;
                model.jobid = item.RecPayID;
                model.Message = item.MessageText;
                model.Datetime = item.EntryDate;
                model.IsEmail = item.NotifyByEmail;
                model.IsSms = item.NotifyBySMS;
                model.IsWhatsapp = item.NotifyByWhatsApp;
                model.EmpName = users.Where(d => d.UserID == item.UserId).FirstOrDefault().UserName;
                customernotification.Add(model);
            }
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();
            ViewBag.CustomerNotification = customernotification;
            return View(cust);

        }

        [HttpPost]
        public ActionResult     Create(CustomerRcieptVM RecP, string Command, string Currency)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            int i = 0;
            decimal TotalAmount = 0;
            StatusModel result = AccountsDAO.CheckDateValidate(RecP.RecPayDate.ToString(),fyearid);
            if (result.Status == "PeriodLock") //Period locked
            {
                TempData["ErrorMsg"] = "Period Locked. Transaction Restricted !";
                return RedirectToAction("Index", "CustomerReceipt");
            }
            else if (result.Status == "Failed")
            {
                TempData["ErrorMsg"] = result.Message;
                return RedirectToAction("Index", "CustomerReceipt");
            }
            int UserID = Convert.ToInt32(Session["UserID"]);
            var StaffNotes = (from d in Context1.StaffNotes where d.RecPayID == RecP.RecPayID && d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var users = (from d in Context1.UserRegistrations select d).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocationall = (List<ReceiptAllocationDetailVM>)Session["AWBAllocation"];
            var staffnotemodel = new List<StaffNoteModel>();
            foreach (var item in StaffNotes)
            {
                var model = new StaffNoteModel();
                model.id = item.NotesId;
                model.employeeid = item.EmployeeId;
                //model.jobid = item.JobId;
                model.TaskDetails = item.Notes;
                model.Datetime = item.EntryDate;
                model.EmpName = users.Where(d => d.UserID == item.EmployeeId).FirstOrDefault().UserName;
                staffnotemodel.Add(model);
            }
            ViewBag.StaffNoteModel = staffnotemodel;
            //if (RecP.RecPayID > 0)
            //{
            //    RP.EditCustomerRecPay(RecP, Session["UserID"].ToString());
            //    RP.EditCustomerRecieptDetails(RecP.recPayDetail, RecP.RecPayID);
            //}
            if (RecP.CashBank != null)
            {
                RecP.StatusEntry = "CS";
                int acheadid = Convert.ToInt32(RecP.CashBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
            }
            else if (RecP.ChequeBank != null)
            {
                RecP.StatusEntry = "BK";
                int acheadid = Convert.ToInt32(RecP.ChequeBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
            }
            else if(RecP.StatusOrigin=="DRR")
            {
                RecP.StatusEntry = "DR";
                RecP.AcOPInvoiceDetailID = 0;
                RecP.BankName = "";
            }
            else
            {
                RecP.StatusEntry = "ST";
                RecP.AcOPInvoiceDetailID = RecP.AcOPInvoiceDetailID;
                RecP.BankName = RecP.OPRefNo;
            }
            if (RecP.CustomerRcieptChildVM == null)
            {
                RecP.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            }
            //Adding Entry in Rec PAY

            ///Insert Entry For RecPay Details 
            ///
            if (RecP.RecPayID <= 0)
            {
                decimal Fmoney = 0;
                for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                {
                    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                }
                //if (Fmoney > 0)
                //{
                //    RecP.FMoney = Fmoney;
                //}
                RecP.DocumentNo = ReceiptDAO.GetMaxCustomerReceiptNo();                
                RecP.AcCompanyID = branchid;
                RecP.FYearID = fyearid;
                RecP.UserID = UserID;
                RecP.StatusOrigin = "CR";
                RPID = ReceiptDAO.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString()); //.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString());

                RecP.RecPayID = RPID; // (from c in Context1.RecPays orderby c.RecPayID descending select c.RecPayID).FirstOrDefault();


                foreach (var item in RecP.CustomerRcieptChildVM)
                {
                    decimal Advance = 0;
                    Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "C";
                        if (item.InvoiceType == "OP") 
                        {
                            invoicetype = "COP";
                            if (item.AcOPInvoiceDetailID>0)
                                ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                            else
                                ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else if (item.InvoiceType == "CJ") // != 0 && item.InvoiceID == 0)
                        {
                            invoicetype = "CJ";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }


                        //RecPayAllocation
                        if (invoicetype == "C")
                        {
                            var recpaydetail = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID && d.InvoiceID == item.InvoiceID select d).FirstOrDefault();
                            if (AWBAllocationall!= null)
                            {
                                var allocationdetail = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == item.InvoiceID).ToList();
                                foreach (var aitem in allocationdetail)
                                {

                                    aitem.RecPayDetailID = 0;
                                    RecPayAllocationDetail allocation = new RecPayAllocationDetail();
                                    allocation.CustomerInvoiceDetailID = aitem.CustomerInvoiceDetailID;
                                    allocation.CustomerInvoiceID = aitem.CustomerInvoiceId;
                                    allocation.RecPayID = RecP.RecPayID;
                                    allocation.InScanID = aitem.InScanID;
                                    allocation.RecPayDetailID = recpaydetail.RecPayDetailID; // salesinvoicedetails.RecPayDetailId;
                                    allocation.AllocatedAmount = aitem.AllocatedAmount;
                                    Context1.RecPayAllocationDetails.Add(allocation);
                                    Context1.SaveChanges();
                                }
                            }

                        }


                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
                //To Balance Invoice AMount
                if (RecP.FMoney > 0)
                {
                    if (RecP.StatusEntry != "ST" && RecP.StatusEntry != "DR")
                        ReceiptDAO.InsertJournalOfCustomer(RecP.RecPayID, fyearid);
                    else if (RecP.StatusEntry == "DR")
                        ReceiptDAO.InsertJournalOfCustomerDRR(RecP.RecPayID, fyearid);


                }
                //var Recpaydata = (from d in Context1.RecPays where d.RecPayID == RecP.RecPayID select d).FirstOrDefault();

                //Recpaydata.RecPayID = RecP.RecPayID;
                //Recpaydata.IsTradingReceipt = true;
                //Context1.Entry(Recpaydata).State = EntityState.Modified;
                //Context1.SaveChanges();

            }
            else //edit mode
            {
                decimal Fmoney = 0;
                //for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                //{
                //    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                //}

                RecPay recpay = new RecPay();
                recpay = Context1.RecPays.Find(RecP.RecPayID);
                recpay.RecPayDate = RecP.RecPayDate;
                recpay.RecPayID = RecP.RecPayID;
                recpay.AcJournalID = RecP.AcJournalID;
                recpay.BankName = RecP.BankName;
                recpay.ChequeDate = RecP.ChequeDate;
                recpay.ChequeNo = RecP.ChequeNo;
                recpay.CustomerID = RecP.CustomerID;
                recpay.DocumentNo = RecP.DocumentNo;
                recpay.EXRate = RecP.EXRate;
                //recpay.FYearID = RecP.FYearID;
                recpay.FMoney = RecP.FMoney;
                recpay.OtherReceipt = RecP.OtherReceipt;
                recpay.StatusEntry = RecP.StatusEntry;
                //recpay.IsTradingReceipt = true;
                recpay.Remarks = RecP.Remarks;
                recpay.ModifiedDate = CommonFunctions.GetBranchDateTime();
                recpay.ModifiedBy = UserID;
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                var consignmentdetails = (from d in Context1.RecPayAllocationDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayAllocationDetails.RemoveRange(consignmentdetails);
                Context1.SaveChanges();

                foreach (var item in RecP.CustomerRcieptChildVM)
                {
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "C";
                        if (item.InvoiceType=="OP" )
                        //    if (item.InvoiceType == "OP")
                            {
                            invoicetype = "COP";
                            if (item.AcOPInvoiceDetailID >0)
                                {
                           ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                            }
                            else
                            {
                                ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                            }
                            
                        }
                    else if(item.InvoiceType=="CJ")
                        {
                            invoicetype = "CJ";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }


                        //RecPayAllocation
                        if (invoicetype == "C")
                        {
                            var recpaydetail = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID && d.InvoiceID == item.InvoiceID select d).FirstOrDefault();
                            if (AWBAllocationall != null) 
                            { 
                                var allocationdetail = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == item.InvoiceID).ToList();
                            foreach (var aitem in allocationdetail)
                            {

                                aitem.RecPayDetailID = 0;
                                RecPayAllocationDetail allocation = new RecPayAllocationDetail();
                                allocation.CustomerInvoiceDetailID = aitem.CustomerInvoiceDetailID;
                                allocation.CustomerInvoiceID = aitem.CustomerInvoiceId;
                                allocation.RecPayID = RecP.RecPayID;
                                allocation.InScanID = aitem.InScanID;
                                allocation.RecPayDetailID = recpaydetail.RecPayDetailID; // salesinvoicedetails.RecPayDetailId;
                                allocation.AllocatedAmount = aitem.AllocatedAmount;
                                Context1.RecPayAllocationDetails.Add(allocation);
                                Context1.SaveChanges();

                            }
                         }
                        }

                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }

                if (RecP.FMoney > 0)
                {
                    if (RecP.StatusEntry != "ST" && RecP.StatusEntry != "DR")
                        ReceiptDAO.InsertJournalOfCustomer(RecP.RecPayID, fyearid);
                    else if (RecP.StatusEntry == "DR")
                        ReceiptDAO.InsertJournalOfCustomerDRR(RecP.RecPayID, fyearid);


                }


            }

            //BindAllMasters(2);
            return RedirectToAction("Index", "CustomerReceipt", new { ID = RecP.RecPayID });
        }

        public JsonResult GetInvoiceOfCustomer(string ID)
        {
            //List<SP_GetCustomerInvoiceDetailsForReciept_Result> AllInvoices = new List<SP_GetCustomerInvoiceDetailsForReciept_Result>();

            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            var AllInvoices = ReceiptDAO.GetCustomerInvoiceDetailsForReciept(Convert.ToInt32(ID), fromdate.Date.ToString(), todate.Date.ToString()).OrderBy(x => x.InvoiceDate).ToList();

            return Json(AllInvoices, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetExchangeRateByCurID(string ID)
        {
            //List<SP_GetCustomerInvoiceDetailsForReciept_Result> AllInvoices = new List<SP_GetCustomerInvoiceDetailsForReciept_Result>();

            var ER = RP.GetExchgeRateByCurID(Convert.ToInt32(ID));

            return Json(ER, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult DeleteCustomerDetTrade(int id)
        {
            string status = "";
            string message = "";
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCustomerReceipt(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            status = dt.Rows[0][0].ToString();
                            message = dt.Rows[0][1].ToString();
                            //TempData["ErrorMsg"] = "Transaction Exists. Deletion Restricted !";
                            return Json(new { status = status, message = message });
                        }

                    }
                    else
                    {
                        //TempData["SuccessMsg"] = "You have successfully Deleted Cost !!";
                        return Json(new { status = "Failed", message = "Delete Failed!" });
                    }
                }
                else
                {
                    //TempData["SuccessMsg"] = "You have successfully Deleted Cost !!";
                    return Json(new { status = "Failed", message = "Delete Failed!" });
                }
            }

            return Json(new { status = "Failed", message = "Delete Failed!" });

        }

        public JsonResult ReceiptReport(int id)
        {
            string reportpath = "";
            //int k = 0;
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GenerateCustomerReceipt(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

        }

       

        public void BindAllMasters(int pagetype)
        {
            //List<CustomerMaster> Customers = new List<CustomerMaster>();
            //Customers = MM.GetAllCustomer();

            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = MM.GetCurrency();

            string DocNo = ReceiptDAO.GetMaxCustomerReceiptNo(); //  RP.GetMaxRecieptDocumentNo();

            ViewBag.DocumentNos = DocNo;
            //if (pagetype == 1)
            //{
            //    var customernew = (from d in Context1.CustomerMasters where d.CustomerType == "CS" select d).ToList();

            //    ViewBag.Customer = new SelectList(customernew, "CustomerID", "Customer1");
            //}
            //else
            //{
            //    var customernew = (from d in Context1.CustomerMasters where d.CustomerType == "CS" select d).ToList();

            //    ViewBag.Customer = new SelectList(customernew, "CustomerID", "Customer1");
            //}

            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName");
        }

        public void BindMasters_ForEdit(CustomerRcieptVM cust)
        {
            //List<CustomerMaster> Customers = new List<CustomerMaster>();
            //Customers = MM.GetAllCustomer();

            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = MM.GetCurrency();


            ViewBag.DocumentNos = cust.DocumentNo;

            //ViewBag.Customer = new SelectList(Customers, "CustomerID", "Customer", cust.CustomerID);

            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName", cust.CurrencyId);

        }

         


        public JsonResult GetAllCustomerByDate(string fdate, string tdate, int FYearID)
        {
            DateTime d = DateTime.Now;
            DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

            int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
            DateTime mend = new DateTime(fyear.Year, d.Month, maxday);

            var sdate = DateTime.Parse(fdate);
            var edate = DateTime.Parse(tdate);

            ViewBag.AllCustomers = MM.GetAllCustomer();

            var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.IsTradingReceipt != true && x.FYearID == FYearID).OrderByDescending(x => x.RecPayDate).ToList();

            //var recpayid = data.FirstOrDefault().RecPayID;
            //var Recdetails = (from x in Context1.RecPayDetails where x.RecPayID == recpayid && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault();


            data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");

            //var cust = Context1.SP_GetAllRecieptsDetailsByDate(fdate, tdate, FYearID).ToList();

            string view = this.RenderPartialView2("_GetAllCustomerByDate", data);

            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    view = view
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };


        }

        public ActionResult GetReceiptsByDate(string fdate, string tdate, int FYearID)
        {
            DateTime d = DateTime.Now;
            DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

            int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
            DateTime mend = new DateTime(fyear.Year, d.Month, maxday);
            var sdate = DateTime.Parse(fdate);
            var edate = DateTime.Parse(tdate);

            ViewBag.AllCustomers = Context1.CustomerMasters.ToList();

            var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.IsTradingReceipt != true && x.FYearID == FYearID).OrderByDescending(x => x.RecPayDate).ToList();
            data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");


            //var cust = Context1.SP_GetAllRecieptsDetailsByDate(fdate, tdate, FYearID).ToList();

            return PartialView("_GetAllCustomerByDate", data);

        }

        //public JsonResult GetAllTradeCustomerByDate(string fdate, string tdate, int FYearID)
        //{
        //    DateTime d = DateTime.Now;
        //    DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
        //    DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

        //    int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
        //    DateTime mend = new DateTime(fyear.Year, d.Month, maxday);

        //    //var sdate = DateTime.Parse(fdate);
        //    //var edate = DateTime.Parse(tdate);
        //    var sdate = Convert.ToDateTime(fdate);
        //    var edate = Convert.ToDateTime(tdate);

        //    //var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.IsTradingReceipt == true && x.FYearID == FYearID).OrderByDescending(x => x.RecPayDate).ToList();
        //    //var cust = Context1.SP_GetAllRecieptsDetailsByDate(fdate, tdate, FYearID).ToList();
        //    var data = ReceiptDAO.GetCustomerReceiptsByDate(fdate, tdate, FYearID);
        //    //data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");

        //    ViewBag.AllCustomers = Context1.CustomerMasters.ToList();
        //    string view = this.RenderPartialView("_GetAllTradeCustomerByDate", data);
        //    //string view = this.RenderPartialView("_Table", data);

        //    return new JsonResult
        //    {
        //        Data = new
        //        {
        //            success = true,
        //            view = view
        //        },
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };


        //}
        //public ActionResult GetTradeReceiptsByDate(string fdate, string tdate, int FYearID)
        //{
        //    DateTime d = DateTime.Now;
        //    DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
        //    DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

        //    int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
        //    DateTime mend = new DateTime(fyear.Year, d.Month, maxday);
        //    var sdate = DateTime.Parse(fdate);
        //    var edate = DateTime.Parse(tdate);
        //    ViewBag.AllCustomers = Context1.CustomerMasters.ToList();
        //    var cust = ReceiptDAO.GetCustomerReceiptsByDate(fdate, tdate, FYearID).ToList();

        //    //var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.FYearID == FYearID && x.IsTradingReceipt == true).OrderByDescending(x => x.RecPayDate).ToList();

        //    //data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");




        //    return PartialView("_GetAllTradeCustomerByDate", cust);

        //}
        [HttpGet]
        public ActionResult CustomerTradeReceiptDetails(int ID, string FromDate, string ToDate)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            List<ReceiptVM> Reciepts = new List<ReceiptVM>();
            DateTime pFromDate;
            DateTime pToDate;
            if (FromDate == null || ToDate == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // // ToDate = DateTime.Now;

                pFromDate = AccountsDAO.CheckParamDate(pFromDate, FyearId).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, FyearId).Date;
            }
            else
            {
                pFromDate = Convert.ToDateTime(FromDate);//.AddDays(-1);
                pToDate = Convert.ToDateTime(ToDate);

            }

            Reciepts = ReceiptDAO.GetCustomerReceipts(FyearId, pFromDate, pToDate); // RP.GetAllReciepts();
            ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
            ViewBag.ToDate = pToDate.Date.ToString("dd-MM-yyyy");
            // var data = (from t in Reciepts where (t.RecPayDate >= Convert.ToDateTime(Session["FyearFrom"]) && t.RecPayDate <= Convert.ToDateTime(Session["FyearTo"])) select t).ToList();
            if (ID > 0)
            {
                ViewBag.SuccessMsg = "You have successfully added Customer Reciept.";
            }


            if (ID == 10)
            {
                ViewBag.SuccessMsg = "You have successfully deleted Customer Reciept.";
            }

            if (ID == 20)
            {
                ViewBag.SuccessMsg = "You have successfully updated Customer Reciept.";
            }


            Session["ID"] = ID;


            return View(Reciepts);
        }
        [HttpGet]
        public ActionResult CustomerTradeReceipt(int id)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Customer Receipt - Modify";
                    cust = RP.GetRecPayByRecpayID(id);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    cust.recPayDetail = Context1.RecPayDetails.Where(item => item.RecPayID == id).ToList();

                    decimal Advance = 0;
                    //Advance = ReceiptDAO.SP_GetCustomerAdvance(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(id), FyearId);
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    cust.Balance = Advance;
                    foreach (var item in cust.recPayDetail)
                    {
                        if (item.AcOPInvoiceDetailID > 0)
                        {
                            var sInvoiceDetail = (from d in Context1.AcOPInvoiceDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                var invoicetotal = sInvoiceDetail.Sum(d => d.Amount); //  sInvoiceDetail.Sum(d=>d.OtherCharge);                                                                
                                //var allrecpay = (from d in Context1.RecPayDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                                //var allrecpay = (from d in Context1.RecPayDetails join c in Context1.RecPays on d.RecPayID equals c.RecPayID where c.RecPayDate.Value < cust.RecPayDate && d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                                var totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(item.AcOPInvoiceDetailID), Convert.ToInt32(id), 0, "OP");
                                //var totamtpaid = allrecpay.Sum(d => d.Amount) * -1;
                                //var totadjust = allrecpay.Sum(d => d.AdjustmentAmount);
                                //var CreditNote = (from d in Context1.CreditNotes where d.InvoiceID == item.InvoiceID && d.CustomerID == Sinvoice.CustomerID select d).ToList();
                                //decimal? CreditAmount = 0;
                                //if (CreditNote.Count > 0)
                                //{
                                //    CreditAmount = CreditNote.Sum(d => d.Amount);
                                //}
                                var totamt = totamtpaid;// + totadjust;// + CreditAmount;
                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.InvoiceID = 0;
                                customerinvoice.AcOPInvoiceDetailID = sInvoiceDetail[0].AcOPInvoiceDetailID;
                                customerinvoice.InvoiceType = "OP";
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.AcOPInvoiceDetailID;
                                customerinvoice.SInvoiceNo = sInvoiceDetail[0].InvoiceNo;
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBeRecieved = Convert.ToDecimal(invoicetotal);// - Convert.ToDecimal(totamt) - Convert.ToDecimal(item.Amount);
                                customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid); // Convert.ToDecimal(totamtpaid) - Convert.ToDecimal(totadjust);// ;// customerinvoice.AmountToBeRecieved;
                                customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                customerinvoice.Balance = customerinvoice.AmountToBeRecieved - totamtpaid;// customerinvoice.AmountToBePaid; // (Convert.ToDecimal(invoicetotal) - Convert.ToDecimal(totamt)) - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                        else if (item.InvoiceID > 0 && (item.AcOPInvoiceDetailID == 0 || item.AcOPInvoiceDetailID == null))
                        {
                            cust.AWBAllocation = ReceiptDAO.GetAWBAllocation(cust.AWBAllocation, Convert.ToInt32(item.InvoiceID), Convert.ToDecimal(item.Amount), cust.RecPayID); //customer invoiceid,amount
                            var sInvoiceDetail = (from d in Context1.CustomerInvoiceDetails where d.CustomerInvoiceID == item.InvoiceID select d).ToList();
                            var awbDetail = (from d in Context1.RecPayAllocationDetails where d.CustomerInvoiceID == item.InvoiceID && d.RecPayDetailID == item.RecPayDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                var invoicetotal = sInvoiceDetail.Sum(d => d.NetValue); //  sInvoiceDetail.Sum(d=>d.OtherCharge);
                                var awbtotal = awbDetail.Sum(d => d.AllocatedAmount);
                                var Sinvoice = (from d in Context1.CustomerInvoices where d.CustomerInvoiceID == item.InvoiceID select d).FirstOrDefault();
                                //var allrecpay = (from d in Context1.RecPayDetails join c in Context1.RecPays on d.RecPayID equals c.RecPayID where c.RecPayDate.Value<cust.RecPayDate &&  d.InvoiceID == item.InvoiceID select d).ToList();
                                var totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(cust.CustomerID), Convert.ToInt32(item.InvoiceID), Convert.ToInt32(id), 0,"TR");
                                //var totamtpaid = allrecpay.Sum(d => d.Amount) * -1;
                                //var totadjust = allrecpay.Sum(d => d.AdjustmentAmount);
                                //var CreditNote = (from d in Context1.CreditNotes where d.InvoiceID == item.InvoiceID && d.CustomerID == Sinvoice.CustomerID select d).ToList();
                                //decimal? CreditAmount = 0;
                                //if (CreditNote.Count > 0)
                                //{
                                //    CreditAmount = CreditNote.Sum(d => d.Amount);
                                //}
                                //var totamt = totamtpaid + totadjust + CreditAmount;
                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.InvoiceID = Convert.ToInt32(item.InvoiceID);
                                customerinvoice.AcOPInvoiceDetailID = 0;
                                customerinvoice.InvoiceType = "TR";
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                customerinvoice.SInvoiceNo = Sinvoice.CustomerInvoiceNo;
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBeRecieved = Convert.ToDecimal(invoicetotal);// - Convert.ToDecimal(totamt) - Convert.ToDecimal(item.Amount);
                                customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid); // - customerinvoice.AmountToBeRecieved;
                                customerinvoice.Balance = (Convert.ToDecimal(invoicetotal) - totamtpaid);// customerinvoice.AmountToBePaid); // ; // - Convert.ToDecimal(totamt)) - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                    }
                    Session["AWBAllocation"] = cust.AWBAllocation;
                    BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "Customer Receipt - Create";
                    BindAllMasters(2);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.RecPayDate = pFromDate;
                    cust.RecPayID = 0;
                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
            var StaffNotes = (from d in Context1.StaffNotes where d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var users = (from d in Context1.UserRegistrations select d).ToList();

            var staffnotemodel = new List<StaffNoteModel>();
            foreach (var item in StaffNotes)
            {
                var model = new StaffNoteModel();
                model.id = item.NotesId;
                model.employeeid = item.EmployeeId;
                //model.jobid = item.JobId;
                model.TaskDetails = item.Notes;
                model.Datetime = item.EntryDate;
                model.EmpName = users.Where(d => d.UserID == item.EmployeeId).FirstOrDefault().UserName;
                staffnotemodel.Add(model);
            }
            ViewBag.StaffNoteModel = staffnotemodel;
            var customerdetails = (from d in Context1.CustomerMasters where d.CustomerID == cust.CustomerID && d.CustomerType == "CS" select d).FirstOrDefault();
            if (customerdetails == null)
            {
                customerdetails = new CustomerMaster();
            }
            ViewBag.CustomerDetail = customerdetails;
            var CustomerNotification = (from d in Context1.CustomerNotifications where d.RecPayID == id && d.PageTypeId == 2 orderby d.NotificationId descending select d).ToList();

            var customernotification = new List<CustomerNotificationModel>();
            foreach (var item in CustomerNotification)
            {
                var model = new CustomerNotificationModel();
                model.id = item.NotificationId;
                model.employeeid = item.UserId;
                model.jobid = item.RecPayID;
                model.Message = item.MessageText;
                model.Datetime = item.EntryDate;
                model.IsEmail = item.NotifyByEmail;
                model.IsSms = item.NotifyBySMS;
                model.IsWhatsapp = item.NotifyByWhatsApp;
                model.EmpName = users.Where(d => d.UserID == item.UserId).FirstOrDefault().UserName;
                customernotification.Add(model);
            }
            cust.AWBAllocation = new List<ReceiptAllocationDetailVM>();
            ViewBag.CustomerNotification = customernotification;
            return View(cust);

        }

        [HttpPost]
        public JsonResult GetTradeInvoiceOfCustomer(int? ID, decimal? amountreceived, int? RecPayId,string RecPayType="CR")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
           
            //var AllInvoices = (from d in Context1.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0,Convert.ToInt32(RecPayId),0, "B",RecPayType);
            int rowindex = 0;
            for(rowindex=0;rowindex<Allinvoices.Count;rowindex++) // each (var Invoice in Allinvoices)
            {
                if (amountreceived!=null)
                {
                    if (amountreceived == 0)
                    {
                        Session["AWBAllocation"] = AWBAllocation;
                        return Json(new { advance = 0, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Allinvoices[rowindex].Balance>0)
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
                        if (RecPayId == null || RecPayId==0)
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
        [HttpPost]
        public ActionResult CustomerTradeReceipt(CustomerRcieptVM RecP, string Command, string Currency)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            int i = 0;
            decimal TotalAmount = 0;

            int UserID = Convert.ToInt32(Session["UserID"]);
            var StaffNotes = (from d in Context1.StaffNotes where d.RecPayID == RecP.RecPayID && d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var users = (from d in Context1.UserRegistrations select d).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocationall = (List<ReceiptAllocationDetailVM>)Session["AWBAllocation"];
            var staffnotemodel = new List<StaffNoteModel>();
            foreach (var item in StaffNotes)
            {
                var model = new StaffNoteModel();
                model.id = item.NotesId;
                model.employeeid = item.EmployeeId;
                //model.jobid = item.JobId;
                model.TaskDetails = item.Notes;
                model.Datetime = item.EntryDate;
                model.EmpName = users.Where(d => d.UserID == item.EmployeeId).FirstOrDefault().UserName;
                staffnotemodel.Add(model);
            }
            ViewBag.StaffNoteModel = staffnotemodel;
            //if (RecP.RecPayID > 0)
            //{
            //    RP.EditCustomerRecPay(RecP, Session["UserID"].ToString());
            //    RP.EditCustomerRecieptDetails(RecP.recPayDetail, RecP.RecPayID);
            //}
            if (RecP.CashBank != null)
            {
                RecP.StatusEntry = "CS";
                int acheadid = Convert.ToInt32(RecP.CashBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
            }
            else if (RecP.ChequeBank !=null)
            {
                RecP.StatusEntry = "BK";
                int acheadid = Convert.ToInt32(RecP.ChequeBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
            }
            else
            {
                RecP.StatusEntry = "ST";
                RecP.AcOPInvoiceDetailID = RecP.AcOPInvoiceDetailID;
                RecP.BankName = RecP.OPRefNo;
            }
            if (RecP.CustomerRcieptChildVM == null)
            {
                RecP.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            }
            //Adding Entry in Rec PAY

            ///Insert Entry For RecPay Details 
            ///
            if (RecP.RecPayID <= 0)
            {
                decimal Fmoney = 0;
                for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                {
                    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                }
                //if (Fmoney > 0)
                //{
                //    RecP.FMoney = Fmoney;
                //}
                RecP.AcCompanyID = branchid;
                RecP.FYearID = fyearid;
                RecP.UserID = UserID;
                RPID = ReceiptDAO.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString()); //.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString());

                RecP.RecPayID = (from c in Context1.RecPays orderby c.RecPayID descending select c.RecPayID).FirstOrDefault();


                foreach (var item in RecP.CustomerRcieptChildVM)
                {
                    decimal Advance = 0;
                    Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "C";
                        if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        {
                            invoicetype = "COP";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }


                        //RecPayAllocation
                        if (invoicetype == "C")
                        {
                            var recpaydetail = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID && d.InvoiceID == item.InvoiceID select d).FirstOrDefault();
                            var allocationdetail = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == item.InvoiceID).ToList();
                            foreach (var aitem in allocationdetail)
                            {

                                aitem.RecPayDetailID = 0;
                                RecPayAllocationDetail allocation = new RecPayAllocationDetail();
                                allocation.CustomerInvoiceDetailID = aitem.CustomerInvoiceDetailID;
                                allocation.CustomerInvoiceID = aitem.CustomerInvoiceId;
                                allocation.RecPayID = RecP.RecPayID;
                                allocation.InScanID = aitem.InScanID;
                                allocation.RecPayDetailID = recpaydetail.RecPayDetailID; // salesinvoicedetails.RecPayDetailId;
                                allocation.AllocatedAmount = aitem.AllocatedAmount;
                                Context1.RecPayAllocationDetails.Add(allocation);
                                Context1.SaveChanges();
                            }

                        }


                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
                //To Balance Invoice AMount
                if (RecP.FMoney > 0)
                {
                    if (RecP.StatusEntry != "ST")
                    {                        
                        ReceiptDAO.InsertJournalOfCustomer(RecP.RecPayID, fyearid);
                    }

                }
                //var Recpaydata = (from d in Context1.RecPays where d.RecPayID == RecP.RecPayID select d).FirstOrDefault();

                //Recpaydata.RecPayID = RecP.RecPayID;
                //Recpaydata.IsTradingReceipt = true;
                //Context1.Entry(Recpaydata).State = EntityState.Modified;
                //Context1.SaveChanges();

            }
            else //edit mode
            {
                decimal Fmoney = 0;
                for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                {
                    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                }

                RecPay recpay = new RecPay();
                recpay = Context1.RecPays.Find(RecP.RecPayID);
                recpay.RecPayDate = RecP.RecPayDate;
                recpay.RecPayID = RecP.RecPayID;
                recpay.AcJournalID = RecP.AcJournalID;
                recpay.BankName = RecP.BankName;
                recpay.ChequeDate = RecP.ChequeDate;
                recpay.ChequeNo = RecP.ChequeNo;
                recpay.CustomerID = RecP.CustomerID;
                recpay.DocumentNo = RecP.DocumentNo;
                recpay.EXRate = RecP.EXRate;
                //recpay.FYearID = RecP.FYearID;
                recpay.FMoney = RecP.FMoney;
                recpay.StatusEntry = RecP.StatusEntry;
                //recpay.IsTradingReceipt = true;
                recpay.Remarks = RecP.Remarks;
                recpay.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                recpay.ModifiedBy = RecP.UserID;
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                var consignmentdetails = (from d in Context1.RecPayAllocationDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayAllocationDetails.RemoveRange(consignmentdetails);
                Context1.SaveChanges();

                foreach (var item in RecP.CustomerRcieptChildVM)
                {
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "C";
                        if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        {
                            invoicetype = "COP";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }

 
                        //RecPayAllocation
                        if (invoicetype == "C")
                        {
                            var recpaydetail = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID && d.InvoiceID == item.InvoiceID select d).FirstOrDefault();
                            var allocationdetail = AWBAllocationall.Where(cc => cc.CustomerInvoiceId == item.InvoiceID).ToList();
                            foreach (var aitem in allocationdetail)
                            {

                                aitem.RecPayDetailID = 0;
                                RecPayAllocationDetail allocation = new RecPayAllocationDetail();
                                allocation.CustomerInvoiceDetailID = aitem.CustomerInvoiceDetailID;
                                allocation.CustomerInvoiceID = aitem.CustomerInvoiceId;
                                allocation.RecPayID = RecP.RecPayID;
                                allocation.InScanID = aitem.InScanID;
                                allocation.RecPayDetailID = recpaydetail.RecPayDetailID; // salesinvoicedetails.RecPayDetailId;
                                allocation.AllocatedAmount = aitem.AllocatedAmount;
                                Context1.RecPayAllocationDetails.Add(allocation);
                                Context1.SaveChanges();

                            }
                        }

                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
               
                if (RecP.FMoney > 0)
                {
                    if (RecP.StatusEntry!="ST")
                        ReceiptDAO.InsertJournalOfCustomer(RecP.RecPayID, fyearid);
                                      

                }

            }


            BindAllMasters(2);
            return RedirectToAction("CustomerTradeReceiptDetails", "CustomerReceipt", new { ID = RecP.RecPayID });
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


        [HttpGet]
        public JsonResult GetOpeningPayment(string term,int CustomerId)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            List<OpeningInvoiceVM> list = new List<OpeningInvoiceVM>();

            //if (Session["OPCustomerPayment"]!=null)
            //list = (List<OpeningInvoiceVM>)Session["OPCustomerPayment"];
            //if (list!=null)
            //{
            //    if (list.Count>0)
            //    {
            //        if (list[0].CustomerId!=CustomerId)
            //        {
            //            list = null;
            //        }
            //    }
            //}
            //if (list==null)
                list = ReceiptDAO.GetCustomerOpeningReceipts(CustomerId, branchid, FyearId);

            Session["OPCustomerPayment"] = list;
            if (term.Trim() != "")
            {
                var lst = list.Where(cc => cc.RefNo.ToLower().Contains(term.ToLower())).ToList();
                
                return Json(lst, JsonRequestBehavior.AllowGet);
            }
            else
            {                

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
            




        

        //public JsonResult ExportToPDF(int recpayid)
        //{
        //    //Report  
        //    try
        //    {
        //        decimal? totalamt = 0;
        //        int? currencyId = 0;

        //        ReportViewer reportViewer = new ReportViewer();

        //        reportViewer.ProcessingMode = ProcessingMode.Local;
        //        reportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ReceiptVoucher.rdlc");

        //        DataTable dtcompany = new DataTable();
        //        dtcompany.Columns.Add("CompanyName");
        //        dtcompany.Columns.Add("Address1");
        //        dtcompany.Columns.Add("Address2");
        //        dtcompany.Columns.Add("Address3");
        //        dtcompany.Columns.Add("Phone");
        //        dtcompany.Columns.Add("AcHead");
        //        dtcompany.Columns.Add("Todate");

        //        var company = Context1.AcCompanies.FirstOrDefault();
        //        string imagePath = new Uri(Server.MapPath("~/Content/Logo/" + company.logo)).AbsoluteUri;

        //        DataRow dr = dtcompany.NewRow();
        //        dr[0] = company.AcCompany1;
        //        dr[1] = company.Address1;
        //        dr[2] = company.Address2;
        //        dr[3] = company.Address3;
        //        dr[4] = company.Phone;
        //        dr[5] = imagePath;
        //        dr[6] = DateTime.Now;

        //        dtcompany.Rows.Add(dr);

        //        var receipt = (from d in Context1.RecPays where d.RecPayID == recpayid select d).FirstOrDefault();
        //        totalamt = receipt.FMoney;

        //        if (receipt.IsTradingReceipt == true)
        //        {
        //            var recpaydetails = (from d in Context1.RecPayDetails where d.RecPayID == recpayid where d.InvoiceID > 0 select d).ToList();
        //            var currency = recpaydetails.Where(d => d.CurrencyID > 0).FirstOrDefault();
        //            if (currency != null)
        //            {
        //                currencyId = currency.CurrencyID;
        //            }
        //            var cust = Context1.CustomerMasters.Where(d => d.CustomerID == receipt.CustomerID).FirstOrDefault();
        //            var listofdet = new List<ReportCustomerReceipt_Result>();
        //            foreach (var item in recpaydetails)
        //            {
        //                var sinvoicedet = (from d in Context1.SalesInvoiceDetails where d.SalesInvoiceDetailID == item.InvoiceID select d).FirstOrDefault();
        //                var sinvoice = (from d in Context1.SalesInvoices where d.SalesInvoiceID == sinvoicedet.SalesInvoiceID select d).FirstOrDefault();
        //                var customerrecpay = new ReportCustomerReceipt_Result();
        //                customerrecpay.Date = receipt.RecPayDate.Value.ToString("dd-MMM-yyyy");
        //                customerrecpay.ReceivedFrom = cust.Customer1;
        //                customerrecpay.DocumentNo = receipt.DocumentNo;
        //                customerrecpay.Amount = Convert.ToDecimal(receipt.FMoney);
        //                customerrecpay.Remarks = receipt.Remarks;
        //                customerrecpay.Account = receipt.BankName;
        //                if (receipt.ChequeDate != null)
        //                {
        //                    customerrecpay.ChequeDate = receipt.ChequeDate.Value.ToString("dd-MMM-yyyy");
        //                }
        //                else
        //                {
        //                    customerrecpay.ChequeDate = "";
        //                }
        //                if (!string.IsNullOrEmpty(receipt.ChequeNo))
        //                {
        //                    customerrecpay.ChequeNo = Convert.ToDecimal(receipt.ChequeNo);
        //                }
        //                customerrecpay.CustomerBank = "";
        //                customerrecpay.DetailDocNo = sinvoice.SalesInvoiceNo;
        //                customerrecpay.DocDate = sinvoice.SalesInvoiceDate.Value.ToString("dd-MMM-yyyy");
        //                customerrecpay.DocAmount = Convert.ToDecimal(sinvoicedet.NetValue);

        //                if (item.Amount > 0)
        //                {
        //                    customerrecpay.SettledAmount = Convert.ToDecimal(item.Amount);
        //                    customerrecpay.AdjustmentAmount = Convert.ToInt32(item.AdjustmentAmount);
        //                }
        //                else
        //                {
        //                    customerrecpay.SettledAmount = Convert.ToDecimal(item.Amount) * -1;
        //                    customerrecpay.AdjustmentAmount = Convert.ToInt32(item.AdjustmentAmount);
        //                }
        //                listofdet.Add(customerrecpay);
        //            }

        //            ReportDataSource _rsource;

        //            //var dd = entity.ReportCustomerReceipt(recpayid).ToList();
        //            _rsource = new ReportDataSource("ReceiptVoucher", listofdet);
        //            reportViewer.LocalReport.DataSources.Add(_rsource);

        //        }
        //        else
        //        {
        //            var recpaydetails = (from d in Context1.RecPayDetails where d.RecPayID == recpayid where d.InvoiceID > 0 select d).ToList();
        //            var currency = recpaydetails.Where(d => d.CurrencyID > 0).FirstOrDefault();
        //            if (currency != null)
        //            {
        //                currencyId = currency.CurrencyID;
        //            }
        //            var cust = Context1.CUSTOMERs.Where(d => d.CustomerID == receipt.CustomerID).FirstOrDefault();
        //            var listofdet = new List<ReportCustomerReceipt_Result>();
        //            foreach (var item in recpaydetails)
        //            {
        //                var sinvoicedet = (from d in Context1.JInvoices where d.InvoiceID == item.InvoiceID select d).FirstOrDefault();
        //                var sinvoice = (from d in Context1.JobGenerations where d.JobID == sinvoicedet.JobID select d).FirstOrDefault();
        //                var customerrecpay = new ReportCustomerReceipt_Result();
        //                customerrecpay.Date = receipt.RecPayDate.Value.ToString("dd-MMM-yyyy");
        //                customerrecpay.ReceivedFrom = cust.Customer1;
        //                customerrecpay.DocumentNo = receipt.DocumentNo;
        //                customerrecpay.Amount = Convert.ToDecimal(receipt.FMoney);
        //                customerrecpay.Remarks = receipt.Remarks;
        //                customerrecpay.Account = receipt.BankName;
        //                if (receipt.ChequeDate != null)
        //                {
        //                    customerrecpay.ChequeDate = receipt.ChequeDate.Value.ToString("dd-MMM-yyyy");
        //                }
        //                else
        //                {
        //                    customerrecpay.ChequeDate = "";
        //                }
        //                if (!string.IsNullOrEmpty(receipt.ChequeNo))
        //                {
        //                    customerrecpay.ChequeNo = Convert.ToDecimal(receipt.ChequeNo);
        //                }
        //                customerrecpay.CustomerBank = "";
        //                customerrecpay.DetailDocNo = sinvoice.InvoiceNo.ToString();
        //                customerrecpay.DocDate = sinvoice.InvoiceDate.Value.ToString("dd-MMM-yyyy");
        //                customerrecpay.DocAmount = Convert.ToDecimal(sinvoicedet.SalesHome);

        //                if (item.Amount > 0)
        //                {
        //                    customerrecpay.SettledAmount = Convert.ToDecimal(item.Amount);
        //                    customerrecpay.AdjustmentAmount = Convert.ToInt32(item.AdjustmentAmount);
        //                }
        //                else
        //                {
        //                    customerrecpay.SettledAmount = Convert.ToDecimal(item.Amount) * -1;
        //                    customerrecpay.AdjustmentAmount = Convert.ToInt32(item.AdjustmentAmount);
        //                }
        //                listofdet.Add(customerrecpay);
        //            }

        //            ReportDataSource _rsource;

        //            //var dd = entity.ReportCustomerReceipt(recpayid).ToList();
        //            _rsource = new ReportDataSource("ReceiptVoucher", listofdet);
        //            reportViewer.LocalReport.DataSources.Add(_rsource);

        //        }
        //        ReportDataSource _rsource1 = new ReportDataSource("Company", dtcompany);


        //        reportViewer.LocalReport.DataSources.Add(_rsource1);



        //        //foreach (var item in dd)
        //        //{
        //        //    totalamt = 5000;
        //        //}


        //        //DataTable dtuser = new DataTable();
        //        //dtuser.Columns.Add("UserName");

        //        //DataRow dr1 = dtuser.NewRow();
        //        //int uid = Convert.ToInt32(Session["UserID"].ToString());
        //        //dr1[0] = (from c in entity.UserRegistrations where c.UserID == uid select c.UserName).FirstOrDefault();
        //        //dtuser.Rows.Add(dr1);

        //        //ReportDataSource _rsource2 = new ReportDataSource("User", dtuser);

        //        //ReportViewer1.LocalReport.DataSources.Add(_rsource2);


        //        DataTable dtcurrency = new DataTable();
        //        dtcurrency.Columns.Add("SalesCurrency");
        //        dtcurrency.Columns.Add("ForeignCurrency");
        //        dtcurrency.Columns.Add("SalesCurrencySymbol");
        //        dtcurrency.Columns.Add("ForeignCurrencySymbol");
        //        dtcurrency.Columns.Add("InWords");

        //        var currencyName = (from d in Context1.CurrencyMasters where d.CurrencyID == currencyId select d).FirstOrDefault();
        //        if (currencyName == null)
        //        {
        //            currencyName = new CurrencyMaster();
        //        }

        //        DataRow r = dtcurrency.NewRow();
        //        r[0] = currencyName.CurrencyName;
        //        r[1] = "";
        //        r[2] = "";
        //        r[3] = "";
        //        r[4] = currencyName.CurrencyName + ",  " + NumberToWords(Convert.ToInt32(totalamt)) + " /00 baisa.";


        //        dtcurrency.Rows.Add(r);


        //        ReportDataSource _rsource3 = new ReportDataSource("Currency", dtcurrency);

        //        reportViewer.LocalReport.DataSources.Add(_rsource3);
        //        reportViewer.LocalReport.EnableExternalImages = true;
        //        reportViewer.LocalReport.Refresh();

        //        //Byte  
        //        Warning[] warnings;
        //        string[] streamids;
        //        string mimeType, encoding, filenameExtension;

        //        byte[] bytes = reportViewer.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

        //        //File  
        //        string FileName = "Customer_" + DateTime.Now.Ticks.ToString() + ".pdf";
        //        string FilePath = Server.MapPath(@"~\TempFile\") + FileName;
        //        string path = Server.MapPath(@"~\TempFile\");
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        string[] files = Directory.GetFiles(path);

        //        foreach (string file in files)
        //        {
        //            FileInfo fi = new FileInfo(file);
        //            if (fi.LastAccessTime < DateTime.Now.AddMinutes(-5))
        //                try
        //                {
        //                    fi.Delete();
        //                }
        //                catch
        //                {

        //                }
        //        }
        //        //create and set PdfReader  
        //        PdfReader reader = new PdfReader(bytes);
        //        FileStream output = new FileStream(FilePath, FileMode.Create);

        //        string Agent = Request.Headers["User-Agent"].ToString();

        //        //create and set PdfStamper  
        //        PdfStamper pdfStamper = new PdfStamper(reader, output, '0', true);

        //        if (Agent.Contains("Firefox"))
        //            pdfStamper.JavaScript = "var res = app.loaded('var pp = this.getPrintParams();pp.interactive = pp.constants.interactionLevel.full;this.print(pp);');";
        //        else
        //            pdfStamper.JavaScript = "var res = app.setTimeOut('var pp = this.getPrintParams();pp.interactive = pp.constants.interactionLevel.full;this.print(pp);', 200);";

        //        pdfStamper.FormFlattening = false;
        //        pdfStamper.Close();
        //        reader.Close();

        //        //return file path  
        //        string FilePathReturn = @"TempFile/" + FileName;
        //        return Json(new { success = true, path = FilePathReturn }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false, message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);


        //    }
        //}
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }
        public JsonResult UpdateStaffNote(int Jobid, string staffnote)
        {
            try
            {
                var note = new StaffNote();
                note.EntryDate = DateTime.Now;
                note.RecPayID = Jobid;
                note.Notes = staffnote;
                note.PageTypeId = 2;//job 
                note.EmployeeId = Convert.ToInt32(Session["UserID"]);
                Context1.StaffNotes.Add(note);
                Context1.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult SendCustomerNotification(int JobId, string Message, int Customerid, bool whatsapp, bool Email, bool sms)
        {
            var customer = (from d in Context1.CustomerMasters where d.CustomerID == Customerid select d).FirstOrDefault();
            var isemail = false;
            var issms = false;
            var iswhatsapp = false;
            if (Email)
            {
                try
                {
                    var status = SendMailForCustomerNotification(customer.CustomerName, Message, customer.Email);
                    isemail = true;
                }
                catch { }
            }
            if (sms)
            {
                try
                {
                    sendsms(Message);
                    issms = true;
                }
                catch (Exception e)
                {

                }
            }
            if (whatsapp)
            {
                iswhatsapp = true;

            }
            try
            {
                UpdateCustomerNotification(JobId, Message, isemail, issms, iswhatsapp);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);

            }
        }

        public string SendMailForCustomerNotification(string UserName, string Message, string Email)
        {
            var Success = "False";
            //System.IO.StreamReader objReader;
            //objReader = new System.IO.StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("/Templates/CustomerNotification.html"));
            //string content = objReader.ReadToEnd();


            //objReader.Close();
            //content = Regex.Replace(content, "@username", UserName);
            //content = Regex.Replace(content, "@Message", Message);
            try
            {
                using (MailMessage msgMail = new MailMessage())
                {

                    msgMail.From = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"].ToString());
                    msgMail.Subject = "Shipping System";
                    msgMail.IsBodyHtml = true;
                    msgMail.Body = "Testing Email by Generating App Password"; // HttpUtility.HtmlDecode(content);
                    msgMail.To.Add(Email);
                    msgMail.IsBodyHtml = true;

                    //client = new SmtpClient(ConfigurationManager.AppSettings["Host"].ToString());
                    //client.Port = int.Parse(ConfigurationManager.AppSettings["SMTPServerPort"].ToString());
                    //client.UseDefaultCredentials = false;
                    //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUserName"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString());
                    //client.EnableSsl = true;
                    //client.Send(msgMail);

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPAdminEmail"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString());
                        //smtp.Credentials = new System.Net.NetworkCredential(fromAddress,appPassword);
                        smtp.EnableSsl = true;
                        
                        smtp.Send(msgMail);
                    }
                }
                Success = "True";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return Success;
        }
        public bool UpdateCustomerNotification(int Jobid, string Messge, bool isemail, bool issms, bool iswhatsapp)
        {
            try
            {
                var note = new CustomerNotification();
                note.EntryDate = DateTime.Now;
                //note.JobId = Jobid;
                note.MessageText = Messge;
                note.PageTypeId = 2;//job 
                note.UserId = Convert.ToInt32(Session["UserID"]);
                note.NotifyByEmail = isemail;
                note.NotifyBySMS = issms;
                note.NotifyByWhatsApp = iswhatsapp;
                Context1.CustomerNotifications.Add(note);
                Context1.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;

            }
        }
        public string sendsms(string Message)
        {
            //var res= sendSMS();
            // return res;
            String message = HttpUtility.UrlEncode(Message);
            using (var wb = new WebClient())
            {

                byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                {
                {"apikey" , "iCglLGvDnCM-UaAfKLWZ1cEveOQhCfSAakkqn86jbv"},
                {"numbers" , "919344452870"},
                {"message" , message},
                {"sender" , "MTRADE"}
                });
                string result = System.Text.Encoding.UTF8.GetString(response);
                return result;
            }
        }
        public string sendSMS()
        {
            String result;
            string apiKey = "iCglLGvDnCM-UaAfKLWZ1cEveOQhCfSAakkqn86jbv";
            string numbers = "919344452870"; // in a comma seperated list
            string message = "This is your message";
            string sender = "MTRADE";

            String url = "https://api.textlocal.in/send/?apikey=" + apiKey + "&numbers=" + numbers + "&message=" + message + "&sender=" + sender;
            //refer to parameters to complete correct url string

            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);

            objRequest.Method = "POST";
            objRequest.ContentLength = Encoding.UTF8.GetByteCount(url);
            objRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(url);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                myWriter.Close();
            }

            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            return result;
        }
        public void sendsmsss(string Message)
        {
            var message = ""; var from = "MTRADE";
            var uname = "veepeek@yahoo.com"; var hash = "d9fe2afa2f0b66e8418ffcc2f892259b04ddbcc37c22674c5717f2f7a8e21ad0";
            var selectednums = "9344452870"; var url = "";
            var address = "https://www.txtlocal.com/sendsmspost.php";
            var info = 1; var test = 1;

            message = Message;
            message = HttpUtility.UrlEncode(message);
            //encode special characters (e.g. £, & etc) 
            from = ""; uname = ""; hash = ""; selectednums = "";
            url = address + "?uname=" + uname + "&hash=" + hash + "&message=" + message + "&from=" + from + "&selectednums=" + selectednums + "&info=" + info + "&test=" + test;
            Response.Redirect(url);

        }

    }
}
