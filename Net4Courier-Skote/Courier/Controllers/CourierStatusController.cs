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
    public class CourierStatusController : Controller
    {
         Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<CourierStatusVM> lst = (from c in db.CourierStatus join t in db.tblStatusTypes on c.StatusTypeID equals t.ID select new CourierStatusVM { CourierStatusID = c.CourierStatusID, Status = t.Name, CourierStatus = c.CourierStatus, StatusCourier=c.StatusCourier, StatusType=c.StatusType }).ToList();
          

           
            return View(lst);
        }

       
        public ActionResult Details(int id = 0)
        {
            CourierStatu courierstatu = db.CourierStatus.Find(id);
            if (courierstatu == null)
            {
                return HttpNotFound();
            }
            return View(courierstatu);
        }

       

        public ActionResult Create()
        {
            ViewBag.statustype = db.tblStatusTypes.ToList();
            return View();
        }

        //
        // POST: /CourierStatus/Create

        [HttpPost]
       
        public ActionResult Create(CourierStatusVM c)
        {
            if (ModelState.IsValid)
            {
                CourierStatu obj = new CourierStatu();

                int max = (from d in db.CourierStatus orderby d.CourierStatusID descending select d.CourierStatusID).FirstOrDefault();

                if (max == null)
                {
                    obj.CourierStatusID = 1;
                    obj.CourierStatus = c.CourierStatus;
                  
                    obj.StatusTypeID = c.StatusTypeID;
                    string statustype = db.tblStatusTypes.Where(cc => cc.ID == c.StatusTypeID).FirstOrDefault().Name;
                    obj.StatusType = statustype;

                }
                else
                {
                    obj.CourierStatusID = max + 1;
                    obj.CourierStatus = c.CourierStatus;                
                    obj.StatusTypeID = c.StatusTypeID;
                    string statustype = db.tblStatusTypes.Where(cc => cc.ID == c.StatusTypeID).FirstOrDefault().Name;
                    obj.StatusType = statustype;
                }


                db.CourierStatus.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Courier Status.";
                return RedirectToAction("Index");
            }

            return View();
        }

       

        public ActionResult Edit(int id)
        {
            CourierStatusVM obj = new CourierStatusVM();
            ViewBag.statustype = db.tblStatusTypes.ToList();


            var c = (from d in db.CourierStatus where d.CourierStatusID == id select d).FirstOrDefault();

            if (c == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.CourierStatusID =c.CourierStatusID;
                obj.CourierStatus = c.CourierStatus;
                obj.StatusCourier = c.StatusCourier;
                obj.StatusTypeID = c.StatusTypeID;
                string statustype = db.tblStatusTypes.Where(cc => cc.ID == c.StatusTypeID).FirstOrDefault().Name;
                obj.StatusType = statustype;
            }
            return View(obj);
        }

        //
        // POST: /CourierStatus/Edit/5

        [HttpPost]
   
        public ActionResult Edit(CourierStatusVM c)
        {
            CourierStatu obj = new CourierStatu();
            
            obj.CourierStatusID = c.CourierStatusID;
            obj.CourierStatus = c.CourierStatus;
            obj.StatusCourier = c.StatusCourier;
            obj.StatusTypeID = c.StatusTypeID;
            string statustype = db.tblStatusTypes.Where(cc => cc.ID == c.StatusTypeID).FirstOrDefault().Name;
            obj.StatusType = statustype;

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Courier Status.";
                return RedirectToAction("Index");
            }
            return View();
        }

      

     

    
        public ActionResult DeleteConfirmed(int id)
        {
            CourierStatu courierstatu = db.CourierStatus.Find(id);
            db.CourierStatus.Remove(courierstatu);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Courier Status.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}