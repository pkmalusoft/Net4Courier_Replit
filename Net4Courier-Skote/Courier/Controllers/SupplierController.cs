using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    //[Authorize]
    public class SupplierController : Controller
    {
        Entities1 db = new Entities1();                
        SourceMastersModel ObjectSourceModel = new SourceMastersModel();
        //
        // GET: /Supplier/

        public ActionResult Index(int TypeId=0)
        {
            if (TypeId == 0)
            {
                var SupplierList = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 1 || cc.SupplierTypeID == 2).ToList();
                ViewBag.pTitle = "Manage Supplier";
                ViewBag.TypeId = TypeId;
                return View(SupplierList);
            }
            else if (TypeId > 0)
            {
                var SupplierList = db.SupplierMasters.Where(cc => cc.SupplierTypeID == TypeId).ToList();
                if (TypeId == 3)
                {
                    ViewBag.pTitle = "Manage Fowarding Agent";
                    ViewBag.NewTitle = "New Fowarding Agent";
                }

                else if (TypeId == 4)
                {
                    ViewBag.pTitle = "Manage Delivery Agent";
                    ViewBag.NewTitle = "New Delivery Agent";
                }
                else
                {
                    ViewBag.pTitle = "Manage Supplier";
                    ViewBag.NewTitle = "New Supplier";

                }



                ViewBag.TypeId = TypeId;
                
                return View(SupplierList);
            }
            return View();
        }

        //
        // GET: /Supplier/Details/5

        public ActionResult Details(int id = 0)
        {
            SupplierMaster supplier = db.SupplierMasters.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        //
        // GET: /Supplier/Create

        public ActionResult Create(int TypeId=0)
        {
            SupplierMaster model = new SupplierMaster();
            if (TypeId == 0)
            {
                var supplierMasterTypes = (from d in db.SupplierTypes where (d.SupplierTypeID==1 || d.SupplierTypeID==2) select d).ToList();
                ViewBag.SupplierType = supplierMasterTypes;
                ViewBag.pTitle = "Supplier";
                model.SupplierTypeID = 1;
                model.StatusActive = true;
            }
            else
            {
                var supplierMasterTypes = (from d in db.SupplierTypes where (d.SupplierTypeID == TypeId) select d).ToList();
                ViewBag.SupplierType = supplierMasterTypes;
                model.SupplierTypeID = TypeId;
                model.StatusActive = true;
                if (TypeId == 3)
                {
                    ViewBag.pTitle = "Forwarding Agent";
                   
                }

                else if (TypeId == 4)
                {
                    ViewBag.pTitle = "Delivery Agent";
                    
                }
            }
           
            ViewBag.achead = db.AcHeads.Where(cc => cc.StatusControlAC == true).OrderBy(cc => cc.AcHead1).ToList();
                       
            return View(model);
        }
        public static class DropDownList<T>
        {
            public static SelectList LoadItems(IList<T> collection, string value, string text)
            {
                return new SelectList(collection, value, text);
            }
        }
        //
        // POST: /Supplier/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SupplierMaster supplier)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            //if (ModelState.IsValid)
            //{
            //ViewBag.country = DropDownList<CountryMaster>.LoadItems(

            //ObjectSourceModel.GetCountry(), "CountryID", "CountryName");

            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
          
                var query = (from t in db.SupplierMasters where t.SupplierName == supplier.SupplierName select t).ToList();

                if (query.Count > 0)
                {

                    ViewBag.SuccessMsg = "Supplier name is already exist";
                    return View();
                }
                supplier.BranchID = branchid;
                supplier.AcCompanyID = companyid;
                //supplier.SupplierID = ObjectSourceModel.GetMaxNumberSupplier();
                db.SupplierMasters.Add(supplier);
                db.SaveChanges();
                ReceiptDAO.ReSaveSupplierCode();
                ViewBag.SuccessMsg = "You have successfully added Supplier.";
                int TypeId = 0;
                if (supplier.SupplierTypeID != null)
                    TypeId = Convert.ToInt32(supplier.SupplierTypeID);
                return RedirectToAction("Index", new { TypeId = TypeId });
            
            
            //}

            
        }

        //
        // GET: /Supplier/Edit/5

        public ActionResult Edit(int id = 0)
        {

            var data = db.RevenueTypes.ToList();
            ViewBag.revenue = data;
            SupplierMaster supplier = db.SupplierMasters.Find(id);
            
            var supplierMasterTypes = (from d in db.SupplierTypes  select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            ViewBag.achead = db.AcHeads.Where(cc => cc.StatusControlAC == true).OrderBy(cc => cc.AcHead1).ToList();
            int TypeId = 0;
            if (supplier.SupplierTypeID!=null)
            {
                TypeId = Convert.ToInt32(supplier.SupplierTypeID);
            }
            if (TypeId == 3)
            {
                ViewBag.pTitle = "Forwarding Agent";

            }

            else if (TypeId == 4)
            {
                ViewBag.pTitle = "Delivery Agent";

            }
            else
            {
                ViewBag.pTitle = "Supplier";
            }
            ViewBag.TypeId = TypeId;
            return View(supplier);
        }

        //
        // POST: /Supplier/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SupplierMaster supplier)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                int TypeId = 0;
                if (supplier.SupplierTypeID!=null)
                {
                    TypeId = Convert.ToInt32(supplier.SupplierTypeID);
                }
                ViewBag.SuccessMsg = "You have successfully updated Supplier.";
                return RedirectToAction("Index",new { TypeId = TypeId });
            }
            else
            {
                var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
                ViewBag.SupplierType = supplierMasterTypes;
            }
            return View(supplier);
        }

        //
        // GET: /Supplier/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    Supplier supplier = db.Suppliers.Find(id);
        //    if (supplier == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(supplier);
        //}

        //
        // POST: /Supplier/Delete/5

        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteSupplier(id);
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



        public JsonResult GetID(string supid)
        {
            int sid = Convert.ToInt32(supid);

            string x = (from c in db.SupplierMasters where c.SupplierID == sid select c.RevenueTypeIds).FirstOrDefault();

            return Json(x, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetSupplierCode(string suppliername)
        {
            string status = "ok";
            string customercode = "";
            //List<CourierStatu> _cstatus = new List<CourierStatu>();
            try
            {
                string custform = "000000";
                string maxcustomercode = (from d in db.SupplierMasters orderby d.SupplierID descending select d.ReferenceCode).FirstOrDefault();
                string last6digit = "";
                    if (maxcustomercode == null)
                {
                    //maxcustomercode="AA000000";
                    last6digit = "0";

                }
                else
                {
                    last6digit = maxcustomercode.Substring(maxcustomercode.Length - 6); //, maxcustomercode.Length - 6);
                }
                if (last6digit != "")
                {

                    string customerfirst = suppliername.Substring(0, 1);
                    string customersecond = "";
                    try
                    {
                        customersecond = suppliername.Split(' ')[1];
                        customersecond = customersecond.Substring(0, 1);
                    }
                    catch (Exception ex)
                    {

                    }

                    if (customerfirst != "" && customersecond != "")
                    {
                        customercode = customerfirst + customersecond + String.Format("{0:000000}", Convert.ToInt32(last6digit) + 1);
                    }
                    else
                    {
                        customercode = customerfirst + "S" + String.Format("{0:000000}", Convert.ToInt32(last6digit) + 1);
                    }

                }

                return Json(new { data = customercode, result = status }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = "", result = "failed" }, JsonRequestBehavior.AllowGet);

        }
        public class Rev
        {
            public int RevenueTypeID { get; set; }
            public string RevenueType1 { get; set; }
        }
        public JsonResult GetRevenue()
        {
            List<Rev> lst = new List<Rev>();

            var data = db.RevenueTypes.ToList();

            foreach (var item in data)
            {
                Rev v = new Rev();
                v.RevenueTypeID = item.RevenueTypeID;
                v.RevenueType1 = item.RevenueType1;
                lst.Add(v);

            }
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SupplierType()
        {
            return View(db.SupplierTypes.OrderBy(x => x.SupplierType1).ToList());
        }
        public ActionResult CreateSupplierType()
        {
            return View();
        }

       
        [HttpPost]
        public ActionResult CreateSupplierType(SupplierType suppliertypemaster)
        {
            if (ModelState.IsValid)
            {
                var query = (from t in db.SupplierTypes where t.SupplierType1 == suppliertypemaster.SupplierType1 select t).ToList();

                if (query.Count > 0)
                {

                    ViewBag.SuccessMsg = "Supplier Type already exist";
                    return View();
                }
                db.SupplierTypes.Add(suppliertypemaster);
                db.SaveChanges();
                ViewBag.SuccessMsg = "You have successfully added SupplierType.";
                return View("SupplierType", db.SupplierTypes.ToList());
            }

            return View(suppliertypemaster);
        }
        public ActionResult EditSupplierType(int id = 0)
        {
            SupplierType Supmaster = db.SupplierTypes.Find(id);
            if (Supmaster == null)
            {
                return HttpNotFound();
            }
            return View(Supmaster);
        }

    

        [HttpPost]
        public ActionResult EditSupplierType(SupplierType SupplierTypemaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(SupplierTypemaster).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.SuccessMsg = "You have successfully updated Role.";
                return View("SupplierType", db.SupplierTypes.ToList());
            }
            return View(SupplierTypemaster);
        }

     
        public ActionResult DeletesupplierTypeConfirmed(int id)
        {
            SupplierType suppliertype = db.SupplierTypes.Find(id);
            db.SupplierTypes.Remove(suppliertype);
            db.SaveChanges();
            ViewBag.SuccessMsg = "You have successfully deleted Supplier Type.";
            return View("SupplierType", db.SupplierTypes.ToList());
        }


        [HttpGet]
        public JsonResult GetFwdAgentName(string term)
        {

            if (term.Trim() != "")
            {

                var customerlist = (from c1 in db.SupplierMasters
                                    where c1.StatusActive ==  true && (c1.SupplierTypeID==3 || c1.SupplierTypeID==4) && c1.SupplierName.ToLower().StartsWith(term.ToLower())
                                    orderby c1.SupplierName ascending
                                    select new { FAgentID = c1.SupplierID, FAgentName = c1.SupplierName }).Take(25).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);

            }
            else
            {

                var customerlist = (from c1 in db.SupplierMasters
                                    where c1.StatusActive == true && (c1.SupplierTypeID == 3 || c1.SupplierTypeID == 4)
                                    orderby c1.SupplierName ascending
                                    select new { FAgentID = c1.SupplierID, FAgentName = c1.SupplierName }).Take(25).ToList();
                return Json(customerlist, JsonRequestBehavior.AllowGet);

            }




        }
        //CreateSupplierType
    }
}