using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class ImportController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            var data = db.Imports.ToList();

            return View(data);
        }

        public ActionResult Create()
        {


            ViewBag.import = (from c in db.Imports where c.ImportID == ImportID select c).ToList();
            ViewBag.import = db.Imports.ToList();
            ViewBag.Package = db.Imports.ToList();
            return View();
        }


        [HttpPost]
        public ActionResult Create(ImportVM v)
        {

            if (ModelState.IsValid)
            {
                Import a = new Import();
                int max = (from c in db.Imports orderby c.ImportID descending select c.ImportID).FirstOrDefault();

                a.ImportID = max + 1;
                a.ImportNo = v.ImportNo;
                if (a.ImportDate == null)
                {
                    a.ImportDate = Convert.ToDateTime(v.ImportDate);
                }
                a.Consignor = v.Agent;
                a.FlightNo = v.FlightNo;
                a.MAWBNO = v.MAWBNo;
                a.TotPackage = v.Package;
                a.StatedWeight = v.Weight;
                a.Pieces = v.Pieces;
                



                db.Imports.Add(a);
                db.SaveChanges();

                
                TempData["SuccessMsg"] = "You have successfully added Import Data.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public int ImportID { get; set; }
    }
}
