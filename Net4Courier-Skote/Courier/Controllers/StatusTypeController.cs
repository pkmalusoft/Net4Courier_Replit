using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    public class StatusTypeController : Controller
    {
        Entities1 db = new Entities1();



        public ActionResult Index()
        {
            List<StatusTypeVM> lst = new List<StatusTypeVM>();
            var data = db.tblStatusTypes.ToList();

            foreach (var item in data)
            {
                StatusTypeVM a = new StatusTypeVM();


                a.ID = item.ID;
                a.Name = item.Name;
                a.Code = item.Code;               

                lst.Add(a);
            }
            return View(lst);
        }

               public ActionResult Details(int id)
        {
            tblStatusType tblstatustype = db.tblStatusTypes.Find(id);
            if (tblstatustype == null)
            {
                return HttpNotFound();
            }
            return View(tblstatustype);
 
        }

        
        public ActionResult Create()
        {
                       
            return View();
        }

        
        [HttpPost]
        public ActionResult Create(StatusTypeVM v)
        {
            if (ModelState.IsValid)
            {

                tblStatusType d = new tblStatusType();

                int max = (from c in db.tblStatusTypes orderby c.ID descending select c.ID).FirstOrDefault();

                if (max == null)
                {
                    d.ID = 1;
                    d.Name = v.Name;
                    d.Code = v.Code;
                    
                }
                else
                {
                    d.ID =max + 1;
                    d.Name = v.Name;
                    d.Code = v.Code;
                    
 

                }



                db.tblStatusTypes.Add(d);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Status Type.";
                return RedirectToAction("Index");
            }
            return View();
        }

        

        public ActionResult Edit(int id)
        {
            StatusTypeVM v = new StatusTypeVM();
            var x = db.tblStatusTypes.Find(id);
            if (x == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.ID = x.ID;
                v.Name = x.Name;
                v.Code = x.Code;
            }
            return View(v);
        }



        [HttpPost]
        public ActionResult Edit(StatusTypeVM v)
        {
            if (ModelState.IsValid)
            {
                tblStatusType c = new tblStatusType();
                c.ID = v.ID;
                c.Name = v.Name;
                c.Code = v.Code;

                db.Entry(c).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully updated Status Type.";
                return RedirectToAction("Index");
            }
            return View();
        }


        public ActionResult DeleteConfirmed(int id)
        {
           
                 tblStatusType statustype = db.tblStatusTypes.Find(id);
                 if (statustype == null)
                {
                    return HttpNotFound();

                }
                else
                {
                    db.tblStatusTypes.Remove(statustype);
                    db.SaveChanges();
                    TempData["SuccessMsg"] = "You have successfully Deleted Status Type.";
                    return RedirectToAction("Index");

                }
               
        }


      
    }
}
