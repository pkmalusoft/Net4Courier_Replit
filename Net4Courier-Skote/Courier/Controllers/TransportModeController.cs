using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    public class TransportModeController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            var data = db.TransportModes.ToList();

            List<TransportModeVM> lst = new List<TransportModeVM>();
            foreach (var item in data)
            {
                TransportModeVM obj = new TransportModeVM();
                obj.TransportModeID = item.TransportModeID;
                obj.TransportModeName = item.Mode;
                lst.Add(obj);
            }

            return View(lst);
        }

        public ActionResult Create()
        {
            return View();
        }



        [HttpPost]

        public ActionResult Create(TransportModeVM c)
        {


            TransportMode obj = new TransportMode();
            int max = (from a in db.TransportModes orderby a.TransportModeID descending select a.TransportModeID).FirstOrDefault();

            if (max == null)
            {
                obj.TransportModeID = 1;
                obj.Mode = c.TransportModeName;
            }
            else
            {
                obj.TransportModeID = max + 1;
                obj.Mode = c.TransportModeName;

            }
            db.TransportModes.Add(obj);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully added Transport Mode.";

            return RedirectToAction("Index");

        }

        public ActionResult Edit(int id)
        {
            TransportModeVM d = new TransportModeVM();
            var data = (from c in db.TransportModes where c.TransportModeID == id select c).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                d.TransportModeID = data.TransportModeID;
                d.TransportModeName = data.Mode;

            }
            return View(d);
        }



        [HttpPost]
        public ActionResult Edit(TransportModeVM a)
        {
            TransportMode d = new TransportMode();
            d.TransportModeID = a.TransportModeID;
            d.Mode = a.TransportModeName;

            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Updated Transport Mode.";

            return RedirectToAction("Index");
        }



        public ActionResult DeleteConfirmed(int id)
        {
            TransportMode mode = db.TransportModes.Find(id);
            db.TransportModes.Remove(mode);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Transport Mode.";
            return RedirectToAction("Index");
        }

    }
}
