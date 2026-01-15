using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    public class PickUpEnquiryController : Controller
    {
       
            Entities1 db = new Entities1();


            public ActionResult Index()
            {
                List<PickUpEnquiryVM> lst = (from c in db.CustomerEnquiries join t1 in db.EmployeeMasters on c.CollectedEmpID equals t1.EmployeeID join t2 in db.EmployeeMasters on c.EmployeeID equals t2.EmployeeID select new PickUpEnquiryVM { EnquiryID = c.EnquiryID, EnquiryDate = c.EnquiryDate, Consignor = c.Consignor, Consignee = c.Consignee, eCollectedBy = t1.EmployeeName, eAssignedTo = t2.EmployeeName, AWBNo = c.AWBNo }).ToList();
                return View(lst);
            }


            public ActionResult DeleteConfirmed(int id)
            {
                CustomerEnquiry a = db.CustomerEnquiries.Find(id);
                if (a == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    db.CustomerEnquiries.Remove(a);
                    TempData["SuccessMsg"] = "You have successfully Deleted PickUp Enquiry.";
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            public ActionResult Create()
            {
                int uid = Convert.ToInt32(Session["UserID"].ToString());
                UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
                int empid = u.UserID;
                string empname = u.UserName;


                ViewBag.Country = db.CountryMasters.ToList();
                ViewBag.City = db.CityMasters.ToList();
                ViewBag.Location = db.LocationMasters.ToList();
                ViewBag.Vehicle = db.VehicleMasters.ToList();
                ViewBag.Employee = db.EmployeeMasters.ToList();
                ViewBag.Customer = db.CustomerMasters.ToList();
                ViewBag.empname = empname;
                ViewBag.empid = empid;
                return View();
            }


            [HttpPost]
            public ActionResult Create(PickUpEnquiryVM v)
            {



                int uid = Convert.ToInt32(Session["UserID"].ToString());
                UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
                int empid = u.UserID;
                string empname = u.UserName;

                CustomerEnquiry _enquiry = new CustomerEnquiry();

                int id = (from c in db.CustomerEnquiries orderby c.EnquiryID descending select c.EnquiryID).FirstOrDefault();
                _enquiry.EnquiryID = id + 1;
                _enquiry.EnquiryNo = (id + 1).ToString();
                _enquiry.AWBNo = GetMaxAWBNo();
                _enquiry.EnquiryDate = Convert.ToDateTime(v.EnquiryDate);
                _enquiry.DescriptionID = 4;
                _enquiry.ConsignerCountryId = v.ConsignerCountryId;
                _enquiry.ConsignerCityId = v.ConsignerCityId;

                _enquiry.ConsigneeLocationName = v.ConsigneeLocationName;
                _enquiry.ConsignorLocationName = v.ConsignorLocationName;
                _enquiry.ConsigneeCountryID = v.ConsigneeCountryID;
                _enquiry.ConsigneeCityId = v.ConsigneeCityId;

                _enquiry.Weight = v.Weight;
                _enquiry.AcCompanyID = 1;
                _enquiry.CustomerID = v.CustomerID;
                _enquiry.Consignee = v.Consignee;
                _enquiry.Consignor = v.Consignor;
                _enquiry.ConsignorAddress = v.ConsignorAddress;
                _enquiry.ConsignorAddress1 = v.ConsignorAddress1;
                _enquiry.ConsignorAddress2 = v.ConsignorAddress2;
                _enquiry.ConsigneeAddress = v.ConsigneeAddress;
                _enquiry.ConsigneeAddress1 = v.ConsigneeAddress1;
                _enquiry.ConsigneeAddress2 = v.ConsigneeAddress2;
                _enquiry.ConsignorPhone = v.ConsignorPhone;
                _enquiry.ConsigneePhone = v.ConsigneePhone;
                _enquiry.EmployeeID = v.EmployeeID;
                _enquiry.Remarks = v.Remarks;
                _enquiry.CollectedEmpID = v.CollectedEmpID;
                _enquiry.ShipmentType = v.ShipmentType;
                if (v.vehreq == true)
                {
                    _enquiry.Vehicle = v.Vehicle;
                    _enquiry.VehicleID = v.VehicleID;
                }


                _enquiry.ConsigneeContact = v.ConsigneeContact;
                _enquiry.ConsignorContact = v.ConsignorContact;
                _enquiry.EnteredByID = empid;
                _enquiry.IsEnquiry = true;

                _enquiry.ReadyTime = v.ReadyTime;
                _enquiry.OfficeTimeFrom = v.OfficeTimeFrom;
                _enquiry.OfficeTimeTo = v.OfficeTimeTo;
                _enquiry.RequestSource = v.RequestSource;

                db.CustomerEnquiries.Add(_enquiry);
                TempData["SuccessMsg"] = "You have successfully Added PickUp Enquiry.";
                db.SaveChanges();

                return RedirectToAction("Index");


                return View();
            }



            public ActionResult Edit(int id)
            {
                int uid = Convert.ToInt32(Session["UserID"].ToString());
                UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
                int empid = u.UserID;
                string empname = u.UserName;


                ViewBag.Country = db.CountryMasters.ToList();

                ViewBag.Location = db.LocationMasters.ToList();
                ViewBag.Vehicle = db.VehicleMasters.ToList();
                ViewBag.Employee = db.EmployeeMasters.ToList();
                ViewBag.Customer = db.CustomerMasters.ToList();
                ViewBag.empname = empname;
                ViewBag.empid = empid;

                PickUpEnquiryVM v = new PickUpEnquiryVM();
                CustomerEnquiry a = db.CustomerEnquiries.Find(id);
                if (a == null)
                {
                    return HttpNotFound();
                }
                else
                {

                    v.EnquiryID = a.EnquiryID;
                    v.EnquiryNo = a.EnquiryNo;
                    v.AWBNo = "101";
                    v.EnquiryDate = a.EnquiryDate;
                    v.DescriptionID = 4;
                    v.ConsignerCountryId = a.ConsignerCountryId;
                    v.ConsignerCityId = a.ConsignerCityId;
                    v.ConsignerLocationId = a.ConsignerLocationId;
                    v.ConsigneeCountryID = a.ConsigneeCountryID;
                    v.ConsigneeCityId = a.ConsigneeCityId;
                    v.ConsigneeLocationId = a.ConsigneeLocationId;
                    v.ConsigneeLocationName = a.ConsigneeLocationName;
                    v.ConsignorLocationName = a.ConsignorLocationName;
                    v.Weight = a.Weight;
                    v.AcCompanyID = 1;
                    v.CustomerID = a.CustomerID;
                    v.Consignee = a.Consignee;
                    v.Consignor = a.Consignor;
                    v.ConsignorAddress = a.ConsignorAddress;
                    v.ConsignorAddress1 = a.ConsignorAddress1;
                    v.ConsignorAddress2 = a.ConsignorAddress2;
                    v.ConsigneeAddress = a.ConsigneeAddress;
                    v.ConsigneeAddress1 = a.ConsigneeAddress1;
                    v.ConsigneeAddress2 = a.ConsigneeAddress2;
                    v.ConsignorPhone = a.ConsignorPhone;
                    v.ConsigneePhone = a.ConsigneePhone;
                    v.EmployeeID = a.EmployeeID;
                    v.Remarks = a.Remarks;
                    v.CollectedEmpID = a.CollectedEmpID;
                    v.ShipmentType = a.ShipmentType;
                    if (a.VehicleID != null)
                    {
                        v.VehicleID = a.VehicleID.Value;
                        v.Vehicle = a.Vehicle;
                        v.vehreq = true;
                    }
                    else
                    {
                        v.vehreq = false;
                        v.Vehicle = "";
                    }
                    v.IsEnquiry = a.IsEnquiry;
                    v.ConsigneeContact = a.ConsigneeContact;
                    v.ConsignorContact = a.ConsignorContact;
                    v.EnteredByID = empid;

                    v.ReadyTime = a.ReadyTime;
                    v.OfficeTimeFrom = a.OfficeTimeFrom;
                    v.OfficeTimeTo = a.OfficeTimeTo;
                    v.RequestSource = a.RequestSource;
                }

                var obj = (from c in db.CityMasters where c.CountryID == a.ConsignerCountryId select c).ToList();
                ViewBag.City = (from c in db.CityMasters where c.CountryID == a.ConsignerCountryId select c).ToList();
                ViewBag.CityConsignee = (from c in db.CityMasters where c.CountryID == a.ConsigneeCountryID select c).ToList();

                return View(v);

            }

            [HttpPost]
            public ActionResult Edit(PickUpEnquiryVM v)
            {
                if (ModelState.IsValid)
                {
                    CustomerEnquiry _enquiry = new CustomerEnquiry();
                    _enquiry.EnquiryID = v.EnquiryID;
                    _enquiry.EnquiryNo = v.EnquiryNo;
                    _enquiry.EnquiryDate = Convert.ToDateTime(v.EnquiryDate);
                    _enquiry.AWBNo = v.AWBNo;
                    _enquiry.DescriptionID = 4;
                    _enquiry.ConsignerCountryId = v.ConsignerCountryId;
                    _enquiry.ConsignerCityId = v.ConsignerCityId;
                    _enquiry.ConsignerLocationId = v.ConsignerLocationId;
                    _enquiry.ConsigneeCountryID = v.ConsigneeCountryID;
                    _enquiry.ConsigneeCityId = v.ConsigneeCityId;
                    _enquiry.ConsigneeLocationId = v.ConsigneeLocationId;
                    _enquiry.ConsigneeLocationName = v.ConsigneeLocationName;
                    _enquiry.ConsignorLocationName = v.ConsignorLocationName;
                    _enquiry.Weight = v.Weight;
                    _enquiry.AcCompanyID = 1;
                    _enquiry.CustomerID = v.CustomerID;
                    _enquiry.Consignee = v.Consignee;
                    _enquiry.Consignor = v.Consignor;
                    _enquiry.ConsignorAddress = v.ConsignorAddress;
                    _enquiry.ConsignorAddress1 = v.ConsignorAddress1;
                    _enquiry.ConsignorAddress2 = v.ConsignorAddress2;
                    _enquiry.ConsigneeAddress = v.ConsigneeAddress;
                    _enquiry.ConsigneeAddress1 = v.ConsigneeAddress1;
                    _enquiry.ConsigneeAddress2 = v.ConsigneeAddress2;
                    _enquiry.ConsignorPhone = v.ConsignorPhone;
                    _enquiry.ConsigneePhone = v.ConsigneePhone;
                    _enquiry.EmployeeID = v.EmployeeID;
                    _enquiry.Remarks = v.Remarks;
                    _enquiry.CollectedEmpID = v.CollectedEmpID;
                    _enquiry.IsEnquiry = true;
                    _enquiry.ShipmentType = v.ShipmentType;
                    if (v.vehreq == true)
                    {
                        _enquiry.Vehicle = v.Vehicle;
                        _enquiry.VehicleID = v.VehicleID;
                    }
                    _enquiry.ConsigneeContact = v.ConsigneeContact;
                    _enquiry.ConsignorContact = v.ConsignorContact;
                    _enquiry.EnteredByID = v.EnteredByID;

                    _enquiry.ReadyTime = v.ReadyTime;
                    _enquiry.OfficeTimeFrom = v.OfficeTimeFrom;
                    _enquiry.OfficeTimeTo = v.OfficeTimeTo;
                    _enquiry.RequestSource = v.RequestSource;

                    db.Entry(_enquiry).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMsg"] = "You have successfully Updated PickUp Enquiry.";
                    return RedirectToAction("Index");
                }
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
            //objCust.CountryID = cust.CountryID; //.Value;
            //objCust.CityID = cust.CityID;//.Value;
              //  objCust.CustCode = cust.CustomerCode;
            //objCust.LocationID = cust.LocationID;//.Value;

                return Json(objCust, JsonRequestBehavior.AllowGet);
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
            }


            public string GetMaxAWBNo()
            {
                string awb = (from c in db.CustomerEnquiries orderby c.AWBNo descending select c.AWBNo).FirstOrDefault();

                if (awb == null)
                {
                    return "1";
                }
                else
                {
                    int x = Convert.ToInt32(awb);
                    x = x + 1;

                    return x.ToString();
                }
            }


        }
    
}
