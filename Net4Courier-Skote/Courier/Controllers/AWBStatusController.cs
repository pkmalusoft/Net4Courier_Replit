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
    public class AWBStatusController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
                        
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            List<QuickAWBVM> lst = PickupRequestDAO.GetAWBConflicts(yearid, branchid);
            
            return View(lst);


        }
        [HttpPost]
        public ActionResult Index(InScanSearch obj)
        {
            Session["InScanSearch"] = obj;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult RefresAWBStatus()
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            StatusModel _model = PickupRequestDAO.RefreshAWBConflicts(FyearId, branchid);
            return Json(new { status = _model.Status ,Message = "AWB Status Checked" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAWBInfo(string awbno)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            var list= AWBDAO.GetAWBInformation(awbno, branchid, FyearId);
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return Json(new { status = "ok", data = list, message = "Data Found" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "Failed", data = list, message = "AWB Not Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = "Failed", data = list, message = "Data Not Found" }, JsonRequestBehavior.AllowGet);
            }
             
        }

        public ActionResult Create()
        {
            
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            AWBStatusUpdate vm = new AWBStatusUpdate();
            ViewBag.CourierStatus = db.CourierStatus.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            
            ViewBag.Vehicle = db.VehicleMasters.ToList();
            ViewBag.Title = "AWB Status Update";
            DateTime pFromDate = CommonFunctions.GetCurrentDateTime();
            vm.EntryDate = pFromDate;
            vm.AWB = new AWBInfor();
            var employee = db.EmployeeMasters.Where(cc => cc.UserID == userid).First();
            if (employee!=null)
            {
                vm.EmployeeID = employee.EmployeeID;
            }
            return View(vm);

        }

        [HttpPost]
        public JsonResult SaveStatus(int EmployeeID,DateTime EntryDate,int CourierStatusId,string Remarks, string Details)
        {
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBInfor>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                string xml = dt.GetXml();
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                int userid = Convert.ToInt32(Session["UserID"]);
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                if (Session["UserID"] != null)
                {
                    SaveStatusModel result = new SaveStatusModel();
                    result=AWBDAO.SaveMultipleAWBStatus(EmployeeID, EntryDate, CourierStatusId,Remarks, branchid, userid, FyearId, xml);

                    return Json(new { status = "OK", message = result.Message }, JsonRequestBehavior.AllowGet);
                    
                }
                else
                {
                    return Json(new { status = "Failed", message = "Save Failed" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
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
            ViewBag.Title = "POD";
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
                                pod.InScanID = item.InScanID;
                            else
                                pod.ShipmentDetailId = item.ShipmentDetailID;
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
                                ImportShipmentDetail _inscan = db.ImportShipmentDetails.Find(item.ShipmentDetailID);

                                int importcourierstatusid = Convert.ToInt32(_inscan.CourierStatusID);

                                _inscan.PODID = pod.PODID;
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
                            if (item.InScanID > 0)
                                awbtrack.InScanId = item.InScanID;
                            else
                                awbtrack.ShipmentDetailID = item.ShipmentDetailID;
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
                    var exists = (from c in details join c1 in details1 on c.InScanID equals c1.InScanID select c1);
                    foreach(var item in exists)
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