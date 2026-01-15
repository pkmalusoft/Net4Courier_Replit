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
    public class TransitStationMasterController : Controller
    {
        private Entities1 db = new Entities1();

        //
        // GET: /TransitStationMaster/

        public ActionResult Index()
        {
            return View(db.tblTransitStationMasters.ToList());
        }

        //
        // GET: /TransitStationMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            tblTransitStationMaster tbltransitstationmaster = db.tblTransitStationMasters.Find(id);
            if (tbltransitstationmaster == null)
            {
                return HttpNotFound();
            }
            return View(tbltransitstationmaster);
        }

        //
        // GET: /TransitStationMaster/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TransitStationMaster/Create

        [HttpPost]
        public ActionResult Create(tblTransitStationMaster tbltransitstationmaster)
        {
            if (ModelState.IsValid)
            {
                db.tblTransitStationMasters.Add(tbltransitstationmaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbltransitstationmaster);
        }

        //
        // GET: /TransitStationMaster/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tblTransitStationMaster tbltransitstationmaster = db.tblTransitStationMasters.Find(id);
            if (tbltransitstationmaster == null)
            {
                return HttpNotFound();
            }
            return View(tbltransitstationmaster);
        }

        //
        // POST: /TransitStationMaster/Edit/5

        [HttpPost]
        public ActionResult Edit(tblTransitStationMaster tbltransitstationmaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbltransitstationmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbltransitstationmaster);
        }

        //
        // GET: /TransitStationMaster/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tblTransitStationMaster tbltransitstationmaster = db.tblTransitStationMasters.Find(id);
            if (tbltransitstationmaster == null)
            {
                return HttpNotFound();
            }
            return View(tbltransitstationmaster);
        }

        //
        // POST: /TransitStationMaster/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            tblTransitStationMaster tbltransitstationmaster = db.tblTransitStationMasters.Find(id);
            db.tblTransitStationMasters.Remove(tbltransitstationmaster);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}