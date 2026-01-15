using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Reflection;
using ExcelDataReader;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class TranshipmentController : Controller
    {
        // GET: Transhipment
        Entities1 db = new Entities1();
        public ActionResult Index()
        {


            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ImportManifestSearch obj = (ImportManifestSearch)Session["TranshipmentSearch"];
            ImportManifestSearch model = new ImportManifestSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<ImportManifestVM> translist = new List<ImportManifestVM>();
                translist = ImportDAO.GetTranshipmentManifestList(4);
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["TranshipmentSearch"] = model;
                List<ImportManifestVM> translist = new List<ImportManifestVM>();
                translist = ImportDAO.GetTranshipmentManifestList(4);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(ImportManifestSearch obj)
        {
            Session["TranshipmentSearch"] = obj;
            return RedirectToAction("Index");
        }

        public ActionResult Create(int id = 0)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);

            var agent = db.CustomerMasters.Where(cc => cc.CustomerType == "CL").ToList();//  db.AgentMasters.ToList(); // .Where(cc => cc.UserID == userid).FirstOrDefault();
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
            ImportManifestVM vm = new ImportManifestVM();
            if (id == 0)
            {
                vm.CompanyCountryName = CompanyCountryName;                
                vm.ManifestDate = CommonFunctions.GetCurrentDateTime().ToString();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID,Convert.ToDateTime(vm.ManifestDate),"T");
                vm.ID = 0;
                vm.TransDetails = new List<TranshipmentModel>();
            }
            else
            {
                vm.CompanyCountryName = CompanyCountryName;
                ImportShipment model = db.ImportShipments.Find(id);
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.ManifestDate = model.CreatedDate.ToString();
                vm.FlightDate1 = model.FlightDate.ToString();
                vm.FlightNo = model.FlightNo;
                vm.MAWB = model.MAWB;
                vm.Route = model.Route;
                vm.ParcelNo = model.ParcelNo;
                vm.Bags = model.Bags;
                vm.Type = model.Type;
                vm.Route = model.Route;
                vm.Weight = model.Weight;
                vm.TotalAWB = model.TotalAWB;
                vm.OriginAirportCity = model.OriginAirportCity;
                vm.DestinationAirportCity = model.DestinationAirportCity;
                vm.AgentID = model.AgentID;
                vm.TransDetails = new List<TranshipmentModel>();
                //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
                var IDetails = ImportDAO.GetTranshipmenItems(vm.ID,"");
                vm.TransDetails = IDetails;
            }

            return View(vm);
        }

        public ActionResult Edit(int id = 0)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);

            var agent = db.CustomerMasters.Where(cc => cc.CustomerType == "CL").ToList();//  db.AgentMasters.ToList(); // .Where(cc => cc.UserID == userid).FirstOrDefault();
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
            ImportManifestVM vm = new ImportManifestVM();
            if (id == 0)
            {
                vm.CompanyCountryName = CompanyCountryName;
                vm.ManifestDate = CommonFunctions.GetCurrentDateTime().ToString();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, Convert.ToDateTime(vm.ManifestDate), "T");
                vm.ID = 0;
                vm.TransDetails = new List<TranshipmentModel>();
            }
            else
            {
                vm.CompanyCountryName = CompanyCountryName;
                ImportShipment model = db.ImportShipments.Find(id);
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.ManifestDate = model.CreatedDate.ToString();
                vm.FlightDate1 = model.FlightDate.ToString();
                vm.FlightNo = model.FlightNo;
                vm.MAWB = model.MAWB;
                vm.Route = model.Route;
                vm.ParcelNo = model.ParcelNo;
                vm.Bags = model.Bags;
                vm.Type = model.Type;
                vm.Route = model.Route;
                vm.Weight = model.Weight;
                vm.TotalAWB = model.TotalAWB;
                vm.OriginAirportCity = model.OriginAirportCity;
                vm.DestinationAirportCity = model.DestinationAirportCity;
                vm.AgentID = model.AgentID;
                Session["TranshipmentAgentID"] = vm.AgentID;
                var AllRateList = AWBDAO.GetAllRateList(vm.AgentID);
                Session["CustomerRateList"] = AllRateList;
                vm.TransDetails = new List<TranshipmentModel>();
                //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
                var IDetails = ImportDAO.GetTranshipmenItems(vm.ID,"-1");
                vm.TransDetails = IDetails;
                vm.TransCountryDetails = ImportDAO.GetTranshipmenCountryList(vm.ID);
                Session["ManifestTranshipment"] = IDetails;
                Session["ManifestTranshipmentCountry"] = vm.TransCountryDetails;
            }

            return View(vm);
        }

        public ActionResult EditCost(int id = 0)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);

            var agent = db.CustomerMasters.Where(cc => cc.CustomerType == "CL").ToList();//  db.AgentMasters.ToList(); // .Where(cc => cc.UserID == userid).FirstOrDefault();
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
            ImportManifestVM vm = new ImportManifestVM();
            if (id == 0)
            {
                vm.CompanyCountryName = CompanyCountryName;
                vm.ManifestDate = CommonFunctions.GetCurrentDateTime().ToString();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, Convert.ToDateTime(vm.ManifestDate), "T");
                vm.ID = 0;
                vm.TransDetails = new List<TranshipmentModel>();
            }
            else
            {
                vm.CompanyCountryName = CompanyCountryName;
                ImportShipment model = db.ImportShipments.Find(id);
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.ManifestDate = model.CreatedDate.ToString();
                vm.FlightDate1 = model.FlightDate.ToString();
                vm.FlightNo = model.FlightNo;
                vm.MAWB = model.MAWB;
                vm.Route = model.Route;
                vm.ParcelNo = model.ParcelNo;
                vm.Bags = model.Bags;
                vm.Type = model.Type;
                vm.Route = model.Route;
                vm.Weight = model.Weight;
                vm.TotalAWB = model.TotalAWB;
                vm.OriginAirportCity = model.OriginAirportCity;
                vm.DestinationAirportCity = model.DestinationAirportCity;
                vm.AgentID = model.AgentID;
                Session["TranshipmentAgentID"] = vm.AgentID;
                var AllRateList = AWBDAO.GetAllRateList(vm.AgentID);
                Session["CustomerRateList"] = AllRateList;
                vm.TransDetails = new List<TranshipmentModel>();
                //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
                var IDetails = ImportDAO.GetTranshipmenItems(vm.ID, "-1");
                vm.TransDetails = IDetails;
                vm.TransCountryDetails = ImportDAO.GetTranshipmenCountryList(vm.ID);
                Session["ManifestTranshipment"] = IDetails;
                Session["ManifestTranshipmentCountry"] = vm.TransCountryDetails;
            }

            return View(vm);
        }
        public ActionResult ShowItemList()
        {
            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            return PartialView("ItemList", vm);
        }
        public ActionResult ShowEditItemList()
        {
            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            return PartialView("EditItemList", vm);
        }
        public ActionResult ShowEditItemList1(int ShipmentId,string CountryName)
        {

            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = new List<TranshipmentModel>();
            //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
            var IDetails = ImportDAO.GetTranshipmenItems(ShipmentId, CountryName);
            vm.TransDetails = IDetails;
            Session["ManifestTranshipment"] = IDetails;
            return PartialView("EditItemList", vm);
        }

        public ActionResult ShowEditCostItemList1(int ShipmentId, string CountryName)
        {

            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = new List<TranshipmentModel>();
            //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
            var IDetails = ImportDAO.GetTranshipmenItems(ShipmentId, CountryName);
            vm.TransDetails = IDetails;
            Session["ManifestTranshipment"] = IDetails;
            return PartialView("EditCostItemList", vm);
        }
        [HttpGet]
        public JsonResult CheckDuplicationTranshipment(string ManifestDate, string MAWB,int AgentID)
        {
            Session["TranshipmentAgentID"] = AgentID;

            
            

            List<TranshipmentAWB> item = PickupRequestDAO.CheckTranshipment(Convert.ToDateTime(ManifestDate), MAWB, "");
            
            if (item.Count>0)
            {
                if (item[0].Status=="Ok")
                {
                    var AllRateList = AWBDAO.GetAllRateList(AgentID);
                    Session["CustomerRateList"] = AllRateList;

                    return Json(new { status="ok"},JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "Failed",Message=item[0].Message }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new { status = "Failed", Message ="" }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                var fileData = GetDataFromCSVFile(importFile.InputStream);
               
                //var duplawb = (from c in fileData join d in db.InScanMasters on c.HAWBNo equals d.AWBNo select c).ToList();
                ImportManifestVM vm = new ImportManifestVM();
                vm.TransDetails = fileData; 
                 Session["ManifestTranshipment"] = vm.TransDetails;
                return Json(new { Status = 1, data = "OK", Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<TranshipmentModel> GetDataFromCSVFile(Stream stream)
        {
            int AgentID = (int)Session["TranshipmentAgentID"];
            var empList = new List<TranshipmentModel>();
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
                        int i = 1;
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {
                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            empList.Add(new TranshipmentModel()
                            {
                                SNo = i++,
                                CustomerId= AgentID,
                                HAWBNo = objDataRow["HAWBNo"].ToString(),
                                AWBDate = objDataRow["AWBDate"].ToString(),
                                Customer = objDataRow["Customer"].ToString(),
                                ConsignorPhone = objDataRow["TelephoneNo"].ToString(),
                                Consignor = objDataRow["Consignor"].ToString(),
                                ConsignorLocationName = objDataRow["ConsignorLocation"].ToString(),
                                ConsignorCountryName = objDataRow["ConsignorCountry"].ToString(),
                                ConsignorCityName = objDataRow["ConsignorCity"].ToString(),
                                Consignee = objDataRow["Consignee"].ToString(),
                                ConsigneeCountryName = objDataRow["ConsigneeCountry"].ToString(),
                                ConsigneeCityName = objDataRow["ConsigneeCity"].ToString(),
                                ConsigneeLocationName = objDataRow["ConsigneeLocation"].ToString(),
                                ConsignorAddress1_Building = objDataRow["ConsignorAddress"].ToString(),
                                ConsigneeAddress1_Building = objDataRow["ConsigneeAddress"].ToString(),
                                ConsignorMobile = objDataRow["ConsignorTelephone"].ToString(),
                                ConsigneeMobile = objDataRow["ConsigneeTelephone"].ToString(),
                                Weight = CommonFunctions.ParseDecimal(objDataRow["Weight"].ToString()),
                                Pieces = objDataRow["Pieces"].ToString(),
                                CourierCharge = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()),
                                OtherCharge = CommonFunctions.ParseDecimal(objDataRow["OtherCharge"].ToString()),
                                PaymentMode = objDataRow["PaymentMode"].ToString(),
                                ReceivedBy = objDataRow["ReceiverName"].ToString(),
                                CollectedBy = objDataRow["CollectedName"].ToString(),
                                FAWBNo = objDataRow["FwdNo"].ToString(),
                                FAgentName = objDataRow["ForwardingAgent"].ToString(),
                                CourierType = objDataRow["Couriertype"].ToString(),
                                ParcelType = objDataRow["ParcelType"].ToString(),
                                MovementType = objDataRow["MovementType"].ToString(),
                                CourierStatus = objDataRow["CourierStatus"].ToString(),
                                remarks = objDataRow["Remarks"].ToString() //Department and Bag no is missing                                                               


                            });
                            //var dupitem=(from c in empList join c2 in InScanMaster on c.awb
                            //AWBNo AWBDate Bag NO.	Shipper ReceiverName    ReceiverContactName ReceiverPhone   ReceiverAddress DestinationLocation DestinationCountry Pcs Weight CustomsValue    COD Content Reference Status  SynchronisedDateTime

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return empList;
        }

        public JsonResult CheckDataValidation()
        {
            List<CustomerRateVM> RateList = (List<CustomerRateVM>)Session["CustomerRateList"];
            List<TranshipmentModel> TransDetail = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            var productype = db.ProductTypes.ToList();
            var parceltype = db.ParcelTypes.ToList();
            var EmpList = db.EmployeeMasters.ToList();
            var fage = db.ForwardingAgentMasters.ToList();
            var CustomerId = Convert.ToInt32(Session["TranshipmentAgentID"].ToString());
            var courierstatus = db.CourierStatus.Where(cc => cc.CourierStatus == "Shipment Delivered").FirstOrDefault();
            int i = 0;
            foreach (var item in TransDetail)
            {
                bool dataError = false;
                string ErrorMessage = "";

                if (item.CourierType != "" && TransDetail[i].ProductTypeID==0)
                {
                    var productype1 = (from p in productype where p.ProductName == item.CourierType select p).FirstOrDefault();
                    if (productype == null)
                    {
                        dataError = true;
                        ErrorMessage = TransDetail[i].ErrorMessage + "Invalid Courier type";
                    }
                    else
                    {
                        TransDetail[i].ProductTypeID = productype1.ProductTypeID;
                    }

                }
                 if (item.ParcelType != "" && TransDetail[i].ParcelTypeID==0)
                {
                    var productype1 = (from p in parceltype where p.ParcelType1 == item.ParcelType select p).FirstOrDefault();
                    if (productype1 == null)
                    {
                        dataError = true;
                        TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                    }
                    else
                    {
                        TransDetail[i].ParcelTypeID = productype1.ID;                        
                    }
                }
                 else if (item.ParcelType == "")
                {
                    dataError = true;
                }
                
                if (item.Weight == null || item.Weight == 0)
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                }
                else if (TransDetail[i].ConsigneeCountryName== null || TransDetail[i].ConsigneeCountryName=="")
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + " Country name";
                }
                else if (item.FAgentName != "" && TransDetail[i].FagentID==0)
                {
                    var fage1 = (from f in fage where f.FAgentName == item.FAgentName select f).FirstOrDefault();
                    if (fage1==null)
                    {
                        dataError = true;
                    }
                    else
                    {
                        TransDetail[i].FagentID = fage1.FAgentID;
                    }
                    
                }
                
                if (item.PaymentMode == "Customer")
                {
                    TransDetail[i].PaymentModeId = 3;
                }
                
                if (item.ReceivedBy != "" && TransDetail[i].DepotReceivedBy==0)
                {
                    var emp = (from e in EmpList where e.EmployeeName == item.ReceivedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].DepotReceivedBy = emp.EmployeeID;
                    }
                }
                
                if (item.CollectedBy != "" && TransDetail[i].PickedUpEmpID==0)
                {
                    var emp =(from e in EmpList where e.EmployeeName == item.CollectedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].PickedUpEmpID = emp.EmployeeID;
                    }

                }
                
                //if (item.CourierStatus != "")
                //{
                  
                //    if (courierstatus != null)
                //    {
                //        TransDetail[i].CourierStatusID = courierstatus.CourierStatusID;
                //        TransDetail[i].StatusTypeId = courierstatus.StatusTypeID;
                //    }
                //}
                //else
                //{
                //    TransDetail[i].DataError = false;
                //}
                if (TransDetail[i].CourierCharge==null)
                {
                    TransDetail[i].CourierCharge = 0;
                }

                if (dataError == false && TransDetail[i].CourierCharge==0)
                {
                    //List<CustomerRateType> lst = GetCustomerRateType("", CustomerId "4", TransDetail[i].ProductTypeID.ToString(), "3", TransDetail[i].FagentID.ToString(), "", TransDetail[i].ConsigneeCountryName, "", "");
                    var rate = (from r in RateList where r.CourierServiceID == TransDetail[i].ProductTypeID && r.MovementID == 4 && r.FAgentID == TransDetail[i].FagentID && r.CountryName == TransDetail[i].ConsigneeCountryName select r).FirstOrDefault();
                    if (rate !=null)
                    {
                        //CustomerRateTypeVM vm = GetCourierCharge(lst[0].CustomerRateTypeID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                        //CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                        if (rate.CustomerRateID > 0)
                        {
                            TransDetail[i].CustomerRateID = rate.CustomerRateID; // vm.CustomerRateTypeID;

                            CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                            if (vm.CourierCharge != null && vm.CourierCharge != 0)
                            {
                                TransDetail[i].CourierCharge = vm.CourierCharge;
                                TransDetail[i].DataError = false;
                            }
                            else
                            {
                                TransDetail[i].DataError = true;
                            }
                        }
                        else
                        {
                            TransDetail[i].DataError = true;
                        }
                    }
                    else
                    {
                        TransDetail[i].DataError = true;
                    }

                    TransDetail[i].OtherCharge = 0;
                    
                }
                else if(TransDetail[i].CourierCharge==0)
                {
                    TransDetail[i].DataError = true;
                }
                else
                {
                    TransDetail[i].DataError = false; 
                }
                Session["ProcessingRow"] = i;
                Session["ManifestTranshipment"] = TransDetail;
                i++;
            }
           
            var itemcount = (from c in TransDetail where c.DataError == true select c).ToList();
            if (itemcount.Count > 0)
            {
                return Json(new { status = "Falied",Message="Invalid Data" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            

        }



        public string CheckDataValidation1()
        {
            List<CustomerRateVM> RateList = (List<CustomerRateVM>)Session["CustomerRateList"];
            List<TranshipmentModel> TransDetail = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            var productype = db.ProductTypes.ToList();
            var parceltype = db.ParcelTypes.ToList();
            var EmpList = db.EmployeeMasters.ToList();
            var fage = db.ForwardingAgentMasters.ToList();
            var CustomerId = Convert.ToInt32(Session["TranshipmentAgentID"].ToString());
            var courierstatus = db.CourierStatus.Where(cc => cc.CourierStatus == "Shipment Delivered").FirstOrDefault();
            int i = 0;
            foreach (var item in TransDetail)
            {
                bool dataError = false;
                string ErrorMessage = "";

                if (item.CourierType != "")
                {
                    var productype1 = (from p in productype where p.ProductName == item.CourierType select p).FirstOrDefault();
                    if (productype == null)
                    {
                        dataError = true;
                        ErrorMessage = TransDetail[i].ErrorMessage + "Invalid Courier type";
                    }
                    else
                    {
                        TransDetail[i].ProductTypeID = productype1.ProductTypeID;
                    }

                }
                if (item.ParcelType != "")
                {
                    var productype1 = (from p in parceltype where p.ParcelType1 == item.ParcelType select p).FirstOrDefault();
                    if (productype1 == null)
                    {
                        dataError = true;
                        TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                    }
                    else
                    {
                        TransDetail[i].ParcelTypeID = productype1.ID;
                    }
                }
                else if (item.ProductType == "")
                {
                    dataError = true;
                }

                if (item.Weight == null || item.Weight == 0)
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                }
                else if (TransDetail[i].ConsigneeCountryName == null || TransDetail[i].ConsigneeCountryName == "")
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + " Country name";
                }
                else if (item.FAgentName != "")
                {
                    var fage1 = (from f in fage where f.FAgentName == item.FAgentName select f).FirstOrDefault();
                    if (fage1 == null)
                    {
                        dataError = true;
                    }
                    else
                    {
                        TransDetail[i].FagentID = fage1.FAgentID;
                    }

                }

                if (item.PaymentMode == "Customer")
                {
                    TransDetail[i].PaymentModeId = 3;
                }

                if (item.ReceivedBy != "")
                {
                    var emp = (from e in EmpList where e.EmployeeName == item.ReceivedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].DepotReceivedBy = emp.EmployeeID;
                    }
                }

                if (item.CollectedBy != "")
                {
                    var emp = (from e in EmpList where e.EmployeeName == item.CollectedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].PickedUpEmpID = emp.EmployeeID;
                    }

                }

                if (item.CourierStatus != "")
                {

                    if (courierstatus != null)
                    {
                        TransDetail[i].CourierStatusID = courierstatus.CourierStatusID;
                        TransDetail[i].StatusTypeId = courierstatus.StatusTypeID;
                    }
                }
                else
                {
                    TransDetail[i].DataError = false;
                }
                if (TransDetail[i].CourierCharge == null)
                {
                    TransDetail[i].CourierCharge = 0;
                }

                //if (dataError == false && TransDetail[i].CourierCharge == 0)
                //{
                //    //List<CustomerRateType> lst = GetCustomerRateType("", CustomerId "4", TransDetail[i].ProductTypeID.ToString(), "3", TransDetail[i].FagentID.ToString(), "", TransDetail[i].ConsigneeCountryName, "", "");
                //    var rate = (from r in RateList where r.CourierServiceID == TransDetail[i].ProductTypeID && r.MovementID == 4 && r.FAgentID == TransDetail[i].FagentID && r.CountryName == TransDetail[i].ConsigneeCountryName select r).FirstOrDefault();
                //    if (rate != null)
                //    {
                //        //CustomerRateTypeVM vm = GetCourierCharge(lst[0].CustomerRateTypeID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                //        //CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                //        if (rate.CustomerRateID > 0)
                //        {
                //            TransDetail[i].CustomerRateID = rate.CustomerRateID; // vm.CustomerRateTypeID;

                //            CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                //            if (vm.CourierCharge != null && vm.CourierCharge != 0)
                //            {
                //                TransDetail[i].CourierCharge = vm.CourierCharge;
                //                TransDetail[i].DataError = false;
                //            }
                //            else
                //            {
                //                TransDetail[i].DataError = true;
                //            }
                //        }
                //        else
                //        {
                //            TransDetail[i].DataError = true;
                //        }
                //    }
                //    else
                //    {
                //        TransDetail[i].DataError = true;
                //    }

                //    TransDetail[i].OtherCharge = 0;

                //}
                //else
                //{
                //    TransDetail[i].DataError = true;
                //}
                Session["ProcessingRow"] = i;
                Session["ManifestTranshipment"] = TransDetail;
                i++;
            }

            var itemcount = (from c in TransDetail where c.DataError == true select c).ToList();
            
            if (itemcount.Count > 0)
            {
                return "Failed";
            }
            else
            {
                return "ok";
            }


        }

        [HttpPost]
        public JsonResult CheckEditDataValidation(bool AllItem,string Details)
        {
            List<CustomerRateVM> RateList = (List<CustomerRateVM>)Session["CustomerRateList"];
            List<TranshipmentModel> TransDetail = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            var productype = db.ProductTypes.ToList();
            var parceltype = db.ParcelTypes.ToList();
            var EmpList = db.EmployeeMasters.ToList();
            var fage = db.ForwardingAgentMasters.ToList();
            var CustomerId = Convert.ToInt32(Session["TranshipmentAgentID"].ToString());
            var courierstatus = db.CourierStatus.Where(cc => cc.CourierStatus == "Shipment Delivered").FirstOrDefault();
            int i = 0;
            foreach (var item in TransDetail)
            {
                bool dataError = false;
                string ErrorMessage = "";

                if (item.CourierType != "" && TransDetail[i].ProductTypeID == 0)
                {
                    var productype1 = (from p in productype where p.ProductName == item.CourierType select p).FirstOrDefault();
                    if (productype == null)
                    {
                        dataError = true;
                        ErrorMessage = TransDetail[i].ErrorMessage + "Invalid Courier type";
                    }
                    else
                    {
                        TransDetail[i].ProductTypeID = productype1.ProductTypeID;
                    }

                }
                if (item.ParcelType != "" && TransDetail[i].ParcelTypeID == 0)
                {
                    var productype1 = (from p in parceltype where p.ParcelType1 == item.ParcelType select p).FirstOrDefault();
                    if (productype1 == null)
                    {
                        dataError = true;
                        TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                    }
                    else
                    {
                        TransDetail[i].ParcelTypeID = productype1.ID;
                    }
                }
                else if (item.ParcelType == "")
                {
                    dataError = true;
                }

                if (item.Weight == null || item.Weight == 0)
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + "Parcel Type";
                }
                else if (TransDetail[i].ConsigneeCountryName == null || TransDetail[i].ConsigneeCountryName == "")
                {
                    dataError = true;
                    TransDetail[i].ErrorMessage = TransDetail[i].ErrorMessage + " Country name";
                }
                else if (item.FAgentName != "" && TransDetail[i].FagentID == 0)
                {
                    var fage1 = (from f in fage where f.FAgentName == item.FAgentName select f).FirstOrDefault();
                    if (fage1 == null)
                    {
                        dataError = true;
                    }
                    else
                    {
                        TransDetail[i].FagentID = fage1.FAgentID;
                    }

                }

                if (item.PaymentMode == "Customer")
                {
                    TransDetail[i].PaymentModeId = 3;
                }

                if (item.ReceivedBy != "" && TransDetail[i].DepotReceivedBy == 0)
                {
                    var emp = (from e in EmpList where e.EmployeeName == item.ReceivedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].DepotReceivedBy = emp.EmployeeID;
                    }
                }

                if (item.CollectedBy != "" && TransDetail[i].PickedUpEmpID == 0)
                {
                    var emp = (from e in EmpList where e.EmployeeName == item.CollectedBy select e).FirstOrDefault();
                    if (emp != null)
                    {
                        TransDetail[i].PickedUpEmpID = emp.EmployeeID;
                    }

                }

                //if (item.CourierStatus != "")
                //{

                //    if (courierstatus != null)
                //    {
                //        TransDetail[i].CourierStatusID = courierstatus.CourierStatusID;
                //        TransDetail[i].StatusTypeId = courierstatus.StatusTypeID;
                //    }
                //}
                //else
                //{
                //    TransDetail[i].DataError = false;
                //}
                if (TransDetail[i].CourierCharge == null)
                {
                    TransDetail[i].CourierCharge = 0;
                }

                if (dataError == false && TransDetail[i].CourierCharge == 0)
                {
                    //List<CustomerRateType> lst = GetCustomerRateType("", CustomerId "4", TransDetail[i].ProductTypeID.ToString(), "3", TransDetail[i].FagentID.ToString(), "", TransDetail[i].ConsigneeCountryName, "", "");
                    var rate = (from r in RateList where r.CourierServiceID == TransDetail[i].ProductTypeID && r.MovementID == 4 && r.FAgentID == TransDetail[i].FagentID && r.CountryName == TransDetail[i].ConsigneeCountryName select r).FirstOrDefault();
                    if (rate != null)
                    {
                        //CustomerRateTypeVM vm = GetCourierCharge(lst[0].CustomerRateTypeID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                        //CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                        if (rate.CustomerRateID > 0)
                        {
                            TransDetail[i].CustomerRateID = rate.CustomerRateID; // vm.CustomerRateTypeID;

                            CustomerRateTypeVM vm = GetCourierCharge(rate.CustomerRateID.ToString(), TransDetail[i].CustomerId.ToString(), "4", TransDetail[i].ProductTypeID.ToString(), "3", item.Weight.ToString(), TransDetail[i].ConsigneeCountryName, "");
                            if (vm.CourierCharge != null && vm.CourierCharge != 0)
                            {
                                TransDetail[i].CourierCharge = vm.CourierCharge;
                                TransDetail[i].DataError = false;
                            }
                            else
                            {
                                TransDetail[i].DataError = true;
                            }
                        }
                        else
                        {
                            TransDetail[i].DataError = true;
                        }
                    }
                    else
                    {
                        TransDetail[i].DataError = true;
                    }

                    TransDetail[i].OtherCharge = 0;

                }
                else if (TransDetail[i].CourierCharge == 0)
                {
                    TransDetail[i].DataError = true;
                }
                else
                {
                    TransDetail[i].DataError = false;
                }
                Session["ProcessingRow"] = i;
                Session["ManifestTranshipment"] = TransDetail;
                i++;
            }

            var itemcount = (from c in TransDetail where c.DataError == true select c).ToList();
            if (itemcount.Count > 0)
            {
                return Json(new { status = "Falied", Message = "Invalid Data" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }


        }
        [HttpPost]
        public JsonResult GetProcessedStatus()
        {
            List<TranshipmentModel> TransDetail = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            var TotalCount = (from c in TransDetail select c).ToList();
            var processedcount = (from c in TransDetail where c.CourierCharge > 0 select c).ToList();
            int ProcessingRow = Convert.ToInt32(Session["ProcessingRow"]);
            return Json(new { TotalRow =TotalCount, ProcessedCount= processedcount,ProcessingRow=ProcessingRow}, JsonRequestBehavior.AllowGet);

        }
        public List<CustomerRateType> GetCustomerRateType(string term, string CustomerId, string MovementId, string ProductTypeID, string PaymentModeId, string FAgentID, string CityName, string CountryName, string OriginCountry, string OriginCity)
        {
            int pCustomerId = 0;
            int pMovementId = 0;
            int pProductTypeID = 0;
            int pPaymentModeId = 0;
            int pAgentID = 0;
            string pCountryname = ""; ;
            string pcityname = "";

            if (CustomerId != "")
                pCustomerId = Convert.ToInt32(CustomerId);

            if (MovementId != "")
                pMovementId = Convert.ToInt32(MovementId);

            if (ProductTypeID != "")
                pProductTypeID = Convert.ToInt32(ProductTypeID);

            if (PaymentModeId != "")
                pPaymentModeId = Convert.ToInt32(PaymentModeId);

            if (FAgentID != "")
                pAgentID = Convert.ToInt32(FAgentID);

            if (pMovementId == 3)
            {
                pCountryname = OriginCountry;
                pcityname = OriginCity;
            }
            else
            {
                pCountryname = CountryName;
                pcityname = CityName;

            }

            List<CustomerRateType> lst = new List<CustomerRateType>();
            var loc = AWBDAO.GetRateList(pCustomerId, pMovementId, pProductTypeID, pPaymentModeId, pAgentID, pcityname, pCountryname);

            if (term.Trim() != "")
            {
                lst = (from c in loc where c.CustomerRateType1.Contains(term) orderby c.CustomerRateType1 select c).ToList();
            }
            else
            {
                lst = (from c in loc orderby c.CustomerRateType1 select c).ToList();
            }
            return lst;
        }
        public CustomerRateTypeVM GetCourierCharge(string CustomerRateTypeID, string CustomerId, string MovementId, string ProductTypeID, string PaymentModeId, string Weight, string CountryName, string CityName)
        {
            int pRateTypeID = 0;
            int pCustomerId = 0;
            int pMovementId = 0;
            int pProductTypeID = 0;
            int pPaymentModeId = 0;
            decimal pWeight = 0;
            if (CustomerRateTypeID != "")
            {
                pRateTypeID = Convert.ToInt32(CustomerRateTypeID);
            }
            if (CustomerId != "")
                pCustomerId = Convert.ToInt32(CustomerId);

            if (MovementId != "")
                pMovementId = Convert.ToInt32(MovementId);

            if (ProductTypeID != "")
                pProductTypeID = Convert.ToInt32(ProductTypeID);

            if (PaymentModeId != "")
                pPaymentModeId = Convert.ToInt32(PaymentModeId);

            if (Weight != "")
                pWeight = Convert.ToDecimal(Weight);

            CustomerRateTypeVM vm = AWBDAO.GetCourierCharge(pRateTypeID, pCustomerId, pMovementId, pProductTypeID, pPaymentModeId, pWeight, CountryName, CityName);

            return vm;
        }
        [HttpPost]
        public string SaveImport(string Master, string Details)
        {
            try
            {
                int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int DepotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                var userid = Convert.ToInt32(Session["UserID"]);
                var model = JsonConvert.DeserializeObject<ImportManifestVM>(Master);
                //var IDetails = JsonConvert.DeserializeObject<List<ImportManifestItem>>(Details);
                var IDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
                string ConsignorCountryName = "";
                string ConsignorCityName = "";
                string ConsignorLocation = "";
                string ConsignorPhone = "";
                string ConsignorMobile = "";
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataSet(IDetails);

                ImportShipment importShipment = new ImportShipment();
                if (model.ID == 0)
                {
                    importShipment.ManifestNumber = model.ManifestNumber;
                    importShipment.CreatedDate = Convert.ToDateTime(model.ManifestDate);
                    importShipment.ManifestNumber = ImportDAO.GetMaxManifestNo(companyid, BranchID, importShipment.CreatedDate, "T");
                    
                    importShipment.ShipmentTypeId = 4;
                }
                else
                {
                    importShipment = db.ImportShipments.Find(model.ID);
                }
                importShipment.Bags = model.Bags;
                importShipment.FlightNo = model.FlightNo;
                if (model.FlightDate1 != null && model.FlightDate1!="")
                {
                    importShipment.FlightDate = Convert.ToDateTime(model.FlightDate1);
                }
               // importShipment.LastEditedByLoginID = userid;
                importShipment.MAWB = model.MAWB;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Type = "";
                importShipment.Status = 1;
                importShipment.DestinationAirportCity = model.DestinationAirportCity;
                importShipment.OriginAirportCity = model.OriginAirportCity;
                importShipment.AcFinancialYearID = FyearId;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Bags = model.Bags;
                importShipment.ParcelNo = model.ParcelNo;
                importShipment.AgentID = model.AgentID;
                importShipment.Weight = model.Weight;
                importShipment.Route = model.Route;
                importShipment.AgentLoginID = 1;
                //importShipment.LastEditedByLoginID = 1;

                if (model.AgentID > 0)
                {
                    var customer = db.CustomerMasters.Find(model.AgentID);
                    if (customer != null)
                    {
                        ConsignorCountryName = customer.CountryName;
                        ConsignorCityName = customer.CityName;
                        ConsignorLocation = customer.LocationName;
                        if (customer.Phone!=null)
                        ConsignorPhone = customer.Phone;
                        if (customer.Mobile != null)
                            ConsignorMobile = customer.Mobile;
                        
                    }
                }
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

                    //var details = (from d in db.ImportShipmentDetails where d.ImportID == model.ID select d).ToList();
                    //db.ImportShipmentDetails.RemoveRange(details);
                    //db.SaveChanges();

                }
                //DataTable ds = new DataTable();
                //DataSet dt = new DataSet();                
                //dt = ToDataTable(IDetails);                
                //string xml = dt.GetXml();
                //AWBDAO.SaveTranshipmentUpload(importShipment.ID,1,1,1,1,1,)
                var courierstatus = db.CourierStatus.Where(cc => cc.CourierStatus == "Shipment Delivered").FirstOrDefault();
                foreach (var item in IDetails)
                {
                    InScanMaster detail = new InScanMaster();
                    InScanInternationalDeatil isid = new InScanInternationalDeatil();
                    InScanInternational isi = new InScanInternational();

                    if (item.InScanID > 0)
                        detail = db.InScanMasters.Find(item.InScanID);

                    detail.ImportShipmentId = importShipment.ID; //forignkey reference for import tranashipment
                    detail.AWBNo = item.HAWBNo;
                    detail.TransactionDate = Convert.ToDateTime(item.AWBDate);
                    detail.Consignor = item.Consignor;
                    detail.ConsignorPhone = item.ConsignorPhone;
                    detail.ConsignorMobileNo = item.ConsignorMobile;
                    detail.Consignee = item.Consignee;
                    detail.ConsigneeAddress1_Building = item.ConsigneeAddress1_Building;                    
                    detail.ConsigneeCountryName = item.ConsigneeCountryName;
                    detail.ConsigneeCityName = item.ConsigneeCityName;                    
                    detail.ConsigneeMobileNo = item.ConsigneeMobile;
                    detail.ParcelTypeId = item.ParcelTypeID;
                    detail.CustomerRateID = item.CustomerRateID;
                    detail.ProductTypeID =item.ProductTypeID;
                    detail.MovementID = 4;

                    //if (item.CourierType !="")
                    //{
                    //    var productype = db.ProductTypes.Where(cc => cc.ProductName == item.CourierType).FirstOrDefault();
                    //    if (productype != null)
                    //    {
                    //        detail.ProductTypeID = productype.ProductTypeID;
                    //    }

                    //}
                    //if (item.ProductType!= "")
                    //{
                    //    var productype = db.ParcelTypes.Where(cc => cc.ParcelType1 == item.ProductType).FirstOrDefault();
                    //    if (productype != null)
                    //    {
                    //        detail.ParcelTypeId = productype.ParcelTypeID;
                    //    }

                    //}

                    if (item.Weight != null)
                        detail.Weight = Convert.ToDecimal(item.Weight);

                    if (item.Pieces != null)
                        detail.Pieces = item.Pieces;

                    if (item.CourierCharge != null)
                        detail.CourierCharge = Convert.ToDecimal(item.CourierCharge);
                    else
                        detail.CourierCharge = 0;

                    if (item.OtherCharge != null)
                        detail.OtherCharge = Convert.ToDecimal(item.OtherCharge);
                    else
                        detail.OtherCharge = 0;

                    detail.NetTotal = detail.CourierCharge + detail.OtherCharge;

                    if (item.FAgentName != "" && item.FagentID==0)
                    {
                        var fage = db.ForwardingAgentMasters.Where(cc => cc.FAgentName == item.FAgentName).FirstOrDefault();
                        detail.FAgentId = fage.FAgentID;
                    }
                    else
                    {
                        detail.FAgentId = item.FagentID;
                    }

                    detail.CustomerID = importShipment.AgentID;
                    detail.ConsignorCountryName = ConsignorCountryName;
                    detail.ConsignorCityName = ConsignorCityName;
                    detail.ConsignorLocationName = ConsignorLocation;
                    if (detail.ConsignorMobileNo=="" || detail.ConsignorMobileNo==null)
                        detail.ConsignorMobileNo = ConsignorMobile;
                    if (item.PaymentMode=="Customer")
                    {
                        detail.PaymentModeId = 3;
                    }
                    else { detail.PaymentModeId = 3; }
                    if (item.ReceivedBy != "" && item.DepotReceivedBy == 0)
                    {
                        var emp = db.EmployeeMasters.Where(cc => cc.EmployeeName == item.ReceivedBy).FirstOrDefault();
                        if (emp != null)
                        {
                            detail.DepotReceivedBy = emp.EmployeeID;
                        }

                    }
                    else {
                        detail.DepotReceivedBy = item.DepotReceivedBy;
                    }
                    if (item.CollectedBy != "" && item.PickedUpEmpID==0)
                    {
                        var emp = db.EmployeeMasters.Where(cc => cc.EmployeeName == item.CollectedBy).FirstOrDefault();
                        if (emp != null)
                        {
                            detail.PickedUpEmpID = emp.EmployeeID;
                        }

                    }
                    else
                    {
                        detail.PickedUpEmpID = item.PickedUpEmpID;
                    }
                    detail.CourierStatusID = courierstatus.CourierStatusID;
                    detail.StatusTypeId = courierstatus.StatusTypeID;

                    if (item.InScanID == 0)
                    {
                        detail.IsDeleted = false;
                        detail.BranchID = BranchID;
                        detail.DepotID = DepotId;
                        detail.CreatedBy = userid;
                        detail.LastModifiedBy = userid;
                        detail.CreatedDate = CommonFunctions.GetCurrentDateTime();
                        detail.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                        detail.AcFinancialYearID = FyearId;
                        detail.AcCompanyID= companyid;    
                        db.InScanMasters.Add(detail);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Entry(detail).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (detail.FAgentId != 0)
                    {
                        isid = db.InScanInternationalDeatils.Where(cc => cc.InScanID == detail.InScanID).FirstOrDefault();
                        isi = db.InScanInternationals.Where(cc => cc.InScanID == detail.InScanID).FirstOrDefault();
                        if (isid == null)
                        {
                            isid = new InScanInternationalDeatil();
                            isid.InScanID = detail.InScanID;
                            isid.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isid.VerifiedWeight = detail.Weight;
                        }
                        if (isi == null)
                        {
                            isi = new InScanInternational();
                            isi.InScanID = detail.InScanID;
                            isi.FAgentID = Convert.ToInt32(detail.FAgentId);
                            isi.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isi.StatusAssignment = false;
                            isi.ForwardingAWBNo = item.FAWBNo;
                            isi.ForwardingDate = detail.TransactionDate; // DateTime.UtcNow;
                            isi.VerifiedWeight = Convert.ToDouble(detail.Weight);

                        }

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
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }



        public string EditSaveImport(string Master, string Details)
        {
            try
            {
                int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int DepotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                var userid = Convert.ToInt32(Session["UserID"]);
                var model = JsonConvert.DeserializeObject<ImportManifestVM>(Master);
                //var IDetails = JsonConvert.DeserializeObject<List<ImportManifestItem>>(Details);
                var IDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
                ImportShipment importShipment = new ImportShipment();
                if (model.ID == 0)
                {
                    importShipment.ManifestNumber = model.ManifestNumber;
                    importShipment.CreatedDate = Convert.ToDateTime(model.ManifestDate);
                    importShipment.ManifestNumber = ImportDAO.GetMaxManifestNo(companyid, BranchID, importShipment.CreatedDate, "T");

                    importShipment.ShipmentTypeId = 4;
                }
                else
                {
                    importShipment = db.ImportShipments.Find(model.ID);
                }
                importShipment.Bags = model.Bags;
                importShipment.FlightNo = model.FlightNo;
                if (model.FlightDate1 != null && model.FlightDate1 != "")
                {
                    importShipment.FlightDate = Convert.ToDateTime(model.FlightDate1);
                }
               // importShipment.LastEditedByLoginID = userid;
                importShipment.MAWB = model.MAWB;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Type = "";
                importShipment.Status = 1;
                importShipment.DestinationAirportCity = model.DestinationAirportCity;
                importShipment.OriginAirportCity = model.OriginAirportCity;
                importShipment.AcFinancialYearID = FyearId;
                importShipment.TotalAWB = model.TotalAWB;
                importShipment.Bags = model.Bags;
                importShipment.ParcelNo = model.ParcelNo;
                importShipment.AgentID = model.AgentID;
                importShipment.Weight = model.Weight;
                importShipment.Route = model.Route;
                importShipment.AgentLoginID = 1;
               // importShipment.LastEditedByLoginID = 1;

                if (model.ID == 0)
                {
                    db.ImportShipments.Add(importShipment);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(importShipment).State = EntityState.Modified;
                    db.SaveChanges();

                    //var details = (from d in db.ImportShipmentDetails where d.ImportID == model.ID select d).ToList();
                    //db.ImportShipmentDetails.RemoveRange(details);
                    //db.SaveChanges();

                }
                //DataTable ds = new DataTable();
                //DataSet dt = new DataSet();                
                //dt = ToDataTable(IDetails);                
                //string xml = dt.GetXml();
                //AWBDAO.SaveTranshipmentUpload(importShipment.ID,1,1,1,1,1,)
                var courierstatus = db.CourierStatus.Where(cc => cc.CourierStatus == "Shipment Delivered").FirstOrDefault();
                foreach (var item in IDetails)
                {
                    InScanMaster detail = new InScanMaster();
                    InScanInternationalDeatil isid = new InScanInternationalDeatil();
                    InScanInternational isi = new InScanInternational();

                    if (item.InScanID > 0)
                        detail = db.InScanMasters.Find(item.InScanID);
                    if (detail == null)
                    {
                        item.InScanID = 0;
                        detail = new InScanMaster();
                           
                        }

                    detail.ImportShipmentId = importShipment.ID; //forignkey reference for import tranashipment
                    detail.AWBNo = item.HAWBNo;
                    detail.TransactionDate = Convert.ToDateTime(item.AWBDate);
                    detail.Consignee = item.Consignee;
                    detail.ConsigneeAddress1_Building = item.ConsigneeAddress1_Building;
                    detail.ConsignorPhone = item.ConsignorPhone;
                    detail.ConsigneeCountryName = item.ConsigneeCountryName;
                    detail.ConsigneeCityName = item.ConsigneeCityName;
                    detail.Consignor = item.Consignor;

                    detail.ParcelTypeId = item.ParcelTypeID;
                    detail.CustomerRateID = item.CustomerRateID;
                    detail.ProductTypeID = item.ProductTypeID;
                    detail.MovementID = 4;                    

                    if (item.Weight != null)
                        detail.Weight = Convert.ToDecimal(item.Weight);

                    if (item.Pieces != null)
                        detail.Pieces = item.Pieces;

                    if (item.CourierCharge != null)
                        detail.CourierCharge = Convert.ToDecimal(item.CourierCharge);
                    else
                        detail.CourierCharge = 0;

                    if (item.OtherCharge != null)
                        detail.OtherCharge = Convert.ToDecimal(item.OtherCharge);
                    else
                        detail.OtherCharge = 0;

                    detail.NetTotal = detail.CourierCharge + detail.OtherCharge;

                    if (item.FAgentName != "" && item.FagentID == 0)
                    {
                        var fage = db.ForwardingAgentMasters.Where(cc => cc.FAgentName == item.FAgentName).FirstOrDefault();
                        detail.FAgentId = fage.FAgentID;
                    }
                    else
                    {
                        detail.FAgentId = item.FagentID;
                    }

                    detail.CustomerID = importShipment.AgentID;
                    
                   
                    if (item.ReceivedBy != "" && item.DepotReceivedBy == 0)
                    {
                        var emp = db.EmployeeMasters.Where(cc => cc.EmployeeName == item.ReceivedBy).FirstOrDefault();
                        if (emp != null)
                        {
                            detail.DepotReceivedBy = emp.EmployeeID;
                        }

                    }
                    else
                    {
                        detail.DepotReceivedBy = item.DepotReceivedBy;
                    }
                    if (item.CollectedBy != "" && item.PickedUpEmpID == 0)
                    {
                        var emp = db.EmployeeMasters.Where(cc => cc.EmployeeName == item.CollectedBy).FirstOrDefault();
                        if (emp != null)
                        {
                            detail.PickedUpEmpID = emp.EmployeeID;
                        }

                    }
                    else
                    {
                        detail.PickedUpEmpID = item.PickedUpEmpID;
                    }
                    detail.CourierStatusID = courierstatus.CourierStatusID;
                    detail.StatusTypeId = courierstatus.StatusTypeID;

                    if (item.InScanID == 0 )
                    {
                        detail.IsDeleted = false;
                        detail.BranchID = BranchID;
                        detail.DepotID = DepotId;
                        detail.CreatedBy = userid;
                        detail.LastModifiedBy = userid;
                        detail.CreatedDate = CommonFunctions.GetCurrentDateTime();
                        detail.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                        detail.AcFinancialYearID = FyearId;
                        detail.AcCompanyID = companyid;
                        db.InScanMasters.Add(detail);
                        db.SaveChanges();
                    }
                    else
                    {
                        detail.LastModifiedBy = userid;
                        detail.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                        db.Entry(detail).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (detail.FAgentId != 0)
                    {
                        isid = db.InScanInternationalDeatils.Where(cc => cc.InScanID == detail.InScanID).FirstOrDefault();
                        isi = db.InScanInternationals.Where(cc => cc.InScanID == detail.InScanID).FirstOrDefault();
                        if (isid == null)
                        {
                            isid = new InScanInternationalDeatil();
                            isid.InScanID = detail.InScanID;
                            isid.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isid.VerifiedWeight = detail.Weight;
                        }
                        else
                        {
                            isid.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isid.VerifiedWeight = detail.Weight;
                        }
                        if (isi == null)
                        {
                            isi = new InScanInternational();
                            isi.InScanID = detail.InScanID;
                            isi.FAgentID = Convert.ToInt32(detail.FAgentId);
                            isi.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isi.StatusAssignment = false;
                            isi.ForwardingAWBNo = item.FAWBNo;
                            isi.ForwardingDate = detail.TransactionDate; // DateTime.UtcNow;
                            isi.VerifiedWeight = Convert.ToDouble(detail.Weight);

                        }
                        else
                        {
                            isi.FAgentID = Convert.ToInt32(detail.FAgentId);
                            isi.ForwardingCharge = Convert.ToDecimal(item.ForwardingCharge);
                            isi.StatusAssignment = false;
                            isi.ForwardingAWBNo = item.FAWBNo;
                            isi.ForwardingDate = detail.TransactionDate; // DateTime.UtcNow;
                            isi.VerifiedWeight = Convert.ToDouble(detail.Weight);

                        }

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
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        [HttpPost]
        public JsonResult GetImportItem(int id)
        {
            List<ImportManifestItem> details = new List<ImportManifestItem>();
            details = (from c in db.ImportShipmentDetails where c.ImportID == id select new ImportManifestItem { AWBNo = c.AWB, AWBDate = c.AWBDate.ToString(), Shipper = c.Shipper, Receiver = c.Receiver, ReceiverContact = c.ReceiverContact, ReceiverAddress = c.ReceiverAddress, ReceiverPhone = c.ReceiverTelephone, Content = c.Contents, Bag = c.BagNo.ToString(), Weight = c.Weight.ToString(), Pcs = c.PCS, Value = c.CustomValue.ToString(), COD = c.COD.ToString(), DestinationCountry = c.DestinationCountry, DestinationLocation = c.DestinationLocation, DestinationCity = c.DestinationCity, ImportType = c.ImportType }).ToList();

            return Json(new { data = details });


        }
        [HttpPost]
        public ActionResult ShowImportDataFixation(string FieldName)
        {
            ImportManifestFixation vm = new ImportManifestFixation();
            ViewBag.ImportFields = db.ImportFields.ToList();
            return PartialView("ImportDataFixation", vm);
        }

        [HttpPost]
        public string UpdateImportedItem(string Details)
        {
            var IDetails = JsonConvert.DeserializeObject<List<ImportManifestItem>>(Details);
            Session["ManifestImported"] = IDetails;
            return "ok";
        }
        [HttpPost]
        public ActionResult UpdateDataFixation(string TargetColumn, string SourceValue, string TargetValue)
        {
            ImportManifestVM model = new ImportManifestVM();
            List<TranshipmentModel> Details = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            if (TargetColumn == "ConsigneeCountryName")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.ConsigneeCountryName == SourceValue)
                    {
                        Details[i].ConsigneeCountryName = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "ConsigneeCityName")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.ConsigneeCityName == SourceValue)
                    {
                        Details[i].ConsigneeCityName = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "Customer")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.Customer == SourceValue)
                    {
                        Details[i].Customer = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "ForwardingAgent" || TargetColumn == "FAgentName")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.FAgentName == SourceValue)
                    {
                        Details[i].FAgentName = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "ReceivedBy")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.ReceivedBy == SourceValue)
                    {
                        Details[i].ReceivedBy = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "CollectedBy")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.CollectedBy == SourceValue)
                    {
                        Details[i].CollectedBy = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "CourierType")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.CourierType == SourceValue)
                    {
                        Details[i].CourierType = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "ParcelType")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.ParcelType == SourceValue || item.ParcelType==null)
                    {
                        Details[i].ParcelType = TargetValue;
                    }
                    i++;
                }
            }
            
            SaveDataFixation(TargetColumn, SourceValue, TargetValue);
            model.TransDetails = Details;
            Session["ManifestTranshipment"] = Details;
            return View("ItemList", model);
        }

        /// <summary>
        /// this will udpate fixation in to importdatafixation table
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="SourceValue"></param>
        /// <param name="TargetValue"></param>
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

        public string GetDataFixation(List<ImportDataFixation> importdata,  string FieldName, string SourceValue)
        {
            //ImportDataFixation importdata = new ImportDataFixation();
            string Targetvalue = "";
            //var data = db.ImportDataFixations.Where(cc => cc.ShipmentType == "Transhipment" && cc.FieldName == FieldName && cc.SourceValue == SourceValue).FirstOrDefault();
            var data = (from c in importdata where c.ShipmentType == "Transhipment" && c.FieldName == FieldName && c.SourceValue.ToLower().Trim() == SourceValue.ToLower().Trim() select c).FirstOrDefault();
            if (data != null)
                Targetvalue = data.TargetValue;

            return Targetvalue;


        }

        [HttpPost]
        public string AutoDataFixation()
        {
            ImportManifestVM model = new ImportManifestVM();
            List<TranshipmentModel> Details = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            DataTable dt = ToDataTable(Details);
            string sourcecol = "";
            ImportDataFixation importdata = new ImportDataFixation();            
            var data = db.ImportDataFixations.ToList();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var colname = dt.Columns[i].ColumnName.Trim();
                
                if (colname != "SNo")
                {
                    if (colname == "ConsigneeCountryName")
                        sourcecol = "FAgentName";

                    int rowindex = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[colname] == null)
                            row[colname] = "";

                        string targetvalue = GetDataFixation(data, colname, row[colname].ToString().Trim());
                        if (targetvalue != "")
                        {
                            dt.Rows[rowindex][colname] = targetvalue;
                        }

                        rowindex++;
                    }
                }

            }
            List<TranshipmentModel> list = TranshipmentList(dt);
            Session["ManifestTranshipment"] = list;
            model.TransDetails = list;
            string result=CheckDataValidation1();
            
            return result;
            

        }

        [HttpGet]
        public JsonResult GetSourceValue(string term, string FieldName)
        {
            var IDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            var matchlist = db.ImportDataFixations.Where(cc => cc.FieldName == FieldName).ToList();
            if (IDetails != null)
            {
                if (term.Trim() != "")
                {
                    
                    if (FieldName == "DestinationCountry" || FieldName =="ConsigneeCountryName")
                    {                        
                        
                        var list = (from c in IDetails
                                    where c.ConsigneeCountryName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeCountryName
                                    select new { SourceValue = c.ConsigneeCountryName }).Distinct().ToList();
                        //if (matchlist != null && matchlist.Count > 0)
                        //{
                        //    var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                        //    return Json(listnew, JsonRequestBehavior.AllowGet);
                        //}
                        //else
                        //{
                        //    return Json(list, JsonRequestBehavior.AllowGet);
                        //}

                        return Json(list, JsonRequestBehavior.AllowGet);

                    }
                    else if (FieldName == "DestinationCity" ||  FieldName=="ConsigneeCityName")
                    {
                        var list = (from c in IDetails
                                    where c.ConsigneeCityName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeCityName
                                    select new { SourceValue = c.ConsigneeCityName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "DestinationLocation" || FieldName =="ConsigneeLocationName")
                    {
                        var list = (from c in IDetails
                                    where c.ConsigneeLocationName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ConsigneeLocationName
                                    select new { SourceValue = c.ConsigneeLocationName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "FAgentName")
                    {
                        var list = (from c in IDetails
                                    where c.FAgentName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.FAgentName
                                    select new { SourceValue = c.FAgentName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "Customer")
                    {
                        var list = (from c in IDetails
                                    where c.Customer.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.Customer
                                    select new { SourceValue = c.Customer }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "ReceivedBy")
                    {
                        var list = (from c in IDetails
                                    where c.ReceivedBy.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ReceivedBy
                                    select new { SourceValue = c.ReceivedBy }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "CollectedBy")
                    {
                        var list = (from c in IDetails
                                    where c.CollectedBy.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.CollectedBy
                                    select new { SourceValue = c.CollectedBy }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "CourierStatus")
                    {
                        var list = (from c in IDetails
                                    where c.CourierStatus.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.CourierStatus
                                    select new { SourceValue = c.CourierStatus }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "CourierType")
                    {
                        var list = (from c in IDetails
                                    where c.CourierType.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.CourierType
                                    select new { SourceValue = c.CourierType}).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "ParcelType")
                    {
                        var list = (from c in IDetails
                                    where c.ParcelType.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ProductType
                                    select new { SourceValue = c.ParcelType }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

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
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "DestinationCity" || FieldName == "ConsigneeCityName")
                    {
                        var list = (from c in IDetails
                                    orderby c.ConsigneeCityName
                                    select new { SourceValue = c.ConsigneeCityName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "DestinationLocation" || FieldName == "ConsigneeLocationName")
                    {
                        var list = (from c in IDetails
                                    orderby c.ConsigneeLocationName
                                    select new { SourceValue = c.ConsigneeLocationName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "FAgentName")
                    {
                        var list = (from c in IDetails
                                    orderby c.FAgentName
                                    select new { SourceValue = c.FAgentName }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "Customer")
                    {
                        var list = (from c in IDetails
                                    orderby c.Customer
                                    select new { SourceValue = c.Customer }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (FieldName == "ReceivedBy")
                    {
                        var list = (from c in IDetails
                                    orderby c.ReceivedBy
                                    select new { SourceValue = c.ReceivedBy }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "CollectedBy") { 
                        var list = (from c in IDetails
                                    orderby c.CollectedBy
                                    select new { SourceValue = c.CollectedBy }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if(FieldName=="CourierStatus")
                    {
                        var list = (from c in IDetails
                                    orderby c.CourierStatus
                                    select new { SourceValue = c.CourierStatus }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "CourierType")
                    {
                        var list = (from c in IDetails                                    
                                    orderby c.CourierType
                                    select new { SourceValue = c.CourierType }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else if (FieldName == "ParcelType")
                    {
                        var list = (from c in IDetails                                    
                                    orderby c.ParcelType
                                    select new { SourceValue = c.ParcelType }).Distinct().ToList();
                        if (matchlist != null && matchlist.Count > 0)
                        {
                            var listnew = list.Where(p => !matchlist.Any(p2 => p2.TargetValue == p.SourceValue));
                            return Json(listnew, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(list, JsonRequestBehavior.AllowGet);
                        }

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
        [HttpGet]
        public JsonResult GetTargetValue(string term, string FieldName)
        {
            var IDetails = (List<TranshipmentModel>)Session["ManifestTranshipment"];
            if (IDetails != null)
            {
                if (term.Trim() != "")
                {
                    if (FieldName == "ReceivedBy" || FieldName=="CollectedBy")
                    {
                        var list = (from c in db.EmployeeMasters
                                    where c.EmployeeName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.EmployeeName
                                    select new { SourceValue = c.EmployeeName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "CourierStatus")
                    {
                        var list = (from c in db.CourierStatus
                                    where c.CourierStatus.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.CourierStatus
                                    select new { SourceValue = c.CourierStatus }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "CourierType")
                    {
                        var list = (from c in db.ProductTypes
                                    where c.ProductName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ProductName
                                    select new { SourceValue = c.ProductName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                        //var list = (from c in db.CustomerRateTypes
                        //            where c.CustomerRateType1.ToLower().Contains(term.Trim().ToLower())
                        //            orderby c.CustomerRateType1
                        //            select new { SourceValue = c.CustomerRateType1 }).Distinct().ToList();
                        //return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "ParcelType")
                    {                     

                        var list = (from c in db.ParcelTypes
                                    where c.ParcelType1.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.ParcelType1
                                    select new { SourceValue = c.ParcelType1 }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "FAgentName")
                    {
                        var list = (from c in db.ForwardingAgentMasters
                                    where c.FAgentName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.FAgentName
                                    select new { SourceValue = c.FAgentName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "Customer")
                    {
                        var list = (from c in db.CustomerMasters
                                    where c.CustomerName.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.CustomerName
                                    select new { SourceValue = c.CustomerName }).Distinct().ToList().Take(100);
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
                    if (FieldName == "ReceivedBy" || FieldName == "CollectedBy")
                    {
                        var list = (from c in db.EmployeeMasters                                    
                                    orderby c.EmployeeName
                                    select new { SourceValue = c.EmployeeName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "CourierStatus")
                    {
                        var list = (from c in db.CourierStatus                                    
                                    orderby c.CourierStatus
                                    select new { SourceValue = c.CourierStatus }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "FAgentName")
                    {
                        var list = (from c in db.ForwardingAgentMasters
                                    orderby c.FAgentName
                                    select new { SourceValue = c.FAgentName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "Customer")
                    {
                        var list = (from c in db.CustomerMasters
                                    orderby c.CustomerName
                                    select new { SourceValue = c.CustomerName }).Distinct().ToList().Take(100);
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "CourierType") //--Productytpe
                    {
                        var list = (from c in db.ProductTypes

                                    orderby c.ProductName
                                    select new { SourceValue = c.ProductName }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                        //var list = (from c in db.CustomerRateTypes                                    
                        //            orderby c.CustomerRateType1
                        //            select new { SourceValue = c.CustomerRateType1 }).Distinct().ToList();
                        //return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "ParcelType")
                    {
                        var list = (from c in db.ParcelTypes

                                    orderby c.ParcelType1
                                    select new { SourceValue = c.ParcelType1 }).Distinct().ToList();
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

        [HttpGet]
        public string GetManifestNo(string ManifestDateTime)
        {
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);
            DateTime mDateTime = CommonFunctions.GetCurrentDateTime();
            try
            {
                mDateTime = Convert.ToDateTime(ManifestDateTime);
            }
            catch (Exception ex)
            {
                mDateTime = CommonFunctions.GetCurrentDateTime();
            }

            string manifestno = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, mDateTime, "T");
            return manifestno;

        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
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
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public static DataSet ToDataSet<T>(List<T> items)
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
        public static List<TranshipmentModel> TranshipmentList(DataTable dt)
        {
            var empList = new List<TranshipmentModel>();
            int i = 1;
            foreach (DataRow objDataRow in dt.Rows)
            {
                empList.Add(new TranshipmentModel()
                {
                    SNo = i++,
                    HAWBNo = objDataRow["HAWBNo"].ToString(),
                    AWBDate = objDataRow["AWBDate"].ToString(),
                    Customer = objDataRow["Customer"].ToString(),
                    ConsignorPhone = objDataRow["ConsignorPhone"].ToString(),
                    Consignor = objDataRow["Consignor"].ToString(),
                    ConsignorLocationName = objDataRow["ConsignorLocationName"].ToString(),
                    ConsignorCountryName = objDataRow["ConsignorCountryName"].ToString(),
                    ConsignorCityName = objDataRow["ConsignorCityName"].ToString(),
                    Consignee = objDataRow["Consignee"].ToString(),
                    ConsigneeCountryName = objDataRow["ConsigneeCountryName"].ToString(),
                    ConsigneeCityName = objDataRow["ConsigneeCityName"].ToString(),
                    ConsigneeLocationName = objDataRow["ConsigneeLocationName"].ToString(),
                    ConsignorAddress1_Building = objDataRow["ConsignorAddress1_Building"].ToString(),
                    ConsignorMobile = objDataRow["ConsignorMobile"].ToString(),
                    ConsigneeMobile = objDataRow["ConsigneeMobile"].ToString(), //ConsigneeTelephone

                    Weight = CommonFunctions.ParseDecimal(objDataRow["Weight"].ToString()),
                    Pieces = objDataRow["Pieces"].ToString(),
                    CourierCharge = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()),
                    OtherCharge = CommonFunctions.ParseDecimal(objDataRow["OtherCharge"].ToString()),
                    PaymentMode = objDataRow["PaymentMode"].ToString(),
                    ReceivedBy = objDataRow["ReceivedBy"].ToString(),
                    CollectedBy = objDataRow["CollectedBy"].ToString(),
                    FAWBNo = objDataRow["FAWBNo"].ToString(),
                    FAgentName = objDataRow["FAgentName"].ToString(),
                    CourierType = objDataRow["CourierType"].ToString(),
                    ParcelType = objDataRow["ParcelType"].ToString(),
                    MovementType = objDataRow["MovementType"].ToString(),
                    CourierStatus = objDataRow["CourierStatus"].ToString(),
                    remarks = objDataRow["remarks"].ToString(), //Department and Bag no is missing                                                               
                    DataError = Convert.ToBoolean(objDataRow["DataError"].ToString()),
                    InScanID= Convert.ToInt32(objDataRow["InScanID"].ToString()),
                });
            }
            return empList;
        }

        #region CreateBulk
        public ActionResult CreateBulk(int id = 0)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            var CompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
            var BranchID = Convert.ToInt32(Session["CurrentBranchID"]);

            var agent = db.CustomerMasters.Where(cc => cc.CustomerType == "CL").ToList();//  db.AgentMasters.ToList(); // .Where(cc => cc.UserID == userid).FirstOrDefault();
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
            ImportManifestVM vm = new ImportManifestVM();
            if (id == 0)
            {
                vm.CompanyCountryName = CompanyCountryName;
                vm.ManifestDate = CommonFunctions.GetCurrentDateTime().ToString();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, Convert.ToDateTime(vm.ManifestDate), "T");
                vm.ID = 0;
                vm.TransDetails = new List<TranshipmentModel>();
            }
            else
            {
                vm.CompanyCountryName = CompanyCountryName;
                ImportShipment model = db.ImportShipments.Find(id);
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.ManifestDate = model.CreatedDate.ToString();
                vm.FlightDate1 = model.FlightDate.ToString();
                vm.FlightNo = model.FlightNo;
                vm.MAWB = model.MAWB;
                vm.Route = model.Route;
                vm.ParcelNo = model.ParcelNo;
                vm.Bags = model.Bags;
                vm.Type = model.Type;
                vm.Route = model.Route;
                vm.Weight = model.Weight;
                vm.TotalAWB = model.TotalAWB;
                vm.OriginAirportCity = model.OriginAirportCity;
                vm.DestinationAirportCity = model.DestinationAirportCity;
                vm.AgentID = model.AgentID;
                vm.TransDetails = new List<TranshipmentModel>();
                //var IDEtails = (from c in db.InScanMasters where c.ImportShipmentId == vm.ID select new TranshipmentModel { InScanID=c.InScanID, HAWBNo =c.AWBNo,AWBDate=c.TransactionDate.ToString("dd-MM-yyyy"),Consignor=c.Consignor,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName,Consignee=c.Consignee,ConsigneeCityName=c.ConsigneeCityName,Weight=c.Weight,Pieces=c.Pieces,CourierCharge=c.CourierCharge,OtherCharge=c.OtherCharge}).ToList();
                var IDetails = ImportDAO.GetTranshipmenItems(vm.ID,"");
                vm.TransDetails = IDetails;
            }

            return View(vm);
        }

        [HttpPost]
        public ActionResult GetBulkItemList(string MAWBNo, int AgentID)
        {
            Session["TranshipmentAgentID"] = AgentID;
            //int AgentID = (int)Session["TranshipmentAgentID"];
            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = ImportDAO.GetBulkTranshipmenItems(MAWBNo,AgentID);// (List<TranshipmentModel>)Session["ManifestTranshipment"];
            return PartialView("ItemList", vm);
        }
        #endregion


        [HttpPost]
        public ActionResult EditUpdateDataFixation(int ShipmentId,int AgentID,string TargetColumn, string SourceValue, string TargetValue)
        {

            string result=ImportDAO.EditManualDataFixation(ShipmentId, AgentID, TargetColumn, SourceValue, TargetValue);
            ImportManifestVM model = new ImportManifestVM();
            
            model.TransCountryDetails = ImportDAO.GetTranshipmenCountryList(ShipmentId);
            var IDetails = ImportDAO.GetTranshipmenItems(ShipmentId, model.TransCountryDetails[0].CountryName);
            model.TransDetails = IDetails;           

            Session["ManifestTranshipment"] = IDetails;
            Session["ManifestTranshipmentCountry"] = model.TransCountryDetails;         
            return View("EditCountryList", model);
        }
        

        [HttpPost]
        public ActionResult EditRateFixation(int ShipmentId, string CountryName)
        {

            string result = ImportDAO.EditManualRateFixation(ShipmentId, CountryName);
            ImportManifestVM model = new ImportManifestVM();

            model.TransCountryDetails = ImportDAO.GetTranshipmenCountryList(ShipmentId);
            var IDetails = ImportDAO.GetTranshipmenItems(ShipmentId, model.TransCountryDetails[0].CountryName);
            model.TransDetails = IDetails;

            Session["ManifestTranshipment"] = IDetails;
            Session["ManifestTranshipmentCountry"] = model.TransCountryDetails;
            return View("EditCountryList", model);
        }


        [HttpPost]
        public ActionResult EditCostFixation(int ShipmentId, string CountryName)
        {

            string result = ImportDAO.EditManualCostFixation(ShipmentId, CountryName);
            ImportManifestVM model = new ImportManifestVM();

            model.TransCountryDetails = ImportDAO.GetTranshipmenCountryList(ShipmentId);
            var IDetails = ImportDAO.GetTranshipmenItems(ShipmentId, model.TransCountryDetails[0].CountryName);
            model.TransDetails = IDetails;

            Session["ManifestTranshipment"] = IDetails;
            Session["ManifestTranshipmentCountry"] = model.TransCountryDetails;
            return View("EditCountryList", model);
        }


        public ActionResult ShowCountryList()
        {
            ImportManifestVM vm = new ImportManifestVM();
            vm.TransDetails = (List<TranshipmentModel>)Session["ManifestTranshipmentCountry"];
            return PartialView("EditCountryList", vm);
        }

    }

    public class DataObject
        {
            public int code { get; set; }
            public List<productdata> data { get; set; }
            public string message { get; set; }

        }

        public class productdata
        {
            public string shipper { get; set; }
            public string pcs { get; set; }
            public string awbNo { get; set; }

            public string receiverName { get; set; }

            public string customsValue { get; set; }

            public string awbDate { get; set; }
            public string destination { get; set; }

            public string weight { get; set; }
            public string lastStatusRemark { get; set; }

            public string destinationCountry { get; set; }
            public string content { get; set; }
            public string reference { get; set; }

            public string destinationCity { get; set; }

            public string receiverAddress { get; set; }
            public string receiverPhone { get; set; }

            public string cod { get; set; }
            public string currency { get; set; }

            public string id { get; set; }

            public string bagNo { get; set; }

            public string status { get; set; }
        }
    
}