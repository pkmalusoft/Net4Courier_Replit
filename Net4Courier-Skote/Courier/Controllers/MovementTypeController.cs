using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class MovementTypeController : Controller
    {

        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            var data = db.CourierMovements.ToList();
            List<MovementTypeVM> lst = new List<MovementTypeVM>();
            foreach (var item in data)
            {
                MovementTypeVM obj = new MovementTypeVM();
                obj.MovementTypeID = item.MovementID;
                obj.MovementTypeName = item.MovementType;
                lst.Add(obj);
            }


            return View(lst);
        }



        public ActionResult Create()
        {
            return View();
        }

      

        [HttpPost]

        public ActionResult Create(MovementTypeVM c)
        {


            CourierMovement obj = new CourierMovement();
            int max = (from a in db.CourierMovements orderby a.MovementID descending select a.MovementID).FirstOrDefault();

            if (max == null)
            {
                obj.MovementID =1;
                obj.MovementType = c.MovementTypeName;
            }
            else
            {
                obj.MovementID = max + 1;
                obj.MovementType = c.MovementTypeName;
             
            }
            db.CourierMovements.Add(obj);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully added Movement Type.";

            return RedirectToAction("Index");

        }


        public ActionResult Edit(int id)
        {
            MovementTypeVM d = new MovementTypeVM();
            var data = (from c in db.CourierMovements where c.MovementID == id select c).FirstOrDefault();
            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                d.MovementTypeID = data.MovementID;
                d.MovementTypeName = data.MovementType;
             
            }
            return View(d);
        }

      

        [HttpPost]
        public ActionResult Edit(MovementTypeVM a)
        {
            CourierMovement d = new CourierMovement();
            d.MovementID = a.MovementTypeID;
            d.MovementType = a.MovementTypeName;
         
            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Updated Movement Type.";

            return RedirectToAction("Index");
        }



        public ActionResult DeleteConfirmed(int id)
        {
            CourierMovement move = db.CourierMovements.Find(id);
            db.CourierMovements.Remove(move);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Movement Type.";
            return RedirectToAction("Index");
        }

       
            
    }
}
