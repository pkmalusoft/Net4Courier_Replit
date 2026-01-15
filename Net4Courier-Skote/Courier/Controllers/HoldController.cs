using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;
//using OfficeOpenXml.FormulaParsing.LexicalAnalysis.TokenSeparatorHandlers;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class HoldController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());

            HoldSearch obj = (HoldSearch)Session["HoldSearch"];
            HoldSearch model = new HoldSearch();
            List<HoldVM> lst = new List<HoldVM>();
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate; 
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth();
                obj = new HoldSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.ActionType = 0;
                Session["HoldSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.ActionType =0;
                model.Details = new List<HoldVM>();
            }
            else
            {
                model = obj;
                lst = PickupRequestDAO.GetHoldAWBList(obj.FromDate, obj.ToDate,branchid,obj.ActionType);
                model.Details = lst;

            }
            var transtypes = new SelectList(new[]
                                       {
                                            new { ID = "0", trans = "All" },
                                            new { ID = "1", trans = "On-Hold" },
                                            new { ID = "2", trans = "Released" }
                                        }, "ID", "trans", 1);
            ViewBag.StatusType = transtypes;
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(HoldSearch obj)
        {
            Session["HoldSearch"] = obj;
            return RedirectToAction("Index");
        }

        public ActionResult Create(int id=0)
        {
            ViewBag.HeldBy = db.EmployeeMasters.ToList();
            HoldVM vm = new HoldVM();
            if (id>0)
            {
                HoldRelease hold = db.HoldReleases.Find(id);
                int InscanId = 0;
                int ShipmentDetailId = 0;
                if (hold.InScanId > 0)
                    InscanId = hold.InScanId;
                else if(hold.ShipmentDetailId!=null)
                {
                    ShipmentDetailId = Convert.ToInt32(hold.ShipmentDetailId);
                }
                vm = PickupRequestDAO.GetAWBDetailsForHold("", InscanId, ShipmentDetailId);
                vm.HoldReleaseID = hold.HoldReleaseid;
                vm.EntryDate = hold.EntryDate;
                vm.EmployeeID = hold.EmployeeId;
                vm.Remarks = hold.Remarks;
                vm.ActionType = hold.ActionType;
                vm.CourierStatus = db.CourierStatus.Find(hold.CourierStatusId).CourierStatus;
                vm.InScanID = InscanId;
                vm.ShipmentDetailID = ShipmentDetailId;
                vm.Action = "Hold/Release Air WayBill- Modify";
                vm.HistoryDetails = PickupRequestDAO.GetHoldHistoryList("", vm.InScanID, vm.ShipmentDetailID);

            }
            else
            {
                vm.Action = "Create";
                vm.HoldReleaseID = 0;
                vm.InScanID = 0;
                vm.ShipmentDetailID = 0;
                vm.EntryDate = CommonFunctions.GetCurrentDateTime();
                ViewBag.Title = "Hold/Release Air WayBill";
                vm.HistoryDetails = new List<HoldVM>();
            }
            
            return View(vm);

        }
        
        [HttpPost]
        public ActionResult Create(HoldVM item)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            if (item.HoldReleaseID==0)
            {                

                HoldRelease v = new HoldRelease();
                v.InScanId = item.InScanID;
                v.EntryDate = item.EntryDate;
                v.EmployeeId = item.EmployeeID;
                v.Remarks = item.Remarks;
                v.ActionType = item.ActionType;
                v.BranchId = branchid;
                v.ModifiedBy = UserId;
                v.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                v.CreatedBy = UserId;
                v.CreatedDate = CommonFunctions.GetCurrentDateTime();
                v.ShipmentDetailId = item.ShipmentDetailID;
                v.CourierStatusId = item.CourierStatusID;

                db.HoldReleases.Add(v);
                db.SaveChanges();
                int InscanId = 0;
                int ShipmentDetailId = 0;
                string AWBNo = "";
                if (item.ShipmentDetailID > 0)
                    ShipmentDetailId = item.ShipmentDetailID;

                if (item.InScanID>0)
                {
                    InscanId = item.InScanID;
                }
                int StatusTypeId = 8;
                int CourierStatusId = 18;
                if (item.ActionType == "On-Hold")
                    CourierStatusId = 18;
                else
                    CourierStatusId = 19;

                //updating status to inscan
                if (item.InScanID > 0)
                {
                    
                    var inscan = db.InScanMasters.Where(itm => itm.InScanID == item.InScanID).FirstOrDefault();
                    InscanId = item.InScanID;
                    AWBNo = inscan.AWBNo;
                    inscan.StatusTypeId = StatusTypeId; // HOLD db.tblStatusTypes.Where(cc => cc.Name == "HOLD").First().ID;
                    inscan.CourierStatusID = CourierStatusId; //OnHold  db.CourierStatus.Where(cc => cc.StatusTypeID == inscan.StatusTypeId && cc.CourierStatus == "OnHold").FirstOrDefault().CourierStatusID;
                    db.Entry(inscan).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (item.ShipmentDetailID> 0)
                {

                    var shipment = db.ImportShipmentDetails.Find(ShipmentDetailId); //.FirstOrDefault();
                    ShipmentDetailId = shipment.ShipmentDetailID;
                    AWBNo = shipment.AWB;
                    shipment.StatusTypeId = StatusTypeId; // HOLD db.tblStatusTypes.Where(cc => cc.Name == "HOLD").First().ID;
                    shipment.CourierStatusID = CourierStatusId; //OnHold  db.CourierStatus.Where(cc => cc.StatusTypeID == inscan.StatusTypeId && cc.CourierStatus == "OnHold").FirstOrDefault().CourierStatusID;
                    db.Entry(shipment).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //updateing awbstaus table for tracking
                AWBTrackStatu _awbstatus = new AWBTrackStatu();

                
                _awbstatus.AWBNo = AWBNo;
                string companyname = Session["CompanyName"].ToString();
                        if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                            _awbstatus.EntryDate = DateTime.UtcNow; // DateTime.Now;
                        else
                          _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();

                _awbstatus.InScanId = InscanId;
                _awbstatus.ShipmentDetailID = ShipmentDetailId;
                _awbstatus.StatusTypeId = StatusTypeId;
                _awbstatus.CourierStatusId = CourierStatusId;
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusId).CourierStatus;
                _awbstatus.UserId = UserId;
                _awbstatus.EmpID = item.EmployeeID;
                _awbstatus.ShipmentDetailID = ShipmentDetailId;
                if (ShipmentDetailId>0)
                    _awbstatus.APIStatus = false;
                else
                    _awbstatus.APIStatus = true;

                db.AWBTrackStatus.Add(_awbstatus);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added AWB to Hold Status";
            }
            else
            {
                HoldRelease vm = db.HoldReleases.Find(item.HoldReleaseID);
                vm.Remarks = item.Remarks;
                vm.EmployeeId = item.EmployeeID;
                db.Entry(vm).State = EntityState.Modified;
                db.SaveChanges();

            }

            
            
            return RedirectToAction("Index");


        }
              


        [HttpPost]

        public JsonResult SaveReleaseStatus(RealeseHoldVM item)
        {                        
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            try
            {
                var inscan = db.InScanMasters.Where(itm => itm.InScanID == item.InScanID).FirstOrDefault();


                HoldRelease v = new HoldRelease();
                v.InScanId = item.InScanID;
                v.EntryDate = item.ReleaseOn;
                v.EmployeeId = item.ReleaseBy;
                v.Remarks = item.ReleaseReason;
                v.ActionType = "Release";
                v.BranchId = branchid;
                db.HoldReleases.Add(v);
                db.SaveChanges();

                //updating status to inscan
                inscan.StatusTypeId = db.tblStatusTypes.Where(cc => cc.Name == "HOLD").First().ID;
                inscan.CourierStatusID = db.CourierStatus.Where(cc => cc.StatusTypeID == inscan.StatusTypeId && cc.CourierStatus == "Released").FirstOrDefault().CourierStatusID;
                // db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();

                //updateing awbstaus table for tracking
                AWBTrackStatu _awbstatus = new AWBTrackStatu();
                int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                if (id == null)
                    id = 1;
                else
                    id = id + 1;

                _awbstatus.AWBTrackStatusId = Convert.ToInt32(id);
                _awbstatus.AWBNo = inscan.AWBNo;
                _awbstatus.EntryDate = DateTime.Now;
                _awbstatus.InScanId = inscan.InScanID;
                _awbstatus.StatusTypeId = Convert.ToInt32(inscan.StatusTypeId);
                _awbstatus.CourierStatusId = Convert.ToInt32(inscan.CourierStatusID);
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(inscan.StatusTypeId).Name;
                _awbstatus.CourierStatus = db.CourierStatus.Find(inscan.CourierStatusID).CourierStatus;
                _awbstatus.UserId = UserId;

                db.AWBTrackStatus.Add(_awbstatus);
                db.SaveChanges();
                return Json(new { status = "ok", message = "AWB Item Released Successfully!" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
                        


        }


        public JsonResult GetAWBData(string id)
        {
            //Received at Origin Facility
            //var l = (from c in db.InScans where c.InScanDate >= s  && c.InScanDate <= e select c).ToList();
            //int courierstatusid = db.CourierStatus.Where(cc => cc.CourierStatus == "Received at Origin Facility").FirstOrDefault().CourierStatusID;

            // var l = (from c in db.InScanMasters where c.AWBNo == id && c.DRSID == null && c.CourierStatusID == courierstatusid select c).FirstOrDefault();
            HoldVM obj = new HoldVM();
            obj = PickupRequestDAO.GetAWBDetailsForHold(id,0,0);
            if (obj != null && obj.AWBNo!="" && obj.AWBNo!=null)
            {
 
                Session["HoldAWBHistory"] = obj.HistoryDetails;
                return Json(new { status = "ok", data = obj, message = "AWB Data Found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Session["HoldAWBHistory"] = new List<HoldVM>();
                return Json(new { status = "failed", data = obj, message = "Invalid AWB -Shipment not in the Depot" }, JsonRequestBehavior.AllowGet);
            }
            //return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowItemList()
        {
            HoldVM vm = new HoldVM();
            vm.HistoryDetails = (List<HoldVM>)Session["HoldAWBHistory"];
            return PartialView("ItemList", vm);
        }

    }
}
