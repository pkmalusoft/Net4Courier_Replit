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
    [SessionExpireFilter]
    public class CODReceiptController : Controller
    {
        Entities1 db = new Entities1();
        // GET: COD
        public ActionResult Index()
        {

            DatePicker model = new DatePicker
            {
                FromDate = CommonFunctions.GetFirstDayofMonth().Date,
                ToDate = CommonFunctions.GetLastDayofMonth().Date //DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59)                
            };
            ViewBag.Token = model;
            SessionDataModel.SetTableVariable1(model);
            return View(model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "FromDate,ToDate")] DatePicker picker)
        {

            DatePicker model = new DatePicker
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Delete = true, // (bool)Token.Permissions.Deletion,
                Update = true, //(bool)Token.Permissions.Updation,
                Create = true //.ToStrin//(bool)Token.Permissions.Creation
            };
            ViewBag.Token = model;
            SessionDataModel.SetTableVariable1(model);
            return View(model);

        }
        public ActionResult AWBSearch()
        {

            DatePicker datePicker = SessionDataModel.GetTableVariable1();

            if (datePicker == null)
            {
                datePicker = new DatePicker();
                datePicker.FromDate = CommonFunctions.GetLastDayofMonth(); // DateTime.Now.Date;
                datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MovementId = "1,2,3,4";
            }
            if (datePicker != null)
            {
                //ViewBag.Customer = (from c in db.InScanMasters
                //                    join cust in db.CustomerMasters on c.CustomerID equals cust.CustomerID
                //                    where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                //                    select new CustmorVM { CustomerID = cust.CustomerID, CustomerName = cust.CustomerName }).Distinct();
             
                if (datePicker.MovementId == null)
                    datePicker.MovementId = "1,2,3,4";
            }
           
            //ViewBag.Movement = new MultiSelectList(db.CourierMovements.ToList(),"MovementID","MovementType");
            ViewBag.Movement = db.CourierMovements.ToList();

            ViewBag.Token = datePicker;
            SessionDataModel.SetTableVariable1(datePicker);
            return View(datePicker);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AWBSearch([Bind(Include = "FromDate,ToDate,MovementId,SelectedValues")] DatePicker picker)
        {
            DatePicker model = new DatePicker
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Delete = true, // (bool)Token.Permissions.Deletion,
                Update = true, //(bool)Token.Permissions.Updation,
                Create = true, //.ToStrin//(bool)Token.Permissions.Creation            
                MovementId = picker.MovementId,                
                SelectedValues = picker.SelectedValues
            };
            model.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (model.MovementId == "")
                    {
                        model.MovementId = item.ToString();
                    }
                    else
                    {
                        model.MovementId = model.MovementId + "," + item.ToString();
                    }

                }
            }
            ViewBag.Token = model;
            SessionDataModel.SetTableVariable1(model);
            return RedirectToAction("Create", "CODReceipt");
            

        }
        public ActionResult Table()
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            DatePicker datePicker = SessionDataModel.GetTableVariable1();
            ViewBag.Token = datePicker;
            DateTime ToDate = datePicker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            List<DCODReceiptVM> _Receipts = (from c in db.DomesticCODReceipts                                                 
                                                 where (c.ReceiptDate >= datePicker.FromDate && c.ReceiptDate < ToDate)                                                 
                                                 orderby c.ReceiptNo descending
                                                 select new DCODReceiptVM
                                                 {
                                                     ReceiptID = c.ReceiptID,
                                                     ReceiptNo = c.ReceiptNo,
                                                     ReceiptDate = c.ReceiptDate                                                     ,
                                                     Amount = c.Amount
                                                 }).ToList();

            return View("Table", _Receipts);

        }



        public ActionResult Create(int id=0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            DatePicker datePicker = SessionDataModel.GetTableVariable1();
            ViewBag.Token = datePicker;
            ViewBag.Movement = db.CourierMovements.ToList();
            var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

            ViewBag.achead = acheadforcash;
            ViewBag.acheadbank = acheadforbank;
            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = db.CurrencyMasters.ToList();
            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName");

            DCODReceiptVM _receipt = new DCODReceiptVM();
            List<DCODReceiptDetailVM> _details = new List<DCODReceiptDetailVM>();
            PickupRequestDAO _dao = new PickupRequestDAO();
            if (id == 0)
            {
                ViewBag.Title = "COD Receipt - Create";                
                _receipt.ReceiptID = 0;
                _receipt.ReceiptDate =CommonFunctions.GetLastDayofMonth();// DateTimeKind. DateTimeOffset.Now.UtcDateTime.AddHours(5.30); // DateTime.Now;            
                _receipt.ReceiptNo = _dao.GetMaxDomesticReceiptNo(companyid, branchid, yearid);
                _receipt.CurrencyID = Convert.ToInt32(Session["CurrencyId"].ToString());
                _receipt.EXRate = Convert.ToInt32(Session["EXRATE"].ToString());
                if (datePicker != null)
                {

                    _details = (from c in db.InScanMasters
                                where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                                && ((c.CODReceiptId == null || c.CODReceiptId == 0) && c.ManifestID==null)
                                && c.PaymentModeId == 2 //account                                
                                select new DCODReceiptDetailVM
                                {
                                    AWBNo = c.AWBNo,
                                    AWBDateTime = c.TransactionDate,
                                    ConsigneeName = c.Consignee,
                                    ConsigneeCountryName = c.ConsigneeCountryName,
                                    CourierCharge = c.CourierCharge == null ? 0 : c.CourierCharge.Value,
                                    //CustomCharge = c.CustomsValue == null ? 0 : c.CustomsValue,
                                    OtherCharge = c.OtherCharge == null ? 0 : c.OtherCharge,
                                    InScanId = c.InScanID,
                                    MovementId = c.MovementID == null ? 0 : c.MovementID.Value,
                                    AWBChecked = true
                                }).ToList(); //.Where(cc => datePicker.SelectedValues.ToList().Contains(cc.MovementId)).ToList();

                    int _index = 0;

                    foreach (var item in _details)
                    {
                        _details[_index].TotalCharge = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].CustomCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                        _details[_index].AmountAllocate = _details[_index].TotalCharge;
                        _receipt.TotalCharges = _receipt.TotalCharges + _details[_index].TotalCharge;

                        _index++;
                    }

                    _receipt.Amount = _receipt.TotalCharges;
                    _receipt.Details = _details;
                    //_receipt.InvoiceTotal = Convert.ToDecimal(customerinvoice.TotalCharges) + Convert.ToDecimal(customerinvoice.ChargeableWT) + customerinvoice.AdminAmt + customerinvoice.FuelAmt + customerinvoice.OtherCharge;
                }
            }
            else
            {
                DomesticCODReceipt receipt = db.DomesticCODReceipts.Find(id);
                _receipt.ReceiptID = receipt.ReceiptID;
                _receipt.ReceiptNo = receipt.ReceiptNo;
                _receipt.ReceiptDate = receipt.ReceiptDate;
                _receipt.ChequeDate = receipt.ChequeDate;
                _receipt.ChequeNo = receipt.ChequeNo;
                _receipt.AchHeadID = receipt.AchHeadID;
                _receipt.CurrencyID = receipt.CurrencyID;
                _receipt.EXRate = receipt.EXRate;
                _receipt.Amount = receipt.Amount;
                _receipt.Remarks = receipt.Remarks;
                _details = (from c in db.DomesticCODReceiptDetails join  c2 in db.InScanMasters on c.InScanId equals c2.InScanID where c2.CODReceiptId== receipt.ReceiptID
                            select new DCODReceiptDetailVM { ReceiptDetailID = c.ReceiptDetailID, ReceiptID = c.ReceiptID, CourierCharge = c.CourierCharge, OtherCharge = c.OtherCharge, TotalCharge = c.TotalCharge, AmountAllocate = c.AmountAllocate, Discount = c.Discount, AWBNo = c.AWBNo, InScanId = c.InScanId,AWBChecked=true,AWBDateTime=c2.TransactionDate,ConsigneeName=c2.Consignee,ConsigneeCountryName=c2.ConsigneeCountryName }).ToList();
                _receipt.Details = _details;
            }
            
            Session["CODReceiptListing"] = _details;
            return View(_receipt);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DCODReceiptVM model)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DomesticCODReceipt _receipt = new DomesticCODReceipt();
            try
            {


                if (model.ReceiptID == 0)
                {

                    _receipt.ReceiptNo = model.ReceiptNo;
                    _receipt.ReceiptDate = model.ReceiptDate;
                    _receipt.FYearID = yearid;
                    _receipt.AcCompanyID = companyId;
                    _receipt.BranchID = branchid;
                }
                else
                {
                    _receipt = db.DomesticCODReceipts.Find(model.ReceiptID);
                }

                _receipt.Amount = model.Amount;             
                _receipt.CurrencyID = model.CurrencyID;
                _receipt.EXRate = model.EXRate;
                _receipt.ChequeNo = model.ChequeNo;
                _receipt.ChequeDate = model.ChequeDate;
                _receipt.AchHeadID = model.AchHeadID;
                _receipt.Remarks = model.Remarks;

                if (model.ReceiptID == 0)
                {
                    db.DomesticCODReceipts.Add(_receipt);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(_receipt).State = EntityState.Modified;
                    db.SaveChanges();
                }

                List<DCODReceiptDetailVM> e_Details = model.Details; //  Session["InvoiceListing"] as List<CustomerInvoiceDetailVM>;

                model.Details = e_Details;

                if (model.Details != null)
                {

                    foreach (var e_details in model.Details)
                    {
                        if (e_details.ReceiptDetailID == 0 && e_details.AWBChecked)
                        {
                            DomesticCODReceiptDetail _detail = new DomesticCODReceiptDetail();

                            _detail.ReceiptID = _receipt.ReceiptID;
                            _detail.AWBNo = e_details.AWBNo;
                            _detail.InScanId = e_details.InScanId;
                            _detail.CourierCharge = e_details.CourierCharge;
                            _detail.OtherCharge = e_details.OtherCharge;
                            _detail.TotalCharge = e_details.TotalCharge;
                            _detail.AmountAllocate = e_details.AmountAllocate;
                            _detail.Discount = e_details.Discount;
                            db.DomesticCODReceiptDetails.Add(_detail);
                            db.SaveChanges();

                            //inscan invoice modified
                            InScanMaster _inscan = db.InScanMasters.Find(e_details.InScanId);
                            _inscan.CODReceiptId = _receipt.ReceiptID;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if(e_details.ReceiptDetailID > 0 && e_details.AWBChecked)
                        {
                            DomesticCODReceiptDetail _detail = db.DomesticCODReceiptDetails.Find(e_details.ReceiptDetailID);
                            _detail.AmountAllocate = e_details.AmountAllocate;
                            _detail.Discount = e_details.Discount;
                            db.Entry(_detail).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                }

                //Accounts Posting
                PickupRequestDAO _dao = new PickupRequestDAO();
                _dao.GenerateDomesticCODPosting(_receipt.ReceiptID);

                TempData["SuccessMsg"] = "You have successfully Saved the COD Receipt";
                return RedirectToAction("Index");

            }
            catch(Exception ex)
            {
                return RedirectToAction("Index");
            }
        }


        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteDomesticCOD(id);
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

            return RedirectToAction("Index", "CODReceipt");

        }
    }
}