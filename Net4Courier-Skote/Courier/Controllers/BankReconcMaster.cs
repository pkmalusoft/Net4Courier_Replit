using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Models
{
    public class BankReconcMasterController : Controller
    {

        Entities1 db = new Entities1();

        public ActionResult Index()
        {
          

            var data = db.BRMasters.ToList();
            
            return View(data);
        }

        public ActionResult Create(int id=0)
        {
            BRMaster model = new BRMaster(); 
            if (id>0)
            {
                model = db.BRMasters.Find(id);
                ViewBag.Title = "Create";
            }
            else
            {
                ViewBag.Title = "Modify";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(BRMaster v)
        {
            if (ModelState.IsValid)
            {
                db.BRMasters.Add(v);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Bank Reconciliation Status";

            }

            return RedirectToAction("Index");
        }
       
        public ActionResult DeleteConfirmed(int id)
        {
            BRMaster role = db.BRMasters.Find(id);
            db.BRMasters.Remove(role);
            db.SaveChanges();
            TempData["SuccessMSG"] = "You have successfully Deleted Bank Reconciliation Status";
            return RedirectToAction("Index");
        }
    }
}
