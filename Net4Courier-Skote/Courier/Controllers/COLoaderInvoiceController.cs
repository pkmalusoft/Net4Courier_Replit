using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.Data;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Reflection;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class COLoaderInvoiceController : Controller
    {
        Entities1 db = new Entities1();



        public ActionResult Index()
        {

            AgentInvoiceSearch obj = (AgentInvoiceSearch)Session["AgentInvoiceSearch"];
            AgentInvoiceSearch model = new AgentInvoiceSearch();
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
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                obj = new AgentInvoiceSearch(); 
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.InvoiceNo = "";
                Session["AgentInvoiceSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.InvoiceNo = "";
            }
            else
            {
                model = obj;
                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid).Date;
                Session["AgentInvoiceSearch"] = model;
            }
            List<AgentInvoiceVM> lst = PickupRequestDAO.GetAgentInvoiceList(model.FromDate, model.ToDate,model.InvoiceNo, yearid);
            model.Details = lst;
            
            return View(model);


        }
        [HttpPost]
        public ActionResult Index(AgentInvoiceSearch obj)
        {
            Session["AgentInvoiceSearch"] = obj;
            return RedirectToAction("Index");
        }
       
        public ActionResult InvoiceSearch()
        {

            AgentDatePicker datePicker =(AgentDatePicker)Session["AgentDatePicker"];

            if (datePicker == null)
            {
                datePicker = new AgentDatePicker();
                datePicker.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
                datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MAWB = "";
            }
            if (datePicker != null)
            {                
                
            }
            else
            {
                ViewBag.Customer = new CustmorVM { CustomerID = 0, CustomerName = "" };
            }                                   
            
            
            Session["AgentDatePicker"] =datePicker;
            return View(datePicker);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceSearch(AgentDatePicker picker)
        {
            AgentDatePicker model = new AgentDatePicker
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),                
                CustomerId = picker.CustomerId,
                //MovementId = picker.MovementId,
                CustomerName=picker.CustomerName,
                //SelectedValues = picker.SelectedValues
            };
            
            ViewBag.Token = model;
            Session["AgentDatePicker"] = model;
            return RedirectToAction("Create", "COLoaderInvoice");
           

        }
        public ActionResult Table(AgentInvoiceSearch model)
        {
            return PartialView("Table", model);            

        }
        public ActionResult Create(int id = 0)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            AgentInvoiceVM _custinvoice = new AgentInvoiceVM();
            List<AgentInvoiceDetailVM> _details = new List<AgentInvoiceDetailVM>();
            if (id == 0)
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                _custinvoice.InvoiceDate = CommonFunctions.GetCurrentDateTime();//  myDt;// DateTimeKind. DateTimeOffset.Now.UtcDateTime.AddHours(5.30); // DateTime.Now;            
                _custinvoice.InvoiceNo = _dao.GetMaxAgentInvoiceNo(companyid, branchid, yearid);
                _custinvoice.Details = _details;
                _custinvoice.InvoiceTotal = 0;
                _custinvoice.ChargeableWT = 0;
               
                _custinvoice.OtherCharge = 0;
                _custinvoice.AdminPer = 0;
                _custinvoice.AdminAmt = 0;
                _custinvoice.FuelPer = 0;
                _custinvoice.FuelAmt = 0;


                _custinvoice.InvoiceTotal = 0;
                ViewBag.Title = "Create";
            }
            else
            {
                ViewBag.Title = "Modify";
                var _invoice = db.AgentInvoices.Find(id);

                _custinvoice.AgentInvoiceID = _invoice.AgentInvoiceID;
                _custinvoice.InvoiceDate = _invoice.InvoiceDate;
                _custinvoice.InvoiceNo = _invoice.InvoiceNo;
                _custinvoice.CustomerID = _invoice.CustomerID;
               
                _custinvoice.FuelPer = _invoice.FuelPer;
                _custinvoice.FuelAmt = _invoice.FuelAmt;
                _custinvoice.AdminPer = _invoice.AdminPer;
                _custinvoice.AdminAmt = _invoice.AdminAmt;
                _custinvoice.OtherCharge = _invoice.OtherCharge;
                _custinvoice.ChargeableWT = _invoice.ChargeableWT;
                _custinvoice.InvoiceTotal = _invoice.InvoiceTotal;

                _custinvoice.ClearingCharge = _invoice.ClearingCharge;
                _custinvoice.Discount = _invoice.Discount;

                var customer = db.CustomerMasters.Find(_custinvoice.CustomerID);
                if (customer != null)
                { _custinvoice.CustomerName = customer.CustomerName; }
                _details = (from c in db.AgentInvoiceDetails
                            join ins in db.InboundShipments on c.ShipmentID equals ins.ShipmentID
                            where c.AgentInvoiceID == id
                            select new AgentInvoiceDetailVM
                            {
                                AgentInvoiceDetailID = c.AgentInvoiceDetailID,
                                AgentInvoiceID = c.AgentInvoiceID,
                                AWBNo = c.AWBNo,
                                AWBDateTime = ins.AWBDate,
                                CustomCharge = c.CustomCharge,
                                CourierCharge = c.CourierCharge,
                                OtherCharge = c.OtherCharge,
                                ConsigneeCountryName = ins.ConsigneeCountryName,
                                ConsigneeName = ins.Consignee,
                             
                                ShipmentID= c.ShipmentID,
                                AWBChecked = true,
                                //VATAmount = c.VATAmount,
                                //SurchargePercent = c.SurchargePercent,
                                FuelSurcharge = c.FuelSurcharge,
                                NetValue = c.NetValue
                            }).ToList();

               var  _details1 = (from c in db.AgentInvoiceDetails
                            join ins in db.InScanMasters on c.InscanID equals ins.InScanID
                            where c.AgentInvoiceID == id
                            select new AgentInvoiceDetailVM
                            {
                                AgentInvoiceDetailID = c.AgentInvoiceDetailID,
                                AgentInvoiceID = c.AgentInvoiceID,
                                AWBNo = c.AWBNo,
                                AWBDateTime = ins.TransactionDate,
                                CustomCharge = c.CustomCharge,
                                CourierCharge = c.CourierCharge,
                                OtherCharge = c.OtherCharge,
                                ConsigneeCountryName = ins.ConsigneeCountryName,
                                ConsigneeName = ins.Consignee,

                                InscanID = c.InscanID,
                                AWBChecked = true,
                                //VATAmount = c.VATAmount,
                                //SurchargePercent = c.SurchargePercent,
                                FuelSurcharge = c.FuelSurcharge,
                                NetValue = c.NetValue
                            }).ToList();

                if (_details1!=null)
                {
                    _details.AddRange(_details1);
                }

                _custinvoice.Details= _details;
                _custinvoice.TotalSurcharge = 0;
                _custinvoice.TotalOtherCharge = 0;
                _custinvoice.TotalCourierCharge = 0;
                int _index = 0;
                foreach (var item in _details)
                {
                    //                _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].OtherCharge) + Convert.ToDecimal(_details[_index].VATAmount) + Convert.ToDecimal(;
                    _custinvoice.TotalCourierCharge += Convert.ToDecimal(_details[_index].CourierCharge);
                    _custinvoice.TotalCharges += Convert.ToDecimal(_details[_index].NetValue);
                    _custinvoice.TotalOtherCharge += Convert.ToDecimal(_details[_index].OtherCharge);
                    //_custinvoice.TotalVat += Convert.ToDecimal(_details[_index] );
                    //_custinvoice.TotalSurcharge += Convert.ToDecimal(_details[_index].FuelSurcharge);
                    _index++;
                }
            }

            _custinvoice.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
            _custinvoice.ToDate = CommonFunctions.GetCurrentDateTime().Date; // DateTime.Now.Date;
            _custinvoice.MovementId = "1,2,3,4";

            StatusModel result = AccountsDAO.CheckDateValidate(_custinvoice.InvoiceDate.ToString(), yearid);
            if (result.Status == "YearClose") //Period locked
            {

                ViewBag.Message = result.Message;
                ViewBag.SaveEnable = false;
            }

            Session["InvoiceListing"] = _details;
            return View(_custinvoice);
        }

        [HttpPost]
        public JsonResult SaveCustomerInvoice(AgentInvoiceVM model, string Details)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var IDetails = JsonConvert.DeserializeObject<List<AgentInvoiceDetailVM>>(Details);
            AgentInvoice _custinvoice = new AgentInvoice();
            try
            {


                model.Details = IDetails;
                if (model.AgentInvoiceID == 0)
                {

                 
                    _custinvoice.InvoiceNo = model.InvoiceNo;
                    _custinvoice.InvoiceDate = model.InvoiceDate;
                    _custinvoice.CustomerID = model.CustomerID;
                    //_custinvoice.CustomerInvoiceTax = model.CustomerInvoiceTax;
                    if (model.ChargeableWT.ToString() != "NaN")
                        _custinvoice.ChargeableWT = model.ChargeableWT;
                    _custinvoice.AdminPer = model.AdminPer;
                    _custinvoice.AdminAmt = model.AdminAmt;
                    _custinvoice.FuelPer = model.FuelPer;
                    _custinvoice.FuelAmt = model.FuelAmt;
                    _custinvoice.ClearingCharge = model.ClearingCharge;
                    _custinvoice.OtherCharge = model.OtherCharge;
                  
                    _custinvoice.Discount = model.Discount;
                 
                    _custinvoice.InvoiceTotal = model.InvoiceTotal;
                    
                    _custinvoice.AcFinancialYearID = yearid;
                    _custinvoice.AcCompanyID = companyId;
                    _custinvoice.BranchID = branchid;
                    _custinvoice.CreatedBy = userid;
                    _custinvoice.CreatedDate = CommonFunctions.GetBranchDateTime();
                    _custinvoice.ModifiedBy = userid;
                    _custinvoice.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    db.AgentInvoices.Add(_custinvoice);
                    db.SaveChanges();
                }
                else
                {
                    _custinvoice = db.AgentInvoices.Find(model.AgentInvoiceID);
                    _custinvoice.InvoiceDate = model.InvoiceDate;
                    if (model.ChargeableWT.ToString() != "NaN")
                    _custinvoice.ChargeableWT = model.ChargeableWT;
                    _custinvoice.AdminPer = model.AdminPer;
                    _custinvoice.AdminAmt = model.AdminAmt;
                    _custinvoice.FuelPer = model.FuelPer;
                    _custinvoice.FuelAmt = model.FuelAmt;
                    _custinvoice.Discount = model.Discount;
                    _custinvoice.ClearingCharge = model.ClearingCharge;
                    _custinvoice.OtherCharge = model.OtherCharge;
                    _custinvoice.InvoiceTotal = model.InvoiceTotal;
                   
                    _custinvoice.ModifiedBy = userid;
                    _custinvoice.ModifiedDate = CommonFunctions.GetBranchDateTime();
                    db.Entry(_custinvoice).State = EntityState.Modified;
                    db.SaveChanges();
                    //revert all awb with invoiceid null 
                    //ReceiptDAO.RevertInvoiceIdtoInscanMaster(model.AgentInvoiceID);
                    //delete customer invoicedetails
                    var invoicedetail = db.AgentInvoiceDetails.Where(cc => cc.AgentInvoiceID == model.AgentInvoiceID).ToList();
                    db.AgentInvoiceDetails.RemoveRange(invoicedetail);
                    db.SaveChanges();
                }

              //  List<AgentInvoiceDetailVM> e_Details = model.Details; //  Session["InvoiceListing"] as List<CustomerInvoiceDetailVM>;

                

                if (model.Details != null)
                {
                    DataTable ds = new DataTable();
                    DataSet dt = new DataSet();
                    dt = ToDataTable(IDetails);
                    string xml = dt.GetXml();
                    SaveStatusModel model1 = new SaveStatusModel();
                    model1 = InboundShipmentDAO.SaveColoaderInvoiceBatch(_custinvoice.AgentInvoiceID, xml);
                    //if (model1.Status == "OK")
                    //{
                    //    //Accounts Posting
                    //    PickupRequestDAO _dao = new PickupRequestDAO();
                    //    _dao.GenerateCOLoaderPosting(_custinvoice.AgentInvoiceID);
                    //}
                    //    foreach (var e_details in model.Details)
                    //{
                    //    if (e_details.AWBChecked)
                    //    {
                    //        AgentInvoiceDetail _detail = new AgentInvoiceDetail();
                    //        _detail.AgentInvoiceDetailID = db.AgentInvoiceDetails.Select(x => x.AgentInvoiceDetailID).DefaultIfEmpty(0).Max() + 1;
                    //        _detail.AgentInvoiceID = _custinvoice.AgentInvoiceID;
                    //        _detail.AWBNo = e_details.AWBNo;
                    //        _detail.ShipmentID = e_details.ShipmentID;
                    //        _detail.InscanID = e_details.InscanID;
                    //        _detail.CourierCharge = e_details.CourierCharge;
                    //        _detail.CustomCharge = e_details.CustomCharge;
                    //        _detail.OtherCharge = e_details.OtherCharge;
                    //        _detail.NetValue = e_details.NetValue;
                    //        if (e_details.VATAmount == null)
                    //            _detail.VATAmount = 0;
                    //        else
                    //            _detail.VATAmount = e_details.VATAmount;
                    //        _detail.FuelSurcharge = e_details.FuelSurcharge;
                    //      //  _detail.SurchargePercent = e_details.SurchargePercent;
                    //        db.AgentInvoiceDetails.Add(_detail);
                    //        db.SaveChanges();
                    //        // updating invoiceid in the backend posting proceure
                    //        ////inscan invoice modified
                    //        //InScanMaster _inscan = db.InScanMasters.Find(e_details.InscanID);
                    //        //_inscan.InvoiceID = _custinvoice.CustomerInvoiceID;
                    //        //db.Entry(_inscan).State = EntityState.Modified;
                    //        //db.SaveChanges();


                    //    }
                      

                    //}
                }

              

                if (model.AgentInvoiceID == 0)
                {
                    return Json(new { status = "OK", CustomerInvoiceID = _custinvoice.AgentInvoiceID, message = "CO-Loader Invoice Added Succesfully!" });
                }
                else
                {
                    return Json(new { status = "OK", CustomerInvoiceID = _custinvoice.AgentInvoiceID, message = "CO-Loader Invoice Updated Succesfully!" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = "OK", CustomerInvoiceID = 0, message = ex.Message });
            }
        }

      

        [HttpPost]
        public ActionResult ShowItemList(int CustomerId,string FromDate,string ToDate,string MAWB, int[] MovementId)
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
            AgentInvoiceVM vm = new AgentInvoiceVM();
            vm.Details = PickupRequestDAO.GetAgentShipmentList(CustomerId, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), MAWB,pMovementId);
            
            vm.InvoiceTotal = vm.Details.Sum(cc => cc.NetValue).Value;
            vm.CustomerID = CustomerId;
            Session["InvoiceListing"] = vm.Details;
           
            return PartialView("InvoiceList", vm);

        }
        [HttpPost]
        public ActionResult ShowItemListSelect(string AWBNo, bool statuschecked)
        {
            AgentInvoiceVM vm = new AgentInvoiceVM();
            try
            {
                vm.Details= new List<AgentInvoiceDetailVM>();
                var Details1 = new List<AgentInvoiceDetailVM>();
                var details = (List<AgentInvoiceDetailVM>)Session["InvoiceListing"];
                if (details != null)
                {
                    if (AWBNo == "")
                    {
                        for (int i = 0; i < details.Count; i++)
                        {
                            if (details[i].AWBNo == AWBNo || AWBNo == "")
                                details[i].AWBChecked = statuschecked;


                            Details1.Add(details[i]);
                        }

                        details = new List<AgentInvoiceDetailVM>();
                        details.AddRange(Details1);
                    }
                    else
                    {


                        var searchitem = details.Where(cc => cc.AWBNo == AWBNo).FirstOrDefault();

                        details.Remove(searchitem);
                        searchitem.AWBChecked = statuschecked;
                        details.Add(searchitem);
                    }
                    Details1 = details.Where(cc => cc.AWBChecked == true).ToList();
                    var Details2 = details.Where(cc => cc.AWBChecked == false).ToList();
                    Details1.AddRange(Details2);

                    //for (int i = 0; i < details.Count; i++)
                    //{
                    //    if (details[i].AWBNo == AWBNo || AWBNo == "")
                    //        details[i].AWBChecked = statuschecked;

                    //    if (details[i].AWBChecked == true)
                    //        Details1.Add(details[i]);
                    //}
                    //for (int i = 0; i < details.Count; i++)
                    //{

                    //    if (details[i].AWBChecked == false)
                    //        Details1.Add(details[i]);
                    //}

                    vm.Details = Details1;
                    Session["InvoiceListing"] = Details1;
                }

                return PartialView("InvoiceList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }




        [HttpPost]
        public ActionResult GetCustomerAWBList(int Id)
        {
            DatePicker datePicker = SessionDataModel.GetTableVariable();
            List<CustomerInvoiceDetailVM> _details = new List<CustomerInvoiceDetailVM>();
            _details = (from c in db.InScanMasters
                        where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                        && (c.InvoiceID == null || c.InvoiceID==0)
                        && c.PaymentModeId == 3 //account
                        && c.IsDeleted==false
                        && c.CustomerID == Id
                        select new CustomerInvoiceDetailVM
                        {
                            AWBNo = c.AWBNo,
                            AWBDateTime=c.TransactionDate,
                            ConsigneeName = c.Consignee,
                            ConsigneeCountryName = c.ConsigneeCountryName,
                            CourierCharge = c.CourierCharge,
                            CustomCharge = c.CustomsValue == null ? 0 : c.CustomsValue,
                            OtherCharge = c.OtherCharge == null ? 0 : c.OtherCharge,
                            //StatusPaymentMode = c.StatusPaymentMode,
                            InscanID = c.InScanID,
                            MovementId = c.MovementID == null ? 0 : c.MovementID.Value,AWBChecked=true
                        }).ToList().Where(tt => tt.MovementId != null).ToList().Where(cc => datePicker.SelectedValues.ToList().Contains(cc.MovementId.Value)).ToList();
            
          

            int _index = 0;
            CustomerInvoiceVM customerInvoice = new CustomerInvoiceVM();
            foreach (var item in _details)
                {
                    _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].CustomCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                    customerInvoice.TotalCharges += _details[_index].TotalCharges;
                    _index++;
                }


            if (customerInvoice.CustomerInvoiceTax != 0)
            {
                customerInvoice.ChargeableWT = (Convert.ToDouble(customerInvoice.TotalCharges) * (Convert.ToDouble(customerInvoice.CustomerInvoiceTax) / Convert.ToDouble(100.00)));

                customerInvoice.AdminAmt = (Convert.ToDecimal(customerInvoice.TotalCharges) * (Convert.ToDecimal(customerInvoice.AdminPer) / Convert.ToDecimal(100)));

                customerInvoice.FuelAmt = (Convert.ToDecimal(customerInvoice.TotalCharges) * (Convert.ToDecimal(customerInvoice.FuelPer) / Convert.ToDecimal(100)));

                customerInvoice.InvoiceTotal = Convert.ToDecimal(customerInvoice.TotalCharges) + Convert.ToDecimal(customerInvoice.ChargeableWT) + customerInvoice.AdminAmt + customerInvoice.FuelAmt;
            }

            customerInvoice.CustomerInvoiceDetailsVM = _details;

            Session["InvoiceListing"] = _details;

            return PartialView("InvoiceList", customerInvoice);

        }

        [HttpGet]
        public JsonResult GetCourierType()
        {
            var lstcourier=db.CourierMovements.ToList();
            return Json(new { data = lstcourier }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetTotalCharge(AgentInvoiceVM customerinvoice)
        {
            int _index = 0;
            List<AgentInvoiceDetailVM> _details = customerinvoice.Details;
            customerinvoice.InvoiceTotal = 0;
            //List<CustomerInvoiceDetailVM> _details = Session["InvoiceListing"] as List<CustomerInvoiceDetailVM>;
            if (_details != null)
            {
                foreach (var item in _details)
                {
                    if (item.AWBChecked)
                    {

                        _details[_index].NetValue = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].CustomCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                        customerinvoice.InvoiceTotal += _details[_index].NetValue;
                    }
                    _index++;

                }
                if (customerinvoice.OtherCharge==null)
                {
                    customerinvoice.OtherCharge = 0;
                }
                //if (customerinvoice.CustomerInvoiceTax==null)
                //{ 
                //    customerinvoice.CustomerInvoiceTax = 0; 
                //}

                //if (customerinvoice.AdminPer ==null)
                //{
                //    customerinvoice.AdminPer = 0;
                //}
                //if (customerinvoice.FuelPer == null)
                //{
                //    customerinvoice.FuelPer = 0;
                //}

                //if (customerinvoice.InvoiceTax != 0)
                //{
                //    customerinvoice.ChargeableWT = (Convert.ToDouble(customerinvoice.TotalCharges) * (Convert.ToDouble(customerinvoice.CustomerInvoiceTax) / Convert.ToDouble(100.00)));
                //}
                //else
                //{
                //    customerinvoice.ChargeableWT = 0;
                //}
                //if (customerinvoice.AdminPer != 0) { 
                //    customerinvoice.AdminAmt = (Convert.ToDecimal(customerinvoice.TotalCharges) * (Convert.ToDecimal(customerinvoice.AdminPer) / Convert.ToDecimal(100)));
                //}
                //else
                //{
                //    customerinvoice.AdminAmt = 0;
                //}
            //if (customerinvoice.FuelPer != 0)
            //{ 
            //    customerinvoice.FuelAmt = (Convert.ToDecimal(customerinvoice.TotalCharges) * (Convert.ToDecimal(customerinvoice.FuelPer) / Convert.ToDecimal(100)));
            //}
            //else
            //{
            //    customerinvoice.FuelAmt = 0;
            //}

            // customerinvoice.InvoiceTotal = Convert.ToDecimal(customerinvoice.TotalCharges) + Convert.ToDecimal(customerinvoice.ChargeableWT) + customerinvoice.AdminAmt + customerinvoice.FuelAmt + customerinvoice.OtherCharge;
                
            }
            return Json(new { data=customerinvoice  }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCoLoaderInvoice(id);
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

            //CustomerInvoice a = db.CustomerInvoices.Find(id);
            //if (a == null)  
            //{
            //    return HttpNotFound();
            //}
            //else
            //{
            //    var _inscans = db.InScanMasters.Where(cc => cc.InvoiceID == id).ToList();
            //    foreach(InScanMaster _inscan in _inscans)
            //    {
            //        _inscan.InvoiceID = null;
            //        db.Entry(_inscan).State = EntityState.Modified;
            //        db.SaveChanges();
            //    }
            //    a.IsDeleted = true;
            //    db.Entry(a).State = EntityState.Modified;
            //    db.SaveChanges();
            //    TempData["SuccessMsg"] = "You have successfully deleted Pickup Request.";


            //    return RedirectToAction("Index");
            //}
        }
        public ActionResult Details(int id)        
        {
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            var _invoice = db.AgentInvoices.Find(id);
            AgentInvoiceVM _custinvoice = new AgentInvoiceVM();
            _custinvoice.AgentInvoiceID = _invoice.AgentInvoiceID;
            _custinvoice.InvoiceDate = _invoice.InvoiceDate;
            _custinvoice.InvoiceNo = _invoice.InvoiceNo;
            _custinvoice.CustomerID = _invoice.CustomerID;
            _custinvoice.InvoiceTax = _invoice.InvoiceTax;
            _custinvoice.FuelPer = _invoice.FuelPer;
            _custinvoice.FuelAmt = _invoice.FuelAmt;
            _custinvoice.AdminPer = _invoice.AdminPer;
            _custinvoice.AdminAmt = _invoice.AdminAmt;
            _custinvoice.OtherCharge = _invoice.OtherCharge;
            _custinvoice.ChargeableWT = _invoice.ChargeableWT;
            _custinvoice.InvoiceTotal = _invoice.InvoiceTotal;

            List<AgentInvoiceDetailVM> _details = new List<AgentInvoiceDetailVM>();
            _details = (from c in db.AgentInvoiceDetails
                        join ins in db.InboundShipments on c.ShipmentID equals ins.ShipmentID
                        where c.AgentInvoiceID == id
                        select new AgentInvoiceDetailVM
                        {
                            AgentInvoiceDetailID = c.AgentInvoiceDetailID,
                            AgentInvoiceID = c.AgentInvoiceID,
                            AWBNo = c.AWBNo,
                            AWBDateTime = ins.AWBDate,
                            CustomCharge = c.CustomCharge,
                            CourierCharge = c.CourierCharge,
                            OtherCharge = c.OtherCharge,
                            ConsigneeCountryName = ins.ConsigneeCountryName,
                            ConsigneeName = ins.Consignee,
                           
                           ShipmentID= c.ShipmentID,
                            AWBChecked = true
                        }).ToList();

            int _index = 0;

            foreach (var item in _details)
            {
                _details[_index].NetValue = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].CustomCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                _custinvoice.InvoiceTotal += _details[_index].NetValue;
                _index++;
            }

            _custinvoice.Details = _details;

            Session["InvoiceListing"] = _details;
            return View(_custinvoice);            

        }

        public ActionResult InvoicePrint(int id,string output="PDF")
        {
            ViewBag.ReportName = "CO Loader Invoice Printing";              
            ViewBag.ReportId=id;
            string filepath=AccountsReportsDAO.COLoaderInvoiceReport(id,output);
            
            if (output != "PDF")
            {
                return RedirectToAction("DownloadFile", "COLoader", new { filePath = filepath });
            }
            else
            {
                return View();
            }            
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
        public FileResult DownloadFile(int id)
        {
            string filepath = AccountsReportsDAO.COLoaderInvoiceReport(id, "EXCEL");
            string filename = "AgentInvoiceReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx"; // Server.MapPath("~" + filePath);

            byte[] fileBytes = GetFile(filepath);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }
        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.CustomerID > 0 && c1.CustomerType == "CL" && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(100).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.CustomerID > 0 && c1.CustomerType == "CL"  
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(100).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);

            }
        }

         

     
    }
}
