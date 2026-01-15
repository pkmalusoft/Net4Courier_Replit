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
using System.IO;
using ClosedXML.Excel;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class AWBEcomController : Controller
    {
        
        Entities1 db = new Entities1();
        // GET: AWBImport
        // GET: AWBBatch
        public ActionResult Index()
        {
            string _USERTYPE = Session["UserType"].ToString();
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int UserId = 0;
            if (_USERTYPE == "Customer" || _USERTYPE == "CoLoader")
            {
                UserId = Convert.ToInt32(Session["UserID"]);
            }
                AWBBatchSearch obj = (AWBBatchSearch)Session["AWBBatchSearch"];
            AWBBatchSearch model = new AWBBatchSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<AWBBatchList> translist = new List<AWBBatchList>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo;
                translist = AWBDAO.GetAWBBatchList(BranchID, FyearId, model,UserId);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["AWBBatchSearch"] = model;
                List<AWBBatchList> translist = new List<AWBBatchList>();

                translist = AWBDAO.GetAWBBatchList(BranchID, FyearId, model,UserId);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(AWBBatchSearch obj)
        {
            Session["AWBBatchSearch"] = obj;
            return RedirectToAction("Index");
        }

        public ActionResult Create(int id = 0)
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


            ViewBag.CourierStatusList = db.CourierStatus.OrderBy(cc=>cc.CourierStatus).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.OtherCharge = db.OtherCharges.ToList();
            ViewBag.ShipmentMode = db.tblShipmentModes.ToList();
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            AWBBatchVM v = new AWBBatchVM();
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
                
                v.TaxPercent = 5;
                //v.PaymentModeId = 3;
                v.EntrySource = "3"; //--1 refer single entry ,2 refer batch entry, 3 refer excel upload
                ViewBag.Title = "Create";
                
                ViewBag.EditMode = "false";
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                v.ID = 0;
                DateTime pFromDate = CommonFunctions.GetCurrentDateTime();
                v.BatchDate = pFromDate;
                string DocNo = AWBDAO.GetMaxBathcNo(pFromDate, branchid, fyearid); //batch no
                v.BatchNumber = DocNo;
                v.CourierStatusID = 1;
                StatusModel result = AccountsDAO.CheckDateValidate(v.BatchDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
                
                v.AWBDate = CommonFunctions.GetCurrentDateTime();
                v.Details = new List<AWBBatchDetail>();
                v.DefaultCurrencyId = CommonFunctions.GetDefaultCurrencyId();
            }
            else
            {
                AWBBatch batch = db.AWBBatches.Find(id);
                v.ID = batch.ID;
                v.BatchDate = batch.BatchDate;
                v.BatchNumber = batch.BatchNumber;
                v.TotalAWB = Convert.ToInt32(batch.TotalAWB);                
                
                v.Remarks = batch.Remarks;
                
                if (batch.CourierStatusID != null)
                    v.CourierStatusID = Convert.ToInt32(batch.CourierStatusID);

            
               
                //ViewBag.Enquiry = db.InScanMasters.FirstOrDefault();
                List<AWBBatchDetail> details = new List<AWBBatchDetail>();

                details = AWBDAO.GetBatchAWBImportInfo(id);
                v.Details = details;

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
                ViewBag.Title = "Modify";
                StatusModel result = AccountsDAO.CheckDateValidate(v.BatchDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
            }

            return View(v);

        }
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile )
        {
            
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<AWBBatchDetail> fileData = GetDataFromCSVFile(importFile.InputStream);
                AWBBatchVM vm = new AWBBatchVM();
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
        private List<AWBBatchDetail> GetDataFromCSVFile(Stream stream)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var details = new List<AWBBatchDetail>();
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
                        string _USERTYPE = Session["UserType"].ToString();
                        if (_USERTYPE == "Employee")
                        {
                            details = AWBDAO.GetEcomShipmentValidAWBDetails(branchid, xml, "");
                        }
                        else
                        {
                            int logincustomerid = Convert.ToInt32(Session["CustomerId"].ToString());
                            details = AWBDAO.GetCustomerShipmentValidAWBDetails(branchid, logincustomerid, 1, xml, "");
                        }
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
            AWBBatchVM vm = new AWBBatchVM();
            vm.Details = (List<AWBBatchDetail>)Session["ShipmentList"];
            return PartialView("AWBList", vm);
        }
        //[HttpPost]
        //public JsonResult DeleteBatchAWB(int BatchID, DateTime BatchDate, string Details)
        //{
        //    int FyearId = Convert.ToInt32(Session["fyearid"]);
        //    int userid = Convert.ToInt32(Session["UserID"].ToString());
        //    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
        //    InboundAWBBatch batch = new InboundAWBBatch();
        //    try
        //    {
        //        Details.Replace("{}", "");
        //        var IDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(Details);
        //        DataTable ds = new DataTable();
        //        DataSet dt = new DataSet();
        //        dt = ToDataTable(IDetails);
        //        string xml = dt.GetXml();


        //        if (BatchID == 0)
        //        {
        //            batch.BatchDate = BatchDate;
        //            batch.BatchNumber = AWBDAO.GetMaxBathcNo(BatchDate, BranchId, FyearId); //batch no
        //            batch.CreatedBy = userid;
        //            batch.CreatedDate = CommonFunctions.GetBranchDateTime();
        //            batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
        //            batch.ModifiedBy = userid;
        //            batch.AcFinancialYearid = FyearId;
        //            batch.BranchID = BranchId;
        //            db.InboundAWBBatches.Add(batch);
        //            db.SaveChanges();
        //        }
        //        else
        //        {
        //            batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
        //            batch.ModifiedBy = userid;
        //            batch = db.InboundAWBBatches.Find(BatchID);
        //            db.SaveChanges();
        //        }
        //        SaveStatusModel model = new SaveStatusModel();
        //        model = AWBDAO.DeleteBatch(batch.ID);
        //        if (model.Status == "Ok")
        //        {
        //            return Json(new { Status = "OK", BatchID = batch.ID, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { Status = "Failed", BatchID = batch.ID, message = model.Message }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public JsonResult SaveBatchAWB(AWBBatchVM vm, string Details, string DeleteDetails)
        {
            string callapipost = "false";
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            AWBBatch batch = new AWBBatch();
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBBatchDetail>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                string xml = dt.GetXml();

                //int BatchID, DateTime BatchDate,int CustomerID,string EntrySource, int CourierStatusID,
                if (vm.ID == 0)
                {
                    batch.BatchDate = vm.BatchDate;
                    batch.BatchNumber = AWBDAO.GetMaxBathcNo(Convert.ToDateTime(vm.BatchDate), BranchId, FyearId);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;
                    batch.AcFinancialYearid = FyearId;
                    batch.CourierStatusID = vm.CourierStatusID;
                    batch.BranchID = BranchId;
                    
                    batch.EntrySource = "Manual";
                   
                    batch.Remarks = vm.Remarks;


                    db.AWBBatches.Add(batch);
                    db.SaveChanges();

                }
                else
                {
                    batch = db.AWBBatches.Find(vm.ID);
                    batch.BatchDate = vm.BatchDate;
                    batch.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    batch.ModifiedBy = userid;

                    db.SaveChanges();
                }
                SaveStatusModel model = new SaveStatusModel();
                model = AWBDAO.SaveEcomAWBImportBatch(batch.ID, BranchId, CompanyID, userid, FyearId, xml);
                if (model.Status == "OK")
                {

                    string companyname = Session["CompanyName"].ToString();
                    

                    //if (DeleteDetails != "" && DeleteDetails != "[]")
                    //{
                    //    var IDeleteDetails = JsonConvert.DeserializeObject<List<InboundShipmentModel>>(DeleteDetails);

                    //    DataSet dt1 = new DataSet();
                    //    dt1 = ToDataTable(IDeleteDetails);
                    //    string xml1 = dt1.GetXml();
                    //    SaveStatusModel model1 = AWBDAO.DeleteBatchAWB(batch.ID, BranchId, CompanyID, userid, FyearId, xml1);
                    //}

                    return Json(new { Status = "OK", BatchID = batch.ID, CallPostAPI = callapipost, message = model.Message, TotalImportCount = model.TotalImportCount, TotalSaved = model.TotalSavedCount }, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public JsonResult DeleteAWB(int BatchId, int InscanId)
        {

            DataTable dt = AWBDAO.DeleteBatchAWB(BatchId, InscanId);
            string message = "";
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    status = dt.Rows[0][0].ToString();
                    message = dt.Rows[0][1].ToString();
                }

            }
            else
            {
                status = "Failed!";
                message = "Delete Failed!";
            }
            return Json(new { Status = status, Message = message }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult SaveBatchTrackStatus(int BatchID, int NewCourierStatusID, DateTime EntryDate, string Remarks)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            SaveStatusModel model = new SaveStatusModel();
            try
            {
                model = InboundShipmentDAO.UpdateShipmementTrackStatus(BatchID, BranchId, CompanyID, userid, FyearId, NewCourierStatusID, EntryDate, Remarks);
                return Json(new { Status = "OK", BatchID = BatchID, message = model.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
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
    }

   


}