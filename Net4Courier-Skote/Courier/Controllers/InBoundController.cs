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
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class InBoundController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index(int? StatusId, string FromDate, string ToDate)
        {
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            DateTime pFromDate;
            DateTime pToDate;
            int pStatusId = 0;
            if (StatusId == null)
            {
                pStatusId = 0;
            }
            else
            {
                pStatusId = Convert.ToInt32(StatusId);
            }
            if (FromDate == null || ToDate == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date.AddDays(1); // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;
            }
            else
            {
                pFromDate = Convert.ToDateTime(FromDate);//.AddDays(-1);
                pToDate = Convert.ToDateTime(ToDate).AddDays(1);

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
                                         join pet in db.AgentMasters on c.AgentID equals pet.AgentID into gj
                                         from subpet in gj.DefaultIfEmpty()
                                         join pet1 in db.EmployeeMasters on c.ReceivedByID equals pet1.EmployeeID into gj1
                                         from subpet1 in gj1.DefaultIfEmpty()
                                         where c.BranchId == branchid && (c.QuickInscanDateTime >= pFromDate && c.QuickInscanDateTime < pToDate)
                                          && c.DepotId == depotId
                                         //&& (c.CourierStatusID == pStatusId || pStatusId == 0)
                                         //&& c.IsDeleted == false
                                         //&& (c.CustomerID == Customerid || Customerid == 0)
                                         && c.Source=="Import"
                                         orderby c.QuickInscanDateTime descending
                                         select new InScanVM{ QuickInscanID=c.QuickInscanID,InScanSheetNo=c.InscanSheetNumber,QuickInscanDateTime=c.QuickInscanDateTime, AgentName= subpet.Name ,ReceivedBy=subpet1.EmployeeName , DriverName=c.DriverName }).ToList();

            //ViewBag.FromDate = pFromDate.Date.AddDays(1).ToString("dd-MM-yyyy");
            ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
            ViewBag.ToDate = pToDate.Date.AddDays(-1).ToString("dd-MM-yyyy");
            ViewBag.PickupRequestStatus = db.CourierStatus.Where(cc => cc.StatusTypeID == 1).ToList();
            ViewBag.StatusId = StatusId;
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
            ViewBag.employee = db.EmployeeMasters.ToList();
            ViewBag.employeerec = db.EmployeeMasters.ToList();
            ViewBag.Vehicles = db.VehicleMasters.ToList();
            ViewBag.Agents = db.AgentMasters.ToList();
            ViewBag.CourierService = db.CourierServices.ToList();
            var branch=db.BranchMasters.Find(BranchId);

            ViewBag.ReceivedStatus = "Received at " + branch.BranchName + " Facility";
            if (id==0)
            {
                ViewBag.Title = "Depot InScan-Import - Create";
                   InScanVM vm = new InScanVM();                
                vm.QuickInscanID = 0;
                
                PickupRequestDAO _dao = new PickupRequestDAO();
                vm.QuickInscanDateTime = CommonFunctions.GetCurrentDateTime();
                vm.ImportDate = vm.QuickInscanDateTime;
                vm.InScanSheetNo = _dao.GetMaxInScanSheetNo(vm.QuickInscanDateTime,companyid, BranchId,"Import");
                vm.DepotID = depotid;
                ViewBag.EditMode ="false";
                List<InBoundAWBList> awblist = new List<InBoundAWBList>();
                vm.AWBDetail = awblist;
                Session["InboundAWB"] = vm.AWBDetail;
                return View(vm);                
            }
            else
            {
                QuickInscanMaster qvm = db.QuickInscanMasters.Find(id);
                InScanVM vm = new InScanVM();
                vm.QuickInscanID = qvm.QuickInscanID;
                vm.QuickInscanDateTime = qvm.QuickInscanDateTime;
                vm.AgentID = Convert.ToInt32(qvm.AgentID);
                vm.ReceivedByID = Convert.ToInt32(qvm.ReceivedByID);
                vm.DriverName = qvm.DriverName;
                vm.InScanSheetNo = qvm.InscanSheetNumber;
                vm.VehicleId = Convert.ToInt32(qvm.VehicleId);
                vm.DepotID = Convert.ToInt32(qvm.DepotId);
                vm.BranchId = Convert.ToInt32(qvm.BranchId);
                
                ViewBag.EditMode = "true";
                ViewBag.Title = "Depot InScan-Import - Modify";
                var awblist = (from _qmaster in db.QuickInscanMasters
                       join _shipdetail in db.InboundShipments on _qmaster.QuickInscanID equals _shipdetail.QuickInscanID
                       
                       where _qmaster.QuickInscanID == qvm.QuickInscanID
                       orderby _shipdetail.AWBNo descending
                       select new InBoundAWBList
                       {
                           ShipmentDetailID = _shipdetail.ShipmentID,
                           AWB = _shipdetail.AWBNo,
                           OriginCity = _shipdetail.ConsignorCityName,
                           OriginCountry = _shipdetail.ConsignorCountryName,
                           DestinationCity = _shipdetail.ConsigneeCityName,
                           DestinationCountry = _shipdetail.ConsigneeCountryName,
                           Shipper = _shipdetail.Consignor,
                           Receiver = _shipdetail.Consignee,
                           CourierStatusId= _shipdetail.CourierStatusID,
                           RemoveAllowed =true,
                           AWBChecked=true,
                           MAWB =_shipdetail.MAWB,
                           ImportDate =_shipdetail.AWBDate                           
                           //StatusTypeId _shipdetail.StatusTypeId
                       }).ToList();

                if (awblist.Count > 0)
                {                    
                        vm.MAWB = awblist[0].MAWB;
                        vm.ImportDate = awblist[0].ImportDate;
                    

                }
                int i = 0;
                foreach(var item in awblist)
                {
                    if (item.CourierStatusId!=21) //received at desitnation/nice facility
                    {
                        awblist[i].RemoveAllowed = false;
                    }
                    i++;
                }
                vm.AWBDetail = awblist;
                Session["InboundAWB"] = vm.AWBDetail;
                return View(vm);
            }
            
        }

        
         [HttpPost]
        public JsonResult SaveQuickInScan(InScanVM v)
        {
            PickupRequestDAO _dao = new PickupRequestDAO();
            var bills = new List<updateitem>();            
            var IDetails = JsonConvert.DeserializeObject<List<AWBList>>(v.Details);
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int CompanyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());                     

            try
            {
                QuickInscanMaster _qinscan = new QuickInscanMaster();
                if (v.QuickInscanID > 0)
                {
                    _qinscan = db.QuickInscanMasters.Find(v.QuickInscanID);
                }
                else
                {                   
                    v.InScanSheetNo = _dao.GetMaxInScanSheetNo(v.QuickInscanDateTime,CompanyId, BranchId, "Import"); 
                    _qinscan.InscanSheetNumber = v.InScanSheetNo;
                    _qinscan.AcCompanyId = CompanyId;
                }                              
                
                _qinscan.ReceivedByID = v.ReceivedByID;
                _qinscan.AgentID = v.AgentID;
                //_qinscan.CollectedByID = v.CollectedByID;
                _qinscan.QuickInscanDateTime = v.QuickInscanDateTime;
                _qinscan.CollectedDate = v.QuickInscanDateTime;
                _qinscan.VehicleId = v.VehicleId;
                _qinscan.DriverName = v.DriverName;
                _qinscan.BranchId = BranchId;
                _qinscan.DepotId = v.DepotID;
                _qinscan.UserId = UserId;
                _qinscan.Source = "Import";
                if (v.QuickInscanID > 0)
                {
                    db.Entry(_qinscan).State= EntityState.Modified;
                    var removeinscanitems = v.RemovedInScanId.Split(',');

                    
                    foreach (var _item in removeinscanitems)
                    {
                        int _inscanid = Convert.ToInt32(_item);

                        //var _inscan = db.ImportShipmentDetails.Find(_inscanid);
                        var _inscan = db.InboundShipments.Find(_inscanid);
                        _inscan.QuickInscanID = null;

                        var couriercstatus = db.CourierStatus.Where(c => c.CourierStatus == "Export Manifest Prepared").FirstOrDefault();
                        if (couriercstatus != null)
                        {
                            _inscan.CourierStatusID = couriercstatus.CourierStatusID;
                        }
                        var statustype = db.tblStatusTypes.Where(c => c.Name == "READY TO EXPORT").FirstOrDefault();
                        if (statustype != null)
                        {
                            _inscan.StatusTypeId = statustype.ID;
                        }
                        
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();

                        //updateing awbstaus table for tracking
                        AWBTrackStatu _awbstatus = new AWBTrackStatu();
                        int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                        if (id == null)
                            id = 1;
                        else
                            id = id + 1;

                        _awbstatus.AWBTrackStatusId = Convert.ToInt32(id);
                        _awbstatus.AWBNo = _inscan.AWBNo;
                        string companyname = Session["CompanyName"].ToString();
                        if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                            _awbstatus.EntryDate = DateTime.UtcNow; // DateTime.Now;
                        else
                          _awbstatus.EntryDate = v.QuickInscanDateTime;// DateTime.UtcNow;
                        _awbstatus.InboundShipmentID = _inscan.ShipmentID;
                        _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                        _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                        _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                        _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                        _awbstatus.UserId = UserId;

                        db.AWBTrackStatus.Add(_awbstatus);
                        db.SaveChanges();
                    }
                }
                else
                {
                    db.QuickInscanMasters.Add(_qinscan);
                    db.SaveChanges();
                }

                foreach (var item in IDetails)
                {
                    int _inscanid = Convert.ToInt32(item.InScanId);
                    InboundShipment _inscan = db.InboundShipments.Find(_inscanid);
                    if (item.AWBChecked && v.QuickInscanID == 0)
                    {
                        updateitem uitem = new updateitem();
                        uitem.AWBNo = item.AWB;
                        uitem.synchronisedDateTime = Convert.ToDateTime(_qinscan.QuickInscanDateTime).ToString("dd-MMM-yyyy HH:mm");
                        bills.Add(uitem);

                        _inscan.QuickInscanID = _qinscan.QuickInscanID;
                        _inscan.StatusTypeId = 9;// db.tblStatusTypes.Where(cc => cc.Name == "INSCAN").First().ID;
                        _inscan.CourierStatusID = 21;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Received at Origin Facility").FirstOrDefault().CourierStatusID;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if(item.AWBChecked==false && v.QuickInscanID>0)
                    {
                        _inscan.QuickInscanID = null;
                        _inscan.StatusTypeId = 9;// db.tblStatusTypes.Where(cc => cc.Name == "INSCAN").First().ID;
                        _inscan.CourierStatusID = 12;// db.CourierStatus.Where(cc => cc.StatusTypeID == _inscan.StatusTypeId && cc.CourierStatus == "Received at Origin Facility").FirstOrDefault().CourierStatusID;
                        db.Entry(_inscan).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    

                    //updateing awbstaus table for tracking
                    AWBTrackStatu _awbstatus = new AWBTrackStatu();
                    if (item.AWBChecked && v.QuickInscanID == 0)
                    {
                        _awbstatus = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == 21).FirstOrDefault();
                        if (_awbstatus == null)
                        {
                            _awbstatus = new AWBTrackStatu();
                            _awbstatus.AWBNo = _inscan.AWBNo;
                            string companyname = Session["CompanyName"].ToString();
                            if (companyname == "NICE AL MARRI EXPRESS SERVICES LLC")
                                _awbstatus.EntryDate = DateTime.UtcNow; // DateTime.Now;
                            else
                                _awbstatus.EntryDate = v.QuickInscanDateTime;// DateTime.UtcNow; // DateTime.Now;
                            _awbstatus.InboundShipmentID = _inscan.ShipmentID;
                            _awbstatus.StatusTypeId = Convert.ToInt32(_inscan.StatusTypeId);
                            _awbstatus.CourierStatusId = Convert.ToInt32(_inscan.CourierStatusID);
                            _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_inscan.StatusTypeId).Name;
                            _awbstatus.CourierStatus = db.CourierStatus.Find(_inscan.CourierStatusID).CourierStatus;
                            _awbstatus.UserId = UserId;
                            _awbstatus.EmpID = v.ReceivedByID; // db.EmployeeMasters.Where(cc => cc.UserID == UserId).FirstOrDefault().EmployeeID;
                            db.AWBTrackStatus.Add(_awbstatus);
                            db.SaveChanges();
                        }
                    }
                    else if (item.AWBChecked ==false && v.QuickInscanID > 0)
                    {
                        _awbstatus = db.AWBTrackStatus.Where(cc => cc.AWBNo == _inscan.AWBNo && cc.CourierStatusId == 21).FirstOrDefault();
                        db.AWBTrackStatus.Remove(_awbstatus);
                        db.SaveChanges();

                    } 
                }
                
                //TempData["SuccessMsg"] = "You have successfully Saved InScan Items.";             
           
                return Json(new { status = "ok", message = "You have successfully Saved Import InScan Items.!" } , JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
                //return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAWB(int id)
        {

            List<InBoundAWBList> obj = new List<InBoundAWBList>();
            
            QuickInscanMaster _qinscanvm = db.QuickInscanMasters.Where(cc => cc.QuickInscanID == id).FirstOrDefault();

            //InScanVM _qinscanvm = new InScanVM();

            //_qinscanvm = (from _qmaster in db.QuickInscanMasters
            //              where _qmaster.InscanSheetNumber == id
            //              select new InScanVM { QuickInscanID=_qmaster.QuickInscanID,QuickInscanDateTime=_qmaster.QuickInscanDateTime,BranchId=_qmaster.BranchId,DepotID=_qmaster.DepotId,VehicleId=_qmaster.VehicleId,DriverName = _qmaster.DriverName });.first

            if (_qinscanvm != null)
            { 
                obj = (from _qmaster in db.QuickInscanMasters
                            join _shipdetail in db.ImportShipmentDetails on _qmaster.QuickInscanID equals _shipdetail.QuickInscanID     
                            join  _shipment in db.ImportShipments on  _shipdetail.ImportID equals _shipment.ID
                            where _qmaster.QuickInscanID == id
                            orderby _shipdetail.AWB descending
                            select new InBoundAWBList { ShipmentDetailID = _shipdetail.ShipmentDetailID, AWB = _shipdetail.AWB, 
                                OriginCity = _shipment.OriginAirportCity, OriginCountry =_shipment.OriginAirportCity, DestinationCity  = _shipdetail.DestinationCity, DestinationCountry = _shipdetail.DestinationCountry,Shipper=_shipdetail.Shipper, Receiver=_shipdetail.Receiver   }).ToList();

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
        public JsonResult GetAWBDetail(string id)
        {
            InScanVM vm = new InScanVM();
            vm.AWBDetail = (List<InBoundAWBList>)Session["InboundAWB"];
            InBoundAWBList obj = new InBoundAWBList();
            //var lst = (from c in db.InScanMasters where c.AWBNo == id select c).FirstOrDefault();
            var couriercstatus = db.CourierStatus.Where(c => c.CourierStatus == "Export Manifest Prepared").FirstOrDefault();
            int CourierStatusId = 12; // 0;At Destination Customs Facility
            var lst = (from c in db.InboundShipments
                      
                       where c.AWBNo == id && c.CourierStatusID == CourierStatusId
                       select new { ConsignorName = "", ConsignorCountryName = "", ConsignorCityName = "", c.ConsigneeCityName, c.ConsigneeCountryName, c.AWBNo, c.ShipmentID, c.Consignor, c.Consignee }).FirstOrDefault();  //forwarded to agent status only

            //var lst = (from c in db.ImportShipmentDetails join i in db.ImportShipments on c.ImportID equals i.ID
            //           where c.AWB == id &&  c.CourierStatusID == CourierStatusId
            //           select new { ConsignorName="", ConsignorCountryName="", ConsignorCityName="", c.DestinationCity, c.DestinationCountry, c.AWB, c.ShipmentDetailID, c.Shipper, c.Receiver }).FirstOrDefault();  //forwarded to agent status only
            if (lst==null)
            {
                return Json(new { status="failed", data = obj, message = "AWB No. Not found"}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                 obj.ConsignorName = lst.ConsignorName;
                 obj.OriginCountry = lst.ConsignorCountryName;
                 obj.OriginCity = lst.ConsignorCityName;
                 obj.DestinationCity = lst.ConsigneeCityName;
                 obj.DestinationCountry = lst.ConsigneeCountryName;
                obj.AWB = lst.AWBNo;
                 obj.Receiver = lst.Consignor;
                 obj.Shipper = lst.Consignee;
                 obj.ShipmentDetailID = lst.ShipmentID;
                obj.AWBChecked = true;
                vm.AWBDetail.Add(obj);
                Session["InboundAWB"] = vm.AWBDetail;
                return Json(new { status = "ok", data = obj, message = "AWB Number found" }, JsonRequestBehavior.AllowGet);

                
            }            
            
        }

        public JsonResult GetMAWBList(string term,string ImportDate)
        {
            DateTime pManifestDate;
            DateTime pManifestDate1;
            if (ImportDate != "")
            {
                pManifestDate = Convert.ToDateTime(ImportDate).Date;
                pManifestDate1 = Convert.ToDateTime(ImportDate).Date.AddDays(1);

                var lst = (from c in db.InboundShipments 
                           join d in db.InboundAWBBatches on c.BATCHID equals d.ID
                           where   c.MovementID == 3 && d.BatchDate >= pManifestDate.Date && d.BatchDate < pManifestDate1 && c.QuickInscanID == null
                           select new { MAWB = c.MAWB }).Distinct().ToList();  //forwarded to agent status only
                //var lst = (from c in db.ImportShipments
                //           join d in db.ImportShipmentDetails on c.ID equals d.ImportID
                //           where c.CreatedDate >= pManifestDate.Date && c.CreatedDate < pManifestDate1 && d.QuickInscanID == null
                //           select new { MAWB = d.MAWB + ":" + d.Route }).Distinct().ToList();  //forwarded to agent status only
                return Json(lst, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var lst = (from c in db.InboundShipments
                          
                           where c.QuickInscanID == null && c.MovementID==3
                           select new { MAWB = c.MAWB }).Distinct().ToList();  //forwarded to agent status only
                //var lst = (from c in db.ImportShipments
                //           join d in db.ImportShipmentDetails on c.ID equals d.ImportID
                //           where  d.QuickInscanID == null
                //           select new { MAWB = d.MAWB + ":" + d.Route }).Distinct().ToList();  //forwarded to agent status only
                return Json(lst, JsonRequestBehavior.AllowGet);
            }
            

        }

        public JsonResult GetMAWBDetail(string ImportDate,string MAWB,bool checkall)
        {

            InScanVM vm = new InScanVM();
            List<InBoundAWBList> AWBDetail = new List<InBoundAWBList>();
            var lst = new List<InBoundAWBList>();
            DateTime pManifestDate=DateTime.Now;
            DateTime pManifestDate1=DateTime.Now;

            if (ImportDate != "")
            {
                
                pManifestDate = Convert.ToDateTime(ImportDate).Date;

                pManifestDate1 = pManifestDate.Date.AddDays(1);
            }
            
            if (MAWB.IndexOf(':')>0)
            {
                MAWB = MAWB.Split(':')[0];
            }
            var couriercstatus = db.CourierStatus.Where(c => c.CourierStatus == "Export Manifest Prepared").FirstOrDefault();
            int CourierStatusId = 12; // 0;At Destination Customs Facility            

            

            if (ImportDate != "")
            {
                lst = (from c in db.InboundShipments
                       join d in db.InboundAWBBatches on c.BATCHID equals d.ID
                       where c.MAWB.Contains(MAWB) && d.BatchDate >= pManifestDate.Date && d.BatchDate < pManifestDate1 && c.CourierStatusID == CourierStatusId && c.QuickInscanID == null
                       select new InBoundAWBList { ConsignorName =c.Consignor,  OriginCountry = c.ConsignorCountryName, OriginCity = c.ConsignorCityName , DestinationCity = c.ConsigneeCityName, DestinationCountry = c.ConsigneeCountryName, AWB = c.AWBNo, ShipmentDetailID = c.ShipmentID, Shipper = c.Consignor, Receiver = c.Consignee, AWBChecked = checkall, RemoveAllowed = true }).ToList();  //forwarded to agent statu

                //lst = (from c in db.ImportShipmentDetails
                //       join i in db.ImportShipments on c.ImportID equals i.ID
                //       where c.MAWB.Contains(MAWB) && i.CreatedDate >= pManifestDate.Date && i.CreatedDate < pManifestDate1 && c.CourierStatusID == CourierStatusId && c.QuickInscanID == null
                //       select new InBoundAWBList { ConsignorName = i.OriginAirportCity, OriginCountry = i.OriginAirportCity, OriginCity = i.OriginAirportCity, DestinationCity = c.DestinationCity, DestinationCountry = c.DestinationCountry, AWB = c.AWB, ShipmentDetailID = c.ShipmentDetailID, Shipper = c.Shipper, Receiver = c.Receiver, AWBChecked = checkall, RemoveAllowed = true }).ToList();  //forwarded to agent status only
            }
            else
            {
                lst = (from c in db.InboundShipments

                       where c.MAWB.Contains(MAWB) && c.CourierStatusID == CourierStatusId && c.QuickInscanID == null
                       select new InBoundAWBList { ConsignorName = c.Consignor, OriginCountry = c.ConsignorCountryName, OriginCity = c.ConsignorCityName, DestinationCity = c.ConsigneeCityName, DestinationCountry = c.ConsigneeCountryName, AWB = c.AWBNo, ShipmentDetailID = c.ShipmentID, Shipper = c.Consignor, Receiver = c.Consignee, AWBChecked = checkall, RemoveAllowed = true }).ToList();  //forwarded to agent statu

                //lst = (from c in db.ImportShipmentDetails
                //       join i in db.ImportShipments on c.ImportID equals i.ID
                //       where c.MAWB.Contains(MAWB) && c.CourierStatusID == CourierStatusId && c.QuickInscanID == null
                //       select new InBoundAWBList { ConsignorName = i.OriginAirportCity, OriginCountry = i.OriginAirportCity, OriginCity = i.OriginAirportCity, DestinationCity = c.DestinationCity, DestinationCountry = c.DestinationCountry, AWB = c.AWB, ShipmentDetailID = c.ShipmentDetailID, Shipper = c.Shipper, Receiver = c.Receiver, AWBChecked = checkall, RemoveAllowed = true }).ToList();  //forwarded to agent status only

            }

            
            if (lst == null)
            {
                return Json(new { status = "Failed", Message = "AWB No. Not found" }, JsonRequestBehavior.AllowGet);
            }
            else if (lst.Count>0)
            {
                //foreach (var item in lst)
                //{
                //    InBoundAWBList obj = new InBoundAWBList();
                //    obj.ConsignorName = item.ConsignorName;
                //    obj.OriginCountry = item.ConsignorCountryName;
                //    obj.OriginCity = item.ConsignorCityName;
                //    obj.DestinationCity = item.DestinationCity;
                //    obj.DestinationCountry = item.DestinationCountry;
                //    obj.AWB = item.AWB;
                //    obj.Receiver = item.Receiver;
                //    obj.Shipper = item.Shipper;
                //    obj.ShipmentDetailID = item.ShipmentDetailID;
                //    AWBDetail.Add(obj);

                //}
                
                Session["InboundAWB"] = lst;
                return Json(new { status = "ok", Message = "AWB Number found" }, JsonRequestBehavior.AllowGet);


            }
            else
            {
                Session["InboundAWB"] = AWBDetail;
                return Json(new { status = "Failed", Message = "No AWB Number found" }, JsonRequestBehavior.AllowGet);


            }

        }
        [HttpPost]
        public async Task<ActionResult> CallPostAPI()
        {
            string URL = "http://www.niceexpress.net/API/v1/postAPI.do";
            //string idate = Convert.ToDateTime(InputDate).ToString("dd-MMM-yyyy hh:mm");

            postbill bills = (postbill)Session["bills"];
            //var json = JsonConvert.SerializeObject(bills);
            //string urlParameters = "?bills=" + json;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                //HTTP GET
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("niceexpress-apikey", "14c3993c0bb082dcafae9183ac7946d5be7a0e565e12bbd32f5d8d5d78bf3121");
                client.DefaultRequestHeaders.Add("niceexpress-signature", "37ef0bccb326b2057121ab74fd81cbeee892debaeccdd632d57fff66ffd86ece");

                // List data response.
                //var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //var result  = await client.PostAsync("method",content);  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                //string resultContent = await result.Content.ReadAsStringAsync();
                var postResponse = await client.PostAsJsonAsync(URL, bills);
                var response = postResponse.EnsureSuccessStatusCode();
                
                return Json(new { Status = "ok", Message = "API Updated Successfully " }, JsonRequestBehavior.AllowGet);
            }
            //return "notworked";
        }

        public ActionResult ShowAWBList()
        {
            InScanVM vm = new InScanVM();
            vm.AWBDetail = (List<InBoundAWBList>)Session["InboundAWB"];
            return PartialView("ItemList", vm);
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

        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {

            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteImportInscan(id);
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
