using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class UserTypeController : Controller
    {
         Entities1 db = new Entities1();


        public ActionResult Index()
        {
            List<UserTypeVM> lst = new List<UserTypeVM>();
            var data = db.UserTypes.ToList();
            foreach (var item in data)
            {
                UserTypeVM obj = new UserTypeVM();
                obj.UserTypeID = item.ID;
                    obj.Name=item.UserType1;
                lst.Add(obj);
            }
            return View(lst);
        }


        public ActionResult Details(int id = 0)
        {
            UserType usertype = db.UserTypes.Find(id);
            if (usertype == null)
            {
                return HttpNotFound();
            }
            return View(usertype);
        }

     

        public ActionResult Create()
        {
            return View();
        }

      

        [HttpPost]
        public ActionResult Create(UserTypeVM item)
        {
            if (ModelState.IsValid)
            {
                UserType obj = new UserType();



                int max = (from c in db.UserTypes orderby c.ID descending select c.ID).FirstOrDefault();

                if (max == null)
                {

                    obj.ID = 1;
                    obj.UserType1 = item.Name;
                }
                else
                {
                    obj.ID = max + 1;
                    obj.UserType1 = item.Name;
                }

                db.UserTypes.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added UserType.";
                return RedirectToAction("Index");
            }

            return View();
        }

      

        public ActionResult Edit(int id)
        {
            UserTypeVM obj = new UserTypeVM();
            var data = (from c in db.UserTypes where c.ID == id select c).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.UserTypeID = data.ID;
                obj.Name = data.UserType1;
            }
            return View(obj);
        }

        //
        // POST: /UserType/Edit/5

        [HttpPost]
        public ActionResult Edit(UserTypeVM user)
        {
            UserType obj = new UserType();
            obj.ID = user.UserTypeID;
            obj.UserType1 = user.Name;

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMsg"] = "You have successfully Updated UserType.";

                return RedirectToAction("Index");
            }
            return View();
        }

     
    
        public ActionResult DeleteConfirmed(int id)
        {
            UserType usertype = db.UserTypes.Find(id);
            db.UserTypes.Remove(usertype);
            db.SaveChanges();

            TempData["SuccessMsg"] = "You have successfully Deleted UserType.";

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}