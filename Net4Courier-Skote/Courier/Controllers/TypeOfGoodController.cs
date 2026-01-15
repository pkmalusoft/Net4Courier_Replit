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
    public class TypeOfGoodController : Controller
    {
         Entities1 db = new Entities1();

      
        public ActionResult Index()
        {
            List<TypeOfGoodsVM> lst = new List<TypeOfGoodsVM>();
            var data = db.TypeOfGoods.ToList();

            foreach (var item in data)
            {
                TypeOfGoodsVM v = new TypeOfGoodsVM();
                v.TypeOfGoodID = item.TypeOfGoodID;
                v.TypeOfGood = item.TypeOfGood1;
                lst.Add(v);
            }
            return View(lst);
        }

        //
        // GET: /TypeOfGood/Details/5

        public ActionResult Details(int id = 0)
        {
            TypeOfGood typeofgood = db.TypeOfGoods.Find(id);
            if (typeofgood == null)
            {
                return HttpNotFound();
            }
            return View(typeofgood);
        }

        //
        // GET: /TypeOfGood/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TypeOfGood/Create

        [HttpPost]
        public ActionResult Create(TypeOfGoodsVM v)
        {

            TypeOfGood t = new TypeOfGood();

            int max = (from d in db.TypeOfGoods orderby d.TypeOfGoodID descending select d.TypeOfGoodID).FirstOrDefault();

            if (max == null)
            {
                t.TypeOfGoodID = 1;
                t.TypeOfGood1 = v.TypeOfGood;
            }
            else
            {
                t.TypeOfGoodID = max + 1;
                t.TypeOfGood1 = v.TypeOfGood;
            }

            if (ModelState.IsValid)
            {
                db.TypeOfGoods.Add(t);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Type Of Goods.";
                return RedirectToAction("Index");
            }

            return View();
        }

        //
        // GET: /TypeOfGood/Edit/5

        public ActionResult Edit(int id)
        {
            TypeOfGoodsVM v = new TypeOfGoodsVM();
            var data = (from d in db.TypeOfGoods where d.TypeOfGoodID == id select d).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.TypeOfGoodID = data.TypeOfGoodID;
                v.TypeOfGood = data.TypeOfGood1;

            }
            return View(v);
        }

        //
        // POST: /TypeOfGood/Edit/5

        [HttpPost]
        public ActionResult Edit(TypeOfGoodsVM data)
        {
            TypeOfGood v = new TypeOfGood();
            v.TypeOfGoodID = data.TypeOfGoodID;
            v.TypeOfGood1 = data.TypeOfGood;

            if (ModelState.IsValid)
            {
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Type Of Goods.";
                return RedirectToAction("Index");
            }
            return View();
        }

  
        public ActionResult DeleteConfirmed(int id)
        {
            TypeOfGood typeofgood = db.TypeOfGoods.Find(id);
            db.TypeOfGoods.Remove(typeofgood);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Type Of Goods.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}