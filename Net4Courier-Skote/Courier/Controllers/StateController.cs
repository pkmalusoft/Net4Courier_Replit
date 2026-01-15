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
    public class StateController : Controller
    {
        private Entities1 db = new Entities1();

        //
        // GET: /State/

        public ActionResult Index()
        {
          
            var data = db.States.ToList();
            List<StateVMcs> lst = new List<StateVMcs>();
            foreach (var item in data)
            {
                State s = new State();
                s.StateID = item.StateID;
                s.StateName = item.StateName;
                s.CountryID = item.CountryID;
                
            }
            return View(lst);
        }

        //
        // GET: /State/Details/5

        public ActionResult Details(int id = 0)
        {
            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }
            return View(state);
        }

        //
        // GET: /State/Create

        public ActionResult Create()
        {
            ViewBag.country = db.CountryMasters.ToList();
            return View();
        }

        //
        // POST: /State/Create

        [HttpPost]
       
        public ActionResult Create(StateVMcs c)
        {
            if (ModelState.IsValid)
            {
                State a= new State();
                int max = (from b in db.States orderby b.StateID descending select b.StateID).FirstOrDefault();

                if (max == null)
                {
                    a.StateID = 1;
                    a.StateName = c.StateName;
                    a.CountryID = c.CountryID;
               
                }
                else
                {
                    a.StateID = max + 1;
                    a.StateName = c.StateName;
                    a.CountryID = c.CountryID;
                
                }
                db.States.Add(a);
                db.SaveChanges();


                
                return RedirectToAction("Index");
            }
            return View();
           
        }

        //
        // GET: /State/Edit/5

        public ActionResult Edit(int id = 0)
        {
            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }
            return View(state);
        }

        //
        // POST: /State/Edit/5

        [HttpPost]
     
        public ActionResult Edit(State state)
        {
            if (ModelState.IsValid)
            {
                db.Entry(state).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(state);
        }

        //
        // GET: /State/Delete/5

        public ActionResult Delete(int id = 0)
        {
            State state = db.States.Find(id);
            if (state == null)
            {
                return HttpNotFound();
            }
            return View(state);
        }

        //
        // POST: /State/Delete/5

        [HttpPost, ActionName("Delete")]
     
        public ActionResult DeleteConfirmed(int id)
        {
            State state = db.States.Find(id);
            db.States.Remove(state);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}