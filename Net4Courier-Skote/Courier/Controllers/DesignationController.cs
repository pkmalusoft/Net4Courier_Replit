using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;


namespace Net4Courier.Controllers
{
    public class DesignationController : Controller
    {
         Entities1 db = new Entities1();

        

        public ActionResult Index()
        {
            List<DesignationVM> lst = new List<DesignationVM>();

            var data = db.Designations.ToList();


            foreach (var item in data)
            {
                DesignationVM s = new DesignationVM();

                s.DesignationID = item.DesignationID;
                s.Designation = item.Designation1;
                //s.StatusDesignation = item.StatusDesignation;
                lst.Add(s);
            }

            return View(data);
        }

        //
        // GET: /Designation/Details/5

        public ActionResult Details(int id = 0)
        {
            Designation designation = db.Designations.Find(id);
            if (designation == null)
            {
                return HttpNotFound();
            }
            return View(designation);
        }

        //
        // GET: /Designation/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Designation/Create

        [HttpPost]
        public ActionResult Create(DesignationVM v)
        {

            if (ModelState.IsValid)
            {
                Designation ob = new Designation();
                int max = (from d in db.Designations orderby d.DesignationID descending select d.DesignationID).FirstOrDefault();


                if (max == null)
                {
                    ob.DesignationID = 1;
                    ob.Designation1 = v.Designation;
                    //ob.StatusDesignation = v.StatusDesignation;
                   
                }
                else
                {
                    ob.DesignationID = max + 1;
                    ob.Designation1 = v.Designation;
                    //ob.StatusDesignation = v.StatusDesignation;
                }




                db.Designations.Add(ob);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Designation.";
                return RedirectToAction("Index");
            }

            return View();
        }

        //
        // GET: /Designation/Edit/5

        public ActionResult Edit(int id)
        {
            DesignationVM d = new DesignationVM();


            var data = (from c in db.Designations where c.DesignationID == id select c).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                d.DesignationID = data.DesignationID;
                d.Designation = data.Designation1;
                //d.StatusDesignation = data.StatusDesignation;

            }
           
            return View(d);
        }

        //
        // POST: /Designation/Edit/5

        [HttpPost]
        public ActionResult Edit(DesignationVM v)
        {

            Designation d = new Designation();
            d.DesignationID = v.DesignationID;
            d.Designation1 = v.Designation;
            //d.StatusDesignation = v.StatusDesignation;

            if (ModelState.IsValid)
            {
                db.Entry(d).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Designation.";
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Designation/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    Designation designation = db.Designations.Find(id);
        //    if (designation == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(designation);
        //}

        ////
        //// POST: /Designation/Delete/5

        //[HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Designation designation = db.Designations.Find(id);
            db.Designations.Remove(designation);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Designation.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}