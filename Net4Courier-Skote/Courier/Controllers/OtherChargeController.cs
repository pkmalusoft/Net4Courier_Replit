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
    //[Authorize]
    public class OtherChargeController : Controller
    {
        SourceMastersModel objSourceMastersModel = new SourceMastersModel();
        //SourceMastersModel objectSourceModel = new SourceMastersModel();
        //SHIPPING_FinalEntities db = new SHIPPING_FinalEntities();
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            List<RevenueAcHeadVM> model = new List<RevenueAcHeadVM>();
            var query = (from t in db.OtherCharges
                         join t1 in db.AcHeads on t.AcHeadID equals t1.AcHeadID
                         select new OtherChargeVM

                         {
                             OtherCharge1=t.OtherCharge1,                             
                             AcHeadID=t1.AcHeadID,
                             AcHead =t1.AcHead1,
                             OtherChargeID=t.OtherChargeID,
                             TypePercent=t.TypePercent,
                             Reimbursement=t.Reimbursement,
                             TaxApplicable=t.TaxApplicable,
                             SetAmount =t.SetAmount

                         }).ToList();
       
            return View(query);
        }

     

        public ActionResult Details(int id = 0)
        {
            
            var othercharge = db.OtherCharges.Find(id);
            if (othercharge == null)
            {
                return HttpNotFound();
            }
            return View(othercharge);
        }
     
        public ActionResult Create()
        {
          //  ViewBag.AcheadId = acheadid;

            ViewBag.accounthead = db.AcHeads.OrderBy(x => x.AcHead1).ToList();
            return View();
        }     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OtherChargeVM vm)
        {
            if (ModelState.IsValid)
            {

                int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
                var query = (from t in db.OtherCharges where t.OtherCharge1 == vm.OtherCharge1 select t).ToList();

                if (query.Count > 0)
                {
                    ViewBag.accounthead = db.AcHeads.ToList();
                    ViewBag.SuccessMsg = "OtherCharge name is already exist!";
                    return View();
                }
                else
                {
                    OtherCharge data = new OtherCharge();
                    int max1 = (from c1 in db.OtherCharges orderby c1.OtherChargeID descending select c1.OtherChargeID).FirstOrDefault();
                    data.OtherChargeID = max1+1;
                    data.OtherCharge1 = vm.OtherCharge1;
                    data.SetAmount = vm.SetAmount;
                    data.TypePercent = vm.TypePercent;
                    data.Reimbursement = vm.Reimbursement;
                    data.TaxApplicable = vm.TaxApplicable;
                    data.AcHeadID = vm.AcHeadID;
                    data.AcCompanyID = companyId;
                    db.OtherCharges.Add(data);
                    db.SaveChanges();
                }
                
                                
                TempData["SuccessMSG"] = "You have successfully added Other Charge.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }


        public JsonResult GetOtherChargeTypeName(string name,string achead)
        {

            int ac = Convert.ToInt32(achead);
            var revenue = (from c in db.OtherCharges where c.OtherCharge1 == name && c.AcHeadID==ac select c).FirstOrDefault();
            
            Status obj = new Status();

            if (revenue == null)
            {
                obj.flag = 0;

            }
            else
            {
                obj.flag = 1;
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public class Status
        {
            public int flag { get; set; }
        }

        public ActionResult Edit(int id = 0)
        {
            ViewBag.accounthead = db.AcHeads.OrderBy(x => x.AcHead1).ToList();
            OtherCharge otherCharge= db.OtherCharges.Find(id);
            if (otherCharge == null)
            {
                return HttpNotFound();
            }
            
            return View(otherCharge);
        }

    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OtherCharge vm)
        {
            if (ModelState.IsValid)
            {
                OtherCharge data = db.OtherCharges.Find(vm.OtherChargeID);
                data.OtherCharge1 = vm.OtherCharge1;
                data.SetAmount = vm.SetAmount;
                data.TypePercent = vm.TypePercent;
                data.Reimbursement = vm.Reimbursement;
                data.TaxApplicable = vm.TaxApplicable;                
                data.AcHeadID = vm.AcHeadID;
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
                
                TempData["SuccessMSG"] = "You have successfully updated Other Charge.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }
            
        
      
        public ActionResult DeleteConfirmed(int id)
        {
            OtherCharge item = db.OtherCharges.Find(id);
            db.OtherCharges.Remove(item);
            db.SaveChanges();
            TempData["SuccessMSG"] = "You have successfully deleted Other Charge.";
            return RedirectToAction("Index");
        }

     
    }
}