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
using System.IO;
using ClosedXML.Excel;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class CustomerMasterController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Home()
        {
            //var Query = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.MenuID where t1.RoleID == 13 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            //var Query1 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == 13 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            //foreach (Menu q in Query)
            //{
               
            //    Query1.Add(q);
            //}

            List<int> RoleId = (List<int>)Session["RoleID"];

            int roleid = RoleId[0];

            //List<Menu> Query2 = new List<Menu>();
            var Query = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.MenuID where t1.RoleID == roleid && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            var Query1 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == roleid && t.ParentID == 0 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            var Query2 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == roleid && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            if (Query2 != null)
            {
                foreach (Menu q in Query1)
                {
                    var query3 = Query.Where(cc => cc.MenuID == q.MenuID).FirstOrDefault();
                    if (query3 == null)
                        Query2.Add(q);
                }
            }

            if (Query1 != null)
            {
                foreach (Menu q in Query1)
                {
                    var query3 = Query.Where(cc => cc.MenuID == q.MenuID).FirstOrDefault();
                    if (query3 == null)
                        Query.Add(q);
                }
            }



            Session["Menu"] = Query;

            ViewBag.UserName = SourceMastersModel.GetUserFullName(Convert.ToInt32(Session["UserId"].ToString()), Session["UserType"].ToString());
            return View();
            //List<MenuVM> Query = (
            //      from a in db.Menus
            //      from b in db.MenuAccessLevels
            //           .Where(bb => bb.MenuID == a.MenuID)
            //      from c in db.MenuAccessLevels
            //           .Where(cc => cc.ParentID == a.MenuID)
            //      where b.RoleID == 13 && c.RoleID == 13
            //      select new MenuVM  { MenuID = a.MenuID, Title = a.Title, Link = a.Link, ParentID = a.ParentID, Ordering = a.Ordering, SubLevel = a.SubLevel, RoleID = a.RoleID , CreatedBy =a.CreatedBy,  CreatedOn =a.CreatedOn,   ModifiedBy =a.ModifiedBy, ModifiedOn=a.ModifiedOn,
            //    IsActive=a.IsActive,
            //    imgclass =a.imgclass,
            //    PermissionRequired=a.PermissionRequired,
            //    MenuOrder=a.MenuOrder,
            //    IsAccountMenu=a.IsAccountMenu
            //      }).ToList();

            //select new
            //{
            //    ss=t.
            //    First_Name = d.First_Name
            //}
            ViewBag.UserName = SourceMastersModel.GetUserFullName(Convert.ToInt32(Session["UserId"].ToString()), Session["UserType"].ToString());

            Session["Menu"] = Query1.Distinct().ToList();
            return View();


        }



        public ActionResult Index(string SearchText="")
        {
            List<CustmorVM> lst = new List<CustmorVM>();
            var emplist = db.EmployeeMasters.ToList();
            if (SearchText.Trim() != "")
            {
                //var data = db.CustomerMasters.Where(ite => ite.StatusActive.HasValue ? ite.StatusActive == true : false).Where(ite => ite.CustomerName.ToLower().Contains(SearchText.ToLower()) && ite.CustomerID > 0).Where(ite => ite.CustomerType == "CS" || ite.CustomerType == "CR").OrderBy(cc=>cc.CustomerName).ToList();
                var data = db.CustomerMasters.Where(ite => ite.CustomerName.ToLower().Contains(SearchText.ToLower()) && ite.CustomerID > 0).OrderBy(cc => cc.CustomerName).ToList();

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
                    c.APIEnabled = item.APIEnabled;
                    if (item.CreatedBy != null)
                    {
                        
                        int createdby = Convert.ToInt32(item.CreatedBy);
                        var user = emplist.Where(cc => cc.UserID == createdby).FirstOrDefault();
                        if (user != null)
                            c.CreatedBy = user.EmployeeName;
                        if (item.CreatedDate!=null)
                        c.CreatedDate = Convert.ToDateTime(item.CreatedDate);

                    }
                    if (item.ModifiedBy != null)
                    {

                        int createdby = Convert.ToInt32(item.ModifiedBy);
                        var user = emplist.Where(cc => cc.UserID == createdby).FirstOrDefault();
                        if (user != null)
                            c.ModifiedBy = user.EmployeeName;
                        if (item.ModifiedDate != null)
                            c.ModifiedDate = Convert.ToDateTime(item.ModifiedDate);

                    }
                    
                    lst.Add(c);
                }
            }
            else
            {
                var data = db.CustomerMasters.OrderBy(cc=>cc.CustomerName).Take(100).Where(cc=>cc.CustomerID>0).ToList();

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
                    c.APIEnabled = item.APIEnabled;
                    
                    if (item.CreatedBy != null)
                    {

                        int createdby = Convert.ToInt32(item.CreatedBy);
                        var user = emplist.Where(cc => cc.UserID == createdby).FirstOrDefault();
                        if (user != null)
                            c.CreatedBy = user.EmployeeName;
                        if (item.CreatedDate != null)
                            c.CreatedDate = Convert.ToDateTime(item.CreatedDate);

                    }
                    if (item.ModifiedBy != null)
                    {

                        int createdby = Convert.ToInt32(item.ModifiedBy);
                        var user = emplist.Where(cc => cc.UserID == createdby).FirstOrDefault();
                        if (user != null)
                            c.ModifiedBy = user.EmployeeName;
                        if (item.ModifiedDate != null)
                            c.ModifiedDate = Convert.ToDateTime(item.ModifiedDate);

                    }

                    lst.Add(c);
                }

            }
            ViewBag.SearchText = SearchText;
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



        public ActionResult Create(int id = 0)
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
            ViewBag.customerrate = db.CustomerRateTypes.ToList();
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var data = db.tblDepots.Where(c => c.BranchID == BranchID).ToList();
            ViewBag.Depot = data;

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.UserRoleId = Convert.ToInt32(Session["UserRoleID"].ToString());
            CustmorVM obj = new CustmorVM();
            if (id == 0)
            {
                ViewBag.Title = "Create";
                PickupRequestDAO doa = new PickupRequestDAO();
                ViewBag.CustomerNo = "";// doa.GetMaxCustomerCode(branchid);
                obj.CustomerID = 0;
                obj.RoleID = 13;
                obj.CustomerType = "CS";

                obj.Password = doa.RandomPassword(6);
                obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                obj.ApprovedUserName = Convert.ToString(Session["UserName"]);
                obj.CurrenceyID = Convert.ToInt32(Session["CurrencyId"].ToString());
                obj.StatusActive = true;
            }
            else
            {
                ViewBag.Title = "Modify";
                obj = GetDetail(id);
            }

            return View(obj);
        }


        [HttpPost]

        public ActionResult Create(CustmorVM c)
        {
            string locationname = c.LocationName;
            string country = c.CountryName;
            string city = c.CityName;

            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            CustomerMaster obj = new CustomerMaster();
            PickupRequestDAO _dao = new PickupRequestDAO();
            CustomerMaster custcount = new CustomerMaster();
            //if (c.CustomerID == 0)
            //    custcount = (from t in db.CustomerMasters where t.CustomerName.Trim().ToLower() == c.CustomerName.Trim().ToLower() select t).FirstOrDefault();
            //else
            //    custcount = (from t in db.CustomerMasters where t.CustomerName.Trim().ToLower() == c.CustomerName.Trim().ToLower() && (t.CustomerID != c.CustomerID) select t).FirstOrDefault();
            
            if (PickupRequestDAO.CheckCustomerNameExist(c.CustomerName, c.CustomerID))
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
                ViewBag.customerrate = db.CustomerRateTypes.ToList();
                int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                var data = db.tblDepots.Where(cc=>cc.BranchID == BranchID).ToList();
                ViewBag.Depot = data;                                
                ViewBag.UserRoleId = Convert.ToInt32(Session["UserRoleID"].ToString());
                TempData["ErrorMsg"] = "Customer Name is already exist";
                ViewBag.SuccessMsg = "Customer Name is already exist";
                return View(c);
            }

            if (c.CustomerID == 0)
            {

                int max = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerID).FirstOrDefault();
                int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                obj.CustomerID = max + 1;
                obj.AcCompanyID = accompanyid;

                obj.CustomerCode = ""; // c.CustomerCode; //  _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
            }
            else
            {
                obj = db.CustomerMasters.Find(c.CustomerID);



            }
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
            if (c.CountryCode != null)
                obj.CountryCode = c.CountryCode;
            obj.CountryID = c.CountryID;
            if (c.CityID == 0)
                obj.CityID = null;
            else
                obj.CityID = c.CityID;
            if (c.LocationID == 0)
            {
                obj.LocationID = null;
            }
            else
            {
                obj.LocationID = c.LocationID;
            }
            obj.CountryName = c.CountryName;
            obj.CityName = c.CityName;
            obj.LocationName = c.LocationName;
            obj.PlaceID = c.PlaceID;
            if (c.CurrenceyID == 0)
            {
                c.CurrenceyID = Convert.ToInt32(Session["CurrencyId"].ToString());
            }
            else
            {
                obj.CurrencyID = c.CurrenceyID;
            }
            obj.StatusActive = c.StatusActive;
            obj.CreditLimit = c.CreditLimit;
            obj.StatusTaxable = c.StatusTaxable;
            obj.EmployeeID = c.EmployeeID;
            obj.statusCommission = c.StatusCommission;
            obj.VATTRN = c.VATTRN;
            obj.APIEnabled = c.APIEnabled;
            obj.POSTAPILink = c.POSTAPI;
            obj.GETAPILink = c.GETAPI;
            obj.CourierServiceID = c.CourierServiceID;
            obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            obj.CustomerUsername = c.CustomerUsername;
            //obj.Password = c.Password;
            obj.Password = _dao.RandomPassword(6);
            obj.BusinessTypeId = c.BusinessTypeId;
            obj.Referal = c.Referal;
            obj.OfficeOpenTime = c.OfficeTimeFrom;
            obj.OfficeCloseTime = c.OfficeTimeTo;
            if (c.DepotID == null)
                obj.DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            else
                obj.DepotID = c.DepotID;

            if ((c.CustomerType =="CL" || c.CustomerType == "CR") && c.ChkApprovedBy)
            {
                if (c.ApprovedBy == null)
                {
                    obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                }
                else
                {
                    obj.ApprovedBy = c.ApprovedBy;
                }
                try
                {
                    if (c.ApprovedOn.ToString().Contains("0001"))
                    {
                        obj.ApprovedOn = CommonFunctions.GetCurrentDateTime();
                    }
                    else
                    {
                        if (c.ApprovedOn == null)
                        {
                            obj.ApprovedOn = CommonFunctions.GetCurrentDateTime();
                        }
                        else
                        {
                            obj.ApprovedOn = c.ApprovedOn;
                        }
                    }
                }
                catch(Exception ex)
                {
                    obj.ApprovedOn = CommonFunctions.GetCurrentDateTime();
                }
            }
            if (c.CustomerRateTypeID!=null)
            {
                obj.CustomerRateTypeID = c.CustomerRateTypeID;
            }
            

            try
            {
                //  obj.UserID = u.UserID;
                if (c.CustomerID > 0)
                {
                    obj.ModifiedBy = UserId;
                    obj.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    ReceiptDAO.ReSaveCustomerCode();
                }
                else
                {
                    obj.CreatedBy = UserId;
                    obj.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    obj.ModifiedBy = UserId;
                    obj.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    db.CustomerMasters.Add(obj);
                    db.SaveChanges();
                    ReceiptDAO.ReSaveCustomerCode();
                }


                if (c.EmailNotify == true)
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


            TempData["SuccessMsg"] = "You have successfully added Customer.";
            return RedirectToAction("Index");



            return View();
        }

        //GetStatus
        [HttpPost]
        public JsonResult GetCustomerCode(string custname)
        {
            string status = "ok";
            string customercode = "";
            //List<CourierStatu> _cstatus = new List<CourierStatu>();
            try
            {

                customercode = ReceiptDAO.GetMaxCustomerCode(custname);
                //string custform = "000000";
                //string maxcustomercode = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerCode).FirstOrDefault();
                //string last6digit = "";
                //if (maxcustomercode==null || maxcustomercode=="")
                //{
                //    //maxcustomercode="AA000000";
                //    last6digit = "0";

                //}
                //else
                //{
                //    last6digit = maxcustomercode.Substring(maxcustomercode.Length - 6); //, maxcustomercode.Length - 6);
                //}
                //if (last6digit !="")
                //{

                //    string customerfirst = custname.Substring(0, 1);
                //    string customersecond = "";
                //    try
                //    {
                //        customersecond = custname.Split(' ')[1];
                //        customersecond = customersecond.Substring(0, 1);
                //    }
                //    catch(Exception ex)
                //    {

                //    }

                //    if (customerfirst !="" && customersecond!="") 
                //    {
                //        customercode = customerfirst + customersecond + String.Format("{0:000000}", Convert.ToInt32(last6digit) + 1); 
                //    }
                //    else
                //    {
                //        customercode = customerfirst + "C" + String.Format("{0:000000}", Convert.ToInt32(last6digit) + 1);
                //    }

                //}

                return Json(new { data = customercode, result = status }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = "", result = "failed" }, JsonRequestBehavior.AllowGet);

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
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var data = db.tblDepots.Where(d => c.BranchID == BranchID).ToList();
            ViewBag.Depot = data;

            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {

                UserRegistration u = new UserRegistration();
                if (c.UserID != null)
                {
                    //UserRegistration x = (from a in db.UserRegistrations where a.UserName == c.Email select a).FirstOrDefault();
                    UserRegistration x = (from a in db.UserRegistrations where a.UserID == c.UserID select a).FirstOrDefault();

                    if (x != null)
                    {
                        if (x.RoleID != null)
                            if (obj.RoleID == 0)
                                obj.RoleID = 13;
                            else
                                obj.RoleID = Convert.ToInt32(x.RoleID);
                    }
                }
                obj.RoleID = 13;
                obj.CustomerID = c.CustomerID;

                if (c.AcCompanyID != null)
                    obj.AcCompanyID = c.AcCompanyID.Value;
                else
                    obj.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

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

                if (c.CountryID != null)
                    obj.CountryID = c.CountryID.Value;
                if (c.CityID != null)
                    obj.CityID = c.CityID.Value;
                if (c.LocationID != null)
                    obj.LocationID = c.LocationID.Value;
                obj.CountryName = c.CountryName;
                obj.LocationName = c.LocationName;
                obj.CityName = c.CityName;

                if (c.CurrencyID != null)
                    obj.CurrenceyID = c.CurrencyID.Value;
                else
                    obj.CurrenceyID = Convert.ToInt32(Session["CurrencyId"].ToString());

                obj.StatusActive = c.StatusActive.Value;
                if (c.CreditLimit != null)
                { obj.CreditLimit = c.CreditLimit.Value; }
                else
                { obj.CreditLimit = 0; }
                if (c.StatusTaxable != null)
                { obj.StatusTaxable = c.StatusTaxable.Value; }
                else
                {
                    obj.StatusTaxable = false;
                }

                if (c.EmployeeID != null)
                    obj.EmployeeID = c.EmployeeID.Value;
                if (c.statusCommission != null)
                { obj.StatusCommission = c.statusCommission.Value; }
                if (c.BusinessTypeId != null)
                {
                    obj.BusinessTypeId = Convert.ToInt32(c.BusinessTypeId);
                }
                if (c.Referal != null)
                { obj.Referal = c.Referal; }

                obj.OfficeTimeFrom = c.OfficeOpenTime;
                obj.OfficeTimeTo = c.OfficeCloseTime;

                if (c.CourierServiceID != null)
                    obj.CourierServiceID = c.CourierServiceID.Value;

                if (c.BranchID != null)
                {
                    obj.BranchID = c.BranchID.Value;
                }
                obj.VATTRN = c.VATTRN;
                int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                obj.DepotID = DepotID;
                obj.CustomerUsername = c.CustomerUsername;
                obj.Password = c.Password;
                if (c.UserID != null)
                {
                    obj.UserID = c.UserID;
                }

                if (c.ApprovedBy == null || c.ApprovedBy == 0)
                {
                    obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                    obj.ApprovedUserName = Convert.ToString(Session["UserName"]);
                }

            }

            return View(obj);
        }
        public CustmorVM GetDetail(int id)
        {
            CustmorVM obj = new CustmorVM();
            var c = (from d in db.CustomerMasters where d.CustomerID == id select d).FirstOrDefault();
            UserRegistration u = new UserRegistration();
            if (c.UserID != null)
            {
                //UserRegistration x = (from a in db.UserRegistrations where a.UserName == c.Email select a).FirstOrDefault();
                UserRegistration x = (from a in db.UserRegistrations where a.UserID == c.UserID select a).FirstOrDefault();

                if (x != null)
                {
                    if (x.RoleID != null)
                        if (obj.RoleID == 0)
                            obj.RoleID = 13;
                        else
                            obj.RoleID = Convert.ToInt32(x.RoleID);
                }
            }
            obj.RoleID = 13;
            obj.CustomerID = c.CustomerID;

            if (c.AcCompanyID != null)
                obj.AcCompanyID = c.AcCompanyID.Value;
            else
                obj.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            obj.CustomerCode = c.CustomerCode;
            obj.CustomerName = c.CustomerName;
            if (c.CustomerType == null)
                obj.CustomerType = "CS";
            else
                obj.CustomerType = c.CustomerType;

            obj.ReferenceCode = c.ReferenceCode;
            obj.ContactPerson = c.ContactPerson;
            obj.Address1 = c.Address1;
            obj.Address2 = c.Address2;
            obj.Address3 = c.Address3;
            obj.Phone = c.Phone;
            obj.Mobile = c.Mobile;
            obj.Email = c.Email;
            obj.Fax = c.Fax;
            obj.Email = c.Email;
            obj.Website = c.WebSite;
            if (c.PlaceID != null)
                obj.PlaceID = c.PlaceID;
            if (c.CountryCode!=null)
            {
                obj.CountryCode = c.CountryCode;
            }
            if (c.CountryID != null)
                obj.CountryID = c.CountryID.Value;
            if (c.CityID != null)
                obj.CityID = c.CityID.Value;
            if (c.LocationID != null)
                obj.LocationID = c.LocationID.Value;
            obj.CountryName = c.CountryName;
            obj.LocationName = c.LocationName;
            obj.CityName = c.CityName;

            if (c.CurrencyID != null)
                obj.CurrenceyID = c.CurrencyID.Value;
            else
                obj.CurrenceyID = Convert.ToInt32(Session["CurrencyId"].ToString());
            if (c.StatusActive == null)
                obj.StatusActive = false;
            else
                obj.StatusActive = c.StatusActive.Value;
            if (c.CreditLimit != null)
            { obj.CreditLimit = c.CreditLimit.Value; }
            else
            { obj.CreditLimit = 0; }
            if (c.StatusTaxable != null)
            { obj.StatusTaxable = c.StatusTaxable.Value; }
            else
            {
                obj.StatusTaxable = false;
            }

            if (c.EmployeeID != null)
                obj.EmployeeID = c.EmployeeID.Value;
            if (c.statusCommission != null)
            { obj.StatusCommission = c.statusCommission.Value; }
            if (c.BusinessTypeId != null)
            {
                obj.BusinessTypeId = Convert.ToInt32(c.BusinessTypeId);
            }
            if (c.Referal != null)
            { obj.Referal = c.Referal; }

            obj.OfficeTimeFrom = c.OfficeOpenTime;
            obj.OfficeTimeTo = c.OfficeCloseTime;

            if (c.CourierServiceID != null)
                obj.CourierServiceID = c.CourierServiceID.Value;

            if (c.BranchID != null)
            {
                obj.BranchID = c.BranchID.Value;
            }
            obj.VATTRN = c.VATTRN;
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            obj.DepotID = DepotID;
            obj.CustomerUsername = c.CustomerUsername;
            obj.Password = c.Password;
            if (c.UserID != null)
            {
                obj.UserID = c.UserID;
            }

            if (c.ApprovedBy == null || c.ApprovedBy == 0)
            {
                obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                obj.ApprovedUserName = Convert.ToString(Session["UserName"]);
                if (c.ApprovedOn!=null)
                {
                    obj.ApprovedOn = Convert.ToDateTime(c.ApprovedOn);
                }
            }
            else
            {
                obj.ApprovedOn = Convert.ToDateTime(c.ApprovedOn);
                obj.ApprovedBy = c.ApprovedBy;
            }

            if (c.CustomerRateTypeID!=null)
            {
                obj.CustomerRateTypeID =Convert.ToInt32(c.CustomerRateTypeID);
            }
            return obj;



        }
        //
        // POST: /CustomerMaster/Edit/5
        [HttpPost]
        public ActionResult Edit(CustmorVM c)
        {
            CustomerMaster obj = db.CustomerMasters.Find(c.CustomerID);

            //obj.CustomerID = c.CustomerID;
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
            obj.CountryID = c.CountryID;
            obj.CityID = c.CityID;
            obj.LocationID = c.LocationID;
            obj.CountryName = c.CountryName;
            obj.CityName = c.CityName;
            obj.LocationName = c.LocationName;
            if (c.CurrenceyID == 0)
            {
                c.CurrenceyID = Convert.ToInt32(Session["CurrencyID"].ToString());
            }
            else
            {
                obj.CurrencyID = c.CurrenceyID;
            }
            obj.StatusActive = c.StatusActive;
            obj.CreditLimit = c.CreditLimit;
            obj.StatusTaxable = c.StatusTaxable;
            obj.EmployeeID = c.EmployeeID;
            obj.statusCommission = c.StatusCommission;

            obj.CourierServiceID = c.CourierServiceID;
            obj.BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            obj.CustomerUsername = c.CustomerUsername;
            obj.Password = c.Password;

            obj.OfficeOpenTime = c.OfficeTimeFrom;
            obj.OfficeCloseTime = c.OfficeTimeTo;
            obj.Referal = c.Referal;
            obj.BusinessTypeId = c.BusinessTypeId;
            //obj.UserID = c.UserID;
            obj.VATTRN = c.VATTRN;
            obj.DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            if (obj.CustomerType == "CS" && c.CustomerType == "CR" && c.ChkApprovedBy)
            {
                obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                obj.ApprovedOn = c.ApprovedOn;
            }

            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Updated Customer.";
            return RedirectToAction("Index");



        }

        //
        // GET: /CustomerMaster/Delete/5



        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCustomer(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string status = dt.Rows[0][0].ToString();
                        string message = dt.Rows[0][1].ToString();

                        return Json(new { status = status, message = message });
                    }

                }
                else
                {
                    return Json(new { status = "OK", message = "Contact Admin!" });
                }
            }

            return Json(new { status = "OK", message = "Contact Admin!" });


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
        public ActionResult CustomerList()
        {
            List<CustmorVM> lst = new List<CustmorVM>();
            var data = db.CustomerMasters.Where(ite => (ite.StatusActive.HasValue ? ite.StatusActive == true : false) && ite.CustomerType != "CR").ToList();

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
        public ActionResult ApproveCustomer(int id)
        {
            var c = (from d in db.CustomerMasters where d.CustomerID == id select d).FirstOrDefault();
            CustmorVM obj = new CustmorVM();


            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {

                UserRegistration u = new UserRegistration();
                if (c.UserID != null)
                {
                    //UserRegistration x = (from a in db.UserRegistrations where a.UserName == c.Email select a).FirstOrDefault();
                    UserRegistration x = (from a in db.UserRegistrations where a.UserID == c.UserID select a).FirstOrDefault();

                    if (x != null)
                    {
                        if (x.RoleID != null)
                            if (obj.RoleID == 0)
                                obj.RoleID = 13;
                            else
                                obj.RoleID = Convert.ToInt32(x.RoleID);
                    }
                }
                obj.RoleID = 13;
                obj.CustomerID = c.CustomerID;

                if (c.AcCompanyID != null)
                    obj.AcCompanyID = c.AcCompanyID.Value;
                else
                    obj.AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

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

                if (c.CurrencyID != null)
                    obj.CurrenceyID = c.CurrencyID.Value;
                else
                    obj.CurrenceyID = Convert.ToInt32(Session["CurrencyId"].ToString());

                obj.StatusActive = c.StatusActive.Value;
                if (c.CreditLimit != null)
                { obj.CreditLimit = c.CreditLimit.Value; }
                else
                { obj.CreditLimit = 0; }

                obj.StatusTaxable = c.StatusTaxable.Value;

                if (c.EmployeeID != null)
                    obj.EmployeeID = c.EmployeeID.Value;
                if (c.statusCommission != null)
                { obj.StatusCommission = c.statusCommission.Value; }
                if (c.BusinessTypeId != null)
                {
                    obj.BusinessTypeId = Convert.ToInt32(c.BusinessTypeId);
                }
                if (c.Referal != null)
                { obj.Referal = c.Referal; }

                obj.OfficeTimeFrom = c.OfficeOpenTime;
                obj.OfficeTimeTo = c.OfficeCloseTime;

                if (c.CourierServiceID != null)
                    obj.CourierServiceID = c.CourierServiceID.Value;

                if (c.BranchID != null)
                {
                    obj.BranchID = c.BranchID.Value;
                }

                int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                obj.DepotID = DepotID;
                obj.CustomerUsername = c.CustomerUsername;
                if (c.UserID != null)
                {
                    obj.UserID = c.UserID;
                }
                obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
                obj.ApprovedUserName = Convert.ToString(Session["UserName"]);
                obj.ApprovedOn = DateTime.Now;
            }

            return View(obj);
        }
        [HttpPost]
        public ActionResult ApproveCustomer(CustmorVM c)
        {
            CustomerMaster obj = db.CustomerMasters.Find(c.CustomerID);
            obj.ApprovedBy = Convert.ToInt32(Session["UserID"]);
            obj.ApprovedOn = DateTime.Now;
            obj.CustomerType = c.CustomerType;

            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Approved Customer.";
            return RedirectToAction("CustomerList");



        }

        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            bool enablecashcustomer = (bool)Session["EnableCashCustomerInvoice"];
            if (term != null && term.Trim() != "")
            {
                if (enablecashcustomer == true)
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CS" || c1.CustomerType == "CR" || c1.CustomerType == "CL") && c1.CustomerName.ToLower().Contains(term.ToLower())
                                        && c1.StatusActive == true
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CR" || c1.CustomerType == "CL") && c1.CustomerName.ToLower().Contains(term.ToLower())
                                         && c1.StatusActive == true
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (enablecashcustomer == true)
                {

                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CS" || c1.CustomerType == "CR" || c1.CustomerType == "CL")
                                       && c1.StatusActive == true
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CR" || c1.CustomerType == "CL")
                                       && c1.StatusActive == true
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }




        }
        //batch wise excel download
        //public ActionResult CustomerListDownload()
        //{

        //    int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    DataTable dt = InboundShipmentDAO.GetColoaderBatchReportExcel();

        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        wb.Worksheets.Add(dt);
        //        wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        wb.Style.Font.Bold = true;
        //        string FileName = "BatchShipment_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
        //        Response.Clear();
        //        Response.Buffer = true;
        //        Response.Charset = "";
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition", "attachment;filename=" + FileName + ".xlsx");

        //        using (MemoryStream MyMemoryStream = new MemoryStream())
        //        {
        //            wb.SaveAs(MyMemoryStream);
        //            MyMemoryStream.WriteTo(Response.OutputStream);
        //            Response.Flush();
        //            Response.End();
        //        }
        //    }
        //    return RedirectToAction("Index");
        //}
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