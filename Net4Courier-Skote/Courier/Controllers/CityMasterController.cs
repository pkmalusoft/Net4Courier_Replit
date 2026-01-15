using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;

namespace Net4Courier.Controllers
{
    public class CityMasterController : Controller
    {
        
         Entities1 db = new Entities1();

        //
        // GET: /CityMaster/

        public ActionResult Index()
        {
            List<CityVM> lst = (from c in db.CityMasters join t in db.CountryMasters on c.CountryID equals t.CountryID select new CityVM { CityID = c.CityID, CityCode = c.CityCode, City = c.City, Country = t.CountryName }).ToList();
           
            return View(lst);
        }

        //
        // GET: /CityMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            CityMaster citymaster = db.CityMasters.Find(id);
            if (citymaster == null)
            {
                return HttpNotFound();
            }
            return View(citymaster);
        }

        //
        // GET: /CityMaster/Create

        public ActionResult Create()
        {
            ViewBag.country = db.CountryMasters.ToList();
            return View();
        }

        //
        // POST: /CityMaster/Create

        [HttpPost]
       
        public ActionResult Create(CityMaster citymaster)
        {
            //if (ModelState.IsValid)
            //{
            //    db.CityMasters.Add(citymaster);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            db.CityMasters.Add(citymaster);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully added City.";

            return RedirectToAction("Index");
        }

        //
        // GET: /CityMaster/Edit/5

        public ActionResult Edit(int id)
        {
            CityVM c = new CityVM();
            ViewBag.country = db.CountryMasters.ToList();

            var data = (from d in db.CityMasters where d.CityID == id select d).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                c.CityID = data.CityID;
                c.CityCode = data.CityCode;
                c.City = data.City;
                c.CountryID = data.CountryID.Value;
                if (data.IsHub !=null)
                {
                    c.IsHub = data.IsHub.Value;
                }
                
            }
            return View(c);
        }

        //
        // POST: /CityMaster/Edit/5

        [HttpPost]
       
        public ActionResult Edit(CityVM c)
        {
            CityMaster t = new CityMaster();
            t.CityID = c.CityID;
            t.CityCode = c.CityCode;
            t.City = c.City;
            t.CountryID = c.CountryID;
            t.IsHub = c.IsHub;

            if (ModelState.IsValid)
            {
                db.Entry(t).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated City.";
                return RedirectToAction("Index");
            }
            return View();
        }

    
    
     
        public ActionResult DeleteConfirmed(int id)
        {
            CityMaster citymaster = db.CityMasters.Find(id);
            db.CityMasters.Remove(citymaster);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted City.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}