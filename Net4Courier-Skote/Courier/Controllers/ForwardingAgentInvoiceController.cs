using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class ForwardingAgentInvoiceController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            SupplierInvoiceSearch obj = (SupplierInvoiceSearch)Session["FwdInvoiceSearch"];
            SupplierInvoiceSearch model = new SupplierInvoiceSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new SupplierInvoiceSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.InvoiceNo = "";
                Session["FwdInvoiceSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.InvoiceNo = "";
            }
            else
            {
                model = obj;
            }
            List<SupplierInvoiceVM> lst = PickupRequestDAO.GetForwardingInvoiceList(obj.FromDate, obj.ToDate, model.InvoiceNo, yearid);
            model.Details = lst;

            return View(model);


        }
        [HttpPost]
        public ActionResult Index(SupplierInvoiceSearch obj)
        {
            Session["FwdInvoiceSearch"] = obj;
            return RedirectToAction("Index");
        }
      
        public ActionResult Create(int id=0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            var suppliers = db.SupplierMasters.ToList();
            ViewBag.Supplier = suppliers;
            SupplierInvoiceVM _supinvoice = new SupplierInvoiceVM();
            ViewBag.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
            if (id > 0)
            {
                ViewBag.Title = "Forwarding Agent Invoice- Modify";
                var _invoice = db.SupplierInvoices.Find(id);
                _supinvoice.SupplierInvoiceID = _invoice.SupplierInvoiceID;
                _supinvoice.InvoiceDate = _invoice.InvoiceDate;
                _supinvoice.InvoiceNo = _invoice.InvoiceNo;
                _supinvoice.SupplierID = _invoice.SupplierID;
                _supinvoice.Remarks = _invoice.Remarks;
                var supplier = suppliers.Where(d => d.SupplierID == _invoice.SupplierID).FirstOrDefault();
                if (supplier != null)
                {
                    _supinvoice.SupplierName = supplier.SupplierName;
                    _supinvoice.SupplierTypeId = Convert.ToInt32(supplier.SupplierTypeID);
                }
                
                List<SupplierInvoiceDetailVM> _details = new List<SupplierInvoiceDetailVM>();
                _details = (from c in db.SupplierInvoiceDetails
                    join a in db.AcHeads on c.AcHeadID equals a.AcHeadID
                  where c.SupplierInvoiceID == id
               select new SupplierInvoiceDetailVM { SupplierInvoiceDetailID = c.SupplierInvoiceDetailID, SupplierInvoiceID = c.SupplierInvoiceID, AcHeadId = c.AcHeadID, AcHeadName = a.AcHead1, Particulars = c.Particulars, TaxPercentage = c.TaxPercentage, CurrencyID = c.CurrencyID, Amount = c.Amount, Rate = c.Rate, Quantity = c.Quantity, Value = c.Value, ItemTypeId = 0 }).ToList();
                _supinvoice.Details = _details;
            }
            else{
                ViewBag.Title = "Forwarding Agent Invoice";
                _supinvoice.SupplierInvoiceID = 0;
                _supinvoice.InvoiceDate = CommonFunctions.GetCurrentDateTime();
                _supinvoice.InvoiceNo = ReceiptDAO.SP_GetMaxSINo(branchid,fyearid);
                _supinvoice.Details = new List<SupplierInvoiceDetailVM>();
                _supinvoice.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
                _supinvoice.ToDate = CommonFunctions.GetCurrentDateTime().Date; // DateTime.Now.Date;
                _supinvoice.MovementId = "1,2,3,4";
                _supinvoice.InvoiceTotal = 0;
            }
           

            return View(_supinvoice);

        }


        //[HttpPost]
        //public ActionResult Create(SupplierInvoiceVM model)
        //{
        //    PickupRequestDAO dao = new PickupRequestDAO();
        //    int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
        //    var userid = Convert.ToInt32(Session["UserID"]);
        //    int yearid = Convert.ToInt32(Session["fyearid"].ToString());
        //    int SupplierInvoiceId = 0;
        //    SupplierInvoice Supplierinvoice = new SupplierInvoice();
        //    if (model.SupplierInvoiceID == 0)
        //    {
        //        string pMovementId = "";
        //        if (model.MovementId != null)
        //        {
        //            foreach (var item in model.MovementId)
        //            {
        //                if (pMovementId == "")
        //                {
        //                    pMovementId = item.ToString();
        //                }
        //                else
        //                {
        //                    pMovementId = pMovementId + "," + item.ToString();
        //                }

        //            }
        //        }
        //        SupplierInvoiceVM _custinvoice = new SupplierInvoiceVM();
        //        SupplierInvoiceId = PickupRequestDAO.GetFowardingAWBCost(model.SupplierID, Convert.ToDateTime(model.FromDate), Convert.ToDateTime(model.ToDate), pMovementId, Convert.ToDateTime(model.InvoiceDate), model.Remarks, model.InvoiceNo);
        //        if (SupplierInvoiceId == 0)
        //        { 
        //            TempData["ErrorMsg"] = "No pending to generate Invoice!";
        //        var suppliers = db.SupplierMasters.ToList();
        //        ViewBag.Supplier = suppliers;
        //        SupplierInvoiceVM _supinvoice = new SupplierInvoiceVM();
        //        ViewBag.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
        //            return View(model);
        //        }
        //    else
        //    {
                    
        //            dao.GenerateSupplierInvoicePosting(SupplierInvoiceId);
        //            TempData["SuccessMsg"] = "You have successfully Saved the Forwarding Agent Invoice!";
        //        return RedirectToAction("Index");
        //    }

                
        //    }
        //    return RedirectToAction("Index");

        //}

        public ActionResult Edit(int id = 0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
            var suppliers = db.SupplierMasters.ToList();
            ViewBag.Supplier = suppliers;
            SupplierInvoiceVM _supinvoice = new SupplierInvoiceVM();
            ViewBag.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
            if (id > 0)
            {
                ViewBag.Title = "Forwarding Agent Invoice- Modify";
                var _invoice = db.SupplierInvoices.Find(id);
                _supinvoice.SupplierInvoiceID = _invoice.SupplierInvoiceID;
                _supinvoice.InvoiceDate = _invoice.InvoiceDate;
                _supinvoice.InvoiceNo = _invoice.InvoiceNo;
                _supinvoice.SupplierID = _invoice.SupplierID;
                _supinvoice.Remarks = _invoice.Remarks;
                _supinvoice.AWBTotal = _invoice.AWBTotal;
                _supinvoice.AdditionalSurCharges = _invoice.AdditionalSurCharges;
                _supinvoice.FuelSurchargePercent = _invoice.FuelSurchargePercent;
                _supinvoice.FuelSurchargeAmount = _invoice.FuelSurchargeAmount;
                _supinvoice.InvoiceTotal = _invoice.InvoiceTotal;
                var supplier = suppliers.Where(d => d.SupplierID == _invoice.SupplierID).FirstOrDefault();
                if (supplier != null)
                {
                    _supinvoice.SupplierName = supplier.SupplierName;
                    _supinvoice.SupplierTypeId = Convert.ToInt32(supplier.SupplierTypeID);
                }

                List<SupplierInvoiceDetailVM> _details = new List<SupplierInvoiceDetailVM>();
                _details = (from c in db.SupplierInvoiceDetails
                            join a in db.AcHeads on c.AcHeadID equals a.AcHeadID
                            where c.SupplierInvoiceID == id
                            select new SupplierInvoiceDetailVM { SupplierInvoiceDetailID = c.SupplierInvoiceDetailID, SupplierInvoiceID = c.SupplierInvoiceID, AcHeadId = c.AcHeadID, AcHeadName = a.AcHead1, Particulars = c.Particulars, TaxPercentage = c.TaxPercentage, CurrencyID = c.CurrencyID, Amount = c.Amount, Rate = c.Rate, Quantity = c.Quantity, Value = c.Value, ItemTypeId = 0 }).ToList();
                _supinvoice.Details = _details;
            }
            else
            {
                ViewBag.Title = "Forwarding Agent Invoice";
                _supinvoice.SupplierInvoiceID = 0;
                _supinvoice.InvoiceDate = CommonFunctions.GetCurrentDateTime();
                _supinvoice.InvoiceNo = ReceiptDAO.SP_GetMaxSINo(branchid,fyearid);
                _supinvoice.Details = new List<SupplierInvoiceDetailVM>();
                _supinvoice.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
                _supinvoice.ToDate = CommonFunctions.GetCurrentDateTime().Date; // DateTime.Now.Date;
                _supinvoice.MovementId = "1,2,3,4";
                _supinvoice.InvoiceTotal = 0;
            }


            return View(_supinvoice);

        }
        [HttpPost]
        public ActionResult Edit(SupplierInvoiceVM model)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            SupplierInvoice Supplierinvoice = new SupplierInvoice();
            if (model.SupplierInvoiceID > 0)
            {                
                Supplierinvoice = db.SupplierInvoices.Find(model.SupplierInvoiceID);
            }

            //Supplierinvoice.SupplierID = model.SupplierID;
            Supplierinvoice.InvoiceDate = model.InvoiceDate;
            Supplierinvoice.InvoiceTotal = model.InvoiceTotal;
            Supplierinvoice.StatusClose = false;
            Supplierinvoice.IsDeleted = false;
            Supplierinvoice.Remarks = model.Remarks;

            if (model.AdditionalSurCharges != null)
                Supplierinvoice.AdditionalSurCharges = model.AdditionalSurCharges;
            else
                Supplierinvoice.AdditionalSurCharges = 0;

            Supplierinvoice.FuelSurchargePercent = model.FuelSurchargePercent;
            var awbtotal = db.SupplierInvoiceDetails.Where(cc => cc.SupplierInvoiceID == model.SupplierInvoiceID).ToList();
            if (awbtotal != null)
                Supplierinvoice.AWBTotal = awbtotal[0].Value;

            if (model.FuelSurchargePercent != null)
            {
                Supplierinvoice.FuelSurchargeAmount = model.AWBTotal * ((Convert.ToDecimal(model.FuelSurchargePercent)) / Convert.ToDecimal(100.0));
                Supplierinvoice.FuelSurchargeAmount = Math.Round(Convert.ToDecimal(Supplierinvoice.FuelSurchargeAmount), 2);
            }
            else
            {
                Supplierinvoice.FuelSurchargeAmount = 0;
            }
            Supplierinvoice.InvoiceTotal = Supplierinvoice.AWBTotal + Supplierinvoice.FuelSurchargeAmount + Supplierinvoice.AdditionalSurCharges;


                Supplierinvoice.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                Supplierinvoice.ModifiedBy = userid;
                db.Entry(Supplierinvoice).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var item in model.Details)
                {
                    var InvoiceDetail = new SupplierInvoiceDetail();
                    InvoiceDetail = db.SupplierInvoiceDetails.Find(item.SupplierInvoiceDetailID);
                    //InvoiceDetail.SupplierInvoiceID = Supplierinvoice.SupplierInvoiceID;
                    //InvoiceDetail.AcHeadID = item.AcHeadId;
                    //InvoiceDetail.Particulars = item.Particulars;
                    //InvoiceDetail.Quantity = item.Quantity;
                    //InvoiceDetail.Rate = item.Rate;
                    //InvoiceDetail.CurrencyID = item.CurrencyID;
                    //InvoiceDetail.CurrencyAmount = item.CurrencyAmount;
                    //InvoiceDetail.Amount = item.Amount;
                    InvoiceDetail.TaxPercentage = item.TaxPercentage;
                    InvoiceDetail.Value = item.Value;

                    db.Entry(InvoiceDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }


                PickupRequestDAO dao = new PickupRequestDAO();
                dao.GenerateForwardingInvoicePosting(Supplierinvoice.SupplierInvoiceID);
                TempData["SuccessMsg"] = "You have successfully Updated the Forwarding Agent Invoice!";
                return RedirectToAction("Index");
            }

        [HttpGet]
        public JsonResult GetSupplierName(string term)
        {
            int SupplierTypeId = 3;
            var customerlist = (from c1 in db.SupplierMasters
                                where c1.SupplierName.ToLower().Contains(term.ToLower()) && c1.SupplierTypeID == SupplierTypeId
                                orderby c1.SupplierName ascending
                                select new { SupplierID = c1.SupplierID, SupplierName = c1.SupplierName }).ToList();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult SaveForwardingInvoice(int SupplierId, string FromDate, string ToDate, int[] MovementId,string InvoiceNo,string Remarks, string InvoiceDate,decimal FuelPercentage,decimal AdditionalCharges)
        {
            string pMovementId = "";
            if (MovementId != null)
            {
                foreach (var item in MovementId)
                {
                    if (pMovementId == "")
                    {
                        pMovementId = item.ToString();
                    }
                    else
                    {
                        pMovementId = pMovementId + "," + item.ToString();
                    }

                }
            }

            SupplierInvoiceVM _custinvoice = new SupplierInvoiceVM();
            int SupplierInvoiceId = PickupRequestDAO.GetFowardingAWBCost(SupplierId, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), pMovementId, Convert.ToDateTime(InvoiceDate), Remarks, InvoiceNo,FuelPercentage,AdditionalCharges);
            if (SupplierInvoiceId == 0)
            {
                TempData["ErrorMsg"] = "No pending to generate Invoice!";
                var suppliers = db.SupplierMasters.ToList();
                ViewBag.Supplier = suppliers;
                SupplierInvoiceVM _supinvoice = new SupplierInvoiceVM();
                ViewBag.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                return Json(new { status = "Failed", message = "No AWB found!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                PickupRequestDAO dao = new PickupRequestDAO();
                dao.GenerateForwardingInvoicePosting(SupplierInvoiceId);
                return Json(new { status = "ok", message = "Forwarding Agent Invoice Saved Successfully!" }, JsonRequestBehavior.AllowGet);
                               
            }           

        }

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = PickupRequestDAO.DeleteForwardingInvoice(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        //if (dt.Rows[0][0] == "OK")
                        TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return RedirectToAction("Index");

            
        }
    }
}
