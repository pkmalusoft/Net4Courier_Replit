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
using System.Reflection;
using ExcelDataReader;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class AgentShipmentController : Controller
    {
        Entities1 db = new Entities1();

        #region "SingleShipment"
        public ActionResult Index(InboundAWBSearch obj)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            string _USERTYPE = "";
            _USERTYPE = Session["UserType"].ToString();
            int logincustomerid = 0;
            if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
            {
                logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());                
            }
            // AWBSearch obj = (AWBSearch)Session["AWBSearch"];
            InboundAWBSearch model = new InboundAWBSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                obj = new InboundAWBSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.StatusID = 0;
                obj.Destination = "";
                obj.MovementTypeID = 0;
                obj.PaymentModeId = 0;
                obj.Origin = "";
                obj.Destination = "";
                obj.ConsignorConsignee = "";
                Session["AWBSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.StatusID = 0;
                model.Details = new List<InboundShipmentModel>();
                List<InboundShipmentModel> lst = InboundShipmentDAO.GetAWBList(obj.StatusID, obj.FromDate, obj.ToDate, branchid, depotId, model.AWBNo, obj.MovementTypeID, obj.PaymentModeId, obj.ConsignorConsignee, obj.Origin, obj.Destination, logincustomerid);
                model.Details = lst;

            }
            else
            {
                model = obj;
                List<InboundShipmentModel> lst = InboundShipmentDAO.GetAWBList(obj.StatusID, obj.FromDate, obj.ToDate, branchid, depotId, model.AWBNo, obj.MovementTypeID, obj.PaymentModeId, obj.ConsignorConsignee, obj.Origin, obj.Destination,logincustomerid);
                model.Details = lst;
            }

            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            // ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }

        public ActionResult Create(int id = 0)
        {

            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            string _USERTYPE=Session["UserType"].ToString();
            
            
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();


            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();
            List<OtherChargeDetailVM> otherchargesvm = new List<OtherChargeDetailVM>();
            List<ShipmentItemDetailVM> shipmentItemVM = new List<ShipmentItemDetailVM>();
            InboundShipmentModel v = new InboundShipmentModel();
            string customername = "";

            if (id == 0)
            {
                if (_USERTYPE=="Customer" || _USERTYPE=="CoLoader")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    customername= db.CustomerMasters.Find(logincustomerid).CustomerName;
                    v.CustomerID = logincustomerid;
                    v.CustomerName = customername;
                }
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    v.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    v.MovementID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    v.ParcelTypeID = defaultparceltype.ID;


                var shipmentmode = db.tblShipmentModes.Where(cc => cc.ByDefault == true).FirstOrDefault();

                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;

                }
                v.ShipmentID = 0;
                v.TaxPercent = 5;
                v.PaymentModeId = 3;

                ViewBag.Title = "Create";
                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                //if (Session["UserType"].ToString() == "Employee")
                //{
                //    var useremp = (from e in db.EmployeeMasters where e.UserID == userId select e).First();
                //    if (useremp != null)
                //    {
                //        if (useremp.AcHeadID != null)
                //        {
                //            v.AcheadID = Convert.ToInt32(useremp.AcHeadID);
                //            var achead = db.AcHeads.Find(Convert.ToInt32(v.AcheadID));

                //            if (achead != null)
                //            {
                //                v.AcHeadName = achead.AcHead1;
                //            }
                //        }
                //    }
                //}

                ViewBag.PeriodLock = "false";
                ViewBag.PeriodLockMessage = "";
                v.AWBDate = CommonFunctions.GetCurrentDateTime();

            }
            else
            {
                ViewBag.Title = "Modify";
                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();
                v = GetAWBDetail(id);

                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;
                }


                customername = "WALK-IN-CUSTOMER";

                customername = "COD-CUSTOMER";

                customername = "FOC CUSTOMER";


                v.TaxPercent = 5;





                ViewBag.StatusType = v.StatusType;
                ViewBag.CourierStatus = v.CourierStatus;
                ViewBag.EditMode = "true";

            }

            return View(v);

        }
        public InboundShipmentModel GetAWBDetail(int id)
        {
            InboundShipmentModel inscan = new InboundShipmentModel();

            InboundShipment data = (from c in db.InboundShipments where c.ShipmentID == id select c).FirstOrDefault();

            inscan.ShipmentID = data.ShipmentID;
            inscan.AWBDate = Convert.ToDateTime(data.AWBDate);
            inscan.AWBNo = data.AWBNo;

            inscan.Consignor = data.Consignor;

            inscan.Consignor = data.Consignor;
            inscan.StatusTypeId = data.StatusTypeId;
            inscan.CourierStatusID = data.CourierStatusID;
            inscan.Remarks = data.Remarks;
            int statustypeid = data.StatusTypeId;
 
           inscan.StatusType = db.tblStatusTypes.Where(cc => cc.ID == statustypeid).FirstOrDefault().Name;
           inscan.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == inscan.CourierStatusID).FirstOrDefault().CourierStatus;
           

            //inscan.AcJournalID = data.AcJournalID.Value;

            if (data.CourierCharge != null)
            {
                inscan.CourierCharge = Convert.ToDecimal(data.CourierCharge);
            }
            else
            {
                inscan.CourierCharge = 0;
            }
            if (data.TaxPercent != null)
            {
                inscan.TaxPercent = Convert.ToDecimal(data.TaxPercent);
            }
            else
            {
                inscan.TaxPercent = 0;
            }

            if (data.TaxAmount != null)
            {
                inscan.TaxAmount = Convert.ToDecimal(data.TaxAmount);
            }
            else
            {
                inscan.TaxAmount = 0;
            }
            if (data.SurchargePercent != null)
            {
                inscan.SurchargePercent = Convert.ToDecimal(data.SurchargePercent);
                inscan.SurchargeAmount = Convert.ToDecimal(data.SurchargeAmount);
            }
            else
            {
                inscan.SurchargePercent = 0;
                inscan.SurchargeAmount = 0;
            }






            if (data.Weight != null)
            {
                inscan.Weight = data.Weight.Value;
            }
            else
            {
                inscan.Weight = 0;
            }

           
                inscan.ProductTypeID = Convert.ToInt32(data.ProductTypeID);

          
 
                inscan.ParcelTypeID = Convert.ToInt32(data.ParcelTypeID);
           


            if (data.PaymentModeId != null)
            {
                inscan.PaymentModeId = data.PaymentModeId;
                inscan.PaymentMode = db.tblPaymentModes.Find(inscan.PaymentModeId).PaymentModeText;
            }
            // inscan.paymentmode = data.StatusPaymentMode;
            inscan.ConsignorCountryName = data.ConsignorCountryName;
            inscan.ConsignorCityName = data.ConsignorCityName;
            inscan.ConsigneeCountryName = data.ConsigneeCountryName;
            inscan.ConsigneeCityName = data.ConsigneeCityName;

            inscan.ConsignorAddress1_Building = data.ConsignorAddress1_Building;
            inscan.ConsignorAddress2_Street = data.ConsignorAddress2_Street;
            inscan.ConsignorAddress3_PinCode= data.ConsignorAddress3_PinCode;

            inscan.CustomerID = data.CustomerID;
                var cust = db.CustomerMasters.Find(inscan.CustomerID);
                if (cust != null)
                    inscan.CustomerName = cust.CustomerName;
                else
                    inscan.CustomerName = "Customer Unknown";

           

            //inscan.TaxconfigurationID = data.TaxconfigurationID.Value;
            inscan.Consignee = data.Consignee;
            inscan.ConsigneeAddress1_Building = data.ConsigneeAddress1_Building;
            inscan.ConsigneeAddress2_Street = data.ConsigneeAddress2_Street;
            inscan.ConsigneeAddress3_PinCode = data.ConsigneeAddress3_PinCode;

            inscan.ConsigneePhone = data.ConsigneePhone;
            inscan.ConsignorPhone = data.ConsignorPhone;


            // inscan.Pieces = data.Pieces;
            inscan.ConsignorLocationName = data.ConsignorLocationName;
            inscan.ConsigneeLocationName = data.ConsigneeLocationName;

            //inscan.totalCharge = data.BalanceAmt.Value;
            //inscan.materialcost = data.MaterialCost.Value;
            inscan.CargoDescription = data.CargoDescription;
            inscan.Pieces = data.Pieces;
            inscan.MaterialCost = data.MaterialCost;
            inscan.CurrencyID = data.CurrencyID;
            inscan.CustomsValue = data.CustomsValue;

            if (data.MovementID != null)
                inscan.MovementID = data.MovementID;

            if (data.CreatedBy != null)
            {
                inscan.CreatedDate = Convert.ToDateTime(data.CreatedDate);
                if (data.CreatedBy != null)
                {
                    var emp = db.EmployeeMasters.Where(CC => CC.UserID == data.CreatedBy).FirstOrDefault();
                    if (emp != null)
                    {
                        inscan.CreatedByName = emp.EmployeeName;
                    }
                }
            }

            if (data.LastModifiedBy != null)
            {
                inscan.LastModifiedDate = Convert.ToDateTime(data.LastModifiedDate);

                if (data.LastModifiedBy != null)
                {
                    var emp = db.EmployeeMasters.Where(CC => CC.UserID == data.LastModifiedBy).FirstOrDefault();
                    if (emp != null)
                    {
                        inscan.LastModifiedByName = emp.EmployeeName;
                    }
                }
            }

            return inscan;

        }

        [HttpPost]
        public JsonResult SaveAWB(InboundShipmentModel v)
        {
            string customersavemessage = "";
            try
            {
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                int userid = Convert.ToInt32(Session["UserID"].ToString());
                                
               
                InboundShipment inscan = new InboundShipment();

                try
                {

                 

                    if (v.ShipmentID == 0)
                    {
                        
                        
                        inscan.AWBNo = v.AWBNo; // _dao.GetMaAWBNo(companyId, branchid);
                        
                        inscan.BranchID = branchid;
                        
                        inscan.AcFinancialYearID = yearid;
                       

                        inscan.CustomerID = v.CustomerID;
                        
                        
                        if (inscan.StatusTypeId == null)
                            inscan.StatusTypeId = 1;
                        if (inscan.CourierStatusID == null)
                            inscan.CourierStatusID = 4;
                     
                       

                        if (inscan.CreatedBy == null)
                            inscan.CreatedBy = userid;

                        if (inscan.CreatedDate == null)
                            inscan.CreatedDate = CommonFunctions.GetCurrentDateTime();

                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                        inscan.PaymentModeId = 3;                        
                        
                    }
                    else
                    {
                        inscan = db.InboundShipments.Find(v.ShipmentID);                                              

                        inscan.AWBDate = v.AWBDate;                        
                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                    }
                    inscan.Weight = v.Weight;

                    inscan.AWBDate = v.AWBDate;

                    inscan.Consignor = v.Consignor;
                    inscan.ConsignorContact = v.ConsignorContact;
                    inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                    inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;
                    inscan.ConsignorAddress3_PinCode = v.ConsignorAddress3_PinCode;
                    inscan.ConsignorPhone = v.ConsignorPhone;
                    inscan.ConsignorMobileNo = v.ConsignorMobileNo;
                    inscan.ConsignorCountryName = v.ConsignorCountryName;
                    inscan.ConsignorCityName = v.ConsignorCityName;
                    inscan.ConsignorLocationName = v.ConsignorLocationName;


                    if (v.Weight != null)
                    {
                        inscan.Weight = Convert.ToDecimal(v.Weight);
                    }

                    inscan.PaymentModeId = v.PaymentModeId;

                    
                    inscan.Consignee = v.Consignee;
                    inscan.ConsigneeContact = v.ConsigneeContact;
                    inscan.ConsigneeCountryName = v.ConsigneeCountryName;
                    inscan.ConsigneeCityName = v.ConsigneeCityName;
                    inscan.ConsigneeLocationName = v.ConsigneeLocationName;
                    inscan.ConsigneeAddress1_Building = v.ConsigneeAddress1_Building;
                    inscan.ConsigneeAddress2_Street = v.ConsigneeAddress2_Street;
                    inscan.ConsigneeAddress3_PinCode = v.ConsigneeAddress3_PinCode;
                    
                    inscan.ConsigneeMobileNo = v.ConsigneeMobileNo;
                    inscan.ConsigneePhone = v.ConsigneePhone;


                    inscan.Pieces = v.Pieces;

                    inscan.ProductTypeID = v.ProductTypeID;
                    inscan.ParcelTypeID = v.ParcelTypeID;
                    //inscan.CourierCharge = v.CourierCharge;
                    //inscan.TaxPercent = v.TaxPercent;
                    //inscan.TaxAmount = v.TaxAmount;
                    //inscan.SurchargePercent = v.SurchargePercent;
                    //inscan.SurchargeAmount = v.SurchargeAmount;                    
                    inscan.MovementID = v.MovementID;
                    //inscan.OtherCharge = v.OtherCharge;
                    inscan.CargoDescription = v.CargoDescription;
                    inscan.Remarks = v.Remarks;
                    inscan.CurrencyID = v.CurrencyID;
                    inscan.CustomsValue = v.CustomsValue;
                    if (v.MaterialCost != null)
                    {
                        inscan.MaterialCost = Convert.ToDecimal(v.MaterialCost);
                    }
                    
                    if (v.ShipmentID == 0 )
                    {
                        inscan.EntrySource = "SGL"; //AWB create by create shipment page

                        if (inscan.MovementID == 3)
                        {
                            inscan.StatusTypeId = 9; //Import Inscan
                            inscan.CourierStatusID = 12; //At Destination Customs Facility
                        }
                        else
                        {
                            inscan.StatusTypeId = 9; //Import Inscan
                            inscan.CourierStatusID = 20; //Received At Transit Facility
                        }

                        inscan.IsDeleted = false;
                        db.InboundShipments.Add(inscan);
                        db.SaveChanges();
                    }
                    else
                    {
                    
                        db.Entry(inscan).State = EntityState.Modified;
                        db.SaveChanges();
                        

                    }

                    if (v.ShipmentID == 0)
                    {
                        AddAWBTrackStatus(inscan.ShipmentID, inscan.AWBNo, inscan.CustomerID, inscan.StatusTypeId, inscan.CourierStatusID);
                        return Json(new { status = "OK", InscanId = inscan.ShipmentID, message = "AWB Added Succesfully!" });

                    }
                    else
                    {
                        return Json(new { status = "OK", InscanId = inscan.ShipmentID, message = "AWB Updated Succesfully!" });
                    }


                }
                catch (Exception ex)
                {
                    
                    return Json(new { status = "Failed", InscanId = 0, message = ex.Message });
                }



            }
            catch (Exception ex)
            {
                
                return Json(new { status = "Failed", InscanId = 0, message = ex.Message });
            }

        }
        public string AddAWBTrackStatus(int ShipmentId, string AWBNo, int CustomerId, int pStatusTypeId = 0, int pCourierStatusId = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());

            AWBTrackStatu _awbstatus = new AWBTrackStatu();

            _awbstatus.AWBNo = AWBNo;
            _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();
            _awbstatus.InboundShipmentID = ShipmentId;

            if (pStatusTypeId > 0)
            {
                _awbstatus.StatusTypeId = pStatusTypeId;
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(pStatusTypeId).Name;
            }
            else
            {
                _awbstatus.StatusTypeId = Convert.ToInt32(pStatusTypeId);
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(pStatusTypeId).Name;
            }

            if (pCourierStatusId > 0)
            {
                _awbstatus.CourierStatusId = pCourierStatusId;
                _awbstatus.CourierStatus = db.CourierStatus.Find(pCourierStatusId).CourierStatus;
            }

            _awbstatus.UserId = uid;
            _awbstatus.CustomerId = CustomerId;

            db.AWBTrackStatus.Add(_awbstatus);
            db.SaveChanges();
            return "ok";
        }
        #endregion
        #region "Batchshipment"
        public ActionResult BatchIndex(AWBBatchSearch obj)
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);            
            AWBBatchSearch model = new AWBBatchSearch();
            var customerid = Convert.ToInt32(Session["CustomerId"].ToString());
            AWBDAO _dao = new AWBDAO();
            if (obj != null && obj.FromDate!=null && !obj.FromDate.ToString().Contains("0001"))
            {
                List<AWBBatchList> translist = new List<AWBBatchList>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo;
                translist = InboundShipmentDAO.GetAWBBatchList(BranchID, FyearId,model, customerid);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                List<AWBBatchList> translist = new List<AWBBatchList>();
                translist = InboundShipmentDAO.GetAWBBatchList(BranchID, FyearId,model, customerid);
                model.Details = translist;

            }
            return View(model);
        }
        public ActionResult BatchCreate(int id = 0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            string _USERTYPE = Session["UserType"].ToString();


            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();



            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();
            InboundAWBBatchModel v = new InboundAWBBatchModel();
            string customername = "";

            if (id == 0)
            {
                if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    customername = db.CustomerMasters.Find(logincustomerid).CustomerName;
                    v.CustomerID = logincustomerid;
                    v.CustomerName = customername;
                }
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    v.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    v.MovementID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    v.ParcelTypeID = defaultparceltype.ID;


                var shipmentmode = db.tblShipmentModes.Where(cc => cc.ByDefault == true).FirstOrDefault();

                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;

                }
                v.ShipmentID = 0;
                v.TaxPercent = 5;
                v.PaymentModeId = 3;
                v.EntrySource = "MNL"; //--1 refer single entry ,2 refer batch entry, 3 refer excel upload

                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                //if (Session["UserType"].ToString() == "Employee")
                //{
                //    var useremp = (from e in db.EmployeeMasters where e.UserID == userId select e).First();
                //    if (useremp != null)
                //    {
                //        if (useremp.AcHeadID != null)
                //        {
                //            v.AcheadID = Convert.ToInt32(useremp.AcHeadID);
                //            var achead = db.AcHeads.Find(Convert.ToInt32(v.AcheadID));

                //            if (achead != null)
                //            {
                //                v.AcHeadName = achead.AcHead1;
                //            }
                //        }
                //    }
                //}

                ViewBag.PeriodLock = "false";
                ViewBag.PeriodLockMessage = "";
                v.BATCHID = 0;
                v.BatchDate = CommonFunctions.GetCurrentDateTime();
                v.BatchNumber = InboundShipmentDAO.GetMaxBathcNo(v.BatchDate, branchid, yearid);
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                v.DefaultCurrencyId = CommonFunctions.GetDefaultCurrencyId();
                v.Details = new List<InboundShipmentModel>();
            }
            else
            {
                InboundAWBBatch batch = db.InboundAWBBatches.Find(id);
                v.BATCHID = batch.ID;
                v.BatchDate = batch.BatchDate;
                v.BatchNumber = batch.BatchNumber;
                v.TotalAWB =Convert.ToInt32(batch.TotalAWB);
                if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    customername = db.CustomerMasters.Find(logincustomerid).CustomerName;
                    v.CustomerID = logincustomerid;
                    v.CustomerName = customername;
                }
                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();
                v.Details = InboundShipmentDAO.GetBatchAWBInfo(id);
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;
                }

                customername = "WALK-IN-CUSTOMER";
                customername = "COD-CUSTOMER";
                customername = "FOC CUSTOMER";
                v.TaxPercent = 5;
                v.DefaultCurrencyId = CommonFunctions.GetDefaultCurrencyId();
                ViewBag.StatusType = v.StatusType;
                ViewBag.CourierStatus = v.CourierStatus;
                ViewBag.EditMode = "true";
            }

            return View(v);

        }
        public ActionResult Details(int id = 0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            string _USERTYPE = Session["UserType"].ToString();


        

           

            InboundAWBBatchModel v = new InboundAWBBatchModel();
            string customername = "";

            if (id == 0)
            {
                if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    customername = db.CustomerMasters.Find(logincustomerid).CustomerName;
                    v.CustomerID = logincustomerid;
                    v.CustomerName = customername;
                }
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    v.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    v.MovementID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    v.ParcelTypeID = defaultparceltype.ID;


                var shipmentmode = db.tblShipmentModes.Where(cc => cc.ByDefault == true).FirstOrDefault();

                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;

                }
                v.ShipmentID = 0;
                v.TaxPercent = 5;
                v.PaymentModeId = 3;
                v.EntrySource = "MNL"; //--1 refer single entry ,2 refer batch entry, 3 refer excel upload

                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                //if (Session["UserType"].ToString() == "Employee")
                //{
                //    var useremp = (from e in db.EmployeeMasters where e.UserID == userId select e).First();
                //    if (useremp != null)
                //    {
                //        if (useremp.AcHeadID != null)
                //        {
                //            v.AcheadID = Convert.ToInt32(useremp.AcHeadID);
                //            var achead = db.AcHeads.Find(Convert.ToInt32(v.AcheadID));

                //            if (achead != null)
                //            {
                //                v.AcHeadName = achead.AcHead1;
                //            }
                //        }
                //    }
                //}

                ViewBag.PeriodLock = "false";
                ViewBag.PeriodLockMessage = "";
                v.BATCHID = 0;
                v.BatchDate = CommonFunctions.GetCurrentDateTime();
                v.BatchNumber = InboundShipmentDAO.GetMaxBathcNo(v.BatchDate, branchid, yearid);
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                v.Details = new List<InboundShipmentModel>();
            }
            else
            {
                InboundAWBBatch batch = db.InboundAWBBatches.Find(id);
                v.BATCHID = batch.ID;
                v.BatchDate = batch.BatchDate;
                v.BatchNumber = batch.BatchNumber;
                v.TotalAWB = Convert.ToInt32(batch.TotalAWB);
                if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    customername = db.CustomerMasters.Find(logincustomerid).CustomerName;
                    v.CustomerID = logincustomerid;
                    v.CustomerName = customername;
                }
                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();
                v.Details = InboundShipmentDAO.GetBatchAWBInfo(id);
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;
                }

                customername = "WALK-IN-CUSTOMER";
                customername = "COD-CUSTOMER";
                customername = "FOC CUSTOMER";
                v.TaxPercent = 5;
                ViewBag.StatusType = v.StatusType;
                ViewBag.CourierStatus = v.CourierStatus;
                ViewBag.EditMode = "true";
            }

            return View(v);

        }
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile)
        {
            var customerid = Convert.ToInt32(Session["CustomerId"].ToString());
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<InboundShipmentModel> fileData = GetDataFromCSVFile(importFile.InputStream, customerid);
                InboundAWBBatchModel vm = new InboundAWBBatchModel();
                vm.Details = fileData;
                vm.Details = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
                Session["ShipmentList"] = vm.Details;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<InboundShipmentModel> GetDataFromCSVFile(Stream stream, int CustomerId)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var details = new List<InboundShipmentModel>();
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
                        details = InboundShipmentDAO.GetShipmentValidAWBDetails(branchid, xml, "",CustomerId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }




            return details;
        }

        public ActionResult ShowShipmentList()
        {
            InboundAWBBatchModel vm = new InboundAWBBatchModel();
            vm.Details = (List<InboundShipmentModel>)Session["ShipmentList"];
            return PartialView("AWBList", vm);
        }
        [HttpPost]
        public JsonResult DeleteBatchAWB(int BatchID, DateTime BatchDate, string Details)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            InboundAWBBatch batch = new InboundAWBBatch();
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                string xml = dt.GetXml();


                if (BatchID == 0)
                {
                    batch.BatchDate = BatchDate;
                    batch.BatchNumber = InboundShipmentDAO.GetMaxBathcNo(BatchDate, BranchId, FyearId);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedBy = userid;
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    db.InboundAWBBatches.Add(batch);
                    db.SaveChanges();
                }
                else
                {
                    batch.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedBy = userid;
                    batch = db.InboundAWBBatches.Find(BatchID);
                    db.SaveChanges();
                }
                SaveStatusModel model = new SaveStatusModel();
                model = InboundShipmentDAO.DeleteAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml);
                if (model.Status == "Ok")
                {
                    return Json(new { Status = "Ok", BatchID = batch.ID, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Failed", BatchID = batch.ID, message = model.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveBatchAWB(int BatchID,DateTime BatchDate,int CustomerID,string Details,string DeleteDetails)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            InboundAWBBatch batch = new InboundAWBBatch();
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                string xml = dt.GetXml();


                if (BatchID == 0)
                {
                    batch.BatchDate = BatchDate;
                    batch.BatchNumber= InboundShipmentDAO.GetMaxBathcNo(BatchDate, BranchId, FyearId);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedBy = userid;
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    batch.CustomerID = CustomerID;
                    batch.CourierStatusID = 20; //In Transit
                    db.InboundAWBBatches.Add(batch);
                    db.SaveChanges();
                }
                else
                {
                    batch.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    batch.ModifiedBy = userid;
                    batch = db.InboundAWBBatches.Find(BatchID);
                    db.SaveChanges();
                }
                SaveStatusModel model = new SaveStatusModel();
                model = InboundShipmentDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml);
                if (model.Status == "OK")
                {
                    if (DeleteDetails!="" && DeleteDetails!="[]")
                    {
                        var IDeleteDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(DeleteDetails);
                         
                        DataSet dt1 = new DataSet();
                        dt1 = ToDataTable(IDeleteDetails);
                        string xml1 = dt1.GetXml();
                        SaveStatusModel model1 = InboundShipmentDAO.DeleteAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml1);
                    }
                   
                    return Json(new { Status = "OK", BatchID=batch.ID, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Failed", BatchID = batch.ID, message = model.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteShipment(int id)
        {
            StatusModel obj = new StatusModel();
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCoLoaderShipment(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        obj.Status = dt.Rows[0][0].ToString();
                        obj.Message = dt.Rows[0][1].ToString();
                        //if (dt.Rows[0][0].ToString() == "OK")
                        //    TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                        //else
                        //    TempData["ErrorMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    obj.Status = "Failed";
                    obj.Message = "Error at delete";

                }
            }

            return Json(obj, JsonRequestBehavior.AllowGet);

        }
        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = InboundShipmentDAO.DeleteBatch(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0].ToString() == "OK")
                            TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                        else
                            TempData["ErrorMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return RedirectToAction("Index");

        }

        #endregion


        #region autocomplete
        [HttpPost]
        public ActionResult AutoDataFixation(string Details)
        {
            ImportManifestVM model = new ImportManifestVM();
            var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
            Session["InboundManifestImported"] = IDetails;
            DataTable ds = new DataTable();
            DataSet dt = new DataSet();
            dt = ToDataTable(IDetails);
            ds = dt.Tables[0];
            for (int i = 0; i < ds.Columns.Count; i++)
            {
                var colname = ds.Columns[i].ColumnName;
                int rowindex = 0;
                foreach (DataRow row in ds.Rows)
                {
                    string targetvalue = GetDataFixation(colname, row[colname].ToString());
                    if (targetvalue != "" && targetvalue != null)
                    {
                        ds.Rows[rowindex][colname] = targetvalue;
                    }

                    rowindex++;
                }

            }
            InboundAWBBatchModel vm = new InboundAWBBatchModel();
            vm.Details = ConvertTabletoList(ds);
            vm.Details = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
            Session["InboundManifestImported"] = vm.Details;
            return PartialView("AWBList", vm);
             


        }
        public string GetDataFixation(string FieldName, string SourceValue)
        {
            ImportDataFixation importdata = new ImportDataFixation();
            string Targetvalue = "";
            var data = db.ImportDataFixations.Where(cc => cc.ShipmentType == "Transhipment" && cc.FieldName == FieldName && cc.SourceValue == SourceValue).FirstOrDefault();
            if (data != null)
                Targetvalue = data.TargetValue;

            return Targetvalue;

        }
        [HttpPost]
        public ActionResult ShowImportDataFixation(string FieldName,string Details)
        {
            ImportManifestFixation vm = new ImportManifestFixation();
            var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
            Session["InboundManifestImported"] = IDetails;
            ViewBag.ImportFields = db.ImportFields.ToList();
            return PartialView("ImportDataFixation", vm);
        }
        public  List<InboundShipmentModel> ConvertTabletoList(DataTable dt)
        {
            List<InboundShipmentModel> list = new List<InboundShipmentModel>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    InboundShipmentModel obj = new InboundShipmentModel();

                    obj.ShipmentID = Convert.ToInt32(dt.Rows[i]["ShipmentID"].ToString());
                    obj.SNo = i + 1;
                    obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                    obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                    obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress"].ToString();
                    obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                    obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                    obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                    obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                    obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                    obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress"].ToString();
                    obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                    obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                    obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                    obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                    obj.PaymentModeId = Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                    obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                    obj.ParcelTypeID = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                    obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                    obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                    obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                    obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                    obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                    obj.Pieces = Convert.ToInt32(dt.Rows[i]["Pieces"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                    obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                    obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                    obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                    obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString());
                    obj.CustomerID = Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                obj.Currency = dt.Rows[i]["Currency"].ToString();
                obj.CustomsValue = Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                list.Add(obj);
                }

            
                return list;
            
        }

        [HttpGet]
        public JsonResult GetSourceValue(string term, string FieldName)
        {
            var IDetails = (List<InboundShipmentModel>)Session["InboundManifestImported"];
            if (IDetails != null)
            {
                if (term.Trim() != "")
                {
                    if (FieldName == "DestinationCountry" || FieldName == "ConsigneeCountryName")
                    {
                        var list = (from c in IDetails
                                    where c.ConsigneeCountryName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeCountryName
                                    select new { SourceValue = c.ConsigneeCountryName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationCity" || FieldName == "ConsigneeCityName")
                    {
                        var list = (from c in IDetails
                                    where c.ConsigneeCityName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeCityName
                                    select new { SourceValue = c.ConsigneeCityName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationLocation" || FieldName == "ConsigneeLocationName")
                    {
                        var list = (from c in IDetails
                                    where c.ConsigneeLocationName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeLocationName
                                    select new { SourceValue = c.ConsigneeLocationName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var list = new { SourceValue = "" };
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (FieldName == "DestinationCountry" || FieldName == "ConsigneeCountryName")
                    {
                        var list = (from c in IDetails
                                    orderby c.ConsigneeCountryName
                                    select new { SourceValue = c.ConsigneeCountryName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationCity" || FieldName == "ConsigneeCityName")
                    {
                        var list = (from c in IDetails
                                    orderby c.ConsigneeCityName
                                    select new { SourceValue = c.ConsigneeCityName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationLocation" || FieldName == "ConsigneeLocationName")
                    {
                        var list = (from c in IDetails
                                    orderby c.ConsigneeLocationName
                                    select new { SourceValue = c.ConsigneeLocationName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var list = new { SourceValue = "" };
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            else
            {
                var list = new { SourceValue = "" };
                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult UpdateDataFixation(string TargetColumn, string SourceValue, string TargetValue,string Details)
        {
            ImportManifestVM model = new ImportManifestVM();
            Details.Replace("{}", "");
            var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
            DataTable ds = new DataTable();
            DataSet dt = new DataSet();
            dt = ToDataTable(IDetails);
            ds = dt.Tables[0];
            foreach(DataColumn col in ds.Columns)
            {
                if (col.ColumnName == TargetColumn)
                {
                    for(int i= 0;i < ds.Rows.Count;i++)
                    {
                        if (ds.Rows[i][TargetColumn].ToString()==SourceValue)
                            ds.Rows[i][TargetColumn] = TargetValue;

                    }
                }
            }
            
            SaveDataFixation(TargetColumn, SourceValue, TargetValue);
            
            InboundAWBBatchModel vm = new InboundAWBBatchModel();
            vm.Details = ConvertTabletoList(ds);
            vm.Details = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
            Session["InboundManifestImported"] = vm.Details;
            return PartialView("AWBList", vm);
        }
        public void SaveDataFixation(string FieldName, string SourceValue, string TargetValue)
        {
            ImportDataFixation importdata = new ImportDataFixation();
            var data = db.ImportDataFixations.Where(cc => cc.ShipmentType == "Transhipment" && cc.FieldName == FieldName && cc.SourceValue == SourceValue).FirstOrDefault();
            if (data == null)
            {
                importdata.ShipmentType = "Transhipment";
                importdata.FieldName = FieldName;
                importdata.SourceValue = SourceValue;
                importdata.TargetValue = TargetValue;
                importdata.UpdateDate = CommonFunctions.GetCurrentDateTime();
                db.ImportDataFixations.Add(importdata);
                db.SaveChanges();
            }
            else
            {

                data.TargetValue = TargetValue;
                data.UpdateDate = CommonFunctions.GetCurrentDateTime();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }


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

        [HttpGet]
        public JsonResult GetShipperName(string term)
        {
        
            var customerid = Convert.ToInt32(Session["CustomerId"].ToString());
            
            if (term.Trim() != "")
            {
              var shipperlist = (from c1 in db.InboundShipments
                               where c1.Consignor.ToLower().Contains(term.Trim().ToLower())
                               && c1.CustomerID == customerid
                               orderby c1.Consignor ascending
                               select new Consignor { ShipperName = c1.Consignor, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                return Json(shipperlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
              
                var shipperlist = (from c1 in db.InboundShipments
                               where c1.CustomerID == customerid
                               orderby c1.Consignor ascending
                               select new Consignor { ShipperName = c1.Consignor, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();
                
                return Json(shipperlist, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet]
        public JsonResult GetConsigneeName(string term, string Shipper = "", bool ShowAll = false)
        {
            if (term.Trim() != "")
            {
                if (ShowAll == false)
                {
           var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignee.ToLower().Contains(term.Trim().ToLower())
                                       && c1.Consignor == Shipper
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building }).Distinct().OrderBy(cc => cc.ConsignorName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignee.ToLower().Contains(term.Trim().ToLower())

                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ShowAll == false)
                {

                    var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignor == Shipper
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.InboundShipments
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion
    }
}