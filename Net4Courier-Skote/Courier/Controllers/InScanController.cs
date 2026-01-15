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
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class InScanController : Controller
    {
        Entities1 db = new Entities1();
        public ActionResult Index() 
        {

            InScanSearch obj = (InScanSearch)Session["InScanSearch"];
            InScanSearch model = new InScanSearch();
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
                obj = new InScanSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;                
                Session["InScanSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;                
            }
            else
            {
                model = obj;
            }
            List<InScanVM> lst = PickupRequestDAO.GetInScanList(obj.FromDate, obj.ToDate,yearid,branchid,depotId);
            model.Details = lst;

            return View(model);


        }
        [HttpPost]
        public ActionResult Index(InScanSearch obj)
        {
            Session["InScanSearch"] = obj;
            return RedirectToAction("Index");
        }
        public ActionResult Index1(string FromDate, string ToDate)
        {
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            DateTime pFromDate;
            DateTime pToDate;
      
            if (FromDate == null || ToDate == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;// DateTime.Now.Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date.AddDays(1); // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
            }
            else
            {
                pFromDate = Convert.ToDateTime(FromDate);//.AddDays(-1);
                pToDate = Convert.ToDateTime(ToDate).Date.AddDays(1);

            }

            // List<PickupRequestVM> lst = (from c in db.CustomerEnquiries join t1 in db.EmployeeMasters on c.CollectedEmpID equals t1.EmployeeID join t2 in db.EmployeeMasters on c.EmployeeID equals t2.EmployeeID select new PickupRequestVM { EnquiryID = c.EnquiryID, EnquiryDate = c.EnquiryDate, Consignor = c.Consignor, Consignee = c.Consignee, eCollectedBy = t1.EmployeeName, eAssignedTo = t2.EmployeeName,AWBNo=c.AWBNo }).ToList();

            //List<PickupRequestVM> lst = (from c in db.CustomerEnquiries
            //            join status in db.PickUpRequestStatus on c.PickupRequestStatusId equals status.Id
            //            join pet in db.EmployeeMasters on c.CollectedEmpID equals pet.EmployeeID into gj
            //            from subpet in gj.DefaultIfEmpty()
            //            join pet1 in db.EmployeeMasters on c.EmployeeID equals  pet1.EmployeeID into gj1
            //            from subpet1 in gj1.DefaultIfEmpty()
            //            where  c.EnquiryDate >=pFromDate &&  c.EnquiryDate <=pToDate
            //            select new PickupRequestVM { EnquiryID = c.EnquiryID, EnquiryNo=c.EnquiryNo, EnquiryDate = c.EnquiryDate, Consignor = c.Consignor, Consignee = c.Consignee, eCollectedBy =subpet.EmployeeName ?? string.Empty, eAssignedTo = subpet1.EmployeeName ?? string.Empty , AWBNo = c.AWBNo ,PickupRequestStatus=status.PickRequestStatus }).ToList();

            int Customerid = 0;
            if (Session["UserType"].ToString() == "Customer")
            {

                Customerid = Convert.ToInt32(Session["CustomerId"].ToString());

            }
            List<InScanVM> lst = (from c in db.QuickInscanMasters
                                         //join status in db.CourierStatus on c.CourierStatusID equals status.CourierStatusID
                                         join pet in db.EmployeeMasters on c.CollectedByID equals pet.EmployeeID into gj
                                         from subpet in gj.DefaultIfEmpty()
                                         join pet1 in db.EmployeeMasters on c.ReceivedByID equals pet1.EmployeeID into gj1
                                         from subpet1 in gj1.DefaultIfEmpty()
                                         where c.BranchId == branchid && (c.QuickInscanDateTime >= pFromDate && c.QuickInscanDateTime < pToDate)
                                         && c.DepotId==depotId
                                         //&& (c.CourierStatusID == pStatusId || pStatusId == 0)
                                         //&& c.IsDeleted == false
                                         //&& (c.CustomerID == Customerid || Customerid == 0)
                                         && c.Source == "Inhouse"
                                        orderby c.QuickInscanDateTime descending
                                         select new InScanVM{ QuickInscanID=c.QuickInscanID,InScanSheetNo=c.InscanSheetNumber,QuickInscanDateTime=c.QuickInscanDateTime, CollectedBy = subpet.EmployeeName ,ReceivedBy=subpet1.EmployeeName , DriverName=c.DriverName }).ToList();

            //ViewBag.FromDate = pFromDate.Date.AddDays(1).ToString("dd-MM-yyyy");
            ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
            ViewBag.ToDate = pToDate.Date.AddDays(-1).ToString("dd-MM-yyyy");
            ViewBag.PickupRequestStatus = db.CourierStatus.Where(cc => cc.StatusTypeID == 1).ToList();
           
            return View(lst);
        }
             

        public ActionResult Details(int id)
        {
            return View();
        }      

        public ActionResult Create(int id=0)
        {
            int BranchId= Convert.ToInt32( Session["CurrentBranchID"].ToString());
            int depotid = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyid= Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            //ViewBag.depot = db.tblDepots.ToList();
            ViewBag.depot = (from c in db.tblDepots where c.BranchID == BranchId select c).ToList();
            ViewBag.employee = db.EmployeeMasters.OrderBy(cc=>cc.EmployeeName).ToList();
            ViewBag.employeerec = db.EmployeeMasters.OrderBy(cc=>cc.EmployeeName).ToList();
            ViewBag.Vehicles = (from c in db.VehicleMasters select new { VehicleID = c.VehicleID, VehicleName = c.RegistrationNo + "-" + c.VehicleDescription }).ToList();
            ViewBag.CourierService = db.CourierServices.ToList();
            if (id==0)
            {
                InScanVM vm = new InScanVM();                
                vm.QuickInscanID = 0;
                
                PickupRequestDAO _dao = new PickupRequestDAO();
                vm.InScanSheetNo = _dao.GetMaxInScanSheetNo(CommonFunctions.GetCurrentDateTime(),companyid, BranchId, "Inhouse");
                vm.QuickInscanDateTime = CommonFunctions.GetCurrentDateTime(); // CommonFunctions.GetLastDayofMonth();
                vm.CollectedDate = CommonFunctions.GetCurrentDateTime(); // CommonFunctions.GetLastDayofMonth();
                vm.DepotID = depotid;
                ViewBag.EditMode ="false";
                ViewBag.Title = "Depot InScan - Create";
                return View(vm);                
            }
            else
            {
                QuickInscanMaster qvm = db.QuickInscanMasters.Find(id);
                InScanVM vm = new InScanVM();
                vm.QuickInscanID = qvm.QuickInscanID;
                vm.CollectedByID = Convert.ToInt32(qvm.CollectedByID);
                vm.ReceivedByID = Convert.ToInt32(qvm.ReceivedByID);
                vm.DriverName = qvm.DriverName;
                vm.QuickInscanDateTime = qvm.QuickInscanDateTime;
                if (qvm.CollectedDate!=null)
                    vm.CollectedDate = Convert.ToDateTime(qvm.CollectedDate);
                else
                {
                    vm.CollectedDate = qvm.QuickInscanDateTime;
                }
                vm.InScanSheetNo = qvm.InscanSheetNumber;
                vm.VehicleId = Convert.ToInt32(qvm.VehicleId);
                vm.DepotID = Convert.ToInt32(qvm.DepotId);
                vm.BranchId = Convert.ToInt32(qvm.BranchId);
                ViewBag.EditMode = "true";
                ViewBag.Title = "Depot InScan - Modify";
                return View(vm);
            }
            
        }
        
        [HttpPost]
        public JsonResult SaveQuickInScan(InScanVM v)
        {
            var IDetails = JsonConvert.DeserializeObject<List<AWBList>>(v.Details);
            //InScan inscan = new InScan();
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            //var inscanitems = v.SelectedInScanId.Split(',');
            try
            {
                QuickInscanMaster _qinscan = new QuickInscanMaster();
                if (v.QuickInscanID > 0)
                {
                    _qinscan = db.QuickInscanMasters.Find(v.QuickInscanID);
                }
                else
                {
                    //int? maxid = (from c in db.QuickInscanMasters orderby c.QuickInscanID descending select c.QuickInscanID).FirstOrDefault();

                    //if (maxid == null)
                    //    _qinscan.QuickInscanID = 1;
                    //else
                    //    _qinscan.QuickInscanID = Convert.ToInt32(maxid) + 1;
                    
                    _qinscan.AcFinancialYearID = yearid;                    

                }

                _qinscan.InscanSheetNumber = v.InScanSheetNo;
                _qinscan.AcCompanyId = CompanyId;
                _qinscan.ReceivedByID = v.ReceivedByID;
                _qinscan.CollectedByID = v.CollectedByID;
                _qinscan.QuickInscanDateTime = v.QuickInscanDateTime;
                _qinscan.CollectedDate = v.CollectedDate;
                _qinscan.VehicleId = v.VehicleId;
                _qinscan.DriverName = v.DriverName;
                _qinscan.BranchId = BranchId;
                _qinscan.DepotId = v.DepotID;
                _qinscan.UserId = UserId;
                _qinscan.Source = "Inhouse";
                _qinscan.OutScanReturned = false;
                if (v.QuickInscanID > 0)
                {
                    db.Entry(_qinscan).State= EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    db.QuickInscanMasters.Add(_qinscan);
                    db.SaveChanges();
                }
               // if (v.QuickInscanID == 0)
            //    {
                    foreach (var item in IDetails)
                    {
                        int _inscanid = Convert.ToInt32(item.InScanId);
                    int _shipmentdetailId = Convert.ToInt32(item.ShipmentDetailId);                      
                        item.AWB =item.AWB.Trim(); 
                    //For domesitc existing inscan items
                    if (_inscanid > 0)
                        {
                            InScanMaster _inscan = db.InScanMasters.Find(_inscanid);
                            _inscan.QuickInscanID = _qinscan.QuickInscanID;
                            _inscan.PickedUpEmpID = v.CollectedByID;
                            _inscan.DepotReceivedBy = v.ReceivedByID;
                            _inscan.DRSID = null;
                        _inscan.StatusTypeId = 2;// db.tblStatusTypes.Where(cc => cc.Name == "Depot Inscan").First().ID;
                            _inscan.CourierStatusID = 5;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Received At Origin Facility").FirstOrDefault().CourierStatusID;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();

                        //For only OutScan Returning Items
                        if (item.CourierStatusId==8 || item.CourierStatusId==9 || item.CourierStatusId==10)
                            {
                            OutScanReturn oreturn = new OutScanReturn();
                            oreturn.AWBNo = item.AWB;
                            oreturn.InScanID = item.InScanId;
                            oreturn.PrevCourierStatusId = item.CourierStatusId;
                            oreturn.PrevDRSID = item.DRSID;
                            oreturn.QuickInScanId = _qinscan.QuickInscanID;
                            oreturn.CreatedBy = UserId;
                            oreturn.CreatedDate = CommonFunctions.GetCurrentDateTime();
                            db.OutScanReturns.Add(oreturn);
                            db.SaveChanges();
                        }

                        AWBTrackStatu _awbstatus = new AWBTrackStatu();

                        var StatusTypeId = 1;// db.tblStatusTypes.Where(cc => cc.Name == "Pickup Request").First().ID;
                        var CourierStatusID = 2;// db.CourierStatus.Where(cc => cc.StatusTypeID == StatusTypeId && cc.CourierStatus == "Assigned For Collection").FirstOrDefault().CourierStatusID;
                        var awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == CourierStatusID).FirstOrDefault();
                            if (awb == null)
                            {
                                _awbstatus.AWBNo = _inscan.AWBNo;
                                _awbstatus.EntryDate =CommonFunctions.GetCurrentDateTime();// v.QuickInscanDateTime;
                                _awbstatus.InScanId = _inscan.InScanID;
                                _awbstatus.StatusTypeId = StatusTypeId;// Convert.ToInt32(_inscan.StatusTypeId);
                                _awbstatus.CourierStatusId = CourierStatusID;// Convert.ToInt32(_inscan.CourierStatusID);

                                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                                _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusID).CourierStatus;
                                _awbstatus.UserId = UserId;
                                _awbstatus.EmpID = v.CollectedByID;                                
                                db.AWBTrackStatus.Add(_awbstatus);
                                db.SaveChanges();

                            }

                            _awbstatus = new AWBTrackStatu();
                            StatusTypeId = db.tblStatusTypes.Where(cc => cc.Name == "Pickup Request").First().ID;
                            CourierStatusID = db.CourierStatus.Where(cc => cc.StatusTypeID == StatusTypeId && cc.CourierStatus == "Shipment Collected").FirstOrDefault().CourierStatusID;
                            awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == CourierStatusID).FirstOrDefault();
                            if (awb == null)
                            {
                                _awbstatus.AWBNo = _inscan.AWBNo;
                            _awbstatus.EntryDate = Convert.ToDateTime(_qinscan.CollectedDate); // CommonFunctions.GetCurrentDateTime();  // v.QuickInscanDateTime;
                                _awbstatus.InScanId = _inscan.InScanID;
                                _awbstatus.StatusTypeId = StatusTypeId;// Convert.ToInt32(_inscan.StatusTypeId);
                                _awbstatus.CourierStatusId = CourierStatusID;// Convert.ToInt32(_inscan.CourierStatusID);

                                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                                _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusID).CourierStatus;
                                _awbstatus.UserId = UserId;
                                _awbstatus.EmpID = v.CollectedByID;
                            
                            db.AWBTrackStatus.Add(_awbstatus);
                                db.SaveChanges();
                            }
                            awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == 5).FirstOrDefault();
                            if (awb == null)
                            {
                                _awbstatus.AWBNo = _inscan.AWBNo;
                            _awbstatus.EntryDate = _qinscan.QuickInscanDateTime; //  CommonFunctions.GetCurrentDateTime(); ;// v.QuickInscanDateTime;
                                _awbstatus.InScanId = _inscan.InScanID;
                                _awbstatus.StatusTypeId = 2;// Convert.ToInt32(_inscan.StatusTypeId);
                                _awbstatus.CourierStatusId = 5;// Convert.ToInt32(_inscan.CourierStatusID);
                                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_awbstatus.StatusTypeId).Name;
                                _awbstatus.CourierStatus = db.CourierStatus.Find(_awbstatus.CourierStatusId).CourierStatus;
                                _awbstatus.UserId = UserId;
                                _awbstatus.EmpID = v.ReceivedByID;
                            
                            db.AWBTrackStatus.Add(_awbstatus);
                                db.SaveChanges();
                            }
                        } 
                    else if (_shipmentdetailId > 0)  //For Imported existing items which are outscan returned
                    {
                        InboundShipment _inscan = db.InboundShipments.Find(_shipmentdetailId);

                        int importcourierstatusid = Convert.ToInt32(_inscan.CourierStatusID);

                        _inscan.QuickInscanID = _qinscan.QuickInscanID;
                        _inscan.StatusTypeId = 2;//Depot Inscan
                        _inscan.CourierStatusID = 22; //Outscan Returned
                        _inscan.DRSID = null;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();

                        if (importcourierstatusid == 8 || importcourierstatusid==9 || importcourierstatusid==9)
                        {
                            //Insert record in outscan return table
                            OutScanReturn oreturn = new OutScanReturn();
                            oreturn.AWBNo = item.AWB;
                            oreturn.ShipmentDetailID = item.ShipmentDetailId;
                            oreturn.PrevCourierStatusId = item.CourierStatusId;
                            oreturn.PrevDRSID = item.DRSID;
                            oreturn.QuickInScanId = _qinscan.QuickInscanID;
                            oreturn.CreatedBy = UserId;

                            oreturn.CreatedDate = CommonFunctions.GetCurrentDateTime();
                            db.OutScanReturns.Add(oreturn);
                            db.SaveChanges();
                        }
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();

                        var StatusTypeId = 2;
                        var CourierStatusID = 22;
                        var awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == CourierStatusID).FirstOrDefault();
                        
                        _awbstatus.AWBNo = _inscan.AWBNo;
                        string companyname = Session["CompanyName"].ToString();
                            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                                _awbstatus.EntryDate = DateTime.UtcNow; // DateTime.Now;
                            else
                             _awbstatus.EntryDate = v.QuickInscanDateTime;// DateTime.UtcNow;

                          //_awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime(); // DateTime.UtcNow;// v.QuickInscanDateTime;
                            _awbstatus.InScanId = 0;
                            _awbstatus.InboundShipmentID = _inscan.ShipmentID;
                            _awbstatus.StatusTypeId = StatusTypeId;// Convert.ToInt32(_inscan.StatusTypeId);
                            _awbstatus.CourierStatusId = CourierStatusID;// Convert.ToInt32(_inscan.CourierStatusID);

                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusID).CourierStatus;
                            _awbstatus.UserId = UserId;
                            _awbstatus.EmpID = v.CollectedByID;
                            _awbstatus.APIStatus = false;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                                               
                       
                    }
                    else if(_inscanid==0 && _shipmentdetailId==0) //For new AWB 
                    {
                            InScanMaster _inscan = new InScanMaster();
                            _inscan.AWBNo = item.AWB.Trim();
                            _inscan.QuickInscanID = _qinscan.QuickInscanID;
                            _inscan.PickedUpEmpID = v.CollectedByID;
                            _inscan.DepotReceivedBy = v.ReceivedByID;
                            _inscan.TransactionDate = v.QuickInscanDateTime;
                            _inscan.BranchID = BranchId;
                            _inscan.DepotID = v.DepotID;
                            _inscan.AcCompanyID = CompanyId;
                            _inscan.AcFinancialYearID = yearid;
                            _inscan.VehicleTypeId = v.VehicleId;
                            _inscan.StatusTypeId = 2; // db.tblStatusTypes.Where(cc => cc.Name == "INSCAN").First().ID;
                            _inscan.CourierStatusID = 5;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Received at Origin Facility").FirstOrDefault().CourierStatusID;
                            _inscan.CreatedBy = UserId;
                            _inscan.CreatedDate = v.QuickInscanDateTime;
                            _inscan.LastModifiedBy = UserId;
                            _inscan.LastModifiedDate = v.QuickInscanDateTime;
                            _inscan.IsDeleted = false;
                            _inscan.AWBProcessed = false;
                            db.InScanMasters.Add(_inscan);
                            db.SaveChanges();

                            //Update AWBDetail
                            var awbdetail = db.AWBDetails.Where(cc => cc.AWBNo == item.AWB).FirstOrDefault();
                            if (awbdetail!=null)
                            {
                                awbdetail.InScanID = _inscan.InScanID;
                                db.Entry(awbdetail).State = EntityState.Modified;
                                db.SaveChanges();
                            } 
                            //updating awbstaus table for tracking
                            //_awbstatus.AWBTrackStatusId = Convert.ToInt32(id);
                            AWBTrackStatu _awbstatus = new AWBTrackStatu();
                            var StatusTypeId = db.tblStatusTypes.Where(cc => cc.Name == "Pickup Request").First().ID;
                            var CourierStatusID = db.CourierStatus.Where(cc => cc.StatusTypeID == StatusTypeId && cc.CourierStatus == "Assigned For Collection").FirstOrDefault().CourierStatusID;
                            var awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == CourierStatusID).FirstOrDefault();
                            if (awb == null)
                            {
                                //Assigned For Collection
                                _awbstatus.AWBNo = _inscan.AWBNo;
                                _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();// v.QuickInscanDateTime;
                                _awbstatus.InScanId = _inscan.InScanID;
                                _awbstatus.StatusTypeId = StatusTypeId;// Convert.ToInt32(_inscan.StatusTypeId);
                                _awbstatus.CourierStatusId = CourierStatusID;// Convert.ToInt32(_inscan.CourierStatusID);

                                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                                _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusID).CourierStatus;
                                _awbstatus.UserId = UserId;
                                _awbstatus.EmpID = v.CollectedByID;
                            
                            db.AWBTrackStatus.Add(_awbstatus);
                                db.SaveChanges();

                            }
                            //Shipment Collected Status Update
                            CourierStatusID = db.CourierStatus.Where(cc => cc.StatusTypeID == StatusTypeId && cc.CourierStatus == "Shipment Collected").FirstOrDefault().CourierStatusID;
                            awb = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == CourierStatusID).FirstOrDefault();
                            if (awb == null)
                            {
                                _awbstatus.AWBNo = _inscan.AWBNo;
                                _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();// v.QuickInscanDateTime;
                                _awbstatus.InScanId = _inscan.InScanID;
                                _awbstatus.StatusTypeId = StatusTypeId;// Convert.ToInt32(_inscan.StatusTypeId);
                                _awbstatus.CourierStatusId = CourierStatusID;// Convert.ToInt32(_inscan.CourierStatusID);

                                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(StatusTypeId).Name;
                                _awbstatus.CourierStatus = db.CourierStatus.Find(CourierStatusID).CourierStatus;
                                _awbstatus.UserId = UserId;
                                _awbstatus.EmpID = v.CollectedByID;
                            
                            db.AWBTrackStatus.Add(_awbstatus);
                                db.SaveChanges();
                            }
                            //Received At Origin Facility
                        _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = _inscan.AWBNo;
                            _awbstatus.EntryDate = CommonFunctions.GetCurrentDateTime();//  v.QuickInscanDateTime;
                            _awbstatus.InScanId = _inscan.InScanID;
                            _awbstatus.StatusTypeId = 2;// Convert.ToInt32(_inscan.StatusTypeId);
                            _awbstatus.CourierStatusId = 5;// Convert.ToInt32(_inscan.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                            _awbstatus.UserId = UserId;
                            _awbstatus.EmpID = v.ReceivedByID;
                        
                        db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                        }
                    }
                //  }
                if (v.QuickInscanID == 0)
                {
                    AWBDAO.GenerateAWBJobCode(v.QuickInscanDateTime);
                    AWBDAO.GenerateBatchforInscan(v.QuickInscanDateTime, _qinscan.QuickInscanID);
                }
                //TempData["SuccessMsg"] = "You have successfully Saved InScan Items.";             

                 return Json(new { status = "ok", message = "You have successfully Saved InScan Items.!" } , JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
                //return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult GetAWB(int id)
        {

            List<AWBList> obj = new List<AWBList>();

            QuickInscanMaster _qinscanvm = db.QuickInscanMasters.Where(cc => cc.QuickInscanID == id).FirstOrDefault();
            
                       

            //InScanVM _qinscanvm = new InScanVM();

            //_qinscanvm = (from _qmaster in db.QuickInscanMasters
            //              where _qmaster.InscanSheetNumber == id
            //              select new InScanVM { QuickInscanID=_qmaster.QuickInscanID,QuickInscanDateTime=_qmaster.QuickInscanDateTime,BranchId=_qmaster.BranchId,DepotID=_qmaster.DepotId,VehicleId=_qmaster.VehicleId,DriverName = _qmaster.DriverName });.first

            if (_qinscanvm != null)
            {
                //obj = (from _qmaster in db.QuickInscanMasters
                //            join _inscan in db.InScanMasters on _qmaster.QuickInscanID equals _inscan.QuickInscanID
                //            //join _inscan in db.InScanMasters on _qdetail.InScanId equals _inscan.InScanID
                //            where _qmaster.QuickInscanID == id
                //            orderby _inscan.AWBNo descending
                //            select new AWBList { InScanId = _inscan.InScanID, AWB = _inscan.AWBNo, Origin = _inscan.ConsignorCountryName, Destination = _inscan.ConsigneeCountryName }).ToList();
                obj = PickupRequestDAO.GetInScannedItems(id);
                return Json(new { status = "ok", masterdata=_qinscanvm, data = obj, message = "Data Found" }, JsonRequestBehavior.AllowGet);
        }
        else
        {
           return Json(new { status = "failed",masterdata= _qinscanvm, data = obj, message = "Data Not Found" }, JsonRequestBehavior.AllowGet);
        }


            //List<AWBList> obj = new List<AWBList>();
            //var lst = (from c in db.CustomerEnquiries where c.CollectedEmpID == id select c).ToList();

            //foreach (var item in lst)
            //{
            //    obj.Add(new AWBList { AWB=item.AWBNo});

            //}
            //return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAWBDetailbyCourier(int collectedby, DateTime Collecteddate)
        {
            
            List<AWBList> obj = new List<AWBList>();
            
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            obj = PickupRequestDAO.GetInScannedItemsByCourier(collectedby, Collecteddate);            

            if (obj.Count>0)
            {                      
            
                return Json(new { status = "ok", data = obj, message = "AWB Found" }, JsonRequestBehavior.AllowGet);
            }            
           else
            {
                obj = new List<AWBList>();
                
                    return Json(new { status = "Failed", data = obj, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
             }
            


        }
        public JsonResult GetAWBDetail(string id,int? collectedby,int? batchid)
        {
            if (collectedby == null)
                collectedby = 0;
            AWBList obj = new AWBList();
            if (batchid == null)
                batchid = 0;
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            //checking Domestic Shipment Collected 4,Out For Delivery 8,Attempted To Deliver 9,Delivery Pending 10 -- Checking AWB In the Inscanmaster
            //var inscanlst = (from c in db.InScanMasters
            //           where   (c.AWBNo == id) && (c.CourierStatusID <= 4 || c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)  //&& (c.PickedUpEmpID == collectedby || collectedby == 0)
            //           select new AWBList { InScanId = c.InScanID, AWB = c.AWBNo, Origin = c.ConsignorCountryName, Destination = c.ConsigneeCountryName,CourierStatusId=c.CourierStatusID,DRSID=c.DRSID }).ToList();

            var inscanlst = (from c in db.InScanMasters
                             where (c.AWBNo == id.Trim())  && (c.IsDeleted==false) && c.BranchID == BranchId //&& (c.PickedUpEmpID == collectedby || collectedby == 0)
                             select new AWBList { InScanId = c.InScanID, AWB = c.AWBNo, Origin = c.ConsignorCountryName, Destination = c.ConsigneeCountryName, CourierStatusId = c.CourierStatusID, DRSID = c.DRSID }).ToList();
            string domesticvalid = "";

            string importvalid = "";

            if (inscanlst!=null && inscanlst.Count >0)
            {
                if (inscanlst.Count > 0)
                {
                    var c = inscanlst[0];
                    if (c.CourierStatusId <= 4 || c.CourierStatusId == 8 || c.CourierStatusId == 9 || c.CourierStatusId == 10)
                    {

                        domesticvalid = "valid";
                    }
                    else
                    {
                        domesticvalid = "invalid";
                    }
                }
                else
                {
                    domesticvalid = "new";
                }

            }
            else
            {
                domesticvalid = "new";
            }
            
            
            if (inscanlst != null && inscanlst.Count>0 && domesticvalid=="valid")
            {
                obj.AWB = id;
                obj.Origin = inscanlst[0].Origin;
                obj.Destination = inscanlst[0].Destination;
                obj.InScanId = inscanlst[0].InScanId;
                obj.ShipmentDetailId = 0;
                obj.DRSID = inscanlst[0].DRSID;
                obj.CourierStatusId = inscanlst[0].CourierStatusId;
                return Json(new { status = "ok", data = obj, message = "AWB Found" }, JsonRequestBehavior.AllowGet);
            }

            //checking Imported Items
            //var importlst1 = (from c in db.ImportShipmentDetails
            //                  join i in db.ImportShipments on c.ImportID equals i.ID
            //                  where c.AWB == id.Trim() //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
            //                  select new AWBList { InScanId = 0, ShipmentDetailId = c.ShipmentDetailID, AWB = c.AWB, Origin = i.OriginAirportCity, Destination = c.DestinationCity, CourierStatusId = c.CourierStatusID, DRSID = c.DRSID }).ToList();

            var importlst1 = (from c in db.InboundShipments
                               where c.AWBNo == id.Trim() && c.IsDeleted==false //&& (c.CourierStatusID == 8 || c.CourierStatusID == 9 || c.CourierStatusID == 10)
                              select new AWBList { InScanId = 0, ShipmentDetailId = c.ShipmentID, AWB = c.AWBNo, Origin = c.ConsignorCityName, Destination = c.ConsigneeCityName, CourierStatusId = c.CourierStatusID, DRSID = c.DRSID }).ToList();


            if (importlst1 != null)
            {
                if (importlst1.Count > 0)
                {
                    var c = importlst1[0];
                    if (c.CourierStatusId == 8 || c.CourierStatusId == 9 || c.CourierStatusId == 10)
                    {

                        importvalid = "valid";
                    }
                    else
                    {
                        importvalid = "invalid";
                    }
                }
                else
                {
                    importvalid = "new";
                }

            }
            else
            {
                importvalid = "new";
            }

            if (importlst1 != null &&  importlst1.Count>0 && importvalid=="valid")
            {
                obj.AWB = id;
                obj.Origin = importlst1[0].Origin;
                obj.Destination = importlst1[0].Destination;
                obj.InScanId = 0;
                obj.CourierStatusId = importlst1[0].CourierStatusId;
                obj.ShipmentDetailId = importlst1[0].ShipmentDetailId;
                obj.DRSID = importlst1[0].DRSID;
                return Json(new { status = "ok", data = obj, message = "AWB Found" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                if (inscanlst == null || inscanlst.Count == 0 && (domesticvalid=="new" && importvalid=="new"))
                {
                    //Checking AWB at aWbdetails
                    var lst1 = db.AWBDetails.Where(cc => cc.AWBNo == id.Trim() && (cc.InScanID == null || cc.InScanID == 0)).FirstOrDefault();
                    if (lst1 != null)
                    {

                        obj.AWB = id;
                        obj.Origin = "";
                        obj.Destination = "";
                        obj.InScanId = 0;
                        obj.DRSID = 0;
                        obj.CourierStatusId = 0;
                        return Json(new { status = "ok", data = obj, message = "AWB found in AWB Details" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    { //Allowing outside AWB
                        obj.AWB = id;
                        obj.Origin = "";
                        obj.Destination = "";
                        obj.InScanId = 0;
                        obj.DRSID = 0;
                        obj.CourierStatusId = 0;
                        return Json(new { status = "ok", data = obj, message = "New AWB Adding!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                //else if (OutScanReturned==true)
                //{
                //    obj.AWB = id;
                //    obj.Origin = "";
                //    obj.Destination = "";
                //    obj.InScanId = 0;
                //    obj.DRSID = 0;
                //    obj.CourierStatusId = 0;
                //    return Json(new { status = "Failed", data = obj, message = "Invalid AWB - Not Outscanned!" }, JsonRequestBehavior.AllowGet);
                //}
                else
                {
                    obj.AWB = id;
                    obj.Origin = "";
                    obj.Destination = "";
                    obj.InScanId = 0;
                    obj.DRSID = 0;
                    obj.CourierStatusId = 0;
                    return Json(new { status = "Failed", data = obj, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
                }
            }
           
            
        }
      
        //
        // GET: /InScan/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /InScan/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /InScan/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /InScan/Delete/5

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteDepotInscan(id);
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

        [HttpGet]
        public JsonResult GetVehicle(int EmployeeId)
        {
            
                var list = db.VehicleMasters.Where(cc => cc.EmployeeId == EmployeeId).FirstOrDefault();

            if (list!=null)
                return Json(new { VehicleId = list.VehicleID }, JsonRequestBehavior.AllowGet);
           else
                return Json(new { VehicleId = 0 }, JsonRequestBehavior.AllowGet);

        

        }
    }
}
