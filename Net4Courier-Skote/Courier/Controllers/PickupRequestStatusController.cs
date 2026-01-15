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
    public class PickupRequestStatusController : Controller
    {
         Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<PickupRequestStatusVM> lst = (from c in db.PickUpRequestStatus select new PickupRequestStatusVM {Id=c.Id, PickupRequestStatus = c.PickRequestStatus }) .ToList();
          

           
            return View(lst);
        }

       
        public ActionResult Details(int id = 0)
        {
            PickUpRequestStatu statu = db.PickUpRequestStatus.Find(id);
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
       
        public ActionResult Create(PickupRequestStatusVM c)
        {
            if (ModelState.IsValid)
            {
                PickUpRequestStatu obj = new PickUpRequestStatu();               
                obj.PickRequestStatus = c.PickupRequestStatus;

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


                db.PickUpRequestStatus.Add(obj);

                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Pickup Request Status.";
                return RedirectToAction("Index");
            }

            return View();
        }

       

        public ActionResult Edit(int id)
        {
            PickupRequestStatusVM obj = new PickupRequestStatusVM();
            ViewBag.statustype = db.tblStatusTypes.ToList();

            var c = (from d in db.PickUpRequestStatus where d.Id == id select d).FirstOrDefault();

            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.PickupRequestStatus = c.PickRequestStatus;                
            }

            return View(obj);
        }

        //
        // POST: /CourierStatus/Edit/5

        [HttpPost]
   
        public ActionResult Edit(PickupRequestStatusVM c)
        {
            PickUpRequestStatu obj = new PickUpRequestStatu();
            obj.Id = c.Id;
            obj.PickRequestStatus = c.PickupRequestStatus;
                       

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Pickup Request Status.";
                return RedirectToAction("Index");
            }
            return View();
        }
    
        public ActionResult DeleteConfirmed(int id)
        {
            PickUpRequestStatu pickupstatu = db.PickUpRequestStatus.Find(id);
            db.PickUpRequestStatus.Remove(pickupstatu);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted PickupReuest Status.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}