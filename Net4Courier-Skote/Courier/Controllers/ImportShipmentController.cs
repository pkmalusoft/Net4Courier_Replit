using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using Newtonsoft.Json;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ImportShipmentController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: ExportShipment
        public ActionResult Index()
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ImportShipmentSearch obj = (ImportShipmentSearch)Session["ImportShipmentSearch"];
            int Fyearid = Convert.ToInt32(Session["fyearid"]);
            ImportShipmentSearch model = new ImportShipmentSearch();
            ExportDAO _dao = new ExportDAO();
            if (obj != null)
            {
                List<ImportShipmentVM> translist = new List<ImportShipmentVM>();
                translist = ExportDAO.GetImportShipmentManifestList(0); 
                //model.FromDate = obj.FromDate;
                //model.ToDate = obj.ToDate;
                model.FromDate = obj.FromDate;
                StatusModel statu = AccountsDAO.CheckDateValidate(obj.FromDate.ToString(), Fyearid);
                string vdate = statu.ValidDate;
                model.FromDate = Convert.ToDateTime(vdate);
                model.ToDate = Convert.ToDateTime(AccountsDAO.CheckDateValidate(obj.ToDate.ToString(), Fyearid).ValidDate);
                model.AWBNo = obj.AWBNo;
                Session["ImportShipmentSearch"] = model;
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetFirstDayofWeek().Date; // CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                model.AWBNo = "";
                Session["ImportShipmentSearch"] = model;
                List<ImportShipmentVM> translist = new List<ImportShipmentVM>();
                translist = ExportDAO.GetImportShipmentManifestList(0); 
                model.Details = translist;

            }
            return View(model);

        }
        [HttpPost]
        public ActionResult Index(ImportShipmentSearch obj)
        {
            Session["ImportShipmentSearch"] = obj;
            return RedirectToAction("Index");
        }
        public ActionResult Create(int id = 0, string InputDate = "")
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);

            var agent = db.AgentMasters.ToList(); // .Where(cc => cc.UserID == userid).FirstOrDefault();
            var company = db.AcCompanies.FirstOrDefault(); // .Select(x => new { Address = x.Address1 
            string selectedVal = ""; ;
            var types = new List<SelectListItem>
            {
                new SelectListItem{Text = "Select Shipment Type", Value = null, Selected = selectedVal == null},
                new SelectListItem{Text = "Transhipment", Value = "Transhipment", Selected = selectedVal == "Transhipment"},
                new SelectListItem{Text = "Import", Value = "Import", Selected = selectedVal == "Import"},
            };

            ViewBag.Type = types;
            var currency = new SelectList(db.CurrencyMasters.OrderBy(x => x.CurrencyName), "CurrencyID", "CurrencyName").ToList();
            ViewBag.CurrencyID = db.CurrencyMasters.ToList();  // db.CurrencyMasters.ToList();
            ViewBag.Currencies = db.CurrencyMasters.ToList();
            ViewBag.Agent = agent;
            string CompanyCountryName = db.BranchMasters.Find(BranchID).CountryName;
            //ViewBag.AgentName = agent.Name;
            //ViewBag.AgentCity = agent.CityName;
            ViewBag.CompanyName = company.AcCompany1;

            ImportShipmentVM vm = new ImportShipmentVM();
            vm.Details = new List<ImportShipmentDetailVM>();
            if (id == 0)
            {
                vm.CreatedDate = CommonFunctions.GetCurrentDateTime();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, Convert.ToDateTime(vm.CreatedDate), "I");
                vm.ID = 0;
            }
            else
            {
             
                ImportShipment model = db.ImportShipments.Find(id);
                
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.CreatedDate = model.CreatedDate;
                //vm.FlightDate1 = "";// model.FlightDate.ToString();
                vm.FlightNo = model.FlightNo;
                vm.MAWB = model.MAWB;
                //vm.Route = model.Route;
                //vm.ParcelNo = model.ParcelNo;
                vm.Bags = model.Bags;
                vm.Type = model.Type;
                //vm.Route = model.Route;
                //vm.Weight = model.Weight;
                vm.TotalAWB = model.TotalAWB;
                vm.OriginAirportCity = model.OriginAirportCity;
                vm.DestinationAirportCity = model.DestinationAirportCity;
                vm.AgentID = model.AgentID;
                var IDetails = (from c in db.ImportShipmentDetails join cu in db.CurrencyMasters on c.CurrencyID equals cu.CurrencyID where c.ImportID == vm.ID select new ImportShipmentDetailVM { AWB = c.AWB, Shipper = c.Shipper, BagNo = c.BagNo, COD = c.COD, Contents = c.Contents, DestinationCity = c.DestinationCity, DestinationCountry = c.DestinationCountry, PCS = c.PCS, Receiver = c.Receiver, ReceiverAddress = c.ReceiverAddress}).ToList();
                int i = 0;
                foreach (var item in IDetails)
                {
                    IDetails[i].Sno = i + 1;
                    i++;
                }
                Session["ShipmentImported"] = IDetails;
                vm.Details = IDetails;

            }



            return View(vm);
        }
        [HttpPost]
        public JsonResult SaveImport(string Master, string Details)
        {
            try
            {
                int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                var bills = new List<updateitem>();
                var userid = Convert.ToInt32(Session["UserID"]);
                int yearid = Convert.ToInt32(Session["fyearid"].ToString());
                var model = JsonConvert.DeserializeObject<ImportManifestVM>(Master);
                int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                //var IDetails = JsonConvert.DeserializeObject<List<ImportManifestItem>>(Details);
                var IDetails = (List<ImportShipmentDetailVM>)Session["ImportShipmentManifest"];
                ImportShipment importShipment = new ImportShipment();
                if (model.ID == 0)
                {

                    importShipment.CreatedDate = Convert.ToDateTime(model.ManifestDate);
                    importShipment.ShipmentTypeId = 3;
                    importShipment.ManifestNumber = ImportDAO.GetMaxManifestNo(companyid, BranchID, importShipment.CreatedDate, "I");
                }
                else
                {
                    importShipment = db.ImportShipments.Find(model.ID);
                }
                importShipment.CreatedDate = Convert.ToDateTime(model.ManifestDate);
                importShipment.Bags = model.Bags;
                importShipment.FlightNo = model.FlightNo;
                if (model.FlightDate1 != "" && model.FlightDate1 != null)
                    importShipment.FlightDate = Convert.ToDateTime(model.FlightDate1);
               // importShipment.LastEditedByLoginID = userid;
                importShipment.MAWB = model.MAWB;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Type = "";
                importShipment.Status = 1;
                importShipment.DestinationAirportCity = model.DestinationAirportCity;
                importShipment.OriginAirportCity = model.OriginAirportCity;
                importShipment.AcFinancialYearID = yearid;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Bags = model.Bags;
                importShipment.ParcelNo = model.ParcelNo;
                importShipment.AgentID = model.AgentID;
                importShipment.Weight = model.Weight;
                importShipment.Route = model.Route;
                importShipment.AgentLoginID = 1;
                //importShipment.LastEditedByLoginID = 1;
                importShipment.BranchID = BranchID;
                if (model.ID == 0)
                {
                    importShipment.EnteredDate = CommonFunctions.GetCurrentDateTime();
                    importShipment.CreatedBy = userid;
                    importShipment.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    importShipment.ModifiedBy = userid;
                    db.ImportShipments.Add(importShipment);
                    db.SaveChanges();
                }
                else
                {
                    importShipment.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    importShipment.ModifiedBy = userid;
                    db.Entry(importShipment).State = EntityState.Modified;
                    db.SaveChanges();

                    var details = (from d in db.ImportShipmentDetails where d.ImportID == model.ID select d).ToList();
                    db.ImportShipmentDetails.RemoveRange(details);
                    db.SaveChanges();

                }

                foreach (var item in IDetails)
                {
                    var checkshipment = db.ImportShipmentDetails.Where(cc => cc.AWB == item.AWB).FirstOrDefault();
                    if (checkshipment == null)
                    {
                        ImportShipmentDetail detail = new ImportShipmentDetail();
                        detail.ImportID = importShipment.ID;
                        detail.AWB = item.AWB;
                        detail.AWBDate = Convert.ToDateTime(item.AWBDate);
                        detail.Shipper = item.Shipper;
                        detail.Contents = item.Contents;
                        detail.Receiver = item.Receiver;
                        if (item.ReceiverAddress == null)
                            detail.ReceiverAddress = "";
                        else
                            detail.ReceiverAddress = item.ReceiverAddress;
                        if (item.ReceiverContact == null)
                        {
                            detail.ReceiverContact = "";
                        }
                        else
                        {
                            detail.ReceiverContact = item.ReceiverContact;
                        }

                        detail.ReceiverTelephone = item.ReceiverTelephone;
                        detail.DestinationCountry = item.DestinationCountry;
                        detail.DestinationCity = item.DestinationCity;
                        detail.DestinationLocation = item.DestinationLocation;
                        detail.ImportType = "Import";
                        detail.Route = "";
                        detail.GroupCode = "";
                        if (item.MAWB == "" || item.MAWB == null)
                        {
                            if (importShipment.MAWB == "")
                                detail.MAWB = importShipment.ManifestNumber;
                            else
                                detail.MAWB = importShipment.MAWB;
                        }
                        else
                        {
                            detail.MAWB = item.MAWB;
                        }
                        if (detail.ImportType == "Import")
                        {
                            detail.StatusTypeId = 9; //--Import inscan
                            detail.CourierStatusID = 12;  //At Destination Customs Facility
                        }
                        else
                        {
                            detail.StatusTypeId = 9; //import inscan
                            detail.CourierStatusID = 21;
                        }

                        //if (item.PCS != null)
                        detail.PCS = Convert.ToInt32(item.PCS);

                        //if (item.Weight != "")
                        detail.Weight = Convert.ToDecimal(item.Weight);

                        //if (item.COD != "")
                        detail.COD = Convert.ToDecimal(item.COD);
                        detail.BagNo = item.BagNo;
                        //detail.CustomValue = Convert.ToDecimal(item.Value);
                        //var currency = db.CurrencyMasters.Where(cc => cc.CurrencyName == item.Currency).FirstOrDefault();
                        //if (currency != null)
                        //    detail.CurrencyID = currency.CurrencyID;
                        //else

                        detail.CurrencyID = 1; //USD default change on Aug 4 2021


                        db.ImportShipmentDetails.Add(detail);
                        db.SaveChanges();

                        //AWB Track status
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();
                        _awbstatus = db.AWBTrackStatus.Where(cc => cc.AWBNo == detail.AWB && cc.CourierStatusId == 12).FirstOrDefault();
                        if (_awbstatus == null)
                        {
                            _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = detail.AWB;
                            _awbstatus.EntryDate = importShipment.CreatedDate; // DateTime.UtcNow; // importShipment.CreatedDate;
                            _awbstatus.ShipmentDetailID = detail.ShipmentDetailID;
                            _awbstatus.StatusTypeId = Convert.ToInt32(detail.StatusTypeId);
                            _awbstatus.CourierStatusId = Convert.ToInt32(detail.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(detail.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(detail.CourierStatusID).CourierStatus;
                            _awbstatus.UserId = userid;
                            _awbstatus.EmpID = db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault().EmployeeID;
                            _awbstatus.APIStatus = false;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                        }
                        
                    }

                }
                //Update import shipmentdetailid to Exportshipment Details table
                ReceiptDAO.UpdateImportIDtoExport(importShipment.ID);
                return Json(new { status = "ok", data = bills });
            }
            catch (Exception ex)
            {
                //return ex.Message;
                return Json(new { status = "Failed", Message = ex.Message });

            }
        }
          
        [HttpPost]
        public ActionResult ShowExportItemList(int ExportShipmentId)
        {
            
            ImportShipmentVM vm = new ImportShipmentVM();
            try
            {
                List<ImportShipmentDetailVM> list = new List<ImportShipmentDetailVM>();

                list = ExportDAO.GetExportedManifestShipmentList(ExportShipmentId);

                
                vm.Details = list;
                Session["ImportShipmentManifest"] = list;


            }
            catch (Exception ex)
            {
                throw;
            }
            return PartialView("ItemList", vm);


        }

        public JsonResult GetMAWBList(string term, int AgentId)
        {

            List<ShipmentInvoiceVM> lst = ExportDAO.GetExportShipmentMAWBList(AgentId);
            if (term.Trim() != "")
            {
                var list = lst.Where(cc => cc.MAWB.Contains(term.Trim())).OrderBy(cc => cc.MAWB).Take(25).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = lst.OrderBy(cc => cc.MAWB).Take(25).ToList();
            }

            return Json(lst, JsonRequestBehavior.AllowGet);

        }
        // GET: ImportShipment/Details/5
        public ActionResult Details(int? id)
        {
            //AuthHelp Token = repos.Authenticate();
            //if (Token.Status)
            //{
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var s_ImportShipment = db.ImportShipments.Where(x => x.ID == id)
                .FirstOrDefault();
            if (s_ImportShipment == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Edit = Token.Permissions.Updation;
            return View(s_ImportShipment);
            //}
            //return RedirectToAction(Token.Function, Token.Controller);
        }

        // GET: ImportShipment/Create
      

    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddOrRemoveShipment(ImportShipmentFormModel s_ImportShipment, int? i)
    {
            var Prevshipmentsession = Session["PreviousShipments"] as List<ImportShipmentDetail>;

            if (i.HasValue)
        {
                if (Prevshipmentsession == null)
                {

                }
                else
                {
                    foreach (var item in Prevshipmentsession)
                    {
                        s_ImportShipment.Shipments.Add(item);
                    }
                }
                s_ImportShipment.Shipments.RemoveAt(i.Value);
                Session["PreviousShipments"] = s_ImportShipment.Shipments;
            }
            else
        {
            if (s_ImportShipment.Shipments == null)
            {
                s_ImportShipment.Shipments = new List<ImportShipmentDetail>();
            }
            var shipmentsession = Session["Shipmentdetails"] as ImportShipmentDetail;
            var Serialnumber = Convert.ToInt32(Session["ShipSerialNumber"]);
            var isupdate = Convert.ToBoolean(Session["IsUpdate"]);
                if (Prevshipmentsession == null)
                {

                }
                else
                {
                    foreach (var item in Prevshipmentsession)
                    {
                        s_ImportShipment.Shipments.Add(item);
                    }
                }
                if (isupdate == true)
            {
                //s_ImportShipment.Shipments.RemoveAt(Serialnumber);                  
                s_ImportShipment.Shipments[Serialnumber] = shipmentsession;
            }
            else
            {
                s_ImportShipment.Shipments.Add(shipmentsession);
            }
            Session["Shipmentdetails"] = new ImportShipmentDetail();
            Session["ShipSerialNumber"] = "";
            Session["IsUpdate"] = false;
        }
        ViewBag.Cities = db.CityMasters.ToList();
        ViewBag.Countries = db.CountryMasters.ToList();
        ViewBag.DestinationCountryID = db.CountryMasters.ToList();
            ViewBag.CurrencyID = db.CurrencyMasters.ToList();
            ViewBag.Currencies = db.CurrencyMasters.ToList();

            return PartialView("ShipmentList", s_ImportShipment);
    }
    public bool AddShippmentToTable(FormCollection data)
    {
        var shipmentmodel = new ImportShipmentDetail();
        shipmentmodel.CurrencyID = Convert.ToInt32(data["tCurrencyID"]);
        shipmentmodel.AWB = data["tAWB"];        
        shipmentmodel.BagNo = data["tBagNo"];
        shipmentmodel.PCS = Convert.ToInt32(data["tPCS"]);
        shipmentmodel.Weight = Convert.ToDecimal(data["tWeight"]);
        shipmentmodel.CustomValue = Convert.ToDecimal(data["tValue"]);
        shipmentmodel.Shipper = data["tShipper"];
        shipmentmodel.Receiver = data["tReciver"];
        shipmentmodel.Contents = data["tContents"];
        shipmentmodel.DestinationCountry = data["tDestinationCountryID"];
        shipmentmodel.DestinationCity = data["tDestinationCityID"];
        shipmentmodel.ShipmentDetailID = Convert.ToInt32(data["tId"]);
        Session["Shipmentdetails"] = shipmentmodel;
        Session["ShipSerialNumber"] = Convert.ToInt32(data["tSerialNum"]);
        Session["IsUpdate"] = Convert.ToBoolean(data["isupdate"]);
        return true;
    }

    public JsonResult GetShipmentDetails(ImportShipmentFormModel s_ImportShipment, int? i)
    {
        if (i.HasValue)
        {
            var s = s_ImportShipment.Shipments[i.Value];
            return Json(new { success = true, data = s, ival = i.Value }, JsonRequestBehavior.AllowGet);
        }
        else
        {
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

    }

    public ActionResult ImportShipmentPrint(int id)
       {
            ViewBag.ReportName = "ImportShipment Printing";
            AccountsReportsDAO.ImportShipmentReport(id);
            //AccountsReportsDAO.CustomerTaxInvoiceReport(id, monetaryunit);
            return View();
     }
       
        // POST: ImportShipment/Delete/5
        [ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
   {

            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteInterBranchImport(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        //if (dt.Rows[0][0] == "OK")
                        TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return RedirectToAction("Index");

        }

      
       
    }
}
