using Net4Courier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    public class ExportController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            var data = db.Imports.ToList();

            return View(data);
        }

        public ActionResult Create()
        {


            ViewBag.export = (from c in db.Imports where c.ExportNo == ExportNo select c).ToList();
            ViewBag.export = db.Imports.ToList();
            
            return View();
        }


        [HttpPost]
        public ActionResult Create(ExportVM v)
        {

            if (ModelState.IsValid)
            {
                Import a = new Import();
                int max = (from c in db.Imports orderby c.ImportID descending select c.ImportID).FirstOrDefault();

                a.ImportID = max + 1;
                a.ExportNo = v.ExportNo;
                a.ExportDate = v.ExportDate;

               




                db.Imports.Add(a);
                db.SaveChanges();


                TempData["SuccessMsg"] = "You have successfully added Export Data.";
                return RedirectToAction("Index");
            }
            return View();
        }

       

        public int? ExportNo { get; set; }
    }
}
