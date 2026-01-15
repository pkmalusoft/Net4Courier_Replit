using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.Data;
using System.Data.Entity;
using System.Text;
using System.IO;
using System.Web.UI;
using ClosedXML.Excel;
using System.Xml;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class CODInvoiceController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index()
        {

            CODInvoiceSearch obj = (CODInvoiceSearch)Session["CODInvoiceSearch"];
            CODInvoiceSearch model = new CODInvoiceSearch();
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
                obj = new CODInvoiceSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.InvoiceNo = "";
                Session["CODInvoiceSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.InvoiceNo = "";
            }
            else
            {
                model = obj;
            }
            List<CODInvoiceVM> lst = PickupRequestDAO.GetCODInvoiceList(obj.FromDate, obj.ToDate, model.InvoiceNo, yearid);
            model.Details = lst;

            return View(model);


        }
        [HttpPost]
        public ActionResult Index(CODInvoiceSearch obj)
        {
            Session["CODInvoiceSearch"] = obj;
            return RedirectToAction("Index");
        }

        public ActionResult InvoiceSearch()
        {

            DatePicker datePicker = SessionDataModel.GetTableVariable();

            if (datePicker == null)
            {
                datePicker = new DatePicker();
                datePicker.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
                datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MovementId = "1,2,3,4";
            }
            if (datePicker != null)
            {
                //ViewBag.Customer = (from c in db.InScanMasters
                //                    join cust in db.CustomerMasters on c.CustomerID equals cust.CustomerID
                //                    where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                //                    select new CustmorVM { CustomerID = cust.CustomerID, CustomerName = cust.CustomerName }).Distinct();

                ViewBag.Customer = (from c in db.CustomerMasters where c.StatusActive == true select new CustmorVM { CustomerID = c.CustomerID, CustomerName = c.CustomerName }).ToList();
                if (datePicker.MovementId == null)
                    datePicker.MovementId = "1,2,3,4";
            }
            else
            {
                ViewBag.Customer = new CustmorVM { CustomerID = 0, CustomerName = "" };
            }


            //ViewBag.Movement = new MultiSelectList(db.CourierMovements.ToList(),"MovementID","MovementType");
            ViewBag.Movement = db.CourierMovements.ToList();

            ViewBag.Token = datePicker;
            SessionDataModel.SetTableVariable(datePicker);
            return View(datePicker);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoiceSearch([Bind(Include = "FromDate,ToDate,CustomerId,MovementId,SelectedValues,CustomerName")] DatePicker picker)
        {
            DatePicker model = new DatePicker
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Delete = true, // (bool)Token.Permissions.Deletion,
                Update = true, //(bool)Token.Permissions.Updation,
                Create = true, //.ToStrin//(bool)Token.Permissions.Creation
                CustomerId = picker.CustomerId,
                MovementId = picker.MovementId,
                CustomerName = picker.CustomerName,
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
            SessionDataModel.SetTableVariable(model);
            return RedirectToAction("Create", "CustomerInvoice");
            //return PartialView("InvoiceSearch",model);

        }
        public ActionResult Table(CODInvoiceSearch model)
        {
            return PartialView("Table", model);
            //int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            //int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());

            //DatePicker datePicker = SessionDataModel.GetTableVariable();
            //ViewBag.Token = datePicker;

            //List<CustomerInvoiceVM> _Invoices = (from c in db.CustomerInvoices
            //                                     join cust in db.CustomerMasters on c.CustomerID equals cust.CustomerID
            //                                     where (c.InvoiceDate >= datePicker.FromDate && c.InvoiceDate < datePicker.ToDate)
            //                                     && c.IsDeleted==false
            //                                     orderby c.InvoiceDate descending
            //                                     select new CustomerInvoiceVM
            //                                     {
            //                                         CustomerInvoiceID = c.CustomerInvoiceID,
            //                                         CustomerInvoiceNo = c.CustomerInvoiceNo,
            //                                         InvoiceDate = c.InvoiceDate,
            //                                         CustomerID = c.CustomerID,
            //                                         CustomerName = cust.CustomerName,
            //                                         InvoiceTotal=c.InvoiceTotal

            //                                     }).ToList();

            //return View("Table", _Invoices);

        }
        public ActionResult Create()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            CODInvoiceVM _custinvoice = new CODInvoiceVM();
            PickupRequestDAO _dao = new PickupRequestDAO();
            _custinvoice.InvoiceDate = CommonFunctions.GetCurrentDateTime();//  myDt;// DateTimeKind. DateTimeOffset.Now.UtcDateTime.AddHours(5.30); // DateTime.Now;            
            _custinvoice.InvoiceNo = _dao.GetMaxCODInvoiceNo(companyid, branchid, yearid);

            List<CODInvoiceDetailVM> _details = new List<CODInvoiceDetailVM>();
            _custinvoice.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
            _custinvoice.ToDate = CommonFunctions.GetCurrentDateTime().Date; // DateTime.Now.Date;
            _custinvoice.MovementId = "1,2,3,4";
            _custinvoice.InvoiceTotal = 0;

            _custinvoice.Discount = 0;

            _custinvoice.InvoiceTotal = _custinvoice.InvoiceTotal;

            _custinvoice.Details = _details;

            Session["CODInvoiceListing"] = _details;
            return View(_custinvoice);
        }


        [HttpPost]
        public ActionResult ShowItemList(string CustomerName, string FromDate, string ToDate, int[] MovementId,string Type)
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
            CODInvoiceVM _custinvoice = new CODInvoiceVM();
            _custinvoice.Details = PickupRequestDAO.GetCODAWBforInvoice(CustomerName, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), pMovementId,Type);
            Session["CODInvoiceListing"] = _custinvoice.Details;
            _custinvoice.CustomerName = CustomerName;
            return PartialView("InvoiceList", _custinvoice);
        }

        [HttpPost]
        public ActionResult ShowItemListSelect(string AWBNo, bool statuschecked)
        {
            CODInvoiceVM vm = new CODInvoiceVM();
            try
            {
                vm.Details = new List<CODInvoiceDetailVM>();
                var Details1 = new List<CODInvoiceDetailVM>();
                var details = (List<CODInvoiceDetailVM>)Session["CODInvoiceListing"];
                if (details != null)
                {
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].AWBNo == AWBNo || AWBNo == "")
                            details[i].AWBChecked = statuschecked;

                        if (details[i].AWBChecked == true)
                            Details1.Add(details[i]);
                    }
                    for (int i = 0; i < details.Count; i++)
                    {

                        if (details[i].AWBChecked == false)
                            Details1.Add(details[i]);
                    }

                    vm.Details = Details1;
                    Session["CODInvoiceListing"] = Details1;
                }

                return PartialView("InvoiceList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CODInvoiceVM model)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var userid = Convert.ToInt32(Session["UserID"]);
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (model.CODInvoiceID == 0)
            {
                CODInvoice _custinvoice = new CODInvoice();
                var max = db.CustomerInvoices.Select(x => x.CustomerInvoiceID).DefaultIfEmpty(0).Max() + 1;
                _custinvoice.CODInvoiceID = max;
                _custinvoice.InvoiceNo = model.InvoiceNo;
                _custinvoice.InvoiceDate = model.InvoiceDate;
                _custinvoice.CustomerID = model.CustomerID;
                _custinvoice.CustomerName = model.CustomerName;
                _custinvoice.InvoiceTotal = model.InvoiceTotal;
                _custinvoice.Discount = model.Discount;
                _custinvoice.AcFinancialYearID = yearid;
                _custinvoice.AcCompanyID = companyId;
                _custinvoice.BranchID = branchid;
                _custinvoice.CreatedBy = userid;
                _custinvoice.CreatedDate = CommonFunctions.GetCurrentDateTime();
                _custinvoice.ModifiedBy = userid;
                _custinvoice.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                db.CODInvoices.Add(_custinvoice);
                db.SaveChanges();

                List<CODInvoiceDetailVM> e_Details = model.Details; //  Session["CODInvoiceListing"] as List<CustomerInvoiceDetailVM>;

                model.Details = e_Details;

                if (model.Details != null)
                {

                    foreach (var e_details in model.Details)
                    {
                        if (e_details.CODInvoiceDetailID == 0 && e_details.AWBChecked)
                        {
                            CODInvoiceDetail _detail = new CODInvoiceDetail();
                            _detail.CODInvoiceDetailID = db.CustomerInvoiceDetails.Select(x => x.CustomerInvoiceDetailID).DefaultIfEmpty(0).Max() + 1;
                            _detail.CODInvoiceID = _custinvoice.CODInvoiceID;
                            _detail.AWBNo = e_details.AWBNo;
                            _detail.InScanID = e_details.InScanID;
                            _detail.CourierCharge = e_details.CourierCharge;
                            _detail.CustomCharge = e_details.CustomCharge;
                            _detail.OtherCharge = e_details.OtherCharge;
                            _detail.NetValue = e_details.NetValue;
                            _detail.VATAmount = e_details.VATAmount;
                            _detail.FuelSurcharge = e_details.FuelSurcharge;
                            _detail.SurchargePercent = e_details.SurchargePercent;
                            db.CODInvoiceDetails.Add(_detail);
                            db.SaveChanges();

                            ////inscan invoice modified
                            InScanMaster _inscan = db.InScanMasters.Find(e_details.InScanID);
                            _inscan.CODInvoiceID = _custinvoice.CODInvoiceID;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();


                        }

                    }
                }


                TempData["SuccessMsg"] = "You have successfully Saved the COD Invoice";
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            var _invoice = db.CODInvoices.Find(id);
            CODInvoiceVM _custinvoice = new CODInvoiceVM();
            _custinvoice.CODInvoiceID = _invoice.CODInvoiceID;
            _custinvoice.InvoiceDate = _invoice.InvoiceDate;
            _custinvoice.InvoiceNo = _invoice.InvoiceNo;
            _custinvoice.CustomerID = _invoice.CustomerID;
            _custinvoice.CustomerName = _invoice.CustomerName;
            _custinvoice.InvoiceTotal = _invoice.InvoiceTotal;
            _custinvoice.Discount = _invoice.Discount;
            
            List<CODInvoiceDetailVM> _details = new List<CODInvoiceDetailVM>();
            _details = (from c in db.CODInvoiceDetails
                        join ins in db.InScanMasters on c.InScanID equals ins.InScanID
                        where c.CODInvoiceID == id
                        select new CODInvoiceDetailVM
                        {
                            CODInvoiceDetailID = c.CODInvoiceDetailID,
                            CODInvoiceID = c.CODInvoiceID,
                            AWBNo = c.AWBNo,
                            AWBDateTime = ins.TransactionDate,
                            CustomCharge = c.CustomCharge,
                            CourierCharge = c.CourierCharge,
                            OtherCharge = c.OtherCharge,
                            ConsigneeCountryName = ins.ConsigneeCountryName,
                            ConsigneeName = ins.Consignee,
                            //StatusPaymentMode = c.StatusPaymep[0 ntMode,
                            InScanID = c.InScanID,
                            AWBChecked = true,
                            VATAmount = c.VATAmount,
                            SurchargePercent = c.SurchargePercent,
                            FuelSurcharge = c.FuelSurcharge,
                            NetValue = c.NetValue
                        }).ToList();

            int _index = 0;
            
            //foreach (var item in _details)
            //{
            //    //                _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].OtherCharge) + Convert.ToDecimal(_details[_index].VATAmount) + Convert.ToDecimal(;
            //    _custinvoice.TotalCourierCharge += Convert.ToDecimal(_details[_index].CourierCharge);
            //    _custinvoice.TotalCharges += Convert.ToDecimal(_details[_index].NetValue);
            //    _custinvoice.TotalOtherCharge += Convert.ToDecimal(_details[_index].OtherCharge);
            //    _custinvoice.TotalVat += Convert.ToDecimal(_details[_index].VATAmount);
            //    _custinvoice.TotalSurcharge += Convert.ToDecimal(_details[_index].FuelSurcharge);
            //    _index++;
            //}

            _custinvoice.Details = _details;

            Session["CODInvoiceListing"] = _details;
            return View(_custinvoice);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CODInvoiceVM model)
        {
            var userid = Convert.ToInt32(Session["UserID"]);

            if (model.CODInvoiceID > 0)
            {
                CODInvoice _custinvoice = new CODInvoice();
                _custinvoice = db.CODInvoices.Find(model.CODInvoiceID);
                _custinvoice.InvoiceDate = model.InvoiceDate;
                //_custinvoice.CustomerInvoiceTax = model.CustomerInvoiceTax;                
                _custinvoice.InvoiceTotal = model.InvoiceTotal;
                _custinvoice.Discount = model.Discount;
                _custinvoice.ModifiedBy = userid;
                _custinvoice.ModifiedDate = CommonFunctions.GetCurrentDateTime();
                db.Entry(_custinvoice).State = EntityState.Modified;
                db.SaveChanges();

                List<CODInvoiceDetailVM> e_Details = model.Details; //  Session["CODInvoiceListing"] as List<CustomerInvoiceDetailVM>;

                model.Details = e_Details;

                if (model.Details != null)
                {

                    foreach (var e_details in model.Details)
                    {
                        if (e_details.CODInvoiceDetailID == 0 && e_details.AWBChecked)
                        {
                            CODInvoiceDetail _detail = new CODInvoiceDetail();
                            _detail.CODInvoiceDetailID = db.CODInvoiceDetails.Select(x => x.CODInvoiceDetailID).DefaultIfEmpty(0).Max() + 1;
                            _detail.CODInvoiceID = _custinvoice.CODInvoiceID;
                            _detail.AWBNo = e_details.AWBNo;
                            _detail.InScanID = e_details.InScanID;
                            
                            _detail.CourierCharge = e_details.CourierCharge;
                            _detail.CustomCharge = e_details.CustomCharge;
                            _detail.OtherCharge = e_details.OtherCharge;
                            db.CODInvoiceDetails.Add(_detail);
                            db.SaveChanges();

                            //inscan invoice modified
                            InScanMaster _inscan = db.InScanMasters.Find(e_details.InScanID);
                            _inscan.CODInvoiceID = _custinvoice.CODInvoiceID;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();


                        }
                        else if (e_details.CODInvoiceDetailID == 0 && e_details.AWBChecked)
                        {
                            //CustomerInvoiceDetail _detail = new CustomerInvoiceDetail();
                            //_detail = db.CustomerInvoiceDetails.Find(e_details.CustomerInvoiceID);                            
                            //_detail.CourierCharge = e_details.CourierCharge;
                            //_detail.CustomCharge = e_details.CustomCharge;
                            //_detail.OtherCharge = e_details.OtherCharge;
                            //db.CustomerInvoiceDetails.Add(_detail);
                            //db.SaveChanges();

                            ////inscan invoice modified
                            //InScanMaster _inscan = db.InScanMasters.Find(e_details.InscanID);
                            //_inscan.InvoiceID = _custinvoice.CustomerInvoiceID;
                            //db.Entry(_inscan).State = System.Data.EntityState.Modified;
                            //db.SaveChanges();
                        }
                        else if (e_details.CODInvoiceDetailID > 0 && e_details.AWBChecked == false)
                        {
                            CODInvoiceDetail _detail = new CODInvoiceDetail();
                            _detail = db.CODInvoiceDetails.Find(e_details.CODInvoiceDetailID);
                            db.CODInvoiceDetails.Remove(_detail);
                            db.SaveChanges();
                            ////inscan invoice modified
                            InScanMaster _inscan = db.InScanMasters.Find(e_details.InScanID);
                            _inscan.CODInvoiceID = null;
                            db.Entry(_inscan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                //Accounts Posting
                PickupRequestDAO _dao = new PickupRequestDAO();
                _dao.GenerateInvoicePosting(_custinvoice.CODInvoiceID);

                TempData["SuccessMsg"] = "You have successfully Updated the Customer Invoice";
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public ActionResult GetCustomerAWBList(int Id)
        {
            DatePicker datePicker = SessionDataModel.GetTableVariable();
            List<CustomerInvoiceDetailVM> _details = new List<CustomerInvoiceDetailVM>();
            _details = (from c in db.InScanMasters
                        where (c.TransactionDate >= datePicker.FromDate && c.TransactionDate < datePicker.ToDate)
                        && (c.InvoiceID == null || c.InvoiceID == 0)
                        && c.PaymentModeId == 3 //account
                        && c.IsDeleted == false
                        && c.CustomerID == Id
                        select new CustomerInvoiceDetailVM
                        {
                            AWBNo = c.AWBNo,
                            AWBDateTime = c.TransactionDate,
                            ConsigneeName = c.Consignee,
                            ConsigneeCountryName = c.ConsigneeCountryName,
                            CourierCharge = c.CourierCharge,
                            CustomCharge = c.CustomsValue == null ? 0 : c.CustomsValue,
                            OtherCharge = c.OtherCharge == null ? 0 : c.OtherCharge,
                            VATAmount = c.TaxAmount == null ? 0 : c.TaxAmount,
                            FuelSurcharge = c.SurchargeAmount == null ? 0 : c.SurchargeAmount,
                            //StatusPaymentMode = c.StatusPaymentMode,
                            InscanID = c.InScanID,
                            MovementId = c.MovementID == null ? 0 : c.MovementID.Value,
                            AWBChecked = true
                        }).ToList().Where(tt => tt.MovementId != null).ToList().Where(cc => datePicker.SelectedValues.ToList().Contains(cc.MovementId.Value)).ToList();



            int _index = 0;
            CustomerInvoiceVM customerInvoice = new CustomerInvoiceVM();
            foreach (var item in _details)
            {
                _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].VATAmount) + Convert.ToDecimal(_details[_index].OtherCharge) + Convert.ToDecimal(_details[_index].FuelSurcharge);
                customerInvoice.TotalCharges += _details[_index].TotalCharges;
                customerInvoice.FuelAmt += _details[_index].FuelSurcharge;
                customerInvoice.ChargeableWT += Convert.ToDouble(_details[_index].VATAmount);
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

            Session["CODInvoiceListing"] = _details;

            return PartialView("InvoiceList", customerInvoice);

        }



        [HttpGet]
        public JsonResult GetInvoiceReceipt(int InvoiceId, int CustomerId)
        {
            int receiptid = PickupRequestDAO.CheckInvoiceReceipt(InvoiceId, CustomerId);
            return Json(receiptid, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCourierType()
        {
            var lstcourier = db.CourierMovements.ToList();
            return Json(new { data = lstcourier }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetTotalCharge(CustomerInvoiceVM customerinvoice)
        {
            int _index = 0;
            List<CustomerInvoiceDetailVM> _details = customerinvoice.CustomerInvoiceDetailsVM;

            //List<CustomerInvoiceDetailVM> _details = Session["CODInvoiceListing"] as List<CustomerInvoiceDetailVM>;
            if (_details != null)
            {
                customerinvoice.FuelAmt = 0;
                customerinvoice.TotalCharges = 0;
                customerinvoice.ChargeableWT = 0;
                customerinvoice.OtherCharge = 0;
                customerinvoice.TotalCourierCharge = 0;
                foreach (var item in _details)
                {

                    if (item.AWBChecked)
                    {

                        _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].VATAmount) + Convert.ToDecimal(_details[_index].FuelSurcharge) + Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                        customerinvoice.TotalCharges += _details[_index].TotalCharges;
                        customerinvoice.FuelAmt += _details[_index].FuelSurcharge;
                        customerinvoice.ChargeableWT += Convert.ToDouble(_details[_index].VATAmount);
                        customerinvoice.OtherCharge += _details[_index].OtherCharge;
                        customerinvoice.TotalCourierCharge += Convert.ToDecimal(_details[_index].CourierCharge);
                    }
                    _index++;

                }
                //if (customerinvoice.OtherCharge == null)
                //{
                //    customerinvoice.OtherCharge = 0;
                //}
                //if (customerinvoice.CustomerInvoiceTax == null)
                //{
                //    customerinvoice.CustomerInvoiceTax = 0;
                //}

                if (customerinvoice.AdminPer == null)
                {
                    customerinvoice.AdminPer = 0;
                    customerinvoice.AdminAmt = 0;
                }
                else
                {
                    customerinvoice.AdminAmt = customerinvoice.TotalCharges * (customerinvoice.AdminPer / 100);
                }
                //if (customerinvoice.FuelPer == null)
                //{
                //    customerinvoice.FuelPer = 0;
                //}

                //if (customerinvoice.CustomerInvoiceTax != 0)
                //{
                //    customerinvoice.ChargeableWT = (Convert.ToDouble(customerinvoice.TotalCharges) * (Convert.ToDouble(customerinvoice.CustomerInvoiceTax) / Convert.ToDouble(100.00)));
                //}
                //else
                //{
                //    customerinvoice.ChargeableWT = 0;
                //}
                if (customerinvoice.AdminPer != 0)
                {
                    customerinvoice.AdminAmt = (Convert.ToDecimal(customerinvoice.TotalCharges) * (Convert.ToDecimal(customerinvoice.AdminPer) / Convert.ToDecimal(100)));
                }
                else
                {
                    customerinvoice.AdminAmt = 0;
                }
                //if (customerinvoice.FuelPer != 0)
                //{
                //    customerinvoice.FuelAmt = (Convert.ToDecimal(customerinvoice.TotalCharges) * (Convert.ToDecimal(customerinvoice.FuelPer) / Convert.ToDecimal(100)));
                //}
                //else
                //{
                //    customerinvoice.FuelAmt = 0;
                //}
                if (customerinvoice.ClearingCharge == null)
                    customerinvoice.ClearingCharge = 0;
                customerinvoice.InvoiceTotal = Convert.ToDecimal(customerinvoice.TotalCharges) + customerinvoice.AdminAmt + customerinvoice.ClearingCharge - customerinvoice.Discount;

            }
            return Json(new { data = customerinvoice }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCODInvoice(id);
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
        public ActionResult Details(int id)
        {
            ViewBag.Customer = db.CustomerMasters.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            var _invoice = db.CustomerInvoices.Find(id);
            CustomerInvoiceVM _custinvoice = new CustomerInvoiceVM();
            _custinvoice = ReceiptDAO.CustomerInvoiceDetail(id);

            string imgPath = Server.MapPath("~/Content/ClientLogo/ClientLogo.png");
            // Convert image to byte array  
            byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
            //Convert byte arry to base64string   
            string imreBase64Data = Convert.ToBase64String(byteData);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view  
            ViewBag.ImageData = imgDataURL;

            List<CustomerInvoiceDetailVM> _details = new List<CustomerInvoiceDetailVM>();
            _details = (from c in db.CustomerInvoiceDetails
                        join ins in db.InScanMasters on c.InscanID equals ins.InScanID
                        where c.CustomerInvoiceID == id
                        select new CustomerInvoiceDetailVM
                        {
                            CustomerInvoiceDetailID = c.CustomerInvoiceDetailID,
                            CustomerInvoiceID = c.CustomerInvoiceID,
                            AWBNo = c.AWBNo,
                            AWBDateTime = ins.TransactionDate,
                            CustomCharge = c.CustomCharge,
                            CourierCharge = c.CourierCharge,
                            OtherCharge = c.OtherCharge,
                            Consignor = ins.Consignor,
                            Origin = ins.ConsignorCityName,
                            ConsigneeCountryName = ins.ConsigneeCountryName,
                            ConsigneeCityName = ins.ConsigneeCityName,
                            ConsigneeName = ins.Consignee,
                          
                            InscanID = c.InscanID,
                            AWBChecked = true,
                            VATAmount = c.VATAmount,
                            NetValue = c.NetValue,
                            Weight = ins.Weight,
                            Pieces = ins.Pieces
                        }).OrderBy(cc => cc.AWBDateTime).ToList();

            int _index = 0;
            _custinvoice.CustomerInvoiceTax = 0;
            foreach (var item in _details)
            {
                _details[_index].TotalCharges = Convert.ToDecimal(_details[_index].CourierCharge) + Convert.ToDecimal(_details[_index].CustomCharge) + Convert.ToDecimal(_details[_index].OtherCharge);
                _custinvoice.TotalCharges += _details[_index].TotalCharges;
                _custinvoice.CustomerInvoiceTax = _custinvoice.CustomerInvoiceTax + Convert.ToDecimal(_details[_index].VATAmount);
                _index++;
            }

            _custinvoice.CustomerInvoiceTax = _details.Sum(cc => cc.VATAmount).Value;
            _custinvoice.TotalCharges = _details.Sum(cc => cc.CourierCharge).Value;

            _custinvoice.CustomerInvoiceDetailsVM = _details;

            Session["CODInvoiceListing"] = _details;
            return View(_custinvoice);

        }

        public ActionResult InvoicePrint(int id)
        {
            ViewBag.ReportName = "Invoice Printing";
            LabelPrintingParam picker = SessionDataModel.GetLabelPrintParam();
            string monetaryunit = Session["MonetaryUnit"].ToString();
            AccountsReportsDAO.CODInvoiceReport(id, monetaryunit);
            //AccountsReportsDAO.CustomerTaxInvoiceReport(id, monetaryunit);
            return View();
        }


        public ActionResult MultipleInvoice()
        {
            InvoiceAllParam vm = new InvoiceAllParam();
            vm.FromDate = CommonFunctions.GetFirstDayofMonth();
            vm.ToDate = CommonFunctions.GetLastDayofMonth();
            vm.MovementId = "1";
            vm.Details = new List<CustomerInvoicePendingModel>();
            return View(vm);
        }
        public ActionResult InvoiceAll()
        {

            DatePicker datePicker = SessionDataModel.GetTableVariable();

            if (datePicker == null)
            {
                datePicker = new DatePicker();
                datePicker.FromDate = CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date;
                datePicker.ToDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                datePicker.MovementId = "1,2,3,4";
            }
            if (datePicker != null)
            {

                if (datePicker.MovementId == null)
                    datePicker.MovementId = "1,2,3,4";
            }
            else
            {

            }


            //ViewBag.Movement = new MultiSelectList(db.CourierMovements.ToList(),"MovementID","MovementType");
            ViewBag.Movement = db.CourierMovements.ToList();

            ViewBag.Token = datePicker;
            SessionDataModel.SetTableVariable(datePicker);
            return View(datePicker);

        }

        [HttpPost]
        public JsonResult GenerateInvoice(InvoiceAllParam picker)
        {
            try
            {


                picker.MovementId = "";
                if (picker.SelectedValues != null)
                {
                    foreach (var item in picker.SelectedValues)
                    {
                        if (picker.MovementId == "")
                        {
                            picker.MovementId = item.ToString();
                        }
                        else
                        {
                            picker.MovementId = picker.MovementId + "," + item.ToString();
                        }

                    }
                }


                PickupRequestDAO _dao = new PickupRequestDAO();
                if (picker.InvoiceType == "Customer")
                {
                    int invoicecount = _dao.GenerateCustomerInvoiceAll(picker);

                    return Json(new { InvoiceCount = invoicecount, status = "ok", message = "Customer Invoice Generated Successfully!" }, JsonRequestBehavior.AllowGet);
                }
                else if (picker.InvoiceType == "COLoader")
                {
                    int invoicecount = _dao.GenerateCOInvoiceAll(picker);
                    return Json(new { InvoiceCount = invoicecount, status = "ok", message = "CO Loader Generated Successfully!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "Failed", message = "Invoice Generation Failed" }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpPost]
        public JsonResult GenerateInvoicePending(InvoiceAllParam picker)
        {
            string message = "";
            InvoiceAllParam vm = new InvoiceAllParam();
            try
            {
                picker.MovementId = "";
                if (picker.SelectedValues != null)
                {
                    foreach (var item in picker.SelectedValues)
                    {
                        if (picker.MovementId == "")
                        {
                            picker.MovementId = item.ToString();
                        }
                        else
                        {
                            picker.MovementId = picker.MovementId + "," + item.ToString();
                        }

                    }
                }


                PickupRequestDAO _dao = new PickupRequestDAO();
                if (picker.InvoiceType == "Customer")
                {
                    try
                    {
                        vm.Details = PickupRequestDAO.GenerateInvoicePending(picker);
                    }
                    catch (Exception ex1)
                    {

                        if (ex1.Message.Contains("Time out"))
                            message = "Time out due to more records the system can handle- Please Retry with one month period";
                        else
                        {
                            message = ex1.Message;
                        }

                        if (vm.Details == null)
                            vm.Details = new List<CustomerInvoicePendingModel>();
                        Session["InvoicePending"] = vm.Details;
                        return Json(new { InvoiceCount = 0, status = "Failed", message = message }, JsonRequestBehavior.AllowGet);
                    }

                    if (vm.Details == null)
                        vm.Details = new List<CustomerInvoicePendingModel>();
                    Session["InvoicePending"] = vm.Details;
                    if (vm.Details.Count == 0)
                    {
                        message = "No Invoice Pending";
                    }
                    else
                    {
                        message = "Customer Invoice Pending Generated Succesfully!";
                    }
                    return Json(new { InvoiceCount = vm.Details.Count, status = "ok", message = message }, JsonRequestBehavior.AllowGet);
                }
                else if (picker.InvoiceType == "COLoader")
                {
                    try
                    {
                        vm.Details = PickupRequestDAO.GenerateCoLoaderInvoicePending(picker);
                    }
                    catch (Exception ex1)
                    {
                        if (ex1.Message.Contains("Time out"))
                            message = "Time out due to more records the system can handle- Please Retry with one month period";
                        else
                        {
                            message = ex1.Message;
                        }
                        if (vm.Details == null)
                            vm.Details = new List<CustomerInvoicePendingModel>();
                        Session["InvoicePending"] = vm.Details;
                        return Json(new { InvoiceCount = 0, status = "Failed", message = message }, JsonRequestBehavior.AllowGet);
                    }

                    if (vm.Details == null)
                        vm.Details = new List<CustomerInvoicePendingModel>();
                    if (vm.Details.Count == 0)
                    {
                        message = "No Invoice Pending";
                    }
                    else
                    {
                        message = "Co-Loader Invoice Pending Generated Succesfully!";
                    }
                    Session["InvoicePending"] = vm.Details;

                    return Json(new { InvoiceCount = vm.Details.Count, status = "ok", message = message }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    TempData["ErrorMsg"] = "";
                    vm.Details = new List<CustomerInvoicePendingModel>();
                    return Json(new { InvoiceCount = 0, status = "ok", message = "No Action done!" }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {

                return Json(new { InvoiceCount = 0, status = "Failed", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        public ActionResult ShownvoicePending(int CustomerId, bool statuschecked)
        {
            InvoiceAllParam vm = new InvoiceAllParam();
            try
            {
                vm.Details = new List<CustomerInvoicePendingModel>();
                var Details1 = new List<CustomerInvoicePendingModel>();
                var details = (List<CustomerInvoicePendingModel>)Session["InvoicePending"];
                if (details != null)
                {
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].CustomerId == CustomerId || CustomerId == 0)
                            details[i].CustomerChecked = statuschecked;

                        if (details[i].CustomerChecked == true)
                            Details1.Add(details[i]);
                    }
                    for (int i = 0; i < details.Count; i++)
                    {

                        if (details[i].CustomerChecked == false)
                            Details1.Add(details[i]);
                    }

                    vm.Details = Details1;
                    Session["InvoicePending"] = Details1;
                }

                return PartialView("InvoicePendingList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }

        public ActionResult GetClientFromByteArray()
        {
            // Get image path  
            string imgPath = Server.MapPath("~/Content/ClientLogo/ClientLogo.png");
            // Convert image to byte array  
            byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
            //Convert byte arry to base64string   
            string imreBase64Data = Convert.ToBase64String(byteData);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view  
            ViewBag.ImageData = imgDataURL;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Details(string GridHtml)
        {
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            //StringWriter objStringWriter = new StringWriter();
            //HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            //gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(GridHtml);
            Response.Flush();
            Response.End();
            return RedirectToAction("Index");
            //return File(Encoding.ASCII.GetBytes(GridHtml), "application/vnd.ms-excel", "Grid6.xls");
        }

        [HttpGet]
        public JsonResult GetCustomerName(string term,string Type)
        {
            if (term==null)
                term="";
            if (Type == "Consignee")
            {

                if (term.Trim() != "")
                {
                    var customerlist = (from c1 in db.ConsigneeMasters
                                        where c1.ConsigneeName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.ConsigneeName ascending
                                        select new {   CustomerName = c1.ConsigneeName }).Distinct().Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var customerlist = (from c1 in db.ConsigneeMasters
                                        orderby c1.ConsigneeName ascending
                                        select new {  CustomerName = c1.ConsigneeName }).Distinct().Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                if (term.Trim() != "")
                {
                    var customerlist = (from c1 in db.InScanMasters // ConsignorMasters
                                        where c1.Consignor.ToLower().StartsWith(term.ToLower())
                                        orderby c1.Consignor ascending
                                        select new { CustomerName = c1.Consignor }).Distinct().Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var customerlist = (from c1 in db.InScanMasters //ConsignorMasters
                                        orderby c1.Consignor ascending
                                        select new { CustomerName = c1.Consignor }).Distinct().Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }

}
