using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using Newtonsoft.Json;
using System.Data;
using System.Data.Entity;
namespace Net4Courier.Controllers
{ [SessionExpire]
    public class SupplierInvoiceController : Controller
    {
        Entities1 db = new Entities1();
        // GET: SupplierInvoice
        //public ActionResult Index(int? id, string FromDate, string ToDate)
        //{
                        
        //    int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
        //    int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
        //    DateTime pFromDate;
        //    DateTime pToDate;
        //    int suppliertypeid = 0;
        //    if (id == null | id == 0)
        //        suppliertypeid = 1;
        //    else
        //        suppliertypeid = Convert.ToInt32(id);

        //    if (FromDate == null || ToDate == null)
        //    {
        //        pFromDate = CommonFunctions.GetFirstDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
        //        pToDate = CommonFunctions.GetLastDayofMonth().Date.AddDays(1); // // ToDate = DateTime.Now;
        //    }
        //    else
        //    {
        //        pFromDate = Convert.ToDateTime(FromDate);//.AddDays(-1);
        //        pToDate = Convert.ToDateTime(ToDate).AddDays(1);

        //    }

        //    var lst = (from c in db.SupplierInvoices
        //               join s in db.SupplierMasters on c.SupplierID equals s.SupplierID
        //               orderby c.InvoiceDate descending
        //             //  where (s.SupplierTypeID == suppliertypeid)
        //               where c.InvoiceDate >= pFromDate && c.InvoiceDate < pToDate
        //               && c.BranchId==branchid
        //               select new SupplierInvoiceVM { SupplierInvoiceID = c.SupplierInvoiceID, InvoiceNo = c.InvoiceNo, InvoiceDate = c.InvoiceDate, SupplierName = s.SupplierName, Amount = 0, SupplierType = "S", Remarks = s.Remarks }).ToList();
        //    lst.ForEach(d => d.Amount = (from s in db.SupplierInvoiceDetails where s.SupplierInvoiceID == d.SupplierInvoiceID select s).ToList().Sum(a => a.Value));
        //    var list= ReceiptDAO.GetSupplierInvoiceList(pFromDate,pToDate, fyearid,)
        //    ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
        //    ViewBag.ToDate = pToDate.Date.AddDays(-1).ToString("dd-MM-yyyy");
        //    ViewBag.SupplierType = db.SupplierTypes.ToList();
        //    ViewBag.SupplierTypeId = suppliertypeid;
        //    return View(lst);
        //}
        public ActionResult Index(SupplierInvoiceSearch obj)
        {

            //CustomerReceiptSearch obj = (CustomerReceiptSearch)Session["CustomerReceiptSearch"];
            SupplierInvoiceSearch model = new SupplierInvoiceSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.SupplierType = db.SupplierTypes.ToList();
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                obj = new SupplierInvoiceSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.InvoiceNo = "";
                obj.SupplierTypeId = 0;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.InvoiceNo = "";

                model.Details = new List<SupplierInvoiceVM>();
            }
            else
            {
                model = obj;
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
            }
            var data = ReceiptDAO.GetSupplierInvoiceList(model.FromDate, model.ToDate, yearid, model.SupplierTypeId, model.InvoiceNo);
            model.Details = data;
            return View(model);

        }
        public ActionResult Create(int id=0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int fyearid=Convert.ToInt32(Session["fyearid"].ToString());
            var suppliers = db.SupplierMasters.ToList();
            ViewBag.Supplier = suppliers;
            ViewBag.SupplierType = db.SupplierTypes.ToList();
            ViewBag.Currency = db.CurrencyMasters.ToList();
            ViewBag.ItemType = db.ItemTypes.ToList();
            SupplierInvoiceVM _supinvoice = new SupplierInvoiceVM();
            ViewBag.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
            if (id > 0)
            {
                ViewBag.Title = "Modify";
                var _invoice = db.SupplierInvoices.Find(id);
                _supinvoice.SupplierInvoiceID = _invoice.SupplierInvoiceID;
                _supinvoice.InvoiceDate = _invoice.InvoiceDate;
                _supinvoice.InvoiceNo = _invoice.InvoiceNo;
                _supinvoice.SupplierID = _invoice.SupplierID;
                _supinvoice.Remarks = _invoice.Remarks;
                _supinvoice.ReferenceNo = _invoice.ReferenceNo;
                var supplier = suppliers.Where(d => d.SupplierID == _invoice.SupplierID).FirstOrDefault();
                if (supplier != null)
                {
                    _supinvoice.SupplierName = supplier.SupplierName;
                    _supinvoice.SupplierTypeId = Convert.ToInt32(supplier.SupplierTypeID);
                }

                //List<SupplierInvoiceDetail> _details = new List<SupplierInvoiceDetail>();
                List<SupplierInvoiceDetailVM> _details = new List<SupplierInvoiceDetailVM>();
                _details = (from c in db.SupplierInvoiceDetails join a in db.AcHeads on c.AcHeadID equals a.AcHeadID
                            where c.SupplierInvoiceID == id
                            select new SupplierInvoiceDetailVM {SupplierInvoiceDetailID=c.SupplierInvoiceDetailID,SupplierInvoiceID=c.SupplierInvoiceID,AcHeadId=c.AcHeadID,AcHeadName=a.AcHead1,Particulars=c.Particulars,TaxPercentage=c.TaxPercentage,CurrencyID=c.CurrencyID,Amount=c.Amount,Rate=c.Rate, Quantity=c.Quantity, Value=c.Value ,ItemTypeId=0 }   ).ToList();

                foreach(SupplierInvoiceDetailVM detail in _details)
                {
                    //var stock = db.SupplierInvoiceStocks.Where(cc => cc.SupplierInvoiceID == detail.SupplierInvoiceID && cc.SupplierInvoiceDetailId == detail.SupplierInvoiceDetailID).FirstOrDefault();

                    var stock = db.ItemPurchases.Where(cc => cc.SupplierInvoiceDetailID == detail.SupplierInvoiceDetailID).FirstOrDefault();
                    if (stock!=null)
                    {
                        detail.AWBCount =Convert.ToInt32(stock.AWBCount);
                        detail.AWBStart =Convert.ToInt32(stock.AWBNOFrom);
                        detail.AWBEnd = Convert.ToInt32(stock.AWBNOTo);
                        detail.BookNo = stock.ReferenceNo;
                        detail.ItemId =Convert.ToInt32(stock.ItemId);
                        var itemtypeid = db.Items.Find(detail.ItemId).ItemTypeId;
                        if (itemtypeid!=null)
                            detail.ItemTypeId =Convert.ToInt32(itemtypeid);

                        detail.Rate = Convert.ToDecimal(stock.Rate);
                        detail.Amount = Convert.ToDecimal(stock.Amount);
                        
                    }
                }

                _supinvoice.Details = _details;
                
                Session["SInvoiceListing"] = _details;
                               
                
                List<SupplierInvoiceAWBVM> AWBAllocationall = (from c in db.SupplierInvoiceAWBs  join d in db.InScanMasters on c.InScanID equals d.InScanID 
                                                                       where c.SupplierInvoiceId == id select new SupplierInvoiceAWBVM { ID = c.ID, SupplierInvoiceId = c.SupplierInvoiceId, SupplierInvoiceDetailId = c.SupplierInvoiceDetailId,
                                                                                    AcHeadId = c.AcHeadId, Amount = c.Amount, InScanID = c.InScanID, ConsignmentNo = d.AWBNo, ConsignmentDate = d.TransactionDate }).ToList();
                Session["SIAWBAllocation"] = AWBAllocationall;
                StatusModel result = AccountsDAO.CheckDateValidate(_supinvoice.InvoiceDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }
            }
            else
            {
                ViewBag.Title = "Create";
               _supinvoice.SupplierTypeId = 1;
                var Maxnumber = db.SupplierInvoices.ToList().LastOrDefault();
               _supinvoice.InvoiceNo = ReceiptDAO.SP_GetMaxSINo(branchid,fyearid);
                _supinvoice.InvoiceDate = CommonFunctions.GetCurrentDateTime();
                StatusModel result = AccountsDAO.CheckDateValidate(_supinvoice.InvoiceDate.ToString(), fyearid);
                if (result.Status == "YearClose") //Period locked
                {

                    ViewBag.Message = result.Message;
                    ViewBag.SaveEnable = false;
                }


            }
            return View(_supinvoice);

        }
        [HttpPost]
        public JsonResult SetSupplierInvDetails(int acheadid,string achead, string invno,string Particulars, decimal Rate, int Qty,decimal amount,int currency, decimal Taxpercent,decimal netvalue)
        {
            Random rnd = new Random();
            int dice = rnd.Next(1, 7);   // creates a number between 1 and 6
           
            var invoice = new SupplierInvoiceDetailVM();
            invoice.AcHeadId = acheadid;
            invoice.AcHeadName = achead;
            invoice.InvNo = invno+"_"+ dice;
            invoice.Particulars = Particulars;
            invoice.Rate =Rate;
            invoice.Quantity = Qty;
            invoice.CurrencyID = currency;
            var currencyMaster = db.CurrencyMasters.Find(currency);
            invoice.CurrencyAmount =Convert.ToDecimal(currencyMaster.ExchangeRate);
            invoice.Currency =currencyMaster.CurrencyName;
            //var amount = (Qty * Rate);
            //var value = amount + (amount * Taxpercent / 100);
          
            invoice.Amount = amount;
            invoice.Value =netvalue;
            invoice.TaxPercentage = Taxpercent;

            return Json(new { InvoiceDetails = invoice }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSupplierInvDetails(int Id)
        {
            Random rnd = new Random();
             // creates a number between 1 and 6

            var _invoice = db.SupplierInvoices.Find(Id);
            List<SupplierInvoiceDetailVM> _details = new List<SupplierInvoiceDetailVM>();
            List<SupplierInvoiceDetailVM> _details1 = new List<SupplierInvoiceDetailVM>();
            _details = (from c in db.SupplierInvoiceDetails
                        join a in db.AcHeads on c.AcHeadID equals a.AcHeadID
                        join cu in db.CurrencyMasters on c.CurrencyID equals cu.CurrencyID
                        where c.SupplierInvoiceID == Id
                        select new SupplierInvoiceDetailVM { SupplierInvoiceDetailID = c.SupplierInvoiceDetailID, SupplierInvoiceID = c.SupplierInvoiceID, AcHeadId = c.AcHeadID, AcHeadName = a.AcHead1, Particulars = c.Particulars, TaxPercentage = c.TaxPercentage, CurrencyID = c.CurrencyID, Amount = c.Amount, Rate = c.Rate, Quantity = c.Quantity, Value = c.Value ,Currency=cu.CurrencyCode,ItemTypeId=0 }).ToList();

            
            foreach (SupplierInvoiceDetailVM detail in _details)
            {
                //var stock = db.SupplierInvoiceStocks.Where(cc => cc.SupplierInvoiceID == detail.SupplierInvoiceID && cc.SupplierInvoiceDetailId == detail.SupplierInvoiceDetailID).FirstOrDefault();
                var stock = db.ItemPurchases.Where(cc => cc.SupplierInvoiceDetailID == detail.SupplierInvoiceDetailID).FirstOrDefault();
                if (stock != null)
                {
                    detail.AWBCount = Convert.ToInt32(stock.AWBCount);
                    detail.AWBStart = Convert.ToInt32(stock.AWBNOFrom);
                    detail.AWBEnd = Convert.ToInt32(stock.AWBNOTo);
                    detail.BookNo = stock.ReferenceNo;
                    detail.ItemId = Convert.ToInt32(stock.ItemId);
                    detail.ItemTypeId = Convert.ToInt32(stock.ItemTypeId);
                    detail.ItemQty = Convert.ToInt32(stock.Qty);
                    //detail.StockType = Convert.ToInt32(stock.StockType);
                }
            }
            foreach (var item in _details)
            {
                int dice = rnd.Next(1, 7);
                var invoice = new SupplierInvoiceDetailVM();
                invoice.SupplierInvoiceDetailID = item.SupplierInvoiceDetailID;
                invoice.AcHeadId = item.AcHeadId;
                invoice.AcHeadName = item.AcHeadName;
                invoice.InvNo = _invoice.InvoiceNo + "_" + dice;
                invoice.Particulars = item.Particulars;
                invoice.Rate = item.Rate;
                invoice.Quantity = item.Quantity;
                invoice.CurrencyID = item.CurrencyID;
                var currencyMaster = db.CurrencyMasters.Find(item.CurrencyID);
                invoice.CurrencyAmount = Convert.ToDecimal(currencyMaster.ExchangeRate);
                invoice.Currency = currencyMaster.CurrencyName;
                //decimal amount = (item.Quantity * item.Rate);
                //decimal value = amount + (amount * Convert.ToDecimal(item.TaxPercentage) / 100);
                invoice.Amount = item.Amount;
                invoice.Value = item.Value;
                invoice.TaxPercentage = item.TaxPercentage;
                invoice.ItemTypeId = item.ItemTypeId;
                invoice.ItemId = item.ItemId;
                invoice.BookNo = item.BookNo;
                invoice.AWBStart = item.AWBStart;
                invoice.AWBEnd = item.AWBEnd;
                invoice.AWBCount = item.AWBCount;
                invoice.ItemQty = item.ItemQty;
                _details1.Add(invoice);
            }

            return Json(new { InvoiceDetails = _details1 }, JsonRequestBehavior.AllowGet);
        }
        //SaveSupplierInvoice
        public JsonResult SaveSupplierInvoice(int Id, int SupplierID, string InvoiceDate, string InvoiceNo, string Remarks,string ReferenceNo,int SupplierTypeId, string Details)
        {
            try
            {
                int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                int fyearid = Convert.ToInt32(Session["fyearid"].ToString());
                int UserId = Convert.ToInt32(Session["UserID"]);
                var IDetails = JsonConvert.DeserializeObject<List<SupplierInvoiceDetailVM>>(Details);
                List<SupplierInvoiceAWBVM> AWBAllocationall = new List<SupplierInvoiceAWBVM>();
                List<SupplierInvoiceAWBVM> AWBAllocation = new List<SupplierInvoiceAWBVM>();
                AWBAllocationall = (List<SupplierInvoiceAWBVM>)Session["SIAWBAllocation"];
                var Supplierinvoice = (from d in db.SupplierInvoices where d.SupplierInvoiceID == Id select d).FirstOrDefault();
                if (Supplierinvoice == null)
                {
                    Supplierinvoice = new SupplierInvoice();
                     
                    Supplierinvoice.InvoiceNo = ReceiptDAO.SP_GetMaxSINo(branchid, fyearid);
                    Supplierinvoice.AccompanyID = Convert.ToInt32(Session["CurrentCompanyID"]);
                    Supplierinvoice.BranchId = branchid;
                    Supplierinvoice.FyearID = fyearid;
                }
                else
                {               
                    var consignmentdetails = (from d in db.SupplierInvoiceAWBs where d.SupplierInvoiceId == Supplierinvoice.SupplierInvoiceID select d).ToList();
                    db.SupplierInvoiceAWBs.RemoveRange(consignmentdetails);
                    db.SaveChanges();

                    //var stockdetails = (from d in db.SupplierInvoiceStocks where d.SupplierInvoiceID == Supplierinvoice.SupplierInvoiceID select d).ToList();
                    var stockdetails = (from d in db.ItemPurchases join c in db.SupplierInvoiceDetails on d.SupplierInvoiceDetailID equals c.SupplierInvoiceDetailID where c.SupplierInvoiceID == Supplierinvoice.SupplierInvoiceID select d).ToList();
                    db.ItemPurchases.RemoveRange(stockdetails);
                    db.SaveChanges();

                    var details = (from d in db.SupplierInvoiceDetails where d.SupplierInvoiceID == Supplierinvoice.SupplierInvoiceID select d).ToList();
                    db.SupplierInvoiceDetails.RemoveRange(details);
                    db.SaveChanges();

                }

                Supplierinvoice.SupplierID = SupplierID;
                Supplierinvoice.InvoiceDate = Convert.ToDateTime(InvoiceDate);
               
                
                var amount = IDetails.Sum(d => d.Value);
                Supplierinvoice.InvoiceTotal = amount;
                Supplierinvoice.StatusClose = false;
                Supplierinvoice.IsDeleted = false;
                Supplierinvoice.Remarks = Remarks;
                Supplierinvoice.ReferenceNo = ReferenceNo;
                Supplierinvoice.SupplierTypeId = SupplierTypeId;

                Supplierinvoice.ModifiedDate = CommonFunctions.GetBranchDateTime();
                Supplierinvoice.ModifiedBy = UserId;
                if (Supplierinvoice.SupplierInvoiceID == 0)
                {
                    Supplierinvoice.CreatedBy = UserId;
                    Supplierinvoice.CreatedDate = CommonFunctions.GetBranchDateTime();
                    db.SupplierInvoices.Add(Supplierinvoice);
                }
                else
                {
                    db.Entry(Supplierinvoice).State = EntityState.Modified;
                }
                db.SaveChanges();
                foreach (var item in IDetails)
                {
                    var InvoiceDetail = new SupplierInvoiceDetail();
                    InvoiceDetail.SupplierInvoiceID = Supplierinvoice.SupplierInvoiceID;
                    InvoiceDetail.AcHeadID = item.AcHeadId;
                    InvoiceDetail.Particulars = item.Particulars;
                    InvoiceDetail.Quantity = item.Quantity;
                    InvoiceDetail.Rate = item.Rate;
                    InvoiceDetail.CurrencyID = item.CurrencyID;
                    InvoiceDetail.CurrencyAmount = item.CurrencyAmount;
                    InvoiceDetail.Amount = item.Amount;
                    InvoiceDetail.TaxPercentage = item.TaxPercentage;
                    InvoiceDetail.Value = item.Value;

                    db.SupplierInvoiceDetails.Add(InvoiceDetail);
                    db.SaveChanges();

                    if (item.ItemTypeId>0)
                    {
                        var stock = new ItemPurchase();//  SupplierInvoiceStock();
                        //stock.SupplierInvoiceID = Supplierinvoice.SupplierInvoiceID;
                        stock.SupplierInvoiceDetailID = InvoiceDetail.SupplierInvoiceDetailID;
                        stock.StockType = "PU";//purhcase
                        stock.ReferenceNo= item.BookNo; 
                        stock.AWBCount = item.AWBCount;
                        stock.AWBNOFrom = item.AWBStart;
                        stock.AWBNOTo = item.AWBEnd;
                        stock.ItemTypeId = item.ItemTypeId;
                        stock.ItemId = item.ItemId;
                        stock.PurchaseDate = Convert.ToDateTime(InvoiceDate);
                        stock.Rate = InvoiceDetail.Rate;
                        //stock.SupplierID = SupplierID;
                        stock.Amount = item.Amount;
                        stock.CreatedBy = UserId;
                        stock.CreatedDate = CommonFunctions.GetBranchDateTime();
                        stock.ModifiedDate = CommonFunctions.GetBranchDateTime();
                        stock.ModifiedBy = UserId;
                        //stock.ItemSize = item.ItemSize;
                        stock.Qty = Convert.ToInt32(item.Quantity);
                        db.ItemPurchases.Add(stock);
                        db.SaveChanges();
                    }

                    //adding consignment referece to this entry
                    int acheadid = Convert.ToInt32(item.AcHeadId);

                    if (AWBAllocationall != null)
                    {

                        var list = AWBAllocationall.Where(cc => cc.AcHeadId == acheadid).ToList();
                        if (list != null)
                        {
                            foreach (var item2 in list)
                            {
                                SupplierInvoiceAWB accons = new SupplierInvoiceAWB();
                                accons.SupplierInvoiceId = Supplierinvoice.SupplierInvoiceID;
                                accons.SupplierInvoiceDetailId = item.SupplierInvoiceDetailID;
                                accons.AcHeadId = acheadid;
                                accons.InScanID = Convert.ToInt32(item2.InScanID);
                                accons.Amount = item2.Amount;
                                db.SupplierInvoiceAWBs.Add(accons);
                                db.SaveChanges();
                            }
                        }
                    }

                }

                PickupRequestDAO dao = new PickupRequestDAO();
                dao.GenerateSupplierInvoicePosting(Supplierinvoice.SupplierInvoiceID);

                return Json(new { status = "ok", message = "Invoice Submitted Successfully!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "failed", message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult DeleteConfirmed(int id)
        {
            string status = "";
            string message = "";
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteSupplierInvoice(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        status = dt.Rows[0][0].ToString();
                        message = dt.Rows[0][1].ToString();
                        //TempData["ErrorMsg"] = "Transaction Exists. Deletion Restricted !";
                        return Json(new { status = status, message = message });
                         
                    }

                }
                else
                {
                    return Json(new { status = "Failed", message = "Delete Failed!" });
                }
            }

            return Json(new { status = "Failed", message = "Delete Failed!" });

        }
        public ActionResult GetAWB(string term)
        {
            int AcCompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (term.Trim() != "")
            {
                var list = (from c in db.InScanMasters where c.IsDeleted == false && c.AWBNo== term.Trim() orderby c.AWBNo select new { InScanID = c.InScanID, TransactionDate = c.TransactionDate, ConsignmentNo = c.AWBNo }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AccountHead(string term)
        {
            int branchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (!String.IsNullOrEmpty(term))
            {
                List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
                AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID).Where(c => c.AcHead.ToLower().Contains(term.ToLower())).OrderBy(x => x.AcHead).ToList(); ;

                //List<AcHeadSelectAll_Result> AccountHeadList = new List<AcHeadSelectAll_Result>();
                //AccountHeadList =db.AcHeadSelectAll(branchID).Where(c => c.AcHead.ToLower().Contains(term.ToLower())).OrderBy(x => x.AcHead).ToList();
                return Json(AccountHeadList, JsonRequestBehavior.AllowGet);

                //List<AcHeadSelectAll_Result> AccountHeadList = new List<AcHeadSelectAll_Result>();
                //AccountHeadList = MM.AcHeadSelectAll(Common.ParseInt(Session["CurrentBranchID"].ToString()), term);

            }
            else
            {
                List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
                AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID);
                //List<AcHeadSelectAll_Result> AccountHeadList = new List<AcHeadSelectAll_Result>();
                //AccountHeadList = db.AcHeadSelectAll(branchID).ToList();
                return Json(AccountHeadList, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetAWBAllocation(int AcHeadId)
        {
            List<SupplierInvoiceAWBVM> AWBAllocationall = new List<SupplierInvoiceAWBVM>();
            List<SupplierInvoiceAWBVM> AWBAllocation = new List<SupplierInvoiceAWBVM>();
            AWBAllocationall = (List<SupplierInvoiceAWBVM>)Session["SIAWBAllocation"];
            if (AWBAllocationall == null)
            {
                return Json(AWBAllocation, JsonRequestBehavior.AllowGet);
            }
            else
            {
                AWBAllocation = AWBAllocationall.Where(cc => cc.AcHeadId == AcHeadId).ToList();
            }

            if (AWBAllocation == null)
            {
                AWBAllocation = new List<SupplierInvoiceAWBVM>();

            }
            return Json(AWBAllocation, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveAWBAllocation(List<SupplierInvoiceAWBVM> list)
        {

            List<SupplierInvoiceAWBVM> AWBAllocationall = new List<SupplierInvoiceAWBVM>();
            List<SupplierInvoiceAWBVM> AWBAllocation = new List<SupplierInvoiceAWBVM>();
            AWBAllocationall = (List<SupplierInvoiceAWBVM>)Session["SIAWBAllocation"];

            if (AWBAllocationall == null)
            {
                AWBAllocationall = new List<SupplierInvoiceAWBVM>();
                foreach (var item2 in list)
                {
                    AWBAllocationall.Add(item2);

                }

            }
            else
            {
                int acheadid = list[0].AcHeadId;
                AWBAllocationall.RemoveAll(cc => cc.AcHeadId == acheadid);
                foreach (var item2 in list)
                {
                    AWBAllocationall.Add(item2);

                }
            }

            Session["SIAWBAllocation"] = AWBAllocationall;

            return Json(AWBAllocationall, JsonRequestBehavior.AllowGet);

        }
    }
}