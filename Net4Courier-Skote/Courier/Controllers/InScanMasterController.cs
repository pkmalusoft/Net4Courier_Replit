using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;

namespace Net4Courier.Controllers
{
    public class InScanMasterController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            //List<QuickAWBVM> lst = (from c in db.InScans join t1 in db.CityMasters on c.ConsignorCityID equals t1.CityID join t2 in db.CityMasters on c.ConsigneeCityID equals t2.CityID join t3 in db.CustomerMasters on c.CustomerID equals t3.CustomerID select new QuickAWBVM { HAWBNo = c.AWBNo, customer = t3.CustomerName, shippername = c.ConsignorContact, consigneename = c.Consignee, origin = t1.City, destination = t2.City,InScanID=c.InScanID,InScanDate=c.InScanDate }).ToList();
            List<InScanMasterVM> lst = (from c in db.InScanMasters select new InScanMasterVM { AWBNo = c.AWBNo, Consignor = c.Consignor, Consignee = c.Consignee, ConsigneeLocationName = c.ConsigneeLocationName, InScanID = c.InScanID, TransactionDate = c.TransactionDate }).ToList();

            return View(lst);
        }

        public ActionResult Create()
        {
            int depotid = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int userId = Convert.ToInt32(Session["UserID"].ToString());
            ViewBag.depot = db.tblDepots.ToList();
            //ViewBag.depot = (from c in db.tblDepots where c.ID == depotid select c).ToList();
            ViewBag.employee = db.EmployeeMasters.ToList();
            ViewBag.employeerec = db.EmployeeMasters.ToList();
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.FAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.customerrate = db.CustomerRates.ToList();
            ViewBag.TypeofGoods = db.TypeOfGoods.ToList();
            ViewBag.CourierDescription = db.CourierDescriptions.ToList();
            InScanMasterVM obj = new InScanMasterVM();
            PickupRequestDAO _dao = new PickupRequestDAO();
            string AWBNo = _dao.GetMaAWBNo(branchid);
            obj.AWBNo = AWBNo;
            obj.BranchID = branchid;
            obj.DepotID = depotid;
            obj.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            obj.DeviceID = "WebSite";
            int pickedupid = (from e in db.EmployeeMasters where e.UserID == userId select e.EmployeeID).First();

            obj.TransactionDate = DateTime.Now;
            return View(obj);
        }
        [HttpPost]
        public ActionResult Create(InScanMasterVM v)
        {
            //PickupRequestDAO _dao = new PickupRequestDAO();

            if (ModelState.IsValid)
            {
                int depotid = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());

                int uid = Convert.ToInt32(Session["UserID"].ToString());
                
                
                InScanMaster _enquiry = new InScanMaster();

                int id = (from c in db.CustomerEnquiries orderby c.EnquiryID descending select c.EnquiryID).FirstOrDefault();

                PickupRequestDAO _dao = new PickupRequestDAO();                
                string AWBNo = _dao.GetMaAWBNo(branchid);

                _enquiry.AWBNo = v.AWBNo;
                _enquiry.TransactionDate = DateTime.Now;

                _enquiry.Consignor = v.Consignor;
                _enquiry.ConsignorContact = v.ConsignorContact;
                _enquiry.ConsignorPhone = v.ConsignorPhone;
                _enquiry.ConsignorAddress1_Building = v.ConsignorAddress1_Building;
                _enquiry.ConsignorAddress2_Street = v.ConsignorAddress2_Street;
                _enquiry.ConsignorAddress3_PinCode = v.ConsignorAddress2_Pincode;
                _enquiry.ConsignorCityName = v.ConsignorCityName;
                _enquiry.ConsignorCountryName = v.ConsignorCountryName;
                _enquiry.ConsignorLocationName = v.ConsignorLocationName;


                _enquiry.Consignee = v.Consignee;
                _enquiry.ConsigneeContact = v.ConsigneeContact;
                _enquiry.ConsigneePhone = v.ConsigneePhone;
                _enquiry.ConsigneeLocationName = v.ConsigneeLocationName;
                _enquiry.ConsigneeCountryName = v.ConsigneeCountryName;
                _enquiry.ConsigneeCityName = v.ConsigneeCityName;

                _enquiry.Weight = v.Weight;
                _enquiry.AcCompanyID = 1;
                _enquiry.CustomerID = v.CustomerID;


                _enquiry.PickedupDate = v.PickupDateTime;
                _enquiry.PickedUpEmpID = v.PickupBy;
                _enquiry.Remarks = v.Remarks;

                _enquiry.DepotReceivedBy= v.ReceivedByID;               

                db.InScanMasters.Add(_enquiry);
                db.SaveChanges();

                TempData["SuccessMsg"] = "You have successfully added InScan Master";
                return RedirectToAction("Index");
            }
            else
            {
                int depotid = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                int userId = Convert.ToInt32(Session["CurrentUserID"].ToString());
                ViewBag.depot = db.tblDepots.ToList();
                //ViewBag.depot = (from c in db.tblDepots where c.ID == depotid select c).ToList();
                ViewBag.employee = db.EmployeeMasters.ToList();
                ViewBag.employeerec = db.EmployeeMasters.ToList();
                ViewBag.Customer = db.CustomerMasters.ToList();
                ViewBag.Movement = db.CourierMovements.ToList();
                ViewBag.Employee = db.EmployeeMasters.ToList();
                ViewBag.FAgent = db.ForwardingAgentMasters.ToList();
                ViewBag.Movement = db.CourierMovements.ToList();
                ViewBag.ProductType = db.ProductTypes.ToList();
                ViewBag.parceltype = db.ParcelTypes.ToList();
                ViewBag.customerrate = db.CustomerRates.ToList();
                ViewBag.TypeofGoods = db.TypeOfGoods.ToList();
                ViewBag.CourierDescription = db.CourierDescriptions.ToList();
                //InScanMasterVM obj = new InScanMasterVM();
                //obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                //obj.DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                //obj.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                //obj.DeviceID = "WebSite";
                //int pickedupid = (from e in db.EmployeeMasters where e.UserID == userId select e.EmployeeID).First();

                //obj.TransactionDate = DateTime.Now;
                return View(v);
            }
            //else
            //{
            //    int uid = Convert.ToInt32(Session["UserID"].ToString());

            //    UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
            //    int empid = u.UserID;
            //    string empname = u.UserName;

            //    ViewBag.Country = db.CountryMasters.ToList();
            //    ViewBag.City = db.CityMasters.ToList();
            //    ViewBag.Location = db.LocationMasters.ToList();
            //    ViewBag.Vehicle = db.VehicleMasters.ToList();
            //    ViewBag.Employee = db.EmployeeMasters.ToList();
            //    ViewBag.Customer = db.CustomerMasters.ToList();
            //    ViewBag.RequestType = db.RequestTypes.ToList();
            //    ViewBag.empname = empname;
            //    ViewBag.empid = empid;
            //    PickupRequestDAO doa = new PickupRequestDAO();
            //    ViewBag.RequestNo = doa.GetMaxPickupRequest();
            //    return View();
            //}


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
            objCust.CountryName = cust.CountryName;
            objCust.CityName = cust.CityName;
            objCust.LocationName = cust.LocationName;
            objCust.OfficeOpenTime = cust.OfficeOpenTime.ToString();
            objCust.OfficeCloseTime = cust.OfficeCloseTime.ToString();

            return Json(objCust, JsonRequestBehavior.AllowGet);
        }


        public class CustM
        {
            public int CityID { get; set; }
            public int LocationID { get; set; }
            public int CountryID { get; set; }
            public string CustName { get; set; }
            public string ContactPerson { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Phone { get; set; }
            public string CustCode { get; set; }
            public int CustID { get; set; }
            public string OfficeOpenTime { get; set; }
            public string OfficeCloseTime { get; set; }
            public string CountryName { get; set; }
            public string CityName { get; set; }
            public string LocationName { get; set; }

        }
    }
}