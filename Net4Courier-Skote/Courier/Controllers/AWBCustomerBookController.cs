using Net4Courier.DAL;
using Net4Courier.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class AWBCustomerBookController : Controller
    {
        Entities1 db = new Entities1();
        // GET: AWBCustomerBook
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            int customerid = 0;
            if (Session["CustomerId"] == null)
            {
                customerid = 0;
            }
            else
            {
                customerid = Convert.ToInt32(Session["CustomerId"].ToString());
            }

              int FyearId = Convert.ToInt32(Session["fyearid"]);
                int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
                int userid = Convert.ToInt32(Session["UserID"].ToString());
                AWBBatchVM vm = new AWBBatchVM();
                ViewBag.Movement = db.CourierMovements.ToList();
                ViewBag.ProductType = db.ProductTypes.ToList();
                ViewBag.parceltype = db.ParcelTypes.ToList();
                ViewBag.Employee = db.EmployeeMasters.ToList();
                ViewBag.PaymentMode = db.tblPaymentModes.ToList();
                ViewBag.Vehicle = db.VehicleMasters.ToList();
                ViewBag.Title = "AWB Shipper Booking - Create";

            DateTime pFromDate = CommonFunctions.GetCurrentDateTime(); //  AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                vm.BatchDate = pFromDate;
                string DocNo = AWBDAO.GetMaxBathcNo(Convert.ToDateTime(vm.BatchDate),branchid,FyearId); //batch no
                vm.BatchNumber = DocNo;
                vm.AWBDate = pFromDate;
                vm.AssignedDate = pFromDate;
                vm.TaxPercent = 5;
                var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultproducttype != null)
                    vm.ProductTypeID = defaultproducttype.ProductTypeID;

                var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultmovementtype != null)
                    vm.MovementID = defaultmovementtype.MovementID;

                var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
                if (defaultparceltype != null)
                    vm.ParcelTypeID = defaultparceltype.ID;

                var Customer = (from c1 in db.CustomerMasters
                                where c1.CustomerID == customerid
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, LocationName = c1.LocationName, CountryName = c1.CountryName, CityName = c1.CityName }).FirstOrDefault();
                if (Customer != null)
                {
                    vm.CustomerID = Customer.CustomerID;
                    vm.CustomerName = Customer.CustomerName;
                    vm.Shipper = Customer.CustomerName;
                    vm.PickUpLocation = Customer.LocationName;
                }

                string customername = "";
                customername = "WALK-IN-CUSTOMER";
                var CashCustomer = (from c1 in db.CustomerMasters
                                    where c1.CustomerName == customername
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
                if (CashCustomer != null)
                {
                    vm.CASHCustomerId = CashCustomer.CustomerID;
                    vm.CASHCustomerName = customername;
                }

                customername = "COD-CUSTOMER";
                var CODCustomer = (from c1 in db.CustomerMasters
                                   where c1.CustomerName == customername
                                   orderby c1.CustomerName ascending
                                   select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
                if (CODCustomer != null)
                {
                    vm.CODCustomerID = CODCustomer.CustomerID;
                    vm.CODCustomerName = "COD-CUSTOMER";
                }

                customername = "FOC CUSTOMER";
                var FOCCustomer = (from c1 in db.CustomerMasters
                                   where c1.CustomerName == customername
                                   orderby c1.CustomerName ascending
                                   select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
                if (FOCCustomer != null)
                {
                    vm.FOCCustomerID = FOCCustomer.CustomerID;
                    vm.FOCCustomerName = "FOC CUSTOMER";
                }


                return View(vm);
            
        }

        [HttpPost]
        public string SaveBatch(string BatchNo, string BatchDate, string Details)
        {
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBBatchDetail>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                string xml = dt.GetXml();
                xml = xml.Replace("T00:00:00+05:30", "");
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                    AWBBatch batch = new AWBBatch();
                    batch.BatchNumber = BatchNo;
                    batch.BatchDate = Convert.ToDateTime(BatchDate);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    db.AWBBatches.Add(batch);
                    db.SaveChanges();

                    string result = AWBDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId, xml);
                    return result;
                }
                else
                {
                    return "Failed!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        public JsonResult GetConsignorCustomer(string customername)
        {
            var customerlist = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }
      


        [HttpGet]
        public JsonResult GetReceiverName(string term, string Shipper)
        {
            if (term.Trim() != "")
            {
                var shipperlist = (from c1 in db.InScanMasters
                                   where c1.Consignee.ToLower().StartsWith(term.ToLower())
                                   && c1.Consignor.ToLower().StartsWith(Shipper.ToLower())
                                   orderby c1.Consignee ascending
                                   select new { Name = c1.Consignee, ContactPerson = c1.ConsigneeContact, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building, Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo, ConsigneeMobileNo = c1.ConsigneeMobileNo }).Distinct();

                return Json(shipperlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var shipperlist = (from c1 in db.InScanMasters
                                   where c1.Consignor.ToLower().StartsWith(Shipper.ToLower())
                                   orderby c1.Consignee ascending
                                   select new { Name = c1.Consignee, ContactPerson = c1.ConsigneeContact, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building, Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo, ConsigneeMobileNo = c1.ConsigneeMobileNo }).Distinct();

                return Json(shipperlist, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpGet]
        public JsonResult GetAWBInfo(string awbno)
        {
            int customerid = Convert.ToInt32(Session["CustomerId"].ToString());
            AWBInfo info = AWBDAO.GetAWBInfo(awbno);
            if (info.CustomerID !=customerid && info.Status=="Available")
            {
                info.Status = "Not Available";
                info.Mode = "Invalid AWB!";
            }
            return Json(info, JsonRequestBehavior.AllowGet);

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