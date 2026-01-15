using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class ShipmentHoldRealeaseController : Controller
    { 
        //
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

           
            return View();
        }


        [HttpPost]
        public ActionResult Create(InScanVM v)
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
