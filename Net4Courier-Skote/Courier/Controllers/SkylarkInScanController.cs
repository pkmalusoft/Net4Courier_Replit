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
    public class SkylarkInScanController : Controller
    {

        SourceMastersModel objSourceMastersModel = new SourceMastersModel();

        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            SkylarkInscanSearch obj = (SkylarkInscanSearch)Session["SkylarkInscanSearch"];
            SkylarkInscanSearch model = new SkylarkInscanSearch();
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
                obj = new SkylarkInscanSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.BatchNo = "";
                Session["SkylarkInscanSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                
            }
            else
            {
                model = obj;
                                
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
            }
            Session["SkylarkInscanSearch"] = model;

            List<SkylarkInscanModel> lst = SkylarkDAO.SKylarkInScanList(yearid, branchid, model.FromDate.Date, model.ToDate,model.CollectedBy,model.CustomerID );
            model.Details = lst;
            ViewBag.Employee = db.EmployeeMasters.OrderBy(cc => cc.EmployeeName).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SkylarkInscanSearch obj)
        {
            Session["SkylarkInscanSearch"] = obj;
            return RedirectToAction("Index");
        }

       
        public ActionResult Create(int id = 0)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.Employee = db.EmployeeMasters.OrderBy(cc => cc.EmployeeName).ToList();
            SkylarkInscanModel vm = new SkylarkInscanModel();
            if (id == 0)
            {

            
                return View(vm);
            }
            else
            {
                 
                vm = new SkylarkInscanModel();
                vm = SkylarkDAO.SKylarkInScanByID(id,fyearid,branchid);
                if (vm.AWBBatchID > 0)
                    ViewBag.SaveEnable = false;
                else
                    ViewBag.SaveEnable = true;
                var detaillist = (from c in db.CMS_BulkInscanMobileDetail where c.BatchID==id select c ).ToList();

                vm.Details = detaillist; 
                return View(vm);
            }


        }

       
        [HttpPost]
        public JsonResult SaveShipment(int ID,int ReceivedBy,string ReceivedDate)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AcJournalMaster ajm = new AcJournalMaster();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            try
            {                
                string Result=SkylarkDAO.SaveBulkInScan(ID,ReceivedBy,Convert.ToDateTime(ReceivedDate));
                if (Result=="OK")
                    return Json(new { status = "OK",  ID = ID, message = "Shipment Added Succesfully!" });
                else
                    return Json(new { status = "Failed", ID = ID, message = Result});

            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", CreditNoteID = 1, message = ex.Message });
            }

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
     
      


       
        
      
       

 
        public JsonResult CreditNoteVoucher(int id)
        {
            string reportpath = "";
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GenerateCreditNoteVoucherPrint(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

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
