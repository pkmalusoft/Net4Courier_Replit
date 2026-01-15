using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class DeliveryRunSheetRecieptController : Controller
    { //
        // GET: /OutScan/

        Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<AWBDetailsVM> lst = new List<AWBDetailsVM>();

            return View();
        }



        public ActionResult Details(int id)
        {
            return View();
        }



        public ActionResult Create()
        {
            ViewBag.Department = new SelectList(db.Departments, "DepartmentID", "Department1");
            ViewBag.Deliverdby = new SelectList(db.EmployeeMasters, "EmployeeID", "EmployeeName");
            ViewBag.vehicle = new SelectList(db.VehicleMasters, "VehicleID", "VehicleNo");

            return View();
        }


        [HttpPost]
        public ActionResult Create(int id)
        {

            try
            {


                return View();
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /InScan/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /InScan/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /InScan/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}