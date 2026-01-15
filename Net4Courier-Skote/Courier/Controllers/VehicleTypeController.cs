using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Newtonsoft.Json;
namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class VehicleTypeController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Item
        public ActionResult Index()
        {
            List<VehicleTypeVM> list = (from c in db.tblVehicleTypes orderby c.Name select new VehicleTypeVM { ID=c.ID,Name=c.Name,VehicleDescription=c.VehicleDescription }).ToList();
            return View(list);
        }

        public ActionResult Create(int id=0)
        {
            VehicleTypeVM vm = new VehicleTypeVM();
            if (id>0)
            {
                ViewBag.Title = "Vehicle Type Master - Modify";
               tblVehicleType obj = db.tblVehicleTypes.Find(id);
                vm.ID = obj.ID;
                vm.Name = obj.Name;
                vm.VehicleDescription = obj.VehicleDescription;
            }
            else
            {
                ViewBag.Title = "Vehicle Type Master - Create";
                vm.ID = 0;
                vm.Name = "";
                vm.VehicleDescription = "";

            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(VehicleTypeVM model)
        {
            tblVehicleType obj = new tblVehicleType();
            if (model.ID == 0)
            {
                obj.Name = model.Name;
                obj.VehicleDescription = model.VehicleDescription;
                db.tblVehicleTypes.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Vehicle Type";
            }
            else
            {
                obj = db.tblVehicleTypes.Find(model.ID);
                obj.Name = model.Name;
                obj.VehicleDescription = model.VehicleDescription;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Vehicle Type";
            }
                                   
            return RedirectToAction("Index");


        }
        //[HttpGet]
        //public JsonResult GetPackageType(string term)
        //{
        //    if (term.Trim() != "")
        //    {
        //        var list = (from c in db.tblVehicleTypes where c.Name.Contains(term.Trim()) orderby c.PackageType select new PackageVM { PackageType = c.PackageType }).Distinct().ToList();
        //       return Json(list, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        var list = (from c in db.Packages orderby c.PackageType select new PackageVM { PackageType = c.PackageType }).Distinct().ToList();
        //        return Json(list, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                tblVehicleType move = db.tblVehicleTypes.Find(id);
                db.tblVehicleTypes.Remove(move);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Vehicle Type.";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["SuccessMsg"] = ex.Message;
                return RedirectToAction("Index");
            }
            
        }
          


    }
}