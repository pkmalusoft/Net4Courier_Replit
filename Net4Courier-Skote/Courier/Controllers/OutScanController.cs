using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using System.IO;
using ExcelDataReader;
using System.Reflection;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class OutScanController : Controller
    {
        //
        // GET: /OutScan/

        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            OutScanSearch obj = (OutScanSearch)Session["OutScanSearch"];
            OutScanSearch model = new OutScanSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new OutScanSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                
                obj.DRSDetails = new List<DRSVM>();
                Session["OutScanSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.DRSDetails = new List<DRSVM>();
                obj.DRSDetails = new List<DRSVM>();
            }
            else
            {
                model = obj;
                StatusModel statu = AccountsDAO.CheckDateValidate(obj.FromDate.ToString(), yearid);
                string vdate = statu.ValidDate;
                model.FromDate = Convert.ToDateTime(vdate);
                model.ToDate = Convert.ToDateTime(AccountsDAO.CheckDateValidate(obj.ToDate.ToString(), yearid).ValidDate);
                Session["OutScanSearch"] =model;
                obj.DRSDetails = new List<DRSVM>();
            }
            List<DRSVM> lst = PickupRequestDAO.GetOutScanList(obj.FromDate, obj.ToDate, yearid, branchid, depotId);
            model.DRSDetails = lst;            
            return View(model);


        }
        [HttpPost]
        public ActionResult Index(OutScanSearch obj)
        {
            Session["OutScanSearch"] = obj;
            return RedirectToAction("Index");
        }
        public ActionResult Index1()
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            List<DRSVM> lst = (from c in db.DRS join e in db.EmployeeMasters on c.DeliveredBy equals e.EmployeeID join v in db.VehicleMasters on c.VehicleID equals v.VehicleID
                               where c.BranchID==BranchId && c.AcCompanyID == CompanyId
                               select new DRSVM {DRSID=c.DRSID,DRSNo=c.DRSNo,DRSDate=c.DRSDate,Deliver=e.EmployeeName,vehicle=v.VehicleNo ,TotalCourierCharge=c.TotalCourierCharge,TotalMaterialCost=c.TotalMaterialCost }).ToList();

            return View(lst);
        }



        public ActionResult Details(int id)
        {
            return View();
        }



        public ActionResult Create(int id=0)
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());  
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc=>cc.EmployeeName);
           // ViewBag.vehicle = db.VehicleMasters.ToList();
            ViewBag.Vehicles = (from c in db.VehicleMasters select new { VehicleID = c.VehicleID, VehicleName = c.RegistrationNo + "-" + c.VehicleDescription }).ToList();
            ViewBag.Checkedby = db.EmployeeMasters.ToList().OrderBy(cc=>cc.EmployeeName);
            DRSVM v = new DRSVM();
            if (id>0)
            {
                DR d = db.DRS.Find(id);
                if (d != null)
                {
                    v.DRSID = d.DRSID;
                    v.DRSNo = d.DRSNo;
                    v.DRSDate = d.DRSDate;
                    v.DeliveredBy = d.DeliveredBy;
                    //v.CheckedBy = d.CheckedBy;
                    v.TotalCourierCharge = d.TotalCourierCharge;
                    v.VehicleID = d.VehicleID;
                    v.StatusDRS = d.StatusDRS;
                    v.AcCompanyID = d.AcCompanyID;
                    v.StatusInbound = d.StatusInbound;
                    v.DrsType = d.DrsType;
                }
                var details = AWBDAO.GetDRSAWBDetails(v.DRSID);
                v.lst = details;
                ViewBag.EditMode = "true";
                ViewBag.Title = "Modify";
            }
            else
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                v.DRSID = 0;
                v.DRSDate = CommonFunctions.GetCurrentDateTime();
                v.DRSNo = _dao.GetMaxDRSNo(CompanyId, BranchId, v.DRSDate);
                ViewBag.EditMode = "false";
                ViewBag.Title = "Create";
                List<DRSDet> details = new List<DRSDet>();
                v.lst = details;
            }
            return View(v);
        }


   

        public JsonResult GetAWBData(string id)
        {
            //Received at Origin Facility
            //var l = (from c in db.InScans where c.InScanDate >= s  && c.InScanDate <= e select c).ToList();
            //int courierstatusid = db.CourierStatus.Where(cc => cc.CourierStatus == "Received at Origin Facility").FirstOrDefault().CourierStatusID;
            //int courierstatusid1 = db.CourierStatus.Where(cc => cc.CourierStatus == "Released").FirstOrDefault().CourierStatusID;

            //--Domestic Items 5,6,7,22,23,19 Received at origin,sorting inprocess,Airwaybill Updated,Outscan Returned, Partially Delivered,Released            
            ///var l = (from c in db.InScanMasters where c.AWBNo == id && ((c.DRSID!=null && (c.CourierStatusID==8 || c.CourierStatusID==9)) ||  ( c.DRSID==null && (c.CourierStatusID== 5 || c.CourierStatusID == 6 || c.CourierStatusID == 7 || c.CourierStatusID == 22 || c.CourierStatusID == 23 || c.CourierStatusID == 19)))  select c).FirstOrDefault();
            var l = (from c in db.InScanMasters where c.AWBNo == id && (((c.CourierStatusID == 8 || c.CourierStatusID == 9)) || ((c.CourierStatusID == 5 || c.CourierStatusID == 6 || c.CourierStatusID == 7 || c.CourierStatusID == 22 || c.CourierStatusID == 23 || c.CourierStatusID == 19))) select c).FirstOrDefault();

            if (l != null)
            {
                DRSDet obj = new DRSDet(); 
                if (l != null)
                {

                    obj.AWB = l.AWBNo;
                    obj.AWBDate = l.TransactionDate;
                    obj.ShipmentDetailID = 0;
                    obj.InScanID = l.InScanID;
                        obj.consignor = l.Consignor;
                    
                    obj.consignee = l.Consignee;
                    if (l.ConsigneeCityName != null)
                    {
                        obj.city = l.ConsigneeCityName.ToString();
                        obj.phone = l.ConsigneePhone;
                        obj.address = l.ConsigneeCountryName;
                    }
                    if (l.PaymentModeId == 2)
                    {
                        if (l.CourierCharge != null)
                            obj.COD = Convert.ToDecimal(l.NetTotal);// + Convert.ToDecimal(l.OtherCharge);
                        else
                            obj.COD = 0;
                    }
                    else
                    {
                        obj.COD = 0;
                    }
                    if (l.MaterialCost != null)
                        obj.MaterialCost = Convert.ToDecimal(l.MaterialCost);
                    else
                        obj.MaterialCost = 0;

                }
                var courierstatus = db.CourierStatus.Find(l.CourierStatusID);
                obj.CourierStatus = courierstatus.CourierStatus;
                return Json(new { status = "ok", data = obj,  message = "Data Found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //Import Shipment Items 21,22,23,19 --Received at delivery Factility,Outscan Returned,Partially Delivered,REleased
                var import = db.InboundShipments.Where(cc => cc.AWBNo == id.Trim() && (((cc.CourierStatusID == 8 || cc.CourierStatusID == 9)) || ((cc.CourierStatusID == 21 || cc.CourierStatusID == 22 || cc.CourierStatusID == 23 || cc.CourierStatusID == 19)))).FirstOrDefault();
                //var import = db.InboundShipments.Where(cc => cc.AWBNo == id.Trim() && ((cc.DRSID!=null && (cc.CourierStatusID==8 || cc.CourierStatusID==9)) || (cc.DRSID == null && (cc.CourierStatusID == 21 || cc.CourierStatusID == 22 || cc.CourierStatusID == 23 || cc.CourierStatusID == 19)))).FirstOrDefault();
                //var import = db.ImportShipmentDetails.Where(cc => cc.AWB == id.Trim() && cc.DRSID == null && (cc.CourierStatusID == 21 || cc.CourierStatusID == 22 || cc.CourierStatusID==23 || cc.CourierStatusID == 19)).FirstOrDefault();
                if (import != null)
                {
                    DRSDet obj = new DRSDet();
                    if (import != null)
                    {

                        obj.AWB = import.AWBNo;
                        obj.AWBDate = import.AWBDate;
                        obj.InScanID = 0;
                        obj.ShipmentDetailID = import.ShipmentID;
                        obj.consignor = import.Consignor; // Shipper;
                        obj.consignee = import.Consignee;
                        if (import.ConsigneeCityName!= null)
                        {
                            obj.city = import.ConsigneeCountryName.ToString();
                            obj.phone = import.ConsigneePhone;
                            obj.address = import.ConsigneeAddress1_Building + import.ConsigneeAddress2_Street + "," + import.ConsignorAddress3_PinCode;
                        }
                        if (import.TaxInvoiceID !=null)
                        {
                            var shipmentinvoice = db.ShipmentInvoiceDetails.Where(cc => cc.ShipmentImportDetailID == import.ShipmentID).FirstOrDefault();
                            if (shipmentinvoice!=null)                        
                                obj.COD = Convert.ToDecimal(shipmentinvoice.adminCharges) + Convert.ToDecimal(shipmentinvoice.Tax);
                            else
                                obj.COD = 0;                            
                        }
                        ////--for nice
                        //var shipmentinvoice = db.ShipmentInvoiceDetails.Where(cc => cc.ShipmentImportDetailID == import.ShipmentDetailID).FirstOrDefault();
                        //if (shipmentinvoice!=null)                        
                        //    obj.COD = Convert.ToDecimal(shipmentinvoice.adminCharges) + Convert.ToDecimal(shipmentinvoice.Tax);
                        //else
                        //    obj.COD = 0;

                        //if (import.CustomValue != null)
                        //    obj.MaterialCost= Convert.ToDecimal(import.);
                        //else
                        if (import.MaterialCost != null)
                        {
                            obj.MaterialCost = Convert.ToDecimal(import.MaterialCost);                           
                        }
                        else
                        {
                            obj.MaterialCost = 0;
                        }
                        var courierstatus = db.CourierStatus.Find(import.CourierStatusID);
                        obj.CourierStatus = courierstatus.CourierStatus;
                        

                    }

                    return Json(new { status = "ok", data = obj, message = "Data Found" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = "failed", data = l, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
                }
            }
            //return Json(obj, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDRSDetails(int id)
        {
            List<DRSDet> list = new List<DRSDet>();

            //var data = (from c in db.DRSDetails where c.DRSID == id select c).ToList();
            var lstawb = (from c in db.InScanMasters where c.DRSID == id select c).ToList();
            var lstawb1 = (from c in db.InboundShipments where c.DRSID == id select c).ToList();
            if (lstawb != null)
            {
                foreach (var item in lstawb)
                {
                    var l = (from c in db.InScanMasters where c.AWBNo == item.AWBNo select c).FirstOrDefault();
                    DRSDet obj = new DRSDet();
                    obj.AWB = l.AWBNo;
                    obj.ShipmentDetailID = 0;
                    obj.InScanID = l.InScanID;
                    obj.consignor = l.Consignor;
                    obj.consignee = l.Consignee;
                    if (l.ConsigneeCityName!=null)
                    obj.city = l.ConsigneeCityName.ToString();
                    if (l.ConsigneePhone!=null)
                    obj.phone = l.ConsigneePhone;
                    if (l.ConsigneeCountryName!=null)
                     obj.address = l.ConsigneeCountryName;
                    
                        if (l.PaymentModeId == 2)
                        {
                        if (l.CourierCharge != null)
                            obj.COD = Convert.ToDecimal(l.NetTotal);
                        else
                            obj.COD = 0;
                        }
                    else
                    {
                        obj.COD = 0;
                    }
                    if (l.MaterialCost != null)
                        obj.MaterialCost = Convert.ToDecimal(l.MaterialCost);
                    else
                        obj.MaterialCost = 0;
                    list.Add(obj);
                }
                
            }

            if (lstawb1 != null)
            {
                foreach (var item in lstawb1)
                {
                    var l = (from c in db.InboundShipments where c.AWBNo == item.AWBNo select c).FirstOrDefault();
                    DRSDet obj = new DRSDet();
                    obj.AWB = l.AWBNo;
                    obj.InScanID = 0;
                    obj.ShipmentDetailID = l.ShipmentID;
                    obj.consignor = l.Consignor;
                    obj.consignee = l.Consignee;
                    if (l.ConsigneeCityName != null)
                        obj.city = l.ConsigneeCityName.ToString();
                    if (l.ConsigneePhone != null)
                        obj.phone = l.ConsigneePhone;
                    if (l.ConsigneeCountryName != null)
                        obj.address = l.ConsigneeCountryName;

                    obj.COD = 0;
                    //var shipmentinvoice = db.ShipmentInvoiceDetails.Where(cc => cc.ShipmentImportDetailID == l.ShipmentDetailID).FirstOrDefault();
                    //if (shipmentinvoice != null)
                    //    obj.COD = Convert.ToDecimal(shipmentinvoice.adminCharges) + Convert.ToDecimal(shipmentinvoice.Tax);
                    //else
                    //    obj.COD = 0;

                    //if (import.CustomValue != null)
                    //    obj.MaterialCost= Convert.ToDecimal(import.);
                    //else
                    obj.MaterialCost = 0;
                    //if (l.COD != null)
                    //    obj.COD = Convert.ToDecimal(l.COD);
                    //if (l.CustomValue != null)
                    //    obj.MaterialCost = Convert.ToDecimal(l.CustomValue);
                    //else
                    //    obj.MaterialCost = 0;
                    list.Add(obj);
                }

            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(DRSVM v)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            decimal couriercharge = 0;
            decimal totalmaterialcost = 0;
            foreach (var item in v.lst)
            {   
                couriercharge = couriercharge + Convert.ToDecimal(item.COD);
                totalmaterialcost = totalmaterialcost + Convert.ToDecimal(item.MaterialCost);
            }
                DR objdrs = new DR();
            if (v.DRSID == 0)
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                v.DRSNo = _dao.GetMaxDRSNo(CompanyId, BranchId,v.DRSDate);
                objdrs.DRSNo = v.DRSNo;
                objdrs.BranchID = BranchId;
                objdrs.AcCompanyID = CompanyId;
                objdrs.StatusDRS = "0";
                objdrs.StatusInbound = false;
                
                objdrs.DrsType = "Courier";
                objdrs.CreatedBy = UserId;
                objdrs.FYearId = yearid;
                objdrs.CreatedDate = CommonFunctions.GetCurrentDateTime();
                
            }
            else
            {
                objdrs = db.DRS.Find(v.DRSID);

            }
            objdrs.TotalCourierCharge = couriercharge;
            objdrs.TotalMaterialCost = totalmaterialcost;
            objdrs.DRSDate = v.DRSDate;
            objdrs.DeliveredBy = v.DeliveredBy;            
            objdrs.VehicleID = v.VehicleID;
            objdrs.Pending = v.Pending;
            objdrs.ModifiedDate = CommonFunctions.GetCurrentDateTime();
            objdrs.ModifiedBy = UserId;

            if (v.DRSID==0)
            {
                db.DRS.Add(objdrs);
                db.SaveChanges();
            }
            else
            {
                db.Entry(objdrs).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            foreach (var item in v.lst)
            {

                if (item.InScanID > 0 && item.deleted == false) //Doemstic item
                {
                    var _inscan = db.InScanMasters.Find(item.InScanID);
                    if (_inscan.DRSID == null || _inscan.DRSID != objdrs.DRSID)
                    {
                        _inscan.DRSID = objdrs.DRSID;
                        _inscan.StatusTypeId = 3; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                        _inscan.CourierStatusID = 8;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId == 3 && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).FirstOrDefault();
                    if (awbtrack == null)
                    {
                        //updateing awbstaus table for tracking
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();
                        _awbstatus.AWBNo = _inscan.AWBNo;
                        _awbstatus.EntryDate = objdrs.DRSDate;// objdrs.DRSDate;
                        _awbstatus.InScanId = _inscan.InScanID;
                        _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                        _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                        _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                        _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                        _awbstatus.UserId = UserId;
                        _awbstatus.EmpID = v.DeliveredBy;
                        _awbstatus.DRSID = objdrs.DRSID;
                        db.AWBTrackStatus.Add(_awbstatus);
                        db.SaveChanges();
                    }
                    //MaterialCostMaster mc = new MaterialCostMaster();
                    //mc = db.MaterialCostMasters.Where(cc => cc.InScanID == item.InScanID).FirstOrDefault();
                    //if (mc != null)
                    //{
                    //    mc.DRSID = objdrs.DRSID;
                    //    mc.Status = "OUTSCAN";
                    //    db.Entry(mc).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //}
                }
                else if (item.InScanID > 0 && item.deleted == true) //Doemstic item removed
                {
                    var _inscan = db.InScanMasters.Find(item.InScanID);
                    if (_inscan.DRSID == null || _inscan.DRSID == objdrs.DRSID)
                    {
                        _inscan.DRSID = null;
                        _inscan.StatusTypeId = 2; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                        _inscan.CourierStatusID = 5;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId == 3 && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).FirstOrDefault();
                    db.AWBTrackStatus.Remove(awbtrack);
                    db.SaveChanges();

                }
                else if (item.InScanID == 0 && item.ShipmentDetailID > 0 && item.deleted == true) //import item item
                {
                    //var _inscan = db.ImportShipmentDetails.Find(item.ShipmentDetailID);
                    var _inscan = db.InboundShipments.Find(item.ShipmentDetailID);
                    var awbtrack1 = db.AWBTrackStatus.Where(cc => cc.InboundShipmentID == item.ShipmentDetailID && cc.StatusTypeId != 3 && cc.CourierStatusId != 8 && cc.DRSID == v.DRSID).OrderByDescending(cc => cc.EntryDate).FirstOrDefault();
                    if (_inscan.DRSID == null || _inscan.DRSID == objdrs.DRSID)
                    {
                        _inscan.DRSID = null;
                        if (awbtrack1 != null)
                        {
                            _inscan.StatusTypeId = awbtrack1.StatusTypeId; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                            _inscan.CourierStatusID = awbtrack1.CourierStatusId;
                        }
                        else
                        {
                            _inscan.StatusTypeId = 9;
                            _inscan.CourierStatusID = 21;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                        }

                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var awbtrack = db.AWBTrackStatus.Where(cc => cc.InboundShipmentID == item.ShipmentDetailID && cc.StatusTypeId == 3 && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).FirstOrDefault();
                    db.AWBTrackStatus.Remove(awbtrack);
                    db.SaveChanges();

                }
                else if (item.InScanID == 0 && item.ShipmentDetailID > 0 && item.deleted == false) //Import items
                {
                    var _inscan = db.InboundShipments.Find(item.ShipmentDetailID);
                    _inscan.DRSID = objdrs.DRSID;
                    _inscan.StatusTypeId = 3;// db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                    _inscan.CourierStatusID = 8; //Out for delivery ;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                    db.Entry(_inscan).State = EntityState.Modified;
                    db.SaveChanges();
                    var awbtrack1 = db.AWBTrackStatus.Where(cc => cc.InboundShipmentID == item.ShipmentDetailID && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).OrderByDescending(cc => cc.EntryDate).FirstOrDefault();

                    if (awbtrack1 == null)
                    {
                        
                            //updateing awbstaus table for tracking
                            AWBTrackStatu _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = _inscan.AWBNo;
                            _awbstatus.EntryDate = objdrs.DRSDate;//  objdrs.DRSDate;
                            _awbstatus.InboundShipmentID = _inscan.ShipmentID;
                            _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                            _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                            _awbstatus.UserId = UserId;
                            _awbstatus.EmpID = v.DeliveredBy;
                            _awbstatus.DRSID = objdrs.DRSID;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                  }
                }
            }


           
            TempData["success"] = "DRS Saved Successfully.";
            return RedirectToAction("Create");
         

        }

        public ActionResult Edit(int id)
        {
            ViewBag.Deliverdby = db.EmployeeMasters.ToList();
            ViewBag.vehicle = db.VehicleMasters.ToList();
            ViewBag.CheckedBy = db.EmployeeMasters.ToList();

            DR d = db.DRS.Find(id);
            DRSVM v = new DRSVM();
            if (d == null)
            {
                return HttpNotFound();

            }
            else
            {

                v.DRSID = d.DRSID;
                v.DRSNo = d.DRSNo;
                v.DRSDate = d.DRSDate;
                v.DeliveredBy = d.DeliveredBy;
                //v.CheckedBy = d.CheckedBy;
                v.TotalCourierCharge = d.TotalCourierCharge;
                v.VehicleID = d.VehicleID;
                v.StatusDRS = d.StatusDRS;
                v.AcCompanyID = d.AcCompanyID;
                v.StatusInbound = d.StatusInbound;
                v.DrsType = d.DrsType;

            }
            return View(v);
        }

        //
        // POST: /InScan/Edit/5

        [HttpPost]
        public ActionResult Edit(DRSVM v)
        {
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            try
            {
                //var data = (from c in db.DRSDetails where c.DRSID == v.DRSID select c).ToList();
                //foreach (var item in data)
                //{
                //    db.DRSDetails.Remove(item);
                //    db.SaveChanges();
                //}

                var data = (from c in db.InScanMasters where c.DRSID == v.DRSID select c).ToList();
                foreach (var item in data)
                {
                    var _inscan = db.InScanMasters.Find(item.InScanID);
                    _inscan.DRSID = null;
                    db.Entry(_inscan).State = EntityState.Modified;
                    db.SaveChanges();

                    var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.ShipmentStatus == "OUTSCAN" && cc.CourierStatus == "Out for Delivery at Origin").First();
                    db.AWBTrackStatus.Remove(awbtrack);
                    db.SaveChanges();
                }


                DR objdrs = db.DRS.Find(v.DRSID);
                //objdrs.DRSNo = objdrs.DRSID.ToString();
                objdrs.DRSDate = v.DRSDate;
                objdrs.DeliveredBy = v.DeliveredBy;
                //objdrs.CheckedBy = v.CheckedBy;
                objdrs.TotalCourierCharge = 0;
                objdrs.VehicleID = v.VehicleID;
                objdrs.StatusDRS = "0";
                objdrs.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                objdrs.StatusInbound = false;
                objdrs.DrsType = "Courier";

                db.Entry(objdrs).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var item in v.lst)
                {

                    var _inscan = db.InScanMasters.Find(item.InScanID);
                    _inscan.DRSID = objdrs.DRSID;
                    _inscan.StatusTypeId = db.tblStatusTypes.Where(cc => cc.Name == "OUTSCAN").First().ID;
                    _inscan.CourierStatusID = db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out for Delivery at Origin").FirstOrDefault().CourierStatusID;
                    db.Entry(_inscan).State = EntityState.Modified;
                    db.SaveChanges();

                    //updateing awbstaus table for tracking
                    AWBTrackStatu _awbstatus = new AWBTrackStatu();
                    int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                    if (id == null)
                        id = 1;
                    else
                        id = id + 1;

                    _awbstatus.AWBTrackStatusId = Convert.ToInt32(id);
                    _awbstatus.AWBNo = _inscan.AWBNo;
                    _awbstatus.EntryDate = DateTime.UtcNow; // DateTime.Now;
                    _awbstatus.InScanId = _inscan.InScanID;
                    _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                    _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                    _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                    _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                    _awbstatus.UserId = UserId;

                    db.AWBTrackStatus.Add(_awbstatus);
                    db.SaveChanges();
                }


                //foreach (var item in v.lst)
                //{
                //    DRSDetail d = new DRSDetail();
                //    d.DRSID = objdrs.DRSID;
                //    d.AWBNO = item.AWB;
                //    d.InScanID = item.InScanID;
                //    d.CourierCharge = item.COD;
                //    d.MaterialCost = 0;
                //    d.StatusPaymentMode = "PKP";
                //    d.CCReceived = 0;
                //    d.CCStatuspaymentType = "CS";
                //    d.MCReceived = 0;
                //    d.MCStatuspaymentType = "CS";
                //    d.Remarks = "";
                //    d.ReceiverName = item.Consignee;
                //    d.CourierStatusID = 9;
                //    d.StatusAWB = "DD";
                //    d.EmployeeID = Convert.ToInt32(Session["UserID"].ToString());
                //    d.ReturnTime = DateTime.Now;

                //    db.DRSDetails.Add(d);
                //    db.SaveChanges();

                //}
                TempData["success"] = "DRS Updated Successfully.";
                return RedirectToAction("Index");

              
             
            }
            catch (Exception c)
            {

            }

            return View();
        }

        //
        // GET: /InScan/Delete/5


        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteDRS(id);
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

          
                return Json(new { status = "OK", message = "Contact Admin!" });
           
        }


        public ActionResult DRSRunSheet(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            ViewBag.ReportId = id;
            AccountsReportsDAO.GenerateDRSRunSheet(id,"PDF");
            ViewBag.ReportName = "DRS Run Sheet";
            return View();

        }

        public FileResult DownloadFile(int id)
        {
            string filepath = AccountsReportsDAO.GenerateDRSRunSheet(id, "EXCEL");
            string filename = "DRSRunSheetReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx"; // Server.MapPath("~" + filePath);

            byte[] fileBytes = GetFile(filepath);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        public ActionResult CreateBulk(int id = 0)
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            // ViewBag.vehicle = db.VehicleMasters.ToList();
            ViewBag.Vehicles = (from c in db.VehicleMasters select new { VehicleID = c.VehicleID, VehicleName = c.RegistrationNo + "-" + c.VehicleDescription }).ToList();
            ViewBag.Checkedby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DRSVM v = new DRSVM();
            if (id > 0)
            {
                DR d = db.DRS.Find(id);
                if (d != null)
                {
                    v.DRSID = d.DRSID;
                    v.DRSNo = d.DRSNo;
                    v.DRSDate = d.DRSDate;
                    v.DeliveredBy = d.DeliveredBy;
                    //v.CheckedBy = d.CheckedBy;
                    v.TotalCourierCharge = d.TotalCourierCharge;
                    v.VehicleID = d.VehicleID;
                    v.StatusDRS = d.StatusDRS;
                    v.AcCompanyID = d.AcCompanyID;
                    v.StatusInbound = d.StatusInbound;
                    v.DrsType = d.DrsType;
                    v.Pending = d.Pending;
                    v.Delivered = d.Delivered;
                }
                List<DRSDet> details = new List<DRSDet>();
                details = AWBDAO.GetDRSAWBDetails(v.DRSID);
                details = details.OrderByDescending(cc => cc.SNo).ToList();
                v.lst = details;
                ViewBag.EditMode = "true";
                ViewBag.Title = "Modify";
                Session["OutScanList"] = details;
            }
            else
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                v.DRSID = 0;
                v.DRSDate = CommonFunctions.GetCurrentDateTime();
                v.DRSNo = _dao.GetMaxDRSNo(CompanyId, BranchId,v.DRSDate);
                ViewBag.EditMode = "false";
                List<DRSDet> details = new List<DRSDet>();
                v.Details = details;
                
                v.lst = details;
                ViewBag.Title = "Create";
                Session["OutScanList"] = details;
            }
            return View(v);
        }

        [HttpPost]
        public ActionResult CreateBulk(DRSVM v)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            decimal couriercharge = 0;
            decimal totalmaterialcost = 0;
            foreach (var item in v.lst)
            {
                couriercharge = couriercharge + Convert.ToDecimal(item.COD);
                totalmaterialcost = totalmaterialcost + Convert.ToDecimal(item.MaterialCost);
            }
            DR objdrs = new DR();
            if (v.DRSID == 0)
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                
                
                v.DRSNo = _dao.GetMaxDRSNo(CompanyId, BranchId, v.DRSDate);
                objdrs.DRSNo = v.DRSNo;
                objdrs.CreatedDate = CommonFunctions.GetCurrentDateTime();
                objdrs.BranchID = BranchId;
                objdrs.AcCompanyID = CompanyId;
                objdrs.StatusDRS = "0";
                objdrs.StatusInbound = false;
                objdrs.DrsType = "Courier";
                objdrs.CreatedBy = UserId;
                objdrs.FYearId = yearid;
                


            }
            else
            {
                objdrs = db.DRS.Find(v.DRSID);

            }
            objdrs.TotalCourierCharge = couriercharge;
            objdrs.TotalMaterialCost = totalmaterialcost;
            objdrs.DRSDate = v.DRSDate;
            objdrs.Pending = v.Pending;
            objdrs.DeliveredBy = v.DeliveredBy;
            objdrs.VehicleID = v.VehicleID;
            objdrs.ModifiedDate = CommonFunctions.GetCurrentDateTime();
            objdrs.ModifiedBy = UserId;
            objdrs.Delivered = v.Delivered;
            if (v.DRSID == 0)
            {
                db.DRS.Add(objdrs);
                db.SaveChanges();
                
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                      
                foreach(var item in v.lst)
                {
                    if (item.deleted == true)
                        v.lst.Remove(item);
                }
                dt = ToDataTable(v.lst);                
                string xml = dt.GetXml();
                string result = AWBDAO.SaveOutScan(BranchId, objdrs.DRSID, UserId, xml);
            }
            else
            {
                db.Entry(objdrs).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var item in v.lst)
                {
                    
                    if (item.InScanID > 0 && item.deleted == false) //Doemstic item
                    {
                        var _inscan = db.InScanMasters.Find(item.InScanID);
                        if (_inscan.DRSID == null || _inscan.DRSID != objdrs.DRSID)
                        {
                            _inscan.DRSID = objdrs.DRSID;
                            if (v.Delivered==true)
                            {
                                _inscan.StatusTypeId = 4; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                                _inscan.CourierStatusID =13;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                            }
                            else
                            {
                                _inscan.StatusTypeId = 3; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                                _inscan.CourierStatusID = 8;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                            }
                            
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId == _inscan.StatusTypeId && cc.CourierStatusId == _inscan.CourierStatusID && cc.DRSID == v.DRSID).FirstOrDefault();
                        if (awbtrack == null)
                        {
                            //updateing awbstaus table for tracking
                            AWBTrackStatu _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = _inscan.AWBNo;
                            _awbstatus.EntryDate = objdrs.DRSDate;// objdrs.DRSDate;
                            _awbstatus.InScanId = _inscan.InScanID;
                            _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                            _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                            if (v.Pending == true)
                                _awbstatus.CourierStatus = _awbstatus.CourierStatus + "(Pending)";
                            //if (v.Delivered==true)
                            //    _awbstatus.CourierStatus = awbstatus.CourierStatus + "(Pending)";
                            _awbstatus.UserId = UserId;
                            _awbstatus.EmpID = v.DeliveredBy;
                            _awbstatus.DRSID = objdrs.DRSID;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                        }
                        //MaterialCostMaster mc = new MaterialCostMaster();
                        //mc = db.MaterialCostMasters.Where(cc => cc.InScanID == item.InScanID).FirstOrDefault();
                        //if (mc != null)
                        //{
                        //    mc.DRSID = objdrs.DRSID;
                        //    mc.Status = "OUTSCAN";
                        //    db.Entry(mc).State = EntityState.Modified;
                        //    db.SaveChanges();
                        //}
                    }
                    else if (item.InScanID > 0 && item.deleted == true) //Doemstic item removed
                    {
                        var _inscan = db.InScanMasters.Find(item.InScanID);
                        if (_inscan.DRSID == null || _inscan.DRSID == objdrs.DRSID)
                        {
                            _inscan.DRSID = null;
                            _inscan.StatusTypeId = 2; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                            _inscan.CourierStatusID = 5;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId == 3 && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).FirstOrDefault();
                        var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID &&  cc.DRSID == v.DRSID).FirstOrDefault();
                        db.AWBTrackStatus.Remove(awbtrack);
                        db.SaveChanges();

                    }
                    else if (item.InScanID == 0 && item.ShipmentDetailID > 0 && item.deleted == true) //import item item
                    {
                        var _inscan = db.ImportShipmentDetails.Find(item.ShipmentDetailID);
                        var awbtrack1 = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId != 3 && cc.CourierStatusId != 8 && cc.DRSID == v.DRSID).OrderByDescending(cc => cc.EntryDate).FirstOrDefault();
                        if (_inscan.DRSID == null || _inscan.DRSID == objdrs.DRSID)
                        {
                            _inscan.DRSID = null;
                            if (awbtrack1 != null)
                            {
                                _inscan.StatusTypeId = awbtrack1.StatusTypeId; // db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                                _inscan.CourierStatusID = awbtrack1.CourierStatusId;
                            }
                            else
                            {
                                _inscan.StatusTypeId = 9;
                                _inscan.CourierStatusID = 21;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                            }

                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == item.InScanID && cc.StatusTypeId == 3 && cc.CourierStatusId == 8 && cc.DRSID == v.DRSID).FirstOrDefault();
                        db.AWBTrackStatus.Remove(awbtrack);
                        db.SaveChanges();

                    }
                    else if (item.InScanID == 0 && item.ShipmentDetailID > 0 && item.deleted == false) //Import items
                    {
                        var _inscan = db.ImportShipmentDetails.Find(item.ShipmentDetailID);
                        _inscan.DRSID = objdrs.DRSID;
                        _inscan.StatusTypeId = 3;// db.tblStatusTypes.Where(cc => cc.Name == "Depot Outscan").First().ID;
                        _inscan.CourierStatusID = 8; //Out for delivery ;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Out For Delivery").FirstOrDefault().CourierStatusID;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();

                        //updating awbstaus table for tracking
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();
                        _awbstatus.AWBNo = _inscan.AWB;
                        _awbstatus.EntryDate = DateTime.UtcNow; // objdrs.DRSDate;//  objdrs.DRSDate;
                        _awbstatus.ShipmentDetailID = _inscan.ShipmentDetailID;
                        _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                        _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                        _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                        _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                        _awbstatus.UserId = UserId;
                        _awbstatus.EmpID = v.DeliveredBy;
                        _awbstatus.DRSID = objdrs.DRSID;
                        _awbstatus.APIStatus = true;
                        db.AWBTrackStatus.Add(_awbstatus);
                        db.SaveChanges();
                    }
                }

            }






            TempData["success"] = "DRS Saved Successfully.";
            return RedirectToAction("CreateBulk");


        }

    
        public JsonResult GetAWBData1(string id,int DeliveredBy)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DRSVM vm = new DRSVM();
            
            List<DRSDet> details = (List<DRSDet>)Session["OutScanList"];

            List<DRSDet> details1= AWBDAO.GetOutscanAWBDetails(branchid, "",id,DeliveredBy);

            if (details != null && details1.Count>0)
            {
                details1[0].SNo = details.Count + 1;
                details.AddRange(details1);
            }
            else if (details == null && details1.Count > 0)
            {
                details = new List<DRSDet>();
                details1[0].SNo = details.Count + 1;
                details.AddRange(details1);
            }

            details = details.OrderByDescending(cc => cc.SNo).ToList();

            Session["OutScanList"] = details;

            if (details1.Count>0)
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

            List<DRSDet> details = (List<DRSDet>)Session["OutScanList"];
            if (details == null)
                details = new List<DRSDet>();

            int i = 0;
            foreach(var item in details)
            {
                if (item.AWB==id)
                {
                    details[i].deleted = true;
                    details[i].deletedclass = "hide";
                }
                i++;
            }
            Session["OutScanList"] = details;

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
        //ShowOutScanList
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile,int DeliveredBy)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
               List<DRSDet> fileData = GetDataFromCSVFile(importFile.InputStream,DeliveredBy);
                DRSVM vm = new DRSVM();
                vm.Details = fileData;
                vm.lst = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
                Session["OutScanList"] = vm.lst;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<DRSDet> GetDataFromCSVFile(Stream stream,int DeliveredBy)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var details = new List<DRSDet>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                       
                        var dataTable = dataSet.Tables[0];
                        string xml = dataSet.GetXml();
                        details = AWBDAO.GetOutscanAWBDetails(branchid, xml,"", DeliveredBy);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

         
            

            return details;
        }
        public ActionResult ShowOutScanList()
        {
            DRSVM vm = new DRSVM();
            vm.lst = (List<DRSDet>)Session["OutScanList"];
            return PartialView("ItemList", vm);
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


