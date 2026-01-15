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
    public class ZoneCategoryController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<ZoneCategoryVM> lst = new List<ZoneCategoryVM>();
            var data = (from c in db.ZoneCategories select c).ToList();


            foreach (var item in data)
            {
                ZoneCategoryVM v = new ZoneCategoryVM();
                v.ZoneCategoryID = item.ZoneCategoryID;
                v.ZoneCategory = item.ZoneCategory1;
                v.StatusBaseCategory = item.StatusBaseCategory.Value;

                lst.Add(v);
            }
            return View(lst);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ZoneCategoryVM v)
        {
            ZoneCategory a = new ZoneCategory();
            
            ZoneCategory v1 = (from c in db.ZoneCategories where c.ZoneCategory1 == v.ZoneCategory select c).FirstOrDefault();

            if (v1!=null)
            {
                TempData["SuccessMsg"] = "Duplicate Zone Category not allowed!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                a.ZoneCategory1 = v.ZoneCategory;
                a.StatusBaseCategory = v.StatusBaseCategory;


                db.ZoneCategories.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Zone Category.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            ZoneCategory a = (from c in db.ZoneCategories where c.ZoneCategoryID == id select c).FirstOrDefault();
            ZoneCategoryVM v = new ZoneCategoryVM();
            v.ZoneCategoryID = a.ZoneCategoryID;
            v.ZoneCategory = a.ZoneCategory1;
            v.StatusBaseCategory = a.StatusBaseCategory.Value;

            return View(v);
        }

        [HttpPost]
        public ActionResult Edit(ZoneCategoryVM v)
        {
            ZoneCategory a = new ZoneCategory();

            ZoneCategory v1 = (from c in db.ZoneCategories where c.ZoneCategory1 == v.ZoneCategory && c.ZoneCategoryID!=v.ZoneCategoryID select c).FirstOrDefault();
            if (v1 != null)
            {
                TempData["SuccessMsg"] = "Duplicate Zone Category not allowed!";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                a.ZoneCategoryID = v.ZoneCategoryID;
                a.ZoneCategory1 = v.ZoneCategory;
                a.StatusBaseCategory = v.StatusBaseCategory;

                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Zone Category.";
                return RedirectToAction("Index");
            }

            return View();

        }
       
        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();
            ZoneCategory a = (from c in db.ZoneCategories where c.ZoneCategoryID == id select c).FirstOrDefault();
            if (a == null)
            {
                obj.Status = "Failed";
                obj.Message = "Could not delete this Employee Category!";
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (a.ZoneCategory1 != "Employee")
                {
                    db.ZoneCategories.Remove(a);
                    db.SaveChanges();
                    obj.Status = "OK";
                    obj.Message = "Zone Chart Deleted Successfully";
                }
                else
                {
                    obj.Status = "Failed";
                    obj.Message = "Could not delete this Employee Category!";
                    TempData["SuccessMsg"] = "Could not delete this Employee Category!";
                     
                }
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}
