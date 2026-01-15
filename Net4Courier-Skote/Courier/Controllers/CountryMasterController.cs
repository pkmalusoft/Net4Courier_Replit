using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class CountryMasterController : Controller
    {
        private Entities1 db = new Entities1();

        //
        // GET: /CountryMaster/

        public ActionResult Index()
        {
            var data = db.CountryMasters.ToList();
            List<CountryMasterVM> lst = new List<CountryMasterVM>();
            foreach(var Item in data)
            {
                CountryMasterVM obj = new CountryMasterVM();
                obj.CountryID = Item.CountryID;
                obj.CountryName = Item.CountryName;
                obj.CountryCode = Item.CountryCode;
                obj.IATACode = Item.IATACode;
                lst.Add(obj);
            }
             
            return View(lst);
        }

        //
        // GET: /CountryMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            CountryMaster countrymaster = db.CountryMasters.Find(id);
            if (countrymaster == null)
            {
                return HttpNotFound();
            }
            return View(countrymaster);
        }

        //
        // GET: /CountryMaster/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /CountryMaster/Create

        [HttpPost]

        public ActionResult Create(CountryMasterVM c)
        {


            CountryMaster obj = new CountryMaster();
            int max = (from a in db.CountryMasters orderby a.CountryID descending select a.CountryID).FirstOrDefault();

            if (max == null)
            {
                obj.CountryID = 1;
                obj.CountryName = c.CountryName;
                obj.CountryCode = c.CountryCode;
                obj.IATACode = c.IATACode;
            }
            else
            {
                obj.CountryID = max + 1;
                obj.CountryName = c.CountryName;
                obj.CountryCode = c.CountryCode;
                obj.IATACode = c.IATACode;
            }
            db.CountryMasters.Add(obj);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully added Country.";

            return RedirectToAction("Index");

        }
      
            
      


        //
        // GET: /CountryMaster/Edit/5


        public ActionResult Edit(int id)
        {
            CountryMasterVM d = new CountryMasterVM();
            var data = (from c in db.CountryMasters where c.CountryID == id select c).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                d.CountryID = data.CountryID;
                d.CountryName = data.CountryName;
                d.CountryCode = data.CountryCode;
                d.IATACode = data.IATACode;
            }
            return View(d);
        }

        //
        // POST: /CountryMaster/Edit/5

        [HttpPost]
        public ActionResult Edit(CountryMasterVM a)
        {
            CountryMaster d = new CountryMaster();
            d.CountryID = a.CountryID;
            d.CountryName = a.CountryName;
            d.CountryCode = a.CountryCode;
            d.IATACode = a.IATACode;

            //if (ModelState.IsValid)
            //{
            //    db.Entry(d).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //return View();
            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Updated Country.";

            return RedirectToAction("Index");
        }

       
       
        public ActionResult DeleteConfirmed(int id)
        {
            CountryMaster countrymaster = db.CountryMasters.Find(id);
            db.CountryMasters.Remove(countrymaster);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Country.";
            return RedirectToAction("Index");
        }

       
    }
}