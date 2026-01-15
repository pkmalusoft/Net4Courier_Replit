using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class CustomerRateTypeController : Controller
    {
         Entities1 db = new Entities1();


        public ActionResult Index()
        {
            //List<CustomerRateTypeVM> lst = (from c in db.CustomerRateTypes join t in db.ZoneCategories on c.ZoneCategoryID equals t.ZoneCategoryID select new CustomerRateTypeVM {CustomerRateTypeID=c.CustomerRateTypeID,CustomerRateType=c.CustomerRateType1,ZoneCategory=t.ZoneCategory1 }).ToList();
            List<CustomerRateTypeVM> lst = (from c in db.CustomerRateTypes select new CustomerRateTypeVM { CustomerRateTypeID = c.CustomerRateTypeID, CustomerRateType = c.CustomerRateType1}).ToList();


            return View(lst);
        }

      

        public ActionResult Details(int id = 0)
        {
            CustomerRateType customerratetype = db.CustomerRateTypes.Find(id);
            if (customerratetype == null)
            {
                return HttpNotFound();
            }
            return View(customerratetype);
        }

     

        public ActionResult Create()
        {
            var ratetypes = new SelectList(new[]
                                         {
                                            new { ID = "1", trans = "BaseWeight" },
                                            new { ID = "2", trans = "BaseMargin" },

                                        },
            "ID", "trans", 1);
            ViewBag.RateType = ratetypes;
            ViewBag.zone = db.ZoneCategories.ToList();
            return View();
        }

        //
        // POST: /CustomerRateType/Create

        [HttpPost]
     
        public ActionResult Create(CustomerRateTypeVM item)
        {
            if (ModelState.IsValid)
            {
                CustomerRateType obj = new CustomerRateType();


                int max = (from c in db.CustomerRateTypes orderby c.CustomerRateTypeID descending select c.CustomerRateTypeID).FirstOrDefault();

                if (max == null)
                {
                    obj.CustomerRateTypeID = 1;
                    obj.CustomerRateType1 = item.CustomerRateType;
                    //obj.ZoneCategoryID = item.ZoneCategoryID;
                    obj.StatusDefault = item.StatusDefault;
                }
                else
                {
                    obj.CustomerRateTypeID = max + 1;
                    obj.CustomerRateType1 = item.CustomerRateType;
                    //obj.ZoneCategoryID = item.ZoneCategoryID;
                    obj.StatusDefault = item.StatusDefault;
                    obj.RateBasedType = item.RateBasedType;
                }
                db.CustomerRateTypes.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Customer Rate Type.";
                return RedirectToAction("Index");
            }


            return View();
        }

        //
        // GET: /CustomerRateType/Edit/5

        public ActionResult Edit(int id)
        {
            CustomerRateTypeVM obj = new CustomerRateTypeVM();
            ViewBag.zone = db.ZoneCategories.ToList();
            var ratetypes = new SelectList(new[]
                                        {
                                            new { ID = "1", trans = "BaseWeight" },
                                            new { ID = "2", trans = "BaseMargin" },

                                        },
           "ID", "trans", 1);
            ViewBag.RateType = ratetypes;
            var item = (from c in db.CustomerRateTypes where c.CustomerRateTypeID == id select c).FirstOrDefault();

            if (item == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.CustomerRateTypeID = item.CustomerRateTypeID;
                obj.CustomerRateType = item.CustomerRateType1;
                //obj.ZoneCategoryID = item.ZoneCategoryID.Value;
                obj.StatusDefault = item.StatusDefault.Value;
                if (item.RateBasedType == null)
                {
                    obj.RateBasedType = 1;
                }
                else
                {
                    obj.RateBasedType = Convert.ToInt32(item.RateBasedType.Value);

                }
            }
         
            return View(obj);
        }

    

        [HttpPost]
     
        public ActionResult Edit(CustomerRateTypeVM item)
        {
            CustomerRateType obj = new CustomerRateType();
            obj.CustomerRateTypeID = item.CustomerRateTypeID;
            obj.CustomerRateType1 = item.CustomerRateType;
            //obj.ZoneCategoryID = item.ZoneCategoryID;
            obj.StatusDefault = item.StatusDefault;
            obj.RateBasedType = item.RateBasedType;
            if (ModelState.IsValid)
            {

                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Customer Rate Type.";
                return RedirectToAction("Index");
            }
         
            return View();
        }

       
        
        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();

            CustomerRate a = db.CustomerRates.Where(cc => cc.CustomerRateTypeID == id).FirstOrDefault();
            try
            {
                if (a != null)
                {
                    obj.Message = "Customer Rate Delete Failed,Rate Type has Rate Details!";
                    obj.Status = "Failed";
                }
                else
                {
                    CustomerRateType customerratetype = db.CustomerRateTypes.Find(id);
                    db.CustomerRateTypes.Remove(customerratetype);
                    db.SaveChanges();

                    obj.Message = "Customer Rate Type Deleted Succesfully!";
                    obj.Status = "OK";
                }
            }
            catch (Exception ex)
            {
                obj.Status = "Failed";
                obj.Message = ex.Message;
            }
            return Json(obj, JsonRequestBehavior.AllowGet);

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}