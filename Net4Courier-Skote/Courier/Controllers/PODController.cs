using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using Newtonsoft.Json;
using System.Reflection;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class PODController : Controller
    {
        Entities1 db = new Entities1();

        // GET: POD
        [HttpGet]
        public ActionResult Index(int id = 0)
        {
            PODSearch obj = (PODSearch)Session["PODSearch"];
            PODSearch model = new PODSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                
                obj = new PODSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;                
                Session["PODSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                
            }
            else
            {
                model = obj;
            }

            //List<PODVM> lst = (from p in db.PODs join c1 in db.InScanMasters on p.InScanID equals c1.InScanID                                     
            //                    join c2 in db.CourierStatus on p.CourierStatusID equals  c2.CourierStatusID
            //                        where c1.BranchID == branchid && c1.DepotID == depotId
            //                        //&& c.AcFinancialYearID==yearid                                
            //                        && (p.DeliveredDate >= obj.FromDate && p.DeliveredDate < obj.ToDate)                                                                         
            //                        orderby p.DeliveredDate descending
            //                        select new PODVM {PODID=p.PODID,CourierStatus=c2.CourierStatus, AWBNo = c1.AWBNo, Shipper = c1.Consignor,Consignee=c1.Consignee,ReceiverName= p.ReceiverName,InScanID= c1.InScanID }).ToList();  //, requestsource=subpet3.RequestTypeName 
            model.Details = PickupRequestDAO.GetPODList(obj.FromDate, obj.ToDate, yearid, branchid);
            return View(model);

        }

        [HttpPost]
        public ActionResult Index(PODSearch obj)
        {
            Session["PODSearch"] = obj;
            return RedirectToAction("Index");
        }


        [HttpGet]
        public JsonResult GetAWBInfo(string awbno)
        {            
            var awb = (from c in db.InScanMasters where c.IsDeleted == false && c.AWBNo == awbno
                       select new PODVM {ShipmentDetailId=0, AWBDate=c.TransactionDate, InScanID = c.InScanID, AWBNo = c.AWBNo,
                           Shipper = c.Consignor, Consignee = c.Consignee, ConsigneeContact = c.ConsigneeContact, CourierStatusId=c.CourierStatusID,StatusTypeId=c.StatusTypeId }).FirstOrDefault();
            
            string pstatus = "Valid";
            if (awb != null)
            {
                if (awb.CourierStatusId != 8 && awb.CourierStatusId != 10) // && awb.CourierStatusId != 16) //outscan for delivery or  pending
                {
                    pstatus = "NotValid";
                    awb.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == awb.CourierStatusId).FirstOrDefault().CourierStatus;
                }
                else if (awb.CourierStatusId == 8 || awb.CourierStatusId == 10) // || awb.CourierStatusId == 16) //outscan for delivery or  pending
                {
                    awb.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == awb.CourierStatusId).FirstOrDefault().CourierStatus;
                    return Json(new { Status = pstatus, data = awb }, JsonRequestBehavior.AllowGet);
                }
                if (awb.CourierStatusId==16) //reado to export forward to agent
                {
                    //checking Imported Items
                    var importlst1 = (from c in db.InboundShipments    
                                       
                                      where c.AWBNo == awbno //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
                                      select new PODVM { InScanID = 0, ShipmentDetailId = c.ShipmentID, AWBNo = c.AWBNo, Shipper = c.Consignor, Consignee = c.Consignee, ConsigneeContact = c.ConsigneeContact, CourierStatusId = c.CourierStatusID, StatusTypeId = c.StatusTypeId, AWBDate = c.AWBDate }).FirstOrDefault();
                    //var importlst1 = (from c in db.ImportShipmentDetails
                    //                  join i in db.ImportShipments on c.ImportID equals i.ID
                    //                  where c.AWB == awbno //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
                    //                  select new PODVM { InScanID = 0, ShipmentDetailId = c.ShipmentDetailID, AWBNo = c.AWB, Shipper = c.Shipper, Consignee = c.Receiver, ConsigneeContact = c.ReceiverContact, CourierStatusId = c.CourierStatusID, StatusTypeId = c.StatusTypeId , AWBDate = c.AWBDate }).FirstOrDefault();
                    if (importlst1 != null)
                    {
                        if (importlst1.CourierStatusId != 8 && importlst1.CourierStatusId != 10) //outscan for delivery or  pending
                        {
                            pstatus = "NotValid";
                            importlst1.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == importlst1.CourierStatusId).FirstOrDefault().CourierStatus;
                            return Json(new { Status = pstatus, data = importlst1 }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            pstatus = "Valid";
                            return Json(new { Status = pstatus, data = importlst1 }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        pstatus = "NotValid";
                        return Json(new { Status = pstatus, data = awb }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    pstatus = "NotValid";
                    return Json(new { Status = pstatus, data = awb }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                pstatus = "Valid";
                var importlst1 = (from c in db.InboundShipments

                                  where c.AWBNo == awbno //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
                                  select new PODVM { InScanID = 0, ShipmentDetailId = c.ShipmentID, AWBNo = c.AWBNo, Shipper = c.Consignor, Consignee = c.Consignee, ConsigneeContact = c.ConsigneeContact, CourierStatusId = c.CourierStatusID, StatusTypeId = c.StatusTypeId, AWBDate = c.AWBDate }).FirstOrDefault();
                //checking Imported Items
                //var importlst1 = (from c in db.ImportShipmentDetails
                //                  join i in db.ImportShipments on c.ImportID equals i.ID
                //                  where c.AWB == awbno //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
                //                  select new PODVM { InScanID = 0, ShipmentDetailId = c.ShipmentDetailID, AWBNo = c.AWB, Shipper=c.Shipper,Consignee=c.Receiver,ConsigneeContact=c.ReceiverContact, CourierStatusId = c.CourierStatusID ,StatusTypeId=c.StatusTypeId }).FirstOrDefault();
                if (importlst1 != null)
                {
                    if (importlst1.CourierStatusId != 8 && importlst1.CourierStatusId != 10) //outscan for delivery or  pending
                    {
                        pstatus = "NotValid";
                        return Json(new { Status = pstatus, data = importlst1 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Status = pstatus, data = importlst1 }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    pstatus = "NotAvailabel";
                    
                    return Json(new { Status = pstatus }, JsonRequestBehavior.AllowGet);
                }
            }
           

        }

        public ActionResult Create()
        {
            
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            PODVM vm = new PODVM();            
            
            ViewBag.Employee = db.EmployeeMasters.ToList();
            
            ViewBag.Vehicle = db.VehicleMasters.ToList();
            ViewBag.Title = "POD - Create";
            DateTime pFromDate = CommonFunctions.GetCurrentDateTime();
            vm.DeliveredDate = pFromDate;
            vm.Delivered = true;
            vm.DelieveryAttemptDate = pFromDate;
            vm.AWBDate = pFromDate;
            return View(vm);

        }

        [HttpPost]
        public string SaveBatchPOD(string Details)
        {
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<PODAWBDetail>>(Details);
                //DataTable ds = new DataTable();
                //DataSet dt = new DataSet();
                //dt = ToDataTable(IDetails);
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                int userid = Convert.ToInt32(Session["UserID"]);
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                if (Session["UserID"] != null)
                {

                    var empid = 0; // db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault();
                    foreach (PODAWBDetail item in IDetails)
                    {
                        if (item.Delivered)
                        {
                            POD pod = new POD();
                            int max = (from d in db.PODs orderby d.PODID descending select d.PODID).FirstOrDefault();
                            if (max == null)
                                max = 1;
                            else
                                max = max + 1;
                            pod.PODID = max;
                            if (item.InScanID > 0)
                                pod.InScanID = item.InScanID;
                            else
                            {
                                //pod.ShipmentDetailId = item.ShipmentDetailId;
                                pod.InboundShipmentID = item.ShipmentDetailId;
                            }
                            pod.ReceiverName = item.ReceiverName;
                            pod.ReceivedTime = Convert.ToDateTime(item.ReceivedTime1); //  item.DeliveredDate;
                            pod.DeliveredBy = item.DeliveredBy;
                            pod.DeliveredDate = item.DeliveredDate;
                            pod.UpdationDate = CommonFunctions.GetCurrentDateTime();
                            pod.CourierStatusID = 13;
                            pod.IsSkyLarkUpdate = false;
                            pod.DeliveryLocation = "";
                            pod.BatchId = 0;
                            pod.BranchId = branchid;
                            pod.CreatedBy = userid;
                            pod.Remarks = item.Remarks;
                            pod.CreatedDate = CommonFunctions.GetCurrentDateTime();
                            db.PODs.Add(pod);
                            db.SaveChanges();
                            empid = Convert.ToInt32(pod.DeliveredBy);
                            if (item.InScanID == null)
                                item.InScanID = 0;
                            if (item.InScanID > 0)
                            {
                                var inscan = db.InScanMasters.Find(item.InScanID);
                                inscan.StatusTypeId = 4;
                                inscan.CourierStatusID = 13;
                                inscan.PODID = pod.PODID;
                                db.Entry(inscan).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else if (item.ShipmentDetailId>0)
                            {
                                InboundShipment _inscan = db.InboundShipments.Find(item.ShipmentDetailId);

                                int importcourierstatusid = Convert.ToInt32(_inscan.CourierStatusID);

                                _inscan.POD = pod.PODID;
                                _inscan.StatusTypeId = 4;//
                                _inscan.CourierStatusID = 13; //Delivered                                
                                db.Entry(_inscan).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            AWBTrackStatu awbtrack = new AWBTrackStatu();
                            int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                            if (id == null)
                                id = 1;
                            else
                                id = id + 1;

                            awbtrack.AWBTrackStatusId = Convert.ToInt32(id);
                            if (item.InScanID>0)
                                awbtrack.InScanId = item.InScanID;
                            else
                            awbtrack.InboundShipmentID = item.ShipmentDetailId;
                            awbtrack.AWBNo = item.AWBNo;
                            if (item.ShipmentDetailId>0)
                            {
                                awbtrack.APIStatus = false;
                            }
                            awbtrack.StatusTypeId = 4; //Delivered
                            awbtrack.CourierStatusId = 13; //Shipment Delivered
                            awbtrack.ShipmentStatus = db.tblStatusTypes.Find(awbtrack.StatusTypeId).Name;
                            awbtrack.CourierStatus = db.CourierStatus.Find(awbtrack.CourierStatusId).CourierStatus;

                            awbtrack.EntryDate =Convert.ToDateTime(item.DeliveredDate);
                            awbtrack.UserId = Convert.ToInt32(Session["UserID"].ToString());
                            awbtrack.PODID = pod.PODID;
                            awbtrack.Remarks = item.Remarks; // + ", Received by " + pod.ReceiverName;
                            awbtrack.EmpID = empid;
                            db.AWBTrackStatus.Add(awbtrack);
                            db.SaveChanges();
                        }
                        else if(item.DeliveryAttempted)
                        {
                            AWBTrackStatu awbtrack = new AWBTrackStatu();
                            int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                            if (id == null)
                                id = 1;
                            else
                                id = id + 1;

                            awbtrack.AWBTrackStatusId = Convert.ToInt32(id);
                            awbtrack.InScanId = item.InScanID;
                            awbtrack.AWBNo = item.AWBNo;
                            awbtrack.StatusTypeId = 3; //Depot Outscan
                            awbtrack.CourierStatusId =10; //9	Attempted To Deliver -- Delivery Pending  Depot Outscan 10 
                            awbtrack.ShipmentStatus = db.tblStatusTypes.Find(awbtrack.StatusTypeId).Name;
                            awbtrack.CourierStatus = db.CourierStatus.Find(awbtrack.CourierStatusId).CourierStatus;

                            awbtrack.EntryDate = Convert.ToDateTime(item.DelieveryAttemptDate);
                            awbtrack.UserId = Convert.ToInt32(Session["UserID"].ToString());
                            awbtrack.EmpID = empid;
                            awbtrack.Remarks = item.Remarks;
                            db.AWBTrackStatus.Add(awbtrack);
                            db.SaveChanges();
                        }
                    }

                    //string result = AWBDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId, xml);
                    return "ok";
                }
                else
                {
                    return "Failed!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string AddAWBTrackStatus(int inscanid, int EMPID, int pStatusTypeId = 0, int pCourierStatusId = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            var inscan = db.InScanMasters.Where(itm => itm.InScanID == inscanid).FirstOrDefault();
            var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == inscanid).OrderByDescending(cc => cc.EntryDate).FirstOrDefault();
            if (awbtrack != null)
            {
                if (awbtrack.CourierStatusId == inscan.CourierStatusID)
                {
                    return "same status";
                }
            }

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

            if (pStatusTypeId > 0)
            {
                _awbstatus.StatusTypeId = pStatusTypeId;
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(pStatusTypeId).Name;
            }
            else
            {
                _awbstatus.StatusTypeId = Convert.ToInt32(inscan.StatusTypeId);
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(inscan.StatusTypeId).Name;
            }

            if (pCourierStatusId > 0)
            {
                _awbstatus.CourierStatusId = pCourierStatusId;
                _awbstatus.CourierStatus = db.CourierStatus.Find(pCourierStatusId).CourierStatus;
            }
            else
            {
                _awbstatus.CourierStatusId = Convert.ToInt32(inscan.CourierStatusID);
                _awbstatus.CourierStatus = db.CourierStatus.Find(inscan.CourierStatusID).CourierStatus;
            }


            _awbstatus.UserId = uid;
            _awbstatus.EmpID = EMPID;

            db.AWBTrackStatus.Add(_awbstatus);
            db.SaveChanges();
            return "ok";
        }


        public ActionResult CreateBulk()
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            // ViewBag.vehicle = db.VehicleMasters.ToList();
            ViewBag.Vehicles = (from c in db.VehicleMasters select new { VehicleID = c.VehicleID, VehicleName = c.RegistrationNo + "-" + c.VehicleDescription }).ToList();
            ViewBag.Checkedby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            PODSearch obj = (PODSearch)Session["PODCreateSearch"];
            PODSearch vm = new PODSearch();
            List<PODVM> details = new List<PODVM>();
            List<DRSDet> details1 = new List<DRSDet>();
            if (obj == null)
            {
                vm.FromDate = CommonFunctions.GetCurrentDateTime();
                vm.ToDate = CommonFunctions.GetCurrentDateTime();
            }
            else
            {
                vm.FromDate = obj.FromDate;
                vm.ToDate = obj.ToDate;
            }
            ViewBag.Title = "CREATE";
            vm.Details = details;
            vm.Details1 = details1;
            vm.DRSNo = "";
            vm.DRSID = 0;
            Session["PODList"] = details1;
            return View(vm);
        }


        [HttpPost]
        public ActionResult CreateBulk(PODSearch model)
        {
            try
            {
                Session["PODCreateSearch"] = model;


                int FyearId = Convert.ToInt32(Session["fyearid"]);
                int userid = Convert.ToInt32(Session["UserID"]);
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                if (Session["UserID"] != null)
                {

                    var empid = 0; // db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault();
                    foreach (DRSDet item in model.Details1)
                    {
                         
                            POD pod = new POD();
                            int max = (from d in db.PODs orderby d.PODID descending select d.PODID).FirstOrDefault();
                            if (max == null)
                                max = 1;
                            else
                                max = max + 1;
                            pod.PODID = max;
                        if (item.InScanID > 0)
                        {
                            pod.InScanID = item.InScanID;
                        }
                        else
                        {
                            //pod.ShipmentDetailId = item.ShipmentDetailID;
                            pod.InboundShipmentID = item.ShipmentDetailID;
                        }
                            pod.ReceiverName = item.consignee;
                            pod.ReceivedTime = Convert.ToDateTime(model.ToDate); //  item.DeliveredDate;
                            pod.DeliveredBy = model.DeliveredBy;// item.DeliveredBy;
                            pod.DeliveredDate = model.FromDate;
                            pod.UpdationDate = CommonFunctions.GetCurrentDateTime();
                            pod.CourierStatusID = 13;
                            pod.IsSkyLarkUpdate = false;
                            pod.DeliveryLocation = "";
                            pod.BatchId = 0;
                            pod.BranchId = branchid;
                            pod.Remarks = item.Remarks;
                            pod.CreatedBy = userid;
                            pod.CreatedDate = CommonFunctions.GetCurrentDateTime();
                            db.PODs.Add(pod);
                            db.SaveChanges();
                            empid = Convert.ToInt32(pod.DeliveredBy);
                            if (item.InScanID == null)
                                item.InScanID = 0;
                            if (item.InScanID > 0)
                            {
                                var inscan = db.InScanMasters.Find(item.InScanID);
                                inscan.StatusTypeId = 4;
                                inscan.CourierStatusID = 13;
                                inscan.PODID = pod.PODID;
                                db.Entry(inscan).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else if (item.ShipmentDetailID > 0)
                            {
                                InboundShipment _inscan = db.InboundShipments.Find(item.ShipmentDetailID);

                                int importcourierstatusid = Convert.ToInt32(_inscan.CourierStatusID);

                                _inscan.POD = pod.PODID;
                                _inscan.StatusTypeId = 4;//
                                _inscan.CourierStatusID = 13; //Delivered                                
                                db.Entry(_inscan).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            AWBTrackStatu awbtrack = new AWBTrackStatu();
                            //int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                            //if (id == null)
                            //    id = 1;
                            //else
                            //    id = id + 1;

                            //awbtrack.AWBTrackStatusId = Convert.ToInt32(id);
                            if (item.InScanID > 0)
                                awbtrack.InScanId = item.InScanID;
                            else
                                awbtrack.InboundShipmentID = item.ShipmentDetailID;
                            awbtrack.AWBNo = item.AWB;
                            if (item.ShipmentDetailID > 0)
                            {
                                awbtrack.APIStatus = false;
                            }
                            awbtrack.StatusTypeId = 4; //Delivered
                            awbtrack.CourierStatusId = 13; //Shipment Delivered
                            awbtrack.ShipmentStatus = db.tblStatusTypes.Find(awbtrack.StatusTypeId).Name;
                            awbtrack.CourierStatus = db.CourierStatus.Find(awbtrack.CourierStatusId).CourierStatus;

                            awbtrack.EntryDate = Convert.ToDateTime(model.DeliveredDate);
                            awbtrack.UserId = Convert.ToInt32(Session["UserID"].ToString());
                            awbtrack.PODID = pod.PODID;
                            awbtrack.EmpID = empid;
                            awbtrack.Remarks = pod.Remarks;
                            db.AWBTrackStatus.Add(awbtrack);
                            db.SaveChanges();
                      
                      
                    }

                    //string result = AWBDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId, xml);
                    TempData["SuccessMsg"] = "POD Saved Successfully";
                    return RedirectToAction("CreateBulk");
                }
                else
                {
                    return View(model);
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;
                return View(model);
            }
           
        }

        public ActionResult ShowPODList()
        {
            PODSearch vm = new PODSearch();
            vm.Details1 = (List<DRSDet>)Session["PODList"];
            return PartialView("ItemList", vm);
        }

        [HttpGet]
        public JsonResult GetDRSNo(string term,int DeliveredBy,string FromDate,string ToDate)
        {

             List<DRSVM> list = new List<DRSVM>();
            list = AWBDAO.GetPODDRSDetails(DeliveredBy,Convert.ToDateTime(FromDate),Convert.ToDateTime(ToDate),term);            
            return Json(list, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult GetAWBNo(string term, int DeliveredBy, string  FromDate, string ToDate,int DRSID=0)
        {

            List<QuickAWBVM> list = new List<QuickAWBVM>();
            list = AWBDAO.GetPODDRSAWBDetails(DeliveredBy,DRSID,Convert.ToDateTime(FromDate),Convert.ToDateTime(ToDate), term);
            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDRSAWB(string term, int DeliveredBy, DateTime FromDate, DateTime ToDate,int DRSID)
        {

            List<DRSDet> details = new List<DRSDet>();
            details = AWBDAO.GetDRSAWBDetails(DRSID);
            PODSearch vm = new PODSearch();
            vm.Details1 = (List<DRSDet>)Session["PODList"];
            vm.Details1.AddRange(details);
            Session["PODList"] = vm.Details;
            return PartialView("ItemList", vm);

        }
        public JsonResult GetAWBData1(string AWBNo, int DRSID,int DeliveredBy)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DRSVM vm = new DRSVM();

            List<DRSDet> details = (List<DRSDet>)Session["PODList"];

            List<DRSDet> details1 = AWBDAO.GetPODAWBDetails(branchid, DRSID,AWBNo, DeliveredBy);
            if (DRSID>0)
            {
                if (details.Count > 0)
                {
                    var exists = (from c in details join c1 in details1 on c.InScanID equals c1.InScanID where c1.InScanID>0 select c1);
                    foreach(var item in exists)
                    {
                        details1.Remove(item);
                    }


                    var exists1 = (from c in details join c1 in details1 on c.ShipmentDetailID equals c1.ShipmentDetailID where c1.ShipmentDetailID > 0 select c1);
                    foreach (var item in exists1)
                    {
                        details1.Remove(item);
                    }

                }
            }
            if (details != null)
                details.AddRange(details1);
            else
            {
                details = new List<DRSDet>();
                details.AddRange(details1);
            }

            Session["PODList"] = details;

            if (details1.Count > 0)
            {
                return Json(new { status = "ok", data = details1[0], message = "Data Found" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                DRSDet l = new DRSDet();
                return Json(new { status = "failed", data = l, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
            }
            //return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAWBData1(string id)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DRSVM vm = new DRSVM();

            List<DRSDet> details = (List<DRSDet>)Session["PODList"];
            if (details == null)
                details = new List<DRSDet>();

            int i = 0;
            foreach (var item in details)
            {
                if (item.AWB == id)
                {
                    details[i].deleted = true;
                    details[i].deletedclass = "hide";
                }
                i++;
            }
            Session["PODList"] = details;

            if (details.Count > 0)
            {
                return Json(new { status = "ok", data = details, message = "Data Found" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                DRSDet l = new DRSDet();
                return Json(new { status = "failed", data = details, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
            }
            //return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public static DataSet ToDataTable<T>(List<T> items)
        {
            DataSet ds = new DataSet();
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            ds.Tables.Add(dataTable);
            //put a breakpoint here and check datatable
            return ds;
        }
    }
}