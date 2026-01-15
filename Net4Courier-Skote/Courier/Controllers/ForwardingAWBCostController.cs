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
    public class ForwardingAWBCostController : Controller
    {
        Entities1 db = new Entities1();
        // GET: ForwardingAWBCost
        public ActionResult Index()
        {
            ViewBag.FAgentID = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 3).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SaveRate(int AgentID,string Details)
        {
            Details.Replace("{}", "");
            var IDetails = JsonConvert.DeserializeObject<List<FAgentCostDetailsVM>>(Details);
            var FAgentRateDetails = IDetails;
            var othercharge= db.OtherCharges.Where(cc => cc.OtherCharge1 == "Other Charges").FirstOrDefault();
            int otherchargeid = 0;
            if (othercharge!=null)
            {
                otherchargeid = othercharge.OtherChargeID;
            }
            try
            {
                ForwardingAgentRate r = new ForwardingAgentRate();
                //if (v.FAgentRateID > 0)
                //    r = db.ForwardingAgentRates.Find(v.FAgentRateID);

                //r.CourierServiceID = v.ProductTypeID;
                //r.ZoneChartID = v.ZoneChartID;
                //r.FAgentID = v.FAgentID;
                //r.BaseWeight = v.BaseWeight;
                //r.BaseRate = v.BaseRate;

                //if (v.FAgentRateID > 0)
                //{
                //    db.Entry(r).State = EntityState.Modified;
                //    db.SaveChanges();
                //    var range = db.ForwardingAgentRateDets.Where(cc => cc.FAgentRateID == r.FAgentRateID).ToList();
                //    db.ForwardingAgentRateDets.RemoveRange(range);
                //    db.SaveChanges();
                //}
                //else
                //{
                //    db.ForwardingAgentRates.Add(r);
                //    db.SaveChanges();

                //}

                if (FAgentRateDetails != null)
                {
                    foreach (var item in FAgentRateDetails)
                    {
                        if (item.AWBNo != "")
                        {
                            InScanMaster _inscan = db.InScanMasters.Where(cc => cc.AWBNo == item.AWBNo).FirstOrDefault();
                            if (_inscan != null)
                            {
                                _inscan.ForwardingCharge = item.TotalCost;
                                _inscan.Remarks = _inscan.Remarks + " " + item.Remarks;
                                _inscan.FAgentId = AgentID;
                                _inscan.ManifestWeight = item.Weight;
                                _inscan.ForwardingAWBNo = item.ForwardingAWBNo; // InvoiceNo + "/" + item.InvoiceDate;
                                if (item.OtherCharge > 0 && otherchargeid > 0)
                                {
                                    InscanOtherCharge _oc = new InscanOtherCharge();
                                    _oc.OtherChargeID = otherchargeid;
                                    _oc.InscanID = _inscan.InScanID;
                                    _oc.Amount = item.OtherCharge;
                                    db.InscanOtherCharges.Add(_oc);
                                    db.SaveChanges();
                                    var totalcharnges = db.InscanOtherCharges.Where(cc => cc.InscanID == _inscan.InScanID).Sum(cc => cc.Amount).Value;
                                    _inscan.OtherCharge = totalcharnges;
                                    db.Entry(_inscan).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    db.Entry(_inscan).State = EntityState.Modified;
                                    db.SaveChanges();
                                }


                            }
                        }
                        else
                        {
                            break;
                        }
                             
                     }
                        
                }
               

                return Json(new { Status = "OK", FAgentRateID = r.FAgentRateID, message = "Forwarding Rate Cost Update!" }, JsonRequestBehavior.AllowGet);

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
                List<FAgentCostDetailsVM> fileData = GetDataFromCSVFile(importFile.InputStream);
                
                Session["ShipmentCostList"] = fileData;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<FAgentCostDetailsVM> GetDataFromCSVFile(Stream stream)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());

            List<FAgentCostDetailsVM> details = new List<FAgentCostDetailsVM>();
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
                                    FAgentCostDetailsVM obj = new FAgentCostDetailsVM();

                                    obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                                    obj.TotalCost = CommonFunctions.ParseDecimal(dt.Rows[i]["TotalCost"].ToString());
                                    obj.OtherCharge =CommonFunctions.ParseDecimal(dt.Rows[i]["OtherCharges"].ToString());
                                    obj.Remarks = dt.Rows[i]["Remarks"].ToString();
                                    obj.Weight= CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                                    obj.ForwardingAWBNo = dt.Rows[i]["ForwardingAWBNo"].ToString();
                                    //obj.IncrWt = Convert.ToDecimal(dt.Rows[i]["IncrementWeight"].ToString());
                                    //obj.ContractRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString());
                                    //if (i == 0)
                                    //{
                                    //    obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString()) - Convert.ToDecimal(dt.Rows[i]["BaseRate"].ToString());
                                    //}
                                    //else
                                    //{
                                    //    obj.AddRate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString()) - Convert.ToDecimal(dt.Rows[i - 1]["Rate"].ToString());
                                    //}

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
    }
}