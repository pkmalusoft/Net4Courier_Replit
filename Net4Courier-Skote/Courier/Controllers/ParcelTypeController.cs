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
    public class ParcelTypeController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            var data = db.ParcelTypes.ToList();
            List<ParcelTypeVM> lst = new List<ParcelTypeVM>();
            foreach (var Item in data)
            {
                ParcelTypeVM obj = new ParcelTypeVM();
                if (Item.ParcelTypeID == 0)
                {
                    obj.DescID = 0;
                    obj.Descr = "Docs";
                }
                else if (Item.ParcelTypeID == 1)
                {
                    obj.DescID = 1;
                    obj.Descr = "Non Docs";
                }

                obj.Description = Item.ParcelType1;
                obj.id = Item.ID;


                lst.Add(obj);
            }
             
            return View(lst);
        }


        public ActionResult Create()
        {
            var doctype = new SelectList(new[] 
                                        {
                                            new { DescID = "0", Desc = "Docs" },
                                            new { DescID = "1", Desc = "Non Docs" },
                                           
                                        },
            "DescID", "Desc", 1);

            return View();
        }



        [HttpPost]

        public ActionResult Create(ParcelTypeVM c)
        {


            ParcelType obj = new ParcelType();
            int max = (from a in db.ParcelTypes orderby a.ID descending select a.ID).FirstOrDefault();

            max = max + 1;

            obj.ID = max;
            obj.ParcelTypeID = c.DescID;
            obj.ParcelType1= c.Description;

            
            db.ParcelTypes.Add(obj);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully added Parcel Type.";

            return RedirectToAction("Index");

        }

        public ActionResult Edit(int id)
        {
            ParcelTypeVM d = new ParcelTypeVM();
            var data = (from c in db.ParcelTypes where c.ID == id select c).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                d.id = data.ID;
                d.DescID = data.ParcelTypeID.Value;
                d.Description = data.ParcelType1;
                
            }
            var doctype = new SelectList(new[] 
                                        {
                                            new { ID = "0", Desc = "Docs" },
                                            new { ID = "1", Desc = "Non Docs" },
                                           
                                        },
           "ID", "Desc", 1);

            return View(d);
        }

      

        [HttpPost]
        public ActionResult Edit(ParcelTypeVM a)
        {
            ParcelType d = new ParcelType();
            d.ParcelTypeID = a.DescID;
            d.ParcelType1 = a.Description;
            d.ID = a.id;
           
            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Updated Parcel Type.";

            return RedirectToAction("Index");
        }



        public ActionResult DeleteConfirmed(int id)
        {
            ParcelType type = db.ParcelTypes.Find(id);
            db.ParcelTypes.Remove(type);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Parcel Type.";
            return RedirectToAction("Index");
        }

       
      
            

    }
}
