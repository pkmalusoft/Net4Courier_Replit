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
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
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
    public class SupplierPaymentController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 Context1 = new Entities1();

        EditCommanFu editfu = new EditCommanFu();
        //
        // GET: /CustomerReciept/
        public ActionResult Index()
        {

            CustomerReceiptSearch obj = (CustomerReceiptSearch)Session["SupplierPaymentSearch"];
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
                Session["SupplierPaymentSearch"] = obj;
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
                Session["SupplierPaymentSearch"] = model;
            }
            
            var data = ReceiptDAO.GetSupplierPaymentsByDate(model.FromDate, model.ToDate, yearid, model.ReceiptNo);
            model.Details = data;
            return View(model);

        }

        [HttpPost]
        public ActionResult Index(CustomerReceiptSearch obj)
        {
            Session["SupplierPaymentSearch"] = obj;
            return RedirectToAction("Index");
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

        [HttpGet]
        public ActionResult DeletePayment(int id)
        {            
            if (id != 0)
            {
                ReceiptDAO.DeleteSupplierPayments(id);
            }

            return RedirectToAction("Index", "SupplierPayment", new { ID = 10 });

        }

        public JsonResult ReceiptReport(int id)
        {
            string reportpath = "";
            //int k = 0;
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GenerateSupplierPayment(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

        }
               

        public void BindAllMasters(int pagetype)
        {
            List<CustomerMaster> Customers = new List<CustomerMaster>();
            Customers = MM.GetAllCustomer();

            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = MM.GetCurrency();

            string DocNo = RP.GetMaxPaymentDocumentNo();

            ViewBag.DocumentNos = DocNo;
            if (pagetype == 1)
            {
                var customernew = (from d in Context1.CustomerMasters where d.CustomerType == "CS" select d).ToList();

                ViewBag.Customer = new SelectList(customernew, "CustomerID", "Customer1");
            }
            else
            {
                var customernew = (from d in Context1.CustomerMasters where d.CustomerType == "CS" select d).ToList();

                ViewBag.Customer = new SelectList(customernew, "CustomerID", "Customer1");
            }

            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName");
        }

        public void BindMasters_ForEdit(CustomerRcieptVM cust)
        {
            List<CustomerMaster> Customers = new List<CustomerMaster>();
            Customers = MM.GetAllCustomer();

            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = MM.GetCurrency();


            ViewBag.DocumentNos = cust.DocumentNo;

            ViewBag.Customer = new SelectList(Customers, "CustomerID", "Customer", cust.CustomerID);

            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName", cust.CurrencyId);

        }

        public JsonResult GetAllCurrencyCustReciept()
        {
            //List<SP_GetCustomerInvoiceDetailsForReciept_Result> AllInvoices = new List<SP_GetCustomerInvoiceDetailsForReciept_Result>();
            // var AllInvoices;


            //var CostReciept = (from t in Context1.SPGetAllLocalCurrencyCustRecievable(Convert.ToInt32(Session["fyearid"].ToString()))
            //                   select t).ToList();

            var CostReciept = ReceiptDAO.SPGetAllLocalCurrencyCustRecievable(Convert.ToInt32(Session["fyearid"].ToString()));


            return Json(CostReciept, JsonRequestBehavior.AllowGet);



        }

        public JsonResult GetAllCustomer()
        {
            DateTime d = DateTime.Now;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

            int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
            DateTime mend = new DateTime(fyear.Year, d.Month, maxday);

            var cust = ReceiptDAO.GetCustomerReceipts(fyearid, mstart, mend);// ().Where(x => x.RecPayDate >= mstart && x.RecPayDate <= mend).OrderByDescending(x => x.RecPayDate).ToList();
            //Context1.SP_GetAllRecieptsDetails().Where(x => x.RecPayDate >= mstart && x.RecPayDate <= mend).OrderByDescending(x => x.RecPayDate).ToList();

            string view = this.RenderPartialView2("_GetAllSupplier", cust);

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

            string view = this.RenderPartialView2("_GetAllSupplierByDate", data);

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

            return PartialView("_GetAllSupplierByDate", data);

        }

        public JsonResult GetAllTradeCustomerByDate(string fdate, string tdate, int FYearID)
        {
            DateTime d = DateTime.Now;
            DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime mstart = new DateTime(fyear.Year, d.Month, 01);

            int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
            DateTime mend = new DateTime(fyear.Year, d.Month, maxday);

            var sdate = DateTime.Parse(fdate);
            var edate = DateTime.Parse(tdate);


            //var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.IsTradingReceipt == true && x.FYearID == FYearID).OrderByDescending(x => x.RecPayDate).ToList();
            //var cust = Context1.SP_GetAllRecieptsDetailsByDate(fdate, tdate, FYearID).ToList();
            //var data = ReceiptDAO.GetCustomerReceiptsByDate(fdate, tdate, FYearID);
            //data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");

            ViewBag.AllCustomers = Context1.CustomerMasters.ToList();
            var Reciepts = (from r in Context1.RecPays
                            //join de in Context1.RecPayDetails on r.RecPayID equals de.RecPayID
                            join s in Context1.SupplierMasters on r.SupplierID equals s.SupplierID
                            select new ReceiptVM
                            {
                                RecPayDate = r.RecPayDate,
                                DocumentNo = r.DocumentNo,
                                RecPayID = r.RecPayID,
                                PartyName = s.SupplierName,
                                Amount =r.FMoney
                            }).ToList();
            //Reciepts.ForEach(x => x.Amount = (from s in Context1.RecPayDetails where s.RecPayID == x.RecPayID where s.Amount > 0 select s).ToList().Sum(a => a.Amount));
            var data = (from t in Reciepts where (t.RecPayDate >= sdate && t.RecPayDate <= edate) select t).OrderByDescending(cc => cc.RecPayDate).ToList();
            var result = data.GroupBy(p => p.RecPayID).Select(grp => grp.FirstOrDefault());

            string view = this.RenderPartialView2("_GetAllTradeSupplierByDate", result);
            //string view = this.RenderPartialView("_Table", data);

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
        public ActionResult GetTradeReceiptsByDate(string fdate, string tdate, int FYearID)
        {
            DateTime d = DateTime.Now;
            DateTime fyear = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime mstart = new DateTime(fyear.Year, d.Month, 01);
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int maxday = DateTime.DaysInMonth(fyear.Year, d.Month);
            DateTime mend = new DateTime(fyear.Year, d.Month, maxday);
            var sdate = DateTime.Parse(fdate);
            var edate = DateTime.Parse(tdate);
            //ViewBag.AllCustomers = Context1.CustomerMasters.ToList();
            //var cust = ReceiptDAO.GetCustomerReceiptsByDate(fdate, tdate, FYearID).ToList();

            //var data = Context1.RecPays.Where(x => x.RecPayDate >= sdate && x.RecPayDate <= edate && x.CustomerID != null && x.FYearID == FYearID && x.IsTradingReceipt == true).OrderByDescending(x => x.RecPayDate).ToList();

            //data.ForEach(s => s.Remarks = (from x in Context1.RecPayDetails where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select x).FirstOrDefault() != null ? (from x in Context1.RecPayDetails join C in Context1.CurrencyMasters on x.CurrencyID equals C.CurrencyID where x.RecPayID == s.RecPayID && (x.CurrencyID != null || x.CurrencyID > 0) select C.CurrencyName).FirstOrDefault() : "");


            var Reciepts = (from r in Context1.RecPays
                            join de in Context1.RecPayDetails on r.RecPayID equals de.RecPayID
                            join s in Context1.SupplierMasters on r.SupplierID equals s.SupplierID
                            where r.AcCompanyID ==branchid
                            select new ReceiptVM
                            {
                                RecPayDate = r.RecPayDate,
                                DocumentNo = r.DocumentNo,
                                RecPayID = r.RecPayID,
                                PartyName = s.SupplierName
                            }).ToList();
            Reciepts.ForEach(x => x.Amount = (from s in Context1.RecPayDetails where s.RecPayID == x.RecPayID where s.Amount > 0 select s).ToList().Sum(a => a.Amount));
            var data = (from t in Reciepts where (t.RecPayDate >= sdate && t.RecPayDate <= edate) select t).ToList();
            var result = data.GroupBy(p => p.RecPayID).Select(grp => grp.FirstOrDefault());


            return PartialView("_GetAllTradeSupplierByDate", result);

        }
        [HttpGet]
        public ActionResult SupplierPaymentDetails()
        {
            //List<ReceiptVM> Reciepts = new List<ReceiptVM>();

            ////Reciepts = ReceiptDAO.GetCustomerReceipts(); // RP.GetAllReciepts();
            //Reciepts = (from r in Context1.RecPays
            //            //join d in Context1.RecPayDetails on r.RecPayID equals d.RecPayID
            //            join s in Context1.SupplierMasters on r.SupplierID equals s.SupplierID
            //            orderby r.RecPayDate descending
            //            select new ReceiptVM
            //            {
            //                RecPayDate = r.RecPayDate,
            //                DocumentNo = r.DocumentNo,
            //                RecPayID = r.RecPayID,
            //                PartyName = s.SupplierName,
            //                Amount =r.FMoney
            //            }).ToList();
            ////Reciepts.ForEach(d => d.Amount = (from s in Context1.RecPayDetails where s.RecPayID == d.RecPayID where s.Amount > 0 select s).ToList().Sum(a => a.Amount));
            //var data = (from t in Reciepts where (t.RecPayDate >= Convert.ToDateTime(Session["FyearFrom"]) && t.RecPayDate <= Convert.ToDateTime(Session["FyearTo"])) select t).ToList();
            //var result = data.GroupBy(p => p.RecPayID).Select(grp => grp.FirstOrDefault());

            //if (ID > 0)
            //{
            //    ViewBag.SuccessMsg = "You have successfully added Supplier Payment.";
            //}


            //if (ID == 10)
            //{
            //    ViewBag.SuccessMsg = "You have successfully deleted Supplier Payment.";
            //}

            //if (ID == 20)
            //{
            //    ViewBag.SuccessMsg = "You have successfully updated Supplier Payment.";
            //}


            //Session["ID"] = ID;


            return View();
        }
        [HttpGet]
        public ActionResult SupplierPayment(int id)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Supplier Payment - Modify";
                    cust = RP.GetSupplierRecPayByRecpayID(id);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.SupplierType = Context1.SupplierTypes.ToList();
                    cust.recPayDetail = Context1.RecPayDetails.Where(item => item.RecPayID == id).OrderBy(cc => cc.InvDate).ToList();
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    decimal Advance = 0;
                    Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), fyearid);
                    cust.Balance = Advance;
                    foreach (var item in cust.recPayDetail)
                    {
                        if (item.AcOPInvoiceDetailID > 0)
                        {
                            var sInvoiceDetail = (from d in Context1.AcOPInvoiceDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {
                                
                                var totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.AcOPInvoiceDetailID), Convert.ToInt32(cust.RecPayID),0, "OP");
                                
                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.InvoiceID = 0;
                                customerinvoice.AcOPInvoiceDetailID = sInvoiceDetail[0].AcOPInvoiceDetailID;
                                customerinvoice.InvoiceType = "OP";
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.AcOPInvoiceDetailID;
                                customerinvoice.SInvoiceNo = sInvoiceDetail[0].InvoiceNo;
                                customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBeRecieved = -1 * Convert.ToDecimal(item.Amount);// - Convert.ToDecimal(totamtpaid);// - Convert.ToDecimal(item.Amount);
                                customerinvoice.AmountToBePaid = totamtpaid; //already paid
                                customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1; //current allocation
                                customerinvoice.Balance = (customerinvoice.AmountToBeRecieved - Convert.ToDecimal(totamtpaid));// - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                        else if (item.InvoiceID > 0 && (item.AcOPInvoiceDetailID == 0 || item.AcOPInvoiceDetailID==null))
                        {
                            var sInvoiceDetail = (from d in Context1.SupplierInvoiceDetails where d.SupplierInvoiceID == item.InvoiceID select d).FirstOrDefault();
                            if (sInvoiceDetail != null)
                            {
                                var totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.InvoiceID), Convert.ToInt32(cust.RecPayID),0, "TR");
                                var Sinvoice = (from d in Context1.SupplierInvoices where d.SupplierInvoiceID == sInvoiceDetail.SupplierInvoiceID select d).FirstOrDefault();
                                
                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.AcOPInvoiceDetailID = 0;
                                customerinvoice.InvoiceType = "TR";
                                customerinvoice.InvoiceID = Convert.ToInt32(item.InvoiceID);
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                customerinvoice.SInvoiceNo = Sinvoice.InvoiceNo;
                                customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid);
                                customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                customerinvoice.Balance = Convert.ToDecimal(Sinvoice.InvoiceTotal) - totamtpaid;
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;
                                customerinvoice.AmountToBeRecieved = Convert.ToDecimal(Sinvoice.InvoiceTotal);
                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                        
                    }

                    BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "Supplier Payment - Create";
                    BindAllMasters(2);
                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.SupplierType = Context1.SupplierTypes.ToList();

                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                    cust.RecPayDate = System.DateTime.UtcNow;
                    List<CustomerRcieptChildVM> list = new List<CustomerRcieptChildVM>();
                    cust.CustomerRcieptChildVM = list;
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
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
            ViewBag.CustomerNotification = customernotification;
            return View(cust);

        }
        [HttpPost]
        public ActionResult SupplierPayment(CustomerRcieptVM RecP)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            int i = 0;

            int userid = Convert.ToInt32(Session["UserID"]);
            var StaffNotes = (from d in Context1.StaffNotes where d.RecPayID == RecP.RecPayID && d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var users = (from d in Context1.UserRegistrations select d).ToList();
            decimal TotalAmount = 0;
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

            if (RecP.CashBank != null)
            {
                RecP.StatusEntry = "CS";
                int acheadid = Convert.ToInt32(RecP.CashBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
            }
            else
            {
                RecP.StatusEntry = "BK";
                int acheadid = Convert.ToInt32(RecP.ChequeBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                RecP.BankName = achead;
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
                if (Fmoney > 0)
                {
                    RecP.AllocatedAmount = Fmoney;
                }
                else
                {

                }
                RecP.Balance = Convert.ToDecimal(RecP.FMoney) - Convert.ToDecimal(RecP.AllocatedAmount);

                RecP.AcCompanyID = branchid;
                RecP.UserID = userid;
                RecP.FYearID = fyearid;
                string DocNo = RP.GetMaxPaymentDocumentNo();
                RecP.DocumentNo = DocNo;
                RPID = ReceiptDAO.AddSupplierRecieptPayment(RecP, Session["UserID"].ToString()); //.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString());

                RecP.RecPayID = (from c in Context1.RecPays orderby c.RecPayID descending select c.RecPayID).FirstOrDefault();

                var recpitem = RecP.CustomerRcieptChildVM.Where(cc => cc.Amount > 0 || cc.AdjustmentAmount > 0).ToList();
                foreach (var item in recpitem)
                {
                    //decimal Advance = 0;                    
                    //Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "S";
                        if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        {
                            invoicetype = "SOP";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        
                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
                if (RecP.Balance > 0)
                {
                    int l = ReceiptDAO.InsertRecpayDetailsForSupplier(RecP.RecPayID, 0, 0, -1 * Convert.ToDecimal(RecP.Balance), null, "S", true, null, null, null, Convert.ToInt32(RecP.CurrencyId), 4, 0);

                }

                if (RecP.FMoney > 0)
                {
                    int l = ReceiptDAO.InsertRecpayDetailsForSupplier(RecP.RecPayID, 0, 0, Convert.ToDecimal(RecP.FMoney), null, "S", false, null, null, null, Convert.ToInt32(RecP.CurrencyId), 4, 0);

                }

                int fyaerId = Convert.ToInt32(Session["fyearid"].ToString());
                ReceiptDAO.InsertJournalOfSupplier(RecP.RecPayID, fyaerId);

                var Recpaydata = (from d in Context1.RecPays where d.RecPayID == RecP.RecPayID select d).FirstOrDefault();

                Recpaydata.RecPayID = RecP.RecPayID;
                Recpaydata.IsTradingReceipt = false;
                Context1.Entry(Recpaydata).State = EntityState.Modified;
                Context1.SaveChanges();

            }
            else //edit mode
            {
                decimal Fmoney = 0;
                for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                {
                    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                }

                RecP.AllocatedAmount = Fmoney;
                RecP.Balance = Convert.ToDecimal(RecP.FMoney) - Convert.ToDecimal(RecP.AllocatedAmount);
                RecPay recpay = new RecPay();
                recpay = Context1.RecPays.Find(RecP.RecPayID);
                recpay.RecPayDate = RecP.RecPayDate;
                recpay.RecPayID = RecP.RecPayID;
                recpay.AcJournalID = RecP.AcJournalID;
                recpay.BankName = RecP.BankName;
                recpay.StatusEntry = RecP.StatusEntry;
                recpay.ChequeDate = RecP.ChequeDate;
                recpay.ChequeNo = RecP.ChequeNo;
                recpay.SupplierID = RecP.SupplierID;
                recpay.DocumentNo = RecP.DocumentNo;
                recpay.EXRate = RecP.EXRate;
                if (recpay.FYearID==null || recpay.FYearID==0)
                    recpay.FYearID = fyearid;
                recpay.FMoney = RecP.FMoney;
                recpay.StatusEntry = RecP.StatusEntry;
                recpay.IsTradingReceipt = true;
                recpay.ModifiedBy = userid;
                recpay.ModifiedDate = CommonFunctions.GetBranchDateTime();
                recpay.Remarks = RecP.Remarks;             
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                var recpitem = RecP.CustomerRcieptChildVM.Where(cc => cc.Amount > 0 || cc.AdjustmentAmount > 0).ToList();
                foreach (var item in recpitem)
                {
                    //decimal Advance = 0;                    
                    //Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        string invoicetype = "S";
                        if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        {
                            invoicetype = "SOP";
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                        else
                        {
                            ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        }
                    }
                    TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
                int editrecPay = 0;

                var sumOfAmount = RecP.FMoney;
                editrecPay = editfu.EditRecpayDetailsCustR(RecP.RecPayID, Convert.ToInt32(sumOfAmount));
                //int editAcJdetails = editfu.EditAcJDetails(RecP.AcJournalID.Value, Convert.ToInt32(sumOfAmount));

                int fyaerId = Convert.ToInt32(Session["fyearid"].ToString());
                ReceiptDAO.InsertJournalOfSupplier(RecP.RecPayID, fyaerId);


            }


            BindAllMasters(2);
            return RedirectToAction("SupplierPaymentDetails", "SupplierPayment", new { ID = 0 });
        }


        [HttpPost]
        public JsonResult GetTradeInvoiceOfSupplier(int? ID, decimal? amountreceived, int? RecPayId, int SupplierTypeId = 1)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            decimal Advance = 0;
            var Allinvoices = new List<CustomerTradeReceiptVM>();
            Allinvoices = ReceiptDAO.SP_GetSupplierInvoicePending(Convert.ToInt32(ID), Convert.ToInt32(RecPayId), SupplierTypeId, "Payment");

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


          

            return Json(new { advance = Advance, salesinvoice = Allinvoices }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetSupplierName(string term, int SupplierTypeId)
        {
            if (term.Trim()=="")
            {
                var customerlist1 = (from c1 in Context1.SupplierMasters
                                    where c1.SupplierTypeID == SupplierTypeId
                                    orderby c1.SupplierName ascending
                                    select new { SupplierID = c1.SupplierID, SupplierName = c1.SupplierName }).ToList();

                return Json(customerlist1, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in Context1.SupplierMasters
                                    where c1.SupplierName.ToLower().Contains(term.ToLower()) && c1.SupplierTypeID == SupplierTypeId
                                    orderby c1.SupplierName ascending
                                    select new { SupplierID = c1.SupplierID, SupplierName = c1.SupplierName }).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
           

        }
        [HttpGet]
        public ActionResult Create(int id=0)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            CustomerRcieptVM cust = new CustomerRcieptVM();
            cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    cust = RP.GetSupplierRecPayByRecpayID(id);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.SupplierType = Context1.SupplierTypes.ToList();
                    cust.recPayDetail = Context1.RecPayDetails.Where(item => item.RecPayID == id).OrderBy(cc => cc.InvDate).ToList();
                    cust.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                    decimal Advance = 0;
                    //Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), fyearid);
                    cust.Balance = Advance;
                    foreach (var item in cust.recPayDetail)
                    {
                        if (item.AcOPInvoiceDetailID > 0)
                        {
                            var sInvoiceDetail = (from d in Context1.AcOPInvoiceDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
                            if (sInvoiceDetail != null)
                            {

                                var totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.AcOPInvoiceDetailID), Convert.ToInt32(cust.RecPayID), 0, "OP");

                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.InvoiceID = 0;
                                customerinvoice.AcOPInvoiceDetailID = sInvoiceDetail[0].AcOPInvoiceDetailID;
                                customerinvoice.InvoiceType = "OP";
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.AcOPInvoiceDetailID;
                                customerinvoice.InvoiceNo = sInvoiceDetail[0].InvoiceNo;
                                customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                if (Convert.ToDecimal(sInvoiceDetail[0].Amount)<0)
                                    customerinvoice.AmountToBeRecieved = -1 * Convert.ToDecimal(sInvoiceDetail[0].Amount);// - Convert.ToDecimal(totamtpaid);// - Convert.ToDecimal(item.Amount);
                                else
                                    customerinvoice.AmountToBeRecieved = Convert.ToDecimal(sInvoiceDetail[0].Amount);// - Convert.ToDecimal(totamtpaid);// - Convert.ToDecimal(item.Amount);

                                customerinvoice.AmountToBePaid = totamtpaid; //already paid
                                if (Convert.ToDecimal(item.Amount) < 0)
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1; //current allocation
                                else
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount);  //current allocation
                                customerinvoice.Balance = (customerinvoice.AmountToBeRecieved - Convert.ToDecimal(totamtpaid));// - Convert.ToDecimal(item.Amount); //  Convert.ToDecimal(sInvoiceDetail.NetValue - totamt);
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;

                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                        else if (item.InvoiceID > 0 && (item.AcOPInvoiceDetailID == 0 || item.AcOPInvoiceDetailID == null))
                        {
                            var sInvoiceDetail = (from d in Context1.SupplierInvoiceDetails where d.SupplierInvoiceID == item.InvoiceID select d).FirstOrDefault();
                            if (sInvoiceDetail != null)
                            {
                                var totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.InvoiceID), Convert.ToInt32(cust.RecPayID), 0, "TR");
                                var Sinvoice = (from d in Context1.SupplierInvoices where d.SupplierInvoiceID == sInvoiceDetail.SupplierInvoiceID select d).FirstOrDefault();

                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.AcOPInvoiceDetailID = 0;
                                customerinvoice.InvoiceType = "TR";
                                customerinvoice.InvoiceID = Convert.ToInt32(item.InvoiceID);
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                customerinvoice.InvoiceNo = Sinvoice.InvoiceNo;
                                customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid);
                                if (Convert.ToDecimal(item.Amount)<0)
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                else
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount);
                                customerinvoice.Balance = Convert.ToDecimal(Sinvoice.InvoiceTotal) - totamtpaid;
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;
                                customerinvoice.AmountToBeRecieved = Convert.ToDecimal(Sinvoice.InvoiceTotal);
                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }
                        else if (item.DebitNoteId > 0 && (item.AcOPInvoiceDetailID == 0 || item.AcOPInvoiceDetailID == null))
                        {
                            var sInvoiceDetail = (from d in Context1.DebitNotes where d.DebitNoteID == item.DebitNoteId select d).FirstOrDefault();
                            if (sInvoiceDetail != null)
                            {
                                var totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.DebitNoteId), Convert.ToInt32(cust.RecPayID),0, "SJ");
                                var Sinvoice = (from d in Context1.DebitNotes where d.DebitNoteID == sInvoiceDetail.DebitNoteID select d).FirstOrDefault();

                                var customerinvoice = new CustomerRcieptChildVM();
                                customerinvoice.AcOPInvoiceDetailID = 0;
                                customerinvoice.InvoiceType = "SJ";
                                customerinvoice.InvoiceID = Convert.ToInt32(item.DebitNoteId);
                                customerinvoice.JobCode = customerinvoice.InvoiceType + customerinvoice.InvoiceID;
                                customerinvoice.InvoiceNo = Sinvoice.DebitNoteNo;
                                customerinvoice.InvoiceDate = Convert.ToDateTime(item.InvDate);
                                customerinvoice.strDate = Convert.ToDateTime(item.InvDate).ToString("dd/MM/yyyy");
                                customerinvoice.AmountToBePaid = Convert.ToDecimal(totamtpaid);
                                if (Convert.ToDecimal(item.Amount) < 0)
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount) * -1;
                                else
                                    customerinvoice.Amount = Convert.ToDecimal(item.Amount);
                                customerinvoice.Balance = Convert.ToDecimal(Sinvoice.Amount) - totamtpaid;
                                customerinvoice.RecPayDetailID = item.RecPayDetailID;
                                customerinvoice.AmountToBeRecieved = Convert.ToDecimal(Sinvoice.Amount);
                                customerinvoice.RecPayID = Convert.ToInt32(item.RecPayID);
                                customerinvoice.AdjustmentAmount = Convert.ToDecimal(item.AdjustmentAmount);
                                cust.CustomerRcieptChildVM.Add(customerinvoice);
                            }
                        }

                    }

                    BindMasters_ForEdit(cust);

                    StatusModel result = AccountsDAO.CheckDateValidate(cust.RecPayDate.ToString(), fyearid);
                    if (result.Status == "YearClose") //Period locked
                    {

                        ViewBag.Message = result.Message;
                        ViewBag.SaveEnable = false;
                    }
                }
                else
                {
                    ViewBag.Title = "Create";
                    BindAllMasters(2);
                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Cash") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1.Contains("Bank") select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                   ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    ViewBag.SupplierType = Context1.SupplierTypes.ToList();

                    cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                    cust.RecPayDate = System.DateTime.UtcNow;
                    List<CustomerRcieptChildVM> list = new List<CustomerRcieptChildVM>();
                    cust.CustomerRcieptChildVM = list;
                    StatusModel result = AccountsDAO.CheckDateValidate(cust.RecPayDate.ToString(), fyearid);
                    if (result.Status == "YearClose") //Period locked
                    {

                        ViewBag.Message = result.Message;
                        ViewBag.SaveEnable = false;
                    }
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
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
            ViewBag.CustomerNotification = customernotification;
            return View(cust);

        }
        [HttpPost]
        public ActionResult Create(CustomerRcieptVM RecP)
        {
            int RPID = 0;
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            int i = 0;

            int userid = Convert.ToInt32(Session["UserID"]);
            var StaffNotes = (from d in Context1.StaffNotes where d.RecPayID == RecP.RecPayID && d.PageTypeId == 2 orderby d.NotesId descending select d).ToList();
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var users = (from d in Context1.UserRegistrations select d).ToList();
            decimal TotalAmount = 0;
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
                if (Fmoney > 0)
                {
                    RecP.AllocatedAmount = Fmoney;
                }
                else
                {

                }
                RecP.Balance = Convert.ToDecimal(RecP.FMoney) - Convert.ToDecimal(RecP.AllocatedAmount);
                string DocNo = RP.GetMaxPaymentDocumentNo();
                RecP.DocumentNo = DocNo;
                RecP.AcCompanyID = branchid;
                RecP.UserID = userid;
                RecP.FYearID = fyearid;
                RecP.StatusOrigin = "S";
                RPID = ReceiptDAO.AddSupplierRecieptPayment(RecP, Session["UserID"].ToString()); //.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString());

                RecP.RecPayID = (from c in Context1.RecPays orderby c.RecPayID descending select c.RecPayID).FirstOrDefault();

                var recpitem = RecP.CustomerRcieptChildVM.Where(cc => cc.Amount > 0 || cc.AdjustmentAmount > 0).ToList();


              
                foreach (var item in recpitem)
                {
                    //decimal Advance = 0;                    
                    //Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        RecPayDetail _detail = new RecPayDetail();
                        _detail.RecPayDetailID = maxrecpaydetailid + 1;
                        _detail.RecPayID = RecP.RecPayID;
                        _detail.StatusInvoice = "S";
                        if (item.InvoiceType == "OP")
                        {
                            if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                            {
                                _detail.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
                            }
                            else if (item.InvoiceID > 0)
                            {
                                _detail.AcOPInvoiceDetailID = item.InvoiceID;
                            }
                        }
                        else if (item.InvoiceType == "SJ")
                        {
                            _detail.DebitNoteId = item.InvoiceID;
                        }
                        else
                        {
                            _detail.InvoiceID = item.InvoiceID;
                        }
                        _detail.Amount = item.Amount;
                        _detail.InvNo = item.InvoiceNo;
                        _detail.InvDate = vInvoiceDate1;
                        _detail.AdjustmentAmount = item.AdjustmentAmount;

                        _detail.CurrencyID = Convert.ToInt32(Session["CurrencyId"].ToString());

                        _detail.Remarks = item.Remarks;
                        Context1.RecPayDetails.Add(_detail);
                        Context1.SaveChanges();

                        //string invoicetype = "S";
                        //if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        //{
                        //    invoicetype = "SOP";
                        //    ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        //}
                        //else
                        //{
                        //    ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        //}
                    }
                    // TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
              
                if (RecP.StatusEntry != "ST")
                    ReceiptDAO.InsertJournalOfSupplier(RecP.RecPayID, fyearid);

                 

            }
            else //edit mode
            {
                decimal Fmoney = 0;
                for (int j = 0; j < RecP.CustomerRcieptChildVM.Count; j++)
                {
                    Fmoney = Fmoney + Convert.ToDecimal(RecP.CustomerRcieptChildVM[j].Amount);
                }

                RecP.AllocatedAmount = Fmoney;
                RecP.Balance = Convert.ToDecimal(RecP.FMoney) - Convert.ToDecimal(RecP.AllocatedAmount);
                RecPay recpay = new RecPay();
                recpay = Context1.RecPays.Find(RecP.RecPayID);
                recpay.RecPayDate = RecP.RecPayDate;
                recpay.RecPayID = RecP.RecPayID;
                recpay.AcJournalID = RecP.AcJournalID;
                recpay.BankName = RecP.BankName;
                recpay.StatusOrigin = "S";
                
                recpay.StatusEntry = RecP.StatusEntry;
                recpay.ChequeDate = RecP.ChequeDate;
                recpay.ChequeNo = RecP.ChequeNo;
                recpay.SupplierID = RecP.SupplierID;
                recpay.DocumentNo = RecP.DocumentNo;
                recpay.EXRate = RecP.EXRate;
                
                //if (recpay.FYearID == null || recpay.FYearID == 0)
                //    recpay.FYearID = fyearid;
                if (recpay.StatusOrigin ==null)
                    RecP.StatusOrigin="S";

                recpay.FMoney = RecP.FMoney;
                
                recpay.IsTradingReceipt = true;
                recpay.ModifiedBy = userid;
                recpay.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                recpay.Remarks = RecP.Remarks;
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                //deleting old entries
                var details = (from d in Context1.RecPayDetails where d.RecPayID == RecP.RecPayID select d).ToList();
                Context1.RecPayDetails.RemoveRange(details);
                Context1.SaveChanges();

                var recpitem = RecP.CustomerRcieptChildVM.Where(cc => cc.Amount > 0 || cc.AdjustmentAmount > 0).ToList();
                foreach (var item in recpitem)
                {
                    //decimal Advance = 0;                    
                    //Advance = Convert.ToDecimal(item.Amount) - item.AmountToBeRecieved;
                    DateTime vInvoiceDate = Convert.ToDateTime(item.InvoiceDate);
                    string vInvoiceDate1 = Convert.ToDateTime(vInvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                    if (item.Amount > 0 || item.AdjustmentAmount > 0)
                    {
                        var maxrecpaydetailid = (from c in Context1.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        RecPayDetail _detail = new RecPayDetail();
                        _detail.RecPayDetailID = maxrecpaydetailid+1;
                        _detail.RecPayID = RecP.RecPayID;
                        _detail.StatusInvoice = "S";
                        if (item.InvoiceType == "OP")
                        {
                            if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                            {
                                _detail.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
                            }
                            else if (item.InvoiceID > 0)
                            {
                                _detail.AcOPInvoiceDetailID = item.InvoiceID;
                            }
                        }
                        else if (item.InvoiceType == "SJ")
                        {
                            if (item.InvoiceID > 0)
                            {
                                _detail.DebitNoteId = item.InvoiceID;
                            }
                            else
                            {
                                _detail.DebitNoteId = item.InvoiceID;
                            }
                            
                        }
                        else
                        {
                            _detail.InvoiceID = item.InvoiceID;
                        }
                        _detail.Amount = item.Amount;
                        _detail.InvNo = item.InvoiceNo;
                        _detail.InvDate = vInvoiceDate1;
                        _detail.AdjustmentAmount = item.AdjustmentAmount;
                        
                        _detail.CurrencyID = Convert.ToInt32(Session["CurrencyId"].ToString());


                        Context1.RecPayDetails.Add(_detail);
                        Context1.SaveChanges();

                        //string invoicetype = "S";
                        //if (item.AcOPInvoiceDetailID != 0 && item.InvoiceID == 0)
                        //{
                        //    invoicetype = "SOP";
                        //    ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.AcOPInvoiceDetailID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        //}
                        //else
                        //{
                        //    ReceiptDAO.InsertRecpayDetailsForCust(RecP.RecPayID, item.InvoiceID, item.InvoiceID, Convert.ToDecimal(-item.Amount), "", invoicetype, false, "", vInvoiceDate1, item.InvoiceNo.ToString(), Convert.ToInt32(RecP.CurrencyId), 3, item.AdjustmentAmount);
                        //}
                    }
                   // TotalAmount = TotalAmount + Convert.ToDecimal(item.Amount);
                }
           
                if (RecP.StatusEntry != "ST")
                {
                    ReceiptDAO.InsertJournalOfSupplier(RecP.RecPayID, fyearid);
                }


            }


            BindAllMasters(2);
            return RedirectToAction("Index", "SupplierPayment", new { ID = 0 });
        }
        
        [HttpGet]
        public JsonResult GetOpeningPayment(string term, int SupplierId)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            List<OpeningInvoiceVM> list = new List<OpeningInvoiceVM>();
 
            list = ReceiptDAO.GetSupplierOpeningPayments(SupplierId, branchid, FyearId);

            Session["OPSupplierPayment"] = list;
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
            System.IO.StreamReader objReader;
            objReader = new System.IO.StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("/Templates/CustomerNotification.html"));
            string content = objReader.ReadToEnd();


            objReader.Close();
            content = Regex.Replace(content, "@username", UserName);
            content = Regex.Replace(content, "@Message", Message);
            try
            {
                using (MailMessage msgMail = new MailMessage())
                {

                    msgMail.From = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"].ToString());
                    msgMail.Subject = "Shipping System";
                    msgMail.IsBodyHtml = true;
                    msgMail.Body = HttpUtility.HtmlDecode(content);
                    msgMail.To.Add(Email);
                    msgMail.IsBodyHtml = true;

                    //client = new SmtpClient(ConfigurationManager.AppSettings["Host"].ToString());
                    //client.Port = int.Parse(ConfigurationManager.AppSettings["SMTPServerPort"].ToString());
                    //client.UseDefaultCredentials = false;
                    //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUserName"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString());
                    //client.EnableSsl = true;
                    //client.Send(msgMail);
                    using (SmtpClient smtp = new SmtpClient("smtp.mail.yahoo.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUserName"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString());
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
