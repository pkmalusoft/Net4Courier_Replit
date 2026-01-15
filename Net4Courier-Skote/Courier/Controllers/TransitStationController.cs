using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class TransitStationController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            List<TransitStationVM> lst = (from c in db.tblTransitStationMasters join t1 in db.CountryMasters on c.CountryID equals t1.CountryID select new TransitStationVM {ID=c.ID,Name=c.Name,IsActive=c.IsActive.Value,Country=t1.CountryName }).ToList();
           

           
            return View(lst);
        }

        public ActionResult Create()
        {
            ViewBag.Country = db.CountryMasters.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(TransitStationVM v)
        {
            tblTransitStationMaster t = new tblTransitStationMaster();
            if (ModelState.IsValid)
            {
                t.Name = v.Name;
                t.CountryID = v.CountryID;
                t.IsActive = v.IsActive;

                db.tblTransitStationMasters.Add(t);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Transit Station.";
                return RedirectToAction("Index");
            }
          
            return View();
        }

        public ActionResult Edit(int id)
        {
            tblTransitStationMaster t = (from c in db.tblTransitStationMasters where c.ID == id select c).First();
            TransitStationVM v = new TransitStationVM();
            ViewBag.Country = db.CountryMasters.ToList();
            if (t == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.ID = t.ID;
                v.Name = t.Name;
                v.CountryID = t.CountryID.Value;
                v.IsActive = t.IsActive.Value;
            }
    
            return View(v);
        }

        [HttpPost]
        public ActionResult Edit(TransitStationVM v)
        {
            tblTransitStationMaster t = new tblTransitStationMaster();
            if (ModelState.IsValid)
            {
                t.ID = v.ID;
                t.Name = v.Name;
                t.CountryID = v.CountryID;
                t.IsActive = v.IsActive;

                db.Entry(t).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Transit Station.";
                return RedirectToAction("Index");
            }
           
          

            return View(v);
        }


        public ActionResult DeleteConfirmed(int id)
        {
            tblTransitStationMaster t = (from c in db.tblTransitStationMasters where c.ID == id select c).FirstOrDefault();
            if (t == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.tblTransitStationMasters.Remove(t);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Transit Station.";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
