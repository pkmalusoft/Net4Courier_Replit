using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ProductTypeController : Controller
    {
        private Entities1 db = new Entities1();

       

        public ActionResult Index()
        {
            List<ProductTypeVM> lst = new List<ProductTypeVM>();
            var data = (from c in db.ProductTypes join d in db.ParcelTypes on c.ParcelTypeID equals d.ID join m in db.CourierMovements on c.TransportModeID equals m.MovementID select new ProductTypeVM { ProductTypeID=c.ProductTypeID,ProductName=c.ProductName,ParcelType=d.ParcelType1,MovementType=m.MovementType}).ToList();

          
            return View(data);
        }

       

        public ActionResult Create()
        {
            ViewBag.ParcelType = db.ParcelTypes.ToList();

            ViewBag.transport = db.CourierMovements.ToList(); // .TransportModes.ToList();
         
            return View();
        }

   

        [HttpPost]
        public ActionResult Create(ProductTypeVM v)
        {

            ProductType a = new ProductType();
            if (ModelState.IsValid)
            {

                int max = (from c in db.ProductTypes orderby c.ProductTypeID descending select c.ProductTypeID).FirstOrDefault();
                a.ProductTypeID = max + 1;
                a.ProductName = v.ProductName;
            
                a.ParcelTypeID = v.ParcelTypeID;
           
                a.TransportModeID = v.TransportModeID;
                a.CBMBasedCharges = v.CBMbasedCharges;
                a.Length = v.Length;
                a.Width = v.Width;
                a.Height = v.Height;
                a.CBM = v.CBM;
                a.VolumeMetricBased = v.VolumeMetricBased;
                a.VolumeWeight = v.VolumeWeight;
                a.CustomBox = v.CustomBox;
                db.ProductTypes.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Product Type.";
                return RedirectToAction("Index");

            }
          
            return View(v);
        }

       

        public ActionResult Edit(int id = 0)
        {
            ViewBag.ParcelType = db.ParcelTypes.ToList();

            ViewBag.transport = db.CourierMovements.ToList(); //db.TransportModes.ToList();

            ProductTypeVM v = new ProductTypeVM();
            ProductType a = (from c in db.ProductTypes where c.ProductTypeID == id select c).FirstOrDefault();

            if (a == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.ProductTypeID = a.ProductTypeID;
                v.ProductName = a.ProductName;
                
                v.ParcelTypeID = a.ParcelTypeID;

                v.TransportModeID = a.TransportModeID;
                v.CBMbasedCharges = a.CBMBasedCharges;
                if (a.Length!=null)
                v.Length = a.Length.Value;
                if (a.Width!=null)
                    v.Width = a.Width.Value;
                if (a.Height != null)
                    v.Height = a.Height.Value;

                if (a.CBM != null)
                {
                    v.CBM = a.CBM.Value;
                }
                    if (a.VolumeWeight != null)
                    v.VolumeWeight = a.VolumeWeight.Value;

                v.VolumeMetricBased = a.VolumeMetricBased;
                v.CustomBox = a.CustomBox;
            }
            return View(v);
        }

       
      

        [HttpPost]
        public ActionResult Edit(ProductTypeVM a)
        {

            if (ModelState.IsValid)
            {
                ProductType v = new ProductType();

                v.ProductTypeID = a.ProductTypeID;
                v.ProductName = a.ProductName;

                v.ParcelTypeID = a.ParcelTypeID;

                v.TransportModeID = a.TransportModeID;
                v.CBMBasedCharges = a.CBMbasedCharges;
                v.Length = a.Length;
                v.Width = a.Width;
                v.Height = a.Height;
                v.CBM = a.CBM;
                v.VolumeMetricBased = a.VolumeMetricBased;
                v.VolumeWeight = a.VolumeWeight;
                v.CustomBox = a.CustomBox;
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Product Type.";
                return RedirectToAction("Index");
            }
            return View();
        }

       

      
        public ActionResult DeleteConfirmed(int id)
        {
            ProductType a = db.ProductTypes.Find(id);
            db.ProductTypes.Remove(a);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Product Type.";
            return RedirectToAction("Index");
        }

     
    }
}