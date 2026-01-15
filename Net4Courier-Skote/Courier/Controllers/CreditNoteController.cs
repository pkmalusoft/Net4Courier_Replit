using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using Newtonsoft.Json;
using System.Data;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class CreditNoteController : Controller
    {

        SourceMastersModel objSourceMastersModel = new SourceMastersModel();

        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            CreditNoteSearch obj = (CreditNoteSearch)Session["CreditNoteSearch"];
            CreditNoteSearch model = new CreditNoteSearch();
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                obj = new CreditNoteSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.InvoiceNo = "";
                Session["CreditNoteSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.InvoiceNo = "";
            }
            else
            {
                model = obj;
                                
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
            }
            Session["CreditNoteSearch"] = model;
            List<CreditNoteVM> lst = CreditNoteDAO.CreditNoteList(yearid, branchid, model.FromDate.Date, model.ToDate,model.CreditNoteNo,model.InvoiceNo );
            model.Details = lst;

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(CreditNoteSearch obj)
        {
            Session["CreditNoteSearch"] = obj;
            return RedirectToAction("Index");
        }

        public CreditNoteVM GetCreditNoteHead(string CustomerType="CR")
        {
            CreditNoteVM vm = new CreditNoteVM();
            AccountSetup acsetup = new AccountSetup();
            if (CustomerType=="CR")
            acsetup = db.AccountSetups.Where(cc => cc.PageName == "Customer Invoice" && cc.TransType=="All").FirstOrDefault();
            else
                acsetup = db.AccountSetups.Where(cc => cc.PageName == "CoLoader Invoice" && cc.TransType=="All").FirstOrDefault();

            if (acsetup != null)
            {
                if (acsetup.CreditAccountId != null)
                {
                    vm.AcHeadID = Convert.ToInt32(acsetup.DebitAccountId);
                    var head = db.AcHeads.Find(acsetup.DebitAccountId);
                    if (head != null)
                    {
                        vm.AcHeadName = head.AcHead1;
                        vm.HDebitAmount = 0;
                        vm.HCreditAmount = 0;
                    }
                    else
                    {
                        vm.AcHeadName = "";
                        vm.AcHeadID = 0;
                    }
                }
                else
                {
                    vm.AcHeadID = 0;
                    vm.AcHeadName = "";
                }
            }
            else
            {
                vm.AcHeadID = 0; //Customer control account
                vm.AcHeadName = "";
            }
            return vm;
        }
        public ActionResult Create(int id = 0)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            //ViewBag.customer = db.CustomerMasters.Where(d => d.CustomerType == "CR").OrderBy(x => x.CustomerName).ToList();
            ViewBag.achead = db.AcHeads.OrderBy(cc => cc.AcHead1).ToList();

            if (id == 0)
            {

                CreditNoteVM vm = new CreditNoteVM();
                vm = GetCreditNoteHead();
                vm.TransType = "CN";
                vm.CreditNoteNo = AccountsDAO.GetMaxCreditNoteNo(fyearid,vm.TransType);
                vm.CustomerType = "CR";
                vm.Date = CommonFunctions.GetLastDayofMonth().Date;
              
                vm.ReferenceType = "0";

                vm.AmountType = "0";
                vm.AcDetailAmountType = "1";
                List<CreditNoteDetailVM> list = new List<CreditNoteDetailVM>();
                vm.Details = list;
                vm.CustomerRcieptChildVM = new List<CustomerRcieptChildVM>();
                Session["CreditNoteDetail"] = list;
                ViewBag.Title = "Create";
                ViewBag.Message = "";
                ViewBag.SaveEnable = true;

                return View(vm);
            }
            else
            {
                ViewBag.Title = "Modify";
                CreditNoteVM vm = new CreditNoteVM();
                vm = GetCreditNoteHead();
                var v = db.CreditNotes.Find(id);
                vm.CreditNoteID = v.CreditNoteID;
                vm.CustomerType = v.CustomerType;
                vm.CreditNoteNo = v.CreditNoteNo;
                vm.Date = Convert.ToDateTime(v.CreditNoteDate);
                vm.AcJournalID = Convert.ToInt32(v.AcJournalID);
                vm.CustomerID = Convert.ToInt32(v.CustomerID);
                var customer = db.CustomerMasters.Find(v.CustomerID).CustomerName;
                if (customer != null)
                    vm.CustomerName = customer;
                vm.AcHeadID = Convert.ToInt32(v.AcHeadID);
                vm.Amount = Convert.ToDecimal(v.Amount);

                if (v.TransType == "CN")
                    vm.HCreditAmount = vm.Amount;
                else if (v.TransType == "CJ")
                    vm.HDebitAmount = vm.Amount;

                vm.Description = v.Description;
                vm.TransType = v.TransType;
                if (v.RecPayID != null && v.RecPayID != 0)
                    vm.RecPayID = Convert.ToInt32(v.RecPayID);
                else
                    vm.RecPayID = 0;

                var detaillist = (from c in db.CreditNoteDetails join d in db.AcHeads on c.AcHeadID equals d.AcHeadID where c.CreditNoteID == v.CreditNoteID select new CreditNoteDetailVM { AcHeadID = c.AcHeadID, AcHeadName = d.AcHead1, Amount = c.Amount, Remarks = c.Remarks }).ToList();
                vm.Details = detaillist;

                for (var i = 0; i < vm.Details.Count; i++)
                {
                    if (v.TransType == "CN")
                        vm.Details[i].DebitAmount = Convert.ToDecimal(detaillist[i].Amount);
                    else
                        vm.Details[i].CreditAmount = Convert.ToDecimal(detaillist[i].Amount);
                }
                StatusModel result = AccountsDAO.CheckDateValidate(vm.Date.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
                Session["CreditNoteDetail"] = detaillist;
                               

                
                return View(vm);

            }


        }

       
        [HttpPost]
        public JsonResult SaveCreditNote(CreditNoteVM v, string Details,string Allocation)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AcJournalMaster ajm = new AcJournalMaster();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            try
            {
                var IDetails = JsonConvert.DeserializeObject<List<CreditNoteDetailVM>>(Details);
                v.Details = IDetails;
                var IDetails1 = JsonConvert.DeserializeObject<List<CustomerRcieptChildVM>>(Allocation);
                v.CustomerRcieptChildVM = IDetails1;
                StatusModel result = AccountsDAO.CheckDateValidate(v.Date.ToString(), fyearid);
                if (result.Status == "PeriodLock") //Period locked
                {
                    return Json(new { status = "Failed", CreditNoteId = 0, message = result.Message });
                }
                else if (result.Status == "Failed")
                {
                    return Json(new { status = "Failed", CreditNoteId = 0, message = result.Message });
                }
                else
                {

                }
                CreditNote d = new CreditNote();
                if (v.CreditNoteID == 0)
                {
                    int maxid = 0;

                    var data = (from c in db.CreditNotes orderby c.CreditNoteID descending select c).FirstOrDefault();

                    if (data == null)
                    {
                        maxid = 1;
                    }
                    else
                    {
                        maxid = data.CreditNoteID + 1;
                    }

                    d.CreditNoteID = maxid;
                    d.CreditNoteNo = AccountsDAO.GetMaxCreditNoteNo(fyearid, v.TransType);
                   
                    d.FYearID = Convert.ToInt32(Session["fyearid"].ToString());
                    d.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                    d.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    d.statusclose = false;

                    d.CreatedBy = userid;
                    d.CreatedDate = CommonFunctions.GetBranchDateTime();
                    d.ModifiedBy = userid;
                    d.ModifiedDate = CommonFunctions.GetBranchDateTime();
                }
                else
                {
                    d = db.CreditNotes.Find(v.CreditNoteID);
                    var det = db.CreditNoteDetails.Where(cc => cc.CreditNoteID == v.CreditNoteID).ToList();
                    if (det != null)
                    {
                        db.CreditNoteDetails.RemoveRange(det);
                        db.SaveChanges();
                    }

                    d.ModifiedBy = userid;
                    d.ModifiedDate = CommonFunctions.GetBranchDateTime();
                }
                d.CustomerType = v.CustomerType;
                d.InvoiceType = v.InvoiceType;
                d.CreditNoteDate = v.Date;
                if (v.TransType == "CN")
                    d.Amount = v.Amount;
                else
                    d.Amount = v.Amount;

                //d.AcJournalID = d.AcJournalID;

                d.AcHeadID = v.AcHeadID;
                d.CustomerID = v.CustomerID;
                d.Description = v.Description;
                d.TransType = v.TransType;
                if (v.ReferenceType == "1")
                {
                     
                        d.InvoiceID = v.InvoiceID;
                     
                         
                }
                else if (v.ReferenceType == "2")
                {
                     
                        d.RecPayID = v.InvoiceID;
                }
                else //driect
                {
                    d.InvoiceID = 0;
                    d.RecPayID = 0;
                }
                //d.IsShipping = true;
                if (v.CreditNoteID == 0)
                {
                    db.CreditNotes.Add(d);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    var cdetail = db.CreditNoteDetails.Where(cc => cc.CreditNoteID == d.CreditNoteID).ToList();
                    db.CreditNoteDetails.RemoveRange(cdetail);
                    db.SaveChanges();


                    var allocationdetail = db.RecPayDetails.Where(cc => cc.ForCreditNoteID == d.CreditNoteID).ToList();
                    db.RecPayDetails.RemoveRange(allocationdetail);
                    db.SaveChanges();
                }

                foreach (var detail in v.Details)
                {
                    CreditNoteDetail det = new CreditNoteDetail();
                    det.AcHeadID = detail.AcHeadID;
                    det.Amount = detail.Amount;
                    det.Remarks = detail.Remarks;
                    det.CreditNoteID = d.CreditNoteID;
                    db.CreditNoteDetails.Add(det);
                    db.SaveChanges();
                }
                if (v.CustomerRcieptChildVM != null)
                {
                    foreach (var detail in v.CustomerRcieptChildVM)
                    {
                        RecPayDetail det = new RecPayDetail();
                        var maxrecpaydetailid = (from c in db.RecPayDetails orderby c.RecPayDetailID descending select c.RecPayDetailID).FirstOrDefault();
                        maxrecpaydetailid = maxrecpaydetailid + 1;
                        det.RecPayDetailID = maxrecpaydetailid;
                        det.Amount = detail.Amount;
                        if (detail.InvoiceType == "OP" && detail.AcOPInvoiceDetailID > 0)
                            det.AcOPInvoiceDetailID = detail.AcOPInvoiceDetailID;
                        else if (detail.InvoiceType == "OP" && detail.InvoiceID > 0)
                            det.AcOPInvoiceDetailID = detail.InvoiceID;
                        else if (detail.InvoiceType == "TR" && detail.InvoiceID > 0)
                            det.InvoiceID = detail.InvoiceID;
                        string vInvoiceDate1 = Convert.ToDateTime(detail.InvoiceDate).ToString("yyyy-MM-dd h:mm tt");
                        det.InvDate = vInvoiceDate1;
                        det.InvNo = detail.InvoiceNo;
                        det.ForCreditNoteID = d.CreditNoteID;
                        db.RecPayDetails.Add(det);
                        db.SaveChanges();
                    }
                }
                CreditNoteDAO.InsertJournalOfCreditNote(d.CreditNoteID, fyearid);

                if (v.CreditNoteID == 0)
                {
                    return Json(new { status = "OK", CreditNoteID = d.CreditNoteID, message = "Credit Note Added Succesfully!" });
                }
                else
                {
                    return Json(new { status = "OK", CreditNoteID = d.CreditNoteID, message = "Credit Note Update Succesfully!" });
                }


            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", CreditNoteID = v.CreditNoteID, message = ex.Message });
            }





        }


        public ActionResult CJIndex(CreditNoteSearch obj)
        {
            DateTime pFromDate;
            DateTime pToDate;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            if ((obj.FromDate == null || obj.ToDate == null) || obj.FromDate.ToString().Contains("0001"))
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;//  DateTimeOffset.Now.Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date; // ToDate = DateTime.Now;
            }
            else 
            {
                pFromDate = Convert.ToDateTime(obj.FromDate);//.AddDays(-1);
                pToDate = Convert.ToDateTime(obj.ToDate);

            }
            obj.FromDate = pFromDate;
            obj.ToDate = pToDate;

            List<CreditNoteVM> lst = CreditNoteDAO.CustomerJVList(fyearid, branchid, pFromDate.Date, pToDate.Date, obj.CreditNoteNo, obj.InvoiceNo);
            obj.Details = lst;

            return View(obj);
        }

        public ActionResult CJCreate(int id = 0)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            //ViewBag.customer = db.CustomerMasters.Where(d => d.CustomerType == "CR").OrderBy(x => x.CustomerName).ToList();
            ViewBag.achead = db.AcHeads.OrderBy(cc => cc.AcHead1).ToList();

            if (id == 0)
            {

                CreditNoteVM vm = new CreditNoteVM();
                vm = GetCreditNoteHead();
                vm.TransType = "CJ";
                vm.CreditNoteNo = AccountsDAO.GetMaxCreditNoteNo(fyearid, vm.TransType);

                vm.Date = CommonFunctions.GetLastDayofMonth().Date;

                vm.ReferenceType = "0";

                vm.AmountType = "0";
                vm.AcDetailAmountType = "1";
                List<CreditNoteDetailVM> list = new List<CreditNoteDetailVM>();
                vm.Details = list;

                Session["CreditNoteDetail"] = list;
                ViewBag.Title = "Create";
                return View(vm);
            }
            else
            {
                ViewBag.Title = "Modify";
                CreditNoteVM vm = new CreditNoteVM();
                vm = GetCreditNoteHead();
                var v = db.CreditNotes.Find(id);
                vm.CreditNoteID = v.CreditNoteID;
                vm.CreditNoteNo = v.CreditNoteNo;
                vm.Date = Convert.ToDateTime(v.CreditNoteDate);
                vm.AcJournalID = Convert.ToInt32(v.AcJournalID);
                vm.CustomerID = Convert.ToInt32(v.CustomerID);
                var customer = db.CustomerMasters.Find(v.CustomerID).CustomerName;
                if (customer != null)
                    vm.CustomerName = customer;
                vm.AcHeadID = Convert.ToInt32(v.AcHeadID);
                vm.Amount = Convert.ToDecimal(v.Amount);

                if (v.TransType == "CN")
                    vm.HCreditAmount = vm.Amount;
                else if (v.TransType == "CJ")
                    vm.HDebitAmount = vm.Amount;

                vm.Description = v.Description;
                vm.TransType = v.TransType;
                if (v.RecPayID != null && v.RecPayID != 0)
                    vm.RecPayID = Convert.ToInt32(v.RecPayID);
                else
                    vm.RecPayID = 0;

                var detaillist = (from c in db.CreditNoteDetails join d in db.AcHeads on c.AcHeadID equals d.AcHeadID where c.CreditNoteID == v.CreditNoteID select new CreditNoteDetailVM { AcHeadID = c.AcHeadID, AcHeadName = d.AcHead1, Amount = c.Amount, Remarks = c.Remarks }).ToList();
                vm.Details = detaillist;

                for (var i = 0; i < vm.Details.Count; i++)
                {
                    if (v.TransType == "CN")
                        vm.Details[i].DebitAmount = Convert.ToDecimal(detaillist[i].Amount);
                    else
                        vm.Details[i].CreditAmount = Convert.ToDecimal(detaillist[i].Amount);
                }

                Session["CreditNoteDetail"] = detaillist;

                if (v.InvoiceID != null && v.InvoiceID != 0 && v.TransType == "CN")
                {
                    vm.InvoiceID = Convert.ToInt32(v.InvoiceID);
                    vm.InvoiceType = v.InvoiceType;
                    vm.ForInvoice = true;
                    vm.ReferenceType = "1";

                    SetTradeInvoiceOfCustomer(vm.CustomerID, 0, vm.CreditNoteID, vm.TransType);
                    List<CustomerTradeReceiptVM> lst = (List<CustomerTradeReceiptVM>)Session["CustomerInvoice"];

                    if (v.InvoiceType == "TR")
                    {
                        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "TR").FirstOrDefault();

                        if (invoice != null)
                        {
                            vm.InvoiceNo = invoice.InvoiceNo;
                            vm.InvoiceDate = invoice.DateTime;
                            vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                            vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                        }
                    }
                    else if (v.InvoiceType == "OP")
                    {
                        //var invoice1 = db.AcOPInvoiceDetails.Where(cc=>cc.AcOPInvoiceDetailID ==vm.InvoiceID).FirstOrDefault();
                        //if (invoice1 != null)
                        //{
                        //    vm.InvoiceNo = invoice1.InvoiceNo;
                        //    vm.InvoiceDate = Convert.ToDateTime(invoice1.InvoiceDate).ToString("dd/MM/yyyy");
                        //    vm.InvoiceAmount = Convert.ToDecimal(invoice1.Amount);
                        //}
                        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();
                        vm.InvoiceNo = invoice.InvoiceNo;
                        vm.InvoiceDate = invoice.DateTime;
                        vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                        vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                    }
                }
                else if (v.InvoiceID != null && v.InvoiceID != 0 && v.TransType == "CJ")
                {
                    vm.InvoiceID = Convert.ToInt32(v.InvoiceID);
                    vm.InvoiceType = v.InvoiceType;
                    vm.ForInvoice = true;
                    vm.ReferenceType = "1";

                    SetTradeInvoiceOfCustomer(vm.CustomerID, 0, vm.CreditNoteID, "CN"); //need invoice pending
                    List<CustomerTradeReceiptVM> lst = (List<CustomerTradeReceiptVM>)Session["CustomerInvoice"];

                    if (v.InvoiceType == "TR")
                    {
                        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "TR").FirstOrDefault();

                        if (invoice != null)
                        {
                            vm.InvoiceNo = invoice.InvoiceNo;
                            vm.InvoiceDate = invoice.DateTime;
                            vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                            vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                        }
                    }
                    else if (v.InvoiceType == "OP")
                    {
                        var invoice1 = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();

                        if (invoice1 != null)
                        {
                            vm.InvoiceNo = invoice1.InvoiceNo;
                            vm.InvoiceDate = invoice1.DateTime;
                            vm.InvoiceAmount = Convert.ToDecimal(invoice1.Amount);
                            vm.ReceivedAmount = Convert.ToDecimal(invoice1.Balance);
                        }
                        //var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();
                        //vm.InvoiceNo = invoice.InvoiceNo;
                        //vm.InvoiceDate = invoice.DateTime;
                        //vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                        //vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                    }
                }
                else if (v.RecPayID != null && v.RecPayID != 0 && v.TransType == "CJ")
                {
                    vm.InvoiceID = Convert.ToInt32(v.RecPayID);
                    vm.InvoiceType = v.InvoiceType;
                    vm.ForInvoice = true;
                    vm.ReferenceType = "2";
                    SetTradeInvoiceOfCustomer(vm.CustomerID, 0, vm.CreditNoteID, vm.TransType);
                    List<CustomerTradeReceiptVM> lst = (List<CustomerTradeReceiptVM>)Session["CustomerInvoice"];

                    if (v.InvoiceType == "TR")
                    {
                        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "TR").FirstOrDefault();
                        if (invoice != null)
                        {
                            vm.InvoiceNo = invoice.InvoiceNo;
                            vm.InvoiceDate = invoice.DateTime;
                            vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                            vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                        }
                    }
                    else if (v.InvoiceType == "OP")
                    {
                        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();
                        vm.InvoiceNo = invoice.InvoiceNo;
                        vm.InvoiceDate = invoice.DateTime;
                        vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                        vm.ReceivedAmount = Convert.ToDecimal(invoice.Balance);
                    }
                }
                else
                {
                    vm.ForInvoice = false;
                    vm.ReferenceType = "0";
                }


                vm.Date = Convert.ToDateTime(v.CreditNoteDate);
                //vm.
                return View(vm);

            }


        }

        [HttpPost]
        public JsonResult GetTradeInvoiceOfCustomer(int? ID, decimal? amountreceived, int? CreditNoteId, string TransType = "CN",string RefType="NA")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            //var AllInvoices = (from d in db.CustomerInvoices where d.CustomerID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            salesinvoice = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0, Convert.ToInt32(CreditNoteId), Convert.ToInt32(CreditNoteId), "B", "CR");

            //if (TransType == "CN"  && (RefType=="1" || RefType=="IN")) //Invoiceid
            //    salesinvoice = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(CreditNoteId), "B","CR");
            //if (TransType == "CJ" && (RefType == "1" || RefType == "IN")) //Invoiceid
            //    salesinvoice = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(CreditNoteId), "A", "CR");
            //else if (TransType == "CJ" && (RefType == "2" || RefType == "RE")) //Receipt
            //    salesinvoice = ReceiptDAO.SP_GetCustomerReceiptPending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(CreditNoteId), "OP");


            

            Session["CustomerInvoice"] = salesinvoice;
            return Json(new { advance = 0, salesinvoice = salesinvoice }, JsonRequestBehavior.AllowGet);
            //return Json(salesinvoice, JsonRequestBehavior.AllowGet);
        }


        public void SetTradeInvoiceOfCustomer(int? ID, decimal? amountreceived, int? CreditNoteId, string TransType)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            //DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            //DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            //var AllInvoices = (from d in db.CustomerInvoices where d.CustomerID == ID select d).ToList();
            List<ReceiptAllocationDetailVM> AWBAllocation = new List<ReceiptAllocationDetailVM>();
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            if (TransType == "CN")
                salesinvoice = ReceiptDAO.SP_GetCustomerInvoicePending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(CreditNoteId), "OP","CR");
            else
                salesinvoice = ReceiptDAO.SP_GetCustomerReceiptPending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(CreditNoteId), "OP");


            //var AllOPInvoices = (from d in db.AcOPInvoiceDetails join m in db.AcOPInvoiceMasters on d.AcOPInvoiceMasterID equals m.AcOPInvoiceMasterID where m.AcFinancialYearID == fyearid && m.StatusSDSC == "C" && m.PartyID == ID && d.RecPayDetailId == null && (d.RecPayStatus == null || d.RecPayStatus < 2) select d).ToList();

            //foreach (var item in AllOPInvoices)
            //{

            //    decimal? totamtpaid = 0;
            //    decimal? totadjust = 0;

            //    totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(ID), Convert.ToInt32(item.AcOPInvoiceDetailID), 0, Convert.ToInt32(CreditNoteId), "OP");

            //    var Invoice = new CustomerTradeReceiptVM();
            //    Invoice.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
            //    Invoice.SalesInvoiceID = item.AcOPInvoiceDetailID;
            //    Invoice.InvoiceType = "OP";
            //    Invoice.InvoiceNo = item.InvoiceNo; ;
            //    Invoice.InvoiceAmount = item.Amount;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = Convert.ToDateTime(item.InvoiceDate).ToString("dd/MM/yyyy");
            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = totadjust;

            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
            //                Invoice.Allocated = true;
            //                Invoice.Amount = Invoice.Balance;
            //                amountreceived = amountreceived - Invoice.Amount;
            //            }
            //            else if (amountreceived > 0)
            //            {
            //                Invoice.Amount = amountreceived;
            //                amountreceived = amountreceived - Invoice.Amount;
            //            }
            //            else
            //            {
            //                Invoice.Amount = 0;
            //            }
            //        }
            //        salesinvoice.Add(Invoice);
            //    }

            //}
            //foreach (var item in AllInvoices)
            //{
            //    //var invoicedeails = (from d in db.SalesInvoiceDetails where d.SalesInvoiceID == item.SalesInvoiceID where (d.RecPayStatus < 2 || d.RecPayStatus == null) select d).ToList();
            //    //var invoicedeails = (from d in db.CustomerInvoiceDetails where d.CustomerInvoiceID == item.CustomerInvoiceID where (d.RecPayStatus < 2 || d.RecPayStatus == null) select d).ToList();                                
            //    decimal? totamtpaid = 0;
            //    decimal? totadjust = 0;

            //    totamtpaid = ReceiptDAO.SP_GetCustomerInvoiceReceived(Convert.ToInt32(ID), Convert.ToInt32(item.CustomerInvoiceID), 0, Convert.ToInt32(CreditNoteId), "TR");

            //    var Invoice = new CustomerTradeReceiptVM();
            //    Invoice.InvoiceType = "TR";
            //    Invoice.JobCode = "";
            //    Invoice.SalesInvoiceID = item.CustomerInvoiceID; // SalesInvoiceID;
            //    Invoice.InvoiceNo = item.CustomerInvoiceNo;
            //    Invoice.InvoiceAmount = item.InvoiceTotal; // CourierCharge;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = item.InvoiceDate.ToString("dd/MM/yyyy");
            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = totadjust;

            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
            //                Invoice.Allocated = true;
            //                Invoice.Amount = Invoice.Balance;
            //                amountreceived = amountreceived - Invoice.Amount;
            //            }
            //            else if (amountreceived > 0)
            //            {
            //                Invoice.Amount = amountreceived;
            //                amountreceived = amountreceived - Invoice.Amount;
            //            }
            //            else
            //            {
            //                Invoice.Amount = 0;
            //            }
            //        }
            //        salesinvoice.Add(Invoice);
            //        //if (RecPayId == null)
            //        //{
            //        //    AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Invoice.SalesInvoiceID), Convert.ToDecimal(Invoice.Amount), 0); //customer invoiceid,amount
            //        //}
            //        //else
            //        //{
            //        //    AWBAllocation = ReceiptDAO.GetAWBAllocation(AWBAllocation, Convert.ToInt32(Invoice.SalesInvoiceID), Convert.ToDecimal(Invoice.Amount), Convert.ToInt32(RecPayId)); //customer invoiceid,amount
            //        //}
            //    }
            //}

            Session["CustomerInvoice"] = salesinvoice;

        }

        [HttpPost]
        public ActionResult AddAccount(int? AcHeadID, decimal? Amount, string Remarks, string TransType = "CN")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            List<CreditNoteDetailVM> list = (List<CreditNoteDetailVM>)Session["CreditNoteDetail"];
            CreditNoteDetailVM item = new CreditNoteDetailVM();
            if (list != null)
            {
                item = list.Where(cc => cc.AcHeadID == AcHeadID).FirstOrDefault();
            }
            else
            {
                list = new List<CreditNoteDetailVM>();
            }
            if (item == null)
            {
                item = new CreditNoteDetailVM();
                item.AcHeadID = AcHeadID;
                item.AcHeadName = db.AcHeads.Find(item.AcHeadID).AcHead1;
                item.Amount = Amount;
                item.Remarks = Remarks;
                list.Add(item);
            }

            Session["CreditNoteDetail"] = list;
            CreditNoteVM vm = new CreditNoteVM();
            vm.Details = list;
            return PartialView("CreditNoteDetail", vm);
        }

        [HttpPost]
        public ActionResult DeleteAccount(int index)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            List<CreditNoteDetailVM> list = (List<CreditNoteDetailVM>)Session["CreditNoteDetail"];
            List<CreditNoteDetailVM> list1 = new List<CreditNoteDetailVM>();
            CreditNoteDetailVM item = new CreditNoteDetailVM();
            list.RemoveAt(index);

            Session["CreditNoteDetail"] = list;
            CreditNoteVM vm = new CreditNoteVM();
            vm.Details = list;
            return PartialView("CreditNoteDetail", vm);
        }
        
        [HttpPost]
        public JsonResult DeleteCreditNote(int id)
        {

            //int k = 0;
            if (id != 0)
            {
                DataTable dt = CreditNoteDAO.DeleteCreditNote(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {

                        string status = dt.Rows[0][0].ToString();
                        string message = dt.Rows[0][1].ToString();

                        return Json(new { status = status, message = message });
                    }

                }
                else
                {
                    return Json(new { status = "OK", message = "Contact Admin!" });
                }
            }
            return Json(new { status = "Failed", message = "Contact Admin!" });

        }
        public JsonResult GetInvoiceNo(string term)
        {
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var lst = new List<CustomerTradeReceiptVM>();
            salesinvoice = (List<CustomerTradeReceiptVM>)Session["CustomerInvoice"];
            if (salesinvoice != null)
            {
                if (term.Trim() != "")
                {
                    lst = salesinvoice.Where(cc => cc.InvoiceNo.Contains(term)).OrderBy(cc => cc.InvoiceNo).ToList();
                }
                else
                {
                    lst = salesinvoice.OrderBy(cc => cc.InvoiceNo).ToList();
                }
            }
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCreditNoteNo(string Type)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            string CreditNoteNo=AccountsDAO.GetMaxCreditNoteNo(fyearid, Type);
            return Json(CreditNoteNo, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetCustomerTypeAccount(string Type)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            
            var vm = GetCreditNoteHead(Type);
            return Json(vm, JsonRequestBehavior.AllowGet);
        }
        public class jobno
        {

            public string JobNo { get; set; }
            public int JobNum { get; set; }
            public bool Istrading { get; set; }

        }
      
        public JsonResult GetAmountByinvono(int invno, bool IsTrading)
        {
            Getamtclass ob = new Getamtclass();
            ob.invoiceamt = 0;
            ob.recamt = 0;
            if (IsTrading == false)
            {
                //int jobid = (from j in db.JobGenerations where j.JobCode == invno select j.JobID).FirstOrDefault();

                //int invid = (from c in db.JInvoices where c.JobID == jobid select c.InvoiceID).FirstOrDefault();

                //decimal invamt = (from c in db.JInvoices where c.InvoiceID == invid select c.SalesHome).FirstOrDefault().Value;

                //decimal recamt = (from r in db.RecPayDetails where r.InvoiceID == invid select r.Amount).FirstOrDefault().Value;


                //ob.invoiceamt = invamt;
                //ob.recamt = recamt;
            }
            else
            {
                var sinvoice = (from d in db.CustomerInvoices where d.CustomerInvoiceID == invno select d).FirstOrDefault();
                var sinDetails = (from c in db.CustomerInvoiceDetails where c.CustomerInvoiceID == sinvoice.CustomerInvoiceID select c).ToList();
                List<int?> pinvoiceids = sinDetails.Select(d => (int?)d.CustomerInvoiceDetailID).ToList();
                ob.invoiceamt = Convert.ToDecimal(sinDetails.Sum(d => d.NetValue));
                var recpay = (from x in db.RecPayDetails where pinvoiceids.Contains(x.InvoiceID) select x).ToList();

                if (recpay.Count > 0)
                {
                    ob.recamt = Convert.ToDecimal(Math.Abs(recpay.Sum(s => s.Amount.Value)));
                }

            }


            return Json(ob, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult ServiceIndex()
        //{
        //    var data = db.CreditNotes.ToList(); .Where(d => d.IsShipping == false).ToList();

        //    List<CreditNoteVM> lst = new List<CreditNoteVM>();
        //    foreach (var item in data)
        //    {
        //        //var job = (from c in db.JInvoices where c.InvoiceID == item.InvoiceID select c).FirstOrDefault();
        //        string jobcode = "";
        //        //if (job != null)
        //        //{
        //        //    var jobid = job.JobID;
        //        //     jobcode = (from j in db.JobGenerations where j.JobID == jobid select j.JobCode).FirstOrDefault();

        //        //}
        //        //else
        //        //{
        //        var purchaseinvoice = (from d in db.CustomerInvoices where d.CustomerInvoiceID == item.InvoiceID select d).FirstOrDefault();
        //        jobcode = purchaseinvoice.CustomerInvoiceNo;

        //        //jobcode = item.InvoiceID.ToString();
        //        //}
        //        string customer = (from c in db.CustomerMasters where c.CustomerID == item.CustomerID && c.CustomerType == "CR" select c.CustomerName).FirstOrDefault();

        //        CreditNoteVM v = new CreditNoteVM();
        //        v.JobNO = jobcode;
        //        v.Date = item.CreditNoteDate.Value;
        //        v.CustomerName = customer;
        //        v.Amount = item.Amount.Value;
        //        lst.Add(v);

        //    }

        //    return View(lst);

        //}

        public JsonResult CreditNoteVoucher(int id)
        {
            string reportpath = "";
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GenerateCreditNoteVoucherPrint(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ServiceCreate()
        {
            ViewBag.customer = db.CustomerMasters.Where(d => d.CustomerType == "CR").OrderBy(x => x.CustomerName).ToList();
            ViewBag.achead = db.AcHeads.ToList();
            List<jobno> lst = new List<jobno>();
            ViewBag.jobno = lst;

            return View();


        }

        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.CustomerID > 0 && (c1.CustomerType == "CR" || c1.CustomerType == "CL") && c1.CustomerName.ToLower().StartsWith(term.Trim().ToLower())
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.CustomerID > 0 && (c1.CustomerType == "CR" || c1.CustomerType == "CL")
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }

            

        }
        public class Getamtclass
        {
            public decimal? invoiceamt { get; set; }
            public decimal? recamt { get; set; }

        }

    }
}
