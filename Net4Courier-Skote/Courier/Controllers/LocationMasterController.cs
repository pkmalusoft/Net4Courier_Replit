using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Newtonsoft.Json;
using Net4Courier.DAL;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class LocationMasterController : Controller
    {
        Entities1 db = new Entities1();


        public ActionResult Index()
        {
            LocationSearch obj = (LocationSearch)Session["CitySearch"];
            if (obj == null)
            {
                obj = new LocationSearch();
                var defaultCountry = db.CountryMasters.Where(cc => cc.CountryName == "United Arab Emirates").FirstOrDefault();
                if (defaultCountry != null)
                {
                    obj.CountryID = defaultCountry.CountryID;
                    obj.CountryName = defaultCountry.CountryName;
                }
                else
                {
                    obj.CountryID = 0;
                    obj.CountryName = "";
                }
            }
            Session["CitySearch"] = obj;
            List<LocationVM> lst = new List<LocationVM>();
            var data = (from c in db.LocationMasters where c.CountryName == obj.CountryName select new LocationVM { LocationID = c.LocationID, Location = c.LocationName, CityName = c.CityName, CountryName = c.CountryName }).ToList();


            //foreach (var item in data)
            //{
            //    LocationVM obj = new LocationVM();
            //    obj.LocationID = item.LocationID;
            //    obj.Location = item.Location;
            //    obj.CityID = item.CityID.Value;
            //    lst.Add(obj);
            //}
            obj.Details = data;
            return View(obj);
        }
        [HttpPost]
        public ActionResult Index(LocationSearch Model)
        {
            Session["CitySearch"] = Model;
            return RedirectToAction("Index");
        }

        //
        // GET: /LocationMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            LocationMaster locationmaster = db.LocationMasters.Find(id);
            if (locationmaster == null)
            {
                return HttpNotFound();
            }
            return View(locationmaster);
        }

        //
        // GET: /LocationMaster/Create

        public ActionResult Create(int id=0)
        {
            LocationVM vm = new LocationVM();
            if (id == 0)
            {
                vm.LocationID = 0;
                return View(vm);
            }
            else
            {
                var location = db.LocationMasters.Find(id);
                vm.LocationID = location.LocationID;
                vm.Location = location.LocationName;
                vm.CityID = Convert.ToInt32(location.CityID);
                vm.CountryName = location.CountryName;
                vm.CityName = location.CityName;
                return View(vm);
            }

            
        }

        //
        // POST: /LocationMaster/Create

        [HttpPost]

        public ActionResult Create(LocationVM v)
        {
            if (ModelState.IsValid)
            {

                LocationMaster ob = new LocationMaster();


                int max = (from c in db.LocationMasters orderby c.LocationID descending select c.LocationID).FirstOrDefault();

                if (max == null)
                {
                    ob.LocationID = 1;
                    ob.Location = v.Location;
                    ob.CityID = v.CityID;

                }
                else
                {
                    ob.LocationID = max + 1;
                    ob.Location = v.Location;
                    ob.CityID = v.CityID;
                }

                db.LocationMasters.Add(ob);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Location.";
                return RedirectToAction("Index");
            }


            return View(v);
        }



        public ActionResult Edit(int id)
        {
            LocationVM v = new LocationVM();
            
            ViewBag.country = db.CountryMasters.ToList();
            var data = (from c in db.LocationMasters where c.LocationID == id select c).FirstOrDefault();

            int countryid=(from c in db.CityMasters where c.CityID==data.CityID select c.CountryID).FirstOrDefault().Value;
            ViewBag.city = (from c in db.CityMasters where c.CountryID == countryid select c).ToList();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.LocationID = data.LocationID;
                v.Location = data.Location;
                v.CityID = data.CityID.Value;
                v.CountryID=countryid;
            }

            return View(v);
        }



        [HttpPost]

        public ActionResult Edit(LocationVM l)
        {
            LocationMaster a = new LocationMaster();
            a.LocationID = l.LocationID;
            a.Location = l.Location;
            a.CityID = l.CityID;
            a.PlaceID = l.PlaceID;
            a.CountryName = l.CountryName;
            a.CityName = l.CityName;
            if (ModelState.IsValid)
            {
                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Location.";
                return RedirectToAction("Index");
            }

            return View();
        }



        public ActionResult DeleteConfirmed(int id)
        {
            LocationMaster locationmaster = db.LocationMasters.Find(id);
            db.LocationMasters.Remove(locationmaster);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Location.";
            return RedirectToAction("Index");
        }

        public JsonResult GetCity(int id)
        {
            List<CityM> objCity = new List<CityM>();
            var city = (from c in db.CityMasters where c.CountryID == id select c).ToList();

            foreach (var item in city)
            {
                objCity.Add(new CityM { City = item.City, CityID = item.CityID });

            }
            return Json(objCity, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [HttpPost]
        public JsonResult SaveLocationEntry(LocationVM model)
        {
            int CountryId = 0;
            int CityId = 0;
            int LocationId = 0;
            
            if (model.LocationID>0)
            {
                var Location = db.LocationMasters.Find(model.LocationID);
                Location.LocationName = model.Location;
                Location.Location = model.Location;
                Location.CountryName = model.CountryName;
                Location.CityName = model.CityName;
                var city = db.CityMasters.Where(cc => cc.City.ToLower() == model.CityName.Trim().ToLower()).FirstOrDefault();
                if (city != null)
                {

                    Location.CityID = city.CityID;

                }
                db.Entry(Location).State = EntityState.Modified;
                db.SaveChanges();


                List<LocationVM> lst1 = PickupRequestDAO.GetLocationName();
                Session["LocationList"] = lst1;
                
                return Json(new {  status = "ok", message = "Location Updated Successfully" }, JsonRequestBehavior.AllowGet);

            }
            
            if (model.CityName == null)
                model.CityName = "";
            if (model.Location == null)
            {
                model.Location = "";
            }
            if (model.CountryName == null)
                model.CountryName = "";

            if (model.CountryCode == null)
            {
                model.CountryCode = "";
            }

            if (model.CountryName != "")
            {
                var country = db.CountryMasters.Where(cc => cc.CountryName.ToLower() == model.CountryName.Trim().ToLower()).FirstOrDefault();
                if (country != null)
                {

                    if (model.CountryCode != null)
                        country.CountryCode = model.CountryCode.Trim();
                    db.Entry(country).State = EntityState.Modified;
                    db.SaveChanges();

                    CountryId = country.CountryID;
                }
                else
                {
                    CountryMaster countrynew = new CountryMaster();
                    countrynew.CountryName = model.CountryName.Trim();
                    if (model.CountryCode != null)
                        countrynew.CountryCode = model.CountryCode.Trim();
                    db.CountryMasters.Add(countrynew);
                    db.SaveChanges();
                    CountryId = countrynew.CountryID;
                }

            }
            if (model.CityName != "" && model.CityName != null)
            {
                var city = db.CityMasters.Where(cc => cc.City.ToLower() == model.CityName.Trim().ToLower()).FirstOrDefault();
                if (city != null)
                {
                    CityId = city.CityID;
                    city.CountryID = CountryId;
                    db.Entry(city).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    CityMaster citynew = new CityMaster();
                    citynew.City = model.CityName.Trim();
                    citynew.CountryID = CountryId;
                    db.CityMasters.Add(citynew);
                    db.SaveChanges();
                    CityId = citynew.CityID;
                }
            }
            if (model.Location != null && model.Location.Trim() != "")
            {
                var Location = db.LocationMasters.Where(cc => cc.Location.ToLower() == model.Location.Trim().ToLower()).FirstOrDefault();
                if (Location != null)
                {
                    LocationId = Location.LocationID;
                    Location.CityID = CityId;
                    db.Entry(Location).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    LocationMaster locationnew = new LocationMaster();
                    locationnew.Location = model.Location;
                    locationnew.LocationName = model.Location;
                    locationnew.CityID = CityId;
                    locationnew.CityName = model.CityName;
                    locationnew.CountryName = model.CountryName;
                    db.LocationMasters.Add(locationnew);
                    db.SaveChanges();
                    LocationId = locationnew.LocationID;
                }
            }
            List<LocationVM> lst2 = PickupRequestDAO.GetLocationName();
            Session["LocationList"] = lst2;

            LocationVM vm = new LocationVM();
            vm.LocationID = LocationId;
            vm.CityID = CityId;
            vm.CountryID = CountryId;
            vm.CityName = model.CityName;
            vm.CountryName = model.CountryName;
            vm.Location = model.Location;
            vm.CountryCode = model.CountryCode;

            return Json(new { data = vm, status = "ok", message = "Location Saved Successfully" }, JsonRequestBehavior.AllowGet);

        }

        public void UpdateLocationMaster()
        {

            string GooglePlaceAPIKey = "AIzaSyAKwJ15dRInM0Vi1IAvv6C4V4vVM5HVnMc";
            //string GooglePlaceAPIUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=geocode&language=en&key={1}";
            string GooglePlaceAPIUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&language=en&key={1}";
            //< add key = "GooglePlaceAPIUrl" value = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=geocode&language=en&key={1}" />
            //< add key = "GooglePlaceAPIKey" value = "Your API Key" ></ add >
            string placeApiUrl = GooglePlaceAPIUrl; // ConfigurationManager.AppSettings["GooglePlaceAPIUrl"];

            try
            {
                var data1 = db.LocationMasters.ToList();
                foreach (var item in data1)
                {
                    placeApiUrl = placeApiUrl.Replace("{0}", item.LocationName);
                    placeApiUrl = placeApiUrl.Replace("{1}", GooglePlaceAPIKey);// ConfigurationManager.AppSettings["GooglePlaceAPIKey"]);

                    var result = new System.Net.WebClient().DownloadString(placeApiUrl);
                    var Jsonobject = JsonConvert.DeserializeObject<RootObject>(result);

                    List<Prediction> list = Jsonobject.predictions;
                    item.PlaceID = list[0].place_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                
                               

                
            }
            catch (Exception ex)
            {
                
            }
        }
    }
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

public class Prediction
{
    public string description { get; set; }
    public string id { get; set; }
    public string place_id { get; set; }
    public string reference { get; set; }
    public List<string> types { get; set; }
}