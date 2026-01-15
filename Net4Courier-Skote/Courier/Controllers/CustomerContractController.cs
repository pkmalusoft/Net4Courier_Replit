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
    [SessionExpire]
    public class CustomerContractController : Controller
    {
        private Entities1 db = new Entities1();
        // GET: CustomerContract
        public ActionResult Index()
        {
            List<CustomerContractVM> lst = PickupRequestDAO.GetCustomerContracts(0,"");
            return View(lst);
        }

        public ActionResult Create(int id=0)
        {
            CustomerContractVM vm = new CustomerContractVM();
            if (id>0)
            {
                ViewBag.Title = "Rate Chart - Modify";
                vm.CustomerID = id;
                vm.CustomerName = db.CustomerMasters.Find(id).CustomerName;
                ViewBag.EditMode = "true";
                vm.MovementId = "1,2,3,4";
            }
            else
            {
                ViewBag.Title = "Rate Chart";
                ViewBag.EditMode = "false";
                vm.CustomerID = 0;
                vm.CustomerName = "";
                vm.MovementId = "1,2,3,4";

            }
            return View(vm);
        }

        public JsonResult GetCustomerContract(int id,string CourierType)
        {            
            List<CustomerContractVM> lst = PickupRequestDAO.GetCustomerContracts(id,CourierType);
            return Json(lst, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult SaveContract(int CustomerId,string selectedvalues,string Details)
        {
            var IDetails = JsonConvert.DeserializeObject<List<CustomerContractVM>>(Details);
            var data = (from c in db.CustomerMultiContracts where c.CustomerID== CustomerId select c).ToList();
            foreach (var item in data)
            {
                db.CustomerMultiContracts.Remove(item);
                db.SaveChanges();
            }

            foreach (var item in IDetails)
            {
                CustomerMultiContract cm = new CustomerMultiContract();
                cm.CustomerID = CustomerId;
                cm.CustomerRateTypeID = item.CustomerRateTypeID;
                db.CustomerMultiContracts.Add(cm);
                db.SaveChanges();
            }

            return Json(new { status = "ok", message = "You have successfully Saved Customer Contract!" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeleteConfirmed(int id = 0)
        {
            StatusModel obj = new StatusModel();
            try
            {
                CustomerMultiContract a = db.CustomerMultiContracts.Find(id);
                if (a == null)
                {
                    obj.Status = "Failed";
                    obj.Message = "Contract not found!";
                }
                else
                {
                    db.CustomerMultiContracts.Remove(a);
                    db.SaveChanges();


                    obj.Message = "Customer Contract Deleted Succesfully!";
                    obj.Status = "OK";
                }
            }
            catch(Exception ex)
            {
                obj.Status = "Failed";
                obj.Message = ex.Message;
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCustomerName(string term)
        {
            bool enablecashcustomer = (bool)Session["EnableCashCustomerInvoice"];
            if (term.Trim() != "")
            {
                if (enablecashcustomer == true)
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive == true && c1.CustomerID >= -1 && (c1.CustomerType == "CL" || c1.CustomerType == "CS" || c1.CustomerType == "CR") && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive == true && (c1.CustomerID==-1 || c1.CustomerID > 0 && (c1.CustomerType == "CL" || c1.CustomerType == "CR")) && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (enablecashcustomer == true)
                {

                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CS" || c1.CustomerType == "CR")
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0 && (c1.CustomerType == "CR")
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                }
            }




        }

    }
}