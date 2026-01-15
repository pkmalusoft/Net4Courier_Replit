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
    public class ForwardingAgentRatesController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            List<FAgentRateVM> lst = AWBDAO.GetForwardingAgentRateList();
            return View(lst);
        }
        
        public ActionResult Create(int id = 0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.CustRate = db.CustomerRateTypes.ToList();
            ViewBag.ProductType = db.ProductTypes.ToList();
            ViewBag.FAgentID = db.SupplierMasters.Where(cc=>cc.SupplierTypeID==3).ToList();
            ViewBag.Zones = db.ZoneCharts.ToList();
            FAgentRateVM vm = new FAgentRateVM();
            ForwardingAgentRate r = new ForwardingAgentRate();
            if (id > 0)
            {
                r = db.ForwardingAgentRates.Find(id);
                vm.FAgentID = r.FAgentID;
                vm.FAgentRateID = r.FAgentRateID;
                vm.BaseRate = r.BaseRate;
                vm.BaseWeight = r.BaseWeight;
                vm.ProductTypeID = r.CourierServiceID;
                vm.ZoneChartID = r.ZoneChartID;
                //var loc = db.GetZoneChartByCustomer(r.CustomerRateTypeID);
                //Zones lst = new Zones();
                var lst = (from c in db.ZoneCharts
                           join cate in db.ZoneCategories on c.ZoneCategoryID equals cate.ZoneCategoryID
                           join z in db.ZoneMasters on c.ZoneID equals z.ZoneID
                           where c.ZoneChartID == r.ZoneChartID
                           select new { ZoneID = c.ZoneChartID, ZoneName = cate.ZoneCategory1 + "-" + z.ZoneName }).FirstOrDefault();
                if (lst != null)
                    vm.ZoneChartName = lst.ZoneName;
                //vm.CountryID = r.CountryID.Value;
                vm.FAgentID = r.FAgentID;

                ViewBag.Title = "Modify";
                ViewBag.EditMode = "true";
            } 
            else
            {
                ViewBag.Title = "Create";
                ViewBag.EditMode = "false";
                vm.FAgentRateID = 0;
            }

            return View(vm);

        }

        [HttpPost]
        public ActionResult SaveRate(FAgentRateVM v,string Details)
        {
            Details.Replace("{}", "");
            var IDetails = JsonConvert.DeserializeObject<List<FAgentRateDetailsVM>>(Details);
            v.FAgentRateDetails = IDetails;
            try
            {
                ForwardingAgentRate r = new ForwardingAgentRate();
                if (v.FAgentRateID > 0)
                    r = db.ForwardingAgentRates.Find(v.FAgentRateID);

                r.CourierServiceID = v.ProductTypeID;
                r.ZoneChartID = v.ZoneChartID;
                r.FAgentID = v.FAgentID;
                r.BaseWeight = v.BaseWeight;
                r.BaseRate = v.BaseRate;

                if (v.FAgentRateID > 0)
                {
                    db.Entry(r).State = EntityState.Modified;
                    db.SaveChanges();
                    var range = db.ForwardingAgentRateDets.Where(cc => cc.FAgentRateID == r.FAgentRateID).ToList();
                    db.ForwardingAgentRateDets.RemoveRange(range);
                    db.SaveChanges();
                }
                else
                {
                    db.ForwardingAgentRates.Add(r);
                    db.SaveChanges();
                   
                }

                if (v.FAgentRateDetails != null)
                {
                    foreach (var item in v.FAgentRateDetails)
                    {
                        if (item.Deleted == false)
                        {
                             
                                //var acgrps = (from d in db.ForwardingAgentRateDets  orderby d.FAgentRateDetID descending select d.FAgentRateDetID).FirstOrDefault();
                                //var maxid = acgrps + 1;
                                ForwardingAgentRateDet a = new ForwardingAgentRateDet();
                                a.FAgentRateID = r.FAgentRateID;
                                a.AdditionalWeightFrom = item.AddWtFrom;
                                a.AdditionalWeightTo = item.AddWtTo;
                                a.IncrementalWeight = item.IncrWt;
                                a.AdditionalRate = item.AddRate;

                                db.ForwardingAgentRateDets.Add(a);
                                db.SaveChanges();
                          
                            //else if (item.FAgentRateDetID > 0)
                            //{
                            //    ForwardingAgentRateDet a = new ForwardingAgentRateDet();
                            //    a = db.ForwardingAgentRateDets.Find(item.FAgentRateDetID);

                            //    //a.FAgentRateID = item.FAgentRateID;
                            //    a.AdditionalWeightFrom = item.AddWtFrom;
                            //    a.AdditionalWeightTo = item.AddWtTo;
                            //    a.IncrementalWeight = item.IncrWt;
                            //    a.AdditionalRate = item.AddRate;

                            //    db.Entry(a).State = EntityState.Modified;
                            //    db.SaveChanges();

                            //}
                        }
                        //else if (item.Deleted == true && item.FAgentRateDetID > 0)
                        //{
                        //    ForwardingAgentRateDet a = new ForwardingAgentRateDet();
                        //    a = db.ForwardingAgentRateDets.Find(item.FAgentRateDetID);
                        //    db.ForwardingAgentRateDets.Remove(a);
                        //    db.SaveChanges();

                        //}
                    }
                }

                return Json(new { Status = "OK", FAgentRateID = r.FAgentRateID, message ="Forwarding Rate Chart Added Succesfully!" }, JsonRequestBehavior.AllowGet);

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



        public ActionResult Edit(int id)
        {

            FAgentRateVM FAge = new FAgentRateVM();
            ViewBag.ProductTypeID = db.ProductTypes.ToList();
            var h = db.ForwardingAgentMasters.ToList();
            ViewBag.FAgentID = db.ForwardingAgentMasters.ToList();
            ViewBag.ZoneID = db.ZoneCharts.ToList();


            ForwardingAgentRate data = (from c in db.ForwardingAgentRates where c.FAgentRateID == id select c).FirstOrDefault();


            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {

                FAge.FAgentRateID = data.FAgentRateID;
                FAge.FAgentID = data.FAgentID;
                FAge.ZoneID = data.ZoneChartID;

                //int countryid = Convert.ToInt32(Session["depotcountry"].ToString());
                //FAge.CountryID = countryid;
                FAge.ProductTypeID = data.CourierServiceID.Value;
                FAge.BaseWeight = data.BaseWeight;
                FAge.BaseRate = data.BaseRate;




            }

            return View(FAge);


        }

        [HttpPost]
        public ActionResult Edit(FAgentRateVM v)
        {

            ForwardingAgentRate obj = new ForwardingAgentRate();
            obj.FAgentRateID = v.FAgentRateID;

            obj.FAgentID = v.FAgentID;
            obj.ZoneChartID = v.ZoneID;

            int countryid = Convert.ToInt32(Session["depotcountry"].ToString());
            obj.CountryID = countryid;
            obj.CourierServiceID = v.ProductTypeID;
            obj.BaseWeight = v.BaseWeight;
            obj.BaseRate = v.BaseRate;
            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();



            var data = (from c in db.ForwardingAgentRateDets where c.FAgentRateID == v.FAgentRateID select c).ToList();
            foreach (var item in data)
            {
                db.ForwardingAgentRateDets.Remove(item);
                db.SaveChanges();
            }
            foreach (var item in v.FAgentRateDetails)
            {
                ForwardingAgentRateDet ob = new ForwardingAgentRateDet();

                ob.FAgentRateID = v.FAgentRateID;
                ob.AdditionalWeightFrom = item.AddWtFrom;
                ob.AdditionalWeightTo = item.AddWtTo;
                ob.IncrementalWeight = item.IncrWt;
                ob.AdditionalRate = item.AddRate;

                db.ForwardingAgentRateDets.Add(ob);

                db.SaveChanges();

            }

            return RedirectToAction("Index");
        }
        public JsonResult GetZoneChart(string term)
        {

            List<ZoneNameVM> lst = new List<ZoneNameVM>();
            var loc = AWBDAO.GetZoneChartList();

            if (term.Trim() != "")
            {
                lst = (from c in loc where c.ZoneName.Contains(term) orderby c.ZoneName select c).ToList();
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
        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile, int CustomerID)
        {
            var pcustomerid = CustomerID;
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<FAgentRateDetailsVM> fileData = GetDataFromCSVFile(importFile.InputStream, pcustomerid);
                FAgentRateVM vm = new FAgentRateVM();
                vm.FAgentRateDetails = fileData;
                //vm.Details = fileData; //.OrderByDescending(cc => cc.SNo).ToList();
                Session["ShipmentList"] = vm.FAgentRateDetails;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<FAgentRateDetailsVM> GetDataFromCSVFile(Stream stream, int CustomerId)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
       
            List<FAgentRateDetailsVM> details = new List<FAgentRateDetailsVM>();
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
                                    FAgentRateDetailsVM obj = new FAgentRateDetailsVM();
                                    obj.FAgentRateDetID = 0;
                                    obj.AddWtFrom = Convert.ToDecimal(dt.Rows[i]["WeightFrom"].ToString());
                                    obj.AddWtTo = Convert.ToDecimal(dt.Rows[i]["WeightTo"].ToString());
                                    obj.IncrWt = Convert.ToDecimal(dt.Rows[i]["IncrementWeight"].ToString());
                                    obj.ContractRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString());
                                    if (i == 0)
                                    {
                                        obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString()) - Convert.ToDecimal(dt.Rows[i]["BaseRate"].ToString());
                                    }
                                    else
                                    {
                                        obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString()) - Convert.ToDecimal(dt.Rows[i - 1]["Rate"].ToString());
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
        public class det
        {
            public int FAgentRateID { get; set; }
            public int FAgentRateDetID { get; set; }
            public decimal AddWtFrom { get; set; }
            public decimal AddWtTo { get; set; }
            public decimal IncrWt { get; set; }
            public decimal ContractRate { get; set; }
            public decimal AddRate { get; set; }
        }


        public JsonResult GetDetails(int id)
        {


            var data = (from c in db.ForwardingAgentRateDets where c.FAgentRateID == id select c).ToList();

            List<det> lst = new List<det>();

            if (data != null)
            {

                foreach (var item in data)
                {
                    det d = new det();

                    d.FAgentRateID = item.FAgentRateID;
                    d.FAgentRateDetID = item.FAgentRateDetID;
                    d.AddWtFrom = item.AdditionalWeightFrom;
                    d.AddWtTo = item.AdditionalWeightTo;
                    d.IncrWt = item.IncrementalWeight;
                    d.AddRate = item.AdditionalRate;

                    lst.Add(d);

                }



            }
            return Json(lst, JsonRequestBehavior.AllowGet);

        }



        public ActionResult DeleteConfirmed(int id = 0)
        {
            ForwardingAgentRate a = db.ForwardingAgentRates.Find(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.ForwardingAgentRates.Remove(a);
                db.SaveChanges();

                List<ForwardingAgentRateDet> lst = (from c in db.ForwardingAgentRateDets where c.FAgentRateDetID == id select c).ToList();

                foreach (var item in lst)
                {
                    db.ForwardingAgentRateDets.Remove(item);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }





    }
}
