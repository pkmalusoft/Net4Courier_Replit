using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.Data.Entity.Validation;

namespace Net4Courier.Controllers
{
    public class CustomerMasterNewController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Home()
        {
            var Query = (from t in db.Menus where t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            Session["Menu"] = Query;
            return View();


        }



        public ActionResult Index()
        {
            List<CustmorVM> lst = new List<CustmorVM>();
            var data = db.CustomerMasters.Where(ite => ite.StatusActive.HasValue ? ite.StatusActive == true : false).ToList();

            foreach (var item in data)
            {
                CustmorVM c = new CustmorVM();

                c.CustomerID = item.CustomerID;
                c.CustomerType = item.CustomerType;
                c.CustomerCode = item.CustomerCode;
                c.CustomerName = item.CustomerName;
                c.ContactPerson = item.ContactPerson;
                c.Mobile = item.Mobile;
                c.Phone = item.Phone;
                lst.Add(c);
            }

            return View(lst);
        }

        //
        // GET: /CustomerMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomerMaster customermaster = db.CustomerMasters.Find(id);
            if (customermaster == null)
            {
                return HttpNotFound();
            }
            return View(customermaster);
        }



        public ActionResult Create()
        {
            var transtypes = new SelectList(new[] 
                                        {
                                            new { ID = "Cr", trans = "Credit" },
                                            new { ID = "Dr", trans = "Debit" },
                                           
                                        },
          "ID", "trans", 1);




            ViewBag.businessType = db.BusinessTypes.ToList();
            ViewBag.country = db.CountryMasters.ToList();
            ViewBag.city = db.CityMasters.ToList();
            ViewBag.location = db.LocationMasters.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            ViewBag.employee = db.EmployeeMasters.ToList();
            ViewBag.roles = db.RoleMasters.ToList();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            
            PickupRequestDAO doa = new PickupRequestDAO();
            ViewBag.CustomerNo = doa.GetMaxCustomerCode(branchid);
            CustmorVM obj = new CustmorVM();
            obj.RoleID = 13;
            obj.Password = doa.RandomPassword(6);
            return View(obj);
        }


        [HttpPost]

        public ActionResult Create(CustmorVM c)
        {
            string locationname = c.LocationName;
            string country = c.CountryName;
            string city = c.CityName;
            CustomerMaster obj = new CustomerMaster();
            PickupRequestDAO _dao = new PickupRequestDAO();
            int max = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerID).FirstOrDefault();


            int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int branchid= Convert.ToInt32(Session["CurrentBranchID"].ToString());
            obj.CustomerID = max + 1;
            obj.AcCompanyID = accompanyid;

            obj.CustomerCode = _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
            obj.CustomerName = c.CustomerName;
            obj.CustomerType = c.CustomerType;

            obj.ReferenceCode = c.ReferenceCode;
            obj.ContactPerson = c.ContactPerson;
            obj.Address1 = c.Address1;
            obj.Address2 = c.Address2;
            obj.Address3 = c.Address3;
            obj.Phone = c.Phone;
            obj.Mobile = c.Mobile;
            obj.Fax = c.Fax;
            obj.Email = c.Email;
            obj.WebSite = c.Website;
            obj.CountryID = 1; ;// c.CountryID;
            obj.CityID = 19; // c.CityID;
            obj.LocationID = 7; // c.LocationID;
            obj.CountryName = c.CountryName;
            obj.CityName = c.CityName;
            obj.LocationName = c.LocationName;
            obj.CurrencyID = c.CurrenceyID;
            obj.StatusActive = c.StatusActive;
            obj.CreditLimit = c.CreditLimit;
            obj.StatusTaxable = c.StatusTaxable;
            obj.EmployeeID = c.EmployeeID;
            obj.statusCommission = c.StatusCommission;


            obj.CourierServiceID = c.CourierServiceID;
            obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            obj.CustomerUsername = c.CustomerUsername;
            //obj.Password = c.Password;
            obj.Password = _dao.RandomPassword(6);
            obj.BusinessTypeId = c.BusinessTypeId;
            obj.Referal = c.Referal;
            obj.OfficeOpenTime = c.OfficeTimeFrom;
            obj.OfficeCloseTime = c.OfficeTimeTo;
                       


            UserRegistration u = new UserRegistration();

            UserRegistration x = (from a in db.UserRegistrations where a.UserName == c.Email select a).FirstOrDefault();
            if (x == null)
            {

                int max1 = (from c1 in db.UserRegistrations orderby c1.UserID descending select c1.UserID).FirstOrDefault();
                u.UserID = max1 + 1;
                u.UserName = c.Email;
                u.EmailId = c.Email;
                u.Password = obj.Password;
                u.Phone = c.Phone;
                u.IsActive = true;
                u.RoleID = c.RoleID;


            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.CustomerMasters.Add(obj);
                    db.SaveChanges();
                    if (c.EmailNotify==true)
                    {
                        EmailDAO _emaildao = new EmailDAO();
                        _emaildao.SendCustomerEmail(c.Email, c.CustomerName, obj.Password);

                    }

                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                }

                try
                {
                    db.UserRegistrations.Add(u);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                }
                TempData["SuccessMsg"] = "You have successfully added Customer.";
                return RedirectToAction("Index");
            }


            return View();
        }



        public ActionResult Edit(int id)
        {
            var c = (from d in db.CustomerMasters where d.CustomerID == id select d).FirstOrDefault();

            //var c = (from d in db.CustomerMasters join t1 in db.UserRegistrations on d.Email equals t1.EmailId   where d.CustomerID == id select d).FirstOrDefault();

            //(from c in db.CustomerEnquiries join t1 in db.EmployeeMasters on c.CollectedEmpID equals t1.EmployeeID join t2 in db.EmployeeMasters on c.EmployeeID equals t2.EmployeeID select new PickupRequestVM { EnquiryID = c.EnquiryID, EnquiryDate = c.EnquiryDate, Consignor = c.Consignor, Consignee = c.Consignee, eCollectedBy = t1.EmployeeName, eAssignedTo = t2.EmployeeName, AWBNo = c.AWBNo }).ToList();
            CustmorVM obj = new CustmorVM();
            //ViewBag.country = db.CountryMasters.ToList();
            //ViewBag.city = db.CityMasters.ToList().Where(x => x.CountryID == c.CountryID);
            //ViewBag.location = db.LocationMasters.ToList().Where(x => x.CityID == c.CityID);
            ViewBag.currency = db.CurrencyMasters.ToList();
            ViewBag.employee = db.EmployeeMasters.ToList();
            ViewBag.businessType = db.BusinessTypes.ToList();
            ViewBag.roles = db.RoleMasters.ToList();


            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {

                UserRegistration u = new UserRegistration();

                UserRegistration x = (from a in db.UserRegistrations where a.UserName == c.Email select a).FirstOrDefault();

                if (x != null)
                {
                    if (x.RoleID != null)
                        if (obj.RoleID == 0)
                            obj.RoleID = 13;
                        else
                            obj.RoleID = Convert.ToInt32(x.RoleID); 
                }

                obj.CustomerID = c.CustomerID;
                obj.AcCompanyID = c.AcCompanyID.Value;
                obj.CustomerCode = c.CustomerCode;
                obj.CustomerName = c.CustomerName;
                obj.CustomerType = c.CustomerType;

                obj.ReferenceCode = c.ReferenceCode;
                obj.ContactPerson = c.ContactPerson;
                obj.Address1 = c.Address1;
                obj.Address2 = c.Address2;
                obj.Address3 = c.Address3;
                obj.Phone = c.Phone;
                obj.Mobile = c.Mobile;
                obj.Fax = c.Fax;
                obj.Email = c.Email;
                obj.Website = c.WebSite;
                //obj.CountryID = c.CountryID; //.Value;
                //obj.CityID = c.CityID; //.Value;
                //obj.LocationID = c.LocationID; //.Value;
                obj.CountryName = c.CountryName;
                obj.LocationName = c.LocationName;
                obj.CityName = c.CityName;
                obj.CurrenceyID = c.CurrencyID.Value;
                obj.StatusActive = c.StatusActive.Value;
                obj.CreditLimit = c.CreditLimit.Value;
                obj.StatusTaxable = c.StatusTaxable.Value;
                obj.EmployeeID = c.EmployeeID.Value;
                obj.StatusCommission = c.statusCommission.Value;
                obj.BusinessTypeId = Convert.ToInt32(c.BusinessTypeId);
                obj.Referal = c.Referal;
                obj.OfficeTimeFrom = c.OfficeOpenTime;
                obj.OfficeTimeTo = c.OfficeCloseTime;
                
                obj.CourierServiceID = c.CourierServiceID.Value;
                obj.BranchID = c.BranchID.Value;
                obj.CustomerUsername = c.CustomerUsername;
                obj.Password = c.Password;
                obj.UserID = c.UserID;
                
            }

            return View(obj);
        }

        //
        // POST: /CustomerMaster/Edit/5
        [HttpPost]
        public ActionResult Edit(CustmorVM c)
        {
            CustomerMaster obj = new CustomerMaster();

            obj.CustomerID = c.CustomerID;
            obj.AcCompanyID = c.AcCompanyID;
            obj.CustomerCode = c.CustomerCode;
            obj.CustomerName = c.CustomerName;
            obj.CustomerType = c.CustomerType;

            obj.ReferenceCode = c.ReferenceCode;
            obj.ContactPerson = c.ContactPerson;
            obj.Address1 = c.Address1;
            obj.Address2 = c.Address2;
            obj.Address3 = c.Address3;
            obj.Phone = c.Phone;
            obj.Mobile = c.Mobile;
            obj.Fax = c.Fax;
            obj.Email = c.Email;
            obj.WebSite = c.Website;
            obj.CountryID = 1;//  c.CountryID;
            obj.CityID = 19; // c.CityID;
            obj.LocationID = 7; // c.LocationID;
            obj.CountryName = c.CountryName;
            obj.CityName = c.CityName;
            obj.LocationName = c.LocationName;
            obj.CurrencyID = c.CurrenceyID;
            obj.StatusActive = c.StatusActive;
            obj.CreditLimit = c.CreditLimit;
            obj.StatusTaxable = c.StatusTaxable;
            obj.EmployeeID = c.EmployeeID;
            obj.statusCommission = c.StatusCommission;


            obj.CourierServiceID = c.CourierServiceID;
            obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString()); ;
            obj.CustomerUsername = c.CustomerUsername;
            obj.Password = c.Password;

            obj.OfficeOpenTime = c.OfficeTimeFrom;
            obj.OfficeCloseTime = c.OfficeTimeTo;
            obj.Referal = c.Referal;
            obj.BusinessTypeId = c.BusinessTypeId;
            obj.UserID = c.UserID;
            
            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Customer.";
                return RedirectToAction("Index");
            }

            return View();
        }

        //
        // GET: /CustomerMaster/Delete/5



        public ActionResult DeleteConfirmed(int id)
        {
            CustomerEnquiry cenquery = db.CustomerEnquiries.Where(t => t.CustomerID == id).FirstOrDefault();
            if (cenquery == null)
            {
                CustomerMaster customermaster = db.CustomerMasters.Find(id);
                UserRegistration a = (from c in db.UserRegistrations where c.UserName == customermaster.Email select c).FirstOrDefault();

                if (customermaster == null)
                {
                    return HttpNotFound();

                }
                else
                {
                    db.CustomerMasters.Remove(customermaster);
                    db.SaveChanges();
                    db.UserRegistrations.Remove(a);
                    db.SaveChanges();
                    TempData["SuccessMsg"] = "You have successfully Deleted Customer.";
                    return RedirectToAction("Index");

                }
            }
            else
            {
                TempData["SuccessMsg"] = "Customer Entry could not delete,because it has reference entries!";
                return RedirectToAction("Index");
            }
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
            var loc = (from c in db.LocationMasters where c.CityID == id select c).ToList();

            foreach (var item in loc)
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


    }
}