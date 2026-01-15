using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using CrystalDecisions.CrystalReports.ViewerObjectModel;
using Net4Courier.DAL;
using System.Data.Entity;
using Newtonsoft.Json;
//using System.IO;
//using Newtonsoft.Json;
//using System.Text.RegularExpressions;
//using System.Net.Mail;
//using System.Configuration;
//using System.Collections.Specialized;
//using System.Net;
//using System.Text;
//using Net4Courier.DAL;
//using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class ManifestReceiptController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 db = new Entities1();

        EditCommanFu editfu = new EditCommanFu();
        // GET: CODReceipt
        public ActionResult Index(CODReceiptSearch obj)
        {
            CODReceiptSearch model = new CODReceiptSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null || obj.FromDate.ToString().Contains("0001"))
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
                model = new CODReceiptSearch();
                model.FromDate = pFromDate;
                model.ToDate = pToDate;


                List<CODReceiptVM> lst = ReceiptDAO.GetCODReceiptList(model.FromDate, model.ToDate, 0, yearid, branchid);
                if (lst==null)
                {
                    model.Details = new List<CODReceiptVM>();
                }
                else
                {
                    model.Details = lst;
                }
                
            }
            else
            {
                model = obj;
                List<CODReceiptVM> lst = ReceiptDAO.GetCODReceiptList(obj.FromDate, obj.ToDate,obj.ShipmentInvoiceID, yearid, branchid);
                model.Details = lst;
            }
            return View(model);

        }





        public ActionResult Create(int id=0)
        {
            var CODReceiptSession= Session["CODReceipt"] as CODReceiptVM;

            CODReceiptVM vm = new CODReceiptVM();
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            
            var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();            
            var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

            ViewBag.achead = acheadforcash;
            ViewBag.acheadbank = acheadforbank;
            List<CurrencyMaster> Currencys = new List<CurrencyMaster>();
            Currencys = MM.GetCurrency();
            ViewBag.Currency = new SelectList(Currencys, "CurrencyID", "CurrencyName");

            //--DeliveryRunSheetRecieptController Agent
            ViewBag.Agents = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 4).OrderBy(cc => cc.SupplierName).ToList();

            vm.ReceiptDetails = new List<CODReceiptDetailVM>();
            vm.allocatedtotalamount = 0;

            if (id>0)
            {
                ViewBag.Title = "Manifest COD Receipt - Modify";
                var receipt = db.ManifestCODReceipts.Find(id);
                vm.ReceiptID = receipt.ReceiptID;
                vm.ReceiptDate = receipt.ReceiptDate;
                vm.ReceiptNo = receipt.ReceiptNo;
                vm.Remarks = receipt.Remarks;
            
                vm.CurrencyID = receipt.CurrencyID;
                vm.EXRate = receipt.EXRate;
                vm.AcHeadID = receipt.AcHeadID;
                vm.StatusEntry = receipt.StatusEntry;
                vm.Amount = receipt.Amount;
                vm.AgentID = receipt.AgentID;
                vm.ShipmentInvoiceID = receipt.ShipmentInvoiceID;
                vm.ChequeNo = receipt.ChequeNo;
                vm.ChequeDate = receipt.ChequeDate;
                var Invoice = db.ShipmentInvoices.Find(vm.ShipmentInvoiceID);
                    if (Invoice != null)
                    vm.InvoiceNo = Invoice.InvoiceNo;

                vm.ReceiptDetails = new List<CODReceiptDetailVM>();
                vm.ReceiptDetails = ReceiptDAO.GetReceiptTAXInvoiceDetail(0,vm.ReceiptID);
                vm.allocatedtotalamount = vm.Amount;
                Session["CODReceiptCreate"] = vm;
            }
            else if (CODReceiptSession!=null)
            {
                vm = CODReceiptSession;
            }
            else
            {
                ViewBag.Title = "Manifest COD Receipt - Create";
                PickupRequestDAO _dao = new PickupRequestDAO();
                vm.CurrencyID = SourceMastersModel.GetCompanyCurrencyID(companyid);
                vm.ReceiptNo = _dao.GetMaxCODReceiptNo(companyid, branchid);
                vm.StatusEntry = "CS";
                CODReceiptDetailVM detail = new CODReceiptDetailVM();                                               
            }
            
            return View(vm);
        }

        [HttpPost]
        public JsonResult GetManifest(int id)
        {
            //var manifests = (from c in db.ExportShipments where c.AgentID == id select new { ID = c.ID, c.ManifestNumber }).ToList();
            var manifests = ReceiptDAO.GetManifestId(id);
            return Json(new { data = manifests }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTaxInvoiceAll(string term)
        {
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var fyearid = Convert.ToInt32(Session["fyearid"]);
            var manifests = (from c in db.ShipmentInvoices where c.FYearId==fyearid && c.BranchId==branchid select new { ShipmentInvoiceID = c.ShipmentInvoiceID, InvoiceNo= c.InvoiceNo }).ToList();
           // var manifests = ReceiptDAO.GetVATInvoice(AgentId, fyearid, branchid);

            return Json(manifests, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTaxInvoice(string term,int AgentId)
        {
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var fyearid = Convert.ToInt32(Session["fyearid"]);
            //var manifests = (from c in db.ExportShipments where c.AgentID == id select new { ID = c.ID, c.ManifestNumber }).ToList();
            var manifests = ReceiptDAO.GetVATInvoice(AgentId,fyearid,branchid);
         
            return Json(manifests, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveReceipt(CODReceiptVM model,string Details)
        {
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            var fyearid = Convert.ToInt32(Session["fyearid"]);
            string savemessage = "";
            PickupRequestDAO _dao = new PickupRequestDAO();
            var IDetails = JsonConvert.DeserializeObject<List<CODReceiptDetailVM>>(Details);
            model.ReceiptDetails = IDetails;
            ManifestCODReceipt codreceipt = new ManifestCODReceipt();
            try
            {


                if (model.ReceiptID == 0)
                {
                    codreceipt.ReceiptNo = model.ReceiptNo;
                    codreceipt.ReceiptDate = model.ReceiptDate;
                    codreceipt.FYearID = fyearid;
                    codreceipt.Deleted = false;
                    codreceipt.BranchID = branchid;
                    codreceipt.AcCompanyID = companyid;
                    codreceipt.AgentID = model.AgentID;
                    codreceipt.AcJournalID = 0;
                    codreceipt.Amount = 0;
                    codreceipt.ShipmentInvoiceID = model.ShipmentInvoiceID;
                }
                else
                {
                    codreceipt = db.ManifestCODReceipts.Find(model.ReceiptID);
                }

                codreceipt.CurrencyID = model.CurrencyID;
                codreceipt.EXRate = model.EXRate;
                codreceipt.ShipmentInvoiceID = model.ShipmentInvoiceID;
                codreceipt.Amount = model.Amount;
                codreceipt.Remarks = model.Remarks;
                codreceipt.AcHeadID = model.AcHeadID;
                codreceipt.StatusEntry = model.StatusEntry;
                codreceipt.ChequeNo = model.ChequeNo;
                codreceipt.ChequeDate = model.ChequeDate;
                var achead = db.AcHeads.Find(codreceipt.AcHeadID);
                codreceipt.BankName = achead.AcHead1;
                if (model.ReceiptID == 0)
                {
                    db.ManifestCODReceipts.Add(codreceipt);
                    db.SaveChanges();
                    savemessage = "You have successfully Saved the COD Receipt";
                }
                else
                {
                    db.Entry(codreceipt).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                  
                    _dao.RevertCODReceiptUpdate(codreceipt.ReceiptID);
                    //var stockdetails = (from d in db.SupplierInvoiceStocks where d.SupplierInvoiceID == Supplierinvoice.SupplierInvoiceID select d).ToList();
                    var sdetails = (from d in db.ManifestCODReceiptDetails where d.ReceiptID == codreceipt.ReceiptID select d).ToList();
                    db.ManifestCODReceiptDetails.RemoveRange(sdetails);
                    db.SaveChanges();
                    savemessage = "You have successfully Updated the COD Receipt";
                }

                //Detail table save and udpate
                foreach (var item in model.ReceiptDetails)
                {
                    if (item.ReceiptDetailID == 0)
                    {
                        if (item.COD > 0 )
                        {
                            ManifestCODReceiptDetail detail = new ManifestCODReceiptDetail();
                            detail.ReceiptID = codreceipt.ReceiptID;
                            detail.InboundShipmentID = item.InboundShipmentID;
                            //detail.ShipmentInvoiceID = item.ManifestID;
                            detail.AWBNo = item.AWBNo;                            
                            detail.COD = item.COD;                            
                            db.ManifestCODReceiptDetails.Add(detail);
                            db.SaveChanges();
                        }
                    }
          

                }

              
                _dao.GenerateCODPosting(codreceipt.ReceiptID);
                return Json(new { status = "ok", message = "Manifest COD Receipt Saved Successfully!" }, JsonRequestBehavior.AllowGet);
                
            }

            catch(Exception ex)
            {
                savemessage = ex.Message;
            


                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

            
        }
        public ActionResult  GetInvoiceDetail(int ShipmentInvoiceID)
        {
            var awblist = ReceiptDAO.GetReceiptTAXInvoiceDetail(ShipmentInvoiceID);
            Session["ManifestTaxInvoiceDetail"] = awblist;
            CODReceiptVM vm = new CODReceiptVM();
            vm.ReceiptDetails = awblist;
            
            return Json(new { status = "ok", Message = "Invoice Item found" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowAWBList()
        {
            CODReceiptVM vm = new CODReceiptVM();
            vm.ReceiptDetails = (List<CODReceiptDetailVM>)Session["ManifestTaxInvoiceDetail"];
            return PartialView("ReceiptDetail", vm);
        }
        //[HttpPost]
        //public JsonResult GetManifestID(CODReceiptVM ship)
        //{
        //    ship.ManifestID = "";
        //    if (ship.SelectedValues != null)
        //    {
        //        foreach (var item in ship.SelectedValues)
        //        {
        //            if (ship.ManifestID == "")
        //            {
        //                ship.ManifestID = item.ToString();
        //            }
        //            else
        //            {
        //                ship.ManifestID = ship.ManifestID + "," + item.ToString();
        //            }

        //        }
        //    }
        //    return Json(new { manifestids = ship.ManifestID }, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]        
        //public ActionResult GetManifestAWB(CODReceiptVM ship)
        //{
        //    ship.ManifestID = "";
        //    if (ship.SelectedValues != null)
        //    {
        //        foreach (var item in ship.SelectedValues)
        //        {
        //            if (ship.ManifestID == "")
        //            {
        //                ship.ManifestID = item.ToString();
        //            }
        //            else
        //            {
        //                ship.ManifestID = ship.ManifestID + "," + item.ToString();
        //            }

        //        }
        //    }


        //    List<CODReceiptDetailVM> manifests = (from c in db.ExportShipments  join d in db.ExportShipmentDetails on c.ID equals d.ExportID
        //                     join i in db.InScanMasters on d.InscanId equals i.InScanID
        //                     where c.AgentID == ship.AgentID &&  ship.ManifestID.Contains(c.ID.ToString()) 
        //                     &&  i.PaymentModeId==2
        //                     && i.IsDeleted==false
        //                     && i.NetTotal>0 //COd
        //                       select new CODReceiptDetailVM { InScanId  = i.InScanID,ManifestID=c.ID,ManifestNumber=c.ManifestNumber,
        //                           AWBNo=d.AWBNo ,AWBDate= i.TransactionDate,Consignee=i.Consignee,ConsigneePhone=i.ConsigneePhone,CourierCharge=((decimal)(i.CourierCharge != null ? i.CourierCharge : 0)),OtherCharge=i.OtherCharge,TotalCharge=(decimal)(i.NetTotal !=null ? i.NetTotal : 0)}).ToList();



        //    if  (ship.Amount >0)
        //    {
        //        decimal totalamount = ship.Amount;
        //        int i = 0;
        //        while(totalamount>0 && i<manifests.Count)
        //        {
        //            if (manifests[i].TotalCharge<=totalamount)
        //            {
        //                manifests[i].AmountAllocate = manifests[i].TotalCharge;
        //                manifests[i].Discount = 0;
        //                totalamount = totalamount - manifests[i].AmountAllocate;
        //                i++;
        //            }
        //            else
        //            {
        //                manifests[i].AmountAllocate = totalamount;
        //                manifests[i].Discount = 0;
        //                totalamount = totalamount - manifests[i].AmountAllocate;
        //                i++;

        //            }
        //        }
        //        if (totalamount>0)
        //        {
        //            ship.allocatedtotalamount = totalamount;
        //        }
        //        else
        //        {
        //            ship.allocatedtotalamount = ship.Amount;
        //        }

        //    }
        //    ship.ReceiptDetails = manifests;
        //    Session["CODReceiptCreate"] = ship;

        //    return PartialView("ReceiptDetail", ship);            


        //}

        public JsonResult DeleteConfirmed(int id)
        {

            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteManifestReceipt(id);
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
            return Json(new { status = "Failed", message = "Contact Admin!" });

        }

        //public ActionResult Details(int id)
        //{
        
        //    CODReceiptVM vm = new CODReceiptVM();
        //    var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
        //    var companyid = Convert.ToInt32(Session["CurrentCompanyID"]);                    
                        
        //    vm.ReceiptDetails = new List<CODReceiptDetailVM>();
        //    if (id > 0)
        //    {
        //        var receipt = db.CODReceipts.Find(id);
        //        vm.ReceiptID = receipt.ReceiptID;
        //        vm.ReceiptDate = receipt.ReceiptDate;
        //        vm.ReceiptNo = receipt.ReceiptNo;
        //        vm.Remarks = receipt.Remarks;
        //        vm.ManifestID = receipt.ManifestID;
        //        vm.CurrencyID = receipt.CurrencyID;
        //        vm.EXRate = receipt.EXRate;
        //        vm.AchHeadID = receipt.AchHeadID;
        //        vm.AcHeadName = db.AcHeads.Find(receipt.AchHeadID).AcHead1;
        //        vm.CurrencyName = db.CurrencyMasters.Find(receipt.CurrencyID).CurrencyName;
        //        vm.Amount = receipt.Amount;
        //        vm.AgentID = receipt.AgentID;
        //        vm.AgentName = db.AgentMasters.Find(vm.AgentID).Name;
        //        List<CODReceiptDetailVM> receiptdetails = (from c in db.CODReceiptDetails
        //                                                   join ins in db.InScanMasters on c.InScanId equals ins.InScanID
        //                                                   join i in db.ExportShipments on c.ManifestID equals i.ID

        //                                                   where c.ReceiptID == vm.ReceiptID
        //                                                   select new CODReceiptDetailVM
        //                                                   {
        //                                                       InScanId = c.InScanId,
        //                                                       ManifestID = c.ManifestID,
        //                                                       ManifestNumber = i.ManifestNumber,
        //                                                       AWBNo = c.AWBNo,
        //                                                       AWBDate = ins.TransactionDate,
        //                                                       Consignee = c.Consignee,
        //                                                       ConsigneePhone = c.ConsigneePhone,
        //                                                       CourierCharge = c.CourierCharge,
        //                                                       OtherCharge = c.OtherCharge,
        //                                                       TotalCharge = c.TotalCharge,
        //                                                       AmountAllocate = c.AmountAllocate,
        //                                                       Discount = c.Discount
        //                                                   }).ToList();

        //        vm.ReceiptDetails = receiptdetails;            
        //    }
            

        //    return View(vm);
        //}
    }
}