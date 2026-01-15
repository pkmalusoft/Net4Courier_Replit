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
    public class MCManagementController : Controller
    {
        Entities1 db = new Entities1();
        // GET: COD
        public ActionResult Table()
        {
            MCDatePicker datePicker = SessionDataModel.GetMCTableVariable1();
            if (datePicker == null)
            {
                datePicker = new MCDatePicker
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date //DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59)                
                };
            }
            SessionDataModel.SetMCTableVariable1(datePicker);
            return View(datePicker);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Table([Bind(Include = "AWBNo,FromDate,ToDate")] MCDatePicker picker)
        {

            MCDatePicker model = new MCDatePicker
            {
                AWBNo=picker.AWBNo,
                FromDate = picker.FromDate,
                ToDate = Convert.ToDateTime(picker.ToDate).Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                
            };

            if (model.AWBNo != null)
                model.SearchOption = "AWBNo";
            ViewBag.Token = model;
            SessionDataModel.SetMCTableVariable1(model);
            return RedirectToAction("Index", "MCManagement");

        }
        public ActionResult Index()
        {
            Session["MCAWBSearch"] = null;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            MCDatePicker datePicker = SessionDataModel.GetMCTableVariable1();

            List<DRRRecPayVM> _Receipts = new List<DRRRecPayVM>();
            if (datePicker != null)            {

                if (datePicker.FromDate.ToString() == "01-01-0001 00:00:00")
                {
                    datePicker.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
                }

                if (datePicker.ToDate.ToString() == "01-01-0001 00:00:00")
                {
                    datePicker.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                }
                DateTime pToDate = Convert.ToDateTime(datePicker.ToDate).AddDays(1);
                 _Receipts = (from c in db.DRRRecPays
                                               where (c.RecPayDate >= datePicker.FromDate && c.RecPayDate < pToDate)
                                               orderby c.DocumentNo descending
                                               select new DRRRecPayVM
                                               {
                                                   RecPayID = c.RecPayID,
                                                   DocumentNo = c.DocumentNo,
                                                   RecPayDate = c.RecPayDate,
                                                   FMoney = c.FMoney
                                               }).ToList();

                SessionDataModel.SetMCTableVariable1(datePicker);
            }
            return View(_Receipts);

        }
        public ActionResult MCAWBSearch()
        {
            MCDatePicker datePicker = new MCDatePicker();
            if (Session["MCAWBSearch"] != null)
            {
                datePicker = (MCDatePicker)Session["MCAWBSearch"];
            }
            //if (datePicker == null)
            //{
            //    datePicker = new MCDatePicker();
            //    datePicker.FromDate = CommonFunctions.GetLastDayofMonth(); // DateTime.Now.Date;
            //    datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            //    datePicker.SearchOption = "Date";
            //}                    
                        
            SessionDataModel.SetMCTableVariable1(datePicker);
            return View(datePicker);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MCAWBSearch([Bind(Include = "FromDate1,ToDate1,SearchOption,AWBNo,Shipper")] MCDatePicker picker)
        {
            MCDatePicker model = new MCDatePicker
            {
                FromDate1 = picker.FromDate1,
                ToDate1 = picker.ToDate1,
                SearchOption =picker.SearchOption,
                AWBNo=picker.AWBNo,
                Shipper=picker.Shipper
            };

            if (model.AWBNo==null)
            {
                model.AWBNo = "";
            }
            ViewBag.Token = model;
            Session["MCAWBSearch"] = model;
            //SessionDataModel.SetMCTableVariable1(model);
            return RedirectToAction("Create", "MCManagement");
            

        }
       



        public ActionResult Create(int id=0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            MCDatePicker datePicker = (MCDatePicker)Session["MCAWBSearch"];
            ViewBag.Token = datePicker;
            ViewBag.Movement = db.CourierMovements.ToList();
            var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

            ViewBag.achead = acheadforcash;
            ViewBag.acheadbank = acheadforbank;
            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = db.CurrencyMasters.ToList();
            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName");

            DRRRecPayVM _receipt = new DRRRecPayVM();
            List<DRRRecPayDetailVM> _details = new List<DRRRecPayDetailVM>();
            _receipt.Details = _details;
            PickupRequestDAO _dao = new PickupRequestDAO();
            if (id == 0)
            {
                ViewBag.Title = "MC Payment - Create";                
                _receipt.RecPayID = 0;
                _receipt.RecPayDate =CommonFunctions.GetLastDayofMonth();// DateTimeKind. DateTimeOffset.Now.UtcDateTime.AddHours(5.30); // DateTime.Now;            
                _receipt.DocumentNo = _dao.GetMaxMCDocumentNo(companyid, branchid, yearid);
                _receipt.CurrencyID = Convert.ToInt32(Session["CurrencyId"].ToString());
                _receipt.EXRate = Convert.ToInt32(Session["EXRATE"].ToString());
                
                if (datePicker != null)
                 {

                    _details = ReceiptDAO.GetMCAWBPending(datePicker, 0,branchid ,yearid);
                    if (datePicker.SearchOption == "Date")
                    {
                        _receipt.ShipperName = datePicker.Shipper;
                        _receipt.ShipperAddress = datePicker.ShipperAddress;
                    }
                    else
                    {
                        var inscan = db.InScanMasters.Where(cc => cc.AWBNo == datePicker.AWBNo).FirstOrDefault();
                        if (inscan!=null)
                        {
                            _receipt.ShipperName = inscan.Consignor;
                            _receipt.ShipperAddress = inscan.ConsignorCountryName;
                        }
                    }
                    _receipt.Details = _details;
                    //_receipt.InvoiceTotal = Convert.ToDecimal(customerinvoice.TotalCharges) + Convert.ToDecimal(customerinvoice.ChargeableWT) + customerinvoice.AdminAmt + customerinvoice.FuelAmt + customerinvoice.OtherCharge;
                }
            }
            else if (id>0)
            {
                ViewBag.Title = "MC Payment - Modify";
                DRRRecPay  receipt = db.DRRRecPays.Find(id);
                _receipt.RecPayID = receipt.RecPayID;
                _receipt.DocumentNo = receipt.DocumentNo;
                _receipt.RecPayDate = receipt.RecPayDate;
                _receipt.ChequeDate = receipt.ChequeDate;
                _receipt.ChequeNo = receipt.ChequeNo;
                _receipt.StatusEntry = receipt.StatusEntry;
                var achead = db.AcHeads.Where(cc => cc.AcHead1 == receipt.BankName).FirstOrDefault(); ;
                if (achead != null)
                    _receipt.AchHeadID = achead.AcHeadID;

                if (receipt.StatusEntry=="CS")
                {
                    _receipt.CashBank = _receipt.AchHeadID.ToString();
                }
                else
                {
                    _receipt.ChequeBank = _receipt.AchHeadID.ToString();
                }
                
                if (receipt.ShipperName!=null)
                {
                    _receipt.ShipperName = receipt.ShipperName;
                }
                if (receipt.ShipperAddress!=null)
                  _receipt.ShipperAddress = receipt.ShipperAddress;
                
                
                _receipt.CurrencyID = receipt.CurrencyID;
                _receipt.EXRate = receipt.EXRate;
                _receipt.FMoney = receipt.FMoney;
                //_receipt.Remarks = receipt.Remarks;
                _details = ReceiptDAO.GetMCAWBPending(datePicker,_receipt.RecPayID, branchid , yearid);
                //_details = (from c in db.DRRRecPayDetails join  c2 in db.InScanMasters on c.InScanID equals c2.InScanID where c.RecPayID==_receipt.RecPayID
                //            select new DRRRecPayDetailVM { RecPayDetailID = c.RecPayDetailID, RecPayID = c.RecPayID,
                //                MaterialCost = c2.MaterialCost == null ? 0 : c2.MaterialCost.Value ,Amount=c.Amount , AdjustmentAmount = c.AdjustmentAmount, AWBNo = c2.AWBNo, InScanID = c.InScanID,AWBChecked=true,AWBDateTime=c2.TransactionDate,ConsigneeName=c2.Consignee,ConsigneeCountryName=c2.ConsigneeCountryName }).ToList();
                _receipt.Details = _details;
            }
            
            Session["DRRPaymentListing"] = _details;
            return View(_receipt);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DRRRecPayVM model)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DRRRecPay _receipt = new DRRRecPay();
            try
            {
                if (model.CashBank != null)
                {
                    model.StatusEntry = "CS";
                    int acheadid = Convert.ToInt32(model.CashBank);
                    var achead = (from t in db.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                    model.BankName = achead;
                }
                else
                {
                    model.StatusEntry = "BK";
                    int acheadid = Convert.ToInt32(model.ChequeBank);
                    var achead = (from t in db.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                    model.BankName = achead;
                }

                if (model.RecPayID == 0)
                {

                    _receipt.DocumentNo = model.DocumentNo;
                    _receipt.RecPayDate = model.RecPayDate;
                    _receipt.FYearID = yearid;
                    _receipt.AcCompanyID = companyId;
                    _receipt.BranchID = branchid;
                }
                else
                {
                    _receipt = db.DRRRecPays.Find(model.RecPayID);
                }
                _receipt.StatusEntry = model.StatusEntry;
                _receipt.BankName = model.BankName;
                _receipt.FMoney = model.FMoney;             
                _receipt.CurrencyID = model.CurrencyID;
                _receipt.EXRate = model.EXRate;
                _receipt.ChequeNo = model.ChequeNo;
                _receipt.ChequeDate = model.ChequeDate;
                _receipt.ShipperName = model.ShipperName;
                _receipt.ShipperAddress = model.ShipperAddress;
                //_receipt.AchHeadID = model.AchHeadID;
                _receipt.Remarks = model.Remarks;

                if (model.RecPayID == 0)
                {
                    db.DRRRecPays.Add(_receipt);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(_receipt).State = EntityState.Modified;
                    db.SaveChanges();

                    var details = (from d in db.DRRRecPayDetails  where d.RecPayID == _receipt.RecPayID  select d).ToList();
                    db.DRRRecPayDetails.RemoveRange(details);
                    db.SaveChanges();
                }

                List<DRRRecPayDetailVM> e_Details = model.Details; //  Session["InvoiceListing"] as List<CustomerInvoiceDetailVM>;

                model.Details = e_Details;

                if (model.Details != null)
                {

                    foreach (var e_details in model.Details)
                    {
                        if (e_details.Amount > 0 || e_details.AWBChecked)
                        {

                            DRRRecPayDetail _detail = new DRRRecPayDetail();
                            _detail.RecPayID = _receipt.RecPayID;
                            _detail.InScanID = e_details.InScanID;
                            _detail.Amount = e_details.Amount;
                            _detail.AdjustmentAmount = e_details.AdjustmentAmount;
                            db.DRRRecPayDetails.Add(_detail);
                            db.SaveChanges();

                            //inscan invoice modified                            
                            MaterialCostMaster mc = db.MaterialCostMasters.Where(cc=>cc.InScanID== e_details.InScanID).FirstOrDefault();
                            if (mc != null)
                            {
                                mc.MCPVID = _receipt.RecPayID;
                                mc.StatusClose = e_details.AWBChecked;
                                db.Entry(mc).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {

                            MaterialCostMaster mc = db.MaterialCostMasters.Where(cc => cc.InScanID == e_details.InScanID && cc.MCPVID != null).FirstOrDefault();
                            if (mc != null)
                            {
                                if (Convert.ToInt32(mc.MCPVID) == _receipt.RecPayID)
                                {
                                    mc.MCPVID = null;
                                    mc.StatusClose = false;
                                    db.Entry(mc).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                     


                    }
                }

                //Accounts Posting
                PickupRequestDAO _dao = new PickupRequestDAO();
                //_dao.GenerateDomesticCODPosting(_receipt.ReceiptID);

                TempData["SuccessMsg"] = "You have successfully Saved the COD Receipt";
                return RedirectToAction("Index");

            }
            catch(Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public JsonResult GetMCAWBDetails(int id)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            MCDatePicker datePicker = SessionDataModel.GetMCTableVariable1();
            
            List<DRRRecPayDetailVM> _details = new List<DRRRecPayDetailVM>();
            _details = ReceiptDAO.GetMCAWBPending(datePicker, id, branchid, yearid);
            return Json(_details, JsonRequestBehavior.AllowGet);
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

        public ActionResult PrintVoucher(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            AccountsReportsDAO.GenerateMCPaymentPrintVoucher(id);
            ViewBag.ReportName = "MC Payment Voucher";
            return View();

        }
    }
}