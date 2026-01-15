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
 
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ShipmentController : Controller
    {
        Entities1 db = new Entities1();
                
        public ActionResult Index()
        {
           
            


            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            // AWBSearch obj = (AWBSearch)Session["AWBSearch"];
            InboundAWBSearch model =new InboundAWBSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var obj = (InboundAWBSearch)Session["COShipmentSearch"];
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
                //Session["AWBSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.StatusID = 0;
                model.Details = new List<InboundShipmentModel>();
            }
            else
            {
                model = obj;
              
                
                
            }
            Session["COShipmentSearch"] = model;
            List<InboundShipmentModel> lst = InboundShipmentDAO.GetAWBList(obj.StatusID, obj.FromDate, obj.ToDate, branchid, depotId, model.AWBNo, obj.MovementTypeID, obj.PaymentModeId, obj.ConsignorConsignee, obj.Origin, obj.Destination, 0);
            model.Details = lst;
            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
           // ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }
        [HttpPost]
        public ActionResult Index(InboundAWBSearch obj)
        {
            Session["COShipmentSearch"] = obj;
            return RedirectToAction("Index");
        }
        public ActionResult Create(int id = 0)
        {

            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            Session["CityList"] = null;
            Session["CountryList"] = null;
            Session["LocationList"] = null;
            ViewBag.Employee = db.EmployeeMasters.ToList();                       
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();            
            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.ShipperInstructions = db.tblShipperInstructions.ToList();
            ViewBag.FAgent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3 || cc.SupplierTypeID == 4).ToList();
            List<OtherChargeDetailVM> otherchargesvm = new List<OtherChargeDetailVM>();
            List<ShipmentItemDetailVM> shipmentItemVM = new List<ShipmentItemDetailVM>();
            InboundShipmentModel v = new InboundShipmentModel();
            string customername = "";                      
            
            if (id == 0)
            {
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    v.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    v.MovementID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    v.ParcelTypeID = defaultparceltype.ID;


                        
                
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
                v.CurrencyID = CommonFunctions.GetDefaultCurrencyId();
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                
                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());

                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                StatusModel result = AccountsDAO.CheckDateValidate(v.AWBDate.ToString(), yearid);
                if (result.Status == "YearClose") //Period locked
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
                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                StatusModel result = AccountsDAO.CheckDateValidate(v.AWBDate.ToString(), yearid);
                if (result.Status == "YearClose") //Period locked
                {
                    ViewBag.PeriodLock = "true";
                    ViewBag.PeriodLockMessage = result.Message;
                }
                else
                {
                    ViewBag.PeriodLock = "false";
                    ViewBag.PeriodLockMessage = "";
                }
                ViewBag.Title = "Modify";
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
            inscan.Pieces = data.Pieces;
            inscan.AgentInvoiceID = data.AgentInvoiceID;

            if (data.AgentInvoiceID != null)
            {
                if (inscan.AgentInvoiceID > 0)
                {
                    
                    var invoice = db.AgentInvoices.Find(inscan.AgentInvoiceID);
                    if (invoice != null)
                        inscan.InvoiceStatus = invoice.InvoiceNo + "/" + invoice.InvoiceDate.ToString("dd-MM-yyyy");
                }
                
            }
            else
            {
                inscan.AgentInvoiceID = 0;
            }

            inscan.TaxInvoiceID = data.TaxInvoiceID;
            if (data.TaxInvoiceID != null)
            {
                if (inscan.TaxInvoiceID > 0)
                {

                    var invoice = db.ShipmentInvoices.Find(inscan.TaxInvoiceID);
                    if (invoice != null)
                        inscan.TaxInvoiceStatus = invoice.InvoiceNo + "/" + Convert.ToDateTime(invoice.InvoiceDate).ToString("dd-MM-yyyy");
                }

            }
            else
            {
                inscan.TaxInvoiceID = 0;
            }

            int statustypeid = 0;
            
            if (data.StatusTypeId != null && data.StatusTypeId != 0)
                statustypeid = Convert.ToInt32(data.StatusTypeId);

            if (inscan.CourierStatusID == null || inscan.CourierStatusID == 0)
            {
                inscan.StatusType = "INSCAN";
                inscan.CourierStatus = "Collected";
            }
            else
            {
                inscan.StatusType = db.tblStatusTypes.Where(cc => cc.ID == statustypeid).FirstOrDefault().Name;
                inscan.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID == inscan.CourierStatusID).FirstOrDefault().CourierStatus;
            }

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

            if (data.MaterialCost != null)
            {
                inscan.MaterialCost = data.MaterialCost.Value;
            }
            else
            {
                inscan.MaterialCost = 0;
            }

            if (data.OtherCharge != null)
            {
                inscan.OtherCharge = data.OtherCharge.Value;
            }
            else
            {
                inscan.OtherCharge = 0;
            }


            if (data.Weight != null)
            {
                inscan.Weight = data.Weight.Value;
            }
            else
            {
                inscan.Weight = 0;
            }
            if (data.CustomsValue!=null)
            {
                inscan.CustomsValue = data.CustomsValue;
            }
            else
            {
                inscan.CustomsValue = 0;
            }
            if (data.CurrencyID==null)
            {
                inscan.CurrencyID = CommonFunctions.GetDefaultCurrencyId();
            }
            else
            {
                inscan.CurrencyID = data.CurrencyID;
            }
            if (data.ProductTypeID != null)
            {
                inscan.ProductTypeID = Convert.ToInt32(data.ProductTypeID);

            }

            if (data.ParcelTypeID != null)
            {
                inscan.ParcelTypeID = Convert.ToInt32(data.ParcelTypeID);
            }
            

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
            inscan.ConsignorAddress3_PinCode = data.ConsignorAddress3_PinCode;
            inscan.ConsignorContact = data.ConsignorContact;
            inscan.ConsignorMobileNo = data.ConsignorMobileNo;
            inscan.ConsignorPhone = data.ConsignorPhone;
            if (data.CustomerID != null)
            {
                inscan.CustomerID = data.CustomerID;
                var cust = db.CustomerMasters.Find(inscan.CustomerID);
                if (cust != null)
                    inscan.CustomerName = cust.CustomerName;
                else
                    inscan.CustomerName = "Customer Unknown";

            }

            //inscan.TaxconfigurationID = data.TaxconfigurationID.Value;
            inscan.Consignee = data.Consignee;
            inscan.ConsigneeAddress1_Building = data.ConsigneeAddress1_Building;
            inscan.ConsigneeAddress2_Street = data.ConsigneeAddress2_Street;
            inscan.ConsigneeAddress3_PinCode = data.ConsigneeAddress3_PinCode;
            inscan.ConsigneeContact = data.ConsigneeContact;
            inscan.ConsigneeMobileNo = data.ConsigneeMobileNo;
           
            inscan.ConsigneePhone = data.ConsigneePhone;            
         
            
            
            // inscan.Pieces = data.Pieces;
            inscan.ConsignorLocationName = data.ConsignorLocationName;
            
            //inscan.totalCharge = data.BalanceAmt.Value;
            //inscan.materialcost = data.MaterialCost.Value;
            inscan.CargoDescription = data.CargoDescription;
            inscan.ManifestWeight = data.ManifestWeight;
            inscan.SpecialInstructions = data.SpecialInstructions;
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
                StatusModel result = AccountsDAO.CheckDateValidate(v.AWBDate.ToString(), yearid);
                if (result.Status == "PeriodLock") //Period locked
                {
                    return Json(new { status = "Failed", InscanId = 0, message = result.Message });
                }
                else
                {

                }

                InboundShipment inscan = new InboundShipment();

                try
                {



                    if (v.ShipmentID == 0)
                    {


                        inscan.AWBNo = v.AWBNo; // _dao.GetMaAWBNo(companyId, branchid);

                        inscan.BranchID = branchid;

                        inscan.AcFinancialYearID = PickupRequestDAO.GetFinancialYearID(v.AWBDate.ToString(), branchid);
                        inscan.AWBDate = v.AWBDate;

                        inscan.CustomerID = v.CustomerID;


                        if (inscan.StatusTypeId == null)
                            inscan.StatusTypeId = 1;
                        if (inscan.CourierStatusID == null)
                            inscan.CourierStatusID = 4;
                        
                        if (inscan.CreatedBy == null)
                            inscan.CreatedBy = userid;

                        if (inscan.CreatedDate == null)
                            inscan.CreatedDate = CommonFunctions.GetBranchDateTime();

                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetBranchDateTime();
                        inscan.PaymentModeId = 3;

                    }
                    else
                    {
                        inscan = db.InboundShipments.Find(v.ShipmentID);

                        inscan.AWBDate = v.AWBDate;
                        inscan.LastModifiedBy = userid;
                        inscan.LastModifiedDate = CommonFunctions.GetBranchDateTime();
                        inscan.AcFinancialYearID = PickupRequestDAO.GetFinancialYearID(v.AWBDate.ToString(), branchid);
                    }
                    inscan.Weight = v.Weight;
                    inscan.ConsignorCountryName = v.ConsignorCountryName;
                    inscan.ConsignorCityName = v.ConsignorCityName;
                    inscan.ConsignorLocationName = v.ConsignorLocationName;


                    inscan.Consignor = v.Consignor;

                    inscan.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                    inscan.ConsignorAddress2_Street = v.ConsignorAddress2_Street;
                    inscan.ConsignorAddress3_PinCode = v.ConsignorAddress3_PinCode;
                    inscan.ConsignorContact = v.ConsignorContact;
                    inscan.ConsignorPhone = v.ConsignorPhone;
                    inscan.ConsignorMobileNo= v.ConsignorMobileNo;


                    if (v.Weight != null)
                    {
                        inscan.Weight = Convert.ToDecimal(v.Weight);
                    }

                    inscan.PaymentModeId = v.PaymentModeId;


                    inscan.Consignee = v.Consignee;
                    inscan.ConsigneeAddress1_Building = v.ConsigneeAddress1_Building;
                    inscan.ConsigneeAddress2_Street = v.ConsigneeAddress2_Street;
                    inscan.ConsigneeAddress3_PinCode = v.ConsigneeAddress3_PinCode;
                    inscan.ConsigneeContact = v.ConsigneeContact;
                    inscan.ConsigneePhone = v.ConsigneePhone;
                    inscan.ConsigneeMobileNo = v.ConsigneeMobileNo;
                    inscan.ConsigneeCityName = v.ConsigneeCityName;
                    inscan.ConsigneeCountryName = v.ConsigneeCountryName;
                    inscan.ConsigneeLocationName = v.ConsigneeLocationName;
                    inscan.Pieces = v.Pieces;

                    inscan.ProductTypeID = v.ProductTypeID;
                    inscan.ParcelTypeID = v.ParcelTypeID;
                    inscan.CourierCharge = v.CourierCharge;
                    inscan.TaxPercent = v.TaxPercent;
                    inscan.TaxAmount = v.TaxAmount;
                    inscan.SurchargePercent = v.SurchargePercent;
                    inscan.SurchargeAmount = v.SurchargeAmount;
                    inscan.NetTotal = v.NetTotal;
                    inscan.MovementID = v.MovementID;
                    inscan.OtherCharge = v.OtherCharge;
                    inscan.CargoDescription = v.CargoDescription;
                    inscan.CustomsValue = v.CustomsValue;
                    inscan.CurrencyID = v.CurrencyID;
                    inscan.Remarks = v.Remarks;
                    inscan.SpecialInstructions = v.SpecialInstructions;

                    if (v.MaterialCost != null)
                    {
                        inscan.MaterialCost = Convert.ToDecimal(v.MaterialCost);
                    }
                    else
                    {
                        inscan.MaterialCost = 0;
                    }

                    if (v.ShipmentID == 0)
                    {
                        inscan.EntrySource = "SNL"; //AWB create by create shipment page

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
                    InboundShipmentDAO.ShipmentAWBPosting(inscan.ShipmentID);
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
        [HttpGet]
        public JsonResult GetShipperName(string term)
        {

            

            if (term.Trim() != "")
            {
                var shipperlist = (from c1 in db.InboundShipments
                                   where c1.Consignor.ToLower().Contains(term.Trim().ToLower())                                   
                                   orderby c1.Consignor ascending
                                   select new Consignor { ShipperName = c1.Consignor, ContactPerson=c1.ConsignorContact, Phone = c1.ConsignorPhone, ConsignorMobileNo = c1.ConsignorMobileNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street,PinCode=c1.ConsignorAddress3_PinCode}).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                return Json(shipperlist, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var shipperlist = (from c1 in db.InboundShipments                                   
                                   orderby c1.Consignor ascending
                                   select new Consignor { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, ConsignorMobileNo=c1.ConsignorMobileNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building,Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                return Json(shipperlist, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet]
        public JsonResult GetConsigneeName(string term, string Shipper = "",bool ShowAll = false)
        {

           

            if (term.Trim() != "")
            {
                if (ShowAll == false)
                {
                    var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignee.ToLower().Contains(term.Trim().ToLower())
                                       && c1.Consignor==Shipper
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ContactPerson = c1.ConsigneeContact, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, ConsignorMobileNo = c1.ConsigneeMobileNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building , Address2 = c1.ConsigneeAddress2_Street,PinCode=c1.ConsigneeAddress3_PinCode}).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignee.ToLower().Contains(term.Trim().ToLower())

                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ContactPerson=c1.ConsigneeContact, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, ConsignorMobileNo=c1.ConsigneeMobileNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building ,Address2=c1.ConsigneeAddress2_Street,PinCode=c1.ConsigneeAddress3_PinCode }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ShowAll == false)
                {

                    var shipperlist = (from c1 in db.InboundShipments
                                       where c1.Consignor==Shipper
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ContactPerson = c1.ConsigneeContact, ConsignorName = c1.Consignee, Phone = c1.ConsigneePhone, ConsignorMobileNo = c1.ConsigneeMobileNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building , Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.InboundShipments
                                       orderby c1.Consignee ascending
                                       select new Consignor { ShipperName = c1.Consignor, ContactPerson = c1.ConsigneeContact, ConsignorName = c1.Consignee, Phone = c1.ConsignorPhone, ConsignorMobileNo = c1.ConsigneeMobileNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode }).Distinct().OrderBy(cc => cc.ShipperName).Take(20).ToList();

                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            
            if (term.Trim() != "")
            {
                
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive == true && c1.CustomerID > 0 && (c1.CustomerType == "CL") && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                 
            }
            else
            {
                
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CL" )
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);                
                
            }




        }

        [HttpPost]
        public JsonResult GetCustomerDataByName(string CustomerName)
        {
            CustmorVM objCust = new CustmorVM();
            var cust = (from c in db.CustomerMasters where c.CustomerType=="CL" & c.CustomerName == CustomerName select c).FirstOrDefault();

            if (cust != null)
            {

                objCust.CustomerID = cust.CustomerID;                
                objCust.CustomerName = cust.CustomerName;
                objCust.ContactPerson = cust.ContactPerson;
                objCust.Address1 = cust.Address1;
                objCust.Address2 = cust.Address2;
                objCust.Address3 = cust.Address3;
                objCust.Phone = cust.Phone;
                objCust.CountryName = cust.CountryName;
                objCust.CityName = cust.CityName;
                objCust.LocationName = cust.LocationName;                
                objCust.CustomerType = cust.CustomerType;
                objCust.Email = cust.Email;
                if (cust.Mobile == null)
                    objCust.Mobile = "";
                else
                    objCust.Mobile = cust.Mobile;
            }
            return Json(objCust, JsonRequestBehavior.AllowGet);
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
                AccountsReportsDAO.GenerateInboundAWBReport(id);
            }

            ViewBag.ReportName = "AWB Print";
            return View();

        }
        #region "Batchshipment"
        public ActionResult BatchIndex()
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            AWBBatchSearch model = new AWBBatchSearch();
            
            var obj = (AWBBatchSearch)Session["COBatchShipmentSearch"];
            
            AWBDAO _dao = new AWBDAO();
            if (obj != null && obj.FromDate != null && !obj.FromDate.ToString().Contains("0001"))
            {
                List<AWBBatchList> translist = new List<AWBBatchList>();
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo; 
                Session["COBatchShipmentSearch"] = model;
                translist = InboundShipmentDAO.GetAWBBatchList(BranchID, FyearId, model,0);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["COBatchShipmentSearch"] = model;
                List<AWBBatchList> translist = new List<AWBBatchList>();
                translist = InboundShipmentDAO.GetAWBBatchList(BranchID, FyearId, model,0);
                model.Details = translist;

            }
            return View(model);
        }
        [HttpPost]
        public ActionResult BatchIndex(AWBBatchSearch obj)
        {
            Session["COBatchShipmentSearch"] = obj;
            return RedirectToAction("BatchIndex");
        }
        public ActionResult BatchCreate(int id = 0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            string _USERTYPE = Session["UserType"].ToString();

            ViewBag.FAgent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3 || cc.SupplierTypeID == 4).ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();


            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
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
                ViewBag.Title = "(Co - Loader) Create";
                v.FlightDate = CommonFunctions.GetCurrentDateTime();
                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());


                StatusModel result = AccountsDAO.CheckDateValidate(v.BatchDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
                v.BATCHID = 0;
                v.BatchDate = CommonFunctions.GetCurrentDateTime();
                v.BatchNumber = InboundShipmentDAO.GetMaxBathcNo(v.BatchDate, branchid, yearid);
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                v.Details = new List<InboundShipmentModel>();
                v.DefaultCurrencyId = CommonFunctions.GetDefaultCurrencyId();
            }
            else
            {
                InboundAWBBatch batch = db.InboundAWBBatches.Find(id);
                v.BATCHID = batch.ID;
                v.BatchDate = batch.BatchDate;
                v.BatchNumber = batch.BatchNumber;
                v.TotalAWB = Convert.ToInt32(batch.TotalAWB);
                v.MAWB = batch.MAWB;
                if (batch.Bags!=null)
                   v.Bags = Convert.ToInt32(batch.Bags);
                if (batch.ParcelNo!=null)
                  v.ParcelNo = Convert.ToInt32(batch.ParcelNo);

                v.RunNo = batch.RunNo;
                v.Remarks = batch.Remarks;
                if (batch.FlightDate!=null)
                   v.FlightDate = Convert.ToDateTime(batch.FlightDate);

                v.FlightNo = batch.FlightNo;
                v.OriginAirportCity = batch.OriginAirportCity;
                v.DestinationAirportCity = batch.DestinationAirportCity;

                if (batch.CourierStatusID!=null)
                    v.CourierStatusID = Convert.ToInt32(batch.CourierStatusID);
                
                   customername = db.CustomerMasters.Find(batch.CustomerID).CustomerName;
                    v.CustomerID = batch.CustomerID ;
                    v.CustomerName = customername;
                
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
                ViewBag.Title = "(Co - Loader) Modify";
                StatusModel result = AccountsDAO.CheckDateValidate(v.BatchDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
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
        public JsonResult ImportFile(HttpPostedFileBase importFile,int CustomerID)
        {
            var pcustomerid = CustomerID;
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<InboundShipmentModel> fileData = GetDataFromCSVFile(importFile.InputStream, pcustomerid);
                InboundAWBBatchModel vm = new InboundAWBBatchModel();
                vm.Details = fileData;
                vm.Details = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
                Session["ShipmentList"] = vm.Details;

                return Json(new { Status = 1, dataCount = fileData.Count, Message = "File Imported Successfully " });
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
                        //DataView dv = dataSet.Tables[0].DefaultView;
                        //dv.RowFilter = "AWBNo==''";
                        //DataSet ds1 = dv.DataViewManager.DataSet;

                        var dataTable = dataSet.Tables[0];
                        string xml = dataSet.GetXml();
                        details = InboundShipmentDAO.GetShipmentValidAWBDetails(branchid, xml, "", CustomerId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }




            return details;
        }

        [HttpPost]
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
                    batch.CreatedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    db.InboundAWBBatches.Add(batch);
                    db.SaveChanges();
                }
                else
                {
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;
                    batch = db.InboundAWBBatches.Find(BatchID);
                    db.SaveChanges();
                }
                SaveStatusModel model = new SaveStatusModel();
                model = InboundShipmentDAO.DeleteAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml);
                if (model.Status == "Ok")
                {
                    return Json(new { Status = "OK", BatchID = batch.ID, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
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
        public JsonResult SaveBatchAWB(InboundAWBBatch vm, string Details, string DeleteDetails)
        {
            string callapipost = "false";
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            InboundAWBBatch batch = new InboundAWBBatch();
            //Details = "[{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095672','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG HUAZHE TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NSK BUILDING MATERIALS TRADING LLC','ConsigneeContact':'ABDUL RUB','ConsigneePhone':'042294144','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 39912  DUBAI AL SHABIB BUILDINGOFFICE NUMBER 101 1ST FLOOR   OPPOSITE DEIRA PARK HOTEL  AL NAKEEL AREA  DIERA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.58','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095671','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG HUAZHE TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PERFECT BUILDING MATERIAL TRADING  LLC','ConsigneeContact':'ABIZAR','ConsigneePhone':' 0559723843','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI UAE RAS AL KHOR  ROD AWER','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.18','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095670','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FAR EAST TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SANDEEP SINGH RANA','ConsigneeContact':'SANDEEP SINGH RANA','ConsigneePhone':'971503182959','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROONI BUILDING2ND FLOOR OFFICE NO 20283 STREET AL RAS DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'WOVEN FABRICS','Pieces':'1','Weight':'14.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'91','CurrencyID':'2','Currency':'USD','CustomsValue':'97.50','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095633','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU YUCHANG TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneeContact':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneePhone':'971 52 6880086','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHIP#08 AL MULLA DUILDINGOPP TARA HOTEL DEIRA DUBAI-U.A.EE-MAIL 634839924 QQ.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PLASTIC SHEET FREE SAMPLES','Pieces':'2','Weight':'33.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'93-94','CurrencyID':'2','Currency':'USD','CustomsValue':'36.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095598','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAI ZHOU AIBIXI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NAC HOME BUILDING MATERIALS LLC','ConsigneeContact':'SIRAJ','ConsigneePhone':'971527386164','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 301 INDIGO OPTIMAINTERNATIONAL CITY DUBAI U. A. E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'GLASS MOSAIC SAMPLE','Pieces':'1','Weight':'7.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'92','CurrencyID':'2','Currency':'USD','CustomsValue':'1.98','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'333000039','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'DISSTON INDUSTRIAL SALES  INC','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TRANSWILL TRADING LLC','ConsigneeContact':'BINDIA','ConsigneePhone':' 971 56 604 6509 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO 597-608  18TH STREETP.O BOX 231725 DUBAI INVESTMENT PARK 2','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'STEEL WIRE ROPE','Pieces':'1','Weight':'0.86','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095599','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XINXIANG CITY ZUOBINGHAN FILTER EQUIPMENT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PROTOSS ME LLC','ConsigneeContact':'MR. BI JU','ConsigneePhone':' 971 56 4330955','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROUNI BUILDING SHOP NO 9AL QUSAIS   INDU AREA   5 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'602925095','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NANTONG FENGSHENG TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FAROUGH ABDULRAHIM ZIYAEI GENTRAL TRADING','ConsigneeContact':'JERRILYU ARELLANO','ConsigneePhone':'97143530808','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 42818 OFFICE 507 NEW ENBB BUILDINGBANIYAS ROAD DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095668','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HANGJIE FAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DMK CO TRADING L.L.C','ConsigneeContact':'DMK CO TRADING L.L.C','ConsigneePhone':'971508874633 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX  119097MALIK NASAR HUSAIN BUILDINGSHOP   20  NEAR SABKHA  DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'251031831','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JASON MARKETING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'WADI AL WAFA TEXTILES & LADIES TAILOR','ConsigneeContact':'KALLEM','ConsigneePhone':'00971501127594','ConsigneeMobileNo':'','ConsigneeCountryName':'AL AIN CITY(AE)','ConsigneeCityName':'AL AIN CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHUMALL MARKET NEAR JEBEL ROUND ABOUT','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'HOTFIX STONE','Pieces':'2','Weight':'22.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'89','CurrencyID':'2','Currency':'USD','CustomsValue':'99.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095625','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANDONG AURORA METAL CYLINDER CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TSSC KITCHEN & LAUNDRY EQUIPMENT TRADING LLC','ConsigneeContact':'MOHIDEEN ABDUL KHADE','ConsigneePhone':' 971 4343 1100','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO. 1  SHEIKH MAJID BLDG  SHEIKH  ZAYED ROAD  P.O. BOX 69  DUBAI  UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095619','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO SHIDA LVYE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'EISA MOHAMMAD','ConsigneeContact':'EISA MOHAMMAD','ConsigneePhone':'00971525121308','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI DEIRA ALMURAR AREA ALSUWAIDI BUILDINGOFFICE NO207','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095579','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HEFEI EVA RUBBER MANUFACTURER CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL FAYADH PRINTING PRESS L L C','ConsigneeContact':'MR MUGHEE SAHMADK HAN','ConsigneePhone':'00971507290056','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 28624 SHARJAH SHARJAH  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'3486168114','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LIU CAI KUI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROYALLINE TRADING PTE LTD','ConsigneeContact':'SHIVAM MENARIA','ConsigneePhone':'971 568649101','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2907-06 & 2901  JBC 3  CLUSTER Y  JLT DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.64','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338017','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOSSY GENERAL TRADING LLC','ConsigneeContact':'GLOSSY GENERAL TRADING LLC','ConsigneePhone':'042268281','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL RAS -DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095628','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JIANG YIN ZHONGNAN HEAVY INDUSTRIES CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CHINA PETROLEUM PIPELINE ENGINEERING COMPANY LIMITED','ConsigneeContact':'JERECA PEARL DELFIN','ConsigneePhone':'971 50 968 2603','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'49TH FLOOR ADDAX OFFICE TOWER AL RAYFAH STAL REEM ISLAND - CITY OF LIGHTS','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'16.66','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'87','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338014','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL KABYEL DISCOUNT CENTER','ConsigneeContact':'ABU DULLAH','ConsigneePhone':'00971506284757','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ABU BAKER AL SIQIQ ROAD','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.21','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666776','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAJID. AHMED. KHAN.','ConsigneeContact':'SAJID. AHMED. KHAN.','ConsigneePhone':'0552855009','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'M.17 MUSSAFAH ABUDHABI.   UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.25','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666765','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'XIAOBING ZHENG','ConsigneeContact':'XIAOBING ZHENG','ConsigneePhone':'00971 55 7068999','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SOMALIA BUILDING 207 SHOP ANTLER UPSTAIRSDOWNTOWN HOTEL OPPOSITEDEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.92','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388337989','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOBAL CONNECTION IMPEX TRADING LLC','ConsigneeContact':'DIANA MONTEIRO','ConsigneePhone':'971 56 116 8480','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.5 ABDUL AZIZ AL MAJID BIDG. P.O.BOX32031 DEIRA DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'HEATING TUBE','Pieces':'1','Weight':'5.77','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'86','CurrencyID':'2','Currency':'USD','CustomsValue':'18.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407899','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S MODERNTEX TEXTILE','ConsigneeContact':'MR RAJESH','ConsigneePhone':'04-3542150','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'#105 TEXMAS BLDG P.O.BOX 43476 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'TEXTILE CUTTING SAMPLE NCV','Pieces':'1','Weight':'3.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407901','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CHARMAX TEXTILES TRADING LLC','ConsigneeContact':'MR LEE CHARMAX TEXTILES','ConsigneePhone':'043534544','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUR DUBAI OFF 102 PLOT#312-1309 P.O.BOX 6926 ALSUQ ALKABEER','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'TEXTILE CUTTING SAMPLE NCV','Pieces':'1','Weight':'5.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'603363709','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FUZE TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MADEEN TRADING CO','ConsigneeContact':'BUNTY BUNTY','ConsigneePhone':'00971502557760','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL DIWAN CENTER 21   SHOP 3/5  DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'COTTON FABRIC HANGER SAMPLES','Pieces':'1','Weight':'3.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095570','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FUZE TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROZI KHAN TEXTILE CO. LLC','ConsigneeContact':'ROZI KHAN','ConsigneePhone':'0551077869','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 46529 BEHIND NAIF POLZCE STATION DERIA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'COTTON FABRIC HANGER SAMPLES','Pieces':'1','Weight':'0.45','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095576','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FIRST SPORT LLC DUBAI UAE','ConsigneeContact':'FIRST SPORT LLC DUBAI UAE','ConsigneePhone':'042351430','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.1 MURSHID BAIAT DEIRA DUBAIP.O.BOX 47877 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'YY25010000021','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUEYANG INTERNATIONAL TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ZAKARYA LABIOD','ConsigneeContact':'ZAKARYA LABIOD','ConsigneePhone':'971-524221873','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A  AL JAWHARA BUILDING  BANK STREET -OFFICE 708 - BUR DUBAI - DUBAIZAKARYA.L VERGER-GROUP.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PAPER LUNCH BOX','Pieces':'1','Weight':'1.38','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095643','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG HUZHOU SENFUJIDIAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASER ASADI TRADING LLC','ConsigneeContact':'NASER ASADI TRADING LLC','ConsigneePhone':'00971569931111','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO-3 NEAR NBF BANK AL RAS ON AL RAS METRO STATION AL RAS DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'ELECTRIC DOOR REMOTE CONTROL','Pieces':'2','Weight':'17.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'88','CurrencyID':'2','Currency':'USD','CustomsValue':'24.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'Y2501030190','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG BOTOLINI MACHINERY CO  LT','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SABA HYDRAULIC PUMPS TRADING LLC','ConsigneeContact':'SAJI VARUGHESE','ConsigneePhone':'971527513456','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'INDUSTRIAL AREA-3 CATERPILLAR ROAD SHARJAHUAE PO BOX.97324','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'ALUMINUM CHAIN ROD','Pieces':'1','Weight':'0.91','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095695','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING JUNCHI  TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL RAFIHA TRADING L.L.C','ConsigneeContact':'AL RAFIHA TRADING L.L.C','ConsigneePhone':'043444735','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HAMMAD BUILDING 4 FIRST FLR OFFICE NO 104BUR-DUBAI U.A.E P.O BOX 43703','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095693','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JI MING GANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FOR NURTUURE GOODS COURIER','ConsigneeContact':'FOR NURTUURE GOODS COURIER','ConsigneePhone':'0567622841','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL REEM TOWERS 1807 RIGGAT AL BUTEEM NEXT TO ETISALAT OFFICE NEAR UNION METRO STATION DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095691','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUANGSHOU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GOLDEN OSKAR TEXTILES','ConsigneeContact':'MR GOPAL GOPAL','ConsigneePhone':'00971504694258','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NEAR SONA TEXTILE GWADIRI  MARKET SABKA BUS STATION  DEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES OF N C V','Pieces':'1','Weight':'10.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102','CurrencyID':'2','Currency':'USD','CustomsValue':'13.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'8522177786','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HENGBANG IMPORT EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL BAZ GENERAL TRADING LLC RULER BLDG','ConsigneeContact':'MR HARSAN HARSAN','ConsigneePhone':'97143536459','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BLDG NO R295 WASL PROPERTY  OFFICE NO 201 2ND FLOOR  AREA AL SOUK AL KABEER OPP BANK OF BARODA P O BOX 111148 BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES OF N C V','Pieces':'1','Weight':'25.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'1','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'510432273','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YANG BO','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'LATATEX INTERNATIONAL TRADING LLC','ConsigneeContact':'MR MANISH SAWLANI','ConsigneePhone':'3530190','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 47293 OFF#201 ESSA SALEH AL GURGBUILDING OPP BANK OF BARODA WHOLESALETEXTILE MARKET BUR-DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES','Pieces':'1','Weight':'2.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'86575613887','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'X Q G','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CITY VIEW TEXTILES LLC','ConsigneeContact':'MR ADITYA','ConsigneePhone':'971527781122','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHD AHMED KABITAL BLDG AL SOUK AL KABIRAL FAHIDI STREET BEHIND SUNCITY HOTEL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES','Pieces':'1','Weight':'1.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095608','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG SUNTE TECHNOLOGY CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DYNATRADE AUTOMOTIVE L.L.C','ConsigneeContact':'FEMIL JAMES','ConsigneePhone':' 971 50 938 1231','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 22842 INDUSTRIAL AREA-17 SHARJAH- UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095581','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO HOMEBEST E COMMERCE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RR MIDDLE EAST FZCO','ConsigneeContact':'PAVAN KUMAR ALWANI','ConsigneePhone':'971 50 439 5240','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2903  2904  2905 & 2906  IRIS BAY AL MUSTAQBAL STREET  BUSINESS BAY P. O. BOX  31680  DUBAI  U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'LED DOWNLIGHT LED','Pieces':'1','Weight':'1.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095564','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LOTUS FANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAXIMAN TRADING CO .LLC','ConsigneeContact':'MS CAMILLE','ConsigneePhone':' 971 501487028','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI  DEIRA AREA  AL RAS MARKET. BESIDE  HOUSE MART P.O BOX  378917','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095555','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'WUXI BRIGHTSKY ELECTRONIC CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SFFECO GLOBAL FZE','ConsigneeContact':'MR. WILLIAM MS. MARY','ConsigneePhone':'009714-8809890','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO. S10833 & S10903 PO BOX 261318 JEBEL ALI FREE ZONE  SOUTH  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'8626618976','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'PRIME FASHION','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S VIP CHOICE FASHIONS LLC','ConsigneeContact':'MR SUNIL','ConsigneePhone':'9714 3538614','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMED BARAKAT BUILDING 3RD FLOOR 3RD FLOOR301 P O BOX 12520 DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'1.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095537','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUOLE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LIMITED','ConsigneeContact':'BEAUTY OR ANGELICA','ConsigneePhone':'971 45175974','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'LANDMARK GROUP-MAIN TOWER MAX FASHIONGROUND FLOOR JAFZA ONE JEBEL ALI P.O BOX113630 DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095527','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HUIMO TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SUMERU INDUSTRIES FZE','ConsigneeContact':'ANDND LOKHANDLWALA','ConsigneePhone':'00 971 55 378 4557','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH FREE ZONE(AE)','ConsigneeCityName':'SHARJAH FREE ZONE(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A2-075 SHARIJAH AIRPORT    FREE ZONE U.A.E PO   BOX#120992','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES OF N C V','Pieces':'1','Weight':'2.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095519','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'AM OL SHINDE BUYER LEECOOPER','ConsigneePhone':'04-809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDM ARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI MOB 050.7845686','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'WOMEN'S SHIRT SAMPLE N C V','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607 - 37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA - DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095518','AWBDate':'01 / 04 / 2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'KHURRAM','ConsigneePhone':'04 - 809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDMARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH - JAN - 2025 SHA - DXB EY DXB GREEN 607 - 37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'WOMEN'S DRESS SAMPLES N C V','Pieces':'1','Weight':'1.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'3','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095502','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'MINGBANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GROUP OF COMPANLES','ConsigneeContact':'MOHAMMAD BILLAL','ConsigneePhone':'971553830226','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'R16 SHOP NO 10 INTERNATIONAL CITYDUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES OF N C V','Pieces':'1','Weight':'0.44','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095497','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING WEI EN IMP EXP  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TOP CITY TEXTILE TRADING LLC','ConsigneeContact':'TOPCITYTEX','ConsigneePhone':'00971555172691','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SABKHA BUILDING SHOP NO 22-24 NEAR  SABKHA BUS STATIONDEIRA DUBAI-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER TEXTILE SAMPLE FABRIC OF N C V','Pieces':'1','Weight':'3.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095551','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU YAOWANG TEXTILE  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MUKESH TEXTDLIUM','ConsigneeContact':'MIKE','ConsigneePhone':'3534349','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HISTOPICA BUILDING SHOP NO 6 BUR DUBAI U A E NEAR DUBAI MUSEUM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC SAMPLE OF N C V','Pieces':'1','Weight':'1.6','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095514','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YOU COME EPX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BHAGAT TRADING CO LLC','ConsigneeContact':'GU RIND','ConsigneePhone':'00971552427355','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 4 7 ABDULLA ABDUL RASOOL  BUILDING WHOLESALE TEXTILE MARKET   BUR DUBAI NEAR DUBAI MUSEUM PO BOX 51617','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'6.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'5','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095503','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HEYI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL NADER GENTS TAILORING L.L .C.','ConsigneeContact':'AMIN HOSSIAN','ConsigneePhone':'97167314749','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 7660 NEW INDUSTRIAL AREAAJMAN-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'1.49','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095684','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'E SUN INVESTMENT GROUP LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MR NASER ABDULAZEEZ AHMED','ConsigneeContact':'MR NASER ABDULAZEEZ AHMED','ConsigneePhone':'97126454444','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROX SHOWROOM JOUD HOTEL BUILDING NO.97AL MAKTOUM RD AL KHABAISIDUBAI DEIRA AL ITTIHAD STREET','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'AUTO SENSOR AND SPARE TIRE COVER AND ZINC TOY','Pieces':'2','Weight':'12.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'29-30','CurrencyID':'2','Currency':'USD','CustomsValue':'35.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{ 'ShipmentID':'0','CustomerID':'66765','AWBNo':'LYW250103AE','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GHAZI ADEL','ConsigneeContact':'GHAZI ADEL','ConsigneePhone':'9711565444443','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'8 AL ASHRI&#39 AH STREET ALAIN ALTOWAYAJAFFER STREET NUMBER 8 HOME 5AL AIN CITYABU DHABI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'EXHAUST PIPE','Pieces':'2','Weight':'43','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'27-28','CurrencyID':'2','Currency':'USD','CustomsValue':'400.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'}]";
            //Details = "[{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095685','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHENZHEN KUANSHANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SICILIA GENERAL TRADING LLC','ConsigneeContact':'TINA LEE','ConsigneePhone':'0097145546240','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 1309  SAHEEL TOWER 2AL NAHAD  AL ITHATT ROADPO BOX   7846','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'BELLING FOR SAMPLE AND KEY SWITCH','Pieces':'1','Weight':'4.7','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'62','CurrencyID':'2','Currency':'USD','CustomsValue':'100.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095678','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'CHANGZHOU XINGYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASEEM','ConsigneeContact':'NASEEM','ConsigneePhone':'971563506755','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROOM 219  BUILDING YASMEEN 4 SHARJAH NEAR CRICKET STADIUM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'ANGLE VALVE','Pieces':'1','Weight':'1.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'62','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095673','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'QINGDAO SHUNFA HAIR CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAMER MOHAMED','ConsigneeContact':'SAMER MOHAMED','ConsigneePhone':' 971504137591','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI - TILAL ALGHAF ELAN 2 VILLA NUMBER B498','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'SYNTHETIC FIBER HAIR','Pieces':'1','Weight':'0.21','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095669','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI WISDOM IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HAIDAROUS TRADING','ConsigneeContact':'MR SUNIL','ConsigneePhone':'0097142254034','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'305 EMIRATES BANK BUILDING   OPP BIG ABRA BANIYAS   ROAD DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'TROUSERS FOR SAMPLE','Pieces':'1','Weight':'1.29','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095636','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LINYI KAIKAI NAIL CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ABBAS BIN HAIDER GENARAL TRADING L.L.C','ConsigneeContact':'JUZER QADER BHAI','ConsigneePhone':'00971552377531','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NASSER SQUARE ROAD  ALBUDOOR BUILDING  ALROSTAMANI EXCHANGE SAME BUILDING  3RD FLOORROOM NO  13  DEIRA  DUBAI  U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095610','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HAN XIUCUI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'IMANCO SANITARY WARE TRD. CO. LLC','ConsigneeContact':'AHMAD TALAL AHMAD ABU ALI','ConsigneePhone':'971 6 543 2664','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UNITED ARAB EMIRATES    SHARJAH P.O.BOX 6282   INDUSTRIAL AREA 6','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.22','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'COSRA734411525','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'RUIAN WEI YE CLOTHING CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALZAYADI TRADING CO. LLC','ConsigneeContact':'WEIWEN JI','ConsigneePhone':'0097142266647','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OPP.MASHREQ BANK NEAR AL ORUBA HOTEL MUR SHIDBAZAR','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.12','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'YY25010000088','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUEYANG INTERNATIONAL TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALI BAKRI','ConsigneeContact':'ALI BAKRI','ConsigneePhone':'00971581982500','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'VILLA 23 - AL HARRAS STREET - REYAD CITY - ABU DHABI - UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'STOCK WALLET','Pieces':'1','Weight':'0.26','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095642','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU KUROMA FLUID CONTROL CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TAPS AND MORE TRADING LLC','ConsigneeContact':'GEORGES GEORGES','ConsigneePhone':'0097150 6452460','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUILDING MATERIALS MARKET  WARSAN3  OS-42 & OS-43 DUBAI  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'BRASS ANGLE VALVE','Pieces':'1','Weight':'1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095620','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SALSABEEL AL WAHA TRADING CO LLC','ConsigneeContact':'SALSABEEL AL WAHA TRADING CO LLC','ConsigneePhone':'00971503070893','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 185621 NAIF ROADNEAR KHALID MASJIDDUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.13','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338009','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHUJI YOUXUAN TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAHE BUILDING MATERIALS LLC','ConsigneeContact':'SHAHANOOM','ConsigneePhone':'00971552429996','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HORLANZ DEIRA PO 83599 DUBAI U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095600','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'JINHAI HUANG','ConsigneeContact':'JINHAI HUANG','ConsigneePhone':'0971 582384166','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'210SQ.FT. 1 DOOR SHOP G9 IN AL DIWAN CENTER P8 312-250','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC SAMPLE COLOR CARD','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'42.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388337985','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHUJI YOUXUAN TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ARISTON BUILDING MATERIALS TRADING','ConsigneeContact':'AHMED ABDUL LATIF','ConsigneePhone':'0508477470','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NASAR SQUARE MURSHID BAZAR DEIRA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'251031024','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'RONGJIAO WU','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MODERN HOUSE TRADING LLC.','ConsigneeContact':'MURSHID BAZAR','ConsigneePhone':'042256324','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUILDING 2ND FLOOR OFFICE NO.201ANDMARK-BESIDE REMAS HOTEL PO.BOX 4614','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.19','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095631','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI QUANFU INTERNATIONAL TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FAQIHI AND SONS GENERAL TRADING LLC','ConsigneeContact':'MOHAMMAD FAQIHI','ConsigneePhone':'00971 42502772','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NAIF ROAD BEHING AL MANAL GENTER SHIO NO 2 P O BOX 186584','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095624','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YIWU RIBOSON SUPPLY MANAGEMENT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOBAL PACKAGING FZE','ConsigneeContact':'AMIT  BOSS','ConsigneePhone':'00971 6 5572143','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'Q3-096  SAIF ZONE P.O BOX  8656SHARJAH AIRPORT INTERNATIONAL FREE ZONE  SHARJAH U.A.E. TEL   00971 6 5526313','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PLASTIC CAP','Pieces':'1','Weight':'2.35','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.67','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095611','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHENZHEN DING SHENG BO TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AWJ INVESTMENTS L.L.C.','ConsigneeContact':'ELENA MUNTEANU','ConsigneePhone':' 971526561094','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI MARINA(AE)','ConsigneeCityName':'DUBAI MARINA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'1204 MARINA PLAZA DUBAI MARINA PO BOX 58523DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'EMBROIDERY CRAFTS','Pieces':'1','Weight':'0.58','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095604','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LINYI HUAFU','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'C O WOODWORTH INTERNATIONAL FZE.','ConsigneeContact':'MOHAMMED ARIF KHATRI','ConsigneePhone':'0097154-4245896','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI HILLS ESTATE  SIDRA 3  VILLA 101OFF UMM SUQUEIM STREETNEAR KINGS COLLEGE LONDON HOSPITAL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095603','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHENZHEN DING SHENG BO TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAGHANMAL JETHANAND','ConsigneeContact':'PRASANNA AND YOGESH','ConsigneePhone':'00971554703840','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO 401 4TH FLOOR ALTHURAYA TOWERS OPP DUBAI MUSUEM ALFAHIDI STREET BURDUBAI DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'89505895','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG WINKING ABRASIVES COLTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'VOLGA INTERNATIONAL LLC','ConsigneeContact':'AJI ABRAHAM','ConsigneePhone':'971565052844','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 2102 AL SAJA INDUSTRIAL.AREA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'ZCL41542544','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG GENUINE MACHINE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NEW TEC GENERAL TRADING LLC','ConsigneeContact':'MR.SHEFEEK ABDUL RAHUMAN','ConsigneePhone':'00971562771960','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PB NO 18745 JURF INDUSTRIAL AREA 3 AJMAN UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095578','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'QUANXING MACHINING GROUP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DIESEL TECHNIC  M.E.  FZE','ConsigneeContact':'SANTHOSH JENVI','ConsigneePhone':'971 4 8120 129','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O. BOX 17051   STREET N305 JEBEL ALI FREE ZONE  DUBAI  U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095569','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YIWU DADAO IMPORT AND EXPORT CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MOHAMMED MAHABUB PERFUMES IND L.L.C','ConsigneeContact':'SAIFUL ISLAM','ConsigneePhone':'971-527703926','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH CITY(AE)','ConsigneeCityName':'SHARJAH CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'INDUSTRIAL AREA 15 MALIHA ROAD BACKSIDE OF AL TALAL SUPERMARKET SHED NO  07 & 08','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095565','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU YILAIDE IMPORT   EXPORT TRADING CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GEMCOOL AIR CONDITIONING & REFRIGERATION INDUSTRY LLC','ConsigneeContact':'MOHAN LAL','ConsigneePhone':'054-4443290','ConsigneeMobileNo':'','ConsigneeCountryName':'UMM AL QUWAIN(AE)','ConsigneeCityName':'UMM AL QUWAIN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 4572 BLOCK NO 3 NEWINDUSTRIAL AREA UMM AL QUWAIN - UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.13','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095556','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'AISTTOOLS CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MOFADAL COMMERCIAL INTERMEDIARY','ConsigneeContact':'SHABBIR SAFDARI','ConsigneePhone':'00971 50 4570478','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O. BOX.25274   IST FLOOR  OFFICE NO.102AKHUND AWAZI BUILDING  NAIF AREA   OPPO. CALIFORNIA HOTEL  NAKHEEL  DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.13','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095535','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YIWU PEIJIAN E COMMERCE FIRM','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SEA MAX TRADING LLC','ConsigneeContact':'MR MANSOUR SHOKRI','ConsigneePhone':'0097143419082','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BANIYAS TOWER - A BLOCK - OFFICE  103BANIYAS   DEIRA   DUBAI   UAEEMAIL  SEAMAXTRADING2022 GMAIL.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095732','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'WEIHAI DALEE CARPET CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'JIFCO TRADING LLC','ConsigneeContact':'MR. ALFRED','ConsigneePhone':'971506530848','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WAREHOUSE 20 5B STREETAL QOUZE INDUSTRIAL AREA 1AL QOUZE DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CARPET FOR SAMPLE','Pieces':'1','Weight':'6.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095645','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU YUCHANG TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CONTACT PERSON HANGFANG CHEN','ConsigneeContact':'HANGFANG CHEN','ConsigneePhone':'00971508976860','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DRAGONMART-AL- AWIR DUBAI FAREAST GOLDEN HILL TRADING FZCO GAI-01','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FESTIVAL LIGHTS','Pieces':'1','Weight':'3.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'62','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8500005239','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIUFU INTL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TIM','ConsigneeContact':'TIM','ConsigneePhone':'971526993955','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'VILLA 15 GATE B AL SALAM. MUDON DUBAILANDDUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'THE BOOK AND THE TOY','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'CT807484415CN','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIUFU INTL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PROTECTOL HEALTH','ConsigneeContact':'PROTECTOL HEALTH','ConsigneePhone':'971503928002','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'FISHERMANS COOPERATIVE SOCIETY WAREHOUSE 13 -WARSAN 3','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CAR SUNSHADE','Pieces':'1','Weight':'0.24','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095694','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'M  D L','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MUHAM MAD SAJID TRADING LLC','ConsigneeContact':'MUHAM MAD SAJID','ConsigneePhone':'0097143596117','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFF 307 AL W AKEED BUILDING ROLLASTREET BUR DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095632','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING NEWFUTURE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MOHD YAMIN TRADING LLC','ConsigneeContact':'MR YA MIN','ConsigneePhone':'0097142257166','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 40501 MASHRIQ BANK BLDG 3RD FLOOR FLAT NO 14 MURSHID BAZAR DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'SC ARVES SAMPLES','Pieces':'1','Weight':'1.77','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'4.50','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095666','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHENZHEN DING SHENG BO TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HELIOS BUSINESS SYSTEMS LLC','ConsigneeContact':'CHANDRASEKHAR THAMPI','ConsigneePhone':'971552230132','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WAREHOUSE NO. 8  BEHIND DHLSHARJAH INDUSTRIAL AREA 11SHARJAH  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.21','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095702','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'MIDKOREA CORPORATION','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'EVEREST INDUSTRIAL CO. LTD','ConsigneeContact':'MR.QAISAR','ConsigneePhone':' 971-56-4143580','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 5897','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PIN FOR SELF CLOSING HINGE','Pieces':'1','Weight':'14.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'63','CurrencyID':'2','Currency':'USD','CustomsValue':'360.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'CZ2367622232','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'OUHANG SUPPLY CHAIN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AWAWDEH AUTO SPARE PARTS L .L.C S.P','ConsigneeContact':'AWAWDEH AUTO SPARE PARTS L .L.C S.P','ConsigneePhone':'971 6539 1144','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX  27248 AL YASMEEN ST  STREET # 22   INDUSTRIAL AREA 12SHARJAH  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.12','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8600976369','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHANGJIAGANG COLOR P LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ATRACO INDUSTRIAL ENTERPRISES','ConsigneeContact':'ANEESH','ConsigneePhone':'009714-8812686','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'POST BOX # 16798  PLOT 17  ROAD NO. N 606  ROUND ABOUT NO.5  EBEL ALI FREE ZONE  DUBAI.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EK DXB WHITE 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.08','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'61','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445a','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095681','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING MAKKAH TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'UNU TEXTILE MANUFACTURING LLC','ConsigneeContact':'ABID','ConsigneePhone':'971509928388','ConsigneeMobileNo':'','ConsigneeCountryName':'UMM AL QUWAIN(AE)','ConsigneeCityName':'UMM AL QUWAIN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'103 EXIT COMPLEX WAREHOUSEPLOT NO 1827  SHOWROOM A2UMM AL QUAWAIN','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOCUMENT','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603364674','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI ZHONGDA WINCOME CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ADREES ABDULLA TRADING L.L.C','ConsigneeContact':'M.HAROON SAFI','ConsigneePhone':'00971-4-2352341','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 116230 MURSHID BAZAR  DOWN AL  SHERAA HOTEL DEIRA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095680','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LIU YANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SKY SUN TEXTILES LLC','ConsigneeContact':'MR.HAR ISH','ConsigneePhone':' 9714-2354551 2','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O. BOX 43567 SABAKHA ROAD GWADRI BAZAR DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'2.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095677','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING YING SI PEI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FANCY LADY TAILORING AND FABRICS','ConsigneeContact':'FANCY LADY TAILORING AND FABRICS','ConsigneePhone':' 971561241809','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WEST BANIYASABU DHABIUAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'1.06','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603390723','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QING DA IMP  EXPCO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SACHDEV TRADING CO LLC','ConsigneeContact':'QURBAN ALI','ConsigneePhone':'043529627','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 2 BAQALI BLDG JUMAMASJID  ROAD BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095675','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HANA TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL RAFIHA TRADING LLC','ConsigneeContact':'AL RAFIHA','ConsigneePhone':'043444795','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HAMMAD BUILDING 4 FIRST FLR OFFICE NO 104 BUR DUBAI U A E P O BOX 43703','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095649','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'AMGT','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MATROOSHI GENERAL TRD LLC','ConsigneeContact':'ANGELA','ConsigneePhone':'0097143356992','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SUITE 307 BIN BELAISHA BLDGNEAR TO ADCB BANK  KARAMA  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603312352','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HONG QI TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NOOROLLAH HASEMY TRADING CO LLC','ConsigneeContact':'MR N O R','ConsigneePhone':'9715 55622358','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 241643 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'ARL250104AE','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SUPER SHINE AUTO CARE','ConsigneeContact':'KEVIN BATONGBAKAL','ConsigneePhone':'971502379789','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI INVESTMENTS PARK 2 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CIRCUIT TERMINALS','Pieces':'3','Weight':'30.57','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'98-100','CurrencyID':'2','Currency':'USD','CustomsValue':'418.18','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'SHHC0104104292931','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TWO OCEANS TRADING LLC','ConsigneeContact':'AADIL HARBHAJUN.','ConsigneePhone':'0971504963616','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WAREHOUSE 5. WASL WAREHOUSE COMPLEX. AL QUOZ1 DUBAI UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PLASTIC BOTTLE SAMPLE','Pieces':'1','Weight':'1.32','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'97','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095667','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'PU HAIJUN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALBEX RETAIL DISPLAYS GENERAL TRADING LLC','ConsigneeContact':'PAULINE RETIZA','ConsigneePhone':' 971 501326309','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 801 SOBHA IVORY 2  AL ASAYEL ST P.O.BOX 213959  BUSINESS BAY  DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'GARMENT HANGERS','Pieces':'1','Weight':'0.86','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1110089815193813','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIUFU INTL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAEED SULTAN','ConsigneeContact':'SAEED SULTAN','ConsigneePhone':'971 502198000','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ALAIN-ZHKER ST. ALRUMAILA HOME.12 P.O.BOX24911 AL AIN CITY','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PERMANENT MAGNET GENERATOR AND GEAR MOTOR','Pieces':'1','Weight':'9.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'97','CurrencyID':'2','Currency':'USD','CustomsValue':'100.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603356204','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'DIVYANSHI FASHIONCO LID','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DIVYANSHI FASHION LLC','ConsigneeContact':'MR BINOY BALAKRISHNAN','ConsigneePhone':'0097158 1432897','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROOM NO 403 AL GURG   BUILDING ALSUQ AL KABEER NEAR BANK   OF BARODA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095607','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SAILE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL RAFIHA TRADING LLC','ConsigneeContact':'MAJL AL RAFIHA','ConsigneePhone':'043444795','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HAMMAD BULDING 4FIRST FLR OFFICE NO  104 BUR DUBAI UAE P O BOX 43703','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095605','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SAILE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALI HAJIPOUR TRADING CO LLC','ConsigneeContact':'ALI HAJIPOUR','ConsigneePhone':'043534955','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 1303 DUBAI UAE TRN 100271461400003','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603347767','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING DE YA TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GREEN CASTLE TRADING LLC','ConsigneeContact':'GREEN CASTLE TRADING LLC','ConsigneePhone':'042261875','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 4 MESMAR  BUILDING GWADRI MARKET NEAR  SABKHA BUS STATION DEIRAP O BOX45049','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'25.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'42','CurrencyID':'2','Currency':'USD','CustomsValue':'26.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603347772','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING DE YA TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S AL SUMAITI TRADING CO LLC','ConsigneeContact':'MR VIC KY','ConsigneePhone':'97143532433','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 12657 DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603347768','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING DE YA TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RAJHANS TRADING LLC','ConsigneeContact':'MR CHE TAN','ConsigneePhone':'042262044','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NEAR SABKHA BUS STAND GWADRIMARKET P O BOX 42847DEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095661','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HEJIA IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MANISH KUMAR JAISWANI','ConsigneeContact':'MANISH KUMAR JAISWANI','ConsigneePhone':'0097145472247','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP 12 NAIRA CREATION TRADING NEW SABHKHA  BUILDING GWADRI MARKET SABHKHA DEIRA DUBAI P.O.BOX 191747','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC SWATCHES','Pieces':'1','Weight':'0.6','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095654','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING KEQIAO TWENTY FOUR CHERISH TEXTILE COMPANY LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MODERNTEX TEXTILES TRADING LLC','ConsigneeContact':'MR RAJESH','ConsigneePhone':'97143542150','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'105 AL BAQALI BLDG TEXMAS BLDG  TEXTILE WHOLESALE MARKET-BUR DUBAI P.O.BOX 43476 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'0.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'bc039','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALI AL ALI','ConsigneeContact':'ALI AL ALI','ConsigneePhone':'971559611246','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ABU DHABI AL RAHA BEACH AL DANA AREA AL REEMTOWER FLAT 701','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'JACKET  JACKET','Pieces':'1','Weight':'1.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'15.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'86574077856','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GOLD HOLDING FZC','ConsigneeContact':'SYED JONS','ConsigneePhone':'97142328999','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'28TH FLOOR VZSZON TOWER BUSINESS BAY P O BOX111999','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC AND SCREWS AND LABEL','Pieces':'1','Weight':'5.56','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'96','CurrencyID':'2','Currency':'USD','CustomsValue':'57.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095705','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI MICROBRILL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SHUAI LUO','ConsigneeContact':'SHUAI LUO','ConsigneePhone':'0563007777','ConsigneeMobileNo':'','ConsigneeCountryName':'INTERNATIONAL CITY(AE)','ConsigneeCityName':'INTERNATIONAL CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROOM NUMBER 408 INDIGO OPTIMA BUILDINGINTERNATIONAL CITY DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CRANE PARTS','Pieces':'1','Weight':'1.36','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'50.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095683','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ENDURA FASHION','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FLORANCE TAILORING MATERIAL TRADING','ConsigneeContact':'ISHAK','ConsigneePhone':'971506313152','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL MUJARRAH OPP. DARWESH MASJID SHARJAH','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'SPUN POLYESTER YARN','Pieces':'1','Weight':'2.75','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'97','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095653','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'KOMAL FASHION CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'KOMAL FASHION LLC','ConsigneeContact':'KOMAL FASHION LLC','ConsigneePhone':'97143536092','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NERA ABRA SARDHANA BUILDING SHOP NO 13  SOUQ AL KABEER NEAR JUMA MASJID  P O BOX 51506 BUR DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'2.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'44','CurrencyID':'2','Currency':'USD','CustomsValue':'2.10','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095651','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHENCUI IMP AND EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'XIA FA FASHION LLC EMIRATES','ConsigneeContact':'XIA FA','ConsigneePhone':'0557029808','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WHOLESALE PLAZA DEIRA DUBAI MUSHEDBAZAR OPPSITE FAKGREE CENTER SHOPNO 4 DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER SAMPLE FABRIC','Pieces':'1','Weight':'1.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095650','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHA DU','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SHADOW TRADING LLC','ConsigneeContact':'VIVEK ROMY','ConsigneePhone':'044 267532 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'KIRPALNI WARE HOUSE NO.232  BEHIND SRAGON MARY A WEER     DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'COTTON FABRIC HANGER SAMPLES','Pieces':'1','Weight':'0.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8626945725','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'UDAI TEX TRADING','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'UDAI TEXTILE TRADING LLC','ConsigneeContact':'MR JEETU','ConsigneePhone':'3536015','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 43530  DUBAI UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'44','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'ST20250102','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIUFU INTL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAXOR GENERAL TRADNG LLC','ConsigneeContact':'JUNAID ALI','ConsigneePhone':'00971567749997','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'FLAT# 606 SHEET 13A MEER 2 BUILDING AL WARQA1 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.18','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'QX8800230665','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI QIXIAN TRADING CO','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BLOOM MASTER AUTO SPARE PARTS TRADING L.L.C','ConsigneeContact':'JOYCE FANG','ConsigneePhone':'050 963 1638','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 4670. 7 STATES BUILDING NO.42 DEIRADUBAI.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'9811995023','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI YONG SHANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HENGLI GENERAL TRADING FZE','ConsigneeContact':'HENGLI GENERAL TRADING FZE','ConsigneePhone':'0582769244','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMAD ALISHOP NO D8 051 AJMANCHINA MALL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'9811994964','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI YONG SHANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HENGLI GENERAL TRADING FZE','ConsigneeContact':'HENGLI GENERAL TRADING FZE','ConsigneePhone':'0582769244','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMAD ALISHOP NO D8 051 AJMANCHINA MALL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PVC WALL PANEL','Pieces':'1','Weight':'1.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'2.50','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8626945493','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'UDAI TEX TRADING','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'UDAI TEXTILE TRADING LLC','ConsigneeContact':'MR JEETU','ConsigneePhone':'3536015','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 43530  DUBAI UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'2.95','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095646','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUANGSHOU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NANCY PALACE TEXTILES','ConsigneeContact':'MR MANEESH','ConsigneePhone':'043536720','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUR DUBAI TEXTILES MARKET NEAR ABRA AREA JUMMA MASJID ROAD','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095644','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHINOR FABRIC CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MANOHAR TRADING CO LLC','ConsigneeContact':'NAVEEN','ConsigneePhone':'043529282','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.7 MUMTAZ BUILDING SOUQ AL KABEER NEAR DUBAI MUSEUM DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'3.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095641','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SIDRA TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SIDRA TRADING CO LTD','ConsigneeContact':'ZISHAN MUDASSAR','ConsigneePhone':'0097143536556','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO-5 BARAKAT BUILDING ALI BIN ABI TALIB STREET NEAR DUBAI MUSEUM  WHOLE SALE TEXTILE MARKET 33713','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'7.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'44','CurrencyID':'2','Currency':'USD','CustomsValue':'8.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'JBGJ0104104292773','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HARMONY INTERIORS','ConsigneeContact':'MARY ANN MARGA','ConsigneePhone':'971543699690','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI AB(AE)','ConsigneeCityName':'ABU DHABI AB(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BLDG 532 1ST FLOOR UNIT 103 PO BOX8037SH.KHALIFA STREET','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'NYLON SQUARE CARPET','Pieces':'1','Weight':'4.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'96','CurrencyID':'2','Currency':'USD','CustomsValue':'24.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'96562058','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUYAO HUALUN IMPORT AND EXPORT LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NEW AL AFRAH STORES TRADING LLC','ConsigneeContact':'MR.SHAMSHEER','ConsigneePhone':'0097142264774','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 223 SOUK MURSHID DEIRA DUBAI.U.A.EPH00971508502810','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'SPARE PARTS FOR SEWING MACHINE','Pieces':'1','Weight':'0.31','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'YX250104005','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIUFU INTL TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL QASR AL SHAMI WORKSHOP EQUIPMENT','ConsigneeContact':'SHAMMI','ConsigneePhone':'971 50 420 6636','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP#3 AL SHAFAR BUILDING RAS AL KHOR INDAREA 1 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.12','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095627','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI JIALAN FURNITURE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DESIGN INFINITY LLC','ConsigneeContact':'WRONEX UY','ConsigneePhone':'97143795902 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 303  BLOCK O  HAMSAH B BLDG.  AL KARAMA  DUBAI  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095638','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ANDY CO','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MARYAM MOHD TAQI TEXTILES TRADING LLC','ConsigneeContact':'MARYAM MOHD TAQI TEXTILES TRADING LLC','ConsigneePhone':'0097143530405','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO 4 MEZZANINE FLOOR HORIZON HOTEL ALI BIN ABI TALEB ROAD BUR DUBAI  P O BOX 45411 CODE AE1000879','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095637','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ANDY','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAHAND GENERAL TRADING LLC','ConsigneeContact':'JAFAR REZAEI','ConsigneePhone':'00971502550652','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 46643  DUBAI TEL 3540157  MOB 0553375741 M FAISAL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'HQ250100001921','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QUANYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAY ALMANSOORI','ConsigneeContact':'MAY ALMANSOORI','ConsigneePhone':'971-564826565','ConsigneeMobileNo':'','ConsigneeCountryName':'KHALIFA CITY(AE)','ConsigneeCityName':'KHALIFA CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UAE  KHALIFA CITY- SEC 41  ALBASMI STREET VILLA 15 ABU DHABI UNITED ARAB EMIRATES 0000','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES 60063400','Pieces':'1','Weight':'9.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'43','CurrencyID':'2','Currency':'USD','CustomsValue':'18.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603329696','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAO XING HONGSHAN TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FOCUS GLOBAL COMMERCIAL BROKERS LLC','ConsigneeContact':'MR FAZAL','ConsigneePhone':'0097143539234','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'TEXTILE WHOLESALE MARKET AL KHAYAT BLDG  OFFICE 206 ABOVE AL BADAYER TRADING POBOX 46780 BUR DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER KNITTED FABRIC','Pieces':'1','Weight':'11.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'45','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'SF0265558613864','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG QIMEIDA RUBBER MATERIAL PRODUCTS CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ZAHRH ESTHER HARDWARE TRADING LLC','ConsigneeContact':'AHMED SAAD NEAMATALLAH','ConsigneePhone':'971565254101','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'INDUSTRIAL AREA 7','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOCUMENT','Pieces':'1','Weight':'0.19','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095707','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI HONGNA CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SHIPSPARESINTRRANSIT C OWILHELMSEN','ConsigneeContact':'VENKATESH SUBRAMANI','ConsigneePhone':'971566833541','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'WPS DUBAI PORT SERVICES LLCWAREHOUSE A3 & A4 RAS AL KHORINDUSTRIAL AREA 1PB NO  8612  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'VALVE VALVE AND SEAL RING','Pieces':'1','Weight':'1.95','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'96','CurrencyID':'2','Currency':'USD','CustomsValue':'25.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'CZ467562453','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'CHANGZHOU SUMA PRECISION MACHINERY CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MR.NITHIN JAYARAJ','ConsigneeContact':'MR.NITHIN JAYARAJ','ConsigneePhone':'971504090508','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'412  CBD 8  INTERNATIONAL CITYDUBAI  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CAM FOLLOWER SC 22 SB','Pieces':'1','Weight':'1.48','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'75.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'CZ5676224312','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'OUHANG SUPPLY CHAIN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ANAS KABIR','ConsigneeContact':'ANAS KABIR','ConsigneePhone':'971521922676','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MUWAILEH COMMERCIAL  LILAC 9  VILLA04  AL ZAHIA COMMUNITY  SHARJAH  UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'HIGH PRESSURE LAMINATE SAMPLE','Pieces':'2','Weight':'8.12','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'95','CurrencyID':'2','Currency':'USD','CustomsValue':'16.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603329235','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAO XING HONGSHAN TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'THAMARA GENERAL TRADING L.L.C.','ConsigneeContact':'MR SONG   MR.MOHAMMED','ConsigneePhone':'971-50-6563941','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMAD SALEH AL GURG BUILDING 1ST FLOOR OFFICE NO#103 BESIDES SHIYA MASHJID RTA PARKING BURDUBAI  U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER KNITTED FABRIC','Pieces':'1','Weight':'11.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'46','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'HQ25010000255','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QUANYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALIA ALZAABI','ConsigneeContact':'ALIA ALZAABI','ConsigneePhone':' 971-509571361','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UNITED ARAB EMIRATES  DUBAI  NAD AL SHEBA 2  23A STREET  VILLA 30 DUBAI UNITED ARAB EMIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES 60063400','Pieces':'1','Weight':'0.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'HQ25010000250','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QUANYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NAHIL','ConsigneeContact':'NAHIL','ConsigneePhone':' 971-503338606','ConsigneeMobileNo':'','ConsigneeCountryName':'AL AIN(AE)','ConsigneeCityName':'AL AIN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UNITED ARAB EMIRATES-AL AIN-STREET 14 HOUSE 3AL AIN CITY ABU DHABI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES 60063400','Pieces':'1','Weight':'1.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'HQ25010000248','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QUANYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALANOUD','ConsigneeContact':'ALANOUD','ConsigneePhone':' 971-555690504','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UAE  ABU DHABI  ALAIN  ZAKHER  STREET 12 HOUSE 10 ABU DHABI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES 60063400','Pieces':'1','Weight':'4.7','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'43','CurrencyID':'2','Currency':'USD','CustomsValue':'11.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1829358967','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUEQING JIXIANG CONNECTOR CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AVANTE VENTILATION EQUIPMENT MANUFACTURING LLC','ConsigneeContact':'AVINASH PODDAR','ConsigneePhone':'04 239 2882','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO 597-4242 STREET NO. 77 SAME LANE OFDRIVE DUBAI NEXT TO GULF RICEFACTORY DIP 2DUBAI DIP 2','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'CABLE GLAND','Pieces':'1','Weight':'0.52','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'33.80','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095704','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI DAIRAWAN DECORATION MATERIAL CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HUANG ZHIJUN','ConsigneeContact':'HUANG ZHIJUN','ConsigneePhone':'00971 507775526','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI CREEK RESIDENTS SOUTH TOWER 3APARTMENT 703  SAME BUILDING GEANTEXPRESS SUPERMARKET DWA1412-40','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'ACRYLIC BRACKET','Pieces':'1','Weight':'2.74','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'103','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'5010362646','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHONGKE PRECISION MACHINERY  ZHEJIANG  CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'EMIRATES PRINTING PRESS L.L.C','ConsigneeContact':'SIVANAND A YYALUSAMY','ConsigneePhone':'971545832743','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O BOX 5106 AL QUOZ.DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'NOZZLE MOUNTING PLATE AND SYNCHRONOUS WHEEL AND EXHAUST VALVE','Pieces':'1','Weight':'0.84','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'57.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'2025000001','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SOON LOGISTICS CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROYAL BLINDS LLC','ConsigneeContact':'RAJEEV RAVINDRAN','ConsigneePhone':' 971562161809  ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'18TH STREET  AL QUOZ INDUSTRIAL AREA 1 DUBAI  UAE PO BOX NO 2107','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'HQ25010000245','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING QUANYOU TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALIA','ConsigneeContact':'ALIA','ConsigneePhone':' 971-502055355','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'UAE- ABU DHABI- ALMUROR- STREET 21 VILLA 114 ABU DHABI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES 60063400','Pieces':'1','Weight':'31.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'41','CurrencyID':'2','Currency':'USD','CustomsValue':'56.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095617','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING JUNCHI  TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DOLLY TEXTILES TRADING L L C','ConsigneeContact':'DOLLY TEXTILES TRADING L L C','ConsigneePhone':'04353523','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 45526 NEAR ABRA MARKET  BUR DUBAI DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'FABRIC HOOK SAMPLE','Pieces':'1','Weight':'2.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'44','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095615','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SAR','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DAR AL QAMAR TEXTILE TRADING LLC','ConsigneeContact':'DAR AL QAMAR  ROOHULLA','ConsigneePhone':'00971542509903','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'JUMA MASJID ROAD TEXTILE MARKET BUR DUBAIUAE 00971542509903','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095614','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'AB','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MUHAMMAD SAAD KHAN','ConsigneeContact':'MUHAMMAD SAAD KHAN','ConsigneePhone':'00971 58 6398815','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'FLAT NO 202 BUILDING R-1087 WASL EMERALD ALKARAMA DUBAI UAE 00971-58-6398815','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095557','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'QING DAO KING ROUTE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SANFREIGHT SHIPPING SERVICES LLC','ConsigneeContact':'AMIT','ConsigneePhone':' 971 55 815 2530','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO. 219  ZAINAL MOHEBI PLAZA  OPP.   CENTERPOINT  NEAR BURJUMAN  AL KARAMA  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'86214392250','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HANSUN TEXTILE PRINTING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HIND WALID','ConsigneeContact':'HIND WALID','ConsigneePhone':'00971505789857','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'VILLA 12 STREET 4A AL MIZHAR 2 P.O.BOX 19909','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095689','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NANTONG ANGUANG TEXTILE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ARA TRADING LLC','ConsigneeContact':'MR INDRAKUMAR','ConsigneePhone':'97143533942','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2ND FLOOR WORLD BLDG TEXTILE WHOLESALE MARKET BEHIND DUBAI MUSEUM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'3.59','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'8.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095676','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI CHAOQI FREIGHT FORWARDER CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'LANDMARK GROUP   MAIN TOWER','ConsigneeContact':'TANVI SINHA','ConsigneePhone':'971 588918320','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BABYSHOP  3RD FLOORJAFZA ONE JABEL ALIP.O. BOX   25030','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'BACKPACK FOR SAMPLE','Pieces':'1','Weight':'4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'92','CurrencyID':'2','Currency':'USD','CustomsValue':'8.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095612','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING CITY ROUSHU CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HASSAN QURESH INTERNATIONAL TEXTILE TRADING LLC','ConsigneeContact':'MUHAMMAD ARSHAD QURESHI','ConsigneePhone':'97143390123','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 81273 OFFICE 101 WAQF OBAID JASSIM AL BAQALI  BUILDING BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER SAMPLE FABRIC','Pieces':'1','Weight':'3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'45','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095602','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'KAIYANG TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'JOGINDER GENERAL TRADING L.L.C','ConsigneeContact':'PAMMI','ConsigneePhone':'971558843265 ','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 29052  SHOP NO 16 AL SAYEGH BLDGBUR DUBAI-U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER SAMPLE FABRIC N C V','Pieces':'1','Weight':'4.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'46','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095601','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'KAIYANG TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'HIGH RISE FREIGHT SERVICES LLC','ConsigneeContact':'RAVI','ConsigneePhone':'971-52-7851415','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PO BOX 49945  OFFICE NO.207 TORONTO BUILDING  BUR DUBAI  DUBAI  U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095577','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING FANLAIGETE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ADNAN','ConsigneeContact':'ADNAN','ConsigneePhone':'971569386833','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 18 MUHAMMAD AQIL ABBASI BUILDINGTEXTILE WHOLESALE MARKET BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095672','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG HUAZHE TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NSK BUILDING MATERIALS TRADING LLC','ConsigneeContact':'ABDUL RUB','ConsigneePhone':'042294144','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 39912  DUBAI AL SHABIB BUILDINGOFFICE NUMBER 101 1ST FLOOR   OPPOSITE DEIRA PARK HOTEL  AL NAKEEL AREA  DIERA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.58','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095671','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG HUAZHE TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PERFECT BUILDING MATERIAL TRADING  LLC','ConsigneeContact':'ABIZAR','ConsigneePhone':' 0559723843','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI UAE RAS AL KHOR  ROD AWER','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.18','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095670','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FAR EAST TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SANDEEP SINGH RANA','ConsigneeContact':'SANDEEP SINGH RANA','ConsigneePhone':'971503182959','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROONI BUILDING2ND FLOOR OFFICE NO 20283 STREET AL RAS DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'WOVEN FABRICS','Pieces':'1','Weight':'14.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'91','CurrencyID':'2','Currency':'USD','CustomsValue':'97.50','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095633','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU YUCHANG TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneeContact':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneePhone':'971 52 6880086','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHIP#08 AL MULLA DUILDINGOPP TARA HOTEL DEIRA DUBAI-U.A.EE-MAIL 634839924 QQ.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PLASTIC SHEET FREE SAMPLES','Pieces':'2','Weight':'33.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'93-94','CurrencyID':'2','Currency':'USD','CustomsValue':'36.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095598','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAI ZHOU AIBIXI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NAC HOME BUILDING MATERIALS LLC','ConsigneeContact':'SIRAJ','ConsigneePhone':'971527386164','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 301 INDIGO OPTIMAINTERNATIONAL CITY DUBAI U. A. E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'GLASS MOSAIC SAMPLE','Pieces':'1','Weight':'7.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'92','CurrencyID':'2','Currency':'USD','CustomsValue':'1.98','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'333000039','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'DISSTON INDUSTRIAL SALES  INC','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TRANSWILL TRADING LLC','ConsigneeContact':'BINDIA','ConsigneePhone':' 971 56 604 6509 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO 597-608  18TH STREETP.O BOX 231725 DUBAI INVESTMENT PARK 2','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'STEEL WIRE ROPE','Pieces':'1','Weight':'0.86','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095599','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XINXIANG CITY ZUOBINGHAN FILTER EQUIPMENT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PROTOSS ME LLC','ConsigneeContact':'MR. BI JU','ConsigneePhone':' 971 56 4330955','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROUNI BUILDING SHOP NO 9AL QUSAIS   INDU AREA   5 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'602925095','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NANTONG FENGSHENG TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FAROUGH ABDULRAHIM ZIYAEI GENTRAL TRADING','ConsigneeContact':'JERRILYU ARELLANO','ConsigneePhone':'97143530808','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 42818 OFFICE 507 NEW ENBB BUILDINGBANIYAS ROAD DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603364814','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING YINGBORUI TEX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASIR ABDUL HAMEAD','ConsigneeContact':'NASIR ABDUL HAMEAD','ConsigneePhone':'00971551141330','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'FLAT 305 AL MATCO 5 AL MUWAIHATZ AJMAN UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603314996','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HEJIN FABRIC CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S KISHINCHAND BASSARMAL TRADING CO LLC','ConsigneeContact':'MR MOHAN','ConsigneePhone':'04 3531463','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 2037ABRA MARKET BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'PLY TEXTILE CUTTING SAMPLE','Pieces':'1','Weight':'0.46','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095595','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NU LI TEX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RIAZ AHMED','ConsigneeContact':'RIAZ AHMED','ConsigneePhone':'971551065089','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MASJID ALI BUR DUBAI FHADHI STREET UAE 39167','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095591','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHULAMI TEX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASIR ABDUL HAMEAD','ConsigneeContact':'NASIR ABDUL HAMEAD','ConsigneePhone':'97155114 1330','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'FLAT 305  AL MATCO 5  AL MUWAIHAT 2AJMAN  UNITED ARABIC EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095668','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HANGJIE FAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DMK CO TRADING L.L.C','ConsigneeContact':'DMK CO TRADING L.L.C','ConsigneePhone':'971508874633 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX  119097MALIK NASAR HUSAIN BUILDINGSHOP   20  NEAR SABKHA  DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'251031831','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JASON MARKETING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'WADI AL WAFA TEXTILES & LADIES TAILOR','ConsigneeContact':'KALLEM','ConsigneePhone':'00971501127594','ConsigneeMobileNo':'','ConsigneeCountryName':'AL AIN CITY(AE)','ConsigneeCityName':'AL AIN CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHUMALL MARKET NEAR JEBEL ROUND ABOUT','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'HOTFIX STONE','Pieces':'2','Weight':'22.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'89','CurrencyID':'2','Currency':'USD','CustomsValue':'99.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095625','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANDONG AURORA METAL CYLINDER CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TSSC KITCHEN & LAUNDRY EQUIPMENT TRADING LLC','ConsigneeContact':'MOHIDEEN ABDUL KHADE','ConsigneePhone':' 971 4343 1100','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO. 1  SHEIKH MAJID BLDG  SHEIKH  ZAYED ROAD  P.O. BOX 69  DUBAI  UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095619','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO SHIDA LVYE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'EISA MOHAMMAD','ConsigneeContact':'EISA MOHAMMAD','ConsigneePhone':'00971525121308','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI DEIRA ALMURAR AREA ALSUWAIDI BUILDINGOFFICE NO207','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'DOC','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095571','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHINOR FABRIC CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ALDAR ALAMIRA TEXTILE TRADING CO LLC','ConsigneeContact':'RAHMAT','ConsigneePhone':'971558750184','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUR DUBAI - WHOLESALE TEXTILE MARKET AIDIWAN CENTER - SHOP NO. G12','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'9.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'47','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095671','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHIJIAZHUANG HUAZHE TRADE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PERFECT BUILDING MATERIAL TRADING  LLC','ConsigneeContact':'ABIZAR','ConsigneePhone':' 0559723843','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI UAE RAS AL KHOR  ROD AWER','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.18','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095568','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHINOR FABRIC CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MANOHAR TRADING CO LLC','ConsigneeContact':'NAVEEN','ConsigneePhone':'043529282','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.7 MUMTAZ BUILDING SOUQ AL KABEER NEAR DUBAI MUSEUM DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'2.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095670','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FAR EAST TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SANDEEP SINGH RANA','ConsigneeContact':'SANDEEP SINGH RANA','ConsigneePhone':'971503182959','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROONI BUILDING2ND FLOOR OFFICE NO 20283 STREET AL RAS DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'14.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'91','CurrencyID':'2','Currency':'USD','CustomsValue':'97.50','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095501','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'CHANGXING YARU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SANGSOFT TEXTILES TRADING LLC','ConsigneeContact':'YOUSUF SIDDIQUE','ConsigneePhone':'00971557623540','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ABDUL REHMAN AHMED ESSAMI BUILDING AL BUTEEN DEIRA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER CUTTING SAMPLE','Pieces':'2','Weight':'22','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'48-48','CurrencyID':'2','Currency':'USD','CustomsValue':'23.40','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095633','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAIZHOU YUCHANG TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneeContact':'BOSS POWER BUILDING MATERIALS L.L.C','ConsigneePhone':'971 52 6880086','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHIP#08 AL MULLA DUILDINGOPP TARA HOTEL DEIRA DUBAI-U.A.EE-MAIL 634839924 QQ.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'33.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'93-94','CurrencyID':'2','Currency':'USD','CustomsValue':'36.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603426814','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'CHANGXING RONGHUI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'METRO CITY TEXTILES TRADING','ConsigneeContact':'KUMAR','ConsigneePhone':'042258781','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SOUQ AL AJMANI BLDG SHOP 11-12 GWAORI MARKET DEIRA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER CUTTING SAMPLE','Pieces':'1','Weight':'7.7','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'49','CurrencyID':'2','Currency':'USD','CustomsValue':'9.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095598','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'TAI ZHOU AIBIXI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NAC HOME BUILDING MATERIALS LLC','ConsigneeContact':'SIRAJ','ConsigneePhone':'971527386164','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE 301 INDIGO OPTIMAINTERNATIONAL CITY DUBAI U. A. E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'7.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'92','CurrencyID':'2','Currency':'USD','CustomsValue':'1.98','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095593','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUANGSHOU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MS VEESHAM TRADERS','ConsigneeContact':'MR MAHESH SHARMA','ConsigneePhone':'558879372','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX  57135  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLES OF N C V','Pieces':'1','Weight':'12','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'3','CurrencyID':'2','Currency':'USD','CustomsValue':'15.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'333000039','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'DISSTON INDUSTRIAL SALES  INC','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TRANSWILL TRADING LLC','ConsigneeContact':'BINDIA','ConsigneePhone':' 971 56 604 6509 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO 597-608  18TH STREETP.O BOX 231725 DUBAI INVESTMENT PARK 2','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.86','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603312458','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'AIYI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AWT TEXTILE TRADING','ConsigneeContact':'NADER YEGANEH','ConsigneePhone':'97143372103','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 252461 ABRA MARKET HASSAN ABDULLA ABDUL RASOOL ALSHAWAB BLDG SHOP NO 13','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'POLYESTER FABRIC SAMPLE','Pieces':'1','Weight':'16.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'2','CurrencyID':'2','Currency':'USD','CustomsValue':'17.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095599','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XINXIANG CITY ZUOBINGHAN FILTER EQUIPMENT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PROTOSS ME LLC','ConsigneeContact':'MR. BI JU','ConsigneePhone':' 971 56 4330955','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL ZAROUNI BUILDING SHOP NO 9AL QUSAIS   INDU AREA   5 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407902','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'PANASIA IMPEX LLC','ConsigneeContact':'MR VISHAL SINGLA','ConsigneePhone':'04-3535780','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 32178 DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'7.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'602925095','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NANTONG FENGSHENG TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FAROUGH ABDULRAHIM ZIYAEI GENTRAL TRADING','ConsigneeContact':'JERRILYU ARELLANO','ConsigneePhone':'97143530808','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 42818 OFFICE 507 NEW ENBB BUILDINGBANIYAS ROAD DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407900','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'VENTUREZ COMMERCIAL BROKERS LLC','ConsigneeContact':'MR NIKKI VENTUREZ COMMERCIAL','ConsigneePhone':'0097143559556','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO.103 FIRST FLOOR SHAMMA JUMA   BUILDING OLD NEEL KAMAL BUILDING    BESIDE  DUBAI MUSUEM AL FAHIDI STREET','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'8.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'5','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095668','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HANGJIE FAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DMK CO TRADING L.L.C','ConsigneeContact':'DMK CO TRADING L.L.C','ConsigneePhone':'971508874633 ','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX  119097MALIK NASAR HUSAIN BUILDINGSHOP   20  NEAR SABKHA  DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095579','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HEFEI EVA RUBBER MANUFACTURER CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL FAYADH PRINTING PRESS L L C','ConsigneeContact':'MR MUGHEE SAHMADK HAN','ConsigneePhone':'00971507290056','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 28624 SHARJAH SHARJAH  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'251031831','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JASON MARKETING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'WADI AL WAFA TEXTILES & LADIES TAILOR','ConsigneeContact':'KALLEM','ConsigneePhone':'00971501127594','ConsigneeMobileNo':'','ConsigneeCountryName':'AL AIN CITY(AE)','ConsigneeCityName':'AL AIN CITY(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHUMALL MARKET NEAR JEBEL ROUND ABOUT','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'22.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'89','CurrencyID':'2','Currency':'USD','CustomsValue':'99.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3486168114','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LIU CAI KUI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROYALLINE TRADING PTE LTD','ConsigneeContact':'SHIVAM MENARIA','ConsigneePhone':'971 568649101','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2907-06 & 2901  JBC 3  CLUSTER Y  JLT DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.64','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095625','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANDONG AURORA METAL CYLINDER CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TSSC KITCHEN & LAUNDRY EQUIPMENT TRADING LLC','ConsigneeContact':'MOHIDEEN ABDUL KHADE','ConsigneePhone':' 971 4343 1100','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'OFFICE NO. 1  SHEIKH MAJID BLDG  SHEIKH  ZAYED ROAD  P.O. BOX 69  DUBAI  UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338017','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOSSY GENERAL TRADING LLC','ConsigneeContact':'GLOSSY GENERAL TRADING LLC','ConsigneePhone':'042268281','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL RAS -DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095619','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO SHIDA LVYE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'EISA MOHAMMAD','ConsigneeContact':'EISA MOHAMMAD','ConsigneePhone':'00971525121308','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI DEIRA ALMURAR AREA ALSUWAIDI BUILDINGOFFICE NO207','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095628','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JIANG YIN ZHONGNAN HEAVY INDUSTRIES CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CHINA PETROLEUM PIPELINE ENGINEERING COMPANY LIMITED','ConsigneeContact':'JERECA PEARL DELFIN','ConsigneePhone':'971 50 968 2603','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'49TH FLOOR ADDAX OFFICE TOWER AL RAYFAH STAL REEM ISLAND - CITY OF LIGHTS','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'16.66','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'87','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095579','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'HEFEI EVA RUBBER MANUFACTURER CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL FAYADH PRINTING PRESS L L C','ConsigneeContact':'MR MUGHEE SAHMADK HAN','ConsigneePhone':'00971507290056','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 28624 SHARJAH SHARJAH  UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.16','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407899','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S MODERNTEX TEXTILE','ConsigneeContact':'MR RAJESH','ConsigneePhone':'04-3542150','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'#105 TEXMAS BLDG P.O.BOX 43476 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'3.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3486168114','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LIU CAI KUI','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROYALLINE TRADING PTE LTD','ConsigneeContact':'SHIVAM MENARIA','ConsigneePhone':'971 568649101','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2907-06 & 2901  JBC 3  CLUSTER Y  JLT DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.64','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603407901','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU HENGYI IMP EXP CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CHARMAX TEXTILES TRADING LLC','ConsigneeContact':'MR LEE CHARMAX TEXTILES','ConsigneePhone':'043534544','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BUR DUBAI OFF 102 PLOT#312-1309 P.O.BOX 6926 ALSUQ ALKABEER','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'5.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338017','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOSSY GENERAL TRADING LLC','ConsigneeContact':'GLOSSY GENERAL TRADING LLC','ConsigneePhone':'042268281','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL RAS -DEIRA','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.17','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'603363709','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FUZE TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL MADEEN TRADING CO','ConsigneeContact':'BUNTY BUNTY','ConsigneePhone':'00971502557760','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL DIWAN CENTER 21   SHOP 3/5  DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'3.4','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095628','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JIANG YIN ZHONGNAN HEAVY INDUSTRIES CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CHINA PETROLEUM PIPELINE ENGINEERING COMPANY LIMITED','ConsigneeContact':'JERECA PEARL DELFIN','ConsigneePhone':'971 50 968 2603','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'49TH FLOOR ADDAX OFFICE TOWER AL RAYFAH STAL REEM ISLAND - CITY OF LIGHTS','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'16.66','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'87','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095570','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'FUZE TEXTILE','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ROZI KHAN TEXTILE CO. LLC','ConsigneeContact':'ROZI KHAN','ConsigneePhone':'0551077869','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 46529 BEHIND NAIF POLZCE STATION DERIA DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.45','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095695','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING JUNCHI  TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL RAFIHA TRADING L.L.C','ConsigneeContact':'AL RAFIHA TRADING L.L.C','ConsigneePhone':'043444735','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HAMMAD BUILDING 4 FIRST FLR OFFICE NO 104BUR-DUBAI U.A.E P.O BOX 43703','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338014','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL KABYEL DISCOUNT CENTER','ConsigneeContact':'ABU DULLAH','ConsigneePhone':'00971506284757','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ABU BAKER AL SIQIQ ROAD','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.21','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095693','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JI MING GANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FOR NURTUURE GOODS COURIER','ConsigneeContact':'FOR NURTUURE GOODS COURIER','ConsigneePhone':'0567622841','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL REEM TOWERS 1807 RIGGAT AL BUTEEM NEXT TO ETISALAT OFFICE NEAR UNION METRO STATION DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666776','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAJID. AHMED. KHAN.','ConsigneeContact':'SAJID. AHMED. KHAN.','ConsigneePhone':'0552855009','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'M.17 MUSSAFAH ABUDHABI.   UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.25','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095691','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUANGSHOU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GOLDEN OSKAR TEXTILES','ConsigneeContact':'MR GOPAL GOPAL','ConsigneePhone':'00971504694258','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NEAR SONA TEXTILE GWADIRI  MARKET SABKA BUS STATION  DEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'10.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102','CurrencyID':'2','Currency':'USD','CustomsValue':'13.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666765','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'XIAOBING ZHENG','ConsigneeContact':'XIAOBING ZHENG','ConsigneePhone':'00971 55 7068999','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SOMALIA BUILDING 207 SHOP ANTLER UPSTAIRSDOWNTOWN HOTEL OPPOSITEDEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.92','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388338014','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL KABYEL DISCOUNT CENTER','ConsigneeContact':'ABU DULLAH','ConsigneePhone':'00971506284757','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ABU BAKER AL SIQIQ ROAD','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.21','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388337989','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOBAL CONNECTION IMPEX TRADING LLC','ConsigneeContact':'DIANA MONTEIRO','ConsigneePhone':'971 56 116 8480','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.5 ABDUL AZIZ AL MAJID BIDG. P.O.BOX32031 DEIRA DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'5.77','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'86','CurrencyID':'2','Currency':'USD','CustomsValue':'18.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666776','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SAJID. AHMED. KHAN.','ConsigneeContact':'SAJID. AHMED. KHAN.','ConsigneePhone':'0552855009','ConsigneeMobileNo':'','ConsigneeCountryName':'ABU DHABI(AE)','ConsigneeCityName':'ABU DHABI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'M.17 MUSSAFAH ABUDHABI.   UAE.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.25','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8522177786','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HENGBANG IMPORT EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL BAZ GENERAL TRADING LLC RULER BLDG','ConsigneeContact':'MR HARSAN HARSAN','ConsigneePhone':'97143536459','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BLDG NO R295 WASL PROPERTY  OFFICE NO 201 2ND FLOOR  AREA AL SOUK AL KABEER OPP BANK OF BARODA P O BOX 111148 BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'25.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'1','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'3701666765','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'XIAOBING ZHENG','ConsigneeContact':'XIAOBING ZHENG','ConsigneePhone':'00971 55 7068999','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SOMALIA BUILDING 207 SHOP ANTLER UPSTAIRSDOWNTOWN HOTEL OPPOSITEDEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.92','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'510432273','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YANG BO','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'LATATEX INTERNATIONAL TRADING LLC','ConsigneeContact':'MR MANISH SAWLANI','ConsigneePhone':'3530190','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 47293 OFF#201 ESSA SALEH AL GURGBUILDING OPP BANK OF BARODA WHOLESALETEXTILE MARKET BUR-DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'2.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'1388337989','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'BXE TRADING CO LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GLOBAL CONNECTION IMPEX TRADING LLC','ConsigneeContact':'DIANA MONTEIRO','ConsigneePhone':'971 56 116 8480','ConsigneeMobileNo':'','ConsigneeCountryName':'DEIRA(AE)','ConsigneeCityName':'DEIRA(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.5 ABDUL AZIZ AL MAJID BIDG. P.O.BOX32031 DEIRA DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'5.77','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'86','CurrencyID':'2','Currency':'USD','CustomsValue':'18.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'86575613887','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'X Q G','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CITY VIEW TEXTILES LLC','ConsigneeContact':'MR ADITYA','ConsigneePhone':'971527781122','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHD AHMED KABITAL BLDG AL SOUK AL KABIRAL FAHIDI STREET BEHIND SUNCITY HOTEL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8522177786','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HENGBANG IMPORT EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL BAZ GENERAL TRADING LLC RULER BLDG','ConsigneeContact':'MR HARSAN HARSAN','ConsigneePhone':'97143536459','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'BLDG NO R295 WASL PROPERTY  OFFICE NO 201 2ND FLOOR  AREA AL SOUK AL KABEER OPP BANK OF BARODA P O BOX 111148 BUR DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'25.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'1','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095576','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FIRST SPORT LLC DUBAI UAE','ConsigneeContact':'FIRST SPORT LLC DUBAI UAE','ConsigneePhone':'042351430','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.1 MURSHID BAIAT DEIRA DUBAIP.O.BOX 47877 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'510432273','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YANG BO','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'LATATEX INTERNATIONAL TRADING LLC','ConsigneeContact':'MR MANISH SAWLANI','ConsigneePhone':'3530190','ConsigneeMobileNo':'','ConsigneeCountryName':'BUR DUBAI(AE)','ConsigneeCityName':'BUR DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P O BOX 47293 OFF#201 ESSA SALEH AL GURGBUILDING OPP BANK OF BARODA WHOLESALETEXTILE MARKET BUR-DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'2.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'YY25010000021','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUEYANG INTERNATIONAL TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ZAKARYA LABIOD','ConsigneeContact':'ZAKARYA LABIOD','ConsigneePhone':'971-524221873','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A  AL JAWHARA BUILDING  BANK STREET -OFFICE 708 - BUR DUBAI - DUBAIZAKARYA.L VERGER-GROUP.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.38','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'86575613887','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'X Q G','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'CITY VIEW TEXTILES LLC','ConsigneeContact':'MR ADITYA','ConsigneePhone':'971527781122','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHD AHMED KABITAL BLDG AL SOUK AL KABIRAL FAHIDI STREET BEHIND SUNCITY HOTEL','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095643','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG HUZHOU SENFUJIDIAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASER ASADI TRADING LLC','ConsigneeContact':'NASER ASADI TRADING LLC','ConsigneePhone':'00971569931111','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO-3 NEAR NBF BANK AL RAS ON AL RAS METRO STATION AL RAS DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'17.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'88','CurrencyID':'2','Currency':'USD','CustomsValue':'24.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095576','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGYOU INTERNATIONAL IMPORT AND EXPORT CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FIRST SPORT LLC DUBAI UAE','ConsigneeContact':'FIRST SPORT LLC DUBAI UAE','ConsigneePhone':'042351430','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO.1 MURSHID BAIAT DEIRA DUBAIP.O.BOX 47877 DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'Y2501030190','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG BOTOLINI MACHINERY CO  LT','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SABA HYDRAULIC PUMPS TRADING LLC','ConsigneeContact':'SAJI VARUGHESE','ConsigneePhone':'971527513456','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'INDUSTRIAL AREA-3 CATERPILLAR ROAD SHARJAHUAE PO BOX.97324','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.91','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'YY25010000021','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YUEYANG INTERNATIONAL TRADE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'ZAKARYA LABIOD','ConsigneeContact':'ZAKARYA LABIOD','ConsigneePhone':'971-524221873','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A  AL JAWHARA BUILDING  BANK STREET -OFFICE 708 - BUR DUBAI - DUBAIZAKARYA.L VERGER-GROUP.COM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.38','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8626618976','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'PRIME FASHION','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S VIP CHOICE FASHIONS LLC','ConsigneeContact':'MR SUNIL','ConsigneePhone':'9714 3538614','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMED BARAKAT BUILDING 3RD FLOOR 3RD FLOOR301 P O BOX 12520 DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095643','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG HUZHOU SENFUJIDIAN','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'NASER ASADI TRADING LLC','ConsigneeContact':'NASER ASADI TRADING LLC','ConsigneePhone':'00971569931111','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO-3 NEAR NBF BANK AL RAS ON AL RAS METRO STATION AL RAS DEIRA DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'17.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'88','CurrencyID':'2','Currency':'USD','CustomsValue':'24.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095537','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUOLE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LIMITED','ConsigneeContact':'BEAUTY OR ANGELICA','ConsigneePhone':'971 45175974','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'LANDMARK GROUP-MAIN TOWER MAX FASHIONGROUND FLOOR JAFZA ONE JEBEL ALI P.O BOX113630 DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'Y2501030190','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG BOTOLINI MACHINERY CO  LT','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SABA HYDRAULIC PUMPS TRADING LLC','ConsigneeContact':'SAJI VARUGHESE','ConsigneePhone':'971527513456','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'INDUSTRIAL AREA-3 CATERPILLAR ROAD SHARJAHUAE PO BOX.97324','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.91','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095527','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HUIMO TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SUMERU INDUSTRIES FZE','ConsigneeContact':'ANDND LOKHANDLWALA','ConsigneePhone':'00 971 55 378 4557','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH FREE ZONE(AE)','ConsigneeCityName':'SHARJAH FREE ZONE(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A2-075 SHARIJAH AIRPORT    FREE ZONE U.A.E PO   BOX#120992','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'2.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'8626618976','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'PRIME FASHION','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'M S VIP CHOICE FASHIONS LLC','ConsigneeContact':'MR SUNIL','ConsigneePhone':'9714 3538614','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'MOHAMMED BARAKAT BUILDING 3RD FLOOR 3RD FLOOR301 P O BOX 12520 DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095519','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'AM OL SHINDE BUYER LEECOOPER','ConsigneePhone':'04-809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDM ARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI MOB 050.7845686','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095537','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUOLE TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LIMITED','ConsigneeContact':'BEAUTY OR ANGELICA','ConsigneePhone':'971 45175974','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'LANDMARK GROUP-MAIN TOWER MAX FASHIONGROUND FLOOR JAFZA ONE JEBEL ALI P.O BOX113630 DUBAI U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095518','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'KHURRAM','ConsigneePhone':'04-809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDMARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'3','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095527','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HUIMO TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SUMERU INDUSTRIES FZE','ConsigneeContact':'ANDND LOKHANDLWALA','ConsigneePhone':'00 971 55 378 4557','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH FREE ZONE(AE)','ConsigneeCityName':'SHARJAH FREE ZONE(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'A2-075 SHARIJAH AIRPORT    FREE ZONE U.A.E PO   BOX#120992','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'2.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095502','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'MINGBANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GROUP OF COMPANLES','ConsigneeContact':'MOHAMMAD BILLAL','ConsigneePhone':'971553830226','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'R16 SHOP NO 10 INTERNATIONAL CITYDUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.44','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095519','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'AM OL SHINDE BUYER LEECOOPER','ConsigneePhone':'04-809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDM ARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI MOB 050.7845686','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'4','CurrencyID':'2','Currency':'USD','CustomsValue':'3.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095608','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG SUNTE TECHNOLOGY CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DYNATRADE AUTOMOTIVE L.L.C','ConsigneeContact':'FEMIL JAMES','ConsigneePhone':' 971 50 938 1231','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 22842 INDUSTRIAL AREA-17 SHARJAH- UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095518','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'XIYA TEXTILE AND CLOTHING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RNA RESOURCES GROUP LTD','ConsigneeContact':'KHURRAM','ConsigneePhone':'04-809 4673','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SPLASH LANDMARK GROUP BUILDING GATE 1ENTRANCE NO.2NEAR DUTCO JEBEL ALIINDUSTRIAL AREA 1 DUBAI UNITED ARAB EMIRATES','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.8','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'3','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095581','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO HOMEBEST E COMMERCE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RR MIDDLE EAST FZCO','ConsigneeContact':'PAVAN KUMAR ALWANI','ConsigneePhone':'971 50 439 5240','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2903  2904  2905 & 2906  IRIS BAY AL MUSTAQBAL STREET  BUSINESS BAY P. O. BOX  31680  DUBAI  U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095502','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'MINGBANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GROUP OF COMPANLES','ConsigneeContact':'MOHAMMAD BILLAL','ConsigneePhone':'971553830226','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'R16 SHOP NO 10 INTERNATIONAL CITYDUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.44','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'1.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095564','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LOTUS FANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAXIMAN TRADING CO .LLC','ConsigneeContact':'MS CAMILLE','ConsigneePhone':' 971 501487028','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI  DEIRA AREA  AL RAS MARKET. BESIDE  HOUSE MART P.O BOX  378917','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095608','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'ZHEJIANG SUNTE TECHNOLOGY CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'DYNATRADE AUTOMOTIVE L.L.C','ConsigneeContact':'FEMIL JAMES','ConsigneePhone':' 971 50 938 1231','ConsigneeMobileNo':'','ConsigneeCountryName':'SHARJAH(AE)','ConsigneeCityName':'SHARJAH(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 22842 INDUSTRIAL AREA-17 SHARJAH- UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.11','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095555','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'WUXI BRIGHTSKY ELECTRONIC CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SFFECO GLOBAL FZE','ConsigneeContact':'MR. WILLIAM MS. MARY','ConsigneePhone':'009714-8809890','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO. S10833 & S10903 PO BOX 261318 JEBEL ALI FREE ZONE  SOUTH  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095581','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'NINGBO HOMEBEST E COMMERCE CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'RR MIDDLE EAST FZCO','ConsigneeContact':'PAVAN KUMAR ALWANI','ConsigneePhone':'971 50 439 5240','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'2903  2904  2905 & 2906  IRIS BAY AL MUSTAQBAL STREET  BUSINESS BAY P. O. BOX  31680  DUBAI  U.A.E.','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.1','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'6.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095695','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING JUNCHI  TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL RAFIHA TRADING L.L.C','ConsigneeContact':'AL RAFIHA TRADING L.L.C','ConsigneePhone':'043444735','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HAMMAD BUILDING 4 FIRST FLR OFFICE NO 104BUR-DUBAI U.A.E P.O BOX 43703','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.2','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095564','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'LOTUS FANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MAXIMAN TRADING CO .LLC','ConsigneeContact':'MS CAMILLE','ConsigneePhone':' 971 501487028','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'DUBAI  DEIRA AREA  AL RAS MARKET. BESIDE  HOUSE MART P.O BOX  378917','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095693','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'JI MING GANG','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'FOR NURTUURE GOODS COURIER','ConsigneeContact':'FOR NURTUURE GOODS COURIER','ConsigneePhone':'0567622841','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'AL REEM TOWERS 1807 RIGGAT AL BUTEEM NEXT TO ETISALAT OFFICE NEAR UNION METRO STATION DUBAI UAE','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.3','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102-1','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095555','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'WUXI BRIGHTSKY ELECTRONIC CO  LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'SFFECO GLOBAL FZE','ConsigneeContact':'MR. WILLIAM MS. MARY','ConsigneePhone':'009714-8809890','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'PLOT NO. S10833 & S10903 PO BOX 261318 JEBEL ALI FREE ZONE  SOUTH  DUBAI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'0.15','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'90','CurrencyID':'2','Currency':'USD','CustomsValue':'0.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095691','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING SHUANGSHOU TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GOLDEN OSKAR TEXTILES','ConsigneeContact':'MR GOPAL GOPAL','ConsigneePhone':'00971504694258','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'NEAR SONA TEXTILE GWADIRI  MARKET SABKA BUS STATION  DEIRA DUBAI U A E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'10.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'102','CurrencyID':'2','Currency':'USD','CustomsValue':'13.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095497','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING WEI EN IMP EXP  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TOP CITY TEXTILE TRADING LLC','ConsigneeContact':'TOPCITYTEX','ConsigneePhone':'00971555172691','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SABKHA BUILDING SHOP NO 22-24 NEAR  SABKHA BUS STATIONDEIRA DUBAI-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'3.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095497','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING WEI EN IMP EXP  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'TOP CITY TEXTILE TRADING LLC','ConsigneeContact':'TOPCITYTEX','ConsigneePhone':'00971555172691','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SABKHA BUILDING SHOP NO 22-24 NEAR  SABKHA BUS STATIONDEIRA DUBAI-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'3.5','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'6','CurrencyID':'2','Currency':'USD','CustomsValue':'4.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095551','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU YAOWANG TEXTILE  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MUKESH TEXTDLIUM','ConsigneeContact':'MIKE','ConsigneePhone':'3534349','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HISTOPICA BUILDING SHOP NO 6 BUR DUBAI U A E NEAR DUBAI MUSEUM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.6','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095551','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SUZHOU YAOWANG TEXTILE  CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MUKESH TEXTDLIUM','ConsigneeContact':'MIKE','ConsigneePhone':'3534349','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'HISTOPICA BUILDING SHOP NO 6 BUR DUBAI U A E NEAR DUBAI MUSEUM','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.6','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'2.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095514','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YOU COME EPX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BHAGAT TRADING CO LLC','ConsigneeContact':'GU RIND','ConsigneePhone':'00971552427355','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 4 7 ABDULLA ABDUL RASOOL  BUILDING WHOLESALE TEXTILE MARKET   BUR DUBAI NEAR DUBAI MUSEUM PO BOX 51617','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'6.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'5','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095514','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'YOU COME EPX','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'BHAGAT TRADING CO LLC','ConsigneeContact':'GU RIND','ConsigneePhone':'00971552427355','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'SHOP NO 4 7 ABDULLA ABDUL RASOOL  BUILDING WHOLESALE TEXTILE MARKET   BUR DUBAI NEAR DUBAI MUSEUM PO BOX 51617','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'6.14','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'5','CurrencyID':'2','Currency':'USD','CustomsValue':'10.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095503','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HEYI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL NADER GENTS TAILORING L.L .C.','ConsigneeContact':'AMIN HOSSIAN','ConsigneePhone':'97167314749','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 7660 NEW INDUSTRIAL AREAAJMAN-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.49','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095503','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHAOXING HEYI TEXTILE CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'AL NADER GENTS TAILORING L.L .C.','ConsigneeContact':'AMIN HOSSIAN','ConsigneePhone':'97167314749','ConsigneeMobileNo':'','ConsigneeCountryName':'AJMAN(AE)','ConsigneeCityName':'AJMAN(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'P.O.BOX 7660 NEW INDUSTRIAL AREAAJMAN-U.A.E','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB EY DXB GREEN 607-37035445','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'1','Weight':'1.49','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'15','CurrencyID':'2','Currency':'USD','CustomsValue':'5.00','MAWB':'607-37035445','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095684','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'E SUN INVESTMENT GROUP LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MR NASER ABDULAZEEZ AHMED','ConsigneeContact':'MR NASER ABDULAZEEZ AHMED','ConsigneePhone':'97126454444','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROX SHOWROOM JOUD HOTEL BUILDING NO.97AL MAKTOUM RD AL KHABAISIDUBAI DEIRA AL ITTIHAD STREET','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'12.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'29-30','CurrencyID':'2','Currency':'USD','CustomsValue':'35.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'N1501095684','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'E SUN INVESTMENT GROUP LIMITED','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'MR NASER ABDULAZEEZ AHMED','ConsigneeContact':'MR NASER ABDULAZEEZ AHMED','ConsigneePhone':'97126454444','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'ROX SHOWROOM JOUD HOTEL BUILDING NO.97AL MAKTOUM RD AL KHABAISIDUBAI DEIRA AL ITTIHAD STREET','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'3','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'12.9','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'29-30','CurrencyID':'2','Currency':'USD','CustomsValue':'35.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'LYW250103AE','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GHAZI ADEL','ConsigneeContact':'GHAZI ADEL','ConsigneePhone':'9711565444443','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'8 AL ASHRI&#39 AH STREET ALAIN ALTOWAYAJAFFER STREET NUMBER 8 HOME 5AL AIN CITYABU DHABI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'43','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'27-28','CurrencyID':'2','Currency':'USD','CustomsValue':'400.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'},{'ShipmentID':'0','CustomerID':'66765','AWBNo':'LYW250103AE','AWBDate':'01/04/2025 00:00:00','PaymentModeId':'2','ConsignorCountryName':'China','ConsignorCityName':'Beijing','ConsignorLocationName':'','CustomerShipperSame':0,'Consignor':'SHANGHAI LIHANG TRADING CO LTD','ConsignorContact':'','ConsignorAddress1_Building':'','ConsignorAddress2_Street':'','ConsignorAddress3_PinCode':'','ConsignorPhone':'','ConsignorMobileNo':'','Consignee':'GHAZI ADEL','ConsigneeContact':'GHAZI ADEL','ConsigneePhone':'9711565444443','ConsigneeMobileNo':'','ConsigneeCountryName':'DUBAI(AE)','ConsigneeCityName':'DUBAI(AE)','ConsigneeLocationName':'','ConsigneeAddress1_Building':'8 AL ASHRI&#39 AH STREET ALAIN ALTOWAYAJAFFER STREET NUMBER 8 HOME 5AL AIN CITYABU DHABI','ConsigneeAddress2_Street':'','ConsigneeAddress3_PinCode':'','MovementID':'3','ProductTypeID':'12','ParcelTypeID':'4','Remarks':'4TH-JAN-2025 SHA-DXB MU DXB BLUE 112-12512393','MaterialCost':'0','NetTotal':'','CargoDescription':'','Pieces':'2','Weight':'43','EntrySource':'','CourierCharge':'','OtherCharge':'','SurchargePercent':'','SurchargeAmount':'','TaxPercent':'','TaxAmount':'','BagNo':'27-28','CurrencyID':'2','Currency':'USD','CustomsValue':'400.00','MAWB':'112-12512393','ManifestWeight':'','FAgentID':'6','ForwardingAWBNo':'','Route':'SHA-DXB'}]";
           try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                string xml = dt.GetXml();

                //int BatchID, DateTime BatchDate,int CustomerID,string EntrySource, int CourierStatusID,
                if (vm.ID == 0)
                {
                    batch.BatchDate =vm.BatchDate;
                    batch.BatchNumber = InboundShipmentDAO.GetMaxBathcNo(vm.BatchDate, BranchId, FyearId);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;
                    batch.AcFinancialYearid = FyearId;
                    batch.CourierStatusID = vm.CourierStatusID;
                    batch.BranchID = BranchId;
                    batch.CustomerID = vm.CustomerID;
                    batch.EntrySource = vm.EntrySource;
                    batch.OriginAirportCity = vm.OriginAirportCity;
                    batch.DestinationAirportCity = vm.DestinationAirportCity;
                    batch.MAWB = vm.MAWB;
                    batch.Bags = vm.Bags;
                    batch.RunNo = vm.RunNo;
                    batch.Remarks = vm.Remarks;
                    batch.FlightDate = vm.FlightDate;
                    batch.FlightNo = vm.FlightNo;
                    batch.ParcelNo = vm.ParcelNo;

                    db.InboundAWBBatches.Add(batch);
                    db.SaveChanges();
                }
                else
                {
                    batch = db.InboundAWBBatches.Find(vm.ID);
                    batch.BatchDate = vm.BatchDate;
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;
                    
                    db.SaveChanges();
                }
                SaveStatusModel model = new SaveStatusModel();
                model = InboundShipmentDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml);
                if (model.Status == "OK")
                {
                   
                    string companyname = Session["CompanyName"].ToString();
                    if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                    {
                        var bills = new List<updateitem>();
                        var _customer = db.CustomerMasters.Find(vm.CustomerID);
                        if (_customer.APIEnabled == true)
                        {
                            foreach (var item in IDetails)
                            {
                                updateitem uitem = new updateitem();
                                uitem.AWBNo = item.AWBNo;
                                uitem.synchronisedDateTime = Convert.ToDateTime(CommonFunctions.GetCurrentDateTime()).ToString("dd-MMM-yyyy HH:mm");
                                bills.Add(uitem);
                            }
                            postbill postbill = new postbill();
                            postbill.bills = bills;
                            Session["bills"] = postbill;
                            callapipost = "true";

                        }
                    }

                    if (DeleteDetails != "" && DeleteDetails != "[]")
                    {
                        var IDeleteDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(DeleteDetails);

                        DataSet dt1 = new DataSet();
                        dt1 = ToDataTable(IDeleteDetails);
                        string xml1 = dt1.GetXml();
                        SaveStatusModel model1 = InboundShipmentDAO.DeleteAWBBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml1);
                    }

                    return Json(new { Status = "OK", BatchID = batch.ID, CallPostAPI=callapipost, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Failed", BatchID = batch.ID, CallPostAPI = callapipost, message = model.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", CallPostAPI = callapipost, message = ex.Message }, JsonRequestBehavior.AllowGet);
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
        public JsonResult DeleteBatch(int id)
        {
            StatusModel obj = new StatusModel();
            if (id != 0)
            {
                DataTable dt = InboundShipmentDAO.DeleteBatch(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        obj.Status = dt.Rows[0][0].ToString();
                        obj.Message = dt.Rows[0][1].ToString();
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

        public JsonResult SaveBatchTrackStatus(int BatchID,int NewCourierStatusID,DateTime EntryDate,string Remarks)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            SaveStatusModel model = new SaveStatusModel();
            try
            {
                model = InboundShipmentDAO.UpdateBatchShipmementTrackStatus(BatchID, BranchId, CompanyID, userid, FyearId,NewCourierStatusID,EntryDate,Remarks);
                return Json(new { Status = "OK", BatchID = BatchID, message = model.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region BatchDetails
        public ActionResult BatchCostDetails(int id = 0)
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
            ViewBag.Currency = db.CurrencyMasters.ToList();


            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();

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
                if (batch.CourierStatusID != null)
                    v.CourierStatusID = Convert.ToInt32(batch.CourierStatusID);

                customername = db.CustomerMasters.Find(batch.CustomerID).CustomerName;
                v.CustomerID = batch.CustomerID;
                v.CustomerName = customername;

                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();
                //v.Details = InboundShipmentDAO.GetBatchAWBInfo(id);
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
        public ActionResult BatchDetails(int id = 0)
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
            ViewBag.Currency = db.CurrencyMasters.ToList();


            ViewBag.CourierStatusList = db.CourierStatus.ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();

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
                if (batch.CourierStatusID != null)
                    v.CourierStatusID = Convert.ToInt32(batch.CourierStatusID);

                customername = db.CustomerMasters.Find(batch.CustomerID).CustomerName;
                v.CustomerID = batch.CustomerID;
                v.CustomerName = customername;

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
        [HttpPost]
        public ActionResult ShowBatchShipmentList(int ID)
        {
            InboundAWBBatchModel vm = new InboundAWBBatchModel();
            vm.Details = InboundShipmentDAO.GetBatchAWBInfo(ID);
            return PartialView("DetailAWBList", vm);
        }
        [HttpGet]
        public JsonResult RateProcess(int BatchID)
        {
            
            try
            {
                string Result=InboundShipmentDAO.UpdateColoaderShipmentRate(BatchID);

                if (Result == "OK")
                {
                    return Json(new { Status = "OK",  Message = "Rate Processed Successfully " }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Failed",Message = Result},JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult FwdCostProcess(int BatchID)
        {

            try
            {
                string Result = InboundShipmentDAO.UpdateColoaderShipmentFWDRate(BatchID);

                if (Result == "OK")
                {
                    return Json(new { Status = "OK", Message = "Forwarding Cost Processed Successfully " });
                }
                else
                {
                    return Json(new { Status = "Failed", Message = Result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", Message = ex.Message });
            }
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
        public ActionResult ShowImportDataFixation(string FieldName, string Details)
        {
            ImportManifestFixation vm = new ImportManifestFixation();
            var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
            Session["InboundManifestImported"] = IDetails;
            ViewBag.ImportFields = db.ImportFields.ToList();
            return PartialView("ImportDataFixation", vm);
        }
        public List<InboundShipmentModel> ConvertTabletoList(DataTable dt)
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
                obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1_Building"].ToString();
                obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1_Building"].ToString();
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


        public ActionResult ShowImportDataFixation1(string FieldName, int BatchID)
        {
            ImportManifestFixation vm = new ImportManifestFixation();
            vm.FieldName = "ConsigneeCountryName";
            ViewBag.ImportFields = db.ImportFields.ToList();
            return PartialView("ImportDataFixation1", vm);
        }
        [HttpGet]
        public JsonResult GetSourceValue1(string term, string FieldName,int BatchID)
        {
            List<ImportFixationSource> list = new List<ImportFixationSource>();
            list=InboundShipmentDAO.GetTranshipmentSource(BatchID, FieldName);
            if (list != null)
            {
                if (term != null && term.Trim() != "")
                {
                    list = list.Where(cc => cc.SourceValue.Contains(term)).ToList();
                }
            }
            else
            {
                list = new List<ImportFixationSource>();
            }
            return Json(list, JsonRequestBehavior.AllowGet);

          

        }
        [HttpPost]
        public JsonResult UpdateDataFixation1(string TargetColumn, string SourceValue, string TargetValue)
        {
           
            
    

            SaveDataFixation(TargetColumn, SourceValue, TargetValue);
            return Json(new { status = "OK" }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult UpdateDataFixation(string TargetColumn, string SourceValue, string TargetValue, string Details)
        {
            ImportManifestVM model = new ImportManifestVM();
            Details.Replace("{}", "");
            var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
            DataTable ds = new DataTable();
            DataSet dt = new DataSet();
            dt = ToDataTable(IDetails);
            ds = dt.Tables[0];
            foreach (DataColumn col in ds.Columns)
            {
                if (col.ColumnName == TargetColumn)
                {
                    for (int i = 0; i < ds.Rows.Count; i++)
                    {
                        if (ds.Rows[i][TargetColumn].ToString() == SourceValue)
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




        #endregion


        #region GetAPI
        

        [HttpPost]
        public async Task<ActionResult> ImportAPI(DateTime EntryDate,int CustomerID)
        {
            //for Redknox API
            //try
            //    {
            //        var client1 = new HttpClient();
            //        var request = new HttpRequestMessage
            //        {
            //            Method = HttpMethod.Get,
            //            RequestUri = new Uri("https://api-test.dhl.com/track/shipments?trackingNumber=7814901043"),
            //            Headers =
            //{
            //    { "DHL-API-Key", "YpueEn12MECYlzhS89yeXIkAXoDKARKT" },
            //},
            //        };
            //        using (var response = await client1.SendAsync(request))
            //        {
            //            response.EnsureSuccessStatusCode();
            //            var body = await response.Content.ReadAsStringAsync();
            //            Console.WriteLine(body);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        string ee = ex.Message;
            //    }

            ///
            string companyname = Session["CompanyName"].ToString();
            List<InboundShipmentModel> fileData = new List<InboundShipmentModel>();
            List<InboundShipmentModel> Details = new List<InboundShipmentModel>();
            var _customer = db.CustomerMasters.Find(CustomerID);
            if (_customer.APIEnabled != true)
            {
                return Json(new { Status = "Failed", Message = "API is not enabled to this Customer!" });
            }
            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
            {
               
                string ConsignorCountryName = _customer.CountryName;
                string consignorCityName = _customer.CityName;
                string URL = "http://www.niceexpress.net/API/v1/getAPI.do";
                string idate = Convert.ToDateTime(EntryDate).ToString("dd-MMM-yyyy");
                string urlParameters = "?dateTime=" + idate;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    //HTTP GET
                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("niceexpress-apikey", "14c3993c0bb082dcafae9183ac7946d5be7a0e565e12bbd32f5d8d5d78bf3121");
                    client.DefaultRequestHeaders.Add("niceexpress-signature", "37ef0bccb326b2057121ab74fd81cbeee892debaeccdd632d57fff66ffd86ece");

                    // List data response.
                    HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response body.

                        var ItemJsonString = await response.Content.ReadAsStringAsync(); // .ReadAsAsync<MasterDataObject>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                        var dataObjects = JsonConvert.DeserializeObject<MasterDataObject>(ItemJsonString);
                        int sno = 0;
                        foreach (Importproductdata d in dataObjects.data)
                        {
                            InboundShipmentModel item = new InboundShipmentModel();
                            sno++;
                            item.SNo = sno;
                            item.CustomerID = CustomerID;
                            item.AWBNo = d.awbNo;
                            if (d.awbNo== "HF380-6-013891-1")
                            {
                                string s = "check";
                            }
                            item.AWBDate = Convert.ToDateTime(d.awbDate);
                            item.BagNo = d.bagNo;
                            if (d.cod == "")
                                item.MaterialCost = 0;
                            else 
                                item.MaterialCost = Convert.ToDecimal(d.cod);
                            item.Weight = d.weight;
                            item.CargoDescription = StripHtml(d.content.Trim());
                            if (item.CargoDescription.Contains("dox"))
                                item.ParcelTypeID = 2;
                            else if (item.Weight <= 30)
                                item.ParcelTypeID = 3;
                            else
                                item.ParcelTypeID = 4;

                            if (item.ParcelTypeID == 2)
                            {
                                var product = db.ProductTypes.Where(cc => cc.ProductName.Contains("Import Dox")).FirstOrDefault();
                                item.ProductTypeID = product.ProductTypeID;
                            }
                            else
                            {
                                var product = db.ProductTypes.Where(cc => cc.ProductName.Contains("Import NDox")).FirstOrDefault();
                                item.ProductTypeID = product.ProductTypeID;
                            }

                            item.MovementID = 3;
                            item.MovementType = "Import";
                            
                            item.Consignor =  d.shipper.Trim();
                             if (d.cnorTel!=null)
                            item.ConsignorPhone = d.cnorTel.Trim();
                            item.PaymentModeId = 2;
                            item.PaymentMode = "COD";

                            item.ConsignorCountryName = ConsignorCountryName.Trim();
                            item.ConsignorCityName = consignorCityName.Trim();

                            item.ConsigneeCountryName = d.destination.Trim();
                            if (d.destinationCity == "")
                                item.ConsigneeCityName = d.destination;
                            else
                                item.ConsigneeCityName = d.destinationCity.Trim();
                            item.ConsigneeAddress1_Building = d.receiverAddress.Trim();
                            if (d.receiverAddress.IndexOf(':') > 0)
                            {
                                string contact = d.receiverAddress.Split(':')[0];
                                string address = d.receiverAddress.Split(':')[1];
                                item.ConsigneeContact = contact.Trim();
                                item.ConsigneeAddress1_Building = StripHtml(address.Trim());
                            }
                            else
                            {
                                item.ConsigneeContact = "";
                                item.ConsigneeAddress1_Building = d.receiverAddress.Trim();
                                
                            }
                            item.Pieces = CommonFunctions.ParseInt(d.pcs);
                            item.Consignee = d.receiverName.Trim();
                            item.Remarks = StripHtml(d.groupcode.Trim());
                            //item.groupCode = d.groupcode;
                            item.MAWB = d.MAWB;
                            item.Route = d.route;

                            item.ConsigneePhone = d.receiverPhone;
                            item.FAgentID = 6;//Nice Express Services Llc
                            item.Currency = d.currency;
                            var _currency = db.CurrencyMasters.Where(cc => cc.CurrencyName == d.currency).First();
                            item.CurrencyID = _currency.CurrencyID;
                            item.CustomsValue = Convert.ToDecimal(d.customsValue);

                            Details.Add(item);
                            //if (sno ==10)
                            //    break;
                        }


                    }
                }
                InboundAWBBatchModel vm = new InboundAWBBatchModel();
                vm.Details = Details;
                vm.Details = vm.Details.OrderByDescending(cc => cc.SNo).ToList();
                vm.Status = "OK";
                Session["ShipmentList"] = vm.Details;
               // return PartialView("AWBList", vm);
                if (vm.Details.Count == 0)
                {
                    return Json(new { Status = "OK", data = Details, Message = "There is no Shipments to import!" });
                }
                else
                {
                    return Json(new { Status = "OK", data = Details, Message = "Import Shipments Retreived Successfully " });
                }
            }
            else
            {
                InboundAWBBatchModel vm = new InboundAWBBatchModel();
                vm.Details = new List<InboundShipmentModel>();                
                vm.Status = "OK";
                Session["ShipmentList"] = vm.Details;
                if (vm.Details.Count == 0)
                {
                    return Json(new { Status = "OK", data = Details, Message = "There is no Shipments to import!" });
                }
                else
                {
                    return Json(new { Status = "OK", data = Details, Message = "Import Shipments Retreived Successfully " });
                }
            }
           
            
            //return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
        }
        private static string StripHtml(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return System.Text.RegularExpressions.Regex.Replace(s, "<.*?>", string.Empty);
        }
        [HttpPost]
        public async Task<ActionResult> CallPostAPI()
        {

            string companyname = Session["CompanyName"].ToString();
            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
            {
                string URL = "http://www.niceexpress.net/API/v1/postAPI.do";
                //string idate = Convert.ToDateTime(InputDate).ToString("dd-MMM-yyyy hh:mm");
                postbill bills = (postbill)Session["bills"];
                //var json = JsonConvert.SerializeObject(bills);
                //string urlParameters = "?bills=" + json;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    //HTTP GET
                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("niceexpress-apikey", "14c3993c0bb082dcafae9183ac7946d5be7a0e565e12bbd32f5d8d5d78bf3121");
                    client.DefaultRequestHeaders.Add("niceexpress-signature", "37ef0bccb326b2057121ab74fd81cbeee892debaeccdd632d57fff66ffd86ece");

                    // List data response.
                    //var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    //var result  = await client.PostAsync("method",content);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                    //string resultContent = await result.Content.ReadAsStringAsync();
                    var postResponse = await client.PostAsJsonAsync(URL, bills);
                    var response = postResponse.EnsureSuccessStatusCode();
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    // Parse the response body.

                    //    var ItemJsonString = await response.Content.ReadAsStringAsync(); // .ReadAsAsync<MasterDataObject>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                    //    var dataObjects = JsonConvert.DeserializeObject<MasterDataObject>(ItemJsonString);
                    //    // return RedirectToAction("Index");
                    //    return Json(new { Status = "ok",  Message = "API Updated Successfully " });
                    //}
                    //else
                    //{
                    //    return Json(new { Status = "Failed", Message = "API Not Updated Successfully " });
                    //}

                    return Json(new { Status = "ok", Message = "API Updated Successfully " }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Status = "ok", Message = "API Not Updated!" }, JsonRequestBehavior.AllowGet);
            }
            //return "notworked";
        }


        //batch wise excel download
        public ActionResult BatchShipmentDownload(int id)
        {

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt = InboundShipmentDAO.GetColoaderBatchReportExcel(id);

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "BatchShipment_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=" + FileName + ".xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            return RedirectToAction("Index");
        }

        #endregion


    }

    
}