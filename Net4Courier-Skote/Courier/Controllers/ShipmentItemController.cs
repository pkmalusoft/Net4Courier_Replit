using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    public class ShipmentItemController : Controller
    {
         Entities1 db = new Entities1();

       

        public ActionResult Index()
        {
            List<ShipmentItemVM> lst = new List<ShipmentItemVM>();
            var data = db.Items.ToList();

            foreach (var item in data)
            {
                ShipmentItemVM obj = new ShipmentItemVM();
                obj.ItemID = item.ItemID;
                obj.Item = item.ItemName;
                lst.Add(obj);
            }
            return View(lst);
        }

        //
        // GET: /ShipmentItem/Details/5

        public ActionResult Details(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        //
        // GET: /ShipmentItem/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ShipmentItem/Create

        [HttpPost]

        public ActionResult Create(ShipmentItemVM v)
        {
      
            if (ModelState.IsValid)
            {

                Item a = new Item();
                int max = (from d in db.Items orderby d.ItemID descending select d.ItemID).FirstOrDefault();

                if (max == null)
                {
                    a.ItemID = 1;
                    a.ItemName = v.Item;
                }
                else
                {
                    a.ItemID = max + 1;
                    a.ItemName = v.Item;
                }


                db.Items.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Shipment Item.";
                return RedirectToAction("Index");
            }

            return View();
        }

        //
        // GET: /ShipmentItem/Edit/5

        public ActionResult Edit(int id)
        {
            ShipmentItemVM a = new ShipmentItemVM();

            var data = (from d in db.Items where d.ItemID == id select d).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                a.ItemID = data.ItemID;
                a.Item = data.ItemName;
            }
            return View(a);
        }

        //
        // POST: /ShipmentItem/Edit/5

        [HttpPost]
    
        public ActionResult Edit(ShipmentItemVM data)
        {
            Item a = new Item();
            a.ItemID = data.ItemID;
            a.ItemName = data.Item;
            if (ModelState.IsValid)
            {
                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Shipment Item.";
                return RedirectToAction("Index");
            }
            return View();
        }

  
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Shipment Item.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}