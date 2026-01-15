using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Newtonsoft.Json;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class VehicleBinMasterController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Item
        public ActionResult Index()
        {
           
            List<VehicleBinMasterVM> list = PickupRequestDAO.GetVehiclesBinList();
            return View(list);
        }

        public ActionResult Create(int id=0)
        {
            
            VehicleBinMasterVM vm = new VehicleBinMasterVM();
            if (id>0)
            {
                ViewBag.Title = "Modify";
               tblVehicleBin obj = db.tblVehicleBins.Find(id);
                vm.VehicleBinID = obj.VehicleBinID;
                vm.BinIds = obj.BinIds;
                vm.VehicleId = obj.VehicleId;
                ViewBag.Vehicles = (from c in db.VehicleMasters select new { VehicleID = c.VehicleID, VehicleDescription = c.RegistrationNo + "-" + c.VehicleDescription }).ToList();
            }
            else
            {
                ViewBag.Vehicles = PickupRequestDAO.GetPendingVehicleforBin();
                
                ViewBag.Title = "Create";
                vm.VehicleBinID = 0;                
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(VehicleBinMasterVM model)
        {
            tblVehicleBin obj = new tblVehicleBin();
            if (model.VehicleBinID == 0)
            {
                obj.VehicleId = model.VehicleId;
                obj.BinIds = "";
                foreach (var item in model.SelectedValues)
                {
                    if (obj.BinIds == "")
                    {
                        obj.BinIds = item.ToString();
                    }
                    else
                    {
                        obj.BinIds = obj.BinIds + "," + item.ToString();
                    }

                }
                db.tblVehicleBins.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Vehicle Bin";
            }
            else
            {
                obj = db.tblVehicleBins.Find(model.VehicleBinID);
                obj.VehicleId = model.VehicleId;
                obj.BinIds = "";
                foreach(var item in model.SelectedValues)
                {
                    if (obj.BinIds=="")
                    {
                        obj.BinIds = item.ToString();
                    }
                    else
                    {
                        obj.BinIds = obj.BinIds + "," + item.ToString();
                    }

                }
                //obj.BinIds = model.BinIds;
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
                tblVehicleBin move = db.tblVehicleBins.Find(id);
                db.tblVehicleBins.Remove(move);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Bin";
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