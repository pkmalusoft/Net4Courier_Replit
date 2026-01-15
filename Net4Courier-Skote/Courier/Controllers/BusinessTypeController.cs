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
    public class BusinessTypeController : Controller
    {
         Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<BusinessTypeVM> lst = (from c in db.BusinessTypes select new BusinessTypeVM {Id=c.Id, BusinessType = c.BusinessType1 }) .ToList();
          

           
            return View(lst);
        }

       
        public ActionResult Details(int id = 0)
        {
            BusinessType statu = db.BusinessTypes.Find(id);
            if (statu == null)
            {
                return HttpNotFound();
            }
            return View(statu);
        }

       

        public ActionResult Create()
        {
            //ViewBag.statustype = db.tblStatusTypes.ToList();
            return View();
        }

        //
        // POST: /CourierStatus/Create

        [HttpPost]
       
        public ActionResult Create(BusinessTypeVM c)
        {
            if (ModelState.IsValid)
            {
                BusinessType obj = new BusinessType();               
                obj.BusinessType1 = c.BusinessType;

                //if (max == null)
                //{
                //    obj.CourierStatusID = 1;
                //    obj.CourierStatus = c.CourierStatus;
                  
                //    obj.StatusTypeID = c.StatusTypeID;
                //}
                //else
                //{
                //    obj.CourierStatusID = max + 1;
                //    obj.CourierStatus = c.CourierStatus;
                
                //    obj.StatusTypeID = c.StatusTypeID;
                //}


                db.BusinessTypes.Add(obj);

                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Business Type.";
                return RedirectToAction("Index");
            }

            return View();
        }

       

        public ActionResult Edit(int id)
        {
            BusinessTypeVM obj = new BusinessTypeVM();
            ViewBag.statustype = db.tblStatusTypes.ToList();

            var c = (from d in db.BusinessTypes where d.Id == id select d).FirstOrDefault();

            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.BusinessType = c.BusinessType1;                
            }

            return View(obj);
        }

        //
        // POST: /CourierStatus/Edit/5

        [HttpPost]
   
        public ActionResult Edit(BusinessTypeVM c)
        {
            BusinessType obj = new BusinessType();
            obj.Id = c.Id;
            obj.BusinessType1= c.BusinessType;
                       

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Business Type.";
                return RedirectToAction("Index");
            }
            return View();
        }
    
        public ActionResult DeleteConfirmed(int id)
        {
            BusinessType businesstype = db.BusinessTypes.Find(id);
            db.BusinessTypes.Remove(businesstype);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Business Type.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}