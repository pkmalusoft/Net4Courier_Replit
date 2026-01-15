using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using Newtonsoft.Json;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class SkylarkReturntoConsignorController : Controller
    {

        SourceMastersModel objSourceMastersModel = new SourceMastersModel();

        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            SkylarkReturntoConsignorSearch obj = (SkylarkReturntoConsignorSearch)Session["SkylarkReturntoConsignorSearch"];
            SkylarkReturntoConsignorSearch model = new SkylarkReturntoConsignorSearch();
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
                obj = new SkylarkReturntoConsignorSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
               
                Session["SkylarkReturntoConsignorSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                
            }
            else
            {
                model = obj;
                                
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
            }
            Session["SkylarkReturntoConsignorSearch"] = model;
            List<SkylarkReturntoConsignorAWB> lst = SkylarkDAO.SKylarkReturntoConsignorList(yearid, branchid, model.FromDate.Date, model.ToDate);
            model.Details = lst;
            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.Employee = db.EmployeeMasters.OrderBy(cc => cc.EmployeeName).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SkylarkReturntoConsignorSearch obj)
        {
            Session["SkylarkReturntoConsignorSearch"] = obj;
            return RedirectToAction("Index");
        }

        public JsonResult GetActiveStatus(int InScanID)
        {
            string status = "ok";
            ChangeStatus _cstatus = new ChangeStatus();
            try
            {
                InScanMaster _inscan = db.InScanMasters.Find(InScanID);

                _cstatus.AWBNo = _inscan.AWBNo;
                _cstatus.InScanID = _inscan.InScanID;
                if (_inscan.StatusTypeId == null)
                    _cstatus.StatusTypeID = 1;
                else
                    _cstatus.StatusTypeID = Convert.ToInt32(_inscan.StatusTypeId);

                _cstatus.CourierStatusID = Convert.ToInt32(_inscan.CourierStatusID);
                if (_cstatus.CourierStatusID == 13)
                {
                    AWBTrackStatu aWBTrackStatu = db.AWBTrackStatus.Where(cc => cc.InScanId == InScanID && cc.CourierStatusId == _inscan.CourierStatusID).FirstOrDefault();
                    _cstatus.ShipmentReturned = aWBTrackStatu.ShipmentReturned;
                    _cstatus.Remarks = aWBTrackStatu.Remarks;
                }
                var courierstatus = db.CourierStatus.Find(_cstatus.CourierStatusID);
                _cstatus.CourierStatusText = courierstatus.StatusType + " - " + courierstatus.CourierStatus;


            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = _cstatus, result = status }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult SaveStatus(ChangeStatus v)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            string status = "ok";
            //   string result = "ok";
            string message = "";
            string statusname = "";
            bool statuschangepersmission = true;
            try
            {

                SkylarkDAO.SaveReturnedtoConsignor(v.InScanID, v.AWBNo, v.EmpID, uid, v.Remarks, CommonFunctions.GetBranchDateTime());

                
                status = "ok";
                message = "Status Updated";

            }

            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message;
            }

            //return Json(new { result=result,statustext=status } , JsonRequestBehavior.AllowGet);
            return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
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









        public class ChangeStatus
        {
            public int InboundShipmentID { get; set; }
            public int InScanID { get; set; }
            public int StatusTypeID { get; set; }
            public int CourierStatusID { get; set; }
            public int EmpID { get; set; }

            public string AWBNo { get; set; }
            public int ReturnDate { get; set; }
            public string CourierStatusText { get; set; }
            public string Remarks { get; set; }
            public bool ShipmentReturned { get; set; }
            

        }
    }
}
