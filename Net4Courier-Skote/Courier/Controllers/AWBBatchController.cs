using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Reflection;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class AWBBatchController : Controller
    {
        Entities1 db = new Entities1();

        // GET: AWBBatch
        public ActionResult Index()
        {

            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            AWBBatchSearch obj = (AWBBatchSearch)Session["AWBBatchSearch"];
            AWBBatchSearch model = new AWBBatchSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<AWBBatchList> translist = new List<AWBBatchList>();
                
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo;
                translist = AWBDAO.GetAWBBatchList(BranchID,FyearId,model,0);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["AWBBatchSearch"] = model;
                List<AWBBatchList> translist = new List<AWBBatchList>();                
                translist = AWBDAO.GetAWBBatchList(BranchID,FyearId,model,0);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(AWBBatchSearch obj)
        {
            Session["AWBBatchSearch"] = obj;
            return RedirectToAction("Index");
        }
        //Screen Preview option
        public ActionResult AWBBatchPrintReport(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());


            string companyname = Session["CompanyName"].ToString();
            if (companyname.Trim() == "Airbest Express Cargo & Courier Llc")
            {
                AWBDAO.GenerateAWBBatchReport(id);
            }
            else
            {
                AWBDAO.GenerateAWBBatchReport(id);
            }

            ViewBag.ReportName = "AWB Batch Print";
            return View();

        }

        public ActionResult Create(int id=0)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            AWBBatchVM vm = new AWBBatchVM();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList().OrderBy(cc=>cc.EmployeeName);
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.Vehicle = db.VehicleMasters.ToList().OrderBy(cc=>cc.VehicleDescription);
            ViewBag.Title = "Create";

            if (id == 0)
            {
                
                DateTime pFromDate = CommonFunctions.GetCurrentDateTime(); //  AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                string DocNo = AWBDAO.GetMaxBathcNo(pFromDate,branchid,FyearId); //batch no
                vm.BatchNumber = DocNo;
                vm.BatchDate = pFromDate;
                vm.AWBDate = pFromDate;
                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    vm.BranchLocation = branch.LocationName;
                    vm.BranchCountry = branch.CountryName;
                    vm.BranchCity = branch.CityName;
                }
                vm.AssignedDate = pFromDate;
                vm.TaxPercent = 5;
                vm.ID = 0;
                List<AWBBatchDetail> details = new List<AWBBatchDetail>();
                vm.Details = details;
            }
            else if (id > 0)
            {
                ViewBag.Title = "Modify";
                AWBBatch batch = db.AWBBatches.Find(id);
                vm.ID = batch.ID;
                    vm.BatchNumber = batch.BatchNumber;
                vm.BatchDate = batch.BatchDate;
                vm.TotalAWB = batch.TotalAWB;
                var branch = db.BranchMasters.Find(branchid);
                if (branch != null)
                {
                    vm.BranchLocation = branch.LocationName;
                    vm.BranchCountry = branch.CountryName;
                    vm.BranchCity = branch.CityName;
                }
                if (batch.QuickInScanId != null && batch.QuickInScanId>0)
                {
                    vm.QuickInScanId = batch.QuickInScanId;
                    vm.CollectedBy = true;
                    vm.ReceivedBy = true;
                    vm = BindInscanDetail(vm);
                }
                if (batch.DRSId !=null && batch.DRSId>0)
                {
                    vm.OutScanDelivery = true;
                    vm.DRSId = batch.DRSId;
                    vm = BindDRSDetails(vm);
                }

                if (batch.PODID!=null && batch.PODID>0)
                {
                    vm.Delivered = true;
                    vm.PODID = batch.PODID;
                    vm = BindPODDetails(vm);
                }
                vm.TaxPercent = 5;
                vm.AWBDate = CommonFunctions.GetCurrentDateTime();
                List<AWBBatchDetail> details = new List<AWBBatchDetail>();
                
                details = AWBDAO.GetBatchAWBInfo(id);
                vm.Details = details;
                
            }
            
            
            
            var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultproducttype != null)
                vm.ProductTypeID = defaultproducttype.ProductTypeID;

            var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultmovementtype != null)
                vm.MovementID = defaultmovementtype.MovementID;

            var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultparceltype != null)
                vm.ParcelTypeID = defaultparceltype.ID;

            string customername = "";
            

            customername = "WALK-IN-CUSTOMER";
            var CashCustomer = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CashCustomer != null)
            {
                vm.CASHCustomerId = CashCustomer.CustomerID;
                vm.CASHCustomerName = customername;
            }

            customername = "COD-CUSTOMER";
            var CODCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CODCustomer != null)
            {
                vm.CODCustomerID = CODCustomer.CustomerID;
                vm.CODCustomerName = "COD-CUSTOMER";
            }

            customername = "FOC CUSTOMER";
            var FOCCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (FOCCustomer != null)
            {
                vm.FOCCustomerID = FOCCustomer.CustomerID;
                vm.FOCCustomerName = "FOC CUSTOMER";
            }

            var FAgent = db.ForwardingAgentMasters.Where(cc => cc.StatusDefault == true).FirstOrDefault();
            if (FAgent != null)
            {
                vm.FAgentName = FAgent.FAgentName;
                vm.FAgentID = FAgent.FAgentID;
            }
            else
            {
                vm.FAgentID = 0;
                vm.FAgentName = "";
            }
           
            return View(vm);
        }

        public AWBBatchVM BindInscanDetail(AWBBatchVM vm)
        {
            QuickInscanMaster qvm = db.QuickInscanMasters.Find(vm.QuickInScanId);            
            vm.QuickInScanId = qvm.QuickInscanID;
            vm.PickedUpEmpID = Convert.ToInt32(qvm.CollectedByID);
            vm.DepotReceivedBy = Convert.ToInt32(qvm.ReceivedByID);
            if (qvm.CollectedDate == null)
            {
                vm.PickedupDate = qvm.QuickInscanDateTime;
            }
            else
            {
                vm.PickedupDate = qvm.CollectedDate;
            }
            vm.ReceivedDate = qvm.QuickInscanDateTime;
            //vm.rece = qvm.DriverName;
            
            vm.InScanSheetNo = qvm.InscanSheetNumber;
            vm.InscanVehicleId = Convert.ToInt32(qvm.VehicleId);
            return vm;
            
        }

        public AWBBatchVM BindDRSDetails(AWBBatchVM vm)
        {
            DR qvm = db.DRS.Find(vm.DRSId);
            
            vm.OutScanDeliveredID  = Convert.ToInt32(qvm.DeliveredBy);
            vm.OutscanVehicleId = Convert.ToInt32(qvm.VehicleID);
            //vm.rece = qvm.DriverName;
            vm.DRSNo = qvm.DRSNo;
            vm.OutScanDate = qvm.DRSDate;
            
            return vm;

        }

        public AWBBatchVM BindPODDetails(AWBBatchVM vm)
        {
            POD qvm = db.PODs.Find(vm.PODID);

            vm.DeliveredBy = Convert.ToInt32(qvm.DeliveredBy);
            vm.DeliveredDate = qvm.DeliveredDate;
            //vm.rece = qvm.DriverName;
                        
            return vm;

        }
        public ActionResult Edit(int id)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            AWBBatch v = db.AWBBatches.Find(id);
            AWBBatchVM vm = new AWBBatchVM();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.Vehicle = db.VehicleMasters.ToList();
            ViewBag.Title = "AWB Batch - Create";
            vm.ID = v.ID;
            vm.BatchNumber = v.BatchNumber;
            vm.BatchDate = v.BatchDate;
            var branch = db.BranchMasters.Find(branchid);
            var FAgent = db.ForwardingAgentMasters.Where(cc => cc.StatusDefault == true).FirstOrDefault();
            if (FAgent != null)
            {
                vm.FAgentName = FAgent.FAgentName;
                vm.FAgentID = FAgent.FAgentID;
            }
            else
            {
                vm.FAgentID = 0;
                vm.FAgentName = "";
            }
            if (branch != null)
            {
                vm.BranchLocation = branch.LocationName;
                vm.BranchCountry = branch.CountryName;
                vm.BranchCity = branch.CityName;
            }
            vm.TaxPercent = 5;
            var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultproducttype != null)
                vm.ProductTypeID = defaultproducttype.ProductTypeID;

            var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultmovementtype != null)
                vm.MovementID = defaultmovementtype.MovementID;

            var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            if (defaultparceltype != null)
                vm.ParcelTypeID = defaultparceltype.ID;

            string customername = "";


            customername = "WALK-IN-CUSTOMER";
            var CashCustomer = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CashCustomer != null)
            {
                vm.CASHCustomerId = CashCustomer.CustomerID;
                vm.CASHCustomerName = customername;
            }

            customername = "COD-CUSTOMER";
            var CODCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CODCustomer != null)
            {
                vm.CODCustomerID = CODCustomer.CustomerID;
                vm.CODCustomerName = "COD-CUSTOMER";
            }

            customername = "FOC CUSTOMER";
            var FOCCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (FOCCustomer != null)
            {
                vm.FOCCustomerID = FOCCustomer.CustomerID;
                vm.FOCCustomerName = "FOC CUSTOMER";
            }
            vm.ID = id;
            vm.Details = AWBDAO.GetBatchAWBInfo(id);
            return View(vm);
        }

        public JsonResult SaveBatch(AWBBatchVM model)
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            AWBBatch batch = new AWBBatch();
            try
            {
                if (model.ID == 0)
                {
                    
                    batch.BatchDate = Convert.ToDateTime(model.BatchDate);
                    string BatchNo = AWBDAO.GetMaxBathcNo(Convert.ToDateTime(batch.BatchDate),BranchId,FyearId); //batch no
                    batch.BatchNumber = BatchNo;
                    
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    batch.TotalAWB = 0;
                    if (model.CollectedBy && model.ReceivedBy)
                    {
                        //add quick inscan
                        int quickinscanid = SaveQuickInscan(model);
                        batch.QuickInScanId = quickinscanid;
                    }
                    //add outscan entry drs
                    if (model.OutScanDelivery)
                    {
                        int drsid = SaveOutScan(model);
                        batch.DRSId = drsid;
                    }
                    //add pod entry 
                    if (model.Delivered)
                    {
                        int podid = SavePOD(model);
                        batch.PODID = podid;
                    }
                    db.AWBBatches.Add(batch);
                    db.SaveChanges();


                    return Json(new { Status = "Ok",BatchID=batch.ID, message = "Batch Added Succesfully!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    batch = db.AWBBatches.Find(model.ID);
                    batch.BatchDate = model.BatchDate;
                    batch.ModifiedBy = userid;
                    batch.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                    if (model.CollectedBy && model.ReceivedBy)
                    {
                        //add quick inscan
                        int quickinscanid = SaveQuickInscan(model);
                        batch.QuickInScanId = quickinscanid;
                    }
                    //add outscan entry drs
                    if (model.OutScanDelivery)
                    {
                        int drsid = SaveOutScan(model);
                        batch.DRSId = drsid;
                    }
                    if (model.Delivered)
                    {
                        int podid = SavePOD(model);
                        batch.PODID = podid;
                    }
                    db.Entry(batch).State = EntityState.Modified;
                    db.SaveChanges();

                    string result = AWBDAO.SaveAWBBatchTrackStatus(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId);

                    return Json(new { Status = "Ok", BatchID = batch.ID, message = "Batch Update Succesfully!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                return Json(new { Status = "Failed", BatchID = model.ID, message = ex.Message  }, JsonRequestBehavior.AllowGet);
            }
        }

        public int SaveQuickInscan(AWBBatchVM model)
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            PickupRequestDAO _dao = new PickupRequestDAO();
            QuickInscanMaster _qinscan = new QuickInscanMaster();
            if (model.QuickInScanId > 0)
            {
                _qinscan = db.QuickInscanMasters.Find(model.QuickInScanId);
            }
            else
            {
                
                _qinscan.AcFinancialYearID = FyearId;
                 string inscansheetnumber = _dao.GetMaxInScanSheetNo(Convert.ToDateTime(model.ReceivedDate),CompanyID, BranchId, "Inhouse");
                _qinscan.InscanSheetNumber = inscansheetnumber;
                _qinscan.AcCompanyId = CompanyID;
                _qinscan.QuickInscanDateTime = Convert.ToDateTime(model.ReceivedDate);
            }
                        
            _qinscan.ReceivedByID = Convert.ToInt32(model.DepotReceivedBy);
            _qinscan.CollectedByID = Convert.ToInt32(model.PickedUpEmpID);
            _qinscan.QuickInscanDateTime =Convert.ToDateTime(model.ReceivedDate);
            _qinscan.CollectedDate  = Convert.ToDateTime(model.PickedupDate);
            _qinscan.VehicleId = model.InscanVehicleId;
            //_qinscan.DriverName = model.DriverName;
            _qinscan.BranchId = BranchId;
            _qinscan.DepotId = DepotID;
            _qinscan.UserId = userid;
            _qinscan.Source = "Inhouse";
            _qinscan.OutScanReturned = false;
            if (_qinscan.QuickInscanID > 0)
            {
                db.Entry(_qinscan).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                db.QuickInscanMasters.Add(_qinscan);
                db.SaveChanges();
            }

            return _qinscan.QuickInscanID;
        }
        public int SaveOutScan(AWBBatchVM model)
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DR objdrs = new DR();
            if (model.DRSId == 0 || model.DRSId==null)
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                objdrs.DRSDate = Convert.ToDateTime(model.OutScanDate);
                objdrs.DRSNo = _dao.GetMaxDRSNo(CompanyID, BranchId,objdrs.DRSDate);                
                objdrs.BranchID = BranchId;
                objdrs.AcCompanyID = CompanyID;
                objdrs.StatusDRS = "0";
                objdrs.StatusInbound = false;
                objdrs.DrsType = "Courier";
                objdrs.CreatedBy = userid;
                objdrs.FYearId = FyearId;
                objdrs.CreatedDate = CommonFunctions.GetCurrentDateTime();
                objdrs.TotalCourierCharge = 0;
                objdrs.TotalMaterialCost = 0;
                
                objdrs.DeliveredBy = model.OutScanDeliveredID;
                objdrs.VehicleID = model.OutscanVehicleId;
                objdrs.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                objdrs.ModifiedBy = userid;

            }
            else
            {
                objdrs = db.DRS.Find(model.DRSId);
                objdrs.DeliveredBy = model.OutScanDeliveredID;
                objdrs.VehicleID = model.OutscanVehicleId;
                objdrs.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                objdrs.ModifiedBy = userid;
            }

            if (model.DRSId == 0 || model.DRSId==null)
            {
                db.DRS.Add(objdrs);
                db.SaveChanges();
            }
            else
            {
                db.Entry(objdrs).State = EntityState.Modified;
                db.SaveChanges();
            }
            return objdrs.DRSID;

        }

       public int SavePOD(AWBBatchVM model)
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            POD pod = new POD();
            if (model.PODID == 0 || model.PODID==null)
            {
                int max = (from d in db.PODs orderby d.PODID descending select d.PODID).FirstOrDefault();
                if (max == null)
                    max = 1;
                else
                    max = max + 1;
                pod.PODID = max;
            }
            else
            {
                pod = db.PODs.Find(model.PODID);
            }
            pod.InScanID = 0;
            pod.ShipmentDetailId = 0;
            //pod.ReceiverName = item.ReceiverName;
            //pod.ReceivedTime = Convert.ToDateTime(item.ReceivedTime1); //  item.DeliveredDate;
            pod.DeliveredBy = model.DeliveredBy;
            pod.DeliveredDate = model.DeliveredDate;
            pod.UpdationDate = CommonFunctions.GetCurrentDateTime();
            pod.CourierStatusID = 13;
            pod.IsSkyLarkUpdate = false;
            pod.DeliveryLocation = "";
            pod.BatchId = model.ID;
            pod.BranchId = BranchId;
            pod.CreatedBy = userid;
            pod.CreatedDate = CommonFunctions.GetCurrentDateTime();
            if (model.PODID == 0 || model.PODID==null)
            {
                db.PODs.Add(pod);
                db.SaveChanges();
            }
            else
            {
                db.Entry(pod).State = EntityState.Modified;
                db.SaveChanges();
            }

            return pod.PODID;

        }

        [HttpPost]
        public JsonResult    SaveBatchAWB(int BatchID,  string Details)
        {
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBBatchDetail>>(Details);
                IDetails[0].SpecialInstructions = "";
                if (IDetails[0].SpecialInstructions1 != null)
                {
                    foreach (var item in IDetails[0].SpecialInstructions1)
                    {
                        if (IDetails[0].SpecialInstructions == "")
                        {
                            IDetails[0].SpecialInstructions = item.ToString();
                        }
                        else
                        {
                            IDetails[0].SpecialInstructions = IDetails[0].SpecialInstructions + "," + item.ToString();
                        }

                    }
                }
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
             
                string xml = dt.GetXml();
                xml = xml.Replace("T00:00:00+05:30", "");
                
                    int FyearId = Convert.ToInt32(Session["fyearid"]);
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                    AWBBatch batch = new AWBBatch();
                    batch = db.AWBBatches.Find(BatchID);

                    string result = AWBDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId, xml);
                    if (result== "Ok")
                {
                    return Json(new { Status = "Ok", message = "AWB Saved Succesfully!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Failed", message =result }, JsonRequestBehavior.AllowGet);
                }

                                       
            }
            catch (Exception ex)
            {
                //return ex.Message;
                return Json(new { Status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult ShowAWBList(int BatchId)
        {
            List<AWBBatchDetail> details = new List<AWBBatchDetail>();
            AWBBatchVM vm = new AWBBatchVM();
            details = AWBDAO.GetBatchAWBInfo(BatchId);
            vm.Details = details;
            return PartialView("AWBList", vm);
        }
        [HttpPost]
        public ActionResult ShowAWBDetail(int BatchId,int InscanId)
        {
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AWBBatchVM vm = new AWBBatchVM();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();            
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            var defaultproducttype = db.ProductTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            var defaultmovementtype = db.CourierMovements.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            var defaultparceltype = db.ParcelTypes.ToList().Where(cc => cc.DefaultType == true).FirstOrDefault();
            
            string customername = "";


            customername = "WALK-IN-CUSTOMER";
            var CashCustomer = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CashCustomer != null)
            {
                vm.CASHCustomerId = CashCustomer.CustomerID;
                vm.CASHCustomerName = customername;
            }

            customername = "COD-CUSTOMER";
            var CODCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (CODCustomer != null)
            {
                vm.CODCustomerID = CODCustomer.CustomerID;
                vm.CODCustomerName = "COD-CUSTOMER";
            }

            customername = "FOC CUSTOMER";
            var FOCCustomer = (from c1 in db.CustomerMasters
                               where c1.CustomerName == customername
                               orderby c1.CustomerName ascending
                               select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();
            if (FOCCustomer != null)
            {
                vm.FOCCustomerID = FOCCustomer.CustomerID;
                vm.FOCCustomerName = "FOC CUSTOMER";
            }

            var FAgent = db.ForwardingAgentMasters.Where(cc => cc.StatusDefault == true).FirstOrDefault();
            if (FAgent != null)
            {
                vm.FAgentName = FAgent.FAgentName;
                vm.FAgentID = FAgent.FAgentID;
            }
            else
            {
                vm.FAgentID = 0;
                vm.FAgentName = "";
            }
            List<AWBBatchDetail> details = new List<AWBBatchDetail>();
            if (InscanId > 0)
            {
                details = AWBDAO.GetBatchAWBDetail(BatchId, InscanId);
                vm.Details = details;
                if (details.Count > 0)
                {
                    vm.InScanID = details[0].InScanID;
                    vm.AWBNo = details[0].AWBNo;
                    vm.AWBDate = Convert.ToDateTime(details[0].AWBDate);
                    vm.MovementID = details[0].MovementID;
                    vm.ParcelTypeID = details[0].ParcelTypeId;
                    vm.ProductTypeID = details[0].ProductTypeID;
                    vm.PaymentTypeId = details[0].PaymentModeId;
                    vm.ItemDescription = details[0].CargoDescription;
                    vm.PickUpLocation = details[0].PickupLocation;
                    vm.DeliveryLocation = details[0].DeliveryLocation;
                    vm.Specialnstructions = details[0].SpecialInstructions;
                    vm.AssignedEmployeeID = details[0].AssignedEmployeeID;
                    vm.CustomerID = details[0].CustomerID;
                    vm.CustomerName = details[0].CustomerName;
                    vm.CourierCharge = details[0].CourierCharge;
                    vm.OtherCharge = details[0].OtherCharge;
                    vm.TaxAmount = details[0].TaxAmount;
                    vm.TaxPercent = details[0].TaxPercent;
                    vm.NetTotal = details[0].NetTotal;
                    vm.Weight = details[0].Weight;
                    vm.Pieces = Convert.ToInt32(details[0].Pieces);
                    vm.MaterialCost = details[0].MaterialCost;
                    vm.ConsignorCountryName = details[0].ConsignorCountryName;
                    vm.ConsignorCityName = details[0].ConsignorCityName;
                    vm.ConsigneeCountryName = details[0].ConsigneeCountryName;
                    vm.ConsigneeCityName = details[0].ConsigneeCityName;
                    vm.OriginCity = details[0].ConsignorCityName;
                    vm.OriginCountry = details[0].ConsignorCountryName;
                    vm.DeliveryCity = details[0].ConsigneeCityName;
                    vm.DeliveryCountry = details[0].ConsigneeCountryName;

                    vm.CustomerRateTypeID = details[0].CustomerRateTypeId;
                    vm.CustomerRateType = details[0].CustomerRateType;
                    vm.Shipper = details[0].Consignor;
                    vm.ConsignorContact = details[0].ConsignorContact;
                    vm.ConsignorAddress1_Building = details[0].ConsignorAddress1_Building;
                    vm.ConsignorAddress2_Street = details[0].ConsignorAddress2_Street;
                    vm.ConsignorAddress3_PinCode = details[0].ConsignorAddress3_PinCode;
                    vm.ConsignorPhone = details[0].ConsignorPhone;
                    vm.ConsignorMobileNo = details[0].ConsignorMobileNo;

                    vm.Consignee = details[0].Consignee;
                    vm.ConsigneeContact = details[0].ConsigneeContact;
                    vm.ConsigneeAddress1_Building = details[0].ConsigneeAddress1_Building;
                    vm.ConsigneeAddress2_Street = details[0].ConsigneeAddress2_Street;
                    vm.ConsigneeAddress3_PinCode = details[0].ConsigneeAddress3_PinCode;
                    vm.ConsigneePhone = details[0].ConsigneePhone;
                    vm.ConsigneeMobileNo = details[0].ConsigneeMobileNo;
                    vm.Remarks = details[0].Remarks;
                }
                vm.TaxPercent = 5;
                var branch = db.BranchMasters.Find(BranchId);
                if (branch != null)
                {
                    vm.BranchLocation = branch.LocationName;
                    vm.BranchCountry = branch.CountryName;
                    vm.BranchCity = branch.CityName;
                }
            }
            else
            {
                vm.InScanID = 0;
                vm.AWBNo = "";
                vm.AWBDate = CommonFunctions.GetCurrentDateTime();
                if (defaultproducttype != null)
                    vm.ProductTypeID = defaultproducttype.ProductTypeID;


                if (defaultmovementtype != null)
                    vm.MovementID = defaultmovementtype.MovementID;


                if (defaultparceltype != null)
                    vm.ParcelTypeID = defaultparceltype.ID;
                vm.TaxPercent = 5;
                var branch = db.BranchMasters.Find(BranchId);
                if (branch != null)
                {
                    vm.BranchLocation = branch.LocationName;
                    vm.BranchCountry = branch.CountryName;
                    vm.BranchCity = branch.CityName;
                }
            }
            return PartialView("AWBEntry", vm);
        }

        [HttpPost]
        public JsonResult DeleteAWB(int BatchId, int InscanId)
        {

            DataTable dt= AWBDAO.DeleteBatchAWB(BatchId, InscanId);
            string message = "";
            string status = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    status = dt.Rows[0][0].ToString();
                    message = dt.Rows[0][1].ToString();
                }

            }
            else
            {
                status = "Failed!";
                message = "Delete Failed!";
            }
            return Json(new { Status = status, Message = message }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public string SaveBatchold(string BatchNo, string BatchDate, string Details)
        {
            try
            {
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBBatchDetail>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                string xml = dt.GetXml();
                xml = xml.Replace("T00:00:00+05:30", "");
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());
                    AWBBatch batch = new AWBBatch();
                    batch.BatchNumber = BatchNo;
                    batch.BatchDate = Convert.ToDateTime(BatchDate);
                    batch.CreatedBy = userid;
                    batch.CreatedDate = CommonFunctions.GetCurrentDateTime();
                    batch.AcFinancialYearid = FyearId;
                    batch.BranchID = BranchId;
                    db.AWBBatches.Add(batch);
                    db.SaveChanges();

                    string result = AWBDAO.SaveAWBBatch(batch.ID, BranchId, CompanyID, DepotID, userid, FyearId, xml);

                    string result1 = AWBDAO.SaveAWBBatchPosting(batch.ID, BranchId, CompanyID, FyearId);
                    
                    return result;


                }
                else
                {
                    return "Failed!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        public string UpdateBatch(int BatchId, string Details)
        {
            try
            {
                Details.Replace("{}", "");
                Details.Replace("[object HTMLInputElement]", "");
                var IDetails = JsonConvert.DeserializeObject<List<AWBBatchDetail>>(Details);
                DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IDetails);
                int FyearId = Convert.ToInt32(Session["fyearid"]);
                string xml = dt.GetXml();
                xml = xml.Replace("T00:00:00+05:30", "");
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int DepotID = Convert.ToInt32(Session["CurrentDepotID"].ToString());

                    string result = AWBDAO.UpdateAWBBatch(BatchId, BranchId, CompanyID, DepotID, userid, FyearId, xml);
                    
                    string result1 = AWBDAO.SaveAWBBatchPosting(BatchId , BranchId, CompanyID, FyearId);

                    return result;
                }
                else
                {
                    return "Failed!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        public JsonResult GetConsignorCustomer(string customername)
        {
            var customerlist = (from c1 in db.CustomerMasters
                                where c1.CustomerName == customername
                                orderby c1.CustomerName ascending
                                select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName }).FirstOrDefault();

            return Json(customerlist, JsonRequestBehavior.AllowGet);

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
        public JsonResult GetShipperName(string term)
        {
            List<Consignor> shipperlist = (List<Consignor>)Session["ConsignorMaster"];
            //if (shipperlist == null)
            //{
            //    shipperlist = (from c1 in db.ConsignorMasters                               
            //                   orderby c1.ConsignorName ascending
            //                   select new Consignor { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorContactName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo =  c1.MobileNo }).Distinct().ToList();
            //    Session["ConsignorMaster"] = shipperlist;
            //}
            if (term.Trim() != "")
            {
                shipperlist = (from c1 in db.ConsignorMasters
                               where c1.ConsignorName.ToLower().Contains(term.Trim().ToLower())
                               orderby c1.ConsignorName ascending
                               select new Consignor { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorContactName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo = c1.MobileNo }).Distinct().OrderBy(cc=>cc.ShipperName).Take(20).ToList();
                //shipperlist = shipperlist.Where(cc => cc.ShipperName.ToLower().Contains(term.Trim().ToLower())).ToList();
                //if (shipperlist.Count > 100)
                //{
                //    shipperlist = shipperlist.Take(100).ToList();
                //}

                //var shipperlist = (from c1 in db.InScanMasters
                //                   where c1.IsDeleted == false && c1.Consignor.ToLower().StartsWith(term.ToLower())
                //                   orderby c1.Consignor ascending
                //                   select new { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();
                return Json(shipperlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //if (shipperlist == null)
                //{
                //    shipperlist = (from c1 in db.ConsignorMasters
                //                   where c1.ConsignorName.ToLower().StartsWith(term.ToLower())
                //                   orderby c1.ConsignorName ascending
                //                   select new Consignor { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo = "" }).Distinct().ToList();
                //}
                //var shipperlist = (from c1 in db.ConsignorMasters                                    
                //                    orderby c1.ConsignorName ascending
                //                    select new { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo = "" }).Distinct();

                //var shipperlist = (from c1 in db.InScanMasters
                //                   where c1.IsDeleted == false
                //                   orderby c1.Consignor ascending
                //                   select new { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();
                shipperlist = (from c1 in db.ConsignorMasters                               
                               orderby c1.ConsignorName ascending
                               select new Consignor { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorContactName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo = c1.MobileNo }).Distinct().OrderBy(cc=>cc.ShipperName).Take(20).ToList();
                if (shipperlist.Count > 100)
                {
                    shipperlist = shipperlist.Take(100).ToList();
                }
                else
                {
                    //shipperlist = shipperlist.Take(100).ToList();
                }
                return Json(shipperlist, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet]
        public JsonResult GetReceiverName(string term, string Shipper="",bool ShowAll=false)
        {
            if (term.Trim() != "")
            {
                if (ShowAll == false)
                {
                    var shipperlist = (from c1 in db.ConsigneeMasters
                                       where c1.ConsigneeName.ToLower().StartsWith(term.ToLower())
                                       && c1.ConsignorName.ToLower().StartsWith(Shipper.ToLower())
                                       orderby c1.ConsigneeName ascending
                                       select new { Name = c1.ConsigneeName, ContactPerson = c1.ConsigneeContactName, Phone = c1.ConsigneePhoneNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryname, Address1 = c1.ConsigneeAddress1, Address2 = c1.ConsigneeAddress2, PinCode = c1.ConsigneeAddress3, ConsigneeMobileNo = c1.MobileNo }).Take(20);

                    //var shipperlist = (from c1 in db.InScanMasters
                    //                   where c1.Consignee.ToLower().StartsWith(term.ToLower())
                    //                   && c1.Consignor.ToLower().StartsWith(Shipper.ToLower())
                    //                   orderby c1.Consignee ascending
                    //                   select new { Name = c1.Consignee, ContactPerson = c1.ConsigneeContact, Phone = c1.ConsigneePhone, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryName, Address1 = c1.ConsigneeAddress1_Building, Address2 = c1.ConsigneeAddress2_Street, PinCode = c1.ConsigneeAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo, ConsigneeMobileNo = c1.ConsigneeMobileNo }).Distinct();
                    //if (shipperlist.Count() > 100)
                    //{
                    //    shipperlist = shipperlist.Take(100);
                    //}
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.ConsigneeMasters
                                       where c1.ConsigneeName.ToLower().StartsWith(term.ToLower())                                       
                                       orderby c1.ConsigneeName ascending
                                       select new { Name = c1.ConsigneeName, ContactPerson = c1.ConsigneeContactName, Phone = c1.ConsigneePhoneNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryname, Address1 = c1.ConsigneeAddress1, Address2 = c1.ConsigneeAddress2, PinCode = c1.ConsigneeAddress3, ConsigneeMobileNo = c1.MobileNo }).Take(20);
                    //if (shipperlist.Count() >100)
                    //{
                    //    shipperlist = shipperlist.Take(100);
                    //}
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ShowAll == false)
                {
                    var shipperlist = (from c1 in db.ConsigneeMasters
                                       where
                                       c1.ConsignorName.ToLower().StartsWith(Shipper.ToLower())
                                       orderby c1.ConsigneeName ascending
                                       select new { Name = c1.ConsigneeName, ContactPerson = c1.ConsigneeContactName, Phone = c1.ConsigneePhoneNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryname, Address1 = c1.ConsigneeAddress1, Address2 = c1.ConsigneeAddress2, PinCode = c1.ConsigneeAddress3, ConsigneeMobileNo = c1.MobileNo }).Take(20);
                    // if (shipperlist.Count() > 100)
                    //{
                    //    shipperlist = shipperlist.Take(100);
                    //}
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var shipperlist = (from c1 in db.ConsigneeMasters                                       
                                       orderby c1.ConsigneeName ascending
                                       select new { Name = c1.ConsigneeName, ContactPerson = c1.ConsigneeContactName, Phone = c1.ConsigneePhoneNo, LocationName = c1.ConsigneeLocationName, CityName = c1.ConsigneeCityName, CountryName = c1.ConsigneeCountryname, Address1 = c1.ConsigneeAddress1, Address2 = c1.ConsigneeAddress2, PinCode = c1.ConsigneeAddress3, ConsigneeMobileNo = c1.MobileNo }).Take(100);
                    //if (shipperlist.Count() > 100)
                    //{
                    //    shipperlist = shipperlist.Take(100);
                    //}
                    return Json(shipperlist, JsonRequestBehavior.AllowGet);
                }

            }

        }

        [HttpGet]
        public JsonResult GetAWBInfo(string awbno)
        {
            AWBInfo info = AWBDAO.GetAWBInfo(awbno.Trim());

            return Json(info, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetAWBTrackStatus(string awbno)
        {
            AWBBatchDetail info = AWBDAO.GetAWBTrackStatus(awbno);

            return Json(info, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetAWBBatch(string term)
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in db.AWBBatches
                                    where c1.BatchNumber.Contains(term)
                                    orderby c1.BatchNumber descending
                                    select new { BatchId = c1.ID, BatchNo = c1.BatchNumber }).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in db.AWBBatches
                                    orderby c1.BatchNumber descending
                                    select new { BatchId = c1.ID, BatchNo = c1.BatchNumber }).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetBatchAWB(int id)
        {
            List<AWBBatchDetail> Details = AWBDAO.GetBatchAWBInfo(id);
            return Json(Details, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTypeofGoods(string term)
        {
            if (term.Trim() !="")
            {
                var list = db.TypeOfGoods.Where(cc => cc.TypeOfGood1.Contains(term.Trim())).OrderBy(cc => cc.TypeOfGood1).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = db.TypeOfGoods.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult ConsignorAddress()
        {
            AWBBatchVM vm = new AWBBatchVM();
            return View(vm);
        }

        [HttpPost]
        public ActionResult ShowConsignorAddres(AWBBatchVM model)
        {
            return PartialView("ConsignorAddress", model);
        }

        [HttpPost]
        public JsonResult SaveConsignorAddress(Consignor model)
        {
            bool newentry = false;
            ConsignorMaster obj = db.ConsignorMasters.Where(cc => cc.ConsignorName == model.ShipperName).FirstOrDefault();

            if (obj == null)
            {
                newentry = true;
                obj = new ConsignorMaster();
            }
            obj.ConsignorName = model.ShipperName;
            obj.ConsignorContactName = model.ContactPerson;
            obj.ConsignorPhoneNo = model.Phone;
            obj.ConsignorAddress1 = model.Address1;
            obj.ConsignorAddress2 = model.Address2;
            obj.ConsignorAddress3 = model.PinCode;
            obj.ConsignorPhoneNo = model.Phone;
            obj.MobileNo = model.ConsignorMobileNo;
            obj.ConsignorLocationName = model.LocationName;
            obj.ConsignorCountryname = model.CountryName;
            obj.ConsignorCityName = model.CityName;
            
            if (newentry==true)
            {
                db.ConsignorMasters.Add(obj);
                db.SaveChanges();
            }
            else
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveConsigeeAddress(Consignor model)
        {
            bool newentry = false;
            ConsigneeMaster obj = db.ConsigneeMasters.Where(cc => cc.ConsigneeName == model.ConsignorName && cc.ConsignorName==model.ShipperName ).FirstOrDefault();

            if (obj == null)
            {
                newentry = true;
                obj = new ConsigneeMaster();
            }
            obj.ConsignorName = model.ShipperName;
            obj.ConsigneeName = model.ConsignorName; //it will return consignee name only
            obj.ConsigneeContactName = model.ContactPerson;
            obj.ConsigneePhoneNo = model.Phone;
            obj.ConsigneeAddress1 = model.Address1;
            obj.ConsigneeAddress2 = model.Address2;
            obj.ConsigneeAddress3 = model.PinCode;
            obj.ConsigneePhoneNo = model.Phone;
            obj.MobileNo = model.ConsignorMobileNo;
            obj.ConsigneeLocationName = model.LocationName;
            obj.ConsigneeCountryname = model.CountryName;
            obj.ConsigneeCityName = model.CityName;

            if (newentry == true)
            {
                db.ConsigneeMasters.Add(obj);
                db.SaveChanges();
            }
            else
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ShowConsigneeAddres(AWBBatchVM model)
        {
            return PartialView("ConsigneeAddress", model);
        }
        [HttpPost]
        public JsonResult GetCourierChargeDRSReconc(int CustomerId, int InscanID)
        {
            int pRateTypeID = 0;
            int pCustomerId = 0;
            int pMovementId = 1;
            int pProductTypeID = 0;
            int pPaymentModeId = 0;
            int pAgentID = 1;
            decimal pWeight = 0;
            var _inscan = db.InScanMasters.Find(InscanID);
            pProductTypeID = Convert.ToInt32(_inscan.ProductTypeID);

            //if (CustomerRateTypeID != "")
            //{
            //    pRateTypeID = Convert.ToInt32(CustomerRateTypeID);
            //}
            
            // pCustomerId = Convert.ToInt32(CustomerId);
             pMovementId = Convert.ToInt32(_inscan.MovementID);
              pPaymentModeId = 3;

            
             pWeight = Convert.ToDecimal(_inscan.Weight);
            List<CustomerRateType> lst = new List<CustomerRateType>();
            var loc = AWBDAO.GetRateList(CustomerId, pMovementId, pProductTypeID, pPaymentModeId, pAgentID,  _inscan.ConsigneeCityName,_inscan.ConsigneeCountryName);
            if (loc.Count > 0)
            {
                pRateTypeID = loc[0].CustomerRateTypeID;
                CustomerRateTypeVM vm = AWBDAO.GetCourierCharge(pRateTypeID, CustomerId, pMovementId, pProductTypeID, pPaymentModeId, pWeight, _inscan.ConsigneeCountryName, _inscan.ConsigneeCityName);
                
                return Json(new { status = "OK", data = vm }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new {status = "Failed"}, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCustomerRateType(string term, string CustomerId,string MovementId,string ProductTypeID,string PaymentModeId,string FAgentID,string CityName,string CountryName,string OriginCountry,string OriginCity,string Weight)
        {
                    int pCustomerId = 0;
            int pMovementId = 0;
            int pProductTypeID = 0;
            int pPaymentModeId = 0;
            int pAgentID = 0;
            decimal pWeight = 0;
            string pCountryname = ""; ;
            string pcityname = "";

            if (Weight != "")
                pWeight = Convert.ToDecimal(Weight);

            if (CustomerId != "")
                pCustomerId = Convert.ToInt32(CustomerId);

            if (MovementId != "")
                pMovementId = Convert.ToInt32(MovementId);

            if (ProductTypeID != "")
                pProductTypeID = Convert.ToInt32(ProductTypeID);

            if (PaymentModeId != "")
                pPaymentModeId = Convert.ToInt32(PaymentModeId);

            if (FAgentID != "")
                pAgentID = Convert.ToInt32(FAgentID);

            if (pMovementId == 3)
            {
                pCountryname = OriginCountry;
                pcityname = OriginCity;
            }
            else
            {
                pCountryname = CountryName;
                pcityname = CityName;

            }
            if (pAgentID >0)
            {
                List<CustomerRateType> lst = new List<CustomerRateType>();
                var loc = AWBDAO.GetRateList(pCustomerId, pMovementId, pProductTypeID, pPaymentModeId, pAgentID, pcityname, pCountryname);
                var frate = new FAgentRate();
                if (MovementId != "1")
                {
                     frate = AWBDAO.GetFAgentRate(pMovementId, pProductTypeID, pAgentID, pCountryname, pWeight);
                }
                    if (term.Trim() != "")
                    {
                        lst = (from c in loc where c.CustomerRateType1.Contains(term) orderby c.CustomerRateType1 select c).ToList();
                    }
                    else
                    {
                        lst = (from c in loc orderby c.CustomerRateType1 select c).ToList();
                    }
                
                return Json(new { Customerdata = lst, FRate = frate }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<CustomerRateType> lst = new List<CustomerRateType>();
                return Json(new { Customerdata = lst, FRate = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public JsonResult GetCourierCharge(string CustomerRateTypeID, string CustomerId, string MovementId, string ProductTypeID, string PaymentModeId,string Weight,string CountryName,string CityName)
        {
            int pRateTypeID = 0;
            int pCustomerId = 0;
            int pMovementId = 0;
            int pProductTypeID = 0;
            int pPaymentModeId = 0;
            decimal pWeight = 0;
            if (CustomerRateTypeID!="")
            {
                pRateTypeID = Convert.ToInt32(CustomerRateTypeID);
            }
            if (CustomerId != "")
                pCustomerId = Convert.ToInt32(CustomerId);

            if (MovementId != "")
                pMovementId = Convert.ToInt32(MovementId);

            if (ProductTypeID != "")
                pProductTypeID = Convert.ToInt32(ProductTypeID);

            if (PaymentModeId != "")
                pPaymentModeId = Convert.ToInt32(PaymentModeId);

            if (Weight != "")
                pWeight = Convert.ToDecimal(Weight);

            CustomerRateTypeVM vm = AWBDAO.GetCourierCharge(pRateTypeID,pCustomerId, pMovementId, pProductTypeID, pPaymentModeId,pWeight,CountryName,CityName);
                        
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetParcelType(string ProductTypeId)
        {
            int pProductTypeID = 0;

            if (ProductTypeId != "")
                pProductTypeID = Convert.ToInt32(ProductTypeId);

            int ParcelTypeId = 0;
            var prod = db.ProductTypes.Find(ProductTypeId);
            if (prod!=null)
            {
                ParcelTypeId=prod.ParcelTypeID;
            }

            return Json(ParcelTypeId, JsonRequestBehavior.AllowGet);

        }

      
        public JsonResult GetForwardingAgent(string term)
        {
            if (term.Trim() != "")
            {
                var list = (from c in db.ForwardingAgentMasters where c.FAgentName.Contains(term.Trim()) orderby c.FAgentName select new { FAgentID = c.FAgentID, AgentName = c.FAgentName }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c in db.ForwardingAgentMasters orderby c.FAgentName select new { FAgentID = c.FAgentID, AgentName = c.FAgentName }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetCustomerRateDetail(int id)
        {
            string ratename = AWBDAO.GetCustomerRateName(id);
            return Json(ratename, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFAgentName(int id)
        {
            var list = (from c in db.ForwardingAgentMasters where c.FAgentID == id orderby c.FAgentName select new { FAgentID = c.FAgentID, AgentName = c.FAgentName }).FirstOrDefault();
            if (list != null)
            {
                return Json(list.AgentName, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CheckShipperName(string term)
        {
            List<Consignor> shipperlist = (List<Consignor>)Session["ConsignorMaster"];
            if (shipperlist == null)
            {
                shipperlist = (from c1 in db.ConsignorMasters
                               orderby c1.ConsignorName ascending
                               select new Consignor { ShipperName = c1.ConsignorName, ContactPerson = c1.ConsignorName, Phone = c1.ConsignorPhoneNo, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryname, Address1 = c1.ConsignorAddress1, Address2 = c1.ConsignorAddress2, PinCode = c1.ConsignorAddress3, ConsignorMobileNo = c1.MobileNo }).Distinct().ToList();
                Session["ConsignorMaster"] = shipperlist;
            }
            if (term.Trim() != "")
            {

                var shipper = shipperlist.Where(cc => cc.ShipperName.ToLower()==term.Trim().ToLower()).FirstOrDefault();

                //var shipperlist = (from c1 in db.InScanMasters
                //                   where c1.IsDeleted == false && c1.Consignor.ToLower().StartsWith(term.ToLower())
                //                   orderby c1.Consignor ascending
                //                   select new { ShipperName = c1.Consignor, ContactPerson = c1.ConsignorContact, Phone = c1.ConsignorPhone, LocationName = c1.ConsignorLocationName, CityName = c1.ConsignorCityName, CountryName = c1.ConsignorCountryName, Address1 = c1.ConsignorAddress1_Building, Address2 = c1.ConsignorAddress2_Street, PinCode = c1.ConsignorAddress3_PinCode, ConsignorMobileNo = c1.ConsignorMobileNo }).Distinct();
                if (shipper!=null)
                    return Json("Exists", JsonRequestBehavior.AllowGet);
                else
                    return Json("NotExists", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            
          
        }

    }
}

