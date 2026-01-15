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
    public class AWBController : Controller
    {
        Entities1 db = new Entities1();

        [HttpGet]
        public ActionResult Index(int id=0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            AWBSearch obj= (AWBSearch)Session["AWBSearch"];
            AWBSearch model = new AWBSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            string _USERTYPE = Session["UserType"].ToString();
            int CreatedBy = 0;
            if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
            {
                CreatedBy = userid;

            }
                if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                
                obj = new AWBSearch();
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
                model.Details = new List<QuickAWBVM>();
            }
            else
            {
                model = obj;
                List<QuickAWBVM> lst = PickupRequestDAO.GetAWBList(obj.StatusID, obj.FromDate, obj.ToDate, branchid, depotId, model.AWBNo, obj.MovementTypeID, obj.PaymentModeId, obj.ConsignorConsignee, obj.Origin, obj.Destination,CreatedBy);
                model.Details = lst;
            }
                       
            ViewBag.CourierStatus = db.CourierStatus.Where(cc=>cc.CourierStatusID>=4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc=>cc.CourierStatusID>=4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }

        [HttpPost]
        public ActionResult Index(AWBSearch obj)
        {
            Session["AWBSearch"] = obj;
            return RedirectToAction("Index");
        }
        public ActionResult Create(int id=0)
        {           
            
                int uid = Convert.ToInt32(Session["UserID"].ToString());
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            Session["CityList"] = null;
            Session["CountryList"] = null;
            Session["LocationList"] = null;
            ViewBag.Employee = db.EmployeeMasters.ToList();
            //ViewBag.FAgent = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList(); // )// .ForwardingAgentMasters.ToList();
            ViewBag.FAgent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3 || cc.SupplierTypeID == 4).ToList(); //  db.ForwardingAgentMasters.OrderBy(cc=>cc.FAgentName).ToList();
                ViewBag.Movement = db.CourierMovements.ToList();
                ViewBag.ProductType = db.ProductTypes.ToList();
                ViewBag.parceltype = db.ParcelTypes.ToList();
                ViewBag.customerrate = db.CustomerRateTypes.OrderBy(cc=>cc.CustomerRateType1).ToList();
               //not using ViewBag.CourierDescription = db.CourierDescriptions.ToList(); // not used
                ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();
                ViewBag.CourierStatusList = db.CourierStatus.ToList();
                ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
                ViewBag.PaymentMode = db.tblPaymentModes.ToList();
                ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();
            string _USERTYPE = Session["UserType"].ToString();
            ViewBag.UserType = _USERTYPE;
            List<OtherChargeDetailVM> otherchargesvm = new List<OtherChargeDetailVM>();
            List<ShipmentItemDetailVM> shipmentItemVM = new List<ShipmentItemDetailVM>();
            QuickAWBVM v = new QuickAWBVM();
            string customername = "";

            v.DRRProcess = false; //retrieve from branch master
            customername = "WALK-IN-CUSTOMER";
            v.CASHCustomerId = -1; 
            v.CASHCustomerName = customername;
            
            //var CashCustomer = (from c1 in db.CustomerMasters
            //                    where c1.CustomerName.Trim() == customername
            //                    orderby c1.CustomerName ascending
            //                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            //if (CashCustomer != null)
            //{
            //    v.CASHCustomerId = CashCustomer.CustomerID;
            //    v.CASHCustomerName = customername;
            //}

            customername = "COD-CUSTOMER";
            v.CODCustomerID = -2;
            v.CODCustomerName = "COD-CUSTOMER";

            //var CODCustomer = (from c1 in db.CustomerMasters
            //                   where c1.CustomerName == customername
            //                   orderby c1.CustomerName ascending
            //                   select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            //if (CODCustomer != null)
            //{
            //    v.CODCustomerID = CODCustomer.CustomerID;
            //    v.CODCustomerName = "COD-CUSTOMER";
            //}

            customername = "FOC CUSTOMER";
            v.FOCCustomerID = -3;
            v.FOCCustomerName = "FOC CUSTOMER";

            //var FOCCustomer = (from c1 in db.CustomerMasters
            //                   where c1.CustomerName == customername
            //                   orderby c1.CustomerName ascending
            //                   select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            //if (FOCCustomer != null)
            //{
            //    v.FOCCustomerID = FOCCustomer.CustomerID;
            //    v.FOCCustomerName = "FOC CUSTOMER";
            //}
            var FAgent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3 && cc.StatusDefault == true).FirstOrDefault(); // . ForwardingAgentMasters.Where(cc => cc.StatusDefault == true).FirstOrDefault();
            if (FAgent != null)
            {
                v.DefaultFAgentName = FAgent.SupplierName;
                v.DefaultFAgentID = FAgent.SupplierID;
            }
            else
            {
                v.DefaultFAgentID = 0;
                v.DefaultFAgentName = "";
            }
            if (id == 0)
                {
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    v.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    v.MovementTypeID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    v.ParcelTypeID = defaultparceltype.ID;

                var branchtax = db.BranchMasters.Find(branchid).TaxEnable;
                if (branchtax == true)
                    v.ChkTaxPercent = true;

                 var shipmentmode = db.tblShipmentModes.Where(cc=>cc.ByDefault==true).FirstOrDefault();
                if (shipmentmode == null)
                    v.ShipmentModeID = 0;
                else
                    v.ShipmentModeID = shipmentmode.ID;
                
                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();//.Where(dd => dd.CourierStatusID == 4).ToList();
                    PickupRequestDAO doa = new PickupRequestDAO();
                    var awbgenerate = db.AcCompanies.Find(companyId).IsAWBAutoGenrated;
                var AWBFOCZero = db.AcCompanies.Find(companyId).AWBFOCZero;
                v.AWBFOCZero = AWBFOCZero;
                if (awbgenerate == null)
                {
                    awbgenerate = false;
                    v.AWBAutoGenerated = false;
                }
                if (Convert.ToBoolean(awbgenerate))
                {
                    ViewBag.AutoGenerate = "true";
                    ViewBag.AWBNO = doa.GetMaAWBNo(companyId, branchid,v.ShipmentModeID);
                    v.HAWBNo = ViewBag.AWBNo;
                    v.AWBAutoGenerated = true;
                }
                else
                {
                    ViewBag.AutoGenerate = "false";
                    ViewBag.AWBNO = "";
                    v.HAWBNo = "";
                    v.AWBAutoGenerated = false;
                }
                    ViewBag.CourierStatusId = 0;
                var branch = db.BranchMasters.Find(branchid);               
                if (branch != null)
                {
                    v.BranchLocation = branch.LocationName;
                    v.BranchCountry = branch.CountryName;
                    v.BranchCity = branch.CityName;
                    v.DRRProcess = branch.DRRProcess;
                }
                v.InScanID = 0;
                v.TaxPercent = 5;
                v.CurrencyID = CommonFunctions.GetDefaultCurrencyId();
                v.DefaultSurchargePercent = PickupRequestDAO.GetSurcharge();
                v.PaymentModeId = 3;
                v.CustomerID = v.CASHCustomerId;
                v.customer = v.CASHCustomerName;
                v.FagentID = v.DefaultFAgentID;
                    ViewBag.EditMode = "false";
                    int userId = Convert.ToInt32(Session["UserID"].ToString());
                if (Session["UserType"].ToString()=="Employee")
                { 
                    var useremp = (from e in db.EmployeeMasters where e.UserID == userId select e).First();
                    if (useremp != null)
                    {
                        if (useremp.AcHeadID != null)
                        {
                            v.AcheadID = Convert.ToInt32(useremp.AcHeadID);
                            var achead = db.AcHeads.Find(Convert.ToInt32(v.AcheadID));

                            if (achead != null)
                            {
                                v.AcHeadName = achead.AcHead1;
                            }
                        }
                    }
                }
                if (_USERTYPE == "Customer")
                {
                    int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    var _logincustomer = db.CustomerMasters.Find(logincustomerid);
                    v.CustomerID = logincustomerid;
                    v.customer = _logincustomer.CustomerName; 
                    v.shippername = _logincustomer.CustomerName;
                    v.ConsignorContact = _logincustomer.ContactPerson;
                    v.ConsignorAddress1_Building = _logincustomer.Address1;
                    v.ConsignorAddress2_Street = _logincustomer.Address2;
                    v.ConsignorAddress3_PinCode = _logincustomer.Address3;
                    v.ConsignorCityName = _logincustomer.CityName;
                    v.ConsignorCountryName = _logincustomer.CountryName;
                    v.ConsignorMobile = _logincustomer.Mobile;
                    v.ConsignorPhone = _logincustomer.Phone;

                }

                v.otherchargesVM = otherchargesvm;
                v.shipmentItemVM = shipmentItemVM;
                v.DRRID = 0;
                ViewBag.PeriodLock = "false";
                ViewBag.PeriodLockMessage = "";
                ViewBag.Title = "Create";
                v.TransactionDate = CommonFunctions.GetCurrentDateTime();
                StatusModel result = AccountsDAO.CheckDateValidate(v.TransactionDate.ToString(), yearid);
                if (result.Status == "PeriodLock" || result.Status=="YearClose") //Period locked
                {
                    ViewBag.PeriodLock = "true";
                    ViewBag.PeriodLockMessage = result.Message;
                }
                else
                {
                    ViewBag.PeriodLock = "false";
                    ViewBag.PeriodLockMessage = "";
                }
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
                if (v.AWBAutoGenerated == true)
                    ViewBag.AutoGenerate = "true";
                else
                    ViewBag.AutoGenerate = "false";

                
                customername = "WALK-IN-CUSTOMER";
                v.CASHCustomerId = -1;
                v.CASHCustomerName = customername;
                customername = "COD-CUSTOMER";
                v.CODCustomerID = -2;
                v.CODCustomerName = "COD-CUSTOMER";
                customername = "FOC CUSTOMER";
                v.FOCCustomerID = -3;
                v.FOCCustomerName = "FOC CUSTOMER";
                if ((v.CustomerID==0 || v.CustomerID==-1) && v.PaymentModeId==1)
                {
                    v.customer = v.CASHCustomerName;
                    v.CustomerID = v.CASHCustomerId;
                }
                else if((v.CustomerID==-2 || v.CustomerID==0) && v.PaymentModeId==2)
                        {
                    v.customer = v.CODCustomerName;
                    v.CustomerID = v.CODCustomerID;
                }
                else if ((v.CustomerID == -3 || v.CustomerID == 0) && v.PaymentModeId == 4)
                {
                    v.customer = v.CODCustomerName;
                    v.CustomerID = v.CODCustomerID;
                }
                v.TaxPercent = 5;
                v.DefaultSurchargePercent = PickupRequestDAO.GetSurcharge();
                otherchargesvm = (from c in db.InscanOtherCharges join o in db.OtherCharges on c.OtherChargeID equals o.OtherChargeID where c.InscanID == id select new OtherChargeDetailVM { InscanID = id, OtherChargeID = c.OtherChargeID, OtherChargeName = o.OtherCharge1, Amount = c.Amount,Deleted=false }).ToList();
                if (otherchargesvm == null)
                { 
                    otherchargesvm  = new List<OtherChargeDetailVM>();
                    v.otherchargesVM = otherchargesvm;
                }                
                else {
                    v.otherchargesVM = otherchargesvm;
                }

                shipmentItemVM = (from c in db.InScanMasterItems where c.InScanID == id select new ShipmentItemDetailVM { ID=c.ID, InScanID = id, BoxName=c.BoxName,Contents=c.Contents , Qty=c.Qty,Value=c.Value , WeightPerCarton=c.WeightPerCarton,TotalWeight=c.TotalWeight,  Deleted = false }).ToList();
                if (otherchargesvm == null)
                {
                    shipmentItemVM = new List<ShipmentItemDetailVM>();
                    v.shipmentItemVM= shipmentItemVM;
                }
                else
                {
                    v.shipmentItemVM = shipmentItemVM;
                }
                ViewBag.AWBNo = v.HAWBNo;
                    if (v.CourierStatusId == null)
                        ViewBag.CourierStatusId = 0;
                    else
                        ViewBag.CourierStatusId = v.CourierStatusId;
                    ViewBag.StatusType = v.StatusType;
                    ViewBag.CourierStatus = v.CourierStatus;
                    ViewBag.EditMode = "true";
                
                StatusModel result = AccountsDAO.CheckDateValidate(v.TransactionDate.ToString(), yearid);
                if (result.Status == "PeriodLock" || result.Status=="YearClose") //Period locked
                {
                    ViewBag.PeriodLock = "true";
                    ViewBag.PeriodLockMessage = result.Message;
                }
                else
                {
                    ViewBag.PeriodLock = "false";
                    ViewBag.PeriodLockMessage = "";
                }
               }

                return View(v);
            
        }

        [HttpPost]
        public ActionResult Create(QuickAWBVM v)
        {
            string customersavemessage = "";
            try
            {
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                int userid = Convert.ToInt32(Session["UserID"].ToString());
                string AWBNo = string.Empty;
                int DepotInScanID = 0;
                PickupRequestDAO _dao = new PickupRequestDAO();
                if (v.InScanID == 0)
                {                   
                    var awbgenerate = db.AcCompanies.Find(companyId).IsAWBAutoGenrated;
                    if (awbgenerate == null)
                        awbgenerate = false;
                    if (Convert.ToBoolean(awbgenerate) && v.AWBAutoGenerated==true)
                    {
                        AWBNo = _dao.GetMaAWBNo(companyId, branchid);
                        v.HAWBNo = AWBNo;
                    }
                    else
                    {
                        int checkinscan = PickupRequestDAO.CheckAWB(v.HAWBNo).InScanID;
                        //var checkinscan = db.InScanMasters.Where(cc => cc.AWBNo == v.HAWBNo && cc.IsDeleted==false).FirstOrDefault(); 
                        //if (checkinscan!=null)
                        if (checkinscan>0)
                        {
                            DepotInScanID = checkinscan;//.InScanID;
                            
                        }


                    }
                }
                try
                {
                    
                    InScanMaster inscan = new InScanMaster();

                    if (v.InScanID == 0)
                    {
                        //int id = (from c in db.InScanMasters orderby c.InScanID descending select c.InScanID).FirstOrDefault();
                        //inscan.InScanID = id + 1;
                        if (DepotInScanID > 0)
                        {
                            inscan = db.InScanMasters.Find(DepotInScanID);
                            v.InScanID = inscan.InScanID;
                            var awbdetail = db.AWBDetails.Where(cc => cc.AWBNo == v.HAWBNo && (cc.InScanID==0 || cc.InScanID==null)).FirstOrDefault();
                            if (awbdetail != null)
                            {
                                awbdetail.InScanID = v.InScanID;
                                db.Entry(awbdetail).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        inscan.AWBNo = v.HAWBNo; // _dao.GetMaAWBNo(companyId, branchid);
                        inscan.AcCompanyID = companyId;
                        inscan.BranchID = branchid;
                        inscan.DepotID = depotId;
                        inscan.AcFinancialYearID = yearid;
                        inscan.TransactionDate = v.TransactionDate;
                            if (inscan.DeviceID==null)
                            inscan.DeviceID = "WebSite";
                        if (inscan.EnteredByID==null)
                            inscan.EnteredByID = Convert.ToInt32(Session["UserID"]);
                        inscan.EnquiryNo = null;
                        inscan.IsEnquiry = false;
                        //inscan.PickupRequestStatusId = 4;
                        inscan.ShipmentModeID = v.ShipmentModeID;
                        inscan.AWBAutoGenerated = v.AWBAutoGenerated;
                        if (inscan.StatusTypeId==null)
                            inscan.StatusTypeId = 1;
                        if (inscan.CourierStatusID==null)
                            inscan.CourierStatusID = 4;
                        inscan.ConsignorCountryName = v.ConsignorCountryName;
                        inscan.ConsignorCityName = v.ConsignorCityName;
                        inscan.ConsignorLocationName = v.ConsignorLocationName;
                        inscan.CustomerShipperSame = v.CustomerandShipperSame;
                        
                        inscan.Consignor = v.shippername;                       
                        
                        inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                        inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;
                                    
                        inscan.ConsignorContact = v.ConsignorContact;

                        if (inscan.CreatedBy==null)
                            inscan.CreatedBy = userid;
                        
                        if (inscan.CreatedDate==null)
                            inscan.CreatedDate = CommonFunctions.GetCurrentDateTime();

                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetCurrentDateTime();

                        if (v.PaymentModeId != null)
                        {
                            if (v.PaymentModeId == 3 && (v.CustomerID==0))
                            {
                                //int _customerid = SaveCustomer(v);
                                //customersavemessage = "New Customer - Customer saved as 'Cash Customer' in the system";
                                inscan.CustomerID = v.CustomerID;
                            }
                            else if (v.PaymentModeId == 1)
                            {
                                inscan.CustomerID = -1; //WALK-IN-CUSTOMER
                            }
                            else if (v.PaymentModeId == 2)
                            {
                                inscan.CustomerID = -2; //COD-CUSTOMER
                            }
                            else if (v.PaymentModeId == 5)
                            {
                                inscan.CustomerID = -3; //FOC-CUSTOMER
                            }
                            else
                            {
                                inscan.CustomerID = v.CustomerID;
                            }


                        }
                        inscan.TransactionDate = v.TransactionDate;
                    }
                    else
                    {
                        inscan = db.InScanMasters.Find(v.InScanID);
                        
                        
                        if (v.PaymentModeId != null)
                        {
                            if (v.PaymentModeId == 3)
                            {
                                if (v.CustomerID == 0)
                                {
                                    int _customerid = SaveCustomer(v);
                                    customersavemessage = "New Customer - Customer saved as 'Cash Customer' in the system";
                                    inscan.CustomerID = _customerid;
                                }
                                else
                                {
                                    inscan.CustomerID = v.CustomerID;
                                }
                            }
                            else if(v.PaymentModeId==1)
                            {
                                inscan.CustomerID = -1; //WALK-IN-CUSTOMER
                            }
                            else if (v.PaymentModeId == 2)
                            {
                                inscan.CustomerID = -2; //COD-CUSTOMER
                            }
                            else if (v.PaymentModeId == 5)
                            {
                                inscan.CustomerID = -3; //COD-CUSTOMER
                            }
                            else
                            {
                                inscan.CustomerID = v.CustomerID;
                            }
                        }

                        inscan.TransactionDate = v.TransactionDate;
                        DateTime localDateTime1 = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                    }
                    inscan.Weight = v.Weight;
                    inscan.ConsignorCountryName = v.ConsignorCountryName;
                    inscan.ConsignorCityName = v.ConsignorCityName;
                    inscan.ConsignorLocationName = v.ConsignorLocationName;
                    inscan.CustomerShipperSame = v.CustomerandShipperSame;

                    inscan.Consignor = v.shippername;

                    inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                    inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;

                    inscan.ConsignorContact = v.ConsignorContact;
                    inscan.ConsignorPhone = v.ConsignorPhone;
                    inscan.ConsignorMobileNo = v.ConsignorMobile;
                    inscan.ConsignorAddress3_PinCode = v.ConsignorAddress3_PinCode;
                    inscan.ConsignorContact = v.ConsignorContact;
                    //inscan.AcJournalID = ajm.AcJournalID;
                    if (v.AcheadID!=null)
                    {
                        inscan.AcHeadID = v.AcheadID;
                    }

                    if (v.CourierCharge != null)
                    {
                        inscan.CourierCharge = Convert.ToDecimal((v.CourierCharge));
                    }
                    
                    if (v.OtherCharge != null)
                    {
                        inscan.OtherCharge = Convert.ToDecimal(v.OtherCharge);
                    }
                 
                    if (v.Weight!=null)
                    {
                        inscan.Weight = Convert.ToDecimal(v.Weight);
                    }
               
                    inscan.PaymentModeId = v.PaymentModeId;

                    if (v.ParentInScanID == null)
                        inscan.ParentInScanID = 0;
                    else
                        inscan.ParentInScanID = v.ParentInScanID;
                  
                    inscan.Consignee = v.Consignee;
                    inscan.ConsigneeCountryName = v.ConsigneeCountryName;
                    inscan.ConsigneeCityName = v.ConsigneeCityName;
                    inscan.ConsigneeLocationName = v.ConsigneeLocationName;
                    inscan.ConsigneeAddress1_Building = v.ConsigneeAddress1_Building;
                    inscan.ConsigneeAddress2_Street = v.ConsigneeAddress2_Street;
                    inscan.ConsigneeAddress3_PinCode = v.ConsigneeAddress3_PinCode;

                    inscan.ConsigneePhone = v.ConsigneePhone;
                    inscan.ConsigneeMobileNo = v.ConsigneeMobile;
                    inscan.ConsigneeContact = v.ConsigneeContact;
                    

                    inscan.Pieces = v.Pieces.ToString();

                    inscan.ProductTypeID = v.ProductTypeID;
                    inscan.ParcelTypeId= v.ParcelTypeID;
                    inscan.CourierCharge = v.CourierCharge;
                    inscan.TaxPercent = v.TaxPercent;
                    inscan.TaxAmount = v.TaxAmount;
                    inscan.SurchargePercent = v.SurchargePercent;
                    inscan.SurchargeAmount = v.SurchargeAmount;
                    inscan.CustomerRateID = v.CustomerRateTypeID;
                    inscan.MovementID = v.MovementTypeID;
                    inscan.OtherCharge = v.OtherCharge;
                    inscan.CargoDescription = v.Description;
                    inscan.Remarks = v.remarks;
                    inscan.FAgentId = v.FagentID;
                    inscan.ForwardingAWBNo = v.FWDAgentNumber;
                    inscan.ForwardingCharge = v.ForwardingCharge;
                   // inscan.AWBProcessed = Convert.ToBoolean(v.AWBProcessed);

                    if (v.CustomCharge !=null )
                    {
                        inscan.CustomsValue = Convert.ToDecimal(v.CustomCharge);
                    }

                    if (v.materialcost != null)
                    {
                        inscan.MaterialCost = Convert.ToDecimal(v.materialcost);
                    }
                    if (v.totalCharge != null) 
                    {
                        inscan.NetTotal = Convert.ToDecimal(v.totalCharge);
                    }

                    //inscan.InScanDate = DateTime.UtcNow;


                    if (inscan.PickedUpEmpID==null)
                      inscan.PickedUpEmpID =v.PickedBy;

                    if (inscan.DepotReceivedBy==null)
                    inscan.DepotReceivedBy = v.ReceivedBy;

                    inscan.IsNCND = v.IsNCND;
                    inscan.IsCashOnly = v.IsCashOnly;
                    inscan.IsChequeOnly = v.IsChequeOnly;
                    inscan.IsCollectMaterial = v.IsCollectMaterial;
                    inscan.IsDOCopyBack = v.IsDOCopyBack;
                    inscan.PickupLocation = v.PickupLocation;
                    inscan.DeliveryLocation = v.DeliveryLocation;
                    inscan.PickupSubLocality = v.PickupSubLocality;
                    inscan.DeliverySubLocality = v.DeliverySubLocality;
                    inscan.OriginPlaceID = v.PickupLocationPlaceId;
                    inscan.DestinationPlaceID = v.DeliveryLocationPlaceId;
                    inscan.AWBProcessed = true;

                    InScanInternationalDeatil isid = new InScanInternationalDeatil();
                    InScanInternational isi = new InScanInternational();
                    if (v.InScanID == 0 && DepotInScanID==0)
                    {
                        inscan.EntrySource = 1; //AWB create by create shipment page
                        if (inscan.PickedUpEmpID != null)
                        {
                            inscan.StatusTypeId = 1;
                            inscan.CourierStatusID = 4;
                        }

                        if (inscan.DepotReceivedBy != null)
                        {
                            inscan.StatusTypeId = 2; //Inscan
                            inscan.CourierStatusID = 5; //received at origin facility
                        }

                        if (inscan.PickedUpEmpID == null && inscan.DepotReceivedBy == null)
                        {
                            inscan.StatusTypeId = 1;
                            inscan.CourierStatusID = 2;
                        }

                        inscan.IsDeleted = false;
                        db.InScanMasters.Add(inscan);
                        db.SaveChanges();


                        //add status of awbdetail
                        var awbdetail = db.AWBDetails.Where(cc => cc.AWBNo == v.HAWBNo && (cc.InScanID==null || cc.InScanID==0)).FirstOrDefault();
                        if (awbdetail != null)
                        {
                            awbdetail.InScanID = v.InScanID;
                            db.Entry(awbdetail).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        if (inscan.PickedUpEmpID != null)
                        {
                            
                            AddAWBTrackStatus(inscan.InScanID,Convert.ToInt32(inscan.PickedUpEmpID),1,4);
                        }

                        if (inscan.DepotReceivedBy != null)
                        {
                            AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(inscan.DepotReceivedBy),2,5);
                        }

                        if (inscan.PickedUpEmpID == null && inscan.DepotReceivedBy == null)
                        {
                            var empid = db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault();
                            if (empid!=null)
                               AddAWBTrackStatus(inscan.InScanID,Convert.ToInt32(empid.EmployeeID),1,2);
                        }
                        
                        //if (inscan.MaterialCost >0)
                        //{
                        //    MaterialCostMaster mc = new MaterialCostMaster();
                        //    mc.MCDate = inscan.TransactionDate;
                        //    mc.InScanID = inscan.InScanID;                            
                        //    mc.MaterialCost =Convert.ToDecimal(inscan.MaterialCost);
                        //    mc.Status = "INSCAN";
                        //    db.MaterialCostMasters.Add(mc);
                        //    db.SaveChanges();
                        //}
                      
                        //if (v.PaymentModeId == 2)
                        //{ SaveConsignee(v); }

                        isid.InScanID = inscan.InScanID;
                        isi.InScanID = inscan.InScanID;
                        isi.InScanInternationalID = 0;
                        TempData["SuccessMsg"] = customersavemessage  + "\n" + "You have successfully Added Quick AirWay Bill.";

                    }
                    else
                    {
                        if (inscan.CourierStatusID < 4)
                        {
                            if (inscan.PickedUpEmpID != null)
                            {
                                inscan.StatusTypeId = 1;
                                inscan.CourierStatusID = 4;
                            }
                        }
                       if (inscan.CourierStatusID < 5) { 
                            if (inscan.DepotReceivedBy != null)
                            {
                                inscan.StatusTypeId = 2; //Inscan
                                inscan.CourierStatusID = 5; //received at origin facility
                            }
                       }
                       
                        db.Entry(inscan).State = EntityState.Modified;
                        db.SaveChanges();

                        ////Material cost table update
                        //MaterialCostMaster mc = new MaterialCostMaster();
                        //mc = db.MaterialCostMasters.Where(cc => cc.InScanID == inscan.InScanID).FirstOrDefault();
                        //if (mc!=null)
                        //{
                        //    if (mc.MaterialCost != inscan.MaterialCost && mc.Status=="INSCAN")
                        //    {
                        //        mc.MaterialCost = Convert.ToDecimal(inscan.MaterialCost);                                
                        //        db.Entry(mc).State = EntityState.Modified;
                        //        db.SaveChanges();
                        //    }
                        //}
                        //else if(mc==null && inscan.MaterialCost>0)
                        //{
                        //    mc = new MaterialCostMaster();
                        //    mc.MCDate = inscan.TransactionDate;
                        //    mc.InScanID = inscan.InScanID;
                        //    mc.Status = "INSCAN";
                        //    mc.MaterialCost = Convert.ToDecimal(inscan.MaterialCost);
                        //    db.MaterialCostMasters.Add(mc);
                        //    db.SaveChanges();
                        //}

                        //if (v.PaymentModeId == 1 || v.PaymentModeId == 2)
                        //    _dao.AWBAccountsPosting(inscan.InScanID);

                        //SaveConsignee(v);
                        TempData["SuccessMsg"] = customersavemessage + "\n" +  "You have successfully updated Airway Bill";
                        
                    }

                  

                    if (v.FagentID >0)
                    {
                        isid = db.InScanInternationalDeatils.Where(cc => cc.InScanID == v.InScanID).FirstOrDefault();
                        isi = db.InScanInternationals.Where(cc => cc.InScanID == v.InScanID).FirstOrDefault();
                        if (isid == null)
                        {
                            isid = new InScanInternationalDeatil();
                            isid.InScanID = inscan.InScanID;
                        }
                        if (isi == null)
                        {
                            isi = new InScanInternational();
                            isi.InScanID = inscan.InScanID;                            
                        }

                        if (v.FagentID != null)
                        {
                            isi.FAgentID = Convert.ToInt32(v.FagentID);
                        }

                        if (v.ForwardingCharge != null)
                        {
                            isi.ForwardingCharge = Convert.ToDecimal(v.ForwardingCharge);
                            isid.ForwardingCharge = Convert.ToDecimal(v.ForwardingCharge);
                        }
                        isi.StatusAssignment = false;
                        isi.ForwardingAWBNo = v.FAWBNo;
                        isi.ForwardingDate = inscan.TransactionDate; // DateTime.UtcNow;
                        isi.VerifiedWeight =Convert.ToDouble(inscan.Weight);

                        isid.VerifiedWeight = inscan.Weight;

                        if (isi.InScanInternationalID==0)
                        {
                            db.InScanInternationals.Add(isi);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(isi).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        if (isid.InScanInternationalDeatilID == 0)
                        {                           
                          
                            isid.InscanInternationalID = isi.InScanInternationalID;
                            db.InScanInternationalDeatils.Add(isid);
                            db.SaveChanges();
                        }
                        else
                        {

                            db.Entry(isid).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    #region "Othercharge"
                    //Other charge data update into inscanothercharge table
                    if (v.otherchargesVM != null)
                        {
                            for (int j = 0; j < v.otherchargesVM.Count; j++)
                            {

                            if (v.otherchargesVM[j].Deleted == false)
                            {
                                int oid = Convert.ToInt32(v.otherchargesVM[j].OtherChargeID);
                                InscanOtherCharge objOtherCharge = db.InscanOtherCharges.Where(cc => cc.InscanID == inscan.InScanID && cc.OtherChargeID == oid).FirstOrDefault();
                                if (objOtherCharge == null)
                                {
                                    objOtherCharge = new InscanOtherCharge();
                                    var maxid = (from c in db.InscanOtherCharges orderby c.InscanOtherChargeID descending select c.InscanOtherChargeID).FirstOrDefault();
                                    objOtherCharge.InscanOtherChargeID = maxid + 1;
                                    objOtherCharge.InscanID = inscan.InScanID;
                                    objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    objOtherCharge.Amount = v.otherchargesVM[j].Amount;
                                    db.InscanOtherCharges.Add(objOtherCharge);
                                    db.SaveChanges();
                                    db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                                else
                                {
                                    //objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    objOtherCharge.Amount = v.otherchargesVM[j].Amount;
                                    db.Entry(objOtherCharge).State = EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                            }
                            else
                            {
                                int oid = Convert.ToInt32(v.otherchargesVM[j].OtherChargeID);
                                InscanOtherCharge objOtherChargeDel = db.InscanOtherCharges.Where(cc => cc.InscanID == inscan.InScanID && cc.OtherChargeID == oid).FirstOrDefault();

                                    if (objOtherChargeDel != null)
                                    {
                                            db.InscanOtherCharges.Remove(objOtherChargeDel);
                                            db.SaveChanges();
                                    }
                            }
                            }

                         

                    }

                    #endregion
                    //Other charge save end

                    #region "Itemshipment"
                    if (v.shipmentItemVM != null)
                    {
                        for (int j = 0; j < v.shipmentItemVM.Count; j++)
                        {

                            if (v.shipmentItemVM[j].Deleted == false)
                            {
                                int oid = Convert.ToInt32(v.shipmentItemVM[j].ID);
                                InScanMasterItem objItem = db.InScanMasterItems.Where(cc => cc.InScanID == inscan.InScanID && cc.ID == oid).FirstOrDefault();
                                if (objItem == null)
                                {
                                    objItem = new  InScanMasterItem();

                                    objItem.InScanID = inscan.InScanID;
                                    if (v.shipmentItemVM[j].BoxName == null)
                                        v.shipmentItemVM[j].BoxName = "";
                                    objItem.BoxName = v.shipmentItemVM[j].BoxName;
                                    objItem.Contents = v.shipmentItemVM[j].Contents;
                                    if (v.shipmentItemVM[j].Qty == null)
                                    {
                                        objItem.Qty = 0;
                                    }
                                    else
                                    {
                                        objItem.Qty = v.shipmentItemVM[j].Qty;
                                    }
                                    if (v.shipmentItemVM[j].WeightPerCarton == null)
                                    {
                                        objItem.WeightPerCarton= 0;
                                    }
                                    else
                                    {
                                        objItem.WeightPerCarton= v.shipmentItemVM[j].WeightPerCarton;
                                    }
                                    if (v.shipmentItemVM[j].TotalWeight == null)
                                    {
                                        objItem.TotalWeight = 0;
                                    }
                                    else
                                    {
                                        objItem.TotalWeight = v.shipmentItemVM[j].TotalWeight;
                                    }

                                    if (v.shipmentItemVM[j].Value == null)
                                        objItem.Value = 0;

                                    else
                                    {
                                        objItem.Value = v.shipmentItemVM[j].Value;
                                    }
                                    
                                    db.InScanMasterItems.Add(objItem);
                                    db.SaveChanges();
                                    db.Entry(objItem).State = EntityState.Detached;
                                }
                                else
                                {
                                    //objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    //objItem.Amount = v.otherchargesVM[j].Amount;
                                    //db.Entry(objOtherCharge).State = EntityState.Modified;
                                    //db.SaveChanges();
                                    //db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                            }
                            else
                            {
                                int oid = Convert.ToInt32(v.shipmentItemVM[j].ID);
                                InScanMasterItem objItemDel = db.InScanMasterItems.Where(cc => cc.InScanID == inscan.InScanID && cc.ID == oid).FirstOrDefault();

                                if (objItemDel != null)
                                {
                                    db.InScanMasterItems.Remove(objItemDel);
                                    db.SaveChanges();
                                }
                            }
                        }



                    }

                    #endregion

                    if (v.ParentInScanID != null)
                    {
                        int parentinscanid = Convert.ToInt32(v.ParentInScanID);
                        if (parentinscanid > 0)
                        {
                            var parentinscan = db.InScanMasters.Find(parentinscanid);
                            parentinscan.ChildInScanID = inscan.InScanID;
                            db.Entry(parentinscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    try
                    {
                        SaveConsignorAddress(v);
                        SaveConsigeeAddress(v);
                    }
                    catch (Exception ex)
                    {

                    }

                    //accounts posting  for payment mode pickupcash and cod and Account on 30/nov/2020
                    //if (v.PaymentModeId == 1 || v.PaymentModeId == 2)
                    if (inscan.DRRId == null)
                        inscan.DRRId = 0;
                    if (inscan.DRRId==0 || inscan.EntrySource==3)//drr wise awb
                    _dao.AWBAccountsPosting(inscan.InScanID);


                    return RedirectToAction("Index");


                }
                catch (Exception ex)
                {
                    TempData["SuccessMsg"] = ex.Message;
                }


            }
            catch (Exception ex)
            {
                TempData["SuccessMsg"] = ex.Message;
            }
            ViewBag.Customer = db.CustomerMasters.ToList();
            //ViewBag.City = db.CityMasters.ToList();
            //ViewBag.Location = db.LocationMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.FAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.customerrate = db.CustomerRateTypes.ToList();
            ViewBag.CourierDescription = db.CourierDescriptions.ToList();
            ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();
            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            return View(v);
        }


        [HttpPost]
        public JsonResult SaveAWB(QuickAWBVM v,string OtherCharge,string ItemDescription)
        {
            string customersavemessage = "";
            try
            {
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                int userid = Convert.ToInt32(Session["UserID"].ToString());
             
                StatusModel result = AccountsDAO.CheckDateValidate(v.TransactionDate.ToString(), yearid);
                if (result.Status == "PeriodLock") //Period locked
                {
                    return Json(new { status = "Failed", InscanId = 0, message = result.Message });
                }
                else if (result.Status=="Failed")
                {
                    return Json(new { status = "Failed", InscanId = 0, message = result.Message });
                }
                else
                {
                   
                }
            

                if (OtherCharge != "")
                {
                    var IDetailsOtherCharge = JsonConvert.DeserializeObject<List<OtherChargeDetailVM>>(OtherCharge);
                    v.otherchargesVM = IDetailsOtherCharge;
                }
                if (ItemDescription!="")
                {
                    var IDetailsitem = JsonConvert.DeserializeObject<List<ShipmentItemDetailVM>>(OtherCharge);
                    v.shipmentItemVM = IDetailsitem;
                }
                string AWBNo = string.Empty;
                int DepotInScanID = 0;
                PickupRequestDAO _dao = new PickupRequestDAO();
                if (v.InScanID == 0)
                {
                    var awbgenerate = db.AcCompanies.Find(companyId).IsAWBAutoGenrated;
                    if (awbgenerate == null)
                        awbgenerate = false;
                    if (Convert.ToBoolean(awbgenerate) && v.AWBAutoGenerated == true)
                    {
                        AWBNo = _dao.GetMaAWBNo(companyId, branchid);
                        v.HAWBNo = AWBNo;
                    }
                    else
                    {
                        int checkinscan = PickupRequestDAO.CheckAWB(v.HAWBNo).InScanID;
                        //var checkinscan = db.InScanMasters.Where(cc => cc.AWBNo == v.HAWBNo && cc.IsDeleted==false).FirstOrDefault(); 
                        //if (checkinscan!=null)
                        if (checkinscan > 0)
                        {
                            DepotInScanID = checkinscan;//.InScanID;

                        }


                    }
                }
                try
                {

                    InScanMaster inscan = new InScanMaster();

                    if (v.InScanID == 0)
                    {
                        //int id = (from c in db.InScanMasters orderby c.InScanID descending select c.InScanID).FirstOrDefault();
                        //inscan.InScanID = id + 1;
                        if (DepotInScanID > 0)
                        {
                            inscan = db.InScanMasters.Find(DepotInScanID);
                            v.InScanID = inscan.InScanID;
                            var awbdetail = db.AWBDetails.Where(cc => cc.AWBNo == v.HAWBNo && (cc.InScanID == 0 || cc.InScanID == null)).FirstOrDefault();
                            if (awbdetail != null)
                            {
                                awbdetail.InScanID = v.InScanID;
                                db.Entry(awbdetail).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        inscan.AWBNo = v.HAWBNo.Trim(); // _dao.GetMaAWBNo(companyId, branchid);
                        inscan.AcCompanyID = companyId;
                        inscan.BranchID = branchid;
                        inscan.DepotID = depotId;
                        inscan.AcFinancialYearID =PickupRequestDAO.GetFinancialYearID(v.TransactionDate.ToString(),branchid);
                        inscan.TransactionDate = v.TransactionDate;
                        if (inscan.DeviceID == null)
                            inscan.DeviceID = "WebSite";
                        if (inscan.EnteredByID == null)
                            inscan.EnteredByID = Convert.ToInt32(Session["UserID"]);
                        inscan.EnquiryNo = null;
                        inscan.IsEnquiry = false;
                        //inscan.PickupRequestStatusId = 4;
                        inscan.ShipmentModeID = v.ShipmentModeID;
                        inscan.AWBAutoGenerated = v.AWBAutoGenerated;
                        if (inscan.StatusTypeId == null)
                            inscan.StatusTypeId = 1;
                        if (inscan.CourierStatusID == null)
                            inscan.CourierStatusID = 4;
                        inscan.ConsignorCountryName = v.ConsignorCountryName;
                        inscan.ConsignorCityName = v.ConsignorCityName;
                        inscan.ConsignorLocationName = v.ConsignorLocationName;
                        inscan.CustomerShipperSame = v.CustomerandShipperSame;

                        inscan.Consignor = v.shippername;

                        inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                        inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;

                        inscan.ConsignorContact = v.ConsignorContact;

                        if (inscan.CreatedBy == null)
                            inscan.CreatedBy = userid;

                        if (inscan.CreatedDate == null)
                            inscan.CreatedDate = CommonFunctions.GetBranchDateTime();

                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetBranchDateTime();

                        if (v.PaymentModeId != null)
                        {
                            if (v.PaymentModeId == 3 && (v.CustomerID == 0))
                            {
                                //  int _customerid = SaveCustomer(v);
                                //customersavemessage = "New Customer - Customer saved as 'Cash Customer' in the system";
                                //inscan.CustomerID = _customerid;
                                return Json(new { status = "Failed", InscanId = 0, message = "Select Customer name!" });
                            }
                            else if (v.PaymentModeId == 1)
                            {
                                inscan.CustomerID = -1; //WALK-IN-CUSTOMER
                            }
                            else if (v.PaymentModeId == 2)
                            {
                                inscan.CustomerID = -2; //COD-CUSTOMER
                            }
                            else if (v.PaymentModeId == 5)
                            {
                                inscan.CustomerID = -3; //FOC-CUSTOMER
                            }
                            else
                            {
                                inscan.CustomerID = v.CustomerID;
                            }


                        }
                        inscan.TransactionDate = v.TransactionDate;
                    }
                    else
                    {
                        inscan = db.InScanMasters.Find(v.InScanID);


                        if (v.PaymentModeId != null)
                        {
                            if (v.PaymentModeId == 3)
                            {
                                if (v.CustomerID == 0)
                                {
                                    // int _customerid = SaveCustomer(v);
                                    //customersavemessage = "New Customer - Customer saved as 'Cash Customer' in the system";
                                    //inscan.CustomerID = _customerid;
                                    return Json(new { status = "Failed", InscanId = 0, message = "Select Customer name!" });
                                }
                                else
                                {
                                    inscan.CustomerID = v.CustomerID;
                                }
                            }
                            else if (v.PaymentModeId == 1)
                            {
                                inscan.CustomerID = -1; //WALK-IN-CUSTOMER
                            }
                            else if (v.PaymentModeId == 2)
                            {
                                inscan.CustomerID = -2; //COD-CUSTOMER
                            }
                            else if (v.PaymentModeId == 5)
                            {
                                inscan.CustomerID = -3; //COD-CUSTOMER
                            }
                            else
                            {
                                inscan.CustomerID = v.CustomerID;
                            }
                        }

                        inscan.TransactionDate = v.TransactionDate;
                        inscan.AcFinancialYearID = PickupRequestDAO.GetFinancialYearID(v.TransactionDate.ToString(), branchid);
                        
                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetBranchDateTime();
                    }
                    inscan.Weight = v.Weight;
                    inscan.ManifestWeight = v.ManifestWeight;
                    inscan.ConsignorCountryName = v.ConsignorCountryName;
                    inscan.ConsignorCityName = v.ConsignorCityName;
                    inscan.ConsignorLocationName = v.ConsignorLocationName;
                    inscan.CustomerShipperSame = v.CustomerandShipperSame;

                    inscan.Consignor = v.shippername;

                    inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                    inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;

                    inscan.ConsignorContact = v.ConsignorContact;
                    inscan.ConsignorPhone = v.ConsignorPhone;
                    inscan.ConsignorMobileNo = v.ConsignorMobile;
                    inscan.ConsignorAddress3_PinCode = v.ConsignorAddress3_PinCode;
                    inscan.ConsignorContact = v.ConsignorContact;
                    //inscan.AcJournalID = ajm.AcJournalID;
                    if (v.AcheadID != null)
                    {
                        inscan.AcHeadID = v.AcheadID;
                    }

                    if (v.CourierCharge != null)
                    {
                        inscan.CourierCharge = Convert.ToDecimal((v.CourierCharge));
                    }

                    if (v.OtherCharge != null)
                    {
                        inscan.OtherCharge = Convert.ToDecimal(v.OtherCharge);
                    }

                    if (v.Weight != null)
                    {
                        inscan.Weight = Convert.ToDecimal(v.Weight);
                    }
                    if (v.CustomsValue != null)
                    {
                        inscan.CustomsValue = Convert.ToDecimal((v.CustomsValue));
                    }
                    else
                    {
                        inscan.CustomsValue = 0;
                    }
                    if (v.CurrencyID==null)
                    {
                        inscan.CurrencyID = CommonFunctions.GetDefaultCurrencyId();
                    }
                    else
                    {
                        inscan.CurrencyID = v.CurrencyID;
                    }                    
                    
                    inscan.PaymentModeId = v.PaymentModeId;

                    if (v.ParentInScanID == null)
                        inscan.ParentInScanID = 0;
                    else
                        inscan.ParentInScanID = v.ParentInScanID;

                    inscan.Consignee = v.Consignee;
                    inscan.ConsigneeCountryName = v.ConsigneeCountryName;
                    inscan.ConsigneeCityName = v.ConsigneeCityName;
                    inscan.ConsigneeLocationName = v.ConsigneeLocationName;
                    inscan.ConsigneeAddress1_Building = v.ConsigneeAddress1_Building;
                    inscan.ConsigneeAddress2_Street = v.ConsigneeAddress2_Street;
                    inscan.ConsigneeAddress3_PinCode = v.ConsigneeAddress3_PinCode;

                    inscan.ConsigneePhone = v.ConsigneePhone;
                    inscan.ConsigneeMobileNo = v.ConsigneeMobile;
                    inscan.ConsigneeContact = v.ConsigneeContact;


                    inscan.Pieces = v.Pieces.ToString();

                    inscan.ProductTypeID = v.ProductTypeID;
                    inscan.ParcelTypeId = v.ParcelTypeID;
                    inscan.CourierCharge = v.CourierCharge;
                    inscan.TaxPercent = v.TaxPercent;
                    inscan.TaxAmount = v.TaxAmount;
                    inscan.SurchargePercent = v.SurchargePercent;
                    inscan.SurchargeAmount = v.SurchargeAmount;
                    inscan.CustomerRateID = v.CustomerRateTypeID;
                    inscan.MovementID = v.MovementTypeID;
                    inscan.OtherCharge = v.OtherCharge;
                    inscan.CargoDescription = v.Description;
                    inscan.Remarks = v.remarks;
                    inscan.FAgentId = v.FagentID;
                    inscan.ForwardingAWBNo = v.FAWBNo;
                    inscan.ForwardingCharge = v.ForwardingCharge;
                    inscan.VerifiedWeight = v.VerifiedWeight;
                    inscan.CBM_length = v.CBMLength;
                    inscan.CBM_height = v.CBMHeight;
                    inscan.CBM_width = v.CBMWidth;
                    inscan.SpotRate = v.SpotRate;
                    inscan.MarginPercent = v.MarginPercent;
                    // inscan.AWBProcessed = Convert.ToBoolean(v.AWBProcessed);
                                        
                    if (v.materialcost != null)
                    {
                        inscan.MaterialCost = Convert.ToDecimal(v.materialcost);
                    }
                    if (v.totalCharge != null)
                    {
                        inscan.NetTotal = Convert.ToDecimal(v.totalCharge);
                    }                                       

                    if (inscan.PickedUpEmpID == null)
                        inscan.PickedUpEmpID = v.PickedBy;

                    if (inscan.DepotReceivedBy == null)
                        inscan.DepotReceivedBy = v.ReceivedBy;

                    inscan.IsNCND = v.IsNCND;
                    inscan.IsCashOnly = v.IsCashOnly;
                    inscan.IsChequeOnly = v.IsChequeOnly;
                    inscan.IsCollectMaterial = v.IsCollectMaterial;
                    inscan.IsDOCopyBack = v.IsDOCopyBack;
                    inscan.PickupLocation = v.PickupLocation;
                    inscan.DeliveryLocation = v.DeliveryLocation;
                    inscan.PickupSubLocality = v.PickupSubLocality;
                    inscan.DeliverySubLocality = v.DeliverySubLocality;
                    inscan.OriginPlaceID = v.PickupLocationPlaceId;
                    inscan.DestinationPlaceID = v.DeliveryLocationPlaceId;
                    inscan.AWBProcessed = true;
                    inscan.CBM_length = v.CBMLength;
                    inscan.CBM_height = v.CBMHeight;
                    inscan.CBM_width = v.CBMWidth;
                    inscan.SpotRate = v.SpotRate;
                    inscan.MarginPercent = v.MarginPercent;
                    InScanInternationalDeatil isid = new InScanInternationalDeatil();
                    InScanInternational isi = new InScanInternational();
                    if (v.InScanID == 0 && DepotInScanID == 0)
                    {
                        inscan.EntrySource = 1; //AWB create by create shipment page
                        if (inscan.PickedUpEmpID != null)
                        {
                            inscan.StatusTypeId = 1;
                            inscan.CourierStatusID = 4;
                        }

                        if (inscan.DepotReceivedBy != null)
                        {
                            inscan.StatusTypeId = 2; //Inscan
                            inscan.CourierStatusID = 5; //received at origin facility
                        }

                        if (inscan.PickedUpEmpID == null && inscan.DepotReceivedBy == null)
                        {
                            inscan.StatusTypeId = 1;
                            inscan.CourierStatusID = 2;
                        }

                        inscan.IsDeleted = false;
                        db.InScanMasters.Add(inscan);
                        db.SaveChanges();


                        //add status of awbdetail
                        var awbdetail = db.AWBDetails.Where(cc => cc.AWBNo == v.HAWBNo && (cc.InScanID == null || cc.InScanID == 0)).FirstOrDefault();
                        if (awbdetail != null)
                        {
                            awbdetail.InScanID = v.InScanID;
                            db.Entry(awbdetail).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        if (inscan.PickedUpEmpID != null)
                        {

                            
                            AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(inscan.PickedUpEmpID), 1, 1);
                        }

                        if (inscan.DepotReceivedBy != null)
                        {
                            AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(inscan.DepotReceivedBy), 2, 5);
                        }

                        if (inscan.PickedUpEmpID == null && inscan.DepotReceivedBy == null)
                        {
                            var empid = db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault();
                            if (empid != null)
                                AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(empid.EmployeeID), 2, 5);
                        }
 

                        isid.InScanID = inscan.InScanID;
                        isi.InScanID = inscan.InScanID;
                        isi.InScanInternationalID = 0;
                        TempData["SuccessMsg"] = customersavemessage + "\n" + "You have successfully Added Quick AirWay Bill.";

                    }
                    else
                    {
                        if (inscan.CourierStatusID < 4)
                        {
                            if (inscan.PickedUpEmpID != null)
                            {
                                inscan.StatusTypeId = 1;
                                inscan.CourierStatusID = 4;
                            }
                        }
                        if (inscan.CourierStatusID < 5)
                        {
                            if (inscan.DepotReceivedBy != null)
                            {
                                inscan.StatusTypeId = 2; //Inscan
                                inscan.CourierStatusID = 5; //received at origin facility
                            }
                        }

                        db.Entry(inscan).State = EntityState.Modified;
                        db.SaveChanges();
                        if (inscan.PickedUpEmpID != null && inscan.EntrySource==3) //only for drr pickupentry
                        {


                            AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(inscan.PickedUpEmpID), 1, 1);
                        }

                        if (inscan.DepotReceivedBy != null && inscan.EntrySource == 3) //only for drr pickupentry
                        {
                            AddAWBTrackStatus(inscan.InScanID, Convert.ToInt32(inscan.DepotReceivedBy), 2, 5);
                        }



                        TempData["SuccessMsg"] = customersavemessage + "\n" + "You have successfully updated Airway Bill";

                    }



                    if (v.FagentID > 0)
                    {
                        isid = db.InScanInternationalDeatils.Where(cc => cc.InScanID == v.InScanID).FirstOrDefault();
                        isi = db.InScanInternationals.Where(cc => cc.InScanID == v.InScanID).FirstOrDefault();
                        if (isid == null)
                        {
                            isid = new InScanInternationalDeatil();
                            isid.InScanID = inscan.InScanID;
                        }
                        if (isi == null)
                        {
                            isi = new InScanInternational();
                            isi.InScanID = inscan.InScanID;
                        }

                        if (v.FagentID != null)
                        {
                            isi.FAgentID = Convert.ToInt32(v.FagentID);
                        }

                        if (v.ForwardingCharge != null)
                        {
                            isi.ForwardingCharge = Convert.ToDecimal(v.ForwardingCharge);
                            isid.ForwardingCharge = Convert.ToDecimal(v.ForwardingCharge);
                        }
                        isi.StatusAssignment = false;
                        isi.ForwardingAWBNo = v.FAWBNo;
                        isi.ForwardingDate = inscan.TransactionDate; // DateTime.UtcNow;
                        isi.VerifiedWeight = Convert.ToDouble(inscan.Weight);

                        isid.VerifiedWeight = inscan.Weight;

                        if (isi.InScanInternationalID == 0)
                        {
                            db.InScanInternationals.Add(isi);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(isi).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        if (isid.InScanInternationalDeatilID == 0)
                        {

                            isid.InscanInternationalID = isi.InScanInternationalID;
                            db.InScanInternationalDeatils.Add(isid);
                            db.SaveChanges();
                        }
                        else
                        {

                            db.Entry(isid).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    #region "Othercharge"
                    //Other charge data update into inscanothercharge table
                    if (v.otherchargesVM != null)
                    {
                        for (int j = 0; j < v.otherchargesVM.Count; j++)
                        {

                            if (v.otherchargesVM[j].Deleted == false)
                            {
                                int oid = Convert.ToInt32(v.otherchargesVM[j].OtherChargeID);
                                InscanOtherCharge objOtherCharge = db.InscanOtherCharges.Where(cc => cc.InscanID == inscan.InScanID && cc.OtherChargeID == oid).FirstOrDefault();
                                if (objOtherCharge == null)
                                {
                                    objOtherCharge = new InscanOtherCharge();
                                    var maxid = (from c in db.InscanOtherCharges orderby c.InscanOtherChargeID descending select c.InscanOtherChargeID).FirstOrDefault();
                                    objOtherCharge.InscanOtherChargeID = maxid + 1;
                                    objOtherCharge.InscanID = inscan.InScanID;
                                    objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    objOtherCharge.Amount = v.otherchargesVM[j].Amount;
                                    db.InscanOtherCharges.Add(objOtherCharge);
                                    db.SaveChanges();
                                    db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                                else
                                {
                                    //objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    objOtherCharge.Amount = v.otherchargesVM[j].Amount;
                                    db.Entry(objOtherCharge).State = EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                            }
                            else
                            {
                                int oid = Convert.ToInt32(v.otherchargesVM[j].OtherChargeID);
                                InscanOtherCharge objOtherChargeDel = db.InscanOtherCharges.Where(cc => cc.InscanID == inscan.InScanID && cc.OtherChargeID == oid).FirstOrDefault();

                                if (objOtherChargeDel != null)
                                {
                                    db.InscanOtherCharges.Remove(objOtherChargeDel);
                                    db.SaveChanges();
                                }
                            }
                        }



                    }

                    #endregion
                    //Other charge save end

                    #region "Itemshipment"
                    if (v.shipmentItemVM != null)
                    {
                        for (int j = 0; j < v.shipmentItemVM.Count; j++)
                        {

                            if (v.shipmentItemVM[j].Deleted == false)
                            {
                                int oid = Convert.ToInt32(v.shipmentItemVM[j].ID);
                                InScanMasterItem objItem = db.InScanMasterItems.Where(cc => cc.InScanID == inscan.InScanID && cc.ID == oid).FirstOrDefault();
                                if (objItem == null)
                                {
                                    objItem = new InScanMasterItem();

                                    objItem.InScanID = inscan.InScanID;
                                    if (v.shipmentItemVM[j].BoxName == null)
                                        v.shipmentItemVM[j].BoxName = "";
                                    objItem.BoxName = v.shipmentItemVM[j].BoxName;
                                    objItem.Contents = v.shipmentItemVM[j].Contents;
                                    if (v.shipmentItemVM[j].Qty == null)
                                    {
                                        objItem.Qty = 0;
                                    }
                                    else
                                    {
                                        objItem.Qty = v.shipmentItemVM[j].Qty;
                                    }
                                    if (v.shipmentItemVM[j].WeightPerCarton == null)
                                    {
                                        objItem.WeightPerCarton = 0;
                                    }
                                    else
                                    {
                                        objItem.WeightPerCarton = v.shipmentItemVM[j].WeightPerCarton;
                                    }
                                    if (v.shipmentItemVM[j].TotalWeight == null)
                                    {
                                        objItem.TotalWeight = 0;
                                    }
                                    else
                                    {
                                        objItem.TotalWeight = v.shipmentItemVM[j].TotalWeight;
                                    }

                                    if (v.shipmentItemVM[j].Value == null)
                                        objItem.Value = 0;

                                    else
                                    {
                                        objItem.Value = v.shipmentItemVM[j].Value;
                                    }

                                    db.InScanMasterItems.Add(objItem);
                                    db.SaveChanges();
                                    db.Entry(objItem).State = EntityState.Detached;
                                }
                                else
                                {
                                    //objOtherCharge.OtherChargeID = v.otherchargesVM[j].OtherChargeID;
                                    //objItem.Amount = v.otherchargesVM[j].Amount;
                                    //db.Entry(objOtherCharge).State = EntityState.Modified;
                                    //db.SaveChanges();
                                    //db.Entry(objOtherCharge).State = EntityState.Detached;
                                }
                            }
                            else
                            {
                                int oid = Convert.ToInt32(v.shipmentItemVM[j].ID);
                                InScanMasterItem objItemDel = db.InScanMasterItems.Where(cc => cc.InScanID == inscan.InScanID && cc.ID == oid).FirstOrDefault();

                                if (objItemDel != null)
                                {
                                    db.InScanMasterItems.Remove(objItemDel);
                                    db.SaveChanges();
                                }
                            }
                        }



                    }

                    #endregion

                    if (v.ParentInScanID != null)
                    {
                        int parentinscanid = Convert.ToInt32(v.ParentInScanID);
                        if (parentinscanid > 0)
                        {
                            var parentinscan = db.InScanMasters.Find(parentinscanid);
                            parentinscan.ChildInScanID = inscan.InScanID;
                            db.Entry(parentinscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    try
                    {
                        SaveConsignorAddress(v);
                        SaveConsigeeAddress(v);
                    }
                    catch (Exception ex)
                    {

                    }

                    //accounts posting  for payment mode pickupcash and cod and Account on 30/nov/2020
                    //if (v.PaymentModeId == 1 || v.PaymentModeId == 2)
                    if (inscan.DRRId == null)
                        inscan.DRRId = 0;
                    if (inscan.DRRId == 0 || inscan.EntrySource == 3)//drr wise awb
                        _dao.AWBAccountsPosting(inscan.InScanID);


                    if (v.InScanID == 0)
                    {
                        return Json(new { status = "OK", InscanId = inscan.InScanID, message = "AWB Added Succesfully!" });

                    }
                    else
                    {
                        return Json(new { status = "OK", InscanId = inscan.InScanID, message = "AWB Updated Succesfully!" });
                    }


                }
                catch (Exception ex)
                {
                    TempData["SuccessMsg"] = ex.Message;
                    return Json(new { status = "Failed", InscanId = 0, message = ex.Message });
                }

               

            }
            catch (Exception ex)
            {
                TempData["SuccessMsg"] = ex.Message;
                return Json(new { status = "Failed", InscanId = 0, message = ex.Message });
            }
                        
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
                //InScanMaster _enquiry = db.InScanMasters.Find(v.InScanID);
                
                //admin level rights checking to revert the status
                
                //    List<int> RoleId = (List<int>)Session["RoleID"];
                //    if (!RoleId.Contains(1))
                //    {
                //        statuschangepersmission = false;
                //    }
                

                //if (statuschangepersmission == false)
                //{
                //    status = "failed";
                //    return Json(new { status = status, message = "User does not have persmission to revert the status,Contact Admin!" }, JsonRequestBehavior.AllowGet);
                //}
                              
                //updateing awbstaus table for tracking
                AWBTrackStatu _awbstatus = new AWBTrackStatu();
                _awbstatus = db.AWBTrackStatus.Where(c=>c.InScanId==v.InScanID && c.CourierStatusId==v.CourierStatusID).FirstOrDefault();
                
                if (v.ShipmentReturned==true)
                {
                    _awbstatus.CourierStatus = "Shipment Returned";
                    _awbstatus.ShipmentReturned = true;
                    _awbstatus.Remarks = v.Remarks;
                    _awbstatus.UserId = uid;
                }
                else
                {
                    _awbstatus.CourierStatus = "Shipment Delivered";
                    _awbstatus.ShipmentReturned = false;
                    _awbstatus.Remarks = v.Remarks;
                    _awbstatus.UserId = uid;
                }

                db.Entry(_awbstatus).State = EntityState.Modified;
                db.SaveChanges();                
                status = "ok";
                message = "Status Updated";

            }

            catch (Exception ex)
            {
                status="failed";
                message = ex.Message;
            }

            //return Json(new { result=result,statustext=status } , JsonRequestBehavior.AllowGet);
            return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddSaveStatus(ChangeStatus v)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            string status = "ok";
            //   string result = "ok";
            string message = "";
            string statusname = "";
            bool statuschangepersmission = true;
            try
            {
                var emp = db.EmployeeMasters.Where(cc => cc.UserID == uid).FirstOrDefault();
                int empid = 0;
                 if (emp!=null)
                {
                    empid = emp.EmployeeID;
                }
                if (v.InScanID > 0)
                {
                    var inscan = db.InScanMasters.Find(v.InScanID);
                    //Adding awbstaus table for tracking
                    AWBTrackStatu _awbstatus = new AWBTrackStatu();
                    //_awbstatus = db.AWBTrackStatus.Where(c => c.InScanId == v.InScanID && c.CourierStatusId == v.CourierStatusID).FirstOrDefault();

                    var courier = db.CourierStatus.Find(v.CourierStatusID);
                    var statustypeid = courier.StatusTypeID;
                    var statustype = courier.StatusType;

                    _awbstatus.CourierStatusId = v.CourierStatusID;
                    _awbstatus.CourierStatus = courier.CourierStatus;
                    _awbstatus.StatusTypeId = Convert.ToInt32(courier.StatusTypeID);
                    _awbstatus.ShipmentStatus = courier.StatusType;
                    _awbstatus.UserId = uid;
                    _awbstatus.Remarks = "Manual Status Update";
                    _awbstatus.AWBNo = inscan.AWBNo;
                    _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();
                    _awbstatus.InScanId = v.InScanID;
                    _awbstatus.EmpID = empid;
                    db.AWBTrackStatus.Add(_awbstatus);
                    db.SaveChanges();


                    inscan.CourierStatusID = v.CourierStatusID;
                    inscan.StatusTypeId = _awbstatus.StatusTypeId;

                    db.Entry(inscan).State = EntityState.Modified;
                    db.SaveChanges();

                    status = "ok";
                    message = "Status Added";
                }
                else if (v.InboundShipmentID>0)
                {
                    var inscan = db.InboundShipments.Find(v.InboundShipmentID);
                    //Adding awbstaus table for tracking
                    AWBTrackStatu _awbstatus = new AWBTrackStatu();
                    //_awbstatus = db.AWBTrackStatus.Where(c => c.InScanId == v.InScanID && c.CourierStatusId == v.CourierStatusID).FirstOrDefault();

                    var courier = db.CourierStatus.Find(v.CourierStatusID);
                    var statustypeid = courier.StatusTypeID;
                    var statustype = courier.StatusType;

                    _awbstatus.CourierStatusId = v.CourierStatusID;
                    _awbstatus.CourierStatus = courier.CourierStatus;
                    _awbstatus.StatusTypeId = Convert.ToInt32(courier.StatusTypeID);
                    _awbstatus.ShipmentStatus = courier.StatusType;
                    _awbstatus.UserId = uid;
                    _awbstatus.Remarks = "Manual Status Update";
                    _awbstatus.AWBNo = inscan.AWBNo;
                    _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();
                    _awbstatus.InboundShipmentID = v.InboundShipmentID;
                    _awbstatus.EmpID = empid;
                    db.AWBTrackStatus.Add(_awbstatus);
                    db.SaveChanges();


                    inscan.CourierStatusID = v.CourierStatusID;
                    inscan.StatusTypeId = _awbstatus.StatusTypeId;

                    db.Entry(inscan).State = EntityState.Modified;
                    db.SaveChanges();

                    status = "ok";
                    message = "Status Added";
                }

            }

            catch (Exception ex)
            {
                status = "failed";
                message = ex.Message;
            }

            //return Json(new { result=result,statustext=status } , JsonRequestBehavior.AllowGet);
            return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
        }

        //GetStatus
        [HttpPost]
        public JsonResult GetStatus(int StatusTypeId)
        {
            string status = "ok";
            
            //List<CourierStatu> _cstatus = new List<CourierStatu>();
            try
            {
            var  _cstatus =(from aa in db.CourierStatus where aa.StatusTypeID == StatusTypeId select new selectdata { id=aa.CourierStatusID ,text=aa.CourierStatus}).ToList();
                return Json(new { data = _cstatus, result = status }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = "undefined", result = "failed" }, JsonRequestBehavior.AllowGet);

        }

        public QuickAWBVM GetAWBDetail(int id)
        {
            QuickAWBVM inscan = new QuickAWBVM();

            InScanMaster data = (from c in db.InScanMasters where c.InScanID == id select c).FirstOrDefault();            
            
                inscan.InScanID = data.InScanID;
                inscan.ShipmentModeID =Convert.ToInt32(data.ShipmentModeID);
            inscan.AWBAutoGenerated = data.AWBAutoGenerated;
                inscan.TransactionDate =Convert.ToDateTime(data.TransactionDate);
                inscan.AcCompanyID = Convert.ToInt32(data.AcCompanyID);
                //inscan.EnquiryID =Convert.ToInt32(data.EnquiryID);
                inscan.HAWBNo = data.AWBNo;
                inscan.InScanDate = data.TransactionDate;
                inscan.Consignor = data.Consignor;
                inscan.CustomerandShipperSame = data.CustomerShipperSame;
                inscan.shippername = data.Consignor;
            if (data.ImportShipmentId == null)
                inscan.ImportShipmentId = 0;
            else
                inscan.ImportShipmentId = data.ImportShipmentId;
                if (data.InvoiceID != null)
                {
                if (inscan.ImportShipmentId >0)
                {
                    inscan.InvoiceId = Convert.ToInt32(data.InvoiceID);
                    var invoice = db.AgentInvoices.Find(inscan.InvoiceId);
                    if (invoice!=null)
                    inscan.InvoiceStatus = invoice.InvoiceNo + "/" + invoice.InvoiceDate.ToString("dd-MM-yyyy");
                }
                else
                {
                    inscan.InvoiceId = Convert.ToInt32(data.InvoiceID);
                    var invoice = db.CustomerInvoices.Find(inscan.InvoiceId);
                    if (invoice!=null)
                    inscan.InvoiceStatus = invoice.CustomerInvoiceNo + "/" + invoice.InvoiceDate.ToString("dd-MM-yyyy");
                }
                  
            }
                else
                {
                    inscan.InvoiceId = 0;
                }
                inscan.ParentAWBNo = "";
                if (data.ParentInScanID!=null)
                {
                    inscan.ParentInScanID = Convert.ToInt32(data.ParentInScanID);
                if (inscan.ParentInScanID > 0)
                {
                    var parentinscan = db.InScanMasters.Find(inscan.ParentInScanID);
                    if (parentinscan!=null)
                        inscan.ParentAWBNo = parentinscan.AWBNo;
                    
                }
                }
            
                inscan.EnquiryNo = data.EnquiryNo;
                //inscan.PickupRequestStatusId = data.PickupRequestStatusId;
                inscan.StatusTypeId = data.StatusTypeId;
                inscan.CourierStatusId = data.CourierStatusID;
                inscan.remarks = data.Remarks;
                int statustypeid = 0;
                if (data.AcHeadID!=null && data.AcHeadID != 0)
                {
                    var achead = db.AcHeads.Find(data.AcHeadID);
                    if (achead!=null)
                    {
                    inscan.AcheadID = data.AcHeadID.Value;
                    inscan.AcHeadName = achead.AcHead1;
                    }
               }
                else
                {

                    int userId = Convert.ToInt32(Session["UserID"].ToString());
                    if (Session["UserType"].ToString() == "Employee")
                    {
                        var useremp = (from e in db.EmployeeMasters where e.UserID == userId select e).First();
                        if (useremp != null)
                        {
                            if (useremp.AcHeadID != null)
                            {
                                inscan.AcheadID = Convert.ToInt32(useremp.AcHeadID);
                                var achead = db.AcHeads.Find(Convert.ToInt32(useremp.AcHeadID));
                                if (achead != null)
                                {
                                    var acgroup = db.AcGroups.Where(cc => cc.AcGroupID == achead.AcGroupID).FirstOrDefault();
                                    if (acgroup.AcGroup1 == "Cash")
                                    {
                                            inscan.AcHeadName = achead.AcHead1;
                                    }
                                }
                            }
                        }
                    }
                }

            if (data.StatusTypeId != null && data.StatusTypeId!=0)
                statustypeid =Convert.ToInt32(data.StatusTypeId);

              if (inscan.CourierStatusId==null || inscan.CourierStatusId==0)
            {
                inscan.StatusType = "INSCAN";
                inscan.CourierStatus = "Collected";
            }
              else
            {
                inscan.StatusType = db.tblStatusTypes.Where(cc => cc.ID == statustypeid).FirstOrDefault().Name;
                inscan.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == inscan.CourierStatusId).FirstOrDefault().CourierStatus;
            }

            //inscan.AcJournalID = data.AcJournalID.Value;

            if (data.CourierCharge != null)
            {
                inscan.CourierCharge =Convert.ToDecimal(data.CourierCharge);
            }
            else
            {
                inscan.CourierCharge = 0;
            }
            if (data.TaxPercent!= null)
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
                inscan.TaxAmount= 0;
            }
            if (data.SurchargePercent!=null)
            {
                inscan.SurchargePercent = Convert.ToDecimal(data.SurchargePercent);
                inscan.SurchargeAmount = Convert.ToDecimal(data.SurchargeAmount);
            }
            else
            {
                inscan.SurchargePercent = 0;
                inscan.SurchargeAmount = 0;
            }

            if (data.Pieces!=null)
            {
                //inscan.Pieces = Convert.ToInt32(data.Pieces);
                inscan.Pieces = data.Pieces;
            }
            else
            {
                inscan.Pieces = "0";              
            }
            if (data.DRRId != null)
            {
                inscan.DRRID = Convert.ToInt32(data.DRRId);
                var drr = (from c in db.DRRs join a in db.AcJournalMasters on c.AcJournalID equals a.AcJournalID where c.DRRID == inscan.DRRID select a.VoucherNo).FirstOrDefault();
                if (drr!=null)
                {
                    inscan.DRRNo = drr;
                    inscan.DRRProcess = true;
                }
            }
            else
            {
                inscan.DRRID = 0;
                inscan.DRRProcess = false;
            }

            if (data.CustomsValue != null)
            {
                 
                inscan.CustomsValue =Convert.ToDecimal(data.CustomsValue);
            }
            else
            {
                inscan.CustomsValue = 0;
            }

            if (data.ReturnedToConsignor == true)
                inscan.ReturnedtoConsignor = "Returned to Shipper";
            else
                inscan.ReturnedtoConsignor = "";
            //if (data.BalanceAmt != null)
            //{
            //    inscan.totalCharge = data.BalanceAmt.Value;
            //}
            //else
            //{
            //    inscan.totalCharge = 0;
            //}

            if (data.NetTotal != null)
                inscan.totalCharge = Convert.ToDecimal(data.NetTotal);
            else
                inscan.totalCharge = 0;

            //if (data.PackingCharge != null)
            //{
            //    inscan.PackingCharge = data.PackingCharge.Value;
            //}
            //else
            //{
            //    inscan.PackingCharge = 0;
            //}
            if (data.MaterialCost != null)
            {
                inscan.materialcost = data.MaterialCost;
            }
            else
            {
                inscan.materialcost = 0;
            }
            if (data.OtherCharge != null)
            {
                inscan.OtherCharge = data.OtherCharge.Value;
            }
            else
            {
                inscan.OtherCharge = 0;
            }

            if (data.NetTotal != null)
            {
                inscan.totalCharge = data.NetTotal.Value;
            }
            else
            {
                inscan.totalCharge= 0;
            }
            if (data.CustomsValue!=null)
            {
                inscan.CustomsValue =Convert.ToDecimal(data.CustomsValue);
            }
            else
            {
                inscan.CustomsValue = 0;
            }
            if (data.CurrencyID!=null)
            {
                inscan.CurrencyID = Convert.ToInt32(data.CurrencyID);
            }
            if (data.Weight != null)
                {
                    inscan.Weight = data.Weight.Value;
                }
                else
                {
                    inscan.Weight = 0;
                }

            if (data.ManifestWeight != null)
            {
                inscan.ManifestWeight = data.ManifestWeight.Value;
            }
            else
            {
                inscan.ManifestWeight = 0;
            }
            if (data.VerifiedWeight != null)
            {
                inscan.VerifiedWeight = data.VerifiedWeight.Value;
                if (inscan.VerifiedWeight == 0)
                    inscan.VerifiedWeight = inscan.Weight.Value;
            }
            else
            {
                inscan.VerifiedWeight= 0;
                if (inscan.VerifiedWeight == 0)
                    inscan.VerifiedWeight = inscan.Weight.Value;
            }

            if (data.SpotRate != null)
            {
                inscan.SpotRate = data.SpotRate.Value;
            }
            else
            {
                inscan.SpotRate = 0;
            }

            if (data.MarginPercent != null)
            {
                inscan.MarginPercent = data.MarginPercent.Value;
            }
            else
            {
                inscan.MarginPercent = 0;
            }

            if (data.CBM_length != null)
            {
                inscan.CBMLength = data.CBM_length.Value;
            }
            else
            {
                inscan.CBMLength = 0;
            }
            if (data.CBM_width != null)
            {
                inscan.CBMWidth= data.CBM_width.Value;
            }
            else
            {
                inscan.CBMWidth = 0;
            }

            if (data.CBM_height != null)
            {
                inscan.CBMHeight = data.CBM_height.Value;
            }
            else
            {
                inscan.CBMHeight = 0;
            }
            if (data.ProductTypeID!=null)
                {
                inscan.ProductTypeID =Convert.ToInt32( data.ProductTypeID);

                }

                if (data.ParcelTypeId != null)
                {
                    inscan.ParcelTypeID =Convert.ToInt32(data.ParcelTypeId);

                }
                if (data.CustomerRateID !=null)
                {
                inscan.CustomerRateTypeID = Convert.ToInt32(data.CustomerRateID);
                }

            if (data.PaymentModeId != null)
            {
                inscan.PaymentModeId = data.PaymentModeId;
                inscan.paymentmode = db.tblPaymentModes.Find(inscan.PaymentModeId).PaymentModeText;
            }
           // inscan.paymentmode = data.StatusPaymentMode;
                inscan.ConsignorCountryName = data.ConsignorCountryName;
                inscan.ConsignorCityName = data.ConsignorCityName;
                inscan.ConsigneeCountryName = data.ConsigneeCountryName;
                inscan.ConsigneeCityName = data.ConsigneeCityName;                

                inscan.ConsignorAddress1_Building = data.ConsignorAddress1_Building;
                inscan.ConsignorAddress2_Street = data.ConsignorAddress2_Street;
                inscan.ConsignorAddress3_PinCode = data.ConsignorAddress3_PinCode;

            if (data.CustomerID != null)
            {
                inscan.CustomerID = data.CustomerID.Value;
                var cust = db.CustomerMasters.Find(inscan.CustomerID);
                if (cust != null)
                    inscan.customer = cust.CustomerName;
                else
                    inscan.customer = "Customer Unknown";

            }

                //inscan.TaxconfigurationID = data.TaxconfigurationID.Value;
                inscan.Consignee = data.Consignee;

                inscan.ConsigneeAddress1_Building = data.ConsigneeAddress1_Building;
                inscan.ConsigneeAddress2_Street = data.ConsigneeAddress2_Street;
                inscan.ConsigneeAddress3_PinCode = data.ConsigneeAddress3_PinCode;

                inscan.ConsigneePhone = data.ConsigneePhone;
            inscan.ConsigneeMobile = data.ConsigneeMobileNo;
            inscan.ConsignorPhone = data.ConsignorPhone;
            inscan.ConsignorMobile = data.ConsignorMobileNo;
            inscan.ConsigneeContact = data.ConsigneeContact;
                inscan.ConsignorContact = data.ConsignorContact;

                // inscan.Pieces = data.Pieces;
                inscan.ConsignorLocationName = data.ConsignorLocationName;
                inscan.ConsigneeLocationName = data.ConsigneeLocationName;

                //inscan.totalCharge = data.BalanceAmt.Value;
                //inscan.materialcost = data.MaterialCost.Value;
                inscan.Description = data.CargoDescription;

               if (data.MovementID!=null)
                    inscan.MovementTypeID = data.MovementID;

            if (data.DepotReceivedBy!=null)
                inscan.ReceivedBy = data.DepotReceivedBy; // "tesT"; // data.ReceivedBy.Value;
            
            if (data.PickedUpEmpID != null)
                inscan.PickedBy = data.PickedUpEmpID;// "test1"; //data.ReceivedBy.Value;           

            inscan.IsNCND = data.IsNCND;
            inscan.IsCashOnly = data.IsCashOnly;
            inscan.IsChequeOnly = data.IsChequeOnly;
            inscan.IsCollectMaterial = data.IsCollectMaterial;
            inscan.IsDOCopyBack = data.IsDOCopyBack;
            inscan.AWBProcessed = data.AWBProcessed;
            

                inscan.PickupLocation = data.PickupLocation;
            inscan.DeliveryLocation = data.DeliveryLocation;

            if (inscan.PickupLocation != "" && (inscan.ConsignorLocationName == null || inscan.ConsignorLocationName == ""))                
                    inscan.ConsignorLocationName = inscan.PickupLocation;

            if (inscan.DeliveryLocation != "" && (inscan.ConsigneeLocationName == null || inscan.ConsigneeLocationName == ""))
                inscan.ConsigneeLocationName = inscan.DeliveryLocation;

            if (inscan.PickupLocation == "" )
                inscan.PickupLocation = inscan.ConsignorCountryName;

            if (inscan.DeliveryLocation ==  "")
                inscan.DeliveryLocation= inscan.ConsigneeCountryName;


            inscan.PickupSubLocality = data.PickupSubLocality;
            inscan.DeliverySubLocality = data.DeliverySubLocality;
            inscan.PickupLocationPlaceId = data.OriginPlaceID;
            if (data.DestinationPlaceID!=null)
                inscan.DeliveryLocationPlaceId = data.DestinationPlaceID;
            if (data.CreatedBy != null)
            {
                inscan.CreatedByDate = Convert.ToDateTime(data.CreatedDate).ToString("dd-MMM-yyyy HH:mm"); ;
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
                inscan.LastModifiedDate = Convert.ToDateTime(data.LastModifiedDate).ToString("dd-MMM-yyyy HH:mm");

                if (data.LastModifiedBy != null)
                {                                            
                        var emp = db.EmployeeMasters.Where(CC => CC.UserID == data.LastModifiedBy).FirstOrDefault();
                        if (emp != null)
                        {
                            inscan.LastModifiedByName  = emp.EmployeeName;
                        }
                    }
             }
            
             if (data.FAgentId!=null)
            {
                inscan.FagentID = Convert.ToInt32(data.FAgentId);
            }
             if (data.ForwardingAWBNo!=null)
            {
                inscan.FAWBNo = data.ForwardingAWBNo;
            }
             if (data.ForwardingCharge!=null)
            {
                inscan.ForwardingCharge = data.ForwardingCharge.Value;
            }

                //var d = (from c in db.InScanInternationals where c.InScanID == inscan.InScanID select c).FirstOrDefault();
                //if (d != null)
                //{
                //    inscan.FagentID = d.FAgentID;
                //    inscan.FAWBNo = d.ForwardingAWBNo;
                //    // inscan.ForwardingDate = d.ForwardingDate;
                //    //inscan.VerifiedWeight = d.VerifiedWeight;
                //    inscan.ForwardingCharge = d.ForwardingCharge;
                //}


            var custrate = db.CustomerRates.Find(data.CustomerRateID);
            if (custrate!=null)
            {
                var ratettype = db.CustomerRateTypes.Find(custrate.CustomerRateTypeID);
                if (ratettype != null)
                {
                    inscan.CustomerRateType = ratettype.CustomerRateType1;
                    inscan.CustomerRateTypeID = Convert.ToInt32(data.CustomerRateID);
                }
            }

            return inscan;

        }

      
        public JsonResult GetActiveStatus(int InScanID)
        {
            string status = "ok";
            ChangeStatus _cstatus = new ChangeStatus();
            try
            {
                InScanMaster _inscan = db.InScanMasters.Find(InScanID);                              
                
                   _cstatus.InScanID = _inscan.InScanID;
                if (_inscan.StatusTypeId == null)
                    _cstatus.StatusTypeID = 1;
                else
                    _cstatus.StatusTypeID = Convert.ToInt32(_inscan.StatusTypeId);
                
                _cstatus.CourierStatusID= Convert.ToInt32(_inscan.CourierStatusID);
                if (_cstatus.CourierStatusID==13)
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
        public JsonResult GetCoAWBActiveStatus(int ShipmentID)
        {
            string status = "ok";
            ChangeStatus _cstatus = new ChangeStatus();
            try
            {
                InboundShipment _inscan = db.InboundShipments.Find(ShipmentID);

                _cstatus.InboundShipmentID = _inscan.ShipmentID;
                if (_inscan.StatusTypeId == null)
                    _cstatus.StatusTypeID = 1;
                else
                    _cstatus.StatusTypeID = Convert.ToInt32(_inscan.StatusTypeId);

                _cstatus.CourierStatusID = Convert.ToInt32(_inscan.CourierStatusID);
                //if (_cstatus.CourierStatusID == 13)
                //{
                //    AWBTrackStatu aWBTrackStatu = db.AWBTrackStatus.Where(cc => cc.InScanId == InScanID && cc.CourierStatusId == _inscan.CourierStatusID).FirstOrDefault();
                //    _cstatus.ShipmentReturned = aWBTrackStatu.ShipmentReturned;
                //    _cstatus.Remarks = aWBTrackStatu.Remarks;
                //}
                var courierstatus = db.CourierStatus.Find(_cstatus.CourierStatusID);
                _cstatus.CourierStatusText = courierstatus.StatusType + " - " + courierstatus.CourierStatus;


            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = _cstatus, result = status }, JsonRequestBehavior.AllowGet);

        }
        public string SaveConsignee(QuickAWBVM v)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName == v.Consignee && c.CustomerType == "CN" select c).FirstOrDefault();

            int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (cust == null)
            {
                CustomerMaster obj = new CustomerMaster();

                int max = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerID).FirstOrDefault();

                obj.CustomerID = max + 1;
                obj.AcCompanyID = accompanyid;

                obj.CustomerCode = ""; // _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
                obj.CustomerName = v.Consignee;
                obj.CustomerType = "CN"; //Consignee

                obj.ContactPerson = v.ConsigneeContact;
                obj.Address1 = v.ConsigneeAddress1_Building;
                obj.Address2 = v.ConsigneeAddress2_Street;
                obj.Address3 = v.ConsigneeAddress3_PinCode;
                obj.Phone = v.ConsigneePhone;
                obj.CountryName = v.ConsigneeCountryName;
                obj.CityName = v.ConsigneeCityName;
                obj.LocationName = v.ConsigneeLocationName;
                db.CustomerMasters.Add(obj);
                db.SaveChanges();

            }


            return "ok";

        }

        public int SaveCustomer(QuickAWBVM v)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName == v.customer select c).FirstOrDefault();  //--c.CustomerType == "CR"
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotid = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            if (cust == null)
            {
                CustomerMaster obj = new CustomerMaster();

                int max = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerID).FirstOrDefault();

                obj.CustomerID = max + 1;
                obj.AcCompanyID = accompanyid;

                obj.CustomerCode = ""; // _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
                obj.CustomerName = v.customer;//  v.Consignor;
                obj.CustomerType = "CS"; //Cash customer

                obj.ContactPerson = v.ConsignorContact;
                obj.Address1 = v.ConsignorAddress1_Building;
                obj.Address2 = v.ConsignorAddress2_Street;
                obj.Address3 = v.ConsignorAddress3_PinCode;
                obj.Phone = v.ConsignorPhone;
                obj.CountryName = v.ConsignorCountryName;
                obj.CityName = v.ConsignorCityName;
                obj.LocationName = v.ConsignorLocationName;
                obj.UserID = null;
                obj.statusCommission = false;
                obj.Referal = "";
                obj.StatusActive = true;
                obj.StatusTaxable = false;
                obj.CreditLimit = 0;
                obj.Email = "";
                obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                obj.CurrencyID = Convert.ToInt32(Session["CurrencyID"].ToString());
                // Convert.ToInt32(Session["UserID"].ToString());
                obj.CreatedBy = uid;
                obj.CreatedDate = CommonFunctions.GetCurrentDateTime();
                obj.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                obj.ModifiedBy = uid;
                obj.DepotID = depotid;
                obj.Mobile = v.ConsignorMobile;
                db.CustomerMasters.Add(obj);
                db.SaveChanges();
                ReceiptDAO.ReSaveCustomerCode();
                return obj.CustomerID;                
            }
            else
            {
                return cust.CustomerID;
            }

        }

        public string AddAWBTrackStatus(int inscanid,int EMPID,int pStatusTypeId=0,int pCourierStatusId=0 )
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            var inscan = db.InScanMasters.Where(itm => itm.InScanID == inscanid).FirstOrDefault();
            var awbtrack = db.AWBTrackStatus.Where(cc => cc.InScanId == inscanid && cc.StatusTypeId==pStatusTypeId && cc.CourierStatusId==pCourierStatusId).OrderByDescending(cc => cc.EntryDate).FirstOrDefault();
            if (awbtrack != null)
            {
                //if (awbtrack.CourierStatusId == inscan.CourierStatusID)
                //{
                    
                //}
                return "same status";
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
            _awbstatus.EntryDate = inscan.TransactionDate; // DateTime.Now;
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
        public ActionResult PrintAWBRegister()
        {
            DatePicker datePicker = (DatePicker)Session["AWBRegisterPrintSearh"]; // SessionDataModel.GetTableVariable();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            DateTime pFromDate;
            DateTime pToDate;
            List<QuickAWBVM> lst = new List<QuickAWBVM>();
            if (datePicker == null)
                {                    

                    //int pStatusId = 0;
                    pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                    pToDate = CommonFunctions.GetLastDayofMonth().Date;                     
                    datePicker = new DatePicker();
                    datePicker.FromDate = pFromDate;
                    datePicker.ToDate = pToDate;
                    datePicker.paymentId = 0;
                    datePicker.StatusId = 0;
                    datePicker.AcHeadId = 0;
                
            }
                            
            Session["AWBRegisterPrintSearch"]=datePicker;
            lst = PickupRequestDAO.PrintAWBRegister();

            //lst = (from c in db.InScanMasters
            //       join pet in db.tblStatusTypes on c.StatusTypeId equals pet.ID into gj
            //       from subpet in gj.DefaultIfEmpty()
            //       join pet1 in db.CourierStatus on c.CourierStatusID equals pet1.CourierStatusID into gj1
            //       from subpet1 in gj1.DefaultIfEmpty()
            //       join pay in db.tblPaymentModes on c.PaymentModeId equals pay.ID
            //       join fwd in db.ForwardingAgentMasters on c.FAgentId equals fwd.FAgentID into gj2
            //       from subpet2 in gj2.DefaultIfEmpty()
            //       where c.BranchID == branchid
            //       && (c.TransactionDate >= pFromDate && c.TransactionDate < pToDate)

            //      //&& (c.CourierStatusID == pStatusId || pStatusId == 0)
            //      && c.IsDeleted == false
            //       && (c.PaymentModeId == datePicker.paymentId || datePicker.paymentId == 0)
            //       && (c.AcHeadID == datePicker.AcHeadId || datePicker.AcHeadId == 0)
            //       orderby c.TransactionDate descending, c.AWBNo descending
            //       select new QuickAWBVM
            //       {
            //           HAWBNo = c.AWBNo,
            //           shippername = c.Consignor,
            //           consigneename = c.Consignee,
            //           origin = c.ConsignorCountryName,
            //           destination = c.ConsigneeCountryName,
            //           InScanID = c.InScanID,
            //           InScanDate = c.TransactionDate,
            //           CourierStatus = subpet1.CourierStatus,
            //           StatusType = subpet.Name,
            //           Pieces = c.Pieces,
            //           Weight = c.Weight,
            //           CourierCharge = c.CourierCharge,
            //           OtherCharge = c.OtherCharge,
            //           totalCharge = c.NetTotal,
            //           MovementTypeID = c.MovementID == null ? 0 : c.MovementID.Value,
            //           paymentmode = pay.PaymentModeText,
            //           ConsigneePhone = c.ConsigneePhone,
            //           ConsigneeMobile = c.ConsigneeMobileNo,
            //           ConsigneeAddress1_Building = c.ConsigneeAddress1_Building + "," + c.ConsigneeAddress2_Street,
            //           ConsigneeAddress3_PinCode = c.ConsigneeAddress3_PinCode,
            //           ConsigneeLocationName = c.ConsigneeLocationName,
            //           ConsigneeCityName = c.ConsigneeCityName,
            //           ConsigneeCountryName = c.ConsigneeCountryName,
            //           Description = c.CargoDescription,
            //       }).ToList();

            //int qindex = 0;
            //foreach (QuickAWBVM item in lst)
            //{
            //    var FAWBDet = (from c in db.InScanInternationals join d in db.ForwardingAgentMasters on c.FAgentID equals d.FAgentID where c.InScanID == item.InScanID select new { FAgentName = d.FAgentName, ForwardingAWBNo = c.ForwardingAWBNo }).FirstOrDefault(); 
            //    if (FAWBDet !=null)
            //    {
            //        lst[qindex].FWDAgentNumber=FAWBDet.ForwardingAWBNo;
            //        lst[qindex].FAgentName = FAWBDet.FAgentName;
            //    }
            //    qindex = qindex + 1;
            //}
            //if (datePicker.SelectedValues != null)
            //{
            //    lst=lst.Where(tt => tt.MovementTypeID != null).ToList().Where(cc => datePicker.SelectedValues.ToList().Contains(cc.MovementTypeID.Value)).ToList(); 
            //}
            
            //foreach(QuickAWBVM item in lst)
            //{
            //    if (lst[qindex].OtherCharge>0)
            //    {
            //        int? _inscanid = lst[qindex].InScanID;
            //        var othercharge = (from c in db.InscanOtherCharges join m in db.OtherCharges on c.OtherChargeID equals m.OtherChargeID where c.InscanID == _inscanid && m.TaxApplicable == true select c).ToList();
            //        if (othercharge.Count > 0)
            //        {
            //            decimal? plAmount = othercharge.Sum(i => i.Amount);
            //            lst[qindex].OtherCharge = plAmount;
            //            lst[qindex].totalCharge = lst[qindex].CourierCharge + plAmount;
            //        }

            //    }
            //    else
            //    {
            //        lst[qindex].totalCharge = lst[qindex].CourierCharge;
            //    }
            //    qindex = qindex + 1;
            //}
            //ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
            //ViewBag.ToDate = pToDate.Date.AddDays(-1).ToString("dd-MM-yyyy");
            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            //ViewBag.StatusId = StatusId;
            ViewBag.AcHeads = (from c in db.EmployeeMasters join a in db.AcHeads on c.AcHeadID equals a.AcHeadID select new { AcHeadId = a.AcHeadID, HeadName = a.AcHead1 }).ToList();
            decimal? plCourierCharge =lst.Sum(i => i.CourierCharge);
            decimal? pltotal = lst.Sum(i => i.totalCharge);
            decimal? plothertotal = lst.Sum(i => i.OtherCharge);

            return View(lst);

        }

        public ActionResult Details(int id=0)
        {
            AWBTracking model = new AWBTracking();
            var inscan1= db.InScanMasters.Find(id);

            if (inscan1 != null)
                model.AWBNo = inscan1.AWBNo;

            if ((model.AWBNo == null || model.AWBNo == "") && (model.ParentAWBNo != null && model.ParentAWBNo != ""))
            {
                var inscan = db.InScanMasters.Where(cc => cc.AWBNo == model.ParentAWBNo).FirstOrDefault();
                if (inscan != null)
                {
                    var inscanawb = db.InScanMasters.Where(cc => cc.ParentInScanID == inscan.InScanID).FirstOrDefault();
                    if (inscanawb != null)
                    {
                        model.AWBNo = inscanawb.AWBNo;
                    }
                }

            }
            Session["AWBTracking"] = model;
            return RedirectToAction("AWBTimeline");
            
        }
        public ActionResult PrintSearch()
        {

            DatePicker datePicker =(DatePicker)Session["AWBRegisterPrintSearch"];

            if (datePicker == null)
            {
                datePicker = new DatePicker();
                datePicker.FromDate = DateTime.Now.Date;
                datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MovementId = "1,2,3,4";
                datePicker.AcHeadId = 0;
            }
            if (datePicker != null)
            {
                if (datePicker.MovementId==null)
                    datePicker.MovementId = "1,2,3,4";

                //ViewBag.Customer = (from c in db.InScanMasters
                //                    join cust in db.CustomerMasters on c.CustomerID equals cust.CustomerID
                //                    where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                //                    select new CustmorVM { CustomerID = cust.CustomerID, CustomerName = cust.CustomerName }).Distinct();

            }

            Session["AWBRegisterPrintSearch"] = datePicker;
            //ViewBag.Movement = new MultiSelectList(db.CourierMovements.ToList(),"MovementID","MovementType");
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.Token = datePicker;
            ViewBag.AcHeads = (from c in db.EmployeeMasters join a in db.AcHeads on c.AcHeadID equals a.AcHeadID select new { AcHeadId = a.AcHeadID, HeadName = a.AcHead1 }).ToList();
            //SessionDataModel.SetTableVariable(datePicker);
            return View(datePicker);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrintSearch([Bind(Include = "FromDate,ToDate,paymentid,MovementId,SelectedValues,AcHeadId")] DatePicker picker)
        {
            DatePicker model = new DatePicker
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Delete = true, // (bool)Token.Permissions.Deletion,
                Update = true, //(bool)Token.Permissions.Updation,
                Create = true, //.ToStrin//(bool)Token.Permissions.Creation
                CustomerId = picker.CustomerId,
                MovementId = picker.MovementId,
                SelectedValues = picker.SelectedValues,
                paymentId=picker.paymentId,
                AcHeadId=picker.AcHeadId
            };
            model.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (model.MovementId == "")
                    {
                        model.MovementId = item.ToString();
                    }
                    else
                    {
                        model.MovementId = model.MovementId + "," + item.ToString();
                    }

                }
            }
            //ViewBag.Token = model;
            Session["AWBRegisterPrintSearh"] = model;
            return RedirectToAction("PrintAWBRegister", "AWB");
            //return PartialView("InvoiceSearch",model);

        }

        public ActionResult GenerateInvoice(int id)
        {
            DatePicker datePicker = new DatePicker();
            var _inscan = db.InScanMasters.Find(id);
            int[] svalues = { Convert.ToInt32(_inscan.MovementID) };

                datePicker = new DatePicker();
                datePicker.FromDate = _inscan.TransactionDate.Date;
                datePicker.ToDate = _inscan.TransactionDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MovementId = _inscan.MovementID.ToString();
                datePicker.SelectedValues = svalues;
                datePicker.CustomerId = _inscan.CustomerID;
                datePicker.CustomerName = db.CustomerMasters.Find(_inscan.CustomerID).CustomerName;

            SessionDataModel.SetTableVariable(datePicker);
            if (_inscan.InvoiceID == null)
            {
                return RedirectToAction("Create", "CustomerInvoice");
            }
            else
            {
                return RedirectToAction("Edit", "CustomerInvoice",new { id = _inscan.InvoiceID });
            }

        }

        [HttpGet]
        public JsonResult GetWalkInCustomer(int id)
        {
            string customername = "";

            if (id == 1)
            { customername = "WALK-IN-CUSTOMER"; }
            else if (id == 2)
            {
                customername = "COD-CUSTOMER";
            }
            else
            {
                return Json(new { CustomerID = 0, CustomerName = "Not found!" }, JsonRequestBehavior.AllowGet);
            }

            var customerlist = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetCODCustomer()
        {
            var customerlist = (from c1 in db.CustomerMasters
                                where c1.CustomerName == "COD-CUSTOMER"
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetCustomerName(string term) 
        {
            bool enablecashcustomer = (bool)Session["EnableCashCustomerInvoice"];
            if (term.Trim()!="")
            {
                if (enablecashcustomer == true)
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive==true && c1.CustomerID > 0 && (c1.CustomerType=="CS" || c1.CustomerType == "CR") && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive==true && c1.CustomerID > 0 && (c1.CustomerType == "CR") && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (enablecashcustomer == true)
                {

                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType=="CS" || c1.CustomerType == "CR")
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CR")
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }
            

          

        }

        [HttpGet]
        public JsonResult GetConsigneeName(string term)
        {
            var customerlist = (from c1 in db.CustomerMasters
                                where c1.CustomerID>0 && c1.CustomerType == "CN" && c1.CustomerName.ToLower().Contains(term.ToLower())
                                orderby c1.CustomerName ascending
                                select new { Name = c1.CustomerName, ContactPerson = c1.ContactPerson, Phone = c1.Phone, LocationName = c1.LocationName, CityName = c1.CityName, CountryName = c1.CountryName, Address1 = c1.Address1, Address2 = c1.Address2, PinCode = c1.Address3 }).ToList();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetReceiverName(string term)
        {
            //c1.ConsignorMobileNo == null ? "" : c1.ConsignorMobileNo,
            var shipperlist = (from c1 in db.InScanMasters
                               where c1.Consignee.ToLower().StartsWith(term.ToLower())
                               orderby c1.Consignee ascending
                               select new { Name = c1.Consignee, ContactPerson = c1.ConsigneeContact, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building, Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();

            return Json(shipperlist, JsonRequestBehavior.AllowGet);                       

        }

        [HttpGet]
        public JsonResult GetShipperName(string term)
        {

            if (term.Trim() !="")
            {
                var shipperlist = (from c1 in db.InScanMasters
                                   where c1.IsDeleted==false && c1.Consignor.ToLower().StartsWith(term.ToLower())
                                   orderby c1.Consignor ascending
                                   select new { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();
                return Json(shipperlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var shipperlist = (from c1 in db.InScanMasters
                                   where c1.IsDeleted==false
                                   orderby c1.Consignor ascending
                                   select new { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();
                return Json(shipperlist, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet]
        public ActionResult GetOtherChargeAll(string term)
        {
            //MastersModel MM = new MastersModel();
            int CompanyId= CommonFunctions.ParseInt(Session["CurrentCompanyID"].ToString());

            if (!String.IsNullOrEmpty(term))
            {
                var othercharges = db.OtherCharges.Where(cc => cc.AcCompanyID== CompanyId && cc.OtherCharge1.ToLower().StartsWith(term.ToLower())).OrderBy(cc => cc.OtherCharge1).ToList();                               

                //MM.GetAnalysisHeadSelectList(Common.ParseInt(Session["AcCompanyID"].ToString()), term);
                return Json(othercharges, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var othercharges = db.OtherCharges.Where(cc => cc.AcCompanyID == CompanyId).OrderBy(cc => cc.OtherCharge1).ToList();
                term = "";
                return Json(othercharges, JsonRequestBehavior.AllowGet);                
            }
        }


        [HttpGet]
        public JsonResult GetProductTypeDetail(int id)
        {
            string customername = "";

            
            var list = (from c1 in db.ProductTypes
                                where c1.ProductTypeID==id
                                
                                select new { ProductTypeID = c1.ProductTypeID, ProductTypeName = c1.ProductName,CBMBased=c1.CBMBasedCharges,Volume=c1.VolumeMetricBased, VolumeWeight=c1.VolumeWeight,CBM=c1.CBM,Length=c1.Length,Width=c1.Width,Height=c1.Height,CustomBox=c1.CustomBox }).FirstOrDefault();

          

            return Json(list, JsonRequestBehavior.AllowGet);

        }
        public void SaveConsignorAddress(QuickAWBVM model)
        {

            bool newentry = false;
            try
            {


                ConsignorMaster obj = db.ConsignorMasters.Where(cc => cc.ConsignorName == model.shippername).FirstOrDefault();

                if (obj == null)
                {
                    newentry = true;
                    obj = new ConsignorMaster();
                }
                obj.ConsignorName = model.shippername;
                if (model.ConsignorContact == null)
                {
                    obj.ConsignorContactName = "";
                }
                else
                {
                    obj.ConsignorContactName = model.ConsignorContact;
                }
                //obj.ConsignorContactName = model.ConsignorContact;
                obj.ConsignorPhoneNo = model.ConsignorPhone;
                if (model.ConsignorAddress1_Building == null)
                {
                    obj.ConsignorAddress1 = "";
                }
                else
                {
                    obj.ConsignorAddress1 = model.ConsignorAddress1_Building;
                }
                if (model.ConsignorAddress2_Street == null)
                {
                    obj.ConsignorAddress2 = "";
                }
                else
                {
                    obj.ConsignorAddress2 = model.ConsignorAddress2_Street;
                }
                if (model.ConsignorAddress3_PinCode == null)
                {
                    obj.ConsignorAddress3 = "";
                }
                else
                {
                    obj.ConsignorAddress3 = model.ConsignorAddress3_PinCode;
                }
                obj.MobileNo = model.ConsignorMobile;
                obj.ConsignorLocationName = model.ConsignorLocationName;
                obj.ConsignorCountryname = model.ConsignorCountryName;
                obj.ConsignorCityName = model.ConsignorCityName;

                if (newentry == true)
                {
                    db.ConsignorMasters.Add(obj);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                string exmessage = ex.Message;
            }
        }


        
        public void SaveConsigeeAddress(QuickAWBVM model)
        {
           bool newentry = false;
            try
            {
                ConsigneeMaster obj = db.ConsigneeMasters.Where(cc => cc.ConsigneeName == model.Consignee && cc.ConsignorName == model.shippername).FirstOrDefault();

                if (obj == null)
                {
                    newentry = true;
                    obj = new ConsigneeMaster();
                }
                obj.ConsignorName = model.shippername;
                obj.ConsigneeName = model.Consignee;
                if (model.ConsigneeContact==null)
                {
                    obj.ConsigneeContactName = "";
                }
                else
                {
                    obj.ConsigneeContactName = model.ConsigneeContact;
                }
                if (model.ConsigneeAddress1_Building==null)
                {
                    obj.ConsigneeAddress1 = "";
                }
                else
                {
                    obj.ConsigneeAddress1 = model.ConsigneeAddress1_Building;
                }
                if (model.ConsigneeAddress2_Street==null)
                {
                    obj.ConsigneeAddress2 = "";
                }
                else
                {
                    obj.ConsigneeAddress2 = model.ConsigneeAddress2_Street;
                }
                if (model.ConsigneeAddress3_PinCode==null)
                {
                    obj.ConsigneeAddress3 = "";
                    
                }
                else
                {
                    obj.ConsigneeAddress3 = model.ConsigneeAddress3_PinCode;
                }
                if (model.ConsigneePhone == null)
                {
                    obj.ConsigneePhoneNo = "";
                }
                else
                {
                    obj.ConsigneePhoneNo = model.ConsigneePhone;
                }
                
                obj.MobileNo = model.ConsigneeMobile;
                obj.ConsigneeLocationName = model.ConsigneeLocationName;
                obj.ConsigneeCountryname = model.ConsigneeCountryName;
                obj.ConsigneeCityName = model.ConsigneeCityName;

                if (newentry == true)
                {
                    db.ConsigneeMasters.Add(obj);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                string exmessage = ex.Message;
            }
        }
        public class selectdata
        {
            public int id { get; set; }
            public string text { get; set; }
        }

        public class ChangeStatus
        {
            public int InboundShipmentID { get; set; }
            public int InScanID { get; set; }
            public int StatusTypeID { get; set; }
            public int CourierStatusID { get; set; }

            public string CourierStatusText { get; set; }
            public string Remarks { get; set; }
            public bool ShipmentReturned { get; set; }
            
        }
        //public static Image ConvertToImage(Binary iBinary)
        //{
        //    var arrayBinary = iBinary.ToArray();
        //    Image rImage = null;

        //    using (MemoryStream ms = new MemoryStream(arrayBinary))
        //    {
        //        rImage = Image.FromStream(ms);
        //    }
        //    return rImage;
        //}


        public JsonResult GetHAWB(string ForwardingAgentNO)
        {
            string status = "ok";
            string awbno = "";
            ChangeStatus _cstatus = new ChangeStatus();
            try
            {
                ExportShipmentDetail _inscan = db.ExportShipmentDetails.Where(cc=>cc.FwdAgentAWBNo==ForwardingAgentNO).FirstOrDefault();
                if (_inscan!=null)
                {
                    awbno = _inscan.AWBNo;
                    return Json(new { status="OK", awbno=awbno }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "Failed", awbno = awbno }, JsonRequestBehavior.AllowGet);
                }

            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = _cstatus, result = status }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AWBTimeline(int id=0)
        {
            string companyname = Session["CompanyName"].ToString();
            AWBTracking model = new AWBTracking();
            if (id > 0)
            {
                
                var inscan1 = db.InScanMasters.Find(id);

                if (inscan1 != null)
                    model.AWBNo = inscan1.AWBNo;

                if ((model.AWBNo == null || model.AWBNo == "") && (model.ParentAWBNo != null && model.ParentAWBNo != ""))
                {
                    var inscan = db.InScanMasters.Where(cc => cc.AWBNo == model.ParentAWBNo).FirstOrDefault();
                    if (inscan != null)
                    {
                        var inscanawb = db.InScanMasters.Where(cc => cc.ParentInScanID == inscan.InScanID).FirstOrDefault();
                        if (inscanawb != null)
                        {
                            model.AWBNo = inscanawb.AWBNo;
                        }
                    }

                }
                Session["AWBTracking"] = model;
            }
            else
            {

            }
            
            AWBTracking obj = (AWBTracking)Session["AWBTracking"];
            
            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            if (obj==null)
            {
                model.AWB = new QuickAWBVM();
                model.AWB.ConsigneeAddress2_Street = "";
                model.AWB.Consignee = "";
                model.AWB.InScanID = 0;
                model.AWB.InboundShipmentID = 0;
                model.AWB.HAWBNo = "";
                model.AWBNo = "";
                model.AWB.Consignor = "";
                model.AWB.ConsignorContact = "";
                model.AWB.ConsignorCountryName = "";
                model.AWB.ConsignorAddress1_Building = "";
                model.AWB.ConsignorAddress2_Street = "";
                model.AWB.ConsignorAddress3_PinCode = "";
                model.AWB.ConsigneeAddress3_PinCode = "";
                model.AWB.ConsigneeAddress2_Street = "";
                model.AWB.ConsigneeAddress1_Building = "";
                model.AWB.Consignee = "";
                model.AWB.ConsigneeContact = "";
                model.AWB.ConsigneeCountryName = "";
                model.AWB.Pieces = "";
                model.AWB.Weight = 0;
                model.AWB.CourierCharge = 0;
                model.AWB.OtherCharge = 0;
                model.AWB.totalCharge = 0;
                model.AWB.Description = "";
                model.AWB.IsCashOnly = false;
                model.AWB.IsNCND = false;
                model.AWB.IsNCND = false;
                model.AWB.IsChequeOnly = false;
                model.AWB.IsChequeOnly = false;
                model.AWB.CourierStatus = "";
                model.AWB.IsCollectMaterial = false;
                model.AWB.IsCollectMaterial = false;
                model.AWB.IsDOCopyBack = false;
                model.AWB.PaymentModeId = null;
                model.AWB.ConsigneePhone = "";
                model.AWB.ConsigneeMobile ="";
                model.Details = new List<AWBTrackStatusVM>();
                model.AWBNo = "";
                model.PODImage = new tblPodImage();
                model.PODStatus = new POD();                
                model.PODSignature = new PODSignature();
                model.MCDetail = new MCDetail();
                model.ManifestDetail = new AWBManifest();
                model.AWB.TaxInvoiceDetail = "";
            }
            else
            {
                if (obj.AWBNo == null)
                    obj.AWBNo = "";
                model.AWBNo = obj.AWBNo.Trim();
                if (obj.AWBNo == null)
                    obj.AWBNo = "";
                if (obj.AWBNo!=null || obj.AWBNo.Trim()!="" )
                {      model.AWBNo = obj.AWBNo.Trim();
                    model.AWB = (from c in db.InScanMasters where (c.IsDeleted == false || c.IsDeleted == null) && c.AWBNo == obj.AWBNo.Trim() select new QuickAWBVM { InboundShipmentID=0, InScanID = c.InScanID, HAWBNo = c.AWBNo, TransactionDate =c.TransactionDate,  Consignor = c.Consignor, ConsignorContact = c.Consignor, ConsignorCountryName = c.ConsignorCountryName,ConsignorAddress1_Building=c.ConsignorAddress1_Building,ConsignorAddress2_Street=c.ConsignorAddress2_Street,ConsignorAddress3_PinCode=c.ConsignorAddress3_PinCode,  ConsigneeAddress1_Building=c.ConsigneeAddress1_Building,ConsigneeAddress2_Street=c.ConsigneeAddress2_Street,ConsigneeAddress3_PinCode=c.ConsigneeAddress3_PinCode,  Consignee = c.Consignee, ConsigneeContact = c.ConsigneeContact, ConsigneeCountryName = c.ConsigneeCountryName, Pieces = c.Pieces, Weight = c.Weight, CourierCharge = c.CourierCharge, OtherCharge = c.OtherCharge, totalCharge = c.NetTotal , TaxAmount=c.TaxAmount,InvoiceId=c.InvoiceID, Description=c.CargoDescription,IsCashOnly=c.IsCashOnly,IsNCND=c.IsNCND,IsChequeOnly=c.IsChequeOnly,IsCollectMaterial=c.IsCollectMaterial, IsDOCopyBack=c.IsDOCopyBack,PaymentModeId=c.PaymentModeId ,ConsigneePhone=c.ConsigneePhone,ConsigneeMobile=c.ConsigneeMobileNo ,FagentID=c.FAgentId,ParentInScanID=c.ParentInScanID,ChildInScanID=c.ChildInScanID,PickupLocation=c.PickupLocation,DeliveryLocation=c.DeliveryLocation,ConsignorLocationName=c.ConsignorLocationName,ConsignorCityName=c.ConsignorCityName,ConsigneeCityName=c.ConsigneeCityName,ConsigneeLocationName=c.ConsigneeLocationName, ImportShipmentId=c.ImportShipmentId ,ManifestID =c.ManifestID,MovementTypeID =c.MovementID ,CourierStatusId=c.CourierStatusID}).FirstOrDefault();
                    string specialinstruc = "";
                    if (model.AWB != null)
                    {
                         if (model.AWB.FagentID!=null)
                        {
                            int fagentid = Convert.ToInt32(model.AWB.FagentID);
                            var FAgent = db.ForwardingAgentMasters.Where(cc=>cc.FAgentID==fagentid).FirstOrDefault();
                            if (FAgent!=null)
                            {
                                model.AWB.FAgentName = FAgent.FAgentName;                                                                
                            }

                        }
                        if (model.AWB.MovementTypeID>0)
                        {
                            model.AWB.MovementType = db.CourierMovements.Find(model.AWB.MovementTypeID).MovementType;
                        }
                        //if (model.AWB.ParentInScanID != null)
                        //{
                        //    int parentinscanid = Convert.ToInt32(model.AWB.ParentInScanID);
                        //    var parentinscan = db.InScanMasters.Find(parentinscanid);
                        //    if (parentinscan != null)
                        //    {
                        //        model.ParentAWBNo = parentinscan.AWBNo;
                        //        model.AWB.ParentAWBNo = parentinscan.AWBNo;
                        //    }

                        //}
                        //else
                        //{
                        //    model.ParentAWBNo = "";
                        //    model.AWB.ParentAWBNo = "";
                        //}


                         

                        if (model.AWB.IsNCND)
                            specialinstruc = "NCND,";
                        if (model.AWB.IsCashOnly)
                        {
                            specialinstruc = specialinstruc + " Cash Only,";
                        }
                        if (model.AWB.IsChequeOnly)
                        {
                            specialinstruc = specialinstruc + " Cheque Only,";
                        }
                        if (model.AWB.IsCollectMaterial)
                        {
                            specialinstruc = specialinstruc + " Collect Material,";
                        }
                        if (model.AWB.IsDOCopyBack)
                        {
                            specialinstruc = specialinstruc + " Do Copy Back,";
                        }
                        model.AWB.SpecialNotes = specialinstruc;
                        if (model.AWB.PaymentModeId != null)
                        {
                            model.AWB.paymentmode = db.tblPaymentModes.Find(model.AWB.PaymentModeId).PaymentModeText;
                        }
                        else
                        {
                            model.AWB.paymentmode = "";
                        }
                        if (model.AWB.InvoiceId == null)
                            model.AWB.InvoiceId = 0;
                        if ( model.AWB.InvoiceId >0)
                        {
                            var invoice = db.CustomerInvoices.Find(model.AWB.InvoiceId);
                            if (invoice!=null)
                            model.AWB.InvoiceStatus = invoice.CustomerInvoiceNo + "/" + invoice.InvoiceDate.ToString("dd-MM-yyyy");
                        }

                        if (model.AWB.ImportShipmentId==null)
                        {
                            model.AWB.ImportShipmentId = 0;
                        }
                        if(model.AWB.ImportShipmentId>0 && model.AWB.InvoiceId>0)
                        {
                            var invoice = db.AgentInvoices.Find(model.AWB.InvoiceId);
                            if (invoice!=null)
                            model.AWB.InvoiceStatus = invoice.InvoiceNo + "/" + invoice.InvoiceDate.ToString("dd-MM-yyyy");
                        }

                        

                        model.AWB.CODStatus = "Pending";
                        model.AWB.MaterialCostStatus = "Pending";
                        if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                        {
                            model.Details = (from c in db.AWBTrackStatus
                                             join c1 in db.EmployeeMasters on c.EmpID equals c1.EmployeeID
                                             where c.InScanId == model.AWB.InScanID
                                             orderby c.EntryDate descending
                                             select new AWBTrackStatusVM { InScanId = c.InScanId, EntryDate = c.EntryDate, CourierStatus = c.CourierStatus, ShipmentStatus = c.ShipmentStatus, UserName = c1.EmployeeName, DRSID = c.DRSID, Remarks = c.Remarks }).ToList();

                        }
                        else
                        {
                            model.Details = (from c in db.AWBTrackStatus
                                             join c1 in db.EmployeeMasters on c.EmpID equals c1.EmployeeID
                                             where c.InScanId == model.AWB.InScanID
                                             orderby c.CourierStatusId descending, c.EntryDate descending
                                             select new AWBTrackStatusVM { InScanId = c.InScanId, EntryDate = c.EntryDate, CourierStatus = c.CourierStatus, ShipmentStatus = c.ShipmentStatus, UserName = c1.EmployeeName, DRSID = c.DRSID, Remarks = c.Remarks }).ToList();
                        }
                        int i = 0;
                  

                        if (model.Details != null)
                        {
                            foreach (var item in model.Details)
                            {
                                if (item.DRSID > 0)
                                {
                                    var drs = db.DRS.Find(item.DRSID);
                                    if (drs != null)
                                    {
                                        if (model.Details[i].Remarks == null)
                                            model.Details[i].Remarks = "";
                                        model.Details[i].Remarks = model.Details[i].Remarks + " " + drs.DRSNo;
                                    }
                                }
                                i++;
                            }
                        }
                        //new code on aug 26 2024
                        if (model.AWB != null)
                        {
                            if (model.AWB.CourierStatusId>0)
                              model.AWB.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == model.AWB.CourierStatusId).FirstOrDefault().CourierStatus;
                        }
                        model.PODStatus = db.PODs.Where(cc => cc.InScanID == model.AWB.InScanID || cc.AWBNo==model.AWB.HAWBNo).FirstOrDefault();
                        if (model.PODStatus != null)
                        {
                            int podid = model.PODStatus.PODID;
                            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                            {
                                model.PODStatus.ReceivedTime = CommonFunctions.GetBranchDateTime(Convert.ToDateTime(model.PODStatus.ReceivedTime));
                            }
                            else
                            {
                                model.PODStatus.ReceivedTime = Convert.ToDateTime(model.PODStatus.ReceivedTime);
                            }
                           
                          
                            var podimage = db.tblPodImages.Where(cc => cc.PODID == podid).FirstOrDefault();
                            if (podimage == null)
                            {
                                model.PODImage = new tblPodImage();
                            }
                            else
                            {
                                model.PODImage = podimage;
                            }
                            int? podid1 = Convert.ToInt32(podid);
                            var podignature = db.PODSignatures.Where(cc => cc.InscanID == model.AWB.InScanID).FirstOrDefault();
                            if (podignature == null)
                            {
                                model.PODSignature = new PODSignature();
                            }
                            else
                            {
                                model.PODSignature = podignature;
                                //Image dd = ConvertToImage(model.PODSignature.SignatureImage);
                                //string ss = Convert.ToBase64String(model.PODSignature.SignatureImage);
                            }
                        }
                        else
                        {
                            model.PODImage = new tblPodImage();
                            model.PODStatus = new POD();
                            model.PODSignature = new PODSignature();
                        }
                        model.MCDetail = new MCDetail();
                        model.MCDetail = PickupRequestDAO.GetAWBMCDetail(model.AWB.InScanID);
                        if (model.MCDetail.MCAmount == 0 && model.AWB.materialcost!=null)
                            model.MCDetail.MCAmount = Convert.ToDecimal(model.AWB.materialcost);
                        //Manifest
                        if (model.AWB.ManifestID != null)
                        {
                            if (model.AWB.ManifestID > 0)
                            {
                                model.ManifestDetail = AWBDAO.GetShipmentManifestInformation(model.AWB.InboundShipmentID, 0, model.AWB.ManifestID);
                            }
                            else
                            {
                                model.ManifestDetail = new AWBManifest();
                            }
                        }
                        else
                        {
                            model.ManifestDetail = new AWBManifest();
                        }
                      
                    }
                    else
                    {
                        model.AWB = (from c in db.InboundShipments where c.AWBNo == obj.AWBNo select new QuickAWBVM { InScanID=0, InboundShipmentID = c.ShipmentID, HAWBNo = c.AWBNo, Consignor = c.Consignor, ConsignorContact = c.ConsignorContact,ConsignorCityName=c.ConsignorCityName, ConsignorCountryName = c.ConsignorCountryName, ConsignorAddress1_Building = c.ConsignorAddress1_Building, ConsignorAddress2_Street = c.ConsignorAddress2_Street, ConsignorAddress3_PinCode = c.ConsignorAddress3_PinCode, ConsigneeAddress1_Building = c.ConsigneeAddress1_Building, ConsigneeAddress2_Street = c.ConsigneeAddress2_Street, ConsigneeAddress3_PinCode = c.ConsigneeAddress3_PinCode, Consignee = c.Consignee, ConsigneeContact = c.ConsigneeContact, ConsigneeCountryName = c.ConsigneeCountryName, Pieces = c.Pieces.ToString(), Weight = c.Weight, CourierCharge = c.CourierCharge, materialcost = c.MaterialCost, OtherCharge = c.OtherCharge, totalCharge = c.NetTotal, Description = c.CargoDescription, IsCashOnly = false, IsNCND = false, IsChequeOnly = false, IsCollectMaterial = false, IsDOCopyBack = false, PaymentModeId = 3, ConsigneePhone = c.ConsigneePhone , TaxAmount =c.TaxAmount,SurchargeAmount=c.SurchargeAmount , ManifestID =c.ManifestID,TransactionDate=c.AWBDate,MovementTypeID=c.MovementID,ConsignorPhone =c.ConsignorPhone,ConsigneeMobile=c.ConsigneeMobileNo,ConsignorMobile=c.ConsignorMobileNo,CourierStatusId=c.CourierStatusID}).FirstOrDefault();
                        //model.AWB = (from c in db.ImportShipmentDetails where c.AWB == obj.AWBNo select new QuickAWBVM { InScanID = c.ShipmentDetailID, HAWBNo = c.AWB, Consignor = c.Shipper, ConsignorContact = "", ConsignorCountryName = "", ConsignorAddress1_Building = "", ConsignorAddress2_Street = "", ConsignorAddress3_PinCode = "", ConsigneeAddress1_Building = c.ReceiverAddress, ConsigneeAddress2_Street = "", ConsigneeAddress3_PinCode = "", Consignee = c.Receiver, ConsigneeContact = c.ReceiverContact, ConsigneeCountryName = "", Pieces = c.PCS.ToString(), Weight = c.Weight, CourierCharge = c.COD, materialcost = c.CustomValue, OtherCharge = 0, totalCharge = c.COD, Description = c.Contents, IsCashOnly = false, IsNCND = false, IsChequeOnly = false, IsCollectMaterial = false, IsDOCopyBack = false, PaymentModeId = 3, ConsigneePhone = c.ReceiverTelephone }).FirstOrDefault();
                        if (model.AWB != null)
                        {
                            var shipmentinvoiceDetail = db.ShipmentInvoiceDetails.Where(cc => cc.ShipmentImportDetailID == model.AWB.InboundShipmentID).FirstOrDefault();
                            if (shipmentinvoiceDetail != null)
                            {
                                var shipmentInvoice = db.ShipmentInvoices.Find(shipmentinvoiceDetail.ShipmentInvoiceID);
                                if (shipmentInvoice != null)
                                {
                                    model.AWB.TaxInvoiceDetail = shipmentInvoice.InvoiceNo + "/" + Convert.ToDateTime(shipmentInvoice.InvoiceDate).ToShortDateString() + " Tax:" + CommonFunctions.GetFormatNumber(shipmentinvoiceDetail.Tax.ToString(),"2") + " Admin Charge:" + CommonFunctions.GetFormatNumber(shipmentinvoiceDetail.adminCharges.ToString(),"2");
                                }
                            }
                            List<AWBTrackStatusVM> details = new List<AWBTrackStatusVM>();
                            model.Details = (from c in db.AWBTrackStatus
                                             join c1 in db.EmployeeMasters on c.EmpID equals c1.EmployeeID
                                             where c.InboundShipmentID== model.AWB.InboundShipmentID
                                             orderby c.EntryDate descending
                                             select new AWBTrackStatusVM { ShipmentDetailID = c.ShipmentDetailID, EntryDate = c.EntryDate, CourierStatus = c.CourierStatus, ShipmentStatus = c.ShipmentStatus, UserName = c1.EmployeeName, CourierStatusId = c.CourierStatusId,Remarks=c.Remarks,DRSID=c.DRSID }).ToList();

                            int i = 0;
                           
                            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                            {
                                if (model.Details != null)
                                {
                                    foreach (var item in model.Details)
                                    {
                                        model.Details[i].EntryDate = CommonFunctions.GetBranchDateTime(Convert.ToDateTime(model.Details[i].EntryDate));
                                        i++;
                                    }
                                }
                            }
                            i = 0;
                            if (model.Details != null)
                            {
                                foreach (var item in model.Details)
                                {
                                    if (item.DRSID > 0)
                                    {
                                        var drs = db.DRS.Find(item.DRSID);
                                        if (drs != null)
                                        {
                                            if (model.Details[i].Remarks == null)
                                                model.Details[i].Remarks = "";
                                            model.Details[i].Remarks = model.Details[i].Remarks + " " + drs.DRSNo;
                                        }
                                    }
                                    i++;
                                }
                            }

                            if (model.AWB.MovementTypeID > 0)
                            {
                                model.AWB.MovementType = db.CourierMovements.Find(model.AWB.MovementTypeID).MovementType;
                            }
                            //model.Details = details;
                            model.AWB.paymentmode = "COD";
                            model.AWB.CODStatus = "Not Applicable";
                            model.AWB.MaterialCostStatus = "Not Applicable";
                            //new code on aug 26 2024
                            if (model.AWB != null)
                            {
                                if (model.AWB.CourierStatusId > 0)
                                    model.AWB.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == model.AWB.CourierStatusId).FirstOrDefault().CourierStatus;
                            }
                            model.PODStatus = db.PODs.Where(cc => cc.AWBNo == model.AWB.HAWBNo).FirstOrDefault();// || cc.AWBNo==model.AWB.HAWBNo).FirstOrDefault();
                            if (model.PODStatus==null)
                                model.PODStatus = db.PODs.Where(cc => cc.InboundShipmentID == model.AWB.InboundShipmentID).FirstOrDefault();// || cc.AWBNo==model.AWB.HAWBNo).FirstOrDefault();
                            
                            if (model.PODStatus != null)
                            {
                                int podid = model.PODStatus.PODID;
                                if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                                {
                                    model.PODStatus.ReceivedTime = CommonFunctions.GetBranchDateTime(Convert.ToDateTime(model.PODStatus.ReceivedTime));
                                }
                                else
                                {
                                    model.PODStatus.ReceivedTime = Convert.ToDateTime(model.PODStatus.ReceivedTime);
                                }
                                var podimage = db.tblPodImages.Where(cc => cc.PODID == podid).FirstOrDefault();
                                if (podimage == null)
                                {
                                    model.PODImage = new tblPodImage();
                                }
                                else
                                {
                                    model.PODImage = podimage;
                                }
                                int? podid1 = Convert.ToInt32(podid);
                                var podignature = db.PODSignatures.Where(cc => cc.PODID == podid1).FirstOrDefault();
                                if (podignature == null)
                                {
                                    model.PODSignature = new PODSignature();
                                }
                                else
                                {
                                    model.PODSignature = podignature;
                                    //Image dd = ConvertToImage(model.PODSignature.SignatureImage);
                                    //string ss = Convert.ToBase64String(model.PODSignature.SignatureImage);
                                }
                                model.MCDetail = new MCDetail();
                                model.MCDetail.MCAmount = 0;
                                if (model.MCDetail.MCAmount == 0 && model.AWB.materialcost != null)
                                    model.MCDetail.MCAmount = Convert.ToDecimal(model.AWB.materialcost);
                            }
                            else
                            {
                                model.PODStatus = new POD();
                                model.PODSignature = new PODSignature();
                                model.PODImage = new tblPodImage();
                                model.MCDetail = new MCDetail();

                            }


                            //Manifest
                            if (model.AWB.ManifestID!=null)
                            {
                                if (model.AWB.ManifestID > 0)
                                {
                                    model.ManifestDetail = AWBDAO.GetShipmentManifestInformation(model.AWB.InboundShipmentID, 0, model.AWB.ManifestID);
                                }
                                else
                                {
                                    model.ManifestDetail = new AWBManifest();
                                }
                            }
                            else
                            {
                                model.ManifestDetail = new AWBManifest();
                            }
                        }
                        
                        else
                        {
                            model.AWB = (from c in db.ImportShipmentDetails where c.AWB == obj.AWBNo select new QuickAWBVM { InScanID = c.ShipmentDetailID, HAWBNo = c.AWB, Consignor = c.Shipper, ConsignorContact = "", ConsignorCountryName = "", ConsignorAddress1_Building = "", ConsignorAddress2_Street = "", ConsignorAddress3_PinCode = "", ConsigneeAddress1_Building = c.ReceiverAddress, ConsigneeAddress2_Street = "", ConsigneeAddress3_PinCode = "", Consignee = c.Receiver, ConsigneeContact = c.ReceiverContact, ConsigneeCountryName = "", Pieces = c.PCS.ToString(), Weight = c.Weight, CourierCharge = c.COD, materialcost = c.CustomValue, OtherCharge = 0, totalCharge = c.COD, Description = c.Contents, IsCashOnly = false, IsNCND = false, IsChequeOnly = false, IsCollectMaterial = false, IsDOCopyBack = false, PaymentModeId = 3, ConsigneePhone = c.ReceiverTelephone, ConsigneeMobile="",ConsigneeCityName="" }).FirstOrDefault();
                            if (model.AWB != null)
                            {
                                List<AWBTrackStatusVM> details3 = new List<AWBTrackStatusVM>();
                                model.Details = (from c in db.AWBTrackStatus
                                                 join c1 in db.EmployeeMasters on c.EmpID equals c1.EmployeeID
                                                 where c.ShipmentDetailID == model.AWB.InScanID
                                                 orderby c.EntryDate
                                                 select new AWBTrackStatusVM { ShipmentDetailID = c.ShipmentDetailID, EntryDate = c.EntryDate, CourierStatus = c.CourierStatus, ShipmentStatus = c.ShipmentStatus, UserName = c1.EmployeeName, CourierStatusId = c.CourierStatusId }).ToList();

                                int i = 0;
                                if (model.Details != null)
                                {
                                    foreach (var item in model.Details)
                                    {
                                        model.Details[i].EntryDate = CommonFunctions.GetBranchDateTime(Convert.ToDateTime(model.Details[i].EntryDate));
                                        i++;
                                    }
                                }
                                //model.Details = details;
                                model.AWB.paymentmode = "COD";
                                model.AWB.CODStatus = "Not Applicable";
                                model.AWB.MaterialCostStatus = "Not Applicable";
                                model.PODStatus = db.PODs.Where(cc => cc.ShipmentDetailId == model.AWB.InScanID).FirstOrDefault();
                                model.ManifestDetail = new AWBManifest();
                                if (model.PODStatus != null)
                                {
                                    int podid = model.PODStatus.PODID;

                                    model.PODStatus.ReceivedTime = CommonFunctions.GetBranchDateTime(Convert.ToDateTime(model.PODStatus.ReceivedTime));
                                    var podimage = db.tblPodImages.Where(cc => cc.PODID == podid).FirstOrDefault();
                                    if (podimage == null)
                                    {
                                        model.PODImage = new tblPodImage();
                                    }
                                    else
                                    {
                                        model.PODImage = podimage;
                                    }
                                    int? podid1 = Convert.ToInt32(podid);
                                    var podignature = db.PODSignatures.Where(cc => cc.PODID == podid1).FirstOrDefault();
                                    if (podignature == null)
                                    {
                                        model.PODSignature = new PODSignature();
                                    }
                                    else
                                    {
                                        model.PODSignature = podignature;
                                        //Image dd = ConvertToImage(model.PODSignature.SignatureImage);
                                        //string ss = Convert.ToBase64String(model.PODSignature.SignatureImage);
                                    }
                                    model.MCDetail = new MCDetail();
                                }
                                else
                                {
                                    model.PODStatus = new POD();
                                    model.PODSignature = new PODSignature();
                                    model.PODImage = new tblPodImage();
                                    model.MCDetail = new MCDetail();
                                    model.ManifestDetail = new AWBManifest();

                                }
                            }
                            else
                            {
                                model.AWB = new QuickAWBVM();
                                model.MCDetail = new MCDetail();
                                List<AWBTrackStatusVM> details4 = new List<AWBTrackStatusVM>();
                                model.PODStatus = new POD();
                                model.PODSignature = new PODSignature();
                                model.PODImage = new tblPodImage();
                                model.Details = details4;
                                model.ManifestDetail = new AWBManifest();
                                TempData["ErrorMsg"] = "AWB No. Not Found!";
                            }
                            //model.AWB = new QuickAWBVM();
                            //model.MCDetail = new MCDetail();
                            //List<AWBTrackStatusVM> details = new List<AWBTrackStatusVM>();
                            //model.PODStatus = new POD();
                            //model.PODSignature = new PODSignature();
                            //model.PODImage = new tblPodImage();
                            //model.Details = details;
                            //model.ManifestDetail = new AWBManifest();
                            //TempData["ErrorMsg"] = "AWB No. Not Found!";
                        }
                    }
                }
                else
                {
                    model.AWB = new QuickAWBVM();
                    List<AWBTrackStatusVM> details = new List<AWBTrackStatusVM>();
                    model.PODStatus = new POD();
                    model.PODSignature = new PODSignature();
                    model.PODImage = new tblPodImage();
                    model.Details = details;
                    model.MCDetail = new MCDetail();
                    model.ManifestDetail = new AWBManifest();
                    TempData["ErrorMsg"] = "AWB No. Not Found!";
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult AWBTimeline(AWBTracking model)
        {
            if ((model.AWBNo==null || model.AWBNo=="") && (model.ParentAWBNo!=null && model.ParentAWBNo!=""))
            {
                var inscan = db.InScanMasters.Where(cc => cc.AWBNo == model.ParentAWBNo).FirstOrDefault();
                if (inscan!=null)
                {
                    var inscanawb = db.InScanMasters.Where(cc => cc.ParentInScanID == inscan.InScanID).FirstOrDefault();
                    if (inscanawb!=null)
                    {
                        model.AWBNo = inscanawb.AWBNo;
                    }
                }

            }
            Session["AWBTracking"] = model;
            return RedirectToAction("AWBTimeline");
        }

        [HttpPost]
        public JsonResult RecheckAWBStatus(string AWBNo)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            string status = "ok";
            //   string result = "ok";
            string message = "";
            
            try
            {

                SkylarkDAO.RecheckAWBStatus(AWBNo);


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
        [HttpPost]
        public JsonResult SaveStaffNotes(StaffNotesVM obj )
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            StaffNote _note = new StaffNote();
            try
            {
                if (obj.NotesId == 0)
                {
                    int id = (from c in db.StaffNotes orderby c.NotesId descending select c.NotesId).FirstOrDefault();
                    _note.NotesId = id + 1;
                    _note.InScanId = obj.InScanId;
                    _note.Notes = obj.Notes;
                    _note.UserId = uid;
                    _note.EmployeeId = db.EmployeeMasters.Where(cc => cc.UserID == uid).FirstOrDefault().EmployeeID;
                    _note.EntryDate = DateTime.Now;
                    db.StaffNotes.Add(_note);
                    db.SaveChanges();
                }
                else
                {
                    _note = db.StaffNotes.Find(obj.NotesId);
                    _note.Notes = obj.Notes;
                    _note.UserId = uid;
                    _note.EmployeeId = db.EmployeeMasters.Where(cc => cc.UserID == uid).FirstOrDefault().EmployeeID;
                    _note.EntryDate = DateTime.Now;
                    db.Entry(_note).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return Json(new { status = "ok", result = "Notes Saved Successfull!" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "failed", result = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetStaffNotes(int InScanId)
        {
            var staffnotes = (from c in db.StaffNotes
                              join e in db.EmployeeMasters on c.EmployeeId equals e.EmployeeID where c.InScanId==InScanId
                              select new StaffNotesVM { NotesId = c.NotesId, InScanId = c.InScanId, Notes = c.Notes, EntryDate = c.EntryDate, EmployeeName = e.EmployeeName, UserId = e.UserID }).ToList();

            return Json(new { status = "ok", data=staffnotes }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveCustomerNotification(CustomerNotificationVM obj)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            CustomerNotification _note = new CustomerNotification();
            try
            {
                if (obj.NotificationId == 0)
                {
                    int id = (from c in db.CustomerNotifications orderby c.NotificationId descending select c.NotificationId).FirstOrDefault();
                    _note.NotificationId = id + 1;
                    _note.InScanId = obj.InScanId;
                    _note.MessageText = obj.MessageText;
                    _note.UserId = uid;                    
                    _note.EntryDate = DateTime.Now;
                    _note.NotifyBySMS = obj.NotifyBySMS;
                    _note.NotifyByWhatsApp = obj.NotifyByWhatsApp;
                    _note.NotifyByEmail = obj.NotifyByEmail;
                    db.CustomerNotifications.Add(_note);
                    db.SaveChanges();

                    if (_note.NotifyByEmail)
                    {
                        EmailDAO _edao = new EmailDAO();
                        _edao.SendCustomerAWBNoNotification(obj.CustomerEmail, obj.CustomerName, obj.MessageText, obj.AWBNo);
                    }

                }
                else
                {
                    _note = db.CustomerNotifications.Find(obj.NotificationId);
                    _note.MessageText = obj.MessageText;
                    _note.UserId = uid;                    
                    _note.EntryDate = DateTime.Now;
                    db.Entry(_note).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return Json(new { status = "ok", result = "Customer Notification Saved Successfull!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "failed", result = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetCustomerNotification(int InScanId)
        {
            var customer = (from _ins in db.InScanMasters join _cus in db.CustomerMasters on _ins.CustomerID equals _cus.CustomerID where _ins.InScanID==InScanId select new { customername = _cus.CustomerName ,custemail=_cus.Email }).FirstOrDefault();

            var customernotes = (from c in db.CustomerNotifications
                              join e in db.EmployeeMasters on c.UserId equals e.UserID where c.InScanId==InScanId
                              select new CustomerNotificationVM { NotificationId = c.NotificationId, InScanId = c.InScanId, MessageText = c.MessageText, EntryDate = c.EntryDate, EmployeeName = e.EmployeeName, UserId = c.UserId }).ToList();

            string _customername = customer.customername;
            string emailid = customer.custemail;
            return Json(new { status = "ok", data = customernotes ,customername= _customername ,custemail=emailid }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AWBPrint(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            // ViewBag.Country = db.CountryMasters.ToList();
            ViewBag.Customer = db.CustomerMasters.ToList();
            //ViewBag.City = db.CityMasters.ToList();
            //ViewBag.Location = db.LocationMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.FAgent = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList(); // )// .ForwardingAgentMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.customerrate = db.CustomerRateTypes.ToList();
            ViewBag.CourierDescription = db.CourierDescriptions.ToList();
            ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();
            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();

            
            QuickAWBVM v = new QuickAWBVM();
            if (id > 0)           
            {
                //ViewBag.Enquiry = db.InScanMasters.ToList();
                v = GetAWBDetail(id);
                v.AWBTermsConditions = db.GeneralSetups.Where(cc => cc.BranchId == branchid && cc.SetupTypeID == 2).FirstOrDefault().Text1;

                ViewBag.AWBNo = v.HAWBNo;
                if (v.CourierStatusId == null)
                    ViewBag.CourierStatusId = 0;
                else
                    ViewBag.CourierStatusId = v.CourierStatusId;
                ViewBag.StatusType = v.StatusType;
                ViewBag.CourierStatus = v.CourierStatus;
                
                var comp = db.AcCompanies.Find(v.AcCompanyID);

                ViewBag.CurrencyName = db.CurrencyMasters.Find(comp.CurrencyID).Symbol;

                if (comp.LogoFileName == "" || comp.LogoFileName == null)
                {
                    ViewBag.LogoPath = "/UploadFiles/" + "defaultlogo.png";
                }
                else
                {
                    ViewBag.LogoPath = "/UploadFiles/" + comp.LogoFileName;
                }

            }

            return View(v);

        }

       
        public ActionResult Edit(int id)
        {
            QuickAWBVM inscan = new QuickAWBVM();

            string AWBNo = string.Empty;
            //ViewBag.Country = db.CountryMasters.ToList();
            ViewBag.Customer = db.CustomerMasters.ToList();
            //ViewBag.City = db.CityMasters.ToList();
            //ViewBag.Location = db.LocationMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.FAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.customerrate = db.CustomerRateTypes.ToList();
            ViewBag.CourierDescription = db.CourierDescriptions.ToList();
            ViewBag.Enquiry = db.CustomerEnquiries.ToList();
            InScanMaster data = (from c in db.InScanMasters where c.InScanID == id select c).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                inscan.InScanID = data.InScanID;
                //inscan.EnquiryID =Convert.ToInt32(data.EnquiryID);
                inscan.HAWBNo = data.AWBNo;
                inscan.InScanDate = data.TransactionDate;
                inscan.Consignor = data.Consignor;
                //inscan.AcJournalID = data.AcJournalID.Value;

                //if (data.CourierCharge != null)
                //{
                //    inscan.CourierCharge = data.CourierCharge;
                //}
                //else
                //{
                //    inscan.CourierCharge = 0;
                //}

                //if (data.BalanceAmt != null)
                //{
                //    inscan.totalCharge = data.BalanceAmt.Value;
                //}
                //else
                //{
                //    inscan.totalCharge = 0;
                //}

                //if (data.PackingCharge != null)
                //{
                //    inscan.PackingCharge = data.PackingCharge.Value;
                //}
                //else
                //{
                //    inscan.PackingCharge = 0;
                //}

                if (data.Weight != null)
                {
                    inscan.Weight = data.Weight.Value;
                }
                else
                {
                    inscan.Weight = 0;
                }

                //inscan.paymentmode = data.StatusPaymentMode;
                inscan.ConsignorCountryName = data.ConsignorCountryName;
                inscan.ConsignorCityName = data.ConsignorCityName;
                inscan.ConsigneeCountryName = data.ConsigneeCountryName;
                inscan.ConsigneeCityName = data.ConsigneeCityName;
                inscan.ConsignorCountryName = data.ConsignorCountryName;
                inscan.ConsigneeCountryName = data.ConsigneeCountryName;

                inscan.ConsignorCityName = data.ConsignorCityName;
                inscan.ConsigneeCityName = data.ConsigneeCityName;
                inscan.CustomerID = data.CustomerID.Value;
                //inscan.ProductType = data.CourierServiceID.Value;
                //inscan.TaxconfigurationID = data.TaxconfigurationID.Value;
                inscan.Consignee = data.Consignee;

                inscan.ConsigneeAddress1_Building = data.ConsigneeAddress1_Building;
                inscan.ConsigneeAddress2_Street = data.ConsigneeAddress2_Street;
                inscan.ConsigneeAddress3_PinCode = data.ConsigneeAddress3_PinCode;

                inscan.ConsigneePhone = data.ConsigneePhone;
                inscan.ConsignorPhone = data.ConsignorPhone;
                inscan.ConsigneeContact = data.ConsigneeContact;
                inscan.ConsignorContact = data.ConsignorContact;

                // inscan.Pieces = data.Pieces;
                inscan.ConsignorLocationName = data.ConsignorLocationName;
                inscan.ConsigneeLocationName = data.ConsigneeLocationName;

                //inscan.totalCharge = data.BalanceAmt.Value;
                //inscan.materialcost = data.MaterialCost.Value;
                inscan.Description = data.CargoDescription;
                
                inscan.ReceivedBy = 1; // "tesT"; // data.ReceivedBy.Value;
                inscan.PickedBy = 1;// "test1"; //data.ReceivedBy.Value;


                var d = (from c in db.InScanInternationals where c.InScanID == inscan.InScanID select c).FirstOrDefault();
                if (d != null)
                {
                    inscan.FagentID = d.FAgentID;
                    inscan.FAWBNo = d.ForwardingAWBNo;
                   // inscan.ForwardingDate = d.ForwardingDate;
                    //inscan.VerifiedWeight = d.VerifiedWeight;
                    inscan.ForwardingCharge = d.ForwardingCharge;
                }
            }

           
            return View(inscan);
        
        }
        [HttpPost]
        public ActionResult Edit(QuickAWBVM v)
        {

            InScanMaster inscan = new InScanMaster();
            inscan.InScanID = v.InScanID;
            inscan.EnteredByID = Convert.ToInt32(Session["UserID"]);
            inscan.AWBNo = v.HAWBNo;
            inscan.InScanID = v.InScanID;
            //inscan.AcJournalID = v.AcJournalID;
           // inscan.InScanDate = v.InScanDate;
            if (v.CourierCharge != null)
            {
                inscan.CourierCharge = Convert.ToDecimal((v.CourierCharge));
            }
            //if (v.totalCharge != null)
            //{
            //    inscan.OtherCharge = Convert.ToDecimal(v.OtherCharge);
            //}
            //if (v.PackingCharge != null)
            //{
            //    inscan.PackingCharge = Convert.ToDecimal(v.PackingCharge);
            //}
            if (v.Weight != null)
            {
                inscan.StatedWeight = Convert.ToDouble(v.Weight);
            }
            //if (v.paymentmode != null)
            //{
            //    inscan.StatusPaymentMode = v.paymentmode;
            //}

            
            inscan.PaymentModeId = v.PaymentModeId;            


            if (v.CustomerID != null)
            {
                inscan.CustomerID = v.CustomerID;
            }
            
            //if (v.ProductType != null)
            //{
            //    inscan.CourierServiceID = v.ProductType;
            //}
            //if (v.CourierType != null)
            //{
            //    inscan.CourierDescriptionID = v.CourierType;
            //}
            //if (v.CourierMode != null)
            //{
            //    inscan.TaxconfigurationID = v.CourierMode;
            //}
            inscan.ConsignorCountryName = v.ConsignorCountryName;
            inscan.ConsignorCityName = v.ConsignorCityName;
            inscan.ConsigneeCountryName = v.ConsigneeCountryName;
            inscan.ConsigneeCityName = v.ConsigneeCityName;
            inscan.Consignee = v.Consignee;
            inscan.Consignor = v.Consignor;
            inscan.ConsigneeAddress1_Building = v.ConsigneeAddress1_Building;
            inscan.ConsigneeAddress2_Street = v.ConsigneeAddress2_Street;
            inscan.ConsigneeAddress3_PinCode = v.ConsigneeAddress3_PinCode;

            inscan.ConsigneePhone = v.ConsigneePhone;
            inscan.ConsignorPhone = v.ConsignorPhone;
            inscan.ConsigneeContact = v.ConsigneeContact;
            inscan.ConsignorContact = v.ConsignorContact;

            inscan.Pieces = v.Pieces.ToString();
            
            

            //if (v.totalCharge != null)
            //{
            //    inscan.BalanceAmt = Convert.ToInt32(v.totalCharge);
            //}
            //if (v.materialcost != null)
            //{
            //    inscan.MaterialCost = Convert.ToInt32(v.materialcost);
            //}
            //inscan.InScanDate = DateTime.UtcNow;


            //inscan.ReceivedByID = v.ReceivedBy;
            //inscan.ReceivedBy = v.PickedBy;

            db.Entry(inscan).State = EntityState.Modified;
            db.SaveChanges();


            var obj = (from c in db.InScanInternationals where c.InScanID == v.InScanID select c).FirstOrDefault();

            obj.InScanID = v.InScanID;
            obj.FAgentID = Convert.ToInt32(v.FagentID);
            obj.ForwardingCharge = v.ForwardingCharge;
            obj.ForwardingAWBNo = v.FAWBNo;
            obj.ForwardingDate = v.ForwardingDate;
            obj.StatusAssignment = v.StatusAssignment;

            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ShowCustomerEntry(string FieldName)
        {
            CustmorVM vm = new CustmorVM();
            return PartialView("CustomerEntry", vm);
        }

        [HttpPost]
        public JsonResult SaveCustomerEntry(CustmorVM model)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName.Trim().ToLower() == model.CustomerName.Trim().ToLower() select c).FirstOrDefault();
            if (cust != null)
            {
                model.CustomerID = cust.CustomerID;
                return Json(new { data = model, message = "Customer Already Exist", status = "Failed" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int uid = Convert.ToInt32(Session["UserID"].ToString());
                CustomerMaster vm1 = new CustomerMaster();
                vm1.CustomerName = model.CustomerName;
                vm1.Address1 = model.Address1;
                vm1.Address2 = model.Address2;
                vm1.Address3 = model.Address3;
                vm1.LocationName = model.LocationName;
                vm1.CityName = model.CityName;
                vm1.CountryName = model.CountryName;
                vm1.CustomerType = model.CustomerType;
                vm1.Phone = model.Phone;
                vm1.Mobile = model.Mobile;
                vm1.AcCompanyID = 1;
                vm1.StatusActive = true;
                vm1.CreatedBy = uid;
                vm1.CreatedDate = CommonFunctions.GetCurrentDateTime();
                vm1.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                vm1.ModifiedBy = uid;
                vm1.ApprovedBy = vm1.CreatedBy;
                vm1.ApprovedOn= CommonFunctions.GetCurrentDateTime();
                db.CustomerMasters.Add(vm1);
                db.SaveChanges();
                ReceiptDAO.ReSaveCustomerCode();
                return Json(new { data = vm1, message = "Customer Saved Successfully",status="Ok" }, JsonRequestBehavior.AllowGet);
            }

            
            

            
        }


        [HttpPost]
        public ActionResult ShowLocationEntry()
        {
            LocationVM vm = new LocationVM();
            return PartialView("LocationEntry", vm);
        }
        [HttpPost]
        public JsonResult SaveLocationEntryold(LocationVM model)
        {
            LocationVM objCust = new LocationVM();
            LocationMaster vm1 = new LocationMaster();
            if (model.Location != "" && model.Location != null)
            {
                vm1 = (from c in db.LocationMasters where c.LocationName == model.Location && c.CountryName == model.CountryName && c.CityName == model.CityName select c).FirstOrDefault();
            }
            else if (vm1.CityName !="" && vm1.CountryName!="")
            {
                model.Location = "";
                vm1 = (from c in db.LocationMasters where c.CityName == model.CityName && c.CountryName ==model.CountryName select c).FirstOrDefault();
            }
            else if (vm1.CountryName != "")
            {
                model.Location = "";
                vm1 = (from c in db.LocationMasters where c.CountryName == model.CountryName select c).FirstOrDefault();
            }
            if (vm1 != null)
            {
                model.LocationID = vm1.LocationID;
                return Json(new { data = model, message = "Location Already Exist", status = "Failed" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                vm1 = new LocationMaster();

                vm1.LocationName = model.Location;
                vm1.CityName = model.CityName;
                vm1.CountryName = model.CountryName;
                
                db.LocationMasters.Add(vm1);
                db.SaveChanges();
                Session["CityList"] = null;
                Session["CountryList"] = null;
                Session["LocationList"] = null;
                return Json(new { data = vm1, message = "Location Saved Successfully", status = "Ok" }, JsonRequestBehavior.AllowGet);
            }





        }

        [HttpPost]
        public JsonResult SaveLocationEntry(LocationVM model)
        {
            int CountryId = 0;
            int CityId = 0;
            int LocationId = 0;
            if (model.CityName == null)
                model.CityName = "";
            if (model.Location == null)
            {
                model.Location = "";
            }
            if (model.CountryName == null)
                model.CountryName = "";

            if (model.CountryCode == null)
            {
                model.CountryCode = "";
            }

            if (model.CountryName != "")
            {
                var country = db.CountryMasters.Where(cc => cc.CountryName.ToLower() == model.CountryName.Trim().ToLower()).FirstOrDefault();
                if (country != null)
                {

                    if (model.CountryCode != null)
                        country.CountryCode = model.CountryCode.Trim();
                    db.Entry(country).State = EntityState.Modified;
                    db.SaveChanges();

                    CountryId = country.CountryID;
                }
                else
                {
                    CountryMaster countrynew = new CountryMaster();
                    countrynew.CountryName = model.CountryName.Trim();
                    if (model.CountryCode != null)
                        countrynew.CountryCode = model.CountryCode.Trim();
                    db.CountryMasters.Add(countrynew);
                    db.SaveChanges();
                    CountryId = countrynew.CountryID;
                }

            }
            if (model.CityName != "" && model.CityName != null)
            {
                var city = db.CityMasters.Where(cc => cc.City.ToLower() == model.CityName.Trim().ToLower()).FirstOrDefault();
                if (city != null)
                {
                    CityId = city.CityID;
                    city.CountryID = CountryId;
                    db.Entry(city).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    CityMaster citynew = new CityMaster();
                    citynew.City = model.CityName.Trim();
                    citynew.CountryID = CountryId;
                    db.CityMasters.Add(citynew);
                    db.SaveChanges();
                    CityId = citynew.CityID;
                }
            }
            if (model.Location != null && model.Location.Trim() != "")
            {
                var Location = db.LocationMasters.Where(cc => cc.Location.ToLower() == model.Location.Trim().ToLower()).FirstOrDefault();
                if (Location != null)
                {
                    LocationId = Location.LocationID;
                    Location.CityID = CityId;
                    db.Entry(Location).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    LocationMaster locationnew = new LocationMaster();
                    locationnew.Location = model.Location;
                    locationnew.LocationName = model.Location;
                    locationnew.CityID = CityId;
                    locationnew.CityName = model.CityName;
                    locationnew.CountryName = model.CountryName;
                    db.LocationMasters.Add(locationnew);
                    db.SaveChanges();
                    LocationId = locationnew.LocationID;
                }
            }
            List<LocationVM> lst1 = PickupRequestDAO.GetLocationName();
            Session["LocationList"] = lst1;
            
            LocationVM vm = new LocationVM();
            vm.LocationID = LocationId;
            vm.CityID = CityId;
            vm.CountryID = CountryId;
            vm.CityName = model.CityName;
            vm.CountryName = model.CountryName;
            vm.Location = model.Location;
            vm.LocationName = model.Location;
            vm.CountryCode = model.CountryCode;

            return Json(new { data = vm, status = "ok", message = "Location Saved Successfully" }, JsonRequestBehavior.AllowGet);

            //LocationVM objCust = new LocationVM();
            //LocationMaster vm1 = new LocationMaster();
            //if (model.Location != "" && model.Location != null)
            //{
            //    vm1 = (from c in db.LocationMasters where c.LocationName == model.Location && c.CountryName == model.CountryName && c.CityName == model.CityName select c).FirstOrDefault();
            //}
            //else if (vm1.CityName != "" && vm1.CountryName != "")
            //{
            //    model.Location = "";
            //    vm1 = (from c in db.LocationMasters where c.CityName == model.CityName && c.CountryName == model.CountryName select c).FirstOrDefault();
            //}
            //else if (vm1.CountryName != "")
            //{
            //    model.Location = "";
            //    vm1 = (from c in db.LocationMasters where c.CountryName == model.CountryName select c).FirstOrDefault();
            //}
            //if (vm1 != null)
            //{
            //    model.LocationID = vm1.LocationID;
            //    return Json(new { data = model, message = "Location Already Exist", status = "Failed" }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{

            //}





        }

        public JsonResult GetImportAWBRef(string AWBNo)
        {

            var inscanentry = db.InScanMasters.Where(cc => cc.AWBNo == AWBNo.Trim() && cc.IsDeleted==false).FirstOrDefault();
            if (inscanentry!=null)
            {
                if (inscanentry.CustomerID != null && inscanentry.CustomerID != 0)
                {
                    var result = (from inscan in db.InScanMasters
                                  join cus in db.CustomerMasters on inscan.CustomerID equals cus.CustomerID
                                  where inscan.InScanID == inscanentry.InScanID
                                  select new { Status = "Ok", InScanID = inscan.InScanID, AWBNo = inscan.AWBNo, AWBDate = inscan.TransactionDate, Consignor = inscan.Consignor, ConsignorCountry = inscan.ConsignorCountryName, Consignee = inscan.Consignee, ConsigneeCountry = inscan.ConsigneeCountryName, ConsigneeCity = inscan.ConsigneeCityName, ConsigneeContact = inscan.ConsigneeContact, ConsigneeAddress1 = inscan.ConsigneeAddress1_Building, ConsigneeAddress2 = inscan.ConsigneeAddress2_Street, ConsigneeAddress3 = inscan.ConsigneeAddress3_PinCode, ConsigneeLocation = inscan.ConsigneeLocationName, ConsigneeMobile = inscan.ConsigneeMobileNo, ConsigneePhone = inscan.ConsigneePhone, CustomerID = inscan.CustomerID, CustomerName = cus.CustomerName,Weight=inscan.Weight,Piecs=inscan.Pieces,CargoDescription=inscan.CargoDescription, ProductTypeID = inscan.ProductTypeID,ParcelTypeID=inscan.ParcelTypeId }).FirstOrDefault();
                    //var customer = db.CustomerMasters.Find(result.CustomerId);
                    //result.CustomerName = customer.CustomerName;
                    return Json(new {data=result,Status="Ok" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = (from inscan in db.InScanMasters                                  
                                  where inscan.InScanID == inscanentry.InScanID
                                  select new { Status = "Ok", PaymentModeId=inscan.PaymentModeId, InScanID = inscan.InScanID, AWBNo = inscan.AWBNo, AWBDate = inscan.TransactionDate, Consignor = inscan.Consignor, ConsignorCountry = inscan.ConsignorCountryName, Consignee = inscan.Consignee, ConsigneeCountry = inscan.ConsigneeCountryName, ConsigneeCity = inscan.ConsigneeCityName, ConsigneeContact = inscan.ConsigneeContact, ConsigneeAddress1 = inscan.ConsigneeAddress1_Building, ConsigneeAddress2 = inscan.ConsigneeAddress2_Street, ConsigneeAddress3 = inscan.ConsigneeAddress3_PinCode, ConsigneeLocation = inscan.ConsigneeLocationName, ConsigneeMobile = inscan.ConsigneeMobileNo, ConsigneePhone = inscan.ConsigneePhone, CustomerID = inscan.CustomerID ,CustomerName="" }).FirstOrDefault();
                    
                    return Json(new { data=result,Status="Ok" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new {  Status = "Failed" }, JsonRequestBehavior.AllowGet);
            }
                        

        }
        //var ahead = db.AcHeadAssigns.Where(x => x.BranchID == CurrentBranchID).FirstOrDefault();

        //var ajm = new AcJournalMaster()
        //{
        //    AcJournalID = GetMaxAcJournalID()+1,
        //    VoucherNo = "C.Note" + AWBNo,
        //    TransDate = DateTime.UtcNow,
        //    AcFinancialYearID = Convert.ToInt32(Session["CurrentYear"].ToString()),
        //    VoucherType = "DX",
        //    TransType = 1,
        //    StatusDelete = false,
        //    Remarks = "",
        //    UserID = Convert.ToInt32(Session["UserID"].ToString()),
        //    AcCompanyID = Convert.ToInt32(Session["CurrenctCompanyID"].ToString()),
        //    //BranchID = this.CurrentBranchID,
        //    Reference = "",
        //    ShiftID = 0,
        //};


        //db.AcJournalMasters.Add(ajm);
        //db.SaveChanges();

        //if (v.paymentmode != "CSR" && v.totalCharge > 0)
        //{
        //    if (ahead != null)
        //    {
        //        int aheadassign = 0;

        //        switch (v.paymentmode)
        //        {
        //            case "COD": aheadassign = ahead.CODControlID.Value; break;
        //            case "FOC": aheadassign = ahead.FOCControlID.Value; break;
        //            case "PKP": aheadassign = ahead.UnPostedSalesAcHeadID.Value; break;
        //            default:
        //                break;
        //        }

        //        if (aheadassign != 0 && Convert.ToDecimal(v.totalCharge) != 0)
        //        {
        //            AcJournalDetail ajd = new AcJournalDetail()
        //            {
        //                AcJournalDetailID = GetMaxAcJournalDetailID()+1,
        //                AcJournalID = ajm.AcJournalID,
        //                AcHeadID = aheadassign,
        //                Amount = Convert.ToDecimal(v.totalCharge),
        //                Remarks = "",
        //            };
        //            db.AcJournalDetails.Add(ajd);
        //            db.SaveChanges();

        //            ajd = new AcJournalDetail()
        //            {
        //                AcJournalDetailID = GetMaxAcJournalDetailID() + 1,
        //                AcJournalID = ajm.AcJournalID,
        //                AcHeadID = aheadassign,
        //                Amount = -Convert.ToDecimal(v.totalCharge),
        //                Remarks = "",
        //            };
        //            db.AcJournalDetails.Add(ajd);
        //            db.SaveChanges();
        //        }
        //    }

        //}
        //else if (v.paymentmode == "CSR" && v.totalCharge > 0)
        //{

        //    AcJournalDetail ajd = new AcJournalDetail()
        //    {
        //        AcJournalDetailID = GetMaxAcJournalDetailID()+1,
        //        AcJournalID = ajm.AcJournalID,
        //        AcHeadID = ahead.MaterialCostControlReceivableAcHeadID,
        //        Amount = Convert.ToDecimal(v.totalCharge),
        //        Remarks = "",
        //    };

        //    db.AcJournalDetails.Add(ajd);
        //    db.SaveChanges();

        //    ajd = new AcJournalDetail()
        //    {
        //        AcJournalDetailID = GetMaxAcJournalDetailID() + 1,
        //        AcJournalID = ajm.AcJournalID,
        //        AcHeadID = ahead.MaterialCostControlReceivableAcHeadID,
        //        Amount = -Convert.ToDecimal(v.totalCharge),
        //        Remarks = "",
        //    };
        //    db.AcJournalDetails.Add(ajd);
        //    db.SaveChanges();

        //}

        public int GetMaxAcJournalID()
        {
            int x = (from c in db.AcJournalMasters orderby c.AcJournalID descending select c.AcJournalID).FirstOrDefault();
            return x;
        }

        public int GetMaxAcJournalDetailID()
        {
            int x = (from c in db.AcJournalDetails orderby c.AcJournalDetailID descending select c.AcJournalDetailID).FirstOrDefault();
            return x;
        }

        
        public ActionResult AWBPrintReport(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            
            string companyname = Session["CompanyName"].ToString();
            if (companyname.Trim() == "Airbest Express Cargo & Courier Llc")
            {
                AccountsReportsDAO.GenerateAirbestAWBReport(id);
            }
            else
            {
                AccountsReportsDAO.GenerateAWBReport(id);    
            }

            ViewBag.ReportName = "AWB Print";
            return View();

        }
        public ActionResult AWBPrintLabelReport(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());


            string companyname = Session["CompanyName"].ToString();

            AccountsReportsDAO.GenerateAWBPrintLabelReport(id);

            ViewBag.ReportName = "AWB Print";
            return View();

        }
        public JsonResult GetCustomerData(int id)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerID == id select c).FirstOrDefault();


            objCust.CustID = cust.CustomerID;
            objCust.CustName = cust.CustomerName;
            objCust.ContactPerson = cust.ContactPerson;
            objCust.Address1 = cust.Address1;
            objCust.Address2 = cust.Address2;
            objCust.Address3 = cust.Address3;
            objCust.Phone = cust.Phone;
            objCust.CountryID = cust.CountryID; //.Value;
            objCust.CityID = cust.CityID; //.Value;
            objCust.CustCode = cust.CustomerCode;
            objCust.LocationID = cust.LocationID; //.Value;
        
            return Json(objCust, JsonRequestBehavior.AllowGet);
        }




        public JsonResult GetCustomerDataByNO(string id)
        {
            CustByNo obj = new CustByNo();
            var custmor = (from c in db.InScanMasters where c.EnquiryNo == id select c).FirstOrDefault();
            if (custmor != null)
            {
            obj.InScanID = custmor.InScanID;
            obj.EnquiryNo = custmor.EnquiryNo;
            obj.AWBNo = custmor.AWBNo;
            obj.ConsignorContact = custmor.ConsignorContact;
                obj.ConsigneeContact = custmor.ConsigneeContact;
            
            obj.Weight =Convert.ToDouble(custmor.Weight);
            obj.CustomerID = custmor.CustomerID.Value;
            obj.Consignee = custmor.Consignee;
            obj.Consignor = custmor.Consignor;
            obj.ConsigneeAddress1_Building = custmor.ConsigneeAddress1_Building;
            obj.ConsigneeAddress2_Street = custmor.ConsigneeAddress2_Street;
            obj.ConsigneeAddress3_PinCode = custmor.ConsigneeAddress3_PinCode;

            obj.ConsignorAddress1_Building = custmor.ConsignorAddress1_Building;
            obj.ConsignorAddress2_Street = custmor.ConsignorAddress2_Street;
            obj.ConsignorAddress3_PinCode = custmor.ConsignorAddress3_PinCode;

             obj.ConsigneePhone = custmor.ConsigneePhone;
            obj.ConsignorPhone = custmor.ConsignorPhone;
                obj.ConsignorCountryName = custmor.ConsignorCountryName;
                obj.ConsignorCityName = custmor.ConsignorCityName;
                obj.ConsignorLocationName = custmor.ConsignorLocationName;
                obj.EmployeeID = custmor.AssignedEmployeeID.Value;
             //obj.CollectedEmpID = custmor.CollectedEmpID.Value;
              obj.ConsigneeContact = custmor.ConsigneeContact;
                
                if (custmor.PickedUpEmpID != null)
                    obj.CollectedEmpID =Convert.ToInt32(custmor.PickedUpEmpID);

            obj.ConsignorContact = custmor.ConsignorContact;
            obj.EnteredByID = custmor.EnteredByID.Value;
                obj.ConsigneeCountryName = custmor.ConsigneeCountryName;
                obj.ConsigneeCityName = custmor.ConsigneeCityName;
            obj.ConsigneeLocationName = custmor.ConsigneeLocationName;
       
                  obj.Exist = 1;
            }
            else
            {
                obj.Exist = 0;
            }

            return Json(obj,JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxAWBNo(int ShipmentModeID)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            PickupRequestDAO doa = new PickupRequestDAO();
            var awbno= doa.GetMaAWBNo(companyId, branchid, ShipmentModeID);
            return Json(awbno, JsonRequestBehavior.AllowGet);
        }
       

        public class CustByNo
        {
            public int EnquiryID { get; set; }
            public int InScanID { get; set; }
            public string EnquiryNo { get; set; }
            public string AWBNo { get; set; }
            public int ConsignerCountryId { get; set; }
            public int ConsigneeCountryID { get; set; }
            public int ConsignerCityId { get; set; }
            public int ConsigneeCityId { get; set; }
            public int DescriptionID { get; set; }
            public double? Weight { get; set; }
            public int CustomerID { get; set; }
            public string Consignee { get; set; }
            public string Consignor { get; set; }
            public string ConsigneeAddress1_Building { get; set; }
            public string ConsigneeAddress2_Street { get; set; }
            public string ConsigneeAddress3_PinCode { get; set; }
            public string ConsignorAddress1_Building { get; set; }
            public string ConsignorAddress2_Street { get; set; }
            public string ConsignorAddress3_PinCode { get; set; }
            public string ConsigneePhone { get; set; }
            public string ConsignorPhone { get; set; }
            public int EmployeeID { get; set; }
            public int CollectedEmpID { get; set; }
            public string ConsigneeContact { get; set; }
            public string ConsignorContact { get; set; }
            public int EnteredByID { get; set; }
            public string ConsignorLocationName { get; set; }
            public string ConsigneeLocationName { get; set; }
            public string ConsignorCountryName { get; set; }
            public string ConsignorCityName { get; set; }
            public string ConsigneeCountryName { get; set; }
            public string ConsigneeCityName { get; set; }
            public int Exist { get; set; }
        }


        public JsonResult GetCity(int id)
        {
            List<CityM> objCity = new List<CityM>();
            var city = (from c in db.CityMasters where c.CountryID == id select c).ToList();

            foreach (var item in city)
            {
                objCity.Add(new CityM { City = item.City, CityID = item.CityID });

            }
            return Json(objCity, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocation(int id)
        {
            List<LocationM> objLoc = new List<LocationM>();
            var city = (from c in db.LocationMasters where c.CityID == id select c).ToList();

            foreach (var item in city)
            {
                objLoc.Add(new LocationM { Location = item.Location, LocationID = item.LocationID });

            }
            return Json(objLoc, JsonRequestBehavior.AllowGet);
        }

        public class CityM
        {
            public int CityID { get; set; }
            public String City { get; set; }
        }

        public class LocationM
        {
            public int LocationID { get; set; }
            public String Location { get; set; }
        }

        public class CustM
        {
            public int? CityID { get; set; }
            public int? LocationID { get; set; }
            public int? CountryID { get; set; }
            public string CustName { get; set; }
            public string ContactPerson { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Phone { get; set; }
            public string CustCode { get; set; }
            public int CustID { get; set; }
        }
     
        public JsonResult GetAWB(string id)
        {
            AWB obj = new AWB();
            //var data = (from c in db.InScanMasters where c.AWBNo == id && c.IsDeleted==false select c).FirstOrDefault();
            QuickAWBVM vm = PickupRequestDAO.CheckAWB(id.Trim());
            if (vm!=null && vm.InScanID==0)
            {
                obj.Exist = 0;
                AWBInfo info = AWBDAO.GetAWBInfo(id.Trim());
                obj.AWBInfo = info;
            }
            else if(vm!=null && vm.InScanID>0 && vm.AWBProcessed==false)
            {
                obj.Exist = 0;
            }
            else
            {
                obj.Exist = 1;
            }
            
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        //for drr new pick
        public JsonResult GetAWB1(string id)
        {
            AWB obj = new AWB();
            var data = (from c in db.InScanMasters where c.AWBNo == id && c.IsDeleted == false select c).FirstOrDefault();
            if (data == null)
            {
                obj.Exist = 0;
                obj.BatchID = 0;
                obj.AWBNo = "";
                obj.InScanID = 0;
            }
            else
            {
                obj.Exist = 1;
                obj.AWBNo = data.AWBNo;
                obj.InScanID = data.InScanID;
                obj.BatchID = Convert.ToInt32(data.BATCHID);
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public class AWB
        {
            public int Exist { get; set; }
            public int BatchID { get; set; }
            public int InScanID { get; set; }
            public string AWBNo { get; set; }
            public AWBInfo AWBInfo { get; set; }
        }


        public JsonResult GetPickUpData(string id)
        {
            PickUp objCust = new PickUp();
            var cust = (from c in db.CustomerEnquiries where c.AWBNo == id select c).FirstOrDefault();
            if (cust != null)
            {
                objCust.CustomerID = cust.CustomerID.Value;
                objCust.shipper = cust.Consignor;
                objCust.contactperson = cust.ConsignorContact;
                objCust.shipperaddress = cust.ConsignorAddress;
                objCust.shipperphone = cust.ConsignorPhone;
                objCust.shippercountry = cust.ConsignerCountryId.Value;
                objCust.shippercity = cust.ConsignerCityId.Value;
                objCust.shipperlocation = cust.ConsignorLocationName;
                objCust.weight = cust.Weight.Value;

                objCust.consignee = cust.Consignee;
                objCust.consigneecontact = cust.ConsigneeContact;
                objCust.consigneeaddress = cust.ConsigneeAddress;
                objCust.consigneephone = cust.ConsigneePhone;
                objCust.consigneecountry = cust.ConsigneeCountryID.Value;
                objCust.consigneecity = cust.ConsigneeCityId.Value;
                objCust.consigneelocation = cust.ConsigneeLocationName;

                objCust.Exist = 1;
            }
            else
            {
                objCust.Exist = 0;
            }

           

            return Json(objCust, JsonRequestBehavior.AllowGet);
        }

        public class PickUp
        {
            public int CustomerID { get; set; }
            public string shipper { get; set; }
            public string contactperson { get; set; }
            public string shipperaddress { get; set; }
            public string shipperphone { get; set; }
            public int shippercountry { get; set; }
            public int shippercity { get; set; }
            public string shipperlocation { get; set; }
            public double weight { get; set; }
            public string consignee { get; set; }
            public string consigneecontact { get; set; }
            public string consigneeaddress { get; set; }
            public string consigneephone { get; set; }
            public int consigneecountry { get; set; }
            public int consigneecity { get; set; }
            public string consigneelocation { get; set; }
            public int Exist { get; set; }
        }


        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteInscan(id);
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

        public JsonResult DeleteShipment(int id)
        {
            StatusModel obj = new StatusModel();
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteInscan(id);
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
    }
    }

