using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using System.Configuration;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class ZoneChartController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            //GetEventVenuesList("dubai");

            //List<ZoneChartVM> lst = (from c in db.ZoneCharts join t in db.ZoneCategories on c.ZoneCategoryID equals t.ZoneCategoryID join t1 in db.ZoneMasters on c.ZoneID equals t1.ZoneID select new ZoneChartVM { ZoneChartID = c.ZoneChartID, ZoneCategory = t.ZoneCategory1, ZoneName = t1.ZoneName }).ToList();
            List<ZoneChartVM> lst=PickupRequestDAO.GetZoneChartList();
            return View(lst);
        }

        public ActionResult Create(int id=0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.ZoneCategory = db.ZoneCategories.ToList();
            ViewBag.Zones = db.ZoneMasters.ToList();
            var zonetype = db.CourierMovements.ToList();
            ViewBag.Movement = zonetype;

            ViewBag.Country = db.CountryMasters.ToList();
            ViewBag.City = db.CityMasters.Where(cc=>cc.CountryID==203).ToList();
            Session["CityList"] = null;
            Session["CountryList"] = null;
            Session["LocationList"] = null;
            if (Session["depotcountry"] == null)
            {
                var depot= (from c in db.BranchMasters where c.BranchID == branchid select c.CountryID).FirstOrDefault();
                if (depot != null)
                {
                    Session["depotcountry"] = depot.Value;
                    ViewBag.depotcountry = Convert.ToInt32(Session["depotcountry"].ToString());
                }
            }
            else
            {
                ViewBag.depotcountry = Convert.ToInt32(Session["depotcountry"].ToString());
            }
            ZoneChartVM v = new ZoneChartVM();
            if (id==0)
            {
                v.ZoneChartID = 0;
                v.Details = new List<ZoneChartDetailsVM>();
                ViewBag.Title = "Create";
            }
            else
            {
                
                ZoneChart z = db.ZoneCharts.Find(id);
                v.ZoneChartID = z.ZoneChartID;
                var zone = (from c in db.ZoneMasters where c.ZoneID == z.ZoneID select c).FirstOrDefault();
                v.StatusZone = zone.StatusZone;
                v.ZoneType = zone.ZoneType;
                v.ZoneCategoryID = z.ZoneCategoryID;
                v.ZoneID = z.ZoneID;

                //var lst = (from c in db.ZoneChartDetails join loc in db.LocationMasters on c.LocationID equals loc.LocationID  where c.ZoneChartID == z.ZoneChartID select new ZoneChartDetailsVM { ZoneChartDetailID=c.ZoneChartDetailID, CountryName=loc.CountryName,CityName=loc.CityName,PlaceID=loc.PlaceID,LocationName=loc.LocationName, SubLocality=c.SubLocality} ).ToList();
                v.Details = PickupRequestDAO.GetZoneChartDetail(z.ZoneChartID); ;
                ViewBag.Title = "Modify";
            }
            //Convert.ToInt32(Session["depotcountry"].ToString());
            return View(v);
        }

        [HttpGet, ActionName("GetSelectedCity")]
        public JsonResult GetSelectedCity(string SelectedValues)
        {
                var citylist = SelectedValues.ToList();
            //var city = "";
            //foreach(var item in SelectedValues)
            //{
            //    if (city == "")
            //    {
            //        city = item.ToString();
            //    }
            //    else
            //    {
            //        city = city + "," + item.ToString();
            //    }
            //}
            List<CityVM> list = PickupRequestDAO.GetSelectedCityName(SelectedValues);
            return Json(new { Status = "OK", CityList = list, message = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, ActionName("GetSelectedCountry")]
        public JsonResult GetSelectedCountry(string SelectedValues)
        {
            //var citylist = SelectedValues.ToList();
            //var city = "";
            //foreach (var item in SelectedValues)
            //{
            //    if (city == "")
            //    {
            //        city = item.ToString();
            //    }
            //    else
            //    {
            //        city = city + "," + item.ToString();
            //    }
            //}
            List<CityVM> list = PickupRequestDAO.GetSelectedCountryName(SelectedValues);
            return Json(new { Status = "OK", CityList = list, message = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Create(ZoneChartVM v)
        {
            ZoneChart z = new ZoneChart();

            if (v.ZoneChartID == 0)
            {
                z.ZoneCategoryID = v.ZoneCategoryID;
                z.ZoneID = v.ZoneID;
                db.ZoneCharts.Add(z);
                db.SaveChanges();
            }
            else
            {
                z = db.ZoneCharts.Find(v.ZoneChartID);
                z.ZoneCategoryID = v.ZoneCategoryID;
                z.ZoneID = v.ZoneID;
                db.Entry(z).State = EntityState.Modified;
                db.SaveChanges();

                //deleting the details items
                //var details = (from d in db.ZoneChartDetails where d.ZoneChartID == z.ZoneChartID select d).ToList();
                //if (details != null)
                //{
                //    db.ZoneChartDetails.RemoveRange(details);
                //    db.SaveChanges();
                //}
            }

                List<ZoneChartDetail> l = new List<ZoneChartDetail>();
            if (v.Details!=null)
            {
                foreach (var i in v.Details)
                {
                    int LocationID = 0;
                    bool isMaanula = i.IsCountryChange; ;

                    // if (i.PlaceID != null &&  i.PlaceID!="undefined")
                    // {
                    //     var location = db.LocationMasters.Where(cc => cc.PlaceID == i.PlaceID).FirstOrDefault();

                    //     if (location == null && i.Deleted == false)
                    //     {
                    //         LocationMaster loc = new LocationMaster();
                    //         loc.CountryName = i.CountryName;
                    //         loc.CityName = i.CityName;
                    //         loc.LocationName = i.LocationName;
                    //         if(isMaanula)
                    //             loc.PlaceID = "PlaceIdNull";
                    //         else
                    //             loc.PlaceID = i.PlaceID;

                    //         db.LocationMasters.Add(loc);
                    //         db.SaveChanges();
                    //         LocationID = loc.LocationID;
                    //     }
                    //     else if (i.Deleted == false)
                    //     {
                    //         LocationID = location.LocationID;
                    //     }
                    // } 
                    //else if(i.PlaceID==null || i.PlaceID=="undefined")
                    //{
                    //     if (i.LocationName == "" && (i.CityName!="" && i.CountryName!=""))
                    //     {
                    //         var location = db.LocationMasters.Where(cc => cc.CountryName == i.CountryName && cc.CityName == i.CityName).FirstOrDefault();
                    //         if (location == null && i.Deleted == false)
                    //         {
                    //             LocationMaster loc = new LocationMaster();
                    //             loc.CountryName = i.CountryName;
                    //             loc.CityName = i.CityName;
                    //             loc.LocationName = i.LocationName;
                    //             if (isMaanula)
                    //                 loc.PlaceID = "PlaceIdNull";
                    //             else
                    //                 loc.PlaceID = i.PlaceID;

                    //             db.LocationMasters.Add(loc);
                    //             db.SaveChanges();
                    //             LocationID = loc.LocationID;
                    //         }
                    //         else if (i.Deleted == false)
                    //         {
                    //             LocationID = location.LocationID;
                    //         }
                    //     }
                    //    else if ((i.LocationName!=null && i.LocationName != "") && (i.CityName != "" && i.CountryName != ""))
                    //     {
                    //         var location = db.LocationMasters.Where(cc => cc.LocationName ==i.LocationName &&  cc.CountryName == i.CountryName && cc.CityName == i.CityName).FirstOrDefault();
                    //         if (location == null && i.Deleted == false)
                    //         {
                    //             LocationMaster loc = new LocationMaster();
                    //             loc.CountryName = i.CountryName;
                    //             loc.CityName = i.CityName;
                    //             loc.LocationName = i.LocationName;
                    //             if (isMaanula)
                    //                 loc.PlaceID = "PlaceIdNull";
                    //             else
                    //                 loc.PlaceID = i.PlaceID;

                    //             db.LocationMasters.Add(loc);
                    //             db.SaveChanges();
                    //             LocationID = loc.LocationID;
                    //         }
                    //         else if (i.Deleted == false)
                    //         {
                    //             LocationID = location.LocationID;
                    //         }
                    //     }
                    //     else if ((i.LocationName==null || i.LocationName == "") && (i.CityName == "" && i.CountryName != ""))
                    //     {
                    //         var location = db.LocationMasters.Where(cc => cc.CountryName == i.CountryName).FirstOrDefault();
                    //         if (location == null && i.Deleted == false)
                    //         {
                    //             LocationMaster loc = new LocationMaster();
                    //             loc.CountryName = i.CountryName;
                    //             loc.CityName = "";
                    //             loc.LocationName ="";                            
                    //             loc.PlaceID = "PlaceIdNull";                            
                    //             db.LocationMasters.Add(loc);
                    //             db.SaveChanges();
                    //             LocationID = loc.LocationID;
                    //         }
                    //         else if (i.Deleted == false)
                    //         {
                    //             LocationID = location.LocationID;
                    //         }
                    //     }
                    // }
                    // else
                    // {
                    //     LocationMaster loc = new LocationMaster();
                    //     loc.CountryName = i.CountryName;
                    //     loc.CityName = i.CityName;
                    //     loc.LocationName = i.LocationName;
                    //     loc.PlaceID = "PlaceIdNull";
                    //     db.LocationMasters.Add(loc);
                    //     db.SaveChanges();
                    //     LocationID = loc.LocationID;
                    // }
                    ZoneChartDetail s = new ZoneChartDetail();

                    if (i.ZoneChartDetailID > 0)
                        s = db.ZoneChartDetails.Find(i.ZoneChartDetailID);
                    else
                    {
                        s = new ZoneChartDetail();
                    }
                    if (i.ZoneChartDetailID > 0 && i.Deleted == true && s != null)
                    {
                        db.ZoneChartDetails.Remove(s);
                        db.SaveChanges();
                    }
                    else if (i.Deleted == false)
                    {
                        // s.PlaceID = i.PlaceID;
                        //s.LocationName = i.LocationName;
                        // s.LocationID = LocationID;
                        s.CountryID = i.CountryID;
                        s.CityID = i.CityID;
                        
                        //if(isMaanula)
                        //    s.PlaceID = "PlaceIdNull";
                        //else
                        //    s.PlaceID = i.PlaceID;

                        //s.PlaceID = i.PlaceID;
                        //s.SubLocality = i.SubLocality;
                        s.ZoneChartID = z.ZoneChartID;
                        if (i.ZoneChartDetailID == 0)
                        {
                            db.ZoneChartDetails.Add(s);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(s).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                //l.Add(s);

            }

            //if (v.country!=null && v.StatusZone == "I")
            //{
            //    z.ZoneCategoryID = v.ZoneCategoryID;
            //    z.ZoneID = v.ZoneID;

              
            //    //char []sep={','};
            //    //string [] lst = v.countries.Split(sep);

            //    List<ZoneChartDetail> l = new List<ZoneChartDetail>();
            //    foreach (var i in v.country)
            //    {
            //        ZoneChartDetail s = new ZoneChartDetail();
            //        s.CountryID = Convert.ToInt32(i);

            //        l.Add(s);
                   
            //    }

            //    z.ZoneChartDetails = l;

            //    db.ZoneCharts.Add(z);
            //    db.SaveChanges();
            //}

            //if (v.StatusZone == "D" && v.city!=null)
            //{
            //    z.ZoneCategoryID = v.ZoneCategoryID;
            //    z.ZoneID = v.ZoneID;


              

              
            //    z.ZoneChartDetails = l;

            //    db.ZoneCharts.Add(z);
            //    db.SaveChanges();
            //}

            TempData["SuccessMsg"] = "You have successfully added Zone Chart.";
            return RedirectToAction("Index");
        }


        public JsonResult DeleteConfirmed(int id)
        {
            StatusModel obj = new StatusModel();
            ZoneChart z=db.ZoneCharts.Find(id);
            try
            {

            
                var list = db.ZoneChartDetails.Where(x => x.ZoneChartID == id).ToList();

                foreach (var item in list)
                {
                    db.ZoneChartDetails.Remove(item);
                    db.SaveChanges();
                }

                db.ZoneCharts.Remove(z);
                db.SaveChanges();
                
            obj.Status = "OK";
            obj.Message="Zone Chart Deleted Successfully";
            }
           catch(Exception ex)
            {
                obj.Status = "Failed";
                obj.Message = ex.Message;
            }
            return Json(obj, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Edit(int id)
        {
            ZoneChartVM v = new ZoneChartVM();
            ZoneChart z = db.ZoneCharts.Find(id);
            if (z == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.ZoneChartID = z.ZoneChartID;
                v.StatusZone = (from c in db.ZoneMasters where c.ZoneID == z.ZoneID select c.StatusZone).FirstOrDefault();
                v.ZoneCategoryID = z.ZoneCategoryID;
                v.ZoneID = z.ZoneID;
               
                if (v.StatusZone == "I")
                {
                    var lst = (from c in db.ZoneChartDetails where c.ZoneChartID == z.ZoneChartID select c.CountryID).ToList();
                    v.country = new List<int>();
                    foreach (var i in lst)
                    {
                        v.country.Add(Convert.ToInt32(i));
                        v.countrylist = v.countrylist + i + ",";
                    }

                    v.countrylist=v.countrylist.Substring(0,v.countrylist.Length - 1);


                }
                else if (v.StatusZone == "D")
                {
                    var lst = (from c in db.ZoneChartDetails where c.ZoneChartID == z.ZoneChartID select c.CityID).ToList();

                    v.city = new List<int>();
                    foreach (var i in lst)
                    {
                        v.city.Add(Convert.ToInt32(i));
                        v.citylist = v.citylist + i + ",";
                    }

                    v.citylist.Substring(0,v.citylist.Length - 1);
                }
            }

            ViewBag.ZoneCategory = db.ZoneCategories.ToList();
            ViewBag.Zones = db.ZoneMasters.ToList();
            return View(v);
        }

        [HttpPost]
        public ActionResult Edit(ZoneChartVM v)
        {
            var list = db.ZoneChartDetails.Where(x => x.ZoneChartID == v.ZoneChartID).ToList();

            foreach (var item in list)
            {
                db.ZoneChartDetails.Remove(item);
                db.SaveChanges();
            }


            ZoneChart z = new ZoneChart();
            z.ZoneChartID = v.ZoneChartID;
            z.ZoneCategoryID = v.ZoneCategoryID;
            z.ZoneID = v.ZoneID;

            db.Entry(z).State = EntityState.Modified;


            if (v.country != null && v.StatusZone == "I")
            {
              

                foreach (var i in v.country)
                {
                    ZoneChartDetail s = new ZoneChartDetail();
                    s.CountryID = Convert.ToInt32(i);
                    s.ZoneChartID = z.ZoneChartID;

                    db.ZoneChartDetails.Add(s);
                    db.SaveChanges();

                }

              
            }

            if (v.StatusZone == "D" && v.city != null)
            {
              
              
                foreach (var i in v.city)
                {
                    ZoneChartDetail s = new ZoneChartDetail();
                    s.CountryID = Convert.ToInt32(Session["depotcountry"].ToString());
                    s.CityID = Convert.ToInt32(i);
                    s.ZoneChartID = z.ZoneChartID;
                    

                    db.ZoneChartDetails.Add(s);
                    db.SaveChanges();

                }

             
            }
            TempData["SuccessMsg"] = "You have successfully Updated Zone Chart.";

            return RedirectToAction("Index");



        }


        public JsonResult GetStatus(int id)
        {
            StatusZone s = new StatusZone();
            s.Status = (from c in db.ZoneMasters where c.ZoneID == id select c.StatusZone).FirstOrDefault();

         


            return Json(s, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetZoneType(int id)
        {
            StatusZone s = new StatusZone();
            var Zonetype = (from c in db.ZoneMasters where c.ZoneID == id select c.ZoneType).FirstOrDefault();

            return Json(Zonetype, JsonRequestBehavior.AllowGet);

        }
        public class StatusZone
        {
            public string Status { get; set; }
        }


        public JsonResult GetCountry()
        {
            List<CountryM> objCountry = new List<CountryM>();
            var country = (from c in db.CountryMasters select c).ToList();

            foreach (var item in country)
            {
                objCountry.Add(new CountryM { CountryName = item.CountryName, CountryID = item.CountryID });

            }
            return Json(objCountry, JsonRequestBehavior.AllowGet);
        }

        public class CountryM
        {
            public int CountryID { get; set; }
            public String CountryName { get; set; }
        }

        public JsonResult GetCity()
        {
            List<CityM> objCity = new List<CityM>();
            int depotcountry = Convert.ToInt32(Session["depotcountry"].ToString());
            var city = (from c in db.CityMasters where c.CountryID==depotcountry select c).ToList();

            foreach (var item in city)
            {
                objCity.Add(new CityM { City = item.City, CityID = item.CityID });

            }
            return Json(objCity, JsonRequestBehavior.AllowGet);
        }
        public class Prediction
        {
            public string description { get; set; }
            public string id { get; set; }
            public string place_id { get; set; }
            public string reference { get; set; }
            public List<string> types { get; set; }
        }
        public class Venue
        {
            #region Properties  
            public int Id { get; set; }
            
            public string Name { get; set; }
         
            public string ZipCode { get; set; }
          
            public string Country { get; set; }
            public string CountryName { get; set; }
         
            public string State { get; set; }
         
            public string City { get; set; }
            public string SubLocality { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
            public List<Country> Countries { get; set; }
            public string CountryCode { get; set; }
            #endregion
        }

        public class Country
        {
            #region Properties  
            public int CountryId { get; set; }
            public string CountryName { get; set; }
            public string MapReference { get; set; }
            public string CountryCode { get; set; }
            public string CountryCodeLong { get; set; }
            #endregion
        }
        public class RootObject
        {
            public List<Prediction> predictions { get; set; }
            public string status { get; set; }
        }
        public class CityM
        {
            public int CityID { get; set; }
            public String City { get; set; }
        }

        [HttpGet, ActionName("GetEventVenuesList")]
        public JsonResult GetEventVenuesList(string SearchText)
        {
            
         string GooglePlaceAPIKey = "AIzaSyAKwJ15dRInM0Vi1IAvv6C4V4vVM5HVnMc";
            //string GooglePlaceAPIUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=geocode&language=en&key={1}";
            string GooglePlaceAPIUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&language=en&key={1}";
            //< add key = "GooglePlaceAPIUrl" value = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=geocode&language=en&key={1}" />
            //< add key = "GooglePlaceAPIKey" value = "Your API Key" ></ add >
            string placeApiUrl = GooglePlaceAPIUrl; // ConfigurationManager.AppSettings["GooglePlaceAPIUrl"];

            try
            {
                placeApiUrl = placeApiUrl.Replace("{0}", SearchText);
                placeApiUrl = placeApiUrl.Replace("{1}", GooglePlaceAPIKey);// ConfigurationManager.AppSettings["GooglePlaceAPIKey"]);

                var result = new System.Net.WebClient().DownloadString(placeApiUrl);
                        var Jsonobject = JsonConvert.DeserializeObject<RootObject>(result);

                List<Prediction> list = Jsonobject.predictions;

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, ActionName("GetVenueDetailsByPlace")]
        public JsonResult GetVenueDetailsByPlace(string placeId)
        {
             string GooglePlaceAPIKey = "AIzaSyAKwJ15dRInM0Vi1IAvv6C4V4vVM5HVnMc";
            string placeDetailsApi = "https://maps.googleapis.com/maps/api/place/details/xml?placeid={0}&key={1}";

          //  string placeApiUrl = GooglePlaceAPIUrl; // ConfigurationManager.AppSettings["GooglePlaceAPIUrl"];
            try
            {
                Venue ven = new Venue();
                placeDetailsApi = placeDetailsApi.Replace("{0}", placeId);
                placeDetailsApi = placeDetailsApi.Replace("{1}", GooglePlaceAPIKey);

                var result = new System.Net.WebClient().DownloadString(placeDetailsApi);

                var xmlElm = XElement.Parse(result);
                ven = GetAllVenueDetails(xmlElm);

               return Json(ven, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>  
        /// This method is creted to get the place details from xml  
        /// </summary>  
        /// <param name="xmlElm"></param>  
        /// <returns></returns>  
        private Venue GetAllVenueDetails(XElement xmlElm)
        {
            Venue ven = new Venue();
            List<string> c = new List<string>();
            List<string> d = new List<string>();
            //get the status of download xml file  
            var status = (from elm in xmlElm.Descendants()
                          where
                              elm.Name == "status"
                          select elm).FirstOrDefault();

            //if download xml file status is ok  
            if (status.Value.ToLower() == "ok")
            {

                //get the address_component element  
                var addressResult = (from elm in xmlElm.Descendants()
                                     where
                                         elm.Name == "address_component"
                                     select elm);
                //get the location element  
                var locationResult = (from elm in xmlElm.Descendants()
                                      where
                                          elm.Name == "location"
                                      select elm);

                foreach (XElement item in locationResult)
                {
                    ven.Latitude = float.Parse(item.Elements().Where(e => e.Name.LocalName == "lat").FirstOrDefault().Value);
                    ven.Longitude = float.Parse(item.Elements().Where(e => e.Name.LocalName == "lng").FirstOrDefault().Value);
                }
                //loop through for each address_component  
                foreach (XElement element in addressResult)
                {
                    string type = element.Elements().Where(e => e.Name.LocalName == "type").FirstOrDefault().Value;

                    if (type.ToLower().Trim() == "locality") //if type is locality the get the locality  
                    {
                        ven.City = element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                    }
                    else if (type.ToLower().Trim()== "sublocality_level_1")
                    {
                        ven.SubLocality= element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                    }
                    else
                    {
                        if (type.ToLower().Trim() == "administrative_area_level_2" && string.IsNullOrEmpty(ven.City))
                        {
                            ven.City = element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                        }
                    }
                    if (type.ToLower().Trim() == "administrative_area_level_1") //if type is locality the get the locality  
                    {
                        ven.State = element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                    }
                    if (type.ToLower().Trim() == "country") //if type is locality the get the locality  
                    {
                        ven.Country = element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                        ven.CountryCode = element.Elements().Where(e => e.Name.LocalName == "short_name").Single().Value;
                        if (ven.Country == "United States") { ven.Country = "USA"; }
                    }
                    if (type.ToLower().Trim() == "postal_code") //if type is locality the get the locality  
                    {
                        ven.ZipCode = element.Elements().Where(e => e.Name.LocalName == "long_name").Single().Value;
                    }
                }
            }
            xmlElm.RemoveAll();
            return ven;
        }


        #region "CountryCitySearch"
        [HttpGet, ActionName("GetCountryList")]
        public JsonResult GetCountryList(string SearchText)
        {
            //List<CountryMasterVM> lst =(List<CountryMasterVM>)Session["CountryList"];
            //if (lst == null)
            //{
            //    lst = PickupRequestDAO.GetCountryName();
            //    Session["CountryList"] = lst;
            //}
            //try
            //{
            //    if (SearchText.Trim() =="")
            //    {
            //        var list = lst.OrderBy(cc => cc.CountryName).Take(20);
            //        return Json(list, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        var list = lst.Where(cc => cc.CountryName.ToLower().StartsWith(SearchText.ToLower())).OrderBy(cc => cc.CountryName).Take(20);
            //        return Json(list, JsonRequestBehavior.AllowGet);
            //    }                
                
            //}
            //catch (Exception ex)
            //{
            //    return Json(ex.Message, JsonRequestBehavior.AllowGet);
            //}

            if (SearchText != null && SearchText.Trim() != "")
            {
                var list = (from c1 in db.CountryMasters
                            where c1.CountryName.ToLower().Contains(SearchText.ToLower())
                            orderby c1.CountryName ascending
                            select new { Id = c1.CountryID, CountryName = c1.CountryName, CountryCode = c1.CountryCode }).Take(10);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c1 in db.CountryMasters
                            orderby c1.CountryName ascending
                            select new { Id = c1.CountryID, CountryName = c1.CountryName, CountryCode = c1.CountryCode }).Take(10);
                return Json(list, JsonRequestBehavior.AllowGet);

            }
        }



        [HttpGet, ActionName("GetCityList")]
        public JsonResult GetCityList(string SearchText)
        {
            //List<CityVM> lst = (List<CityVM>)Session["CityList"];
            //if (lst == null)
            //{
            //    lst = PickupRequestDAO.GetCityName();
            //    Session["CityList"] = lst;
            //}
            //try
            //{
            //    if (SearchText.Trim() == "")
            //    {
            //        var list = lst.OrderBy(cc => cc.City).Take(20);
            //        return Json(list, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        var list = lst.Where(cc => cc.City.ToLower().StartsWith(SearchText.ToLower())).OrderBy(cc=>cc.City).Take(20);
            //        return Json(list, JsonRequestBehavior.AllowGet);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    return Json(ex.Message, JsonRequestBehavior.AllowGet);
            //}

            if (SearchText != null && SearchText.Trim() != "")
            {
                var list = (from c1 in db.CityMasters
                            join c2 in db.CountryMasters on c1.CountryID equals c2.CountryID
                            where c1.City.ToLower().StartsWith(SearchText.ToLower())
                            orderby c1.City ascending
                            select new CityVM { CityID = c1.CityID, City = c1.City, CountryID = c2.CountryID, CountryName = c2.CountryName, CountryCode = c2.CountryCode }).Take(20).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c1 in db.CityMasters
                            join c2 in db.CountryMasters on c1.CountryID equals c2.CountryID
                            orderby c1.City ascending
                            select new CityVM { CityID = c1.CityID, City = c1.City, CountryID = c2.CountryID, CountryName = c2.CountryName, CountryCode = c2.CountryCode }).Take(20);
                return Json(list, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpGet, ActionName("GetLocationList")]
        public JsonResult GetLocationList(string SearchText)
        {
            List<LocationVM> lst = (List<LocationVM>)Session["LocationList"];
            if (lst == null || lst.Count==0)
            {
                lst = PickupRequestDAO.GetLocationName();
                Session["LocationList"] = lst;
            }
            
            try
            {
                if (SearchText.Trim() == "")
                {
                    var list = lst.OrderBy(cc => cc.Location).Take(20);
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = lst.Where(cc => cc.Location.ToLower().Contains(SearchText.ToLower())).OrderBy(cc=>cc.Location).Take(20);
                    return Json(list, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}


