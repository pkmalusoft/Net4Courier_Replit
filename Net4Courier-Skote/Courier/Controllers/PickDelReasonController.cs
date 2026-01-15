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
    [SessionExpire]
    public class PickDelReasonController : Controller
    {
         Entities1 db = new Entities1();

      
        public ActionResult Index()
        {
            List<PickUpDelAttemptReason> lst = new List<PickUpDelAttemptReason>();
            var data = (from c in db.PkpDelAttemptReasons select new PickUpDelAttemptReason { ID = c.ID, Reason = c.Reason }).ToList();                       
            
            return View(data);
        }

        //
        // GET: /TypeOfGood/Details/5

        public ActionResult Details(int id = 0)
        {
            PkpDelAttemptReason typeofgood = db.PkpDelAttemptReasons.Find(id);
            if (typeofgood == null)
            {
                return HttpNotFound();
            }
            return View(typeofgood);
        }

        //
        // GET: /TypeOfGood/Create

        public ActionResult Create(int id=0)
        {
            PickUpDelAttemptReason vm = new PickUpDelAttemptReason();
            if (id==0)
            {
                vm.ID = 0;                
            }
            else
            {
                PkpDelAttemptReason v=db.PkpDelAttemptReasons.Find(id);
                vm.ID = v.ID;
                vm.Reason = v.Reason;
            }
            return View(vm);
        }

        //
        // POST: /TypeOfGood/Create

        [HttpPost]
        public ActionResult Create(PkpDelAttemptReason v)
        {

            PkpDelAttemptReason t = new PkpDelAttemptReason();

            

            if (v.ID==0)
            {
                t.Reason = v.Reason;
            }
            else
            {
                t = db.PkpDelAttemptReasons.Find(v.ID);
                t.Reason = v.Reason;
            }

            
                db.PkpDelAttemptReasons.Add(t);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Reason.";
                return RedirectToAction("Index");
                                   
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
            PkpDelAttemptReason item = db.PkpDelAttemptReasons.Find(id);
            db.PkpDelAttemptReasons.Remove(item);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted the Reason";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}