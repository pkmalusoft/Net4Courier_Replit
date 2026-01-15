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
    public class ImportManifestController : Controller
    {
        // GET: ImportManifest
        Entities1 db = new Entities1();
        public ActionResult Index()
        {

            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ImportManifestSearch obj = (ImportManifestSearch)Session["ImportManifestSearch"];
            ImportManifestSearch model = new ImportManifestSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<ImportManifestVM> translist = new List<ImportManifestVM>();
                translist = ImportDAO.GetImportManifestList(3);
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.AWBNo = obj.AWBNo;
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetFirstDayofWeek().Date; // CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                model.AWBNo = "";
                Session["ImportManifestSearch"] = model;
                List<ImportManifestVM> translist = new List<ImportManifestVM>();
                translist = ImportDAO.GetImportManifestList(3);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(ImportManifestSearch obj)
        {
            Session["ImportManifestSearch"] = obj;
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

            ImportManifestVM vm = new ImportManifestVM();
            vm.Details = new List<ImportManifestItem>();
            if (id == 0)
            {
                vm.EnableAPI = company.EnableAPI;
                vm.CompanyCountryName = CompanyCountryName;
                vm.ManifestDate = CommonFunctions.GetCurrentDateTime().ToString();
                vm.ManifestNumber = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, Convert.ToDateTime(vm.ManifestDate), "I");
                vm.ID = 0;
            }
            else
            {
                vm.CompanyCountryName = CompanyCountryName;
                ImportShipment model = db.ImportShipments.Find(id);
                vm.EnableAPI = company.EnableAPI;
                vm.ID = model.ID;
                vm.ManifestNumber = model.ManifestNumber;
                vm.ManifestDate = model.CreatedDate.ToString();
                vm.FlightDate1 = "";// model.FlightDate.ToString();
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
                var IDetails = (from c in db.ImportShipmentDetails join cu in db.CurrencyMasters on c.CurrencyID equals cu.CurrencyID where c.ImportID == vm.ID select new ImportManifestItem { AWBNo = c.AWB, Shipper = c.Shipper, Bag = c.BagNo, COD = c.COD.ToString(), Content = c.Contents, DestinationCity = c.DestinationCity, DestinationCountry = c.DestinationCountry, Pcs = c.PCS, Receiver = c.Receiver, ReceiverAddress = c.ReceiverAddress, Value = c.CustomValue.ToString(), Currency = cu.CurrencyName }).ToList();
                int i = 0;
                foreach (var item in IDetails)
                {
                    IDetails[i].Sno = i + 1;
                    i++;
                }
                Session["ManifestImported"] = IDetails;
                vm.Details = IDetails;

            }



            return View(vm);
        }


        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                var fileData = GetDataFromCSVFile(importFile.InputStream);
                ImportManifestVM vm = new ImportManifestVM();
                vm.Details = fileData;
                Session["ManifestImported"] = vm.Details;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        public ActionResult ShowImportItemList()
        {
            ImportManifestVM vm = new ImportManifestVM();
            vm.Details = (List<ImportManifestItem>)Session["ManifestImported"];
            return PartialView("ItemList", vm);
        }
        private List<ImportManifestItem> GetDataFromCSVFile(Stream stream)
        {
            var empList = new List<ImportManifestItem>();
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
                        int i = 1;
                        var dataTable = dataSet.Tables[0];
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {

                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            empList.Add(new ImportManifestItem()
                            {
                                Sno = i++,
                                Bag = objDataRow["BagNo"].ToString(),
                                AWBNo = objDataRow["AWBNo"].ToString(),
                                AWBDate = objDataRow["AWBDate"].ToString(),
                                Shipper = objDataRow["Shipper"].ToString(),
                                Receiver = objDataRow["ReceiverName"].ToString(),
                                ReceiverContact = objDataRow["ReceiverContactName"].ToString(),
                                ReceiverPhone = objDataRow["ReceiverPhone"].ToString(),
                                ReceiverAddress = objDataRow["ReceiverAddress"].ToString(),
                                DestinationLocation = objDataRow["DestinationLocation"].ToString(),
                                DestinationCity = objDataRow["DestinationCity"].ToString(),
                                DestinationCountry = objDataRow["DestinationCountry"].ToString(),
                                Pcs = CommonFunctions.ParseInt(objDataRow["Pcs"].ToString()),
                                Weight = objDataRow["Weight"].ToString(),
                                Value = objDataRow["CustomsValue"].ToString(),
                                COD = objDataRow["COD"].ToString(),
                                Content = objDataRow["Content"].ToString(),
                                ImportType = "Import",
                                route = objDataRow["Route"].ToString(),
                                groupCode = objDataRow["GroupCode"].ToString(),
                                MAWB = objDataRow["MAWB"].ToString(),
                                // Reference = objDataRow["Content"].ToString(),



                            });


                            //AWBNo AWBDate Bag NO.	Shipper ReceiverName    ReceiverContactName ReceiverPhone   ReceiverAddress DestinationLocation DestinationCountry Pcs Weight CustomsValue    COD Content Reference Status  SynchronisedDateTime

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            int j = 0;
            foreach (var item in empList)
            {
                if (item.ReceiverAddress.IndexOf(':') > 0)
                {
                    string contact = item.ReceiverAddress.Split(':')[0];
                    string address = item.ReceiverAddress.Split(':')[1];
                    item.ReceiverContact = contact;
                    item.ReceiverAddress = address;
                    empList[j].ReceiverAddress = address;
                    empList[j].ReceiverContact = contact;
                }
                j++;
            }

            return empList;
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
                var IDetails = (List<ImportManifestItem>)Session["ManifestImported"];
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
                importShipment.Bags = model.Bags;
                importShipment.FlightNo = model.FlightNo;
                if (model.FlightDate1 != "" && model.FlightDate1 != null)
                    importShipment.FlightDate = Convert.ToDateTime(model.FlightDate1);
                //  importShipment.LastEditedByLoginID = userid;
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
                    var checkdupliateawb = db.ImportShipmentDetails.Where(cc => cc.AWB == item.AWBNo).FirstOrDefault();
                    if (checkdupliateawb == null)
                    {
                        updateitem uitem = new updateitem();
                        uitem.AWBNo = item.AWBNo;
                        uitem.synchronisedDateTime = Convert.ToDateTime(model.ManifestDate).ToString("dd-MMM-yyyy HH:mm");
                        bills.Add(uitem);
                        ImportShipmentDetail detail = new ImportShipmentDetail();
                        detail.ImportID = importShipment.ID;
                        detail.AWB = item.AWBNo;
                        detail.AWBDate = Convert.ToDateTime(item.AWBDate);
                        detail.Shipper = item.Shipper;
                        detail.Contents = item.Content;
                        detail.Receiver = item.Receiver;
                        detail.ReceiverAddress = item.ReceiverAddress;
                        detail.ReceiverContact = item.ReceiverContact;
                        detail.ReceiverTelephone = item.ReceiverPhone;
                        detail.DestinationCountry = item.DestinationCountry;
                        detail.DestinationCity = item.DestinationCity;
                        detail.DestinationLocation = item.DestinationLocation;
                        detail.ImportType = "Import";
                        detail.Route = item.route;
                        detail.GroupCode = item.groupCode;
                        if (item.MAWB == "" || item.MAWB == null)
                        {
                            detail.MAWB = importShipment.ManifestNumber;
                        }
                        else
                        {
                            detail.MAWB = item.MAWB;
                        }
                        if (detail.ImportType == "Import")
                        {
                            detail.StatusTypeId = 3;
                            detail.CourierStatusID = 12;  //At Destination Customs Facility
                        }
                        else
                        {
                            detail.StatusTypeId = 2;
                            detail.CourierStatusID = 21;
                        }

                        if (item.Pcs != null)
                            detail.PCS = Convert.ToInt32(item.Pcs);

                        if (item.Weight != "")
                            detail.Weight = Convert.ToDecimal(item.Weight);

                        if (item.COD != "")
                            detail.COD = Convert.ToDecimal(item.COD);
                        detail.BagNo = item.Bag;
                        detail.CustomValue = Convert.ToDecimal(item.Value);
                        var currency = db.CurrencyMasters.Where(cc => cc.CurrencyName == item.Currency).FirstOrDefault();
                        if (currency != null)
                            detail.CurrencyID = currency.CurrencyID;
                        else
                            detail.CurrencyID = 2; //USD default change on Aug 4 2021


                        db.ImportShipmentDetails.Add(detail);
                        db.SaveChanges();

                        //AWB Track status
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();
                        _awbstatus = db.AWBTrackStatus.Where(cc => cc.AWBNo == detail.AWB && cc.CourierStatusId == 12).FirstOrDefault();
                        if (_awbstatus == null)
                        {
                            _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = detail.AWB;
                            _awbstatus.EntryDate = DateTime.UtcNow; // importShipment.CreatedDate;
                            _awbstatus.ShipmentDetailID = detail.ShipmentDetailID;
                            _awbstatus.StatusTypeId = Convert.ToInt32(detail.StatusTypeId);
                            _awbstatus.CourierStatusId = Convert.ToInt32(detail.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(detail.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(detail.CourierStatusID).CourierStatus;
                            _awbstatus.UserId = userid;
                            _awbstatus.EmpID = db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault().EmployeeID;
                            _awbstatus.APIStatus = true;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                        }
                    }

                }
                postbill postbill = new postbill();
                postbill.bills = bills;
                Session["bills"] = postbill;
                //var json = JsonConvert.SerializeObject(bills);
                // CallPostAPI(model.ManifestDate.ToString());
                return Json(new { status = "ok", data = bills });
            }
            catch (Exception ex)
            {
                //return ex.Message;
                return Json(new { status = "Failed", Message = ex.Message });

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

        [HttpGet]
        public JsonResult GetSourceValue(string term, string FieldName)
        {
            var IDetails = (List<ImportManifestItem>)Session["ManifestImported"];
            if (IDetails != null)
            {
                if (term.Trim() != "")
                {
                    if (FieldName == "DestinationCountry")
                    {
                        var list = (from c in IDetails
                                    where c.DestinationCountry.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.DestinationCountry
                                    select new { SourceValue = c.DestinationCountry }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationCity")
                    {
                        var list = (from c in IDetails
                                    where c.DestinationCity.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.DestinationCity
                                    select new { SourceValue = c.DestinationCity }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationLocation")
                    {
                        var list = (from c in IDetails
                                    where c.DestinationLocation.ToLower().Contains(term.Trim().ToLower())
                                    orderby c.DestinationLocation
                                    select new { SourceValue = c.DestinationLocation }).Distinct().ToList();
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
                    if (FieldName == "DestinationCountry")
                    {
                        var list = (from c in IDetails
                                    orderby c.DestinationCountry
                                    select new { SourceValue = c.DestinationCountry }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationCity")
                    {
                        var list = (from c in IDetails
                                    orderby c.DestinationCity
                                    select new { SourceValue = c.DestinationCity }).Distinct().ToList();
                        return Json(list, JsonRequestBehavior.AllowGet);
                    }
                    else if (FieldName == "DestinationLocation")
                    {
                        var list = (from c in IDetails
                                    orderby c.DestinationLocation
                                    select new { SourceValue = c.DestinationLocation }).Distinct().ToList();
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
        public ActionResult UpdateDataFixation(string TargetColumn, string SourceValue, string TargetValue)
        {
            ImportManifestVM model = new ImportManifestVM();
            List<ImportManifestItem> Details = (List<ImportManifestItem>)Session["ManifestImported"];
            if (TargetColumn == "DestinationCountry")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.DestinationCountry == SourceValue)
                    {
                        Details[i].DestinationCountry = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "DestinationCity")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.DestinationCity == SourceValue)
                    {
                        Details[i].DestinationCity = TargetValue;
                    }
                    i++;
                }
            }
            else if (TargetColumn == "DestinationLocation")
            {
                int i = 0;
                foreach (var item in Details)
                {
                    if (item.DestinationLocation == SourceValue)
                    {
                        Details[i].DestinationLocation = TargetValue;
                    }
                    i++;
                }
            }
            SaveDataFixation(TargetColumn, SourceValue, TargetValue);
            model.Details = Details;
            Session["ManifestImported"] = Details;
            return View("ItemList", model);
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

        public async Task<ActionResult> SaveItemList(string InputDate)
        {
            ImportManifestVM vm = new ImportManifestVM();
            vm.Details = new List<ImportManifestItem>();
            string URL = "http://www.niceexpress.net/API/v1/getAPI.do";
            string idate = Convert.ToDateTime(InputDate).ToString("dd-MMM-yyyy");
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
                        ImportManifestItem item = new ImportManifestItem();
                        sno++;
                        item.Sno = sno;
                        item.AWBNo = d.awbNo;
                        item.AWBDate = d.awbDate;
                        item.Bag = d.bagNo;
                        if (d.cod == "")
                            item.COD = "0";
                        else
                            item.COD = d.cod;
                        item.Weight =d.weight.ToString();
                        item.ImportType = d.reference;
                        item.Shipper = d.shipper;
                        item.ShipperPhone = d.cnorTel;
                        item.Content = d.content;
                        item.DestinationCountry = d.destination;
                        if (d.destinationCity == "")
                            item.DestinationCity = d.destination;
                        else
                            item.DestinationCity = d.destinationCity;
                        item.DestinationLocation = d.receiverAddress;
                        if (d.receiverAddress.IndexOf(':') > 0)
                        {
                            string contact = d.receiverAddress.Split(':')[0];
                            string address = d.receiverAddress.Split(':')[1];
                            item.ReceiverContact = contact;
                            item.ReceiverAddress = address;
                        }
                        else
                        {
                            item.ReceiverContact = "";
                            item.ReceiverAddress = d.receiverAddress;
                        }
                        item.Pcs = CommonFunctions.ParseInt(d.pcs);
                        item.Receiver = d.receiverName;
                        item.MAWB = d.MAWB;
                        item.groupCode = d.groupcode;
                        item.route = d.route;

                        item.ReceiverPhone = d.receiverPhone;

                        item.Currency = d.currency;
                        item.Value = d.customsValue;
                        vm.Details.Add(item);
                        // Console.WriteLine("{0}", d.awbNo);
                    }
                    Session["ManifestImported"] = vm.Details;
                    vm.Route = vm.Details[0].route;

                    return PartialView("ItemList", vm);
                    //return View(vm);
                    //return Json(new { data = "ok"});
                }
                else
                {
                    //return View(vm);
                    return PartialView("ItemList", vm);
                    //return Json(new { data = response.StatusCode });
                    //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
            }
        }
        public async Task<ActionResult> ShowItemList(string InputDate)
        {
            string companyname = Session["CompanyName"].ToString();
            ImportManifestVM vm = new ImportManifestVM();
            vm.Details = new List<ImportManifestItem>();
            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
            {
                string URL = "http://www.niceexpress.net/API/v1/getAPI.do";
                string idate = Convert.ToDateTime(InputDate).ToString("dd-MMM-yyyy");
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
                            ImportManifestItem item = new ImportManifestItem();
                            sno++;
                            item.Sno = sno;
                            item.AWBNo = d.awbNo;
                            item.AWBDate = d.awbDate;
                            item.Bag = d.bagNo;
                            if (d.cod == "")
                                item.COD = "0";
                            else
                                item.COD = d.cod;
                            item.Weight = d.weight.ToString();
                            item.ImportType = d.reference;
                            item.Shipper = d.shipper;
                            item.ShipperPhone = d.cnorTel;
                            item.Content = d.content;
                            item.DestinationCountry = d.destination;
                            if (d.destinationCity == "")
                                item.DestinationCity = d.destination;
                            else
                                item.DestinationCity = d.destinationCity;
                            item.DestinationLocation = d.receiverAddress;
                            if (d.receiverAddress.IndexOf(':') > 0)
                            {
                                string contact = d.receiverAddress.Split(':')[0];
                                string address = d.receiverAddress.Split(':')[1];
                                item.ReceiverContact = contact;
                                item.ReceiverAddress = address;
                            }
                            else
                            {
                                item.ReceiverContact = "";
                                item.ReceiverAddress = d.receiverAddress;
                            }
                            item.Pcs = CommonFunctions.ParseInt(d.pcs);
                            item.Receiver = d.receiverName;
                            item.route = d.route;
                            item.groupCode = d.groupcode;
                            item.MAWB = d.MAWB;

                            item.ReceiverPhone = d.receiverPhone;

                            item.Currency = d.currency;
                            item.Value = d.customsValue;
                            item.route = d.route;
                            vm.Details.Add(item);
                            // Console.WriteLine("{0}", d.awbNo);
                        }
                        Session["ManifestImported"] = vm.Details;
                        return PartialView("ItemList", vm);
                        //return View(vm);
                        //return Json(new { data = "ok"});
                    }
                    else
                    {
                        //return View(vm);
                        return PartialView("ItemList", vm);
                        //return Json(new { data = response.StatusCode });
                        //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
            }
            else
            {
                return PartialView("ItemList", vm);
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

            string manifestno = ImportDAO.GetMaxManifestNo(CompanyID, BranchID, mDateTime, "I");
            return manifestno;

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
        public string AutoDataFixation()
        {
            ImportManifestVM model = new ImportManifestVM();
            List<ImportManifestItem> Details = (List<ImportManifestItem>)Session["ManifestImported"];
            DataTable dt = ToDataTable(Details);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var colname = dt.Columns[i].ColumnName;
                int rowindex = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string targetvalue = GetDataFixation(colname, row[colname].ToString());
                    if (targetvalue != "" && targetvalue != null)
                    {
                        dt.Rows[rowindex][colname] = targetvalue;
                    }

                    rowindex++;
                }

            }
            List<ImportManifestItem> list = ImportManifestList(dt);
            Session["ManifestImported"] = list;
            model.Details = list;
            return "ok";


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

        public static List<ImportManifestItem> ImportManifestList(DataTable dt)
        {
            var empList = new List<ImportManifestItem>();
            int i = 0;
            foreach (DataRow objDataRow in dt.Rows)
            {
                empList.Add(new ImportManifestItem()
                {
                    Sno = i++,
                    Bag = objDataRow["Bag"].ToString(),
                    AWBNo = objDataRow["AWBNo"].ToString(),
                    AWBDate = objDataRow["AWBDate"].ToString(),
                    Shipper = objDataRow["Shipper"].ToString(),
                    Receiver = objDataRow["Receiver"].ToString(),
                    ReceiverContact = objDataRow["ReceiverContact"].ToString(),
                    ReceiverPhone = objDataRow["ReceiverPhone"].ToString(),
                    ReceiverAddress = objDataRow["ReceiverAddress"].ToString(),
                    DestinationLocation = objDataRow["DestinationLocation"].ToString(),
                    DestinationCity = objDataRow["DestinationCity"].ToString(),
                    DestinationCountry = objDataRow["DestinationCountry"].ToString(),
                    Pcs = CommonFunctions.ParseInt(objDataRow["Pcs"].ToString()),
                    Weight = objDataRow["Weight"].ToString(),
                    Value = objDataRow["Value"].ToString(),
                    COD = objDataRow["COD"].ToString(),
                    Content = objDataRow["Content"].ToString(),
                    ImportType = "Import"


                });
            }
            return empList;
        }


        public ActionResult AWBList()
        {

            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ImportManifestSearch obj = (ImportManifestSearch)Session["ImportManifestAWBSearch"];
            ImportManifestSearch model = new ImportManifestSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {

                model.AWBNo = obj.AWBNo;

                List<ImportManifestItem> IDetails = (from c in db.ImportShipmentDetails join cu in db.CurrencyMasters on c.CurrencyID equals cu.CurrencyID where c.AWB == obj.AWBNo select new ImportManifestItem { ShipmentDetailID = c.ShipmentDetailID, AWBDate1 = c.AWBDate.ToString(), AWBNo = c.AWB, Shipper = c.Shipper, Bag = c.BagNo, COD = c.COD.ToString(), Content = c.Contents, DestinationCity = c.DestinationCity, DestinationCountry = c.DestinationCountry, Pcs = c.PCS, Receiver = c.Receiver, ReceiverAddress = c.ReceiverAddress, Value = c.CustomValue.ToString(), Currency = cu.CurrencyName, Weight = c.Weight.ToString() }).ToList();
                List<AWBTrackStatusVM> awbdetails = new List<AWBTrackStatusVM>();
                int i = 0;
                foreach (var item in IDetails)
                {
                    IDetails[i].Sno = i + 1;
                    IDetails[i].awbtrackdetails = new List<AWBTrackStatusVM>();

                    var awbtrackDetails1 = (from c in db.AWBTrackStatus
                                            join c1 in db.EmployeeMasters on c.EmpID equals c1.EmployeeID
                                            where c.ShipmentDetailID == item.ShipmentDetailID
                                            orderby c.EntryDate
                                            select new AWBTrackStatusVM { ShipmentDetailID = c.ShipmentDetailID, EntryDate = c.EntryDate, CourierStatus = c.CourierStatus, ShipmentStatus = c.ShipmentStatus, UserName = c1.EmployeeName, CourierStatusId = c.CourierStatusId }).ToList();
                    if (awbtrackDetails1.Count > 0)
                        IDetails[i].awbtrackdetails = awbtrackDetails1;
                    i++;
                }


                //int j = model.Details.Count;

                //if (model.Details != null)
                //{
                //    foreach (var item in Details1)
                //    {
                //        model.Details.Add(item);

                //    }
                //}
                Session["AWBManifestImported"] = IDetails;
                model.Details1 = IDetails;
            }
            else
            {
                model.AWBNo = "";
                Session["ImportManifestAWBSearch"] = model;
                List<ImportManifestItem> translist = new List<ImportManifestItem>();
                model.Details1 = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult AWBList(ImportManifestSearch obj)
        {
            Session["ImportManifestAWBSearch"] = obj;
            return RedirectToAction("AWBList");
        }

        public JsonResult DeleteShipment(int id)
        {
            StatusModel obj = new StatusModel();
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteImportShipmentAWB(id);
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


    public class MasterDataObject
    {
        public int code { get; set; }
        public List<Importproductdata> data { get; set; }
        public string message { get; set; }

    }

    public class Importproductdata
    {
        public string shipper { get; set; }
        public string pcs { get; set; }
        public string awbNo { get; set; }

        public string receiverName { get; set; }

        public string customsValue { get; set; }

        public string awbDate { get; set; }
        public string destination { get; set; }

        public decimal weight { get; set; }
        public string lastStatusRemark { get; set; }

        public string destinationCountry { get; set; }
        public string content { get; set; }
        public string reference { get; set; }

        public string destinationCity { get; set; }

        public string receiverAddress { get; set; }
        public string receiverPhone { get; set; }
        public string cnorTel { get; set; }
        public string cod { get; set; }
        public string currency { get; set; }

        public string id { get; set; }

        public string bagNo { get; set; }

        public string MAWB { get; set; }
        public string route { get; set; }
        public string groupcode { get; set; }
        public string status { get; set; }
    }

    public class postbill
    {
        public List<updateitem> bills { get; set; }
    }
    public class updateitem
    {
        public string AWBNo { get; set; }
        public string synchronisedDateTime { get; set; }
    }

    //RainbowExport Manifest Tracking API result
    public class EcoShipmentTrackingResult
    {

        public EcoTrackingdata data { get; set; }
        public string status { get; set; }
    }
    public class EcoTrackingdata
    {
        public int total { get; set; }
        public string status { get; set; }
        public List<EcoShipmentTrackinglist> list { get; set; }

    }
   
    public class EcoShipmentTrackinglist {
        public int id { get; set; }
        public string status { get; set; } // "DATA UPLOADED",
        public string description { get; set; }
        public string status_code { get; set; }
        public EcoShipmentTrackingCreated created_at { get; set; }

    }
    public class EcoShipmentTrackingCreated
    {

        public string date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }
    }



    //Nice japan Import API shipment
    public class NiceJapanMasterData
    {
    public int status { get; set; }
    public string errorMsg { get; set; }
    public List<NiceJapanResultBody> resultBody { get; set; }
          
}

    public class NiceJapanResultBody
    {
        public int id { get; set; }
         public int     order_id { get; set; } // 26852,
        public int      jc_number { get; set; } // 34821,
         public string    AWBNo { get; set; } //  JC34821-1/3 ,
        public string     AWBDate { get; set; } //  2023-04-26 ,
         public string    Consignor { get; set; } //  Fila Agustin ,
        public string     ConsignorAddress { get; set; } //  Japan ,
        public string     ConsignorPhone { get; set; } //  080-4144-7389 ,
        public string     ConsignorCountryName { get; set; } //  Japan ,
        public string     ConsignorCityName { get; set; } //  739-0651 ,
        public string     ConsignorLocationName { get; set; } //   ,
        public string     Consignee { get; set; } //  Eva lestari ,
        public string     ConsigneeAddress { get; set; } //  RT 01/01, Ds.Mega Sakti, Labuhan Ratu,\r\n(lapangan mega sakti, rumah ibu Eva lestari) ,
       public string      ConsigneePhone { get; set; } //  0853-7726-7515 ,
       public string      ConsigneeCountryName { get; set; } //  LABUHAN RATU ,
        public string     ConsigneeCityName { get; set; } //  LAMPUNG TIMUR - SUMATERA ,
        public string     Pieces { get; set; } //  1 ,
        public string     Weight { get; set; } //  17 ,
       public string      CargoDescription { get; set; } //   ,
       public string      BagNo { get; set; } //  R-108 ,
       public string      Remarks { get; set; } //   ,
       public string      MovementType { get; set; } //  outside java ,
       public string      ProductType { get; set; } //   ,
       public string      ParcelType { get; set; } //  Cartonbox ,
       public string      CourierCharge { get; set; } //   ,
       public string      OtherCharge { get; set; } //   ,
       public string      CustomsCharge { get; set; } //   ,
       public string      MaterialCost { get; set; } //   ,
       public string      Currency { get; set; } //   ,
       public string      to_malusoft { get; set; } // 0,
       public string      created_at { get; set; } //  2023-04-26 18{ get; set; } //06{ get; set; } //33 ,
       public string      updated_at { get; set; } //  2023-04-26 18{ get; set; } //06{ get; set; } //33 ,
       public string      ConsignorMobileNo { get; set; } // null,
       public string      ConsigneeMobileNo { get; set; } // null,
       public string      ConsignorAddress1_Building { get; set; } // null,
       public string      ConsignorAddress2_Street { get; set; } // null,
       public string      ConsignorAddress3_PinCode { get; set; } // null,
       public string      ConsigneeAddress1_Building { get; set; } // null,
       public string      ConsigneeAddress2_Street { get; set; } // null,
       public string      ConsigneeAddress3_PinCode { get; set; } // null,
       public string      ConsigneeLocationName { get; set; } // null
        }

  }
