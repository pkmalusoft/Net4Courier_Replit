using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExcelDataReader;
using Net4Courier.DAL;
using Net4Courier.Models;
using Newtonsoft.Json;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class CustomerRatesMasterController : Controller
    {
        private Entities1 db = new Entities1();


        public ActionResult Index()
        {
            var data = PickupRequestDAO.GetCustomerRateList();
            //var data = db.GetCustomerRates();
            return View(data);
        }

        public ActionResult Create(int id=0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.CustRate = db.CustomerRateTypes.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.ForwardingAgent = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3).OrderBy(cc => cc.SupplierName).ToList(); // db.ForwardingAgentMasters.ToList();
            ViewBag.Zones = db.ZoneCharts.ToList();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var branch = db.BranchMasters.Find(branchid);
            int CustomerRateType =1;
            if (branch.CustomerRateType == null)
                CustomerRateType = 1;
            else
                CustomerRateType = Convert.ToInt32(branch.CustomerRateType);
            CustRateVM vm = new CustRateVM();
            CustomerRate r = new CustomerRate();
            if (id > 0)
            {
                r = db.CustomerRates.Find(id);
               
            
              
                vm.BaseRate = r.BaseRate;
                vm.BaseWt = r.BaseWeight;
                vm.CustomerRateID = r.CustomerRateID;
                vm.MovementID = Convert.ToInt32(r.MovementID);
                vm.PaymentModeID = Convert.ToInt32(r.PaymentModeID);
                vm.CustomerRateTypeID = r.CustomerRateTypeID;
                vm.ContractRateTypeID = r.CustomerRateTypeID;
                vm.ProductTypeID = r.CourierServiceID;
                vm.ZoneChartID = r.ZoneChartID;
                if (r.MarginPerentage==null)
                {
                    vm.BaseMargin = 0;
                }
                else
                {
                    vm.BaseMargin = Convert.ToDecimal(r.MarginPerentage);
                }
                
                List<ZoneNameVM> vm1 = new List<ZoneNameVM>();
                vm1 = AWBDAO.GetZoneChartMaster(vm.CustomerRateTypeID,vm.ZoneChartID);
                if (vm1 != null)
                {
                    if (vm1.Count > 0)
                    {
                        vm.ZoneType = vm1[0].ZoneType;
                        if (vm.ZoneType == "Domestic")
                        {
                            vm.CitiesCountries = vm1[0].Cities;
                        }
                        else
                        {
                            vm.CitiesCountries = vm1[0].Countries;
                        }

                        vm.ZoneChartName = vm1[0].ZoneName;
                    }
                }
                //var loc = db.GetZoneChartByCustomer(r.CustomerRateTypeID);
                //Zones lst = new Zones(); 
                //lst = (from c in loc where c.ZoneChartID==r.ZoneChartID select new Zones { ZoneID = c.ZoneChartID, ZoneName = c.ZoneName }).FirstOrDefault();
                //if (lst!=null)
                //    vm.ZoneChartName = lst.ZoneName;
                //vm.CountryID = r.CountryID.Value;
                vm.FAgentID = r.FAgentID.Value;
                vm.CustomerRateID = r.CustomerRateID;
                if (r.AdditionalCharges != null)
                {
                    vm.AdditionalCharges = r.AdditionalCharges.Value;
                }
                if (r.WithTax != null)
                {
                    vm.withtax = r.WithTax.Value;
                }
                if (r.WithoutTax != null)
                {
                    vm.withouttax = r.WithoutTax.Value;
                }
                ViewBag.Title = "Modify";
                ViewBag.EditMode = "true";
                vm.BranchCustomerRateType = CustomerRateType;
            }
            else
            {
                ViewBag.Title = "Create";
                ViewBag.EditMode = "false";
                vm.CustomerRateID = 0;
                vm.BranchCustomerRateType = CustomerRateType;
            }

            return View(vm);

        }

        [HttpPost]
        public ActionResult Create(CustRateVM v)
        {
            try
            {
                CustomerRate r = new CustomerRate();
                if (v.CustomerRateID > 0)
                    r = db.CustomerRates.Find(v.CustomerRateID);

                r.CustomerRateTypeID = v.ContractRateTypeID;
                r.CourierServiceID = v.ProductTypeID;
                r.ZoneChartID = v.ZoneChartID;
                r.MovementID = v.MovementID;
                r.PaymentModeID = v.PaymentModeID;
                r.FAgentID = v.FAgentID;
                r.BaseWeight = v.BaseWt;
                r.WithTax = v.withtax;
                r.WithoutTax = v.withouttax;
                r.AdditionalCharges = v.AdditionalCharges;
                r.MarginPerentage = v.BaseMargin;
                r.BaseRate = v.BaseRate;
              //  r.BaseRate

                if (v.CustomerRateID > 0)
                {
                    db.Entry(r).State = EntityState.Modified;
                    db.SaveChanges();
                    //var data = (from c in db.CustomerRateDets where c.CustomerRateID == v.CustomerRateID select c).ToList();
                    //foreach (var item in data)
                    //{
                    //    db.CustomerRateDets.Remove(item);
                    //    db.SaveChanges();
                    //}
                }
                else
                {
                    db.CustomerRates.Add(r);
                    db.SaveChanges();
                }

                if (v.CustRateDetails != null)
                {
                    foreach (var item in v.CustRateDetails)
                    {
                        if (item.Deleted == false)
                        {
                            if (item.CustomerRateDetID == 0)
                            {
                                var acgrps = (from d in db.CustomerRateDets orderby d.CustomerRateDetID descending select d.CustomerRateDetID).FirstOrDefault();
                                var maxid = acgrps + 1;
                                CustomerRateDet a = new CustomerRateDet();
                                a.CustomerRateDetID = maxid;
                                a.CustomerRateID = r.CustomerRateID;
                                a.AdditionalWeightFrom = item.AddWtFrom;
                                a.AdditionalWeightTo = item.AddWtTo;
                                a.IncrementalWeight = item.IncrWt;
                                a.AdditionalRate = item.AddRate;

                                db.CustomerRateDets.Add(a);
                                db.SaveChanges();
                            }
                            else if (item.CustomerRateDetID > 0)
                            {
                                CustomerRateDet a = new CustomerRateDet();
                                a = db.CustomerRateDets.Find(item.CustomerRateDetID);

                                a.CustomerRateID = r.CustomerRateID;
                                a.AdditionalWeightFrom = item.AddWtFrom;
                                a.AdditionalWeightTo = item.AddWtTo;
                                a.IncrementalWeight = item.IncrWt;
                                a.AdditionalRate = item.AddRate;

                                db.Entry(a).State = EntityState.Modified;
                                db.SaveChanges();

                            }
                        }
                        else if (item.Deleted == true && item.CustomerRateDetID > 0)
                        {
                            CustomerRateDet a = new CustomerRateDet();
                            a = db.CustomerRateDets.Find(item.CustomerRateDetID);
                            db.CustomerRateDets.Remove(a);
                            db.SaveChanges();

                        }
                    }
                    }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {

            }

            ViewBag.CustRate = db.CustomerRateTypes.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.ForwardingAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Zones = db.ZoneCharts.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult SaveRate(CustRateVM v, string Details)
        {
            Details.Replace("{}", "");
            var IDetails = JsonConvert.DeserializeObject<List<CustRateDetailsVM>>(Details);
            v.CustRateDetails = IDetails;
            try
            {
                CustomerRate r = new CustomerRate();
                if (v.CustomerRateID > 0)
                    r = db.CustomerRates.Find(v.CustomerRateID);

                r.CustomerRateTypeID = v.ContractRateTypeID;
                r.CourierServiceID = v.ProductTypeID;
                r.ZoneChartID = v.ZoneChartID;
                r.MovementID = v.MovementID;
                r.PaymentModeID = v.PaymentModeID;
                r.FAgentID = v.FAgentID;
                r.BaseWeight = v.BaseWt;
                r.WithTax = v.withtax;
                r.WithoutTax = v.withouttax;
                r.AdditionalCharges = v.AdditionalCharges;
                r.MarginPerentage = v.BaseMargin;
                r.BaseRate = v.BaseRate;

                if (v.CustomerRateID > 0)
                {
                    db.Entry(r).State = EntityState.Modified;
                    db.SaveChanges();
                    var range = db.CustomerRateDets.Where(cc => cc.CustomerRateID == r.CustomerRateID).ToList();
                    db.CustomerRateDets.RemoveRange(range);
                    db.SaveChanges();
                }
                else
                {
                    db.CustomerRates.Add(r);
                    db.SaveChanges(); 
                    
                }

                if (v.CustRateDetails != null)
                {
                    foreach (var item in v.CustRateDetails)
                    {
                        if (item.Deleted == false)
                        {

                            var acgrps = (from d in db.CustomerRateDets orderby d.CustomerRateDetID descending select d.CustomerRateDetID).FirstOrDefault();
                            var maxid = acgrps + 1;
                            CustomerRateDet a = new CustomerRateDet();
                            a.CustomerRateDetID = maxid;
                            a.CustomerRateID = r.CustomerRateID;
                            a.AdditionalWeightFrom = item.AddWtFrom;
                            a.AdditionalWeightTo = item.AddWtTo;
                            a.IncrementalWeight = item.IncrWt;
                            a.AdditionalRate = item.AddRate;

                            db.CustomerRateDets.Add(a);
                            db.SaveChanges();

                            //    else if (item.CustomerRateDetID > 0)
                            //    {
                            //        CustomerRateDet a = new CustomerRateDet();
                            //        a = db.CustomerRateDets.Find(item.CustomerRateDetID);

                            //        a.CustomerRateID = r.CustomerRateID;
                            //        a.AdditionalWeightFrom = item.AddWtFrom;
                            //        a.AdditionalWeightTo = item.AddWtTo;
                            //        a.IncrementalWeight = item.IncrWt;
                            //        a.AdditionalRate = item.AddRate;

                            //        db.Entry(a).State = EntityState.Modified;
                            //        db.SaveChanges();

                            //    }
                            //}
                            //else if (item.Deleted == true && item.CustomerRateDetID > 0)
                            //{
                            //    CustomerRateDet a = new CustomerRateDet();
                            //    a = db.CustomerRateDets.Find(item.CustomerRateDetID);
                            //    db.CustomerRateDets.Remove(a);
                            //    db.SaveChanges();

                            //}

                        }
                    }
                }

                return Json(new { Status = "OK", CustomerRateID = r.CustomerRateID, message = "Rate Chart Added Succesfully!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

            }

            ViewBag.CustRate = db.CustomerRateTypes.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.ForwardingAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Zones = db.ZoneCharts.ToList();
            return View();
        }


        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile, int CustomerID)
        {
            var pcustomerid = CustomerID;
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<CustRateDetailsVM> fileData = GetDataFromCSVFile(importFile.InputStream, pcustomerid);
                CustRateVM vm = new CustRateVM();
                vm.CustRateDetails = fileData;
                //vm.Details = fileData; //.OrderByDescending(cc => cc.SNo).ToList();
                Session["ShipmentList"] = vm.CustRateDetails;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<CustRateDetailsVM> GetDataFromCSVFile(Stream stream, int CustomerId)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());

            List<CustRateDetailsVM> details = new List<CustRateDetailsVM>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        //DataView dv = dataSet.Tables[0].DefaultView;
                        //dv.RowFilter = "AWBNo==''";
                        //DataSet ds1 = dv.DataViewManager.DataSet;

                        var dataTable = dataSet.Tables[0];
                        string xml = dataSet.GetXml();
                        if (dataSet != null && dataSet.Tables.Count > 0)
                        {
                            if (dataSet.Tables[0].Rows.Count > 0)
                            {
                                DataTable dt = dataSet.Tables[0];
                                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                                {
                                    CustRateDetailsVM obj = new CustRateDetailsVM();
                                    obj.CustomerRateDetID = 0;
                                    obj.AddWtFrom = Convert.ToDecimal(dt.Rows[i]["WeightFrom"].ToString());
                                    obj.AddWtTo = Convert.ToDecimal(dt.Rows[i]["WeightTo"].ToString());
                                    obj.IncrWt = Convert.ToDecimal(dt.Rows[i]["IncrementWeight"].ToString());
                                    obj.ContractRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString());
                                    if (i == 0)
                                    {
                                        obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString())-Convert.ToDecimal(dt.Rows[i]["BaseRate"].ToString()) ;
                                    }
                                    else
                                    {
                                        obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString()) - Convert.ToDecimal(dt.Rows[i-1]["Rate"].ToString());
                                    }
                                    details.Add(obj);
                                }
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }




            return details;
        }

        public ActionResult Edit(int id)
        {

            ViewBag.CustRate = db.CustomerRateTypes.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.ForwardingAgent = db.ForwardingAgentMasters.ToList();
            ViewBag.Zones = db.ZoneCharts.ToList();

            CustRateVM vm = new CustRateVM();
            CustomerRate r = db.CustomerRates.Find(id);
            if (r != null)
            {
                vm.BaseRate = r.BaseRate;
                vm.BaseWt = r.BaseWeight;
                vm.ContractRateID = r.CustomerRateID;
                vm.ContractRateTypeID = r.CustomerRateTypeID;
                vm.ProductTypeID = r.CourierServiceID;
                vm.ZoneChartID = r.ZoneChartID;
                //vm.CountryID = r.CountryID.Value;
                vm.FAgentID = r.FAgentID.Value;
                vm.CustomerRateID = r.CustomerRateID;
                vm.AdditionalCharges = r.AdditionalCharges.Value;
                vm.withtax = r.WithTax.Value;
                vm.withouttax = r.WithoutTax.Value;
            }
            else
            {
                return HttpNotFound();
            }


            return View(vm);


        }

        [HttpPost]
        public ActionResult Edit(CustRateVM v)
        {
            CustomerRate r = new CustomerRate();
            r.CustomerRateID = v.CustomerRateID;
            r.CustomerRateTypeID = v.ContractRateTypeID;
            r.CourierServiceID = v.ProductTypeID;
            r.ZoneChartID = v.ZoneChartID;
            r.FAgentID = v.FAgentID;
            r.BaseWeight = v.BaseWt;
            r.WithTax = v.withtax;
            r.WithoutTax = v.withouttax;
            r.AdditionalCharges = v.AdditionalCharges;

            r.BaseRate = v.BaseRate;

            db.Entry(r).State = EntityState.Modified;
            db.SaveChanges();

            //var data = (from c in db.CustomerRateDets where c.CustomerRateID == v.CustomerRateID select c).ToList();
            //foreach (var item in data)
            //{
            //    db.CustomerRateDets.Remove(item);
            //    db.SaveChanges();
            //}

            foreach (var item in v.CustRateDetails)
            {
                if (item.CustomerRateDetID == 0)
                {
                    CustomerRateDet a = new CustomerRateDet();
                    a.CustomerRateID = r.CustomerRateID;
                    a.AdditionalWeightFrom = item.AddWtFrom;
                    a.AdditionalWeightTo = item.AddWtTo;
                    a.IncrementalWeight = item.IncrWt;
                    a.AdditionalRate = item.AddRate;

                    db.CustomerRateDets.Add(a);
                    db.SaveChanges();
                }
                else
                {
                    CustomerRateDet a = new CustomerRateDet();
                    a = db.CustomerRateDets.Find(item.CustomerRateDetID);
                    a.CustomerRateID = r.CustomerRateID;
                    a.AdditionalWeightFrom = item.AddWtFrom;
                    a.AdditionalWeightTo = item.AddWtTo;
                    a.IncrementalWeight = item.IncrWt;
                    a.AdditionalRate = item.AddRate;

                    db.Entry(a).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");

        }

        public JsonResult GetRateBasedType(int id)
        {
            var ratetype = db.CustomerRateTypes.Find(id);
            var ratebasedtype = 1;
            if (ratetype.RateBasedType==null)
            {
                ratebasedtype = 1;
            }
            else
            {
                ratebasedtype = Convert.ToInt32(ratetype.RateBasedType);
            }
            return Json(ratebasedtype, JsonRequestBehavior.AllowGet);
        }
        public class det
        {
            public int CustomerRateID { get; set; }
            public int CustomerRateDetID { get; set; }
            public decimal AddWtFrom { get; set; }
            public decimal AddWtTo { get; set; }
            public decimal IncrWt { get; set; }
            public decimal ContractRate { get; set; }
            public decimal AddRate { get; set; }
        }


        public JsonResult GetDetails(int id)
        {


            var data = (from c in db.CustomerRateDets where c.CustomerRateID == id orderby c.AdditionalWeightFrom select c).ToList();

            List<det> lst = new List<det>();

            if (data != null)
            {

                foreach (var item in data)
                {
                    det d = new det();
                      d.CustomerRateDetID = item.CustomerRateDetID;
                    d.CustomerRateID = item.CustomerRateID;
                    d.AddWtFrom = item.AdditionalWeightFrom;
                    d.AddWtTo = item.AdditionalWeightTo;
                    d.IncrWt = item.IncrementalWeight;
                    d.AddRate = item.AdditionalRate;

                    lst.Add(d);

                }



            }
            return Json(lst, JsonRequestBehavior.AllowGet);

        }


        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();

            CustomerRate a = db.CustomerRates.Find(id);
            try
            {
                if (a == null)
                {
                    obj.Message = "Customer Rate Delete Failed!";
                    obj.Status = "Failed";
                }
                else
                {
                    List<CustomerRateDet> lst = (from c in db.CustomerRateDets where c.CustomerRateID == id select c).ToList();

                    foreach (var item in lst)
                    {
                        db.CustomerRateDets.Remove(item);
                        db.SaveChanges();
                    }

                    db.CustomerRates.Remove(a);
                    db.SaveChanges();
                                        
                    obj.Message = "Customer Rate Deleted Succesfully!";
                    obj.Status = "OK";
                }
            }
            catch (Exception ex)
            {
                obj.Status = "Failed";
                obj.Message = ex.Message;
            }
            return Json(obj, JsonRequestBehavior.AllowGet);

        }
       

        public JsonResult GetZoneByCustomer(string term,int contractid)
        {

            List<ZoneNameVM> lst = new List<ZoneNameVM>();
            var loc = AWBDAO.GetZoneChartMaster(contractid);

            if (term.Trim()!="")
            {
                lst = (from c in loc   where c.ZoneName.Contains(term) orderby c.ZoneName select c).ToList();
            }
            else
            {
                lst = (from c in loc orderby c.ZoneName select c).ToList();
            }
            //foreach (var item in loc)
            //{
            //    lst.Add(new Zones { ZoneID = item.ZoneChartID, ZoneName = item.ZoneName });

            //}
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetZoneByCustomerold(int contractid)
        {
            List<Zones> lst = new List<Zones>();
            var loc = db.GetZoneChartByCustomer(contractid);

            foreach (var item in loc)
            {
                lst.Add(new Zones { ZoneID = item.ZoneChartID, ZoneName = item.ZoneName });

            }
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        public class Zones
        {
            public int ZoneID { get; set; }
            public string ZoneName { get; set; }           
        }
        public ActionResult RateChartPrint(int id)
        {
            ViewBag.ReportName = "Rate Chart Printing";
            AccountsReportsDAO.RateChartPrintReport(id);            
            return View();

        }
    }
}