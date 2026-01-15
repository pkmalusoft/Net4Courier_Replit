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
    public class ZoneNameController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<ZoneNameVM> lst = new List<ZoneNameVM>();
            var data = (from c in db.ZoneMasters select c).ToList();


            foreach (var item in data)
            {
                ZoneNameVM v = new ZoneNameVM();
                v.ZoneID = item.ZoneID;
                v.ZoneName = item.ZoneName;
                v.StatusZone = item.StatusZone;
                v.ZoneType = item.ZoneType;

                lst.Add(v);
            }
            return View(lst);
        }

        public ActionResult Create()
        {
            var zonetype = new SelectList(new[]
                                        {
                                            new { ID = "D", type = "Local" },
                                            new { ID = "I", type = "International" },

                                        },
            "ID", "type", 1);
            ViewBag.zonetype = zonetype;
            //ViewBag.Movement = zonetype;

            return View();
        }

        [HttpPost]
        public ActionResult Create(ZoneNameVM v)
        {
            ZoneMaster a = new ZoneMaster();
            int max = (from c in db.ZoneMasters orderby c.ZoneID descending select c.ZoneID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                a.ZoneID = max + 1;
                a.ZoneName = v.ZoneName;
                a.StatusZone = v.StatusZone;
                a.ZoneType = v.ZoneType;

                db.ZoneMasters.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Zone Name.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            var zonetype = new SelectList(new[]
                                          {
                                            new { ID = "D", type = "Local" },
                                            new { ID = "I", type = "International" },

                                        },
              "ID", "type", "Local");

            ViewBag.zonetype = zonetype;
            //var zonetype = db.CourierMovements.ToList();
            //ViewBag.Movement = zonetype;
            ZoneMaster a = (from c in db.ZoneMasters where c.ZoneID == id select c).FirstOrDefault();
            ZoneNameVM v = new ZoneNameVM();
            v.ZoneID = a.ZoneID;
            v.ZoneName = a.ZoneName;
            v.StatusZone = a.StatusZone;
            v.ZoneType = a.ZoneType;
            return View(v);
        }

        [HttpPost]
        public ActionResult Edit(ZoneNameVM v)
        {
            ZoneMaster a = new ZoneMaster();
            if (ModelState.IsValid)
            {
                a.ZoneID = v.ZoneID;
                a.ZoneName = v.ZoneName;
                a.StatusZone = v.StatusZone;
                a.ZoneType = v.ZoneType;

                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Update Zone Name.";
                return RedirectToAction("Index");
            }

            return View();

        }

      
        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();
            ZoneMaster a = (from c in db.ZoneMasters where c.ZoneID == id select c).FirstOrDefault();
            if (a == null)
            {
                obj.Status = "Failed";
                obj.Message = "Zone Not Found!";
            }
            else
            {
                db.ZoneMasters.Remove(a);
                db.SaveChanges();
              
                obj.Status = "OK";
                obj.Message = "Zone Name Deleted Successfully";
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}
