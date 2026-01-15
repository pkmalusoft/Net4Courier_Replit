using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.IO;
using Net4Courier.DAL;
using System.Data.Entity;
using System.Reflection;
using Newtonsoft.Json;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class DRSReconcController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 db = new Entities1();
        EditCommanFu editfu = new EditCommanFu();
        // GET: DRSReconc
        [HttpGet]
        public ActionResult Index()
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRRSearchVM obj = (DRRSearchVM)Session["DRRSearchVM"];
            DRRSearchVM model = new DRRSearchVM();
            DateTime pFromDate;
            DateTime pToDate;
            if (obj == null)
            {
                obj = new DRRSearchVM();
                pFromDate = CommonFunctions.GetLastDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // // ToDate = DateTime.Now;

                pFromDate = AccountsDAO.CheckParamDate(pFromDate, FyearId).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, FyearId).Date;
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.DRRNo = "";
                Session["DRRSearchVM"] = obj;
                model = obj;

            }
            else
            {
                model = obj;
              
                
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, FyearId);
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, FyearId);
                Session["DRRSearchVM"] = model;
            }

            List<DRRVM> list = ReceiptDAO.GetDRRList(FyearId, model.FromDate, model.ToDate,model.DRRNo); // RP.GetAllReciepts();
            model.Details = list;
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(DRRSearchVM obj)
        {
            Session["DRRSearchVM"] = obj;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Create(int id = 0)
        {
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRRVM cust = new DRRVM();
            cust.Details = new List<DRRDetailVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    //cust = RP.GetRecPayByRecpayID(id);
                    DRR dr = db.DRRs.Find(id);
                    cust.DRRID = dr.DRRID;
                    cust.DRRDate = dr.DRRDate;
                    cust.CourierId = dr.CourierId;
                    cust.DeliveredBy = Convert.ToInt32(dr.CourierId);
                    cust.DRSID = dr.DRSID;
                    //var acjournal = db.AcJournalMasters.Find(dr.AcJournalID);
                    cust.DRRNo = dr.DRRNo;;
                    ViewBag.DRRNo = dr.DRRNo;
                    //if (acjournal != null)
                    //{
                    //    ViewBag.DRRNo = acjournal.VoucherNo;
                    //    cust.DRRNo = acjournal.VoucherNo;
                    //}

                    var drs = db.DRS.Find(dr.DRSID);
                    if (drs != null)
                    {
                        cust.DRSNo = drs.DRSNo;
                        cust.DRSDate = drs.DRSDate;
                    }
                    var drsrecpay = db.DRSRecPays.Find(dr.DRSRecPayID);
                    if (drsrecpay != null)
                    {
                        cust.DRSRecPayID = dr.DRSRecPayID;
                        cust.DRSReceiptNo = drsrecpay.DocumentNo;
                        cust.DRSReceiptDate = drsrecpay.DRSRecPayDate.ToString("dd/MM/yyyy");
                        cust.ReceiptDate = drsrecpay.DRSRecPayDate;
                        cust.ReceivedAmount = Convert.ToDecimal(drsrecpay.ReceivedAmount);
                        var totalawb = ReceiptDAO.GetSklyarkTotalAWB(Convert.ToInt32(dr.DRSRecPayID));
                        ViewBag.TotalAWB = totalawb;

                    }
                    cust.ReconciledAmount = dr.ReconciledAmount;

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    cust.Details = new List<DRRDetailVM>();
                    cust.Details = (from c in db.DRRDetails where c.DRRID == id select new DRRDetailVM { DRRID = c.DRRID, DRRDetailID = c.ID, Reference = c.Reference, ReferenceId = c.ReferenceId, COD = c.CODAmount, CODVat = c.CODVat, PKPCash = c.PKPCash, MCReceived = c.MCReceived, MCPayment = c.MCPaid, Discount = c.Discount, Expense = c.Expense, Total = c.Total, Type = c.TransactionType, PODStatus = c.PODStatus, Receipt = c.Receipt, ChequeStatus = c.ChequeStatus, ChequeAmount = c.ChequeAmount, SettlementAmount = c.SettlementAmount, ChequeNo = c.ChequeNo, RecPayID=c.RecPayID }).ToList();
                    ViewBag.Title = "Modify - " + cust.DRRNo;
                    //BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "Create";
                    ViewBag.DRRNo = "";

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    //cust.DRSReceiptDate = pFromDate;
                    cust.DRSRecPayID = 0;
                    cust.DRRDate = CommonFunctions.GetCurrentDateTime();
                    //cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }

            return View(cust);

        }

        [HttpGet]
        public JsonResult GetDRSReceiptNo(string term, int DeliveredBy = 0)
        {
             int FyearId = Convert.ToInt32(Session["fyearid"]);
            if (term.Trim() != "")
            {
                var drslist = (from c1 in db.DRSRecPays
                                   //join c2 in db.DRS on c1.DRSID equals c2.DRSID
                               where c1.DocumentNo.ToLower().Contains(term.ToLower()) //oct 15 2024 // && c1.FYearID == FyearId
                                && c1.DRRID == null
                                && (c1.DeliveredBy == DeliveredBy || DeliveredBy == 0)
                               orderby c1.DocumentNo descending
                               select new { DRSRecPayId = c1.DRSRecPayID, DRSRecPayDate = c1.DRSRecPayDate, DocumentNo = c1.DocumentNo, DRSID = c1.DRSID, DeliveredBy = c1.DeliveredBy, ReceivedAmount = c1.ReceivedAmount, CollectedAmount = c1.CollectedAmount }).ToList();

                return Json(drslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var drslist = (from c1 in db.DRSRecPays
                                   //join c2 in db.DRS on c1.DRSID equals c2.DRSID
                               where  //c1.FYearID == FyearId commented as on oct 15 2024
                                c1.DRRID == null
                               && (c1.DeliveredBy == DeliveredBy || DeliveredBy == 0)
                               orderby c1.DocumentNo descending
                               select new { DRSRecPayId = c1.DRSRecPayID, DRSRecPayDate = c1.DRSRecPayDate, DocumentNo = c1.DocumentNo, DRSID = c1.DRSID, DeliveredBy = c1.DeliveredBy, ReceivedAmount = c1.ReceivedAmount, CollectedAmount = c1.CollectedAmount }).ToList();
                return Json(drslist, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetDRS(string term)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            if (term.Trim() != "")
            {
                var drslist = (from c1 in db.DRSRecPays
                               join c2 in db.DRS on c1.DRSID equals c2.DRSID
                               where c1.DocumentNo.ToLower().Contains(term.ToLower()) && c1.FYearID == FyearId
                                && c1.DRRID == null
                               orderby c1.DocumentNo ascending
                               select new { DRSRecPayId = c1.DRSRecPayID, DRSRecPayDate = c1.DRSRecPayDate, DocumentNo = c1.DocumentNo, DRSID = c1.DRSID, DRSNo = c2.DRSNo, DRSDate = c2.DRSDate, DeliveredBy = c1.DeliveredBy, ReceivedAmount = c1.ReceivedAmount }).ToList();

                return Json(drslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var drslist = (from c1 in db.DRSRecPays
                               join c2 in db.DRS on c1.DRSID equals c2.DRSID
                               where c1.FYearID == FyearId
                               && c1.DRRID == null
                               orderby c1.DocumentNo ascending
                               select new { DRSRecPayId = c1.DRSRecPayID, DRSRecPayDate = c1.DRSRecPayDate, DocumentNo = c1.DocumentNo, DRSID = c1.DRSID, DRSNo = c2.DRSNo, DRSDate = c2.DRSDate, DeliveredBy = c1.DeliveredBy, ReceivedAmount = c1.ReceivedAmount}).ToList();
                return Json(drslist, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetCourierAWBDetail(int DRSRecPayID)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            
            var drslist = ReceiptDAO.GetSklyarkAWB(DRSRecPayID);
            return Json(drslist, JsonRequestBehavior.AllowGet);
            

        }

        [HttpGet]
        public JsonResult GetSkylarkTotalAWB(int DRSRecPayID)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);

            var totalawb = ReceiptDAO.GetSklyarkTotalAWB(DRSRecPayID);
            return Json(totalawb, JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public JsonResult GetDRSDetail(int DRSID)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);

            var drslist = (from c1 in db.DRS
                           where c1.DRSID == DRSID
                           select new DRSVM { DRSID = c1.DRSID, DRSNo = c1.DRSNo, DRSDate = c1.DRSDate, DeliveredBy = c1.DeliveredBy }).FirstOrDefault();

            if (drslist != null)
                drslist.DRSDate = Convert.ToDateTime(drslist.DRSDate.ToShortDateString());

            return Json(drslist, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult GetDRSAWB(string term, int DRSID, int DRSRecPayId = 0)
        {
                int FyearId = Convert.ToInt32(Session["fyearid"]);

            var drslist = ReceiptDAO.GetDRSAWBPending(DRSID, term, DRSRecPayId);
            return Json(drslist, JsonRequestBehavior.AllowGet);
            //if (term.Trim() != "")
            //{
            //    var drslist = (from c3 in db.InScanMasters 
            //                   where c3.AWBNo.ToLower().Contains(term.Trim().ToLower()) && c3.AcFinancialYearID == FyearId
            //                   && (c3.DRSID !=null)
            //                   && c3.IsDeleted == false
            //                   && (c3.PaymentModeId==2 && c3.CODReceiptId==null)                               
            //                   orderby c3.AWBNo ascending
            //                   select new { InscanId= c3.InScanID, AWBNo=c3.AWBNo,Shipper=c3.Consignor,Consignee=c3.Consignee,AWBDate=c3.TransactionDate,CourierCharge=c3.CourierCharge,OtherCharge=c3.OtherCharge,TotalCharge=c3.NetTotal,MaterialCost=c3.MaterialCost,IsNCND=c3.IsNCND,IsCashOnly=c3.IsCashOnly,IsCollectMaterial=c3.IsCollectMaterial,IsCheque=c3.IsChequeOnly, IsDoCopyBack =c3.IsDOCopyBack ,Pieces=c3.Pieces,Weight=c3.Weight}).ToList();

            //    return Json(drslist, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var drslist = (from c3 in db.InScanMasters 
            //                   where c3.AcFinancialYearID == FyearId
            //                   && c3.IsDeleted==false
            //                   //&& (c3.DRSID == DRSID || DRSID == 0)
            //                   && (c3.DRSID != null)
            //                   && (c3.PaymentModeId == 2 && c3.CODReceiptId == null)
            //                   orderby c3.AWBNo ascending
            //                    select new { InscanId = c3.InScanID, AWBNo = c3.AWBNo, Shipper = c3.Consignor, Consignee = c3.Consignee, AWBDate = c3.TransactionDate, CourierCharge = c3.CourierCharge, OtherCharge = c3.OtherCharge, TotalCharge = c3.NetTotal, MaterialCost = c3.MaterialCost, IsNCND = c3.IsNCND, IsCashOnly = c3.IsCashOnly, IsCollectMaterial = c3.IsCollectMaterial, IsCheque = c3.IsDOCopyBack, d = c3.IsDOCopyBack, Pieces = c3.Pieces, Weight = c3.Weight }).ToList();

            //    return Json(drslist, JsonRequestBehavior.AllowGet);
            //}

        }

        [HttpGet]
        public JsonResult GetDRSAWB1(int DRSID)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);

            List<DRSAWBList> drslist1 = (from c3 in db.InScanMasters
                                         where c3.AcFinancialYearID == FyearId
                                         && c3.IsDeleted == false
                                         && (c3.DRSID == DRSID)
                                         && (c3.DRRId == null)
                                         && (c3.PaymentModeId == 2 && c3.CODReceiptId == null)
                                         orderby c3.AWBNo ascending
                                         select new DRSAWBList { InscanId = c3.InScanID, ShipmentDetailID = 0, AWBNo = c3.AWBNo, Shipper = c3.Consignor, Consignee = c3.Consignee, AWBDate = c3.TransactionDate, CourierCharge = c3.CourierCharge, OtherCharge = c3.OtherCharge, TotalCharge = c3.NetTotal, MaterialCost = c3.MaterialCost, IsNCND = c3.IsNCND, IsCashOnly = c3.IsCashOnly, IsCollectMaterial = c3.IsCollectMaterial, IsCheque = c3.IsChequeOnly, IsDOCopyBack = c3.IsDOCopyBack, Pieces = c3.Pieces, Weight = c3.Weight,CODVat=0, TotalVAT = 0 }).ToList();


            List<DRSAWBList> drslist3 = (from c3 in db.ImportShipmentDetails
                                         join inv in db.ShipmentInvoiceDetails on c3.ShipmentDetailID equals inv.ShipmentImportDetailIDOld
                                         where (c3.DRSID == DRSID)
                                         && (c3.DRSID != null)
                                         && (c3.DRRID == null)
                                         && (c3.CODReceiptId == null)
                                         && (c3.CourierStatusID == 13)  //Delivered
                                         orderby c3.AWB ascending
                                         select new DRSAWBList { InscanId = 0, ShipmentDetailID = c3.ShipmentDetailID, AWBNo = c3.AWB, Shipper = c3.Shipper, Consignee = c3.Receiver, AWBDate = c3.AWBDate, CourierCharge = 0, OtherCharge = 0, TotalCharge = 0, MaterialCost = 0, IsNCND = false, IsCashOnly = false, IsCollectMaterial = false, IsCheque = false, IsDOCopyBack = false, Pieces = c3.PCS.ToString(), Weight = c3.Weight, CODVat = inv.Tax + inv.adminCharges, TotalVAT = inv.Tax + inv.adminCharges }).ToList();
            //               select new DRSAWBList { InscanId =0, ShipmentDetailID = c3.ShipmentDetailID, AWBNo = c3.AWB, Shipper = c3.Shipper, Consignee = c3.Receiver, AWBDate = c3.AWBDate, CourierCharge = inv.Tax+inv.adminCharges, OtherCharge =0, TotalCharge =inv.Tax + inv.adminCharges , MaterialCost = 0, IsNCND = false , IsCashOnly = false, IsCollectMaterial = false, IsCheque = false, IsDOCopyBack = false, Pieces = c3.PCS.ToString(), Weight = c3.Weight }).ToList();

            List<DRSAWBList> drslist2 = (from c3 in db.InboundShipments
                                         join inv in db.ShipmentInvoiceDetails on c3.ShipmentID equals inv.ShipmentImportDetailID
                                         where (c3.DRSID == DRSID)
                                         && (c3.DRRID == null)
                                         // --&& (c3.DRRID == null)
                                         // (c3.CODReceiptId == null)
                                         && (c3.CourierStatusID == 13)  //Delivered
                                         orderby c3.AWBNo ascending
                                         select new DRSAWBList { InscanId = 0, ShipmentDetailID = c3.ShipmentID, AWBNo = c3.AWBNo, Shipper = c3.Consignor, Consignee = c3.Consignee, AWBDate = c3.AWBDate, CourierCharge = 0, OtherCharge = 0, TotalCharge = 0, MaterialCost = c3.MaterialCost, IsNCND = false, IsCashOnly = false, IsCollectMaterial = false, IsCheque = false, IsDOCopyBack = false, Pieces = c3.Pieces.ToString(), Weight = c3.Weight,  CODVat = inv.Tax + inv.adminCharges, TotalVAT = inv.Tax + inv.adminCharges }).ToList();

            if (drslist3.Count>0)
            {
                foreach (var item in drslist3)
                {
                    drslist1.Add(item);
                }

            }
            if (drslist1.Count > 0 && drslist2.Count > 0)
            {
                foreach (var item in drslist2)
                {
                    drslist1.Add(item);
                }

                return Json(drslist1, JsonRequestBehavior.AllowGet);
            }
            else if (drslist1.Count > 0 && drslist2.Count == 0)
            {
                return Json(drslist1, JsonRequestBehavior.AllowGet);
            }
            else if (drslist2.Count > 0)
            {
                return Json(drslist2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<DRSAWBList> lst = new List<DRSAWBList>();

                return Json(lst, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public string SaveReconc(int DRRID, DateTime DRRDate, int DRSID, int DRSRecpayID, decimal ReconcAmount, int CourierId, string Details)
        {
            var IDetails = JsonConvert.DeserializeObject<List<DRRDetailVM>>(Details);
            DataTable ds = new DataTable();
            DataSet dt = new DataSet();
            dt = ToDataTable(IDetails);
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            string xml = dt.GetXml();
            if (Session["UserID"] != null)
            {
                int userid = Convert.ToInt32(Session["UserID"].ToString());
                int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                DateTime drrdate = DRRDate; // CommonFunctions.GetCurrentDateTime();
                ReceiptDAO.SaveReconc(DRRID, drrdate, DRSID, DRSRecpayID, ReconcAmount, CourierId, BranchId, FyearId, userid, xml);
                return "Ok";
            }
            else
            {
                return "Failed!";
            }

        }

        [HttpGet]
        public JsonResult DeleteDRSAWB(int DrrDetailID, int DRRID)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);

            var drslist = ReceiptDAO.DeleteDRSReconcDetail(DRRID, DrrDetailID);

            return Json("ok", JsonRequestBehavior.AllowGet);


        }

        [HttpGet]
        public ActionResult Details(int id = 0)
        {
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRRVM cust = new DRRVM();
            cust.Details = new List<DRRDetailVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "DRS Reconciliation - Modify";
                    //cust = RP.GetRecPayByRecpayID(id);
                    DRR dr = db.DRRs.Find(id);
                    cust.DRRID = dr.DRRID;
                    cust.DRRDate = dr.DRRDate;
                    cust.CourierId = dr.CourierId;
                    cust.DeliveredBy = Convert.ToInt32(dr.CourierId);
                    var emp = db.EmployeeMasters.Find(cust.DeliveredBy);
                    cust.DeliveredByName = emp.EmployeeName;
                    cust.DRSID = dr.DRSID;
                    var acjournal = db.AcJournalMasters.Find(dr.AcJournalID);
                    cust.DRRNo = "";
                    ViewBag.DRRNo = "";
                    if (acjournal != null)
                    {
                        ViewBag.DRRNo = acjournal.VoucherNo;
                        cust.DRRNo = acjournal.VoucherNo;
                    }

                    var drs = db.DRS.Find(dr.DRSID);
                    if (drs != null)
                    {
                        cust.DRSNo = drs.DRSNo;
                        cust.DRSDate = drs.DRSDate;
                    }
                    var drsrecpay = db.DRSRecPays.Find(dr.DRSRecPayID);
                    if (drsrecpay != null)
                    {
                        cust.DRSRecPayID = dr.DRSRecPayID;
                        cust.DRSReceiptNo = drsrecpay.DocumentNo;
                        cust.DRSReceiptDate = drsrecpay.DRSRecPayDate.ToString("dd/MM/yyyy");
                        cust.ReceiptDate = drsrecpay.DRSRecPayDate;
                        cust.ReceivedAmount = Convert.ToDecimal(drsrecpay.ReceivedAmount);
                    }
                    cust.ReconciledAmount = dr.ReconciledAmount;

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    cust.Details = new List<DRRDetailVM>();
                    cust.Details = (from c in db.DRRDetails where c.DRRID == id select new DRRDetailVM { DRRID = c.DRRID, DRRDetailID = c.ID, Reference = c.Reference, ReferenceId = c.ReferenceId, COD = c.CODAmount, PKPCash = c.PKPCash, MCReceived = c.MCReceived, MCPayment = c.MCPaid, Discount = c.Discount, Expense = c.Expense, Total = c.Total, Type = c.TransactionType, PODStatus = c.PODStatus, Receipt = c.Receipt, ChequeStatus = c.ChequeStatus, ChequeAmount = c.ChequeAmount, SettlementAmount = c.SettlementAmount, ChequeNo = c.ChequeNo, Confirmation = c.confirmation }).ToList();

                    //BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "DRS Reconciliation - Create";
                    ViewBag.DRRNo = "";

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    //cust.DRSReceiptDate = pFromDate;
                    cust.DRSRecPayID = 0;
                    cust.DRRDate = CommonFunctions.GetCurrentDateTime();
                    //cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }

            return View(cust);

        }

        public static DataSet ToDataTable<T>(List<T> items)
        {
            DataSet ds = new DataSet();
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            ds.Tables.Add(dataTable);
            //put a breakpoint here and check datatable
            return ds;
        }
        [HttpGet]
        public ActionResult CreateBulk(int id = 0)
        {
            ViewBag.Deliverdby = db.EmployeeMasters.OrderBy(cc => cc.EmployeeName).ToList();
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRRVM cust = new DRRVM();
            cust.Details = new List<DRRDetailVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "DRS Reconciliation - Modify";
                    //cust = RP.GetRecPayByRecpayID(id);
                    DRR dr = db.DRRs.Find(id);
                    cust.DRRID = dr.DRRID;
                    cust.DRRDate = dr.DRRDate;
                    cust.CourierId = dr.CourierId;
                    cust.DeliveredBy = Convert.ToInt32(dr.CourierId);
                    cust.DRSID = dr.DRSID;
                    var drs = db.DRS.Find(dr.DRSID);
                    cust.DRSNo = drs.DRSNo;
                    cust.DRSDate = drs.DRSDate;
                    var drsrecpay = db.DRSRecPays.Find(dr.DRSRecPayID);
                    cust.DRSRecPayID = dr.DRSRecPayID;
                    cust.DRSReceiptNo = drsrecpay.DocumentNo;
                    cust.DRSReceiptDate = drsrecpay.DRSRecPayDate.ToString("dd/MM/yyyy");
                    cust.ReceiptDate = drsrecpay.DRSRecPayDate;
                    cust.ReceivedAmount = Convert.ToDecimal(drsrecpay.ReceivedAmount);
                    cust.ReconciledAmount = dr.ReconciledAmount;

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    cust.Details = new List<DRRDetailVM>();
                    cust.Details = (from c in db.DRRDetails where c.DRRID == id select new DRRDetailVM { DRRID = c.DRRID, Reference = c.Reference, ReferenceId = c.ReferenceId, COD = c.CODAmount, PKPCash = c.PKPCash, MCReceived = c.MCReceived, Discount = c.Discount, Expense = c.Expense, Total = c.Total }).ToList();

                    //BindMasters_ForEdit(cust);
                }
                else
                {
                    ViewBag.Title = "(Summary) Create";


                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    //cust.DRSReceiptDate = pFromDate;
                    cust.DRSRecPayID = 0;
                    cust.DRRDate = CommonFunctions.GetCurrentDateTime();
                    //cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }

            return View(cust);

        }
        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteDRSReconc(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string status = dt.Rows[0][0].ToString();
                        string message = dt.Rows[0][1].ToString();

                        return Json(new { status = status, message = message });
                    }

                }
                else
                {
                    return Json(new { status = "OK", message = "Contact Admin!" });
                }
            }

            return Json(new { status = "OK", message = "Contact Admin!" });


        }
    }
        

    public class DRSAWBList
    {
        public int InscanId { get; set; }
            public int ShipmentDetailID { get; set; }            
            public string AWBNo { get; set; }
            
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public DateTime AWBDate { get; set; }
        public decimal? CourierCharge { get; set; }       
        public decimal? OtherCharge { get; set; }
        public decimal? TotalCharge { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? TotalVAT { get; set; }
        public decimal? CODVat { get; set; }
        public bool IsNCND { get; set; }
        public bool IsCashOnly { get; set; }
        public bool  IsCollectMaterial { get; set; }
        public bool  IsCheque { get; set; }
        public bool IsDOCopyBack { get; set; }
        public string Pieces { get; set; }
       public decimal? Weight { get; set; }
    }
}