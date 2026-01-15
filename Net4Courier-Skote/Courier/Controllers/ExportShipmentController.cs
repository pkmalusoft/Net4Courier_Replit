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
using System.Reflection;
using ClosedXML.Excel;
using System.Web;
using ExcelDataReader;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ExportShipmentController : Controller
    {
        private Entities1 db = new Entities1();
        //private Repos repos = new Repos();

        // GET: ExportShipment
        public ActionResult Index()
        {
           
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ExportShipmentSearch obj = (ExportShipmentSearch)Session["ExportShipmentSearch"];
            ExportShipmentSearch model = new ExportShipmentSearch();
            int Fyearid = Convert.ToInt32(Session["fyearid"]);
            ExportDAO _dao = new ExportDAO();
            if (obj != null)
            {
                List<ExportShipmentFormModel> translist = new List<ExportShipmentFormModel>();
                translist = ExportDAO.GetExportManifestList(0);
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                StatusModel statu = AccountsDAO.CheckDateValidate(obj.FromDate.ToString(), Fyearid);
                string vdate = statu.ValidDate;
                model.FromDate = Convert.ToDateTime(vdate);
                model.ToDate = Convert.ToDateTime(AccountsDAO.CheckDateValidate(obj.ToDate.ToString(), Fyearid).ValidDate);
                
                model.AWBNo = obj.AWBNo;
                Session["ExportShipmentSearch"] = model;
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetFirstDayofWeek().Date; // CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                model.AWBNo = "";
                Session["ExportShipmentSearch"] = model;
                List<ExportShipmentFormModel> translist = new List<ExportShipmentFormModel>();
                translist = ExportDAO.GetExportManifestList(0); ;
                model.Details = translist;

            }
            return View(model);

        }
        [HttpPost]
        public ActionResult Index(ExportShipmentSearch obj)
        {
            Session["ExportShipmentSearch"] = obj;
            return RedirectToAction("Index");
        }

      
        public ActionResult CreateExport(int id=0)
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);            
            var today = DateTime.Now.Date;
            int Fyearid = Convert.ToInt32(Session["fyearid"]);
           // Session["PreviousShipments"] = new List<ExportShipmentDetail>();
            var company = db.AcCompanies.Select(x => new { Address = x.Address1 + ", " + x.Address2 + ", " + x.Address3, x.CountryName, x.CountryID, x.Phone, x.AcCompany1, x.CityName }).FirstOrDefault();
            var Emp = db.EmployeeMasters.Where(x => x.UserID == userid).Select(x => new { Address = x.Address1 + ", " + x.Address2 + ", " + x.Address3, x.CountryID, x.EmployeeName, x.EmployeeCode, x.EmployeeID }).FirstOrDefault();

            ExportShipmentFormModel _ExportShipment = new ExportShipmentFormModel();
            if (id == 0) //new entry
            {
                ViewBag.Title = "Create";
                long manifestNumber = 0;
                _ExportShipment.ID = 0;                             
               
                _ExportShipment.OriginAirportCity = company.CityName;
                _ExportShipment.FlightDate = DateTime.Now;
                _ExportShipment.ManifestDate = CommonFunctions.GetCurrentDateTime();
                StatusModel statu = AccountsDAO.CheckDateValidate(_ExportShipment.ManifestDate.ToString(), Fyearid);
                string vdate = statu.ValidDate;
                _ExportShipment.ManifestDate = Convert.ToDateTime(vdate);
                var shipmenttype = db.tblShipmentTypes.FirstOrDefault();
                string ManifestNumber = "";
                if (shipmenttype != null)
                {
                    _ExportShipment.ShipmentTypeId = shipmenttype.ID;
                    ManifestNumber = ExportDAO.GetMaxExportManifestNo(CompanyID, BranchID, Convert.ToDateTime(_ExportShipment.ManifestDate), shipmenttype.ID);
                }
                else
                {
                    ManifestNumber = ExportDAO.GetMaxExportManifestNo(CompanyID, BranchID, Convert.ToDateTime(_ExportShipment.ManifestDate), 0);
                }

                

                _ExportShipment.ManifestNumber = ManifestNumber;


                if (_ExportShipment.Shipments == null)
                {
                    _ExportShipment.Shipments = new List<ExportShipmentDetail>();
                }

                if (_ExportShipment.ShipmentsVM == null)
                {
                    _ExportShipment.ShipmentsVM = new List<ExportShipmentDetailVM>();
                }
            }
            else
            {
                ViewBag.Title = "Modify";
                var exportshipment = db.ExportShipments.Find(id);
                _ExportShipment.ID = exportshipment.ID;
                _ExportShipment.ManifestNumber = exportshipment.ManifestNumber;
                _ExportShipment.ManifestDate = exportshipment.ManifestDate;
                _ExportShipment.FAgentID = exportshipment.FAgentID;
                _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
                _ExportShipment.FlightNo = exportshipment.FlightNo;
                _ExportShipment.RunNo = exportshipment.RunNo;                
                _ExportShipment.AgentID = exportshipment.AgentID;
                _ExportShipment.Bags = exportshipment.Bags;
                _ExportShipment.CD = exportshipment.CD;                
                _ExportShipment.MAWB = exportshipment.MAWB;
                if (exportshipment.MAWBWeight==null)
                {
                    _ExportShipment.MAWBWeight = 0;
                }
                else
                {
                    _ExportShipment.MAWBWeight = Convert.ToDecimal(exportshipment.MAWBWeight);
                }
                
                _ExportShipment.TotalAWB = exportshipment.TotalAWB;
                _ExportShipment.FlightDate = exportshipment.FlightDate;
                _ExportShipment.CreatedDate = exportshipment.CreatedDate;
                _ExportShipment.OriginAirportCity = exportshipment.OriginAirportCity;
                _ExportShipment.DestinationAirportCity = exportshipment.DestinationAirportCity;
                _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
                _ExportShipment.Shipments = db.ExportShipmentDetails.Where(cc => cc.ExportID == _ExportShipment.ID).ToList();
                List<Net4Courier.Models.ExportShipmentDetailVM> _details = new List<ExportShipmentDetailVM>(); // _ExportShipment.Shipments;
                _details = ExportDAO.GetExportShipmentDetail(_ExportShipment.ID);

                
                _ExportShipment.ShipmentsVM = _details;
                _ExportShipment.TotalAWB = _details.Count;
                Session["PreviousShipments"] = _details;
            }                               
                
                string selectedVal = _ExportShipment.Type;           

            ViewBag.Type = db.tblShipmentTypes.ToList();
            //ViewBag.Type = db.tblStatusTypes.ToList(); // db.tblStatusTypes.ToList();
            var currency= new SelectList(db.CurrencyMasters.OrderBy(x => x.CurrencyName), "CurrencyID", "CurrencyName").ToList();
                ViewBag.CurrencyID = db.CurrencyMasters.ToList();  // db.CurrencyMasters.ToList();
                ViewBag.Currencies = db.CurrencyMasters.ToList();
                ViewBag.AgentName = "ss"; // Emp.EmployeeName;
                ViewBag.CompanyName = company.AcCompany1;
                ViewBag.FwdAgentId = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList();// .ForwardingAgentMasters.ToList();
            var agent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 4); // db.AgentMasters.OrderBy(x => x.Name).ToList(); // .ToList new SelectList(db.AgentMasters.OrderBy(x => x.Name), "AgentID", "Name").ToList();

            ViewBag.AgentList = agent;  
            //ViewBag.FwdAgentId = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList(); //. ForwardingAgentMasters.ToList(); // .Where(d => d.IsForwardingAgent == true).ToList();
            ViewBag.FwdAgentId = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3);// db.ForwardingAgentMasters.OrderBy(cc=>cc.FAgentName).ToList(); //. ForwardingAgentMasters.ToList(); // .Where(d => d.IsForwardingAgent == true).ToList();
            return View(_ExportShipment);
                        
        }
         
        public JsonResult SaveExportShipment(ExportShipmentFormModel model,string Details,string DeleteDetails)
        {
             int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var IDetails = JsonConvert.DeserializeObject<List<ExportShipmentDetailVM>>(Details);
            var IDeleteDetails = JsonConvert.DeserializeObject<List<ExportShipmentDetailVM>>(DeleteDetails);
            var emp = db.EmployeeMasters.Where(cc => cc.UserID == userid).FirstOrDefault();
            model.ShipmentsVM = IDetails;
            var _exportShipment = new ExportShipment();
             
                if (model.ID == 0) // new entry mode
                {
                    string ManifestNumber = "";
                    var CreatedDate = model.ManifestDate; //  CommonFunctions.GetCurrentDateTime();
                    ManifestNumber = ExportDAO.GetMaxExportManifestNo(companyid, branchid, Convert.ToDateTime(CreatedDate), Convert.ToInt32(model.ShipmentTypeId));

                    _exportShipment = new ExportShipment
                    {
                        AgentID = model.AgentID,
                        FAgentID = model.FAgentID,
                        EmployeeID = emp.EmployeeID, //1.Value,
                        Bags = model.Bags,
                        CD = model.CD,
                        
                        AcFinancialYearID = yearid,
                      
                        FlightDate = model.FlightDate,

                        ManifestDate = model.ManifestDate,
                        DestinationAirportCity = model.DestinationAirportCity,
                        OriginAirportCity = model.OriginAirportCity,
                        FlightNo = model.FlightNo,
                        LastEditedByLoginID = emp.EmployeeID,// 1.Value,
                        ManifestNumber = ManifestNumber, //  $"{DateTime.Now.ToString("yyyyMMdd")}{emp.EmployeeCode}{(db.ExportShipments.Where(x => x.EmployeeID == emp.EmployeeID && x.CreatedDate >= today).Count() + 1).ToString("D4")}",
                        ID = db.ExportShipments.Select(x => x.ID).DefaultIfEmpty(0).Max() + 1,
                        MAWB = model.MAWB,
                        MAWBWeight =model.MAWBWeight,
                        RunNo = model.RunNo,
                        TotalAWB = model.TotalAWB,
                        ShipmentTypeId = model.ShipmentTypeId,
                        Type = "import",
                        AcCompanyId = companyid,
                        BranchID = branchid,

                        CreatedDate = CommonFunctions.GetCurrentDateTime(),
                        CreatedBy = userid,
                        ModifiedBy = userid,
                        ModifiedDate = CommonFunctions.GetCurrentDateTime()

                    };
                }
                else
                {
                    _exportShipment = db.ExportShipments.Find(model.ID);
                    _exportShipment.CD = model.CD;
                    _exportShipment.Bags = model.Bags;
                    _exportShipment.MAWB = model.MAWB;
                    _exportShipment.MAWBWeight = model.MAWBWeight;
                    _exportShipment.RunNo = model.RunNo;
                    _exportShipment.TotalAWB = model.TotalAWB;
                    _exportShipment.AgentID = model.AgentID;
                    _exportShipment.FAgentID = model.FAgentID;
                    _exportShipment.ManifestDate = model.ManifestDate;
                    _exportShipment.Type = model.Type;
                    _exportShipment.DestinationAirportCity = model.DestinationAirportCity;
                    _exportShipment.OriginAirportCity = model.OriginAirportCity;
                    _exportShipment.FlightDate = model.FlightDate;
                    _exportShipment.FlightNo = model.FlightNo;
                    _exportShipment.Type = "";
                    _exportShipment.ModifiedBy = userid;
                    _exportShipment.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                }

                if (model.ID == 0)
                {
                    db.ExportShipments.Add(_exportShipment);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(_exportShipment).State = EntityState.Modified;
                    db.SaveChanges();
                }
            DataSet dt = new DataSet();
            dt = ToDataTable(IDetails);
            string xml = dt.GetXml();
            //deleted awb
            DataSet dt1 = new DataSet();
            dt1 = ToDataTable(IDeleteDetails);
            string xml1 = dt1.GetXml();

            SaveStatusModel result = new SaveStatusModel();
            result= ExportDAO.SaveExportShipment(_exportShipment.ID, userid, xml);

            //PickupRequestDAO _dao = new PickupRequestDAO();
            //_dao.GenerateExportManifestPosting(_exportShipment.ID);

            if (result.Status == "OK")
            {
                if (DeleteDetails != "" && DeleteDetails != "[]")
                {
                    SaveStatusModel result1 = new SaveStatusModel();
                    result1 = ExportDAO.SaveExportShipmentDeleted(_exportShipment.ID, userid, xml1);
                    if (result1.Status=="OK")
                    {
                        return Json(new { Status = "Ok", ExportID = _exportShipment.ID, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Status = "Failed", ExportID = _exportShipment.ID, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { Status = "Ok", ExportID= _exportShipment.ID, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Status = "Failed", ExportID = _exportShipment.ID, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            
               

           



            //return RedirectToAction(Token.Function, Token.Controller);
        }


        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile,int ExportID)
        {
            
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<ExportShipmentDetailVM> fileData = GetDataFromCSVFile(importFile.InputStream,ExportID);
                ExportShipmentFormModel vm = new ExportShipmentFormModel();
                vm.ShipmentsVM = fileData;
                //vm.ShipmentsVM = vm.ShipmentsVM.OrderByDescending(cc => cc.SNo).ToList();
                Session["ShipmentList"] = vm.ShipmentsVM;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<ExportShipmentDetailVM> GetDataFromCSVFile(Stream stream, int ExportID)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var details = new List<ManifestCSV>();
            var List = new List<ExportShipmentDetailVM>();
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
                        List = InboundShipmentDAO.GetManifestValidAWBDetails(branchid, xml, "", ExportID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }




            return List;
        }

        public ActionResult ShowShipmentList()
        {
            ExportShipmentFormModel vm = new ExportShipmentFormModel();
            vm.ShipmentsVM = (List<ExportShipmentDetailVM>)Session["ShipmentList"];
            return PartialView("ExportShipmentList", vm);
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



        [HttpPost]
        public async Task<ActionResult> EcoExpressTrackingAPI()
        {

            string companyname = Session["CompanyName"].ToString();
            if (companyname == "RAINBOW SKY CARGO LLC.")
            {
                string URL = "https://app.ecofreight.ae/api/webservices/client/order/ECO0002153654/history_items";
                //string idate = Convert.ToDateTime(InputDate).ToString("dd-MMM-yyyy hh:mm");
                //EcoShipmentTrackingResult bills = (postbill)Session["bills"];
                //var json = JsonConvert.SerializeObject(bills);
                //string urlParameters = "?bills=" + json;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    //HTTP GET
                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization= new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJiYXIiLCJiYXoiOiJib2IiLCJleHAiOjE3MTE3MDU2NTcsInN1YiI6MjI5NiwiaXNzIjoiaHR0cHM6Ly9hcHAuZWNvZnJlaWdodC5hZS9lbi9kYXNoYm9hcmQvc2hpcG1lbnRzL2FwaS8yMjk2IiwiaWF0IjoxNjgwMTY5NjU3LCJuYmYiOjE2ODAxNjk2NTcsImp0aSI6IkppUjd2UWhENzVzOWRNbUMifQ.eUzMCxm7fyDArFTF6nQ-FRHXRZlXFFo36ugPr5ogao4");
                    
                    // List data response.
                    //var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    //var result  = await client.PostAsync("method",content);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                    //string resultContent = await result.Content.ReadAsStringAsync();
                   // var postResponse = await client.PostAsJsonAsync(URL, bills);
                    HttpResponseMessage response = client.GetAsync("").Result;
                   
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response body.

                        var ItemJsonString = await response.Content.ReadAsStringAsync(); // .ReadAsAsync<MasterDataObject>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                        var dataObjects = JsonConvert.DeserializeObject<EcoShipmentTrackingResult>(ItemJsonString);
                        // return RedirectToAction("Index");
                        return Json(new { Status = "ok",  Message = "API Updated Successfully " });
                    }
                    else
                    { 
                        return Json(new { Status = "Failed", Message = "API Not Updated Successfully " });
                    }

                   // return Json(new { Status = "ok", Message = "API Updated Successfully " }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Status = "ok", Message = "API Not Updated!" }, JsonRequestBehavior.AllowGet);
            }
            //return "notworked";
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult AddOrRemoveShipment1(ExportShipmentFormModel s_ImportShipment, int? i)
        {
            var Prevshipmentsession = Session["PreviousShipments"] as List<ExportShipmentDetailVM>;
           
            if (i.HasValue) //delete mode
            {
                if (Prevshipmentsession == null)
                {

                }
                else
                {
                    if (s_ImportShipment.ShipmentsVM == null)
                    {
                        s_ImportShipment.ShipmentsVM = new List<ExportShipmentDetailVM>();
                    }
                    int index = 0;
                    foreach (var item in Prevshipmentsession)
                    {
                        if (i != index)
                        {
                            s_ImportShipment.ShipmentsVM.Add(item);
                        }
                        index++;
                    }
                }
               
                //s_ImportShipment.Shipments.RemoveAt(i.Value);
                Session["PreviousShipments"] = s_ImportShipment.ShipmentsVM;
            }
            else
            {
                if (s_ImportShipment.ShipmentsVM == null)
                {
                    s_ImportShipment.ShipmentsVM = new List<ExportShipmentDetailVM>();
                }
                var shipmentsession = Session["EShipmentdetails"] as ExportShipmentDetailVM;
                var Serialnumber = Convert.ToInt32(Session["EShipSerialNumber"]);
                var isupdate = Convert.ToBoolean(Session["EIsUpdate"]);
                if (Prevshipmentsession==null)
                {

                }
                else
                {
                    foreach(var item in Prevshipmentsession)
                    {
                        s_ImportShipment.ShipmentsVM.Add(item);
                    }
                }
                if (isupdate == true)
                {
                    if (s_ImportShipment.ShipmentsVM.Count ==0)
                    {
                        s_ImportShipment.ShipmentsVM.Add(shipmentsession);
                    }
                    else
                    {
                        s_ImportShipment.ShipmentsVM[Serialnumber] = shipmentsession;
                    }
                    //s_ImportShipment.Shipments.RemoveAt(Serialnumber);                  
                    
                }
                else
                {
                    s_ImportShipment.ShipmentsVM.Add(shipmentsession);
                }
                Session["EShipmentdetails"] = new ExportShipmentDetailVM();
                Session["EShipSerialNumber"] = "";
                Session["EIsUpdate"] = false;
                Session["PreviousShipments"] = s_ImportShipment.ShipmentsVM;
            }
            //ViewBag.Cities =db.CityMasters.ToList();
            ViewBag.FwdAgentId =db.AgentMasters.Where(cc=>cc.AgentType==4).ToList(); //  db.ForwardingAgentMasters.ToList();
            //ViewBag.Countries = db.CountryMasters.ToList();
            //ViewBag.DestinationCountryID = db.CountryMasters.ToList();
            ViewBag.CurrencyID = db.CurrencyMasters.ToList();
            ViewBag.Currencies = db.CurrencyMasters.ToList();
            return PartialView("ExportShipmentList", s_ImportShipment);
        }

       

        
        public JsonResult GetAgentBy_Id(int Id)
        {
            var agent = db.AgentMasters.Find(Id); // db.ForwardingAgentMasters.FirstOrDefault();
            var CountryId = agent.CountryName;
            var address = agent.Address1 + ", " + agent.Address2 + ", " +  ", Tel: " + agent.Phone;
            return Json(new { CountryName = CountryId, Cityname= agent.CityName, address = address }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddOrRemoveShipment(ExportShipmentFormModel s_Exportshipment, string DestinationId)
        //{
        //    s_Exportshipment.Shipments = new List<ExportShipmentDetail>();

        //    var shipmentdetails = db.ImportShipmentDetails.Where(d => d.DestinationCity == DestinationId &&( d.CourierStatusID==15  || d.CourierStatusID==null)).ToList();
        //    foreach (var item in shipmentdetails)
        //    {
        //        var model = new ExportShipmentDetail();
        //        model.AWB = item.AWB;
        //        model.BagNo = item.BagNo;
        //        model.Contents = item.Contents;
        //        model.CurrencyID = item.CurrencyID;               
        //        model.DestinationCity = item.DestinationCity;
        //        model.DestinationCountry = item.DestinationCountry;
        //        model.HAWB = item.AWB;
        //        model.ImportDetailID = item.ShipmentDetailID;
        //        model.Shipper = item.Shipper;
        //        model.Value = item.CustomValue;
        //        model.Weight = item.Weight;
        //        model.Reciver = item.Receiver;
        //        model.ExportID = s_Exportshipment.ID;                
        //        model.PCS = item.PCS;
             
        //        s_Exportshipment.Shipments.Add(model);

        //    }

        //    //ViewBag.Cities = db.CityMasters.ToList();
        //    ViewBag.FwdAgentId = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList(); //  db.ForwardingAgentMasters.ToList();
        //    //ViewBag.Countries = db.CountryMasters.ToList();
        //    //ViewBag.DestinationCountryID = db.CountryMasters.ToList();
        //    ViewBag.CurrencyID = db.CurrencyMasters.ToList();
        //    ViewBag.Currencies = db.CurrencyMasters.ToList();


        //    return PartialView("ShipmentList", s_Exportshipment);
        //}
     
       
       
           
        public JsonResult GetShipmentDetails(ExportShipmentFormModel s_ImportShipment, int? i)
        {
            if (i.HasValue)
            {
                var Prevshipmentsession = Session["PreviousShipments"] as List<ExportShipmentDetailVM>;
                if (Prevshipmentsession == null)
                {

                }
                else
                {
                    if (s_ImportShipment.ShipmentsVM == null)
                    {
                        s_ImportShipment.ShipmentsVM = new List<ExportShipmentDetailVM>();
                        foreach (var item in Prevshipmentsession)
                        {
                            s_ImportShipment.ShipmentsVM.Add(item);
                        }
                    }
                    
                }
                s_ImportShipment.ShipmentsVM = Session["PreviousShipments"] as List<ExportShipmentDetailVM>;
                Net4Courier.Models.ExportShipmentDetail s = s_ImportShipment.ShipmentsVM[Convert.ToInt32(i)];
               
                return Json(new { success = true, data = s, ival = i.Value }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }
       
        public ActionResult Details(int? id)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ExportShipmentFormModel _ExportShipment = new ExportShipmentFormModel();

                var exportshipment = db.ExportShipments.Find(id);
                _ExportShipment.ID = exportshipment.ID;
                _ExportShipment.ManifestNumber = exportshipment.ManifestNumber;
                _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
                _ExportShipment.FlightNo = exportshipment.FlightNo;
                _ExportShipment.RunNo = exportshipment.RunNo;
            var branchmaster = db.BranchMasters.Find(branchid);

            _ExportShipment.ConsignorName = branchmaster.BranchName;
            _ExportShipment.ConsignorCountryName = branchmaster.CountryName;
            _ExportShipment.ConsignorCityName = branchmaster.CityName;
            _ExportShipment.ConsignorAddress = branchmaster.Address1;
            _ExportShipment.ConsignorAddress2 = branchmaster.Address2;
                 
            if (exportshipment.ShipmentTypeId ==2) //Forwarding agent
            {
                if (exportshipment.FAgentID != null)
                {
                    var fagent = db.ForwardingAgentMasters.Where(cc=>cc.FAgentID== exportshipment.FAgentID).FirstOrDefault();
                    if (fagent != null)
                    {
                        _ExportShipment.ConsigneeName = fagent.FAgentName;
                        _ExportShipment.ConsigneeAddress = fagent.Address1;
                        _ExportShipment.ConsigneeAddress2 = fagent.Address2;
                        _ExportShipment.ConsigneeCityName = fagent.CityName;
                        _ExportShipment.ConsigneeCountryname = fagent.CountryName;
                    }
                }
            }
            else
            {
                if (exportshipment.AgentID != null)
                {
                    var fagent = db.AgentMasters.Find(exportshipment.AgentID);
                    if (fagent != null)
                    {
                        _ExportShipment.ConsigneeName = fagent.Name;
                        _ExportShipment.ConsigneeAddress = fagent.Address1;
                        _ExportShipment.ConsigneeAddress2 = fagent.Address2;
                        _ExportShipment.ConsigneeCityName = fagent.CityName;
                        _ExportShipment.ConsigneeCountryname = fagent.CountryName;
                    }
                }
            }

            _ExportShipment.AgentID = exportshipment.AgentID;
                _ExportShipment.Bags = exportshipment.Bags;
                _ExportShipment.CD = exportshipment.CD;
                _ExportShipment.MAWB = exportshipment.MAWB;
                _ExportShipment.TotalAWB = exportshipment.TotalAWB;
                _ExportShipment.FlightDate = exportshipment.FlightDate;
                _ExportShipment.CreatedDate = exportshipment.CreatedDate;
                _ExportShipment.OriginAirportCity = exportshipment.OriginAirportCity;
                _ExportShipment.DestinationAirportCity = exportshipment.DestinationAirportCity;
                _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
                _ExportShipment.Type = db.tblShipmentTypes.Find(exportshipment.ShipmentTypeId).ShipmentType;
            _ExportShipment.ShipmentsVM = ExportDAO.GetExportShipmentDetail(exportshipment.ID);
                //_ExportShipment.ShipmentsVM = (from c in db.ExportShipmentDetails 
                //                               join cur in db.CurrencyMasters on c.CurrencyID equals cur.CurrencyID
                //                               join agent in db.AgentMasters on c.FwdAgentId equals agent.AgentID into gj
                //                               from subpet in gj.DefaultIfEmpty()
                //                               where c.ExportID == id
                //                            select new ExportShipmentDetailVM {ShipmentDetailID =c.ShipmentDetailID,
                //                      //  ImportDetailID=c.ImportDetailID,
                //                        ExportID = c.ExportID,MAWB=c.MAWB,AWBNo=c.AWBNo       ,PCS=c.PCS,
                //                        Weight=c.Weight,Contents=c.Contents, Shipper=c.Shipper,Value=c.Value,            
                //                        Receiver=c.Receiver,DestinationCountry=c.DestinationCountry,DestinationCity=c.DestinationCity,
                //                        BagNo=c.BagNo,CurrencyID=c.CurrencyID,FwdAgentId=c.FwdAgentId,FwdAgentAWBNo=c.FwdAgentAWBNo,FwdDate=c.FwdDate,FwdFlight=c.FwdFlight,
                //                        FwdCharge=c.FwdCharge,OtherCharge=c.OtherCharge,InscanId=c.InscanId,ImportShipmentDetailID=c.ImportShipmentDetailID,
                //                                ForwardAgentName=subpet.Name,CurrencyName=cur.CurrencyName,
                //                                CurrenySymbol=cur.Symbol
                //                            }).ToList();

               ViewBag.Edit = true; // Token.Permissions.Updation;
               ViewBag.agents = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList();
               return View(_ExportShipment);            
        }
        public ActionResult ExportShipmentReport(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ExportShipmentFormModel _ExportShipment = new ExportShipmentFormModel();

            var exportshipment = db.ExportShipments.Find(id);
            _ExportShipment.ID = exportshipment.ID;
            _ExportShipment.ManifestNumber = exportshipment.ManifestNumber;
            _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
            _ExportShipment.FlightNo = exportshipment.FlightNo;
            _ExportShipment.RunNo = exportshipment.RunNo;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            string ConsigneeName = "";
            string ConsigneeCountryName = "";
            string ConsigneeCityName = "";
            string ConsigneeADdress = "";


            string ConsignorName = "";
            string ConsignorCountryName = "";
            string ConsignorCityName = "";
            string ConsignorADdress = "";
            var branch = db.BranchMasters.Find(branchid);
            ConsignorName = branch.BranchName;
            ConsignorADdress = branch.Address1 + "," + branch.Address2 + "," + branch.Address3;
            ConsignorCityName = branch.CityName;
            ConsignorCountryName = branch.CountryName;

            if (exportshipment.FAgentID > 0)
            {
                var fagent = db.ForwardingAgentMasters.Where(c=>c.FAgentID== exportshipment.FAgentID).FirstOrDefault();
                ConsigneeName = fagent.FAgentName;
                ConsigneeCountryName = fagent.CountryName;
                ConsigneeCityName = fagent.CityName;
                ConsigneeADdress = fagent.Address1 + "," + fagent.Address2 + "," + fagent.Address3;
            }
            if (exportshipment.AgentID > 0)
            {
                var fagent = db.AgentMasters.Find(exportshipment.AgentID);
                ConsigneeName = fagent.Name;
                ConsigneeCountryName = fagent.CountryName;
                ConsigneeCityName = fagent.CityName;
                ConsigneeADdress = fagent.Address1 + "," + fagent.Address2 + "," + fagent.Address3;
            }


            _ExportShipment.ConsignorName = ConsignorName;
            _ExportShipment.ConsignorCountryName = ConsignorCountryName;
            _ExportShipment.ConsignorCityName = ConsignorCityName;
            _ExportShipment.ConsignorAddress = ConsignorADdress;
            _ExportShipment.ConsigneeName = ConsigneeName;
            _ExportShipment.ConsigneeAddress = ConsigneeADdress;
            _ExportShipment.ConsigneeCountryname = ConsigneeCountryName;
            _ExportShipment.ConsigneeCityName = ConsigneeCityName;
            _ExportShipment.AgentID = exportshipment.AgentID;
            _ExportShipment.Bags = exportshipment.Bags;
            _ExportShipment.CD = exportshipment.CD;
            _ExportShipment.MAWB = exportshipment.MAWB;
            _ExportShipment.TotalAWB = exportshipment.TotalAWB;
            _ExportShipment.FlightDate = exportshipment.FlightDate;
            _ExportShipment.CreatedDate = exportshipment.CreatedDate;
            _ExportShipment.OriginAirportCity = exportshipment.OriginAirportCity;
            _ExportShipment.DestinationAirportCity = exportshipment.DestinationAirportCity;
            _ExportShipment.ShipmentTypeId = exportshipment.ShipmentTypeId;
            _ExportShipment.Type = db.tblShipmentTypes.Find(exportshipment.ShipmentTypeId).ShipmentType;
            _ExportShipment.ShipmentsVM = (from c in db.ExportShipmentDetails
                                           join ins in db.InScanMasters on c.InscanId equals ins.InScanID
                                           join cur in db.CurrencyMasters on c.CurrencyID equals cur.CurrencyID
                                           join agent in db.AgentMasters on c.FwdAgentId equals agent.AgentID into gj
                                           join pay in db.tblPaymentModes on ins.PaymentModeId equals pay.ID
                                           from subpet in gj.DefaultIfEmpty()
                                           where c.ExportID == id
                                           select new ExportShipmentDetailVM
                                           {
                                               ShipmentDetailID = c.ShipmentDetailID,
                                               //ImportDetailID = c.ImportDetailID,
                                               ExportID = c.ExportID,
                                               MAWB = c.MAWB,
                                               AWBNo = c.AWBNo,
                                               PCS = c.PCS,
                                               Weight = c.Weight,
                                               Contents = c.Contents,
                                               Shipper = c.Shipper,
                                               Value = c.Value,
                                               Receiver = c.Receiver,
                                               PaymentMode=pay.PaymentModeText,
                                               DestinationCountry = c.DestinationCountry,
                                               DestinationCity = c.DestinationCity,
                                               OriginCountry = ins.ConsignorCountryName,
                                               ConsignorPhone = ins.ConsignorPhone,
                                               ConsigneePhone = ins.ConsigneePhone,
                                               AWBCourierCharge = ins.CourierCharge,
                                               AWBOtherCharge = ins.OtherCharge,
                                               BagNo = c.BagNo,
                                               CurrencyID = c.CurrencyID,
                                               FwdAgentId = c.FwdAgentId,
                                               FwdAgentAWBNo = c.FwdAgentAWBNo,
                                               FwdDate = c.FwdDate,
                                               FwdFlight = c.FwdFlight,
                                               FwdCharge = c.FwdCharge,
                                               OtherCharge = c.OtherCharge,
                                               InscanId = c.InscanId,
                                               ImportShipmentDetailID = c.ImportShipmentDetailID,
                                               FwdAgentName = subpet.Name,
                                               CurrencyName = cur.CurrencyName,
                                               CurrenySymbol = cur.Symbol
                                           }).ToList();

            ViewBag.Edit = true; // Token.Permissions.Updation;
            ViewBag.agents = db.AgentMasters.Where(cc => cc.AgentType == 4).ToList();
            return View(_ExportShipment);
        }
        public ActionResult ExportShipmentPrint(int id)
        {
            ViewBag.ReportName = "Export Shipment Printing";
            AccountsReportsDAO.ExportShipmentReport(id);
            //AccountsReportsDAO.CustomerTaxInvoiceReport(id, monetaryunit);
            return View();
        }
        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();
            if (id != 0)
            {
                DataTable dt =  PickupRequestDAO.DeleteExportShipment(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        obj.Status = dt.Rows[0][0].ToString();
                        obj.Message = dt.Rows[0][1].ToString();
                        TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    obj.Status = "Failed";
                    obj.Message = "Error at delete";
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult GetAWBDetailold(string id,int exportid=0)
        {
            ExportAWBList obj = new ExportAWBList();
            
            var lst = (from c in db.InScanMasters where c.AWBNo == id &&  c.AWBProcessed==true &&  c.IsDeleted==false && ( c.ManifestID==null || c.ManifestID==exportid ) select c).FirstOrDefault();
            if (lst == null)
            {
                return Json(new { status = "failed", data = obj, message = "AWB No. Not found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //if (lst.QuickInscanID == null)
                //{
                obj.DestinationCity = lst.ConsigneeCityName;
                 obj.DestinationCountry = lst.ConsigneeCountryName;
                //obj.Bags = lst.BagNo;
                obj.Weight = lst.Weight;
                obj.Shipper = lst.Consignor;
                obj.Receiver = lst.Consignee;
                obj.Pieces = Convert.ToInt32(lst.Pieces);
                obj.Contents = lst.CargoDescription;
                obj.AWB = lst.AWBNo;
                obj.InScanId = lst.InScanID;
                if (lst.PaymentModeId==3 || lst.PaymentModeId==1)
                    {
                    obj.Value = Convert.ToDecimal(lst.NetTotal);
                }
                else
                {
                    obj.Value = Convert.ToDecimal(lst.CourierCharge);
                }
                

                    return Json(new { status = "ok", data = obj, message = "AWB NO.found" }, JsonRequestBehavior.AllowGet);

                //}
                //else
                //{
                //    return Json(new { status = "failed", data = obj, message = "InScan already Done!" }, JsonRequestBehavior.AllowGet);
                //}
            }

        }
        public JsonResult GetAWBDetail(int SearchOption,string id, string OriginCountry="",string DestinationCountry="",int CoLoaderID=0, int exportid = 0)
        {
            var fyearid = Convert.ToInt32(Session["fyearid"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            
            List<ExportAWBList> obj = new List<ExportAWBList>();
            obj =InboundShipmentDAO.GetManifestAWBInfo(SearchOption,id, exportid, fyearid,branchid,OriginCountry,DestinationCountry,CoLoaderID);

            if (obj.Count > 0)
            {
                return Json(new { status = "ok", data = obj, message = "AWB NO.found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "ok", data = obj, message = "AWB Not found" }, JsonRequestBehavior.AllowGet);
            }
              
           

        }

      
        public ActionResult GetAWB(string term)
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (term.Trim() != "")
            {
                var list = (from c in db.InScanMasters where c.AWBProcessed == true && c.BranchID== BranchId && c.IsDeleted == false && (c.ManifestID==null || c.ManifestID==0)  && c.AWBNo.Contains(term.Trim()) orderby c.AWBNo select new { InScanID = c.InScanID, TransactionDate = c.TransactionDate, AWBNo = c.AWBNo }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c in db.InScanMasters where c.AWBProcessed == true && c.BranchID==BranchId &&  c.IsDeleted == false  && (c.ManifestID == null || c.ManifestID==0) orderby c.AWBNo select new { InScanID = c.InScanID, TransactionDate = c.TransactionDate, AWBNo = c.AWBNo }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);

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
            _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();
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

     
        public ActionResult ManifestDownload(int id)
        {
            
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt= AccountsReportsDAO.ExportShipmentExcelReport(id);
           
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "ManifestRegister_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
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

         
    


        public FileResult ExportShipmentXML(int id)
        {
            ViewBag.ReportName = "Invoice Printing";
            // XmlDocument xmlDoc;
            string reportname = "Manifest_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xml";
            string filepath = ExportDAO.GetExportShipmentXML(id,reportname);
            byte[] fileBytes = GetFile(filepath);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, reportname);
            //DataSet dS = new DataSet();
            //dS.DataSetName = "Manifest";
            //dS.Tables.Add(dt);
            //StringWriter sw = new StringWriter();
            ////dS.WriteXml(sw, XmlWriteMode.IgnoreSchema);
            //sw.WriteLine(dt.Rows[0][0].ToString());
            //string s = sw.ToString();
            //string attachment = "attachment; filename=" + reportname;
            //Response.ClearContent();
            //Response.ContentType = "application/xml";
            //Response.AddHeader("content-disposition", attachment);
            //Response.Write(s);
            //Response.End();
            ////AccountsReportsDAO.CustomerTaxInvoiceReport(id, monetaryunit);
           // return View();
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
    }

  


}