using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ShipmentModeController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<tblShipmentMode> lst = new List<tblShipmentMode>();
            var data = (from c in db.tblShipmentModes select c).ToList();

            
            return View(data);
        }

        public ActionResult Create(int id=0)
        {
            tblShipmentMode vm = new tblShipmentMode();
            if (id > 0)
            {
                vm = db.tblShipmentModes.Find(id);
            }
             return View(vm);
        }

        [HttpPost]
        public ActionResult Create(tblShipmentMode v)
        {
            tblShipmentMode a = new tblShipmentMode();
            if (v.ID >0)
            {
                a = db.tblShipmentModes.Find(v.ID);
            }
            if (ModelState.IsValid)
            {
                a.ShipmentMode = v.ShipmentMode;
                a.Prefix = v.Prefix;
                a.ByDefault = v.ByDefault;

                if (a.ID == 0)
                {
                    db.tblShipmentModes.Add(a);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(a).State = EntityState.Modified;
                    db.SaveChanges();
                   
                }
                if (v.ByDefault == true)
                {
                    var list = db.tblShipmentModes.ToList();
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item.ID != a.ID)
                            {
                                item.ByDefault = false;
                            }
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                TempData["SuccessMsg"] = "You have successfully added Shipment Mode";
                return RedirectToAction("Index");
            }
            return View();
        }
 
        public ActionResult DeleteConfirmed(int id)
        {
            tblShipmentMode a = (from c in db.tblShipmentModes where c.ID == id select c).FirstOrDefault();
            if (a == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.tblShipmentModes.Remove(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Shipment Mode";
                return RedirectToAction("Index");
            }
        }
    }
}
