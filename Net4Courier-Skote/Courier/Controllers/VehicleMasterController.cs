using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class VehicleMasterController : Controller
    {
         Entities1 db = new Entities1();

       

        public ActionResult Index()
        {
            List<VehiclesVM> lst = new List<VehiclesVM>();
            lst = PickupRequestDAO.GetVehiclesVM(); //

            //foreach (var item in data)
            //{
            //    VehiclesVM v = new VehiclesVM();

            //    v.VehicleID = item.VehicleID;
            //    v.VehicleDescription = item.VehicleDescription;
            //    v.RegistrationNo = item.RegistrationNo;
            //    v.Model = item.Model;
            //    v.VehicleNO = item.VehicleNo;
            //    lst.Add(v);
            //}


            return View(lst);
        }

        //
        // GET: /VehicleMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            VehicleMaster vehiclemaster = db.VehicleMasters.Find(id);
            if (vehiclemaster == null)
            {
                return HttpNotFound();
            }
            return View(vehiclemaster);
        }

        //
        // GET: /VehicleMaster/Create

        public ActionResult Create()
        {
            ViewBag.VehicleType = db.tblVehicleTypes.ToList();
            ViewBag.employee = db.EmployeeMasters.ToList();
            return View();
        }

        //
        // POST: /VehicleMaster/Create

        [HttpPost]
        public ActionResult Create(VehiclesVM vm)
        {
            
                VehicleMaster v = new VehicleMaster();

                int max = (from d in db.VehicleMasters orderby d.VehicleID descending select d.VehicleID).FirstOrDefault();

                if (max == null)
                {
                    v.VehicleID = 1;
                    v.VehicleDescription = vm.VehicleDescription;
                    v.RegistrationNo = vm.RegistrationNo;
                    v.Model = vm.Model;
                    v.VehicleTypeId= vm.VehicleTypeId;
                    v.VehicleValue = vm.VehicleValue;
                    v.ValueDate = vm.ValueDate;
                    v.PurchaseDate = vm.PurchaseDate;
                    v.RegExpirydate = vm.RegExpirydate;
                    v.AcCompanyID = 1;
                    v.VehicleNo = vm.VehicleNO;
                    if (vm.EmployeeId != 0)
                        v.EmployeeId = vm.EmployeeId;
                    else
                        v.EmployeeId = null;

                }
                else
                {
                    v.VehicleID = max + 1;
                    v.VehicleDescription = vm.VehicleDescription;
                    v.RegistrationNo = vm.RegistrationNo;
                    v.Model = vm.Model;
                    v.VehicleTypeId = vm.VehicleTypeId;
                    v.VehicleValue = vm.VehicleValue;
                    v.ValueDate = vm.ValueDate;
                    v.PurchaseDate = vm.PurchaseDate;
                    v.RegExpirydate = vm.RegExpirydate;
                    v.AcCompanyID = 1;
                    v.VehicleNo = vm.VehicleNO;
                if (vm.EmployeeId != 0)
                    v.EmployeeId = vm.EmployeeId;
                else
                    v.EmployeeId = null;
            }


                db.VehicleMasters.Add(v);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Vehicle.";
                return RedirectToAction("Index");
           

            
        }

        //
        // GET: /VehicleMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ViewBag.VehicleType = db.tblVehicleTypes.ToList();
            ViewBag.employee = db.EmployeeMasters.ToList();
            VehiclesVM v = new VehiclesVM();
            var data = (from d in db.VehicleMasters where d.VehicleID == id select d).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.VehicleID = data.VehicleID;
                v.VehicleDescription = data.VehicleDescription;
                v.RegistrationNo = data.RegistrationNo;
                v.Model = data.Model;

                if (data.VehicleTypeId!=null)
                    v.VehicleTypeId =Convert.ToInt32(data.VehicleTypeId);
                if (data.VehicleValue!=null)
                v.VehicleValue = data.VehicleValue.Value;

                if (data.ValueDate != null)
                    v.ValueDate = data.ValueDate.Value;
                else
                    v.ValueDate = DateTime.Now;

                if (data.PurchaseDate != null)
                    v.PurchaseDate = data.PurchaseDate.Value;
                else
                    v.PurchaseDate = DateTime.Now;

                if (data.RegExpirydate != null)
                    v.RegExpirydate = data.RegExpirydate.Value;
                else
                    v.RegExpirydate = DateTime.Now;

                v.AcCompanyID = 1;
                v.VehicleNO = data.VehicleNo;
                if (data.EmployeeId != null)
                {
                    if (data.EmployeeId != 0)
                        v.EmployeeId = data.EmployeeId.Value;
                }
                
            }
            return View(v);
        }

        //
        // POST: /VehicleMaster/Edit/5

        [HttpPost]
        public ActionResult Edit(VehiclesVM data)
        {
            VehicleMaster v = new VehicleMaster();
            v.VehicleID = data.VehicleID;
                v.VehicleDescription = data.VehicleDescription;
                v.RegistrationNo = data.RegistrationNo;
                v.Model = data.Model;
                v.VehicleTypeId = data.VehicleTypeId;
                v.VehicleValue = data.VehicleValue;
                v.ValueDate = data.ValueDate;
                v.PurchaseDate = data.PurchaseDate;
                v.RegExpirydate = data.RegExpirydate;
                v.AcCompanyID = 1;
                v.VehicleNo = data.VehicleNO;

            if (data.EmployeeId != 0)
                v.EmployeeId = data.EmployeeId;
            else
                v.EmployeeId = null;

           
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Vehicle.";
                return RedirectToAction("Index");
            
            return View();
        }

      
        public ActionResult DeleteConfirmed(int id)
        {
            VehicleMaster vehiclemaster = db.VehicleMasters.Find(id);
            db.VehicleMasters.Remove(vehiclemaster);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Vehicle.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}