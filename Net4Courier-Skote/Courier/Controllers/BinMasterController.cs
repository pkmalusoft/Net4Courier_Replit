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
    public class BinMasterController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Item
        public ActionResult Index()
        {
            List<VehicleTypeVM> list = (from c in db.tblBinMasters orderby c.BinName select new VehicleTypeVM { ID = c.BinID, Name = c.BinName, VehicleDescription = c.ShortCode }).ToList();
            return View(list);
        }

        public ActionResult Create(int id = 0)
        {
            tblBinMaster vm = new tblBinMaster();
            if (id > 0)
            {
                ViewBag.Title = "Modify";
                vm = db.tblBinMasters.Find(id);

            }
            else
            {
                ViewBag.Title = "Create";
                vm.BinID = 0;


            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(tblBinMaster model)
        {
            tblBinMaster obj = new tblBinMaster();
            if (model.BinID == 0)
            {
                obj.BinName = model.BinName;
                obj.ShortCode = model.ShortCode;
                db.tblBinMasters.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Bin";
            }
            else
            {
                obj = db.tblBinMasters.Find(model.BinID);
                obj.BinName = model.BinName;
                obj.ShortCode = model.ShortCode;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Bin";
            }

            return RedirectToAction("Index");


        }


        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                tblBinMaster move = db.tblBinMasters.Find(id);
                db.tblBinMasters.Remove(move);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Bin";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["SuccessMsg"] = ex.Message;
                return RedirectToAction("Index");
            }

        }
        [HttpGet]
        public JsonResult GetBinList(string term)
        {

            var customerlist = db.tblBinMasters.ToList();

            return Json(customerlist, JsonRequestBehavior.AllowGet);



        }
    }
}