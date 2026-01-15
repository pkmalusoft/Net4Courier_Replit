using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.DAL;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ForwardingAgentController : Controller
    {
        Entities1  db = new Entities1();
         

         public ActionResult Home()
         {
             //var Query = (from t in db.Menus where t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();
            var Query = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.MenuID where t1.RoleID == 14 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            var Query1 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == 14 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

            foreach (Menu q in Query)
            {
                Query1.Add(q);
            }
            Session["Menu"] = Query1;
            ViewBag.UserName = SourceMastersModel.GetUserFullName(Convert.ToInt32(Session["UserId"].ToString()), Session["UserType"].ToString());
            return View();

         }

        public ActionResult Index()
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            List<AgentVM> lst = new List<AgentVM>();
            //t.BranchID == BranchID
            lst = (from t in db.ForwardingAgentMasters select new AgentVM { ID = t.FAgentID, AgentName = t.FAgentName, AgentCode = t.FAgentCode, Phone = t.Phone, Fax = t.Fax }).ToList();

          
            return View(lst);
        }

       

        public ActionResult Details(int id = 0)
        {
            tblAgent tblagent = db.tblAgents.Find(id);
            if (tblagent == null)
            {
                return HttpNotFound();
            }
            return View(tblagent);
        }

      

        public ActionResult Create()
        {
            //ViewBag.country = db.CountryMasters.ToList();
            //ViewBag.city = db.CityMasters.ToList();
            //ViewBag.location = db.LocationMasters.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            ViewBag.zonecategory = db.ZoneCategories.ToList();
            ViewBag.achead = db.AcHeads.ToList();
            ViewBag.roles = db.RoleMasters.ToList();
            FAgentVM v = new FAgentVM();
          
            v.FAgentID = 0;
            v.StatusActive = true;
            return View(v);
        }



        [HttpPost]

        public ActionResult Create(FAgentVM item)
        {
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int? max = (from c in db.ForwardingAgentMasters orderby c.FAgentID descending select c.FAgentID).FirstOrDefault();
            ForwardingAgentMaster a = new ForwardingAgentMaster();
            PickupRequestDAO _dao = new PickupRequestDAO();
         
            if (max == null || max==0)
            {

                a.FAgentID = 1;
                a.FAgentName = item.FAgentName;
                a.FAgentCode = "";// item.FAgentCode;
                a.Address1 = item.Address1;
                a.Address2 = item.Address2;
                a.Address3 = item.Address3;
                a.Phone = item.Phone;
                a.Fax = item.Fax;
                a.WebSite = item.WebSite;
                a.ContactPerson = item.ContactPerson;
                a.AcCompanyID = companyId;                
                a.CurrencyID = Convert.ToInt32(item.CurrencyID);
                a.ZoneCategoryID = item.ZoneCategoryID;
                a.AcHeadID = item.AcHeadID;
                a.CountryName = item.CountryName;
                a.CityName = item.CityName;
                a.LocationName = item.LocationName;
                //a.CreditLimit = item.CreditLimit;
                //a.CountryName = item.CountryName;
                //a.CityName = item.CityName;
                //a.LocationName = item.LocationName;
                               
                
                //a.UserID = u.UserID;
                
                if (item.StatusActive == null)
                    a.StatusActive = false;
                else
                    a.StatusActive = Convert.ToBoolean(item.StatusActive);

               // a.BranchID = BranchID;
                a.AcHeadID = item.AcHeadID;

            }
            else
            {
                 a.FAgentID = Convert.ToInt32(max) + 1;
                a.FAgentName = item.FAgentName;
                a.FAgentCode = item.FAgentCode;
                a.Address1 = item.Address1;
                a.Address2 = item.Address2;
                a.Address3 = item.Address3;
                a.Phone = item.Phone;
                a.Fax = item.Fax;
                a.WebSite = item.WebSite;
                a.ContactPerson = item.ContactPerson;                
                a.AcCompanyID = companyId;
                a.CountryName = item.CountryName;
                a.CityName = item.CityName;
                a.LocationName = item.LocationName;
                a.CurrencyID = Convert.ToInt32(item.CurrencyID);
                a.ZoneCategoryID = item.ZoneCategoryID;
                a.AcHeadID = item.AcHeadID;
              
                a.Email = item.Email;              
               
                a.AcHeadID = item.AcHeadID;
                //a.UserID = u.UserID;
                //a.BranchID = BranchID;
                if (item.StatusActive == null)
                    a.StatusActive = false;
                else
                    a.StatusActive = Convert.ToBoolean(item.StatusActive);

            }

            try
            {
                if (item.SupplierID == 0 || item.SupplierID == null)
                    a.SupplierID = SaveNewSupplier(item);
                else
                    a.SupplierID = item.SupplierID;

                db.ForwardingAgentMasters.Add(a);
                db.SaveChanges();

                //save FCode
                ReceiptDAO.ReSaveForwardingAgentCode();

                TempData["SuccessMsg"] = "You have successfully added Forwarding Agent.";
                return RedirectToAction("Index");

            }

            catch(Exception ex )
            {
                ViewBag.currency = db.CurrencyMasters.ToList();
                ViewBag.zonecategory = db.ZoneCategories.ToList();
                ViewBag.achead = db.AcHeads.ToList();
                ViewBag.roles = db.RoleMasters.ToList();
                TempData["WarningMsg"] = ex.Message;
                return View(item);
            }
                                                 
                
                        


        }
        
        public int SaveNewSupplier(FAgentVM vm )
        {
            SupplierMaster model = new SupplierMaster();
            
                model.SupplierName = vm.FAgentName;
                model.Address1 = vm.Address1;
                model.Address2 = vm.Address2;
                model.CityName = vm.CityName;
                model.CountryName = vm.CountryName;
                model.LocationName = vm.LocationName;
                model.ContactPerson = vm.ContactPerson;
                model.BranchID = 1;
                model.Phone = vm.Phone;
                model.AcHeadID = vm.AcHeadID;

                model.SupplierTypeID = 3;
                db.SupplierMasters.Add(model);
                db.SaveChanges();
            return model.SupplierID;
            

        }
        //public int SaveNewSupplier(FAgentVM vm)
        //{
        //    SupplierMaster model = new SupplierMaster();
        //    // if (vm.SupplierID!=null && vm.SupplierID!=0)
        //    //{
        //    //    model = db.SupplierMasters.Find(vm.SupplierID);
        //    //}
        //    model.SupplierName = vm.FAgentName;
        //    model.Address1 = vm.Address1;
        //    model.Address2 = vm.Address2;
        //    model.CityName = vm.CityName;
        //    model.CountryName = vm.CountryName;
        //    model.LocationName = vm.LocationName;
        //    model.ContactPerson = vm.ContactPerson;
        //    model.BranchID = 1;
        //    model.Phone = vm.Phone; 
        //    model.AcHeadID = vm.AcHeadID;
        //    db.SupplierMasters.Add(model);
        //    db.SaveChanges();
        //    return model.SupplierID;


        //}

        public ActionResult Edit(int id)
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());

            FAgentVM a = new FAgentVM();                      

            var item = (from c in db.ForwardingAgentMasters where c.FAgentID == id select c).FirstOrDefault();

            //ViewBag.country = db.CountryMasters.ToList();
            //ViewBag.city = (from c in db.CityMasters where c.CountryID == item.CountryID select c).ToList();
            //ViewBag.location = (from c in db.LocationMasters where c.CityID == item.CityID select c).ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            ViewBag.zonecategory = db.ZoneCategories.ToList();
            ViewBag.achead = db.AcHeads.ToList();
            ViewBag.roles = db.RoleMasters.ToList();

            if (item == null)
            {
                return HttpNotFound();
            }
            else
            {
                a.FAgentID = item.FAgentID;
                //a.Fagent = item.AgentID;
                a.FAgentName = item.FAgentName;
                a.FAgentCode = item.FAgentCode;
                a.Address1 = item.Address1;
                a.Address2 = item.Address2;
                a.Address3 = item.Address3;
                a.Phone = item.Phone; 
                a.Fax = item.Fax;
                a.WebSite = item.WebSite;
                a.ContactPerson = item.ContactPerson;
                a.CountryName = item.CountryName;
                a.CityName = item.CityName;
                a.LocationName = item.LocationName;
                a.CurrencyID = item.CurrencyID;
                //a.ZoneCategoryID = item.ZoneCategoryID;
                if (item.SupplierID != null)
                {
                    a.SupplierID = item.SupplierID;
                    var Supplier = db.SupplierMasters.Find(item.SupplierID);
                    if (Supplier!=null)
                        a.SupplierName = Supplier.SupplierName;

                }

                a.AcHeadID =Convert.ToInt32(item.AcHeadID);
                if (item.AcHeadID!=null)
                {
                    var achead = db.AcHeads.Find(item.AcHeadID);
                    if (achead!=null)
                        a.AcHeadName = db.AcHeads.Find(item.AcHeadID).AcHead1;
                }
                
              
               
                
             

          //      a.BranchID = BranchID;
                a.Email = item.Email;
                if (item.StatusDefault!=null)
                    a.StatusDefault =Convert.ToBoolean(item.StatusDefault);
                a.StatusActive = item.StatusActive;
            }
            return View(a);
        }

       

        [HttpPost]
     
        public ActionResult Edit(FAgentVM item)
        {
            UserRegistration u = new UserRegistration();
            PickupRequestDAO _dao = new PickupRequestDAO();
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            ForwardingAgentMaster a = db.ForwardingAgentMasters.Where(cc => cc.FAgentID == item.FAgentID).FirstOrDefault();    
            
            a.AcCompanyID = accompanyid;
            a.FAgentName = item.FAgentName;
            a.FAgentCode = item.FAgentCode;
            a.Address1 = item.Address1;
            a.Address2 = item.Address2;
            a.Address3 = item.Address3;
            a.Phone = item.Phone;
            a.Fax = item.Fax;
            a.Email = item.Email;
            a.WebSite = item.WebSite;
            a.ContactPerson = item.ContactPerson;
            a.CountryName = item.CountryName;
            a.CityName = item.CityName;
            a.LocationName = item.LocationName;
            a.CurrencyID = Convert.ToInt32(item.CurrencyID);                      
            
            a.AcHeadID = item.AcHeadID;
            if (item.StatusActive!=null)
                a.StatusActive =Convert.ToBoolean(item.StatusActive);
           if (item.StatusDefault!=null)
                a.StatusDefault  = Convert.ToBoolean(item.StatusDefault);
            if (item.SupplierID == 0)
                a.SupplierID = SaveNewSupplier(item);
            else
                a.SupplierID = item.SupplierID;
            db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                
                
                TempData["SuccessMsg"] = "You have successfully Updated Forwarding Agent.";
                return RedirectToAction("Index");
          
            
        }

        [HttpGet]
        public JsonResult GetAgentName()
        {
            var agentlist = (from c1 in db.AgentMasters where c1.StatusActive == true select c1.Name).ToList();

            return Json(new { data = agentlist }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetFwdAgentName(string term)
        {

            if (term.Trim() != "")
            {

                var customerlist = (from c1 in db.ForwardingAgentMasters
                                    where c1.StatusActive == true && c1.FAgentName.ToLower().StartsWith(term.ToLower())
                                    orderby c1.FAgentName ascending
                                    select new { FAgentID = c1.FAgentID, FAgentName = c1.FAgentName }).Take(25).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);

            }
            else
            {

                var customerlist = (from c1 in db.ForwardingAgentMasters
                                    where c1.StatusActive == true 
                                    orderby c1.FAgentName ascending
                                    select new { FAgentID = c1.FAgentID, FAgentName = c1.FAgentName }).Take(25).ToList();
                return Json(customerlist, JsonRequestBehavior.AllowGet);

            }




        }
        //
        // POST: /Agent/Delete/5

        public JsonResult DeleteConfirmed(int id)
        {
            string message = "";
            string status = "OK";
            try
            {
                if (id != 0)
                {
                    DataTable dt = ReceiptDAO.DeleteForwardingAgent(id);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            //if (dt.Rows[0][0] == "OK")
                            TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                            message = dt.Rows[0][1].ToString();
                            status = "OK";
                            return Json(new { status = status, message = message });
                        }

                    }
                    else
                    {
                        message = "Contact Admin";
                        status = "Failed";
                        TempData["ErrorMsg"] = "Error at delete";
                        return Json(new { status = status, message = message });
                    }
                    return Json(new { status = status, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message}); 
            }
            return Json(new { status = "Failed", message ="Contact Admin" });
        }
      
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}