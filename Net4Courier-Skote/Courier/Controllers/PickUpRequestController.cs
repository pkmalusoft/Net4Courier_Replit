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
    public class PickUpRequestController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index(int? StatusId, string FromDate,string ToDate)
        {
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int UserId = Convert.ToInt32(Session["UserID"].ToString());
            DateTime pFromDate;
            DateTime pToDate;
            int pStatusId = 0;
            if (StatusId==null)
            {
                pStatusId = 0;
            }
            else
            {
                pStatusId =Convert.ToInt32(StatusId);
            }
            if (FromDate == null || ToDate == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date.AddDays(1); // // ToDate = DateTime.Now;
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
            List<PickupRequestVM> lst = (from c in db.InScanMasters
                                         join statustype in db.tblStatusTypes on c.StatusTypeId equals statustype.ID
                                         join status in db.CourierStatus on c.CourierStatusID equals status.CourierStatusID
                                         join pet in db.EmployeeMasters on c.PickedUpEmpID equals pet.EmployeeID into gj
                                         from subpet in gj.DefaultIfEmpty()
                                         join pet1 in db.EmployeeMasters on c.AssignedEmployeeID equals pet1.EmployeeID into gj1
                                         from subpet1 in gj1.DefaultIfEmpty()
                                         where c.BranchID==branchid &&  c.IsEnquiry == true
                                         && (c.PickupRequestDate >= pFromDate && c.PickupRequestDate < pToDate) && (c.CourierStatusID == pStatusId || pStatusId == 0)
                                         && c.IsDeleted==false
                                         //&& (c.CustomerID==Customerid || Customerid==0)
                                         && (c.CreatedBy== UserId)
                                         orderby c.PickupRequestDate descending
                                         select new PickupRequestVM {InScanID=c.InScanID,  PickupRequestStatusId=c.CourierStatusID, EnquiryID = c.InScanID, EnquiryNo = c.EnquiryNo, EnquiryDate = c.PickupRequestDate, Consignor = c.Consignor, Consignee = c.Consignee, eCollectedBy = subpet.EmployeeName ?? string.Empty, eAssignedTo = subpet1.EmployeeName ?? string.Empty, AWBNo = c.AWBNo, PickupRequestStatus = status.CourierStatus , ShipmentType=statustype.Name,ConsigneeCityName=c.ConsigneeCityName,ConsigneeCountryName=c.ConsigneeCountryName,ConsignorCountryName=c.ConsignorCountryName,ConsignorCityName=c.ConsignorCityName }).ToList();

            //ViewBag.FromDate = pFromDate.Date.AddDays(1).ToString("dd-MM-yyyy");
            ViewBag.FromDate = pFromDate.Date.ToString("dd-MM-yyyy");
            ViewBag.ToDate = pToDate.Date.AddDays(-1).ToString("dd-MM-yyyy");
            ViewBag.PickupRequestStatus = db.CourierStatus.Where(cc => cc.StatusTypeID == 1).ToList();
            ViewBag.StatusId = StatusId;
            return View(lst);
        }


        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCustomerBooking(id);
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

        public ActionResult Create(int id=0)
        {
            
            int uid = Convert.ToInt32(Session["UserID"].ToString());

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            //ViewBag.Vehicle = db.VehicleMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.RequestType = db.RequestTypes.ToList();
            ViewBag.DocumentType = db.tblDocumentTypes.ToList();
            //ViewBag.PickupRequestStatus = db.PickUpRequestStatus.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.PickupRequestStatus = db.CourierStatus.Where(cc=>cc.StatusTypeID==1).ToList();
            ViewBag.VehicleType = db.tblVehicleTypes.ToList();
            ViewBag.PickupSubReason = db.SubStatus.ToList();
            PickupRequestVM v = new PickupRequestVM();

            UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
            int empid = u.UserID;            
            string empname = u.UserName;
            ViewBag.empname = empname;
            ViewBag.empid = empid;

            if (id==0)
            {
                PickupRequestDAO doa = new PickupRequestDAO();
                ViewBag.RequestNo = doa.GetMaxPickupRequest(companyId,branchid);
                ViewBag.StatusId = 1;
                int Customerid = 0;
                 if (Session["UserType"].ToString() == "Customer")
                {
                    Customerid = Convert.ToInt32(Session["CustomerId"].ToString());
                    v.CustomerID = Customerid;
                    var _cust = db.CustomerMasters.Find(Customerid);
                    v.CustomerName = _cust.CustomerName;
                    v.CustomerCode= _cust.CustomerCode;
                    v.Consignor = _cust.CustomerName;
                    if (_cust.Phone == null)
                        v.ConsignorPhone = _cust.Mobile;
                    else
                       v.ConsignorPhone = _cust.Phone;
                    
                    v.OfficeTimeFrom = _cust.OfficeOpenTime;
                    v.OfficeTimeTo = _cust.OfficeCloseTime;
                    v.ConsignorAddress = _cust.Address1;
                    v.ConsignorAddress1 = _cust.Address2;
                    v.ConsignorAddress2 = _cust.Address3;
                    v.ConsignorCountryName = _cust.CountryName;
                    v.ConsignorLocationName = _cust.LocationName;
                    v.ConsignorCityName = _cust.CityName;
                    v.ConsignorContact = _cust.ContactPerson;
                    v.Email = _cust.Email;
                    v.RequestSource ="4";
                }


            }
            else
            {
                
                v = GetPickupRequestDetail(id);
                ViewBag.AWBNo = v.AWBNo;
                ViewBag.StatusId = v.PickupRequestStatusId;
                ViewBag.SubReasonStatusId = v.SubReasonId;
                ViewBag.Status = db.CourierStatus.Where(cc => cc.CourierStatusID == v.PickupRequestStatusId).FirstOrDefault().CourierStatus;
            }
            
            return View(v);
        }


        [HttpPost]
        public ActionResult Create(PickupRequestVM v)
        {
            PickupRequestDAO _dao = new PickupRequestDAO();
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
            int empid = u.UserID;
            string empname = u.UserName;

            InScanMaster _enquiry = new InScanMaster();
            if (v.InScanID == 0)
            {
                int id = (from c in db.InScanMasters orderby c.InScanID descending select c.InScanID).FirstOrDefault();
                _enquiry.InScanID = id + 1;
                _enquiry.EnquiryNo = _dao.GetMaxPickupRequest(companyId, branchid); // (id + 1).ToString();
                _enquiry.AWBNo = _dao.GetMaAWBNo(companyId,branchid);
                _enquiry.AcCompanyID = companyId;
                _enquiry.BranchID = branchid;
                _enquiry.DepotID = depotId;
                _enquiry.TransactionDate = DateTime.Now;
                _enquiry.DeviceID = "WebSite";
                _enquiry.IsDeleted = false;
                int statustypeid = 1;// db.tblStatusTypes.Where(c => c.Name == "Pickup Request").FirstOrDefault().ID;
                _enquiry.StatusTypeId = statustypeid; //pickuprequest       
                _enquiry.AcFinancialYearID = yearid;
                _enquiry.CreatedDate = CommonFunctions.GetCurrentDateTime();
                _enquiry.CreatedBy = uid;
            }
            else
            {
                _enquiry = db.InScanMasters.Find(v.InScanID);
                _enquiry.LastModifiedDate = CommonFunctions.GetCurrentDateTime();
                _enquiry.LastModifiedBy = uid;
            }
            _enquiry.PaymentModeId=v.PaymentModeId;
            _enquiry.DocumentTypeId = v.DocumentTypeId;
            _enquiry.VehicleTypeId = v.VehicleTypeId;
            _enquiry.PickupRequestDate = Convert.ToDateTime(v.EnquiryDate);
                _enquiry.CustomerID = v.CustomerID;

                _enquiry.ConsignorCountryName = v.ConsignorCountryName;
                _enquiry.ConsignorCityName = v.ConsignorCityName;
                _enquiry.ConsigneeLocationName = v.ConsigneeLocationName;
                _enquiry.ConsignorLocationName = v.ConsignorLocationName;
  
                _enquiry.ConsigneeCountryName = v.ConsigneeCountryName;
                _enquiry.ConsigneeCityName = v.ConsigneeCityName;
                _enquiry.Weight = Convert.ToDecimal(v.Weight);
                
                
                _enquiry.Consignee = v.Consignee;
                _enquiry.Consignor = v.Consignor;
                _enquiry.ConsignorAddress1_Building = v.ConsignorAddress;
                _enquiry.ConsignorAddress2_Street = v.ConsignorAddress1;
                _enquiry.ConsignorAddress3_PinCode = v.ConsignorAddress2;
                _enquiry.ConsigneeAddress1_Building = v.ConsigneeAddress;
                _enquiry.ConsigneeAddress2_Street = v.ConsigneeAddress1;
                _enquiry.ConsigneeAddress3_PinCode = v.ConsigneeAddress2;
                _enquiry.ConsignorPhone = v.ConsignorPhone;
                _enquiry.ConsigneePhone = v.ConsigneePhone;

                _enquiry.AssignedEmployeeID= v.EmployeeID;

                _enquiry.Remarks = v.Remarks;
                _enquiry.PickedUpEmpID = v.CollectedEmpID;
                _enquiry.PickedupDate = v.CollectedTime;

                //_enquiry.ShipmentType = v.ShipmentType;
                //if (v.vehreq == true)
                //{
                //    _enquiry.Vehicle = v.Vehicle;
                //    _enquiry.VechileTypeId = v.VehicleTypeId
                //}


                _enquiry.ConsigneeContact = v.ConsigneeContact;
                _enquiry.ConsignorContact = v.ConsignorContact;
                _enquiry.EnteredByID = empid; //userid
                _enquiry.IsEnquiry = true;

                _enquiry.PickupReadyTime = v.ReadyTime;

            //_enquiry.OfficeTimeFrom = v.OfficeTimeFrom;
            //_enquiry.OfficeTimeTo = v.OfficeTimeTo;
            _enquiry.PickupLocation = v.PickupLocation;
            _enquiry.PickupSubLocality = v.PickupSubLocality;
            _enquiry.DeliverySubLocality = v.DeliverySubLocality;
            _enquiry.DeliveryLocation = v.DeliveryLocation;
            _enquiry.OriginPlaceID = v.PickupLocationPlaceId;
            _enquiry.DestinationPlaceID = v.DeliveryLocationPlaceId;
                _enquiry.RequestSource = v.RequestSource;

            _enquiry.CourierCharge = v.CourierCharge;
            _enquiry.NetTotal = v.CourierCharge;

            if (_enquiry.ConsignorCountryName==_enquiry.ConsigneeCountryName)
                _enquiry.MovementID = 1; //Doemstic
            else
                _enquiry.MovementID = 2;  //Export

            if (v.MaterialCost == null)
            { _enquiry.MaterialCost = 0; 
            }
            else
            {
                
                _enquiry.MaterialCost = v.MaterialCost;
            }

            _enquiry.CargoDescription = v.Description;
            _enquiry.Pieces = v.Pieces;

            if (_enquiry.StatusTypeId == 1)
            {
                if (_enquiry.AssignedEmployeeID == null)
                { _enquiry.CourierStatusID = 1; }
                else if (_enquiry.AssignedEmployeeID != null && _enquiry.PickedUpEmpID == null)
                { _enquiry.CourierStatusID = 2; }
                else if (_enquiry.PickedUpEmpID != null)
                { _enquiry.PickupRequestStatusId = 3; }
                else if (_enquiry.StatusTypeId == 1 && _enquiry.CourierStatusID == null)
                {
                    _enquiry.CourierStatusID = 1; //request
                }
            }

            //db.CustomerEnquiries.Add(_enquiry);
            if (v.InScanID == 0)
            {
                db.InScanMasters.Add(_enquiry);
                db.SaveChanges();
                //
                AWBTrackStatu _awbstatus = new AWBTrackStatu();
                int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                if (id == null)
                    id = 1;
                else
                    id = id + 1;

                _awbstatus.AWBTrackStatusId = Convert.ToInt32(id);
                _awbstatus.AWBNo = _enquiry.AWBNo;
                _awbstatus.EntryDate = DateTime.Now;
                _awbstatus.InScanId = _enquiry.InScanID;
                _awbstatus.StatusTypeId = Convert.ToInt32(_enquiry.StatusTypeId);
                _awbstatus.CourierStatusId = Convert.ToInt32(_enquiry.CourierStatusID);
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_enquiry.StatusTypeId).Name;
                _awbstatus.CourierStatus = db.CourierStatus.Find(_enquiry.CourierStatusID).CourierStatus;
                _awbstatus.UserId = uid;

                db.AWBTrackStatus.Add(_awbstatus);
                db.SaveChanges();
                //

                SaveConsignee(v);
                TempData["SuccessMsg"] = "You have successfully added Pickup Request.";
            }
            else
            {
                SaveConsignee(v);
                db.Entry(_enquiry).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully updated Pickup Request.";
            }                           

                return RedirectToAction("Index");

            //}
            //else
            //{
            //    int uid = Convert.ToInt32(Session["UserID"].ToString());

            //    UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
            //    int empid = u.UserID;
            //    string empname = u.UserName;

            //    ViewBag.Country = db.CountryMasters.ToList();
            //    ViewBag.City = db.CityMasters.ToList();
            //    ViewBag.Location = db.LocationMasters.ToList();
            //    ViewBag.Vehicle = db.VehicleMasters.ToList();
            //    ViewBag.Employee = db.EmployeeMasters.ToList();
            //    ViewBag.Customer = db.CustomerMasters.ToList();
            //    ViewBag.RequestType = db.RequestTypes.ToList();
            //    ViewBag.empname = empname;
            //    ViewBag.empid = empid;
            //    PickupRequestDAO doa = new PickupRequestDAO();
            //    ViewBag.RequestNo = doa.GetMaxPickupRequest();
            //    return View();
            //}


        }

       [HttpPost] 
       public JsonResult SaveStatus(ChangeStatus v)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            string status = "ok";
            string message = "";
            bool statuschangepersmission = true;
            try
            {
                InScanMaster _enquiry = db.InScanMasters.Find(v.InScanID);
               
                //admin level rights checking to revert the status
                if (_enquiry.CourierStatusID > v.ChangeStatusId)
                {
                    List<int> RoleId = (List<int>)Session["RoleID"];
                    if (RoleId[0]!=1)
                    {
                        statuschangepersmission = false;
                    }                    
                }

                if (statuschangepersmission==false)
                {
                    status = "failed";
                    return Json(new { status = status, message = "User does not have persmission to revert the status,Contact Admin!" }, JsonRequestBehavior.AllowGet);
                }


                if (v.ChangeStatusId == 2)
                {
                    _enquiry.AssignedEmployeeID = v.AssignedEmployee;
                    _enquiry.CourierStatusID = 2;
                }
                else if (v.ChangeStatusId == 3)
                {
                    _enquiry.CourierStatusID = 3;
                    _enquiry.SubReasonId = v.SubStatusReason;
                }
                else if (v.ChangeStatusId == 4)
                {
                    _enquiry.PickedUpEmpID = v.PickedUpId;
                    _enquiry.PickedupDate = v.PickedUpDateTime;
                    _enquiry.CourierStatusID= 4;
                }

                db.Entry(_enquiry).State = EntityState.Modified;
                db.SaveChanges();

                
                //updateing awbstaus table for tracking
                AWBTrackStatu _awbstatus = new AWBTrackStatu();
                int? id = (from c in db.AWBTrackStatus orderby c.AWBTrackStatusId descending select c.AWBTrackStatusId).FirstOrDefault();

                if (id == null)
                    id = 1;
                else
                    id = id + 1;

                _awbstatus.AWBTrackStatusId =Convert.ToInt32(id);
                _awbstatus.AWBNo = _enquiry.AWBNo;
                _awbstatus.EntryDate = DateTime.Now;
                _awbstatus.InScanId = _enquiry.InScanID;
                _awbstatus.StatusTypeId = Convert.ToInt32(_enquiry.StatusTypeId);
                _awbstatus.CourierStatusId = Convert.ToInt32(_enquiry.CourierStatusID);
                _awbstatus.ShipmentStatus = db.tblStatusTypes.Find(_enquiry.StatusTypeId).Name;
                _awbstatus.CourierStatus = db.CourierStatus.Find(_enquiry.CourierStatusID).CourierStatus;
                _awbstatus.UserId = uid;

                db.AWBTrackStatus.Add(_awbstatus);
                db.SaveChanges();

                message = "Status Changed Successfully!";
            }

            catch(Exception ex )
            {
                status = "failed";
                message = ex.Message;
            }
            return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        public JsonResult GetStatus(int InScanID)
        {
            string status = "ok";
            ChangeStatus _cstatus = new ChangeStatus();
            try
            {
                InScanMaster _inscan = db.InScanMasters.Find(InScanID);

                _cstatus.InScanID = _inscan.InScanID;
                _cstatus.StatusType = db.tblStatusTypes.Where(cc => cc.ID == _inscan.StatusTypeId).FirstOrDefault().Name;
                _cstatus.ChangeStatusId =Convert.ToInt32(_inscan.CourierStatusID);
                
                if (_inscan.PickedUpEmpID!=null)
                    _cstatus.PickedUpId = Convert.ToInt32(_inscan.PickedUpEmpID);

                if (_inscan.AssignedEmployeeID !=null)
                    _cstatus.AssignedEmployee = Convert.ToInt32(_inscan.AssignedEmployeeID);

                if (_inscan.PickedupDate != null)
                    _cstatus.PickedUpDateTime = Convert.ToDateTime(_inscan.PickedupDate);

                if (_inscan.SubReasonId != null)
                    _cstatus.SubStatusReason =Convert.ToInt32(_inscan.SubReasonId);
            }

            catch (Exception ex)
            {
                status = ex.Message;
            }

            return Json(new { data = _cstatus, result = status }, JsonRequestBehavior.AllowGet);

        }
        
        [HttpGet]
        public JsonResult GetCustomerName()
        {
            var customerlist = (from c1 in db.CustomerMasters where c1.CustomerType != "CN" orderby c1.CustomerName select c1.CustomerName).ToList();

            return Json(new { data = customerlist }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetCustomerId(string id)
        {
            int custid = 0;
            var customerlist = db.CustomerMasters.Where(c1 => c1.CustomerType != "CN" & c1.CustomerName == id).FirstOrDefault();
            if (customerlist == null)
                custid = 0;
            else
                custid = customerlist.CustomerID;

            return Json(new { data = custid }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetConsignee()
        {
            var consineelist = (from c1 in db.CustomerMasters where c1.CustomerType== "CN" orderby c1.CustomerName select c1.CustomerName).ToList();

            return Json(new { data = consineelist }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Edit(int id)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            UserRegistration u = (from c in db.UserRegistrations where c.UserID == uid select c).FirstOrDefault();
            int empid = u.UserID;
            string empname = u.UserName;


            //ViewBag.Country = db.CountryMasters.ToList();
            
            //ViewBag.Location = db.LocationMasters.ToList();
            ViewBag.Vehicle = db.VehicleMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.empname = empname;
            ViewBag.empid = empid;

            PickupRequestVM v = new PickupRequestVM();
            //CustomerEnquiry a = db.CustomerEnquiries.Find(id);
            InScanMaster a = db.InScanMasters.Find(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            else
            {
               
                v.EnquiryID = a.InScanID;
                v.EnquiryNo = a.EnquiryNo;
                v.AWBNo = a.AWBNo;
                v.EnquiryDate = a.PickupRequestDate;
                v.DescriptionID = 4;
                //v.ConsignerCountryId = a.ConsignerCountryId;
                //v.ConsignerCityId = 19; // a.ConsignerCityId;
                //v.ConsignerLocationId = 7;// a.ConsignerLocationId;
                v.ConsignorCountryName = a.ConsignorCountryName;
                v.ConsignorCityName = a.ConsignorCityName;
                v.ConsignorLocationName = a.ConsignorLocationName;
                //v.ConsigneeCountryID = a.ConsigneeCountryID;
                //v.ConsigneeCityId = a.ConsigneeCityId;
                v.ConsigneeCountryName = a.ConsigneeCountryName;
                v.ConsigneeCityName = a.ConsigneeCityName;
                //v.ConsigneeLocationId = a.ConsigneeLocationId;
                v.ConsigneeLocationName = a.ConsigneeLocationName;                
                v.Weight = Convert.ToDouble(a.Weight);
                v.AcCompanyID = 1;
                v.CustomerID = a.CustomerID;
                
                v.Consignee = a.Consignee;
                v.Consignor = a.Consignor;
                v.ConsignorAddress = a.ConsignorAddress1_Building;
                v.ConsignorAddress1 = a.ConsignorAddress2_Street;
                v.ConsignorAddress2 = a.ConsignorAddress3_PinCode;
                v.ConsigneeAddress = a.ConsigneeAddress1_Building;
                v.ConsigneeAddress1 = a.ConsigneeAddress2_Street;
                v.ConsigneeAddress2 = a.ConsigneeAddress3_PinCode;
                v.ConsignorPhone = a.ConsignorPhone;
                v.ConsigneePhone = a.ConsigneePhone;
                v.EmployeeID = a.AssignedEmployeeID;
                v.Remarks = a.Remarks;
                v.CollectedEmpID = a.PickedUpEmpID; ;
                v.CollectedTime = a.PickedupDate; ;
              //  v.ShipmentType = a.ShipmentType;
                if (a.VehicleTypeId != null)
                {
                    v.VehicleTypeId = a.VehicleTypeId.Value;                    
                    //v.vehreq = true;
                }
                //else
                //{
                //    v.vehreq = false;
                //    v.Vehicle = "";
                //}
                v.IsEnquiry = a.IsEnquiry;
                v.ConsigneeContact = a.ConsigneeContact;
                v.ConsignorContact = a.ConsignorContact;
                v.EnteredByID = empid;

                v.ReadyTime = a.PickupReadyTime;
                CustomerMaster cm = db.CustomerMasters.Where(cm1 => cm1.CustomerID == a.CustomerID).FirstOrDefault();
                v.CustomerCode = cm.CustomerCode;
                v.OfficeTimeFrom = cm.OfficeOpenTime;
                v.OfficeTimeTo = cm.OfficeCloseTime;
                v.RequestSource = a.RequestSource;
            }

            //var obj = (from c in db.CityMasters where c.CountryID == a.ConsignerCountryId select c).ToList();
            //ViewBag.City = (from c in db.CityMasters where c.CountryID == a.ConsignerCountryId select c).ToList();
            //ViewBag.CityConsignee = (from c in db.CityMasters where c.CountryID == a.ConsigneeCountryID select c).ToList();

            return View(v);

        }
        public PickupRequestVM GetPickupRequestDetail(int id)
        {
            PickupRequestVM v = new PickupRequestVM();            
            InScanMaster a = db.InScanMasters.Find(id);

                v.InScanID = a.InScanID;
                v.BranchID = a.BranchID;
                v.DepotID = a.DepotID;                
                v.EnquiryID = a.InScanID;
                v.EnquiryNo = a.EnquiryNo;
                v.AWBNo = a.AWBNo;
                v.EnquiryDate = a.PickupRequestDate;
                v.DescriptionID = 4;                
                v.ConsignorCountryName = a.ConsignorCountryName;
                v.ConsignorCityName = a.ConsignorCityName;
                v.ConsignorLocationName = a.ConsignorLocationName;                
                v.ConsigneeCountryName = a.ConsigneeCountryName;
                v.ConsigneeCityName = a.ConsigneeCityName;                
                v.ConsigneeLocationName = a.ConsigneeLocationName;
                
            if (a.Weight!=null)
                    v.Weight = Convert.ToDouble(a.Weight);

                v.AcCompanyID = a.AcCompanyID;
                if (a.CustomerID != null)
                {
                    v.CustomerID = a.CustomerID;
                    string customername = db.CustomerMasters.Find(v.CustomerID).CustomerName;

                    v.CustomerName = customername;
                }
                v.Consignee = a.Consignee;
                v.Consignor = a.Consignor;
                v.ConsignorAddress = a.ConsignorAddress1_Building;
                v.ConsignorAddress1 = a.ConsignorAddress2_Street;
                v.ConsignorAddress2 = a.ConsignorAddress3_PinCode;
                v.ConsigneeAddress = a.ConsigneeAddress1_Building;
                v.ConsigneeAddress1 = a.ConsigneeAddress2_Street;
                v.ConsigneeAddress2 = a.ConsigneeAddress3_PinCode;
                v.ConsignorPhone = a.ConsignorPhone;
                v.ConsigneePhone = a.ConsigneePhone;
            v.ConsignorCountryName = a.ConsignorCountryName;
            v.ConsigneeCountryName = a.ConsigneeCountryName;
            v.ConsignorCityName = a.ConsignorCityName;
            v.ConsigneeCityName = a.ConsigneeCityName;
            
            if (a.VehicleTypeId!=null)
                v.VehicleTypeId = Convert.ToInt32(a.VehicleTypeId);

            //v.PickupRequestStatusId = a.PickupRequestStatusId;
            v.PickupRequestStatusId = a.CourierStatusID;
            v.SubReasonId = a.SubReasonId;
            if (a.AssignedEmployeeID!=null)
                v.EmployeeID = a.AssignedEmployeeID;

                v.Remarks = a.Remarks;
            
            if (a.PickedUpEmpID!=null)
                v.CollectedEmpID = a.PickedUpEmpID; 

                v.CollectedTime = a.PickedupDate; ;
              //  v.ShipmentType = a.ShipmentType;

            if (a.CourierCharge != null)
                v.CourierCharge = a.CourierCharge.Value;
            if (a.MaterialCost != null)
                v.MaterialCost = a.MaterialCost.Value;

            v.Pieces = a.Pieces;
            v.Description = a.CargoDescription;
                //if (a.VehicleID != null)
                //{
                //    v.VehicleID = a.VehicleID.Value;
                //    v.Vehicle = a.Vehicle;
                //    v.vehreq = true;
                //}
                //else
                //{
                //    v.vehreq = false;
                //    v.Vehicle = "";
                //}
                v.IsEnquiry = a.IsEnquiry;
                v.ConsigneeContact = a.ConsigneeContact;
                v.ConsignorContact = a.ConsignorContact;
                v.EnteredByID = a.EnteredByID;
            if (a.PaymentModeId != null)
            { v.PaymentModeId = Convert.ToInt32(a.PaymentModeId); }
                v.ReadyTime = a.PickupReadyTime;
                CustomerMaster cm = db.CustomerMasters.Where(cm1 => cm1.CustomerID == a.CustomerID).FirstOrDefault();
                v.CustomerCode = cm.CustomerCode;
                v.OfficeTimeFrom = cm.OfficeOpenTime;
                v.OfficeTimeTo = cm.OfficeCloseTime;

            if (a.RequestSource!=null)
                v.RequestSource = a.RequestSource;

            if (a.DocumentTypeId != null)
            {
                v.DocumentTypeId = a.DocumentTypeId;
            }
            v.PickupLocation = a.PickupLocation;
            v.PickupLocationPlaceId = a.OriginPlaceID;
            v.DeliveryLocation = a.DeliveryLocation;
            v.DeliveryLocationPlaceId = a.DestinationPlaceID;
            v.PickupSubLocality = a.PickupSubLocality;
            v.DeliverySubLocality = a.DeliverySubLocality;
                
            return v;

         }
        
        [HttpPost]
        public ActionResult Edit(PickupRequestVM v)
        {
            
                CustomerEnquiry _enquiry = new CustomerEnquiry();
                _enquiry.EnquiryID = v.EnquiryID;
                _enquiry.EnquiryNo = v.EnquiryNo;
                _enquiry.EnquiryDate = Convert.ToDateTime(v.EnquiryDate);
                _enquiry.AWBNo = v.AWBNo;
                _enquiry.DescriptionID = 4;
                _enquiry.ConsignerCountryId = v.ConsignerCountryId;
                _enquiry.ConsignerCityId = v.ConsignerCityId;
                _enquiry.ConsignerLocationId = v.ConsignerLocationId;
                _enquiry.ConsigneeCountryID = v.ConsigneeCountryID;
                _enquiry.ConsigneeCityId = v.ConsigneeCityId;
                _enquiry.ConsigneeLocationId = v.ConsigneeLocationId;
                _enquiry.ConsigneeLocationName = v.ConsigneeLocationName;
                _enquiry.ConsignorLocationName = v.ConsignorLocationName;
                _enquiry.Weight = v.Weight;
                _enquiry.AcCompanyID = 1;
                _enquiry.CustomerID = v.CustomerID;
                _enquiry.Consignee = v.Consignee;
                _enquiry.Consignor = v.Consignor;
                _enquiry.ConsignorAddress = v.ConsignorAddress;
                _enquiry.ConsignorAddress1 = v.ConsignorAddress1;
                _enquiry.ConsignorAddress2 = v.ConsignorAddress2;
                _enquiry.ConsigneeAddress = v.ConsigneeAddress;
                _enquiry.ConsigneeAddress1 = v.ConsigneeAddress1;
                _enquiry.ConsigneeAddress2 = v.ConsigneeAddress2;
                _enquiry.ConsignorPhone = v.ConsignorPhone;
                _enquiry.ConsigneePhone = v.ConsigneePhone;
            
                _enquiry.EmployeeID = v.EmployeeID;
                _enquiry.Remarks = v.Remarks;
                _enquiry.CollectedEmpID = v.CollectedEmpID;
                _enquiry.IsEnquiry = false;
                _enquiry.ShipmentType = v.ShipmentType;
            //if (v.vehreq == true)
            //{
            //    _enquiry.Vehicle = v.Vehicle;
            //    _enquiry.VehicleID = v.VehicleID;
            //}
           // _enquiry.Vehicle = v.VehicleTypeId;
                _enquiry.ConsigneeContact = v.ConsigneeContact;
                _enquiry.ConsignorContact = v.ConsignorContact;
                _enquiry.EnteredByID = v.EnteredByID;

                _enquiry.ReadyTime = v.ReadyTime;
                //_enquiry.OfficeTimeFrom = v.OfficeTimeFrom;
                //_enquiry.OfficeTimeTo = v.OfficeTimeTo;
                _enquiry.RequestSource = v.RequestSource;

            if (_enquiry.EmployeeID == null)
                _enquiry.PickupRequestStatusId = 1;
            else if (_enquiry.EmployeeID != null && _enquiry.CollectedEmpID == null)
                _enquiry.PickupRequestStatusId = 2;
            else if (_enquiry.CollectedEmpID != null)
                _enquiry.PickupRequestStatusId = 3;

                db.Entry(_enquiry).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully updated Pickup Request.";
                return RedirectToAction("Index");
            
            //return View();

        }
        
        [HttpPost]
        public JsonResult GetCustomerData(int id)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerID==id select c).FirstOrDefault();


             objCust.CustID = cust.CustomerID;
            objCust.CustCode = cust.CustomerCode;
            objCust.CustName = cust.CustomerName;
            objCust.ContactPerson = cust.ContactPerson;
            objCust.Address1 = cust.Address1;
            objCust.Address2 = cust.Address2;
            objCust.Address3 = cust.Address3;
            objCust.Phone = cust.Phone;
            objCust.CountryName = cust.CountryName;
            objCust.CityName = cust.CityName;
            objCust.LocationName = cust.LocationName;            
            objCust.OfficeOpenTime = cust.OfficeOpenTime.ToString();
            objCust.OfficeCloseTime = cust.OfficeCloseTime.ToString();
            objCust.CustomerType = cust.CustomerType;
            objCust.Email = cust.Email;
            if (cust.Mobile == null)
                objCust.Mobile = "";
            else
                objCust.Mobile = cust.Mobile;
            return Json(objCust, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetCustomerDataByName(string CustomerName)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName == CustomerName select c).FirstOrDefault();

            if (cust!=null){

                objCust.CustID = cust.CustomerID;
                objCust.CustCode = cust.CustomerCode;
                objCust.CustName = cust.CustomerName;
                objCust.ContactPerson = cust.ContactPerson;
                objCust.Address1 = cust.Address1;
                objCust.Address2 = cust.Address2;
                objCust.Address3 = cust.Address3;
                objCust.Phone = cust.Phone;
                objCust.CountryName = cust.CountryName;
                objCust.CityName = cust.CityName;
                objCust.LocationName = cust.LocationName;
                objCust.OfficeOpenTime = cust.OfficeOpenTime.ToString();
                objCust.OfficeCloseTime = cust.OfficeCloseTime.ToString();
                objCust.CustomerType = cust.CustomerType;
                objCust.Email = cust.Email;
                if (cust.Mobile == null)
                    objCust.Mobile = "";
                else
                    objCust.Mobile = cust.Mobile; }
            return Json(objCust, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetConsigneeData(string consigneename)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName == consigneename && c.CustomerType=="CN" select c).FirstOrDefault();

            if (cust != null)
            {
                objCust.CustID = cust.CustomerID;
                objCust.CustCode = cust.CustomerCode;
                objCust.CustName = cust.CustomerName;
                objCust.ContactPerson = cust.ContactPerson;
                objCust.Address1 = cust.Address1;
                objCust.Address2 = cust.Address2;
                objCust.Address3 = cust.Address3;
                objCust.Phone = cust.Phone;
                objCust.CountryName = cust.CountryName;
                objCust.CityName = cust.CityName;
                objCust.LocationName = cust.LocationName;
            }
            return Json(objCust, JsonRequestBehavior.AllowGet);
        }

        public string SaveConsignee(PickupRequestVM v)
        {
            CustM objCust = new CustM();
            var cust = (from c in db.CustomerMasters where c.CustomerName == v.Consignee && c.CustomerType=="CN" select c).FirstOrDefault();

            int accompanyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            if (cust==null)
            {
                CustomerMaster obj= new CustomerMaster();
                
                int max = (from d in db.CustomerMasters orderby d.CustomerID descending select d.CustomerID).FirstOrDefault();

                obj.CustomerID = max + 1;
                obj.AcCompanyID = accompanyid;

                obj.CustomerCode = ""; // _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
                obj.CustomerName = v.Consignee;
                obj.CustomerType = "CN"; //Consignee
                
                obj.ContactPerson =v.ConsigneeContact;
                obj.Address1 = v.ConsigneeAddress;
                obj.Address2 = v.ConsigneeAddress1;
                obj.Address3 = v.ConsigneeAddress2;
                obj.Phone = v.ConsigneePhone;
                obj.CountryName = v.ConsigneeCountryName;
                obj.CityName = v.ConsigneeCityName;
                obj.LocationName = v.ConsigneeLocationName;
                db.CustomerMasters.Add(obj);
                db.SaveChanges();
                
            }


            return "ok";

        }
        public class ChangeStatus
        {

            public int InScanID { get; set; }
            public int ChangeStatusId { get; set; }
            public int AssignedEmployee { get; set; }
            public int PickedUpId { get; set; }
            public DateTime PickedUpDateTime { get; set; }
            public int SubStatusReason { get; set; }

            public string StatusType { get; set; }
        }
      

     

        public class CustM
        {
            public int CityID { get; set; }
            public int LocationID { get; set; }
            public int CountryID { get; set; }
            public string CustName { get; set; }
            public string ContactPerson { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Phone { get; set; }
            public string Mobile { get; set; }
            public string CustCode { get; set; }
            public int CustID { get; set; }
            public string OfficeOpenTime { get; set; }
            public string OfficeCloseTime { get; set; }
            public string CountryName { get; set; }
            public string CityName { get; set; }
            public string LocationName { get; set; }
            public string CustomerType { get; set; }
            public string Email { get; set; }

        }


        public string GetMaxAWBNo()
        {
            string awb = (from c in db.CustomerEnquiries orderby c.AWBNo descending select c.AWBNo).FirstOrDefault();

            if (awb == null)
            {
                return "1";
            }
            else
            {
                int x = Convert.ToInt32(awb);
                x=x+1;

                return x.ToString();
            }
        }


    }
}
