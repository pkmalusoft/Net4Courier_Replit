using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Dynamic;
using System.Data;
using Net4Courier.DAL;
using Newtonsoft.Json;
//using Microsoft.Reporting.WebForms;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class DebitNoteController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 db = new Entities1();

        EditCommanFu editfu = new EditCommanFu();
        public ActionResult Index()
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int Fyearid = Convert.ToInt32(Session["fyearid"]);

            var data = db.DebitNotes.Where(cc=>cc.FYearID==Fyearid && cc.BranchID==BranchID).OrderByDescending(c=>c.DebitNoteDate).ToList();

            List<DebitNoteVM> lst = new List<DebitNoteVM>();
            foreach (var item in data)
            {
                

                string supplier = (from c in db.SupplierMasters where c.SupplierID == item.SupplierID select c.SupplierName).FirstOrDefault();

                DebitNoteVM v = new DebitNoteVM();
                v.DebitNoteNo = item.DebitNoteNo;
                v.DebitNoteId = item.DebitNoteID;
                //v.JobNo = jobcode;
                v.Remarks = item.Remarks;
                v.Date = item.DebitNoteDate.Value;
                v.SupplierName = supplier;
                v.Amount = item.Amount.Value;
                lst.Add(v);

            }

            return View(lst);

        }


        public ActionResult Create(int id = 0)
        {
            //List<Invoices> lst = new List<Invoices>();
            //ViewBag.Supplier = db.SupplierMasters.OrderBy(x => x.SupplierName).ToList();
            //ViewBag.AcHead = db.AcHeads.OrderBy(x => x.AcHead1).ToList();
            //ViewBag.Invoice = lst;
            //return View();
            ViewBag.SupplierType = db.SupplierTypes.ToList();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Supplier = db.SupplierMasters.OrderBy(x => x.SupplierName).ToList();
            ViewBag.AcHead = db.AcHeads.OrderBy(x => x.AcHead1).OrderBy(cc=>cc.AcHead1).ToList();

            if (id == 0)
            {
                ViewBag.Title = "Create";
                DebitNoteVM vm = new DebitNoteVM();
                
                vm.SupplierTypeId = 1;
                vm = GetDebitNoteHead(vm.SupplierTypeId);
                vm.DebitNoteNo = AccountsDAO.GetMaxDebiteNoteNo(fyearid);
                vm.Date = CommonFunctions.GetLastDayofMonth().Date;
                
                //var supplierachead = db.AccountSetups.Where(cc => cc.PageName == "Debit Note/Supplier Journal").FirstOrDefault();
                //vm.AcHeadID = Convert.ToInt32(supplierachead.DebitAccountId); //527

                vm.TransType = "DN";
                //vm.AmountType = "1";
                //vm.AcDetailAmountType = "0";
                List<DebitNoteDetailVM> list = new List<DebitNoteDetailVM>();
                vm.Details = list;

                Session["DebitNoteDetail"] = list;
                ViewBag.Message = "";
                ViewBag.SaveEnable = true;
                return View(vm);
            }
            else
            {
                ViewBag.Title = "Modify";
                DebitNoteVM vm = new DebitNoteVM();
                var v = db.DebitNotes.Find(id);
                
                vm.SupplierTypeId = Convert.ToInt32(v.SupplierTypeId);
                vm = GetDebitNoteHead(vm.SupplierTypeId);
                vm.DebitNoteId = v.DebitNoteID;
                vm.Date = v.DebitNoteDate;
                vm.AcJournalID = Convert.ToInt32(v.AcJournalID);
                vm.DebitNoteNo = v.DebitNoteNo;
                vm.SupplierID = Convert.ToInt32(v.SupplierID);
                vm.SupplierTypeId = Convert.ToInt32(v.SupplierTypeId);
                vm.SupplierName = db.SupplierMasters.Find(v.SupplierID).SupplierName;
                vm.AcHeadID = Convert.ToInt32(v.AcHeadID);
                vm.Amount = Convert.ToDecimal(v.Amount);
                vm.InvoiceType = v.InvoiceType;
                vm.Remarks = v.Remarks;
                vm.TransType = v.TransType;
                if (v.TransType == "SJ")
                    vm.HCreditAmount = vm.Amount;
                else if (v.TransType == "DN")
                    vm.HDebitAmount = vm.Amount;
                //vm.InvoiceID = Convert.ToInt32(v.InvoiceID);
                //if (v.RecPayID != null && v.RecPayID!=0)
                //    vm.RecPayID = Convert.ToInt32(v.RecPayID);
                //else
                //    vm.RecPayID = 0;

                var detaillist = (from c in db.DebitNoteDetails join d in db.AcHeads on c.AcHeadID equals d.AcHeadID where c.DebitNoteID == v.DebitNoteID select new DebitNoteDetailVM { AcHeadID = c.AcHeadID, AcHeadName = d.AcHead1, Amount = c.Amount, Remarks = c.Remarks }).ToList();
                vm.Details = detaillist;
                for (var i = 0; i < vm.Details.Count; i++)
                {
                    if (v.TransType == "SJ")
                        vm.Details[i].DebitAmount = Convert.ToDecimal(detaillist[i].Amount);
                    else
                        vm.Details[i].CreditAmount = Convert.ToDecimal(detaillist[i].Amount);
                }
                
                Session["DebitNoteDetail"] = detaillist;
                StatusModel result = AccountsDAO.CheckDateValidate(vm.Date.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
                //SetTradeInvoiceOfSupplier(vm.SupplierID, 0, vm.DebitNoteId, vm.TransType);
                List<CustomerTradeReceiptVM> lst = (List<CustomerTradeReceiptVM>)Session["SupplierInvoice"];

                //if (v.TransType == "DN")
                //{
                //    if (v.InvoiceType == "TR")
                //    {
                //        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "TR").FirstOrDefault();
                //        //var invoice = db.SupplierInvoices.Find(vm.InvoiceID);
                //        if (invoice != null)
                //        {
                //            vm.InvoiceNo = invoice.InvoiceNo;
                //            vm.InvoiceDate = invoice.DateTime;
                //            vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                //            vm.AmountPaid = Convert.ToDecimal(invoice.AmountReceived);
                //        }
                //    }
                //    else if (v.InvoiceType == "OP")
                //    {
                //        var invoice1 = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();
                //        //var invoice1 = db.AcOPInvoiceDetails.Where(cc => cc.AcOPInvoiceDetailID == vm.InvoiceID).FirstOrDefault();
                //        if (invoice1 != null)
                //        {
                //            vm.InvoiceNo = invoice1.InvoiceNo;
                //            vm.InvoiceDate = invoice1.DateTime;
                //            vm.InvoiceAmount = Convert.ToDecimal(invoice1.InvoiceAmount);
                //            vm.AmountPaid = Convert.ToDecimal(invoice1.AmountReceived);
                //        }
                //    }
                //}
                //else if(v.RecPayID != null && v.RecPayID != 0 && v.TransType == "SJ")
                //{
                //    vm.InvoiceID = Convert.ToInt32(v.RecPayID);
                //    vm.InvoiceType = v.InvoiceType;
                //    vm.ForInvoice = true;

                //    if (v.InvoiceType == "TR")
                //    {
                //        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "TR").FirstOrDefault();
                //        if (invoice != null)
                //        {
                //            vm.InvoiceNo = invoice.InvoiceNo;
                //            vm.InvoiceDate = invoice.DateTime;
                //            vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                //            vm.AmountPaid = Convert.ToDecimal(invoice.AmountReceived);
                //        }
                //    }
                //    else if (v.InvoiceType == "OP")
                //    {
                //        var invoice = lst.Where(cc => cc.SalesInvoiceID == vm.InvoiceID && cc.InvoiceType == "OP").FirstOrDefault();
                //        vm.InvoiceNo = invoice.InvoiceNo;
                //        vm.InvoiceDate = invoice.DateTime;
                //        vm.InvoiceAmount = Convert.ToDecimal(invoice.InvoiceAmount);
                //        vm.AmountPaid = Convert.ToDecimal(invoice.AmountReceived);
                //    }
                //}
                //else
                //{
                //    vm.ForInvoice = false;
                //}
                //SetTradeInvoiceOfCustomer(vm.CustomerID, 0, vm.CreditNoteID);
                vm.Date = Convert.ToDateTime(v.DebitNoteDate);

                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult Create(DebitNoteVM v)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AcJournalMaster ajm = new AcJournalMaster();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (v.DebitNoteId > 0)
            {
                ajm = db.AcJournalMasters.Find(v.AcJournalID);
                var ajmd = db.AcJournalDetails.Where(cc => cc.AcJournalID == v.AcJournalID).ToList();
                if (ajmd != null)
                {
                    if (ajmd!=null)
                        db.AcJournalDetails.RemoveRange(ajmd);
                    if (ajm!=null)
                        db.AcJournalMasters.Remove(ajm);
                    db.SaveChanges();
                }
                v.AcJournalID = 0;
            }
            if (v.AcJournalID == 0)
            {
                ajm = new AcJournalMaster();
                int acjm = 0;
                acjm = (from c in db.AcJournalMasters orderby c.AcJournalID descending select c.AcJournalID).FirstOrDefault();

                ajm.AcJournalID = acjm + 1;
                ajm.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                ajm.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                ajm.AcFinancialYearID = fyearid;
                ajm.PaymentType = 1;
                var customer = db.SupplierMasters.Find(v.SupplierID).SupplierName;
                ajm.Remarks = v.Remarks; // + " DN: for " + customer + " invoice : " + v.InvoiceNo;
                ajm.StatusDelete = false;
                ajm.VoucherNo = AccountsDAO.GetMaxVoucherNo(v.TransType, fyearid);
                ajm.TransDate = v.Date;
                ajm.TransType = 1;
                ajm.VoucherType = v.TransType;
              
                db.AcJournalMasters.Add(ajm);
                db.SaveChanges();
            }
            
            AcJournalDetail a = new AcJournalDetail();
            a = db.AcJournalDetails.Where(cc => cc.AcJournalID == ajm.AcJournalID && cc.Amount > 0).FirstOrDefault();
            if (a == null)
            {
                a = new AcJournalDetail();
                a.AcJournalDetailID = 0;
            }
            if (a.AcJournalDetailID == 0)
            {
                int maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();
                a.AcJournalDetailID = maxacj + 1;
                a.AcJournalID = ajm.AcJournalID;
                //var customercon = db.AcHeads.Where(cc => cc.AcHead1 == "Supplier Control A/c ( Cr)").FirstOrDefault();
                //var  supplierachead1= db.AccountSetups.Where(cc => cc.PageName == "Debit Note/Supplier Journal").FirstOrDefault();                
                //a.AcHeadID = supplierachead1.DebitAccountId; //527 customercon.AcHeadID; ;
                //if (a.AcHeadID!=null)
                //    v.AcHeadID  =Convert.ToInt32(a.AcHeadID);

            }
            a.AcHeadID = v.AcHeadID;
            a.Amount = v.Amount;
            a.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            a.Remarks = v.Remarks;

            if (a.ID == 0)
            {
                db.AcJournalDetails.Add(a);
                db.SaveChanges();
            }
            else
            {
                db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            foreach (var detail in v.Details)
            {
                AcJournalDetail b = new AcJournalDetail();
                b = db.AcJournalDetails.Where(cc => cc.AcJournalID == ajm.AcJournalID && cc.Amount < 0).FirstOrDefault();
                if (b == null)
                {
                    b = new AcJournalDetail();
                    b.AcJournalDetailID = 0;
                }
                if (b.AcJournalDetailID == 0)
                {
                    int maxacj = 0;
                    maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();

                    b.AcJournalDetailID = maxacj + 1;
                    b.AcJournalID = ajm.AcJournalID;
                }
                b.AcHeadID = detail.AcHeadID;
                if (v.TransType == "DN")
                    b.Amount = -1 * detail.Amount;
                else
                    b.Amount =  detail.Amount;
                b.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                b.Remarks = detail.Remarks;
                if (b.ID == 0)
                {
                    db.AcJournalDetails.Add(b);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(b).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            
            DebitNote d = new DebitNote();
            if (v.DebitNoteId == 0)
            {
                int maxid = 0;

                var data = (from c in db.DebitNotes orderby c.DebitNoteID descending select c).FirstOrDefault();

                if (data == null)
                {
                    maxid = 1;
                }
                else
                {
                    maxid = data.DebitNoteID + 1;
                }
                d.CreatedBy = userid;
                d.CreatedDate = CommonFunctions.GetBranchDateTime();
                d.DebitNoteID = maxid;
                d.DebitNoteNo = AccountsDAO.GetMaxDebiteNoteNo(fyearid);
            }
            else
            {
                d = db.DebitNotes.Find(v.DebitNoteId);
                var det = db.DebitNoteDetails.Where(cc => cc.DebitNoteID == v.DebitNoteId).ToList();
                if (det != null)
                {
                    db.DebitNoteDetails.RemoveRange(det);
                    db.SaveChanges();
                }
            }
            
            
            d.InvoiceType = v.InvoiceType;
            d.TransType = v.TransType;
            if (v.InvoiceID != 0)
            {
                if (v.TransType == "DN")
                    d.InvoiceID = v.InvoiceID;
                else
                    d.RecPayID = v.InvoiceID;
            }
            else
            {
                d.InvoiceID = 0;
            }
            d.DebitNoteDate = v.Date;
            d.Amount = v.Amount;
            d.AcJournalID = ajm.AcJournalID;
            d.FYearID = Convert.ToInt32(Session["fyearid"].ToString());
            d.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            d.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var supplierachead = db.AccountSetups.Where(cc => cc.PageName == "Debit Note/Supplier Journal").FirstOrDefault();
            d.AcHeadID = v.AcHeadID; // supplierachead.DebitAccountId; //527
            
            d.SupplierID = v.SupplierID;
           
            d.ModifiedBy = userid;
            d.ModifiedDate = CommonFunctions.GetBranchDateTime();
            d.Remarks = v.Remarks;
            //d.IsShipping = true;
            if (v.DebitNoteId == 0)
            {
                db.DebitNotes.Add(d);
                db.SaveChanges();
                TempData["SuccessMsg"] = "Successfully Added Debit Note";
            }
            else
            {
                db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "Successfully Updated Debit Note";
            }
            foreach (var detail in v.Details)
            {
                DebitNoteDetail det = new DebitNoteDetail();
                det.AcHeadID = detail.AcHeadID;
                det.Amount = detail.Amount;
                det.Remarks = detail.Remarks;
                det.DebitNoteID = d.DebitNoteID;
                db.DebitNoteDetails.Add(det);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "DebitNote");





        }


        public DebitNoteVM GetDebitNoteHead(int SupplierTypeID)
        {
            DebitNoteVM vm = new DebitNoteVM();
            
            var acsetup = db.SupplierTypes.Where(cc => cc.SupplierTypeID == SupplierTypeID).FirstOrDefault();

            //var acsetup = db.AccountSetups.Where(cc => cc.PageName == "Credit Note").FirstOrDefault();
            if (acsetup != null)
            {
                if (acsetup.AcHeadID != null)
                {
                    vm.AcHeadID = Convert.ToInt32(acsetup.AcHeadID);
                    var head = db.AcHeads.Find(acsetup.AcHeadID);
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

        public JsonResult GetDebitNoteNo(string Type,int SupplierTypeID)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            string DebitNoteNo = AccountsDAO.GetMaxDebiteNoteNo(fyearid,Type);
            DebitNoteVM vm = new DebitNoteVM();

            var acsetup = db.SupplierTypes.Where(cc => cc.SupplierTypeID == SupplierTypeID).FirstOrDefault();

            //var acsetup = db.AccountSetups.Where(cc => cc.PageName == "Credit Note").FirstOrDefault();
            if (acsetup != null)
            {
                if (acsetup.AcHeadID != null)
                {
                    vm.AcHeadID = Convert.ToInt32(acsetup.AcHeadID);
                    var head = db.AcHeads.Find(acsetup.AcHeadID);
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

            return Json(new { DebitNoteNo = DebitNoteNo, AcHeadID = vm.AcHeadID, AcHeadName = vm.AcHeadName }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult SaveDebitNote(DebitNoteVM v, string Details, string Allocation)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AcJournalMaster ajm = new AcJournalMaster();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            try
            {
                var IDetails = JsonConvert.DeserializeObject<List<DebitNoteDetailVM>>(Details);
                v.Details = IDetails;
                var IDetails1 = JsonConvert.DeserializeObject<List<CustomerRcieptChildVM>>(Allocation);
                v.CustomerRcieptChildVM = IDetails1;

                DebitNote d = new DebitNote();
                if (v.DebitNoteId == 0)
                {
                    int maxid = 0;

                    var data = (from c in db.DebitNotes orderby c.DebitNoteID descending select c).FirstOrDefault();

                    if (data == null)
                    {
                        maxid = 1;
                    }
                    else
                    {
                        maxid = data.DebitNoteID + 1;
                    }

                    d.DebitNoteID = maxid;
                    d.DebitNoteNo = AccountsDAO.GetMaxDebiteNoteNo(fyearid,v.TransType);

                    d.FYearID = Convert.ToInt32(Session["fyearid"].ToString());
                    d.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                    d.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    //d.statusclose = false;

                    d.CreatedBy = userid;
                    d.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    d.ModifiedBy = userid;
                    d.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                }
                else
                {
                    d = db.DebitNotes.Find(v.DebitNoteId);
                    var det = db.DebitNoteDetails.Where(cc => cc.DebitNoteID == v.DebitNoteId).ToList();
                    if (det != null)
                    {
                        db.DebitNoteDetails.RemoveRange(det);
                        db.SaveChanges();
                    }

                    d.ModifiedBy = userid;
                    d.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                }

                d.InvoiceType = v.InvoiceType;
                d.DebitNoteDate = v.Date;
                if (v.TransType == "CN")
                    d.Amount = v.Amount;
                else
                    d.Amount = v.Amount;

                //d.AcJournalID = d.AcJournalID;

                d.AcHeadID = v.AcHeadID;
                d.SupplierID = v.SupplierID;
                d.SupplierTypeId = v.SupplierTypeId;
                d.Remarks = v.Remarks;
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
                if (v.DebitNoteId == 0)
                {
                    db.DebitNotes.Add(d);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    var cdetail = db.DebitNoteDetails.Where(cc => cc.DebitNoteID == d.DebitNoteID).ToList();
                    db.DebitNoteDetails.RemoveRange(cdetail);
                    db.SaveChanges();


                    var allocationdetail = db.RecPayDetails.Where(cc => cc.ForDebitNoteId == d.DebitNoteID).ToList();
                    db.RecPayDetails.RemoveRange(allocationdetail);
                    db.SaveChanges();
                }

                foreach (var detail in v.Details)
                {
                    DebitNoteDetail det = new DebitNoteDetail();
                    det.AcHeadID = detail.AcHeadID;
                    det.Amount = detail.Amount;
                    det.Remarks = detail.Remarks;
                    det.DebitNoteID = d.DebitNoteID;
                    db.DebitNoteDetails.Add(det);
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
                        det.ForDebitNoteId = d.DebitNoteID;
                        db.RecPayDetails.Add(det);
                        db.SaveChanges();
                    }
                }
                CreditNoteDAO.InsertJournalOfDebitNote(d.DebitNoteID, fyearid);

                if (v.DebitNoteId == 0)
                {
                    return Json(new { status = "OK", DebitNoteID = d.DebitNoteID, message = "Debit Note Added Succesfully!" });
                }
                else
                {
                    return Json(new { status = "OK", DebitNoteID = d.DebitNoteID, message = "Debit Note Update Succesfully!" });
                }


            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", DebitNoteID = v.DebitNoteId, message = ex.Message });
            }





        }

        [HttpPost]
        public JsonResult GetTradeInvoiceOfSupplier(int? ID, decimal? amountreceived, int? DebitNoteId, string TransType = "DN")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            decimal Advance = 0;
            //Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(ID), Convert.ToInt32(RecPayId), fyearid);
            //if (amountreceived > 0)
            //    amountreceived = amountreceived + Advance;
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            
            //    if (TransType == "DN")
            //  //  salesinvoice = ReceiptDAO.SP_GetSupplierInvoicePending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(DebitNoteId), "OP");
            //else if (TransType == "SJ")
            //    salesinvoice = ReceiptDAO.SP_GetSupplierReceiptPending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(DebitNoteId), "OP");

            //var AllOPInvoices = (from d in db.AcOPInvoiceDetails join m in db.AcOPInvoiceMasters on d.AcOPInvoiceMasterID equals m.AcOPInvoiceMasterID where m.AcFinancialYearID == fyearid && d.Amount < 0 && m.StatusSDSC != "C" && m.PartyID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();

            //foreach (var item in AllOPInvoices)
            //{

            //    decimal? totamtpaid = 0;
            //    var allrecpay = (from d in db.RecPayDetails where d.AcOPInvoiceDetailID == item.AcOPInvoiceDetailID select d).ToList();
            //    totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(ID), item.AcOPInvoiceDetailID, 0, 0, "OP");

            //    var Invoice = new CustomerTradeReceiptVM();
            //    Invoice.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
            //    Invoice.SalesInvoiceID = 0;
            //    Invoice.InvoiceType = "OP";
            //    Invoice.JobCode = "OP" + item.AcOPInvoiceDetailID.ToString();
            //    Invoice.InvoiceNo = item.InvoiceNo; ;

            //    Invoice.InvoiceAmount = item.Amount * -1;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = Convert.ToDateTime(item.InvoiceDate).ToString("dd/MM/yyyy");
            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = 0;
            //    Invoice.Amount = 0;
            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
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

            ////transaction
            //var AllInvoices = (from d in db.SupplierInvoices where (d.IsDeleted == null || d.IsDeleted == false) && d.SupplierID == ID select d).OrderBy(cc => cc.InvoiceDate).ToList();
            //foreach (var item in AllInvoices)
            //{

            //    var invoicedeails = (from d in db.SupplierInvoiceDetails where d.SupplierInvoiceID == item.SupplierInvoiceID select d).ToList();

            //    decimal? totamtpaid = 0;
            //    decimal? totadjust = 0;


            //    totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(ID), item.SupplierInvoiceID, 0, 0, "TR");
            //    var Invoice = new CustomerTradeReceiptVM();
            //    Invoice.JobCode = "TR" + item.SupplierInvoiceID.ToString();
            //    Invoice.SalesInvoiceID = item.SupplierInvoiceID; // SalesInvoiceID;
            //    Invoice.InvoiceNo = item.InvoiceNo;
            //    Invoice.InvoiceType = "TR";
            //    Invoice.InvoiceAmount = item.InvoiceTotal; // CourierCharge;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = item.InvoiceDate.ToString("dd/MM/yyyy");
            //    //var RecPay = (from d in db.RecPayDetails where d.RecPayDetailID == det.RecPayDetailId select d).FirstOrDefault();

            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = totadjust;
            //    Invoice.Amount = 0;
            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
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
            Session["SupplierInvoice"] = salesinvoice;
            return Json(new { advance = Advance, salesinvoice = salesinvoice }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SetTradeInvoiceOfSupplier(int? ID, decimal? amountreceived, int DebitNoteID,string TransType)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());

            //DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            //DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            //var receipts = (from d in db.RecPays where d.FYearID == fyearid && d.SupplierID == ID select d.FMoney).Sum();
            //var receiptdetails = (from d in db.RecPays join c in db.RecPayDetails on d.RecPayID equals c.RecPayID where d.FYearID == fyearid && d.SupplierID == ID && c.InvoiceID > 0 && c.Amount < 0 select (-1 * c.Amount)).Sum();

            //            decimal Advance = 0;
            //            Advance = ReceiptDAO.SP_GetSupplierAdvance(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(id), fyearid);

            ////            var advance = receipts - receiptdetails;
            
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            //if (TransType == "DN")
            //    salesinvoice = ReceiptDAO.SP_GetSupplierInvoicePending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(DebitNoteID), "OP");
            //else
            //    salesinvoice = ReceiptDAO.SP_GetSupplierReceiptPending(Convert.ToInt32(ID), 0, 0, Convert.ToInt32(DebitNoteID), "OP");


            //var AllOPInvoices = (from d in db.AcOPInvoiceDetails join m in db.AcOPInvoiceMasters on d.AcOPInvoiceMasterID equals m.AcOPInvoiceMasterID where m.AcFinancialYearID == fyearid && m.StatusSDSC != "C" && m.PartyID == ID select d).ToList();

            //foreach (var item in AllOPInvoices)
            //{
            //    decimal? totamt = 0;
            //    decimal? totamtpaid = 0;
            //    decimal? totadjust = 0;
            //    totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.AcOPInvoiceDetailID), 0, DebitNoteID, "OP");
            //    var Invoice = new CustomerTradeReceiptVM();
            //    Invoice.AcOPInvoiceDetailID = item.AcOPInvoiceDetailID;
            //    Invoice.SalesInvoiceID = item.AcOPInvoiceDetailID;
            //    Invoice.InvoiceType = "OP";
            //    Invoice.JobCode = "OP" + item.AcOPInvoiceDetailID.ToString();
            //    Invoice.InvoiceNo = item.InvoiceNo; ;
            //    Invoice.InvoiceAmount = item.Amount * -1;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = Convert.ToDateTime(item.InvoiceDate).ToString("dd/MM/yyyy");
            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = totadjust;
            //    Invoice.Amount = 0;
            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
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

            ////transaction
            //var AllInvoices = (from d in db.SupplierInvoices where d.SupplierID == ID && (d.IsDeleted == false || d.IsDeleted == null) select d).ToList();
            //foreach (var item in AllInvoices)
            //{
            //    decimal? totamtpaid = 0;
            //    decimal? totadjust = 0;

            //    totamtpaid = ReceiptDAO.SP_GetSupplierInvoicePaid(Convert.ToInt32(cust.SupplierID), Convert.ToInt32(item.SupplierInvoiceID), 0, DebitNoteID, "TR");

            //    var Invoice = new CustomerTradeReceiptVM();

            //    Invoice.JobCode = "TR" + item.SupplierInvoiceID.ToString();
            //    Invoice.SalesInvoiceID = item.SupplierInvoiceID; // SalesInvoiceID;
            //    Invoice.InvoiceNo = item.InvoiceNo;
            //    Invoice.InvoiceType = "TR";
            //    Invoice.InvoiceAmount = item.InvoiceTotal; // CourierCharge;
            //    Invoice.date = item.InvoiceDate;
            //    Invoice.DateTime = item.InvoiceDate.ToString("dd/MM/yyyy");
            //    Invoice.AmountReceived = totamtpaid;
            //    Invoice.Balance = Invoice.InvoiceAmount - totamtpaid;
            //    Invoice.AdjustmentAmount = totadjust;
            //    Invoice.Amount = 0;
            //    if (Invoice.Balance > 0)
            //    {
            //        if (amountreceived != null)
            //        {
            //            if (amountreceived >= Invoice.Balance)
            //            {
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
            Session["SupplierInvoice"] = salesinvoice;
            return Json(salesinvoice, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSupplierAccount(int ID)
        {
                    var acheadid = 0;
            var setupheadid = 0;
           var supplier=db.SupplierMasters.Find(ID);
            var supplierachead1 = db.AccountSetups.Where(cc => cc.PageName == "Debit Note/Supplier Journal").FirstOrDefault();
            if (supplierachead1 != null)
                if (supplierachead1.DebitAccountId!=null)
                     setupheadid =Convert.ToInt32(supplierachead1.DebitAccountId); //527 customercon.AcHeadID; ;
            
            if (supplier != null)
            {
                if (supplier.AcHeadID != null)
                {
                    acheadid = Convert.ToInt32(supplier.AcHeadID);
                }
            }

            if (acheadid==0 && setupheadid>0)
            {
                acheadid = setupheadid;
            }
            string acheadname = "";
            if (acheadid > 0)
            {
                acheadname = db.AcHeads.Find(acheadid).AcHead1;
                return Json(new { AcheadID = acheadid, AcheadName = acheadname }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { AcheadID = 0, AcheadName = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult AddAccount(int? AcHeadID, decimal? Amount, string Remarks, string TransType = "CN")
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            List<DebitNoteDetailVM> list = (List<DebitNoteDetailVM>)Session["DebitNoteDetail"];
            DebitNoteDetailVM item = new DebitNoteDetailVM();
            if (list != null)
            {
                item = list.Where(cc => cc.AcHeadID == AcHeadID).FirstOrDefault();
            }
            else
            {
                list = new List<DebitNoteDetailVM>();
            }
            if (item == null)
            {
                item = new DebitNoteDetailVM();
                item.AcHeadID = AcHeadID;
                item.AcHeadName = db.AcHeads.Find(item.AcHeadID).AcHead1;
                item.Amount = Amount;
                item.Remarks = Remarks;
                list.Add(item);
            }

            Session["DebitNoteDetail"] = list;
            DebitNoteVM vm = new DebitNoteVM();
            vm.Details = list;
            return PartialView("DebitNoteDetail", vm);
        }

        [HttpPost]
        public ActionResult DeleteAccount(int index)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime fromdate = Convert.ToDateTime(Session["FyearFrom"].ToString());
            DateTime todate = Convert.ToDateTime(Session["FyearTo"].ToString());
            List<DebitNoteDetailVM> list = (List<DebitNoteDetailVM>)Session["DebitNoteDetail"];
            List<DebitNoteDetailVM> list1 = new List<DebitNoteDetailVM>();
            DebitNoteDetailVM item = new DebitNoteDetailVM();
            list.RemoveAt(index);

            Session["DebitNoteDetail"] = list;
            DebitNoteVM vm = new DebitNoteVM();
            vm.Details = list;
            return PartialView("DebitNoteDetail", vm);
        }

        [HttpPost]
        public JsonResult DeleteDebitNote(int id)
        { //int k = 0;
            if (id != 0)
            {
                DataTable dt = CreditNoteDAO.DeleteDebiteNote(id);
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

        public JsonResult DebitNoteVoucher(int id)
        {
            string reportpath = "";
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GenerateDebitNoteVoucherPrint(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetInvoiceNo(string term)
        {
            var salesinvoice = new List<CustomerTradeReceiptVM>();
            var lst = new List<CustomerTradeReceiptVM>();
            salesinvoice = (List<CustomerTradeReceiptVM>)Session["SupplierInvoice"];
            if (salesinvoice != null && salesinvoice.Count >0)
            {
                if (term.Trim() != "")
                {
                    lst = salesinvoice.Where(cc => cc.InvoiceNo.Contains(term.Trim())).OrderBy(cc => cc.InvoiceNo).ToList();
                }
                else
                {
                    lst = salesinvoice.OrderBy(cc => cc.InvoiceNo).ToList();
                }
            }
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        public class Invoices
        {
            public int InvoiceNo { get; set; }
            public bool Istrading { get; set; }
            public string InvoiceNum { get; set; }
            public decimal? Amount { get; set; }
        }

        [HttpPost]
        public ActionResult Create1(DebitNoteVM v)
        {
            AcJournalMaster ajm = new AcJournalMaster();

            int acjm = 0;
            acjm = (from c in db.AcJournalMasters orderby c.AcJournalID descending select c.AcJournalID).FirstOrDefault();

            ajm.AcJournalID = acjm + 1;

            ajm.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            ajm.AcFinancialYearID = Convert.ToInt32(Session["fyearid"].ToString());
            ajm.PaymentType = 1;
            ajm.Remarks = "Debit Note";
            ajm.StatusDelete = false;
            ajm.TransDate = v.Date;
            ajm.VoucherNo = "DB-" + ajm.AcJournalID;
            ajm.TransType = 1;
            ajm.VoucherType = "";

            db.AcJournalMasters.Add(ajm);
            db.SaveChanges();


            AcJournalDetail a = new AcJournalDetail();

            int maxacj = 0;
            maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();

            a.AcJournalDetailID = maxacj + 1;

            a.AcJournalID = ajm.AcJournalID;
            a.AcHeadID = v.AcHeadID;
            a.Amount = v.Amount;
            a.BranchID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            a.Remarks = "";

            db.AcJournalDetails.Add(a);
            db.SaveChanges();


            AcJournalDetail b = new AcJournalDetail();
            maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();
            b.AcJournalDetailID = maxacj + 1;
            b.AcJournalID = ajm.AcJournalID;
            b.AcHeadID = v.AcHeadID;
            b.Amount = -v.Amount;
            b.BranchID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            b.Remarks = "";





            db.AcJournalDetails.Add(b);
            db.SaveChanges();

            //var ids = (from x in db.PurchaseInvoiceDetails where x.PurchaseInvoiceID == v.InvoiceNo select (int?)x.PurchaseInvoiceDetailID).ToList();

            //int recpayid = (from c in db.RecPayDetails where ids.Contains(c.InvoiceID) select c.RecPayID).FirstOrDefault().Value;
            int recpayid = 0;
            int max = 0;

            var data = (from c in db.DebitNotes orderby c.DebitNoteID descending select c).FirstOrDefault();
            if (data == null)
            {
                max = 1;
            }
            else
            {
                max = data.DebitNoteID + 1;
            }


            DebitNote d = new DebitNote();
            d.DebitNoteID = max + 1;
            d.DebitNoteNo = "D-" + (max + 1);
            d.InvoiceID = v.InvoiceID;
            d.DebitNoteDate = v.Date;
            d.Amount = v.Amount;
            d.AcJournalID = ajm.AcJournalID;
            d.FYearID = Convert.ToInt32(Session["fyearid"].ToString());
            d.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            d.RecPayID = recpayid;
            d.AcHeadID = v.AcHeadID;
            d.SupplierID = v.SupplierID;
            d.InvoiceType = "S";
            //d.IsShipping = true;
            d.Remarks = v.Remarks;
            db.DebitNotes.Add(d);
            db.SaveChanges();

            TempData["SuccessMsg"] = "Successfully Added Debit Note";
            return RedirectToAction("List", "DebitNote");




        }
        public ActionResult GetInvoices(int id, string term)
        {
            List<Invoices> lst = new List<Invoices>();


            var data1 = (from c in db.PurchaseInvoices where c.SupplierID == id && c.PurchaseInvoiceNo.Contains(term) select c).ToList();
            foreach (var item in data1)
            {
                var purchaseinvoicedetails = (from d in db.PurchaseInvoiceDetails where d.PurchaseInvoiceID == item.PurchaseInvoiceID select d).ToList();

                Invoices s = new Invoices();
                s.InvoiceNo = item.PurchaseInvoiceID;
                s.Istrading = true;
                s.InvoiceNum = item.PurchaseInvoiceNo + "/ " + purchaseinvoicedetails.Sum(d => d.Rate);

                lst.Add(s);

            }
            //lst = lst.Where(d => d.InvoiceNum.Contains(term)).ToList();
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
     
        public class Amounts
        {
            public decimal InvAmt { get; set; }
            public decimal AmtPaid { get; set; }
        }

        public JsonResult GetAmount(int invno)
        {
            Amounts a = new Amounts();
            var purchaseinvoicedetails = (from d in db.PurchaseInvoiceDetails where d.PurchaseInvoiceID == invno select d).ToList();

            decimal pamt = Math.Abs((from x in db.RecPayDetails where x.InvoiceID == invno select x.Amount).FirstOrDefault().Value);

            a.InvAmt = Convert.ToDecimal(purchaseinvoicedetails.Sum(d => d.Rate));
            a.AmtPaid = pamt;

            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAmountByinvono(int invno, bool IsTrading)
        {
            Amounts a = new Amounts();
            decimal iamt = 0;
            decimal pamt = 0;

            var PurchaseInvoice = (from d in db.PurchaseInvoices where d.PurchaseInvoiceID == invno select d).FirstOrDefault();
            var PinDetails = (from c in db.PurchaseInvoiceDetails where c.PurchaseInvoiceID == PurchaseInvoice.PurchaseInvoiceID select c).ToList();
            List<int?> pinvoiceids = PinDetails.Select(d => (int?)d.PurchaseInvoiceDetailID).ToList();

            iamt = Convert.ToDecimal(PinDetails.Sum(d => d.Rate));
            var recpay = (from x in db.RecPayDetails where pinvoiceids.Contains(x.InvoiceID) select x).ToList();
            if (recpay.Count > 0)
            {
                pamt = Convert.ToDecimal(Math.Abs(recpay.Sum(s => s.Amount.Value)));
            }



            a.InvAmt = iamt;
            a.AmtPaid = pamt;

            return Json(a, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public ActionResult ServiceIndex(DebitNoteVM v)
        //{
        //    AcJournalMaster ajm = new AcJournalMaster();

        //    int acjm = 0;
        //    acjm = (from c in db.AcJournalMasters orderby c.AcJournalID descending select c.AcJournalID).FirstOrDefault();

        //    ajm.AcJournalID = acjm + 1;

        //    ajm.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
        //    ajm.AcFinancialYearID = Convert.ToInt32(Session["fyearid"].ToString());
        //    ajm.PaymentType = 1;
        //    ajm.Remarks = "Debit Note";
        //    ajm.StatusDelete = false;
        //    ajm.TransDate = v.Date;
        //    ajm.VoucherNo = "DB-" + ajm.AcJournalID;
        //    ajm.TransType = 1;
        //    ajm.VoucherType = "";

        //    db.AcJournalMasters.Add(ajm);
        //    db.SaveChanges();


        //    AcJournalDetail a = new AcJournalDetail();

        //    int maxacj = 0;
        //    maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();

        //    a.AcJournalDetailID = maxacj + 1;

        //    a.AcJournalID = ajm.AcJournalID;
        //    a.AcHeadID = v.AcHeadID;
        //    a.Amount = v.Amount;
        //    a.BranchID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
        //    a.Remarks = "";

        //    db.AcJournalDetails.Add(a);
        //    db.SaveChanges();


        //    AcJournalDetail b = new AcJournalDetail();
        //    maxacj = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();
        //    b.AcJournalDetailID = maxacj + 1;
        //    b.AcJournalID = ajm.AcJournalID;
        //    b.AcHeadID = v.AcHeadID;
        //    b.Amount = -v.Amount;
        //    b.BranchID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
        //    b.Remarks = "";





        //    db.AcJournalDetails.Add(b);
        //    db.SaveChanges();

        //    var ids = (from x in db.PurchaseInvoiceDetails where x.PurchaseInvoiceID == v.InvoiceNo select (int?)x.PurchaseInvoiceDetailID).ToList();

        //    int recpayid = (from c in db.RecPayDetails where ids.Contains(c.InvoiceID) select c.RecPayID).FirstOrDefault().Value;

        //    int max = 0;

        //    var data = (from c in db.DebitNotes orderby c.DebitNoteID descending select c).FirstOrDefault();
        //    if (data == null)
        //    {
        //        max = 1;
        //    }
        //    else
        //    {
        //        max = data.DebitNoteID + 1;
        //    }


        //    DebitNote d = new DebitNote();

        //    d.DebitNoteID = max + 1;
        //    d.DebitNoteNo = "D-" + (max + 1);
        //    d.InvoiceID = v.InvoiceID;
        //    d.DebitNoteDate = v.Date;
        //    d.Amount = v.Amount;
        //    d.AcJournalID = ajm.AcJournalID;
        //    d.FYearID = Convert.ToInt32(Session["fyearid"].ToString());
        //    d.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
        //    d.RecPayID = recpayid;
        //    d.AcHeadID = v.AcHeadID;
        //    d.SupplierID = v.SupplierID;
        //    d.InvoiceType = "S";
        //    d.IsShipping = false;
        //    db.DebitNotes.Add(d);
        //    db.SaveChanges();

        //    TempData["SuccessMsg"] = "Successfully Added Debit Note";
        //    return RedirectToAction("ServiceList", "DebitNote");




        //}

    }
}