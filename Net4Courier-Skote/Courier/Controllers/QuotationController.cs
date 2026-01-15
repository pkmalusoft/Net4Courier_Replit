using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class QuotationController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Quotation
        #region "Quotation"

        public ActionResult Index(QuotationSearch obj)
        {


            QuotationSearch model = new QuotationSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new QuotationSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.QuotationNo = "";
                
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.QuotationNo = "";
            }
            else
            {
                model = obj;
            }
            List<QuotationVM> lst = QuotationDAO.GetQuotationList(obj.FromDate, obj.ToDate, model.QuotationNo, yearid);
            model.Details = lst;

            return View(model);


        }
        public ActionResult Create(int id = 0,int newversion=0)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.Curency = db.CurrencyMasters.ToList();
            ViewBag.Unit = db.ItemUnits.ToList();
            ViewBag.Quotations = db.Quotations.Where(cc => cc.CustomerID == id).ToList();
            QuotationVM vm = new QuotationVM();
            if (id == 0)
            {
                var dta1 = QuotationDAO.GetMaxQuotationNo(branchid, fyearid);
                vm.QuotationNo = dta1.Split('-')[0];
                //vm.Version = dta1.Split('-')[1];
                vm.Version = 1;
                vm.QuotationDate = CommonFunctions.GetCurrentDateTime();

                ViewBag.Title = "Create";
                vm.QuotationDetails = new List<QuotationDetailVM>();
            }
            else
            {
               
                
                Quotation item = db.Quotations.Find(id);
                vm.QuotationID = item.QuotationID;
                vm.QuotationNo = item.QuotationNo;
                vm.QuotationDate = item.QuotationDate;
                vm.MobileNumber = item.MobileNumber;
                vm.ContactPerson = item.ContactPerson;
                vm.CustomerID = item.CustomerID;
                var cust = db.CustomerMasters.Find(vm.CustomerID);
                vm.CustomerName = cust.CustomerName;

                vm.Version = item.Version;
                vm.CurrencyId = item.CurrencyId;
                vm.Salutation = item.Salutation;
                vm.TermsandConditions = item.TermsandConditions;
                vm.PaymentTerms = item.PaymentTerms;
                vm.SubjectText = item.SubjectText;
                vm.QuotationValue = item.QuotationValue;
                vm.CustomerID = item.CustomerID;
                vm.Validity = item.Validity;
                vm.ClientDetail = item.ClientDetail;


                var details = (from c in db.QuotationDetails
                      where c.QuotationID == id
                      select new QuotationDetailVM { QuotationID = c.QuotationID, QuotationDetailId = c.QuotationDetailId, GroupName=c.GroupName, ItemDescription = c.ItemDescription, UOM = c.UOM, Quantity = c.Quantity, Rate = c.Rate, Value = c.Value, Remarks = c.Remarks }).ToList();
                Session["JQuotationDetail"] = details;
                vm.QuotationDetails = details;
                if (newversion == 1)
                {
                    ViewBag.Title = "New Version";
                    vm.QuotationID = 0;
                    var quotes = db.Quotations.Where(cc => cc.QuotationNo == vm.QuotationNo).Select(cc => cc.Version).Max();
                    if (quotes != null)
                        vm.Version = quotes + 1;
                    else
                        vm.Version = 1;
                }
                else if (newversion==-1)
                {
                    var dta1 = QuotationDAO.GetMaxQuotationNo(branchid, fyearid);
                    vm.QuotationNo = dta1.Split('-')[0];
                    vm.QuotationID =0;
                    vm.Version = 1;
                    ViewBag.Title = "Create";
                }
                else
                {
                    ViewBag.Title = "Modify";
                }
                if (newversion == 1)
                {
                   
                }
                
            }
            return View(vm);
        }

        
        public JsonResult SaveQuotation(Quotation quotation, string Details)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Unit = db.ItemUnits.ToList();
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            var IDetails = JsonConvert.DeserializeObject<List<QuotationDetailVM>>(Details);

           QuotationDetail item = new QuotationDetail();
           Quotation model = new Quotation();
            if (quotation.QuotationID == 0)
            {
                model = new Quotation();
            }
            else
            {
                model = db.Quotations.Find(quotation.QuotationID);
            }

            model.QuotationNo = quotation.QuotationNo;
            model.QuotationDate = quotation.QuotationDate;
            model.PaymentTerms = quotation.PaymentTerms;
            model.TermsandConditions = quotation.TermsandConditions;
            model.ClientDetail = quotation.ClientDetail;
            model.Salutation = quotation.Salutation;
            model.SubjectText = quotation.SubjectText;
            model.CustomerID = quotation.CustomerID;
            model.MobileNumber = quotation.MobileNumber;
            model.Version = quotation.Version;
            model.CurrencyId = quotation.CurrencyId;
            model.ContactPerson = quotation.ContactPerson;
            model.QuotationValue = quotation.QuotationValue;
            model.Validity = quotation.Validity;
            model.ModifiedBy = userid;
            model.ModifiedDate = CommonFunctions.GetCurrentDateTime();
            model.AcFinancialYearID = fyearid;
            model.BranchID = branchid; 
            if (quotation.QuotationID == 0)
            {
                model.CreatedBy = userid;
                model.CreatedDate = CommonFunctions.GetCurrentDateTime();

                db.Quotations.Add(model);
                db.SaveChanges();

            }
            else
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                var qdetails = (from d in db.QuotationDetails where d.QuotationID == quotation.QuotationID select d).ToList();
                db.QuotationDetails.RemoveRange(qdetails);
                db.SaveChanges();
            }


            foreach (var detail in IDetails)
            {
                if (detail.Deleted == false)
                {
                    item = new QuotationDetail();
                    item.QuotationID = model.QuotationID;
                    item.GroupName = detail.GroupName;
                    item.ItemDescription = detail.ItemDescription;
                    item.UOM = detail.UOM;
                    item.Quantity = detail.Quantity;
                    item.Value = detail.Value;
                    item.Rate = detail.Rate;
                    item.Remarks = detail.Remarks;
                    db.QuotationDetails.Add(item);
                    db.SaveChanges();
                }
            }
            
            
            if (quotation.QuotationID == 0)
            {
                return Json(new { QuotationId = model.QuotationID, message = "Quotation Added Succesfully!", status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { QuotationId = model.QuotationID, message = "Quotation Updated Succesfully!", status = "ok" }, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult DeleteConfirmed(int id)
        {
            string status = "";
            string message = "";
            //int k = 0;
            if (id != 0)
            {
              
                var quotation = db.Quotations.Find(id);
                if (quotation!=null)
                { 
                quotation.IsDeleted = true;
                db.Entry(quotation).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { status = "OK", message = "Delete Succesfully!" });

                }
                else
                {
                        //TempData["SuccessMsg"] = "You have successfully Deleted Cost !!";
                        return Json(new { status = "Failed", message = "Delete Failed!" });
                }
               
            }

            return Json(new { status = "Failed", message = "Delete Failed!" });

        }

        //public JsonResult ShowQuotationEntry(int Id = 0, int JobId = 0)
        //{
        //    int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
        //    int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    ViewBag.Curency = db.CurrencyMasters.ToList();
        //    ViewBag.Unit = db.ItemUnits.ToList();
        //    QuotationVM vm = new QuotationVM();
        //    if (Id == 0)
        //    {
        //        var dta1 = QuotationDAO.GetQuotationNo(branchid, fyearid, JobId);
        //        vm.QuotationNo = dta1.Split('-')[0];
        //        vm.Version = dta1.Split('-')[1];
        //        vm.QuotationDate = CommonFunctions.GetCurrentDateTime();



        //    }
        //    else
        //    {
        //        Quotation item = db.Quotations.Find(Id);
        //        vm.QuotationID = item.QuotationID;
        //        vm.QuotationNo = item.QuotationNo;
        //        vm.QuotationDate = item.QuotationDate;
        //        vm.MobileNumber = item.MobileNumber;
        //        vm.ContactPerson = item.ContactPerson;

        //        vm.Version = item.Version;
        //        vm.CurrencyId = item.CurrencyId;
        //        vm.Salutation = item.Salutation;
        //        vm.TermsandConditions = item.TermsandConditions;
        //        vm.PaymentTerms = item.PaymentTerms;
        //        vm.SubjectText = item.SubjectText;
        //        vm.QuotationValue = item.QuotationValue;
        //        vm.CustomerID = item.CustomerID;
        //        vm.Validity = item.Validity;
        //        vm.ClientDetail = item.ClientDetail;

        //    }
        //    return Json(new { QuotationId = Id, data = vm, message = "Data Found Succesfully!", status = "ok" }, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult QuotationDetailList()
        {
            ViewBag.Unit = db.ItemUnits.ToList();
            QuotationVM vm = new QuotationVM();
            return View(vm);
        }
        public ActionResult ShowQuotationDetailList(int QuotationId)
        {
            ViewBag.Unit = db.ItemUnits.ToList();
            List<QuotationDetailVM> vm = new List<QuotationDetailVM>();


            vm = (from c in db.QuotationDetails
                  where c.QuotationID == QuotationId
                  select new QuotationDetailVM { QuotationID = c.QuotationID, QuotationDetailId = c.QuotationDetailId, ItemDescription = c.ItemDescription, UOM = c.UOM, Quantity = c.Quantity, Rate = c.Rate, Value = c.Value, Remarks = c.Remarks }).ToList();
            return PartialView("QuotationDetailList", vm);
        }
      
        public ActionResult AddQuotationInventory(QuotationDetailVM invoice, string Details)
        {

            var IDetails = JsonConvert.DeserializeObject<List<QuotationDetailVM>>(Details);
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Unit = db.ItemUnits.ToList();
            List<QuotationDetailVM> list = new List<QuotationDetailVM>(); //(List<JobQuotationDetailVM>)Session["JQuotationDetail"];
            QuotationDetailVM item = new QuotationDetailVM();

            if (IDetails.Count > 0 && Details != "[{}]")
                list = IDetails;
            else
                list = new List<QuotationDetailVM>();

            item = new QuotationDetailVM();


            item.ItemDescription = invoice.ItemDescription;
            item.UOM = invoice.UOM;
            item.Quantity = invoice.Quantity;
            item.Value = invoice.Value;
            item.Rate = invoice.Rate;
            item.Remarks = invoice.Remarks;

            if (list == null)
            {
                list = new List<QuotationDetailVM>();
            }

            list.Add(item);

            Session["JQuotationDetail"] = list;
            QuotationVM vm = new QuotationVM();
            vm.QuotationDetails = list;
            return PartialView("QuotationDetailList", vm);
        }
        [HttpPost]
        public JsonResult DeleteQuotation(int id)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            int JobID = 0;
            Quotation obj = db.Quotations.Find(id);
            List<QuotationVM> list = new List<QuotationVM>();
            try
            {
                if (obj != null)
                {
                  
                    db.Quotations.Remove(obj);
                    db.SaveChanges();
                }
                
                
                return Json(new { message = "Quotation Deleted Succesfully!", status = "ok", data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, status = "Failed", data = list }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult QuotationPrint(int id)
        {
            ViewBag.ReportName = "Quotation Printing";
            var Quotation = db.Quotations.Find(id);
            var customer = db.CustomerMasters.Find(Quotation.CustomerID);
            ViewBag.CustomerEmailId = customer.Email;
            ViewBag.CustomerName = customer.ContactPerson;
            QuotationDAO.QuotationReport(id);
            
            return View();
        }
        public List<QuotationVM> BindQuotations(int JobID)
        {
            List<QuotationVM> List = new List<QuotationVM>();

            List = (from c in db.Quotations join d in db.CurrencyMasters on c.CurrencyId equals d.CurrencyID select new QuotationVM { CustomerID = c.CustomerID, QuotationID = c.QuotationID, QuotationNo = c.QuotationNo, QuotationDate = c.QuotationDate, Version = c.Version, Validity = c.Validity, QuotationValue = c.QuotationValue, CurrencyName = d.CurrencyName }).ToList();

            return List;

        }
        #endregion
    }
}