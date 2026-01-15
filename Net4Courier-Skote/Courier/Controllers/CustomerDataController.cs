using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.IO;
using System.Data;
using System.Data.SqlClient;
//using System.Web.Mvc;
using System.Web.Http;
using AttributeRouting.Web.Mvc;

namespace Net4Courier.Controllers
{
    public class CustomerDataController : ApiController
    {
        //[Route("api/CustomerData/GetAccounts")]
        //[HttpPost]
        //public string SelectDocumentDetails(ModalDocumentClass _ModalDocumentClass)
        //{

        //    ModelDocument ObjDocumentModel = new ModelDocument();
        //    DataTable dt = ObjDocumentModel.SelectDocumentDetails(_ModalDocumentClass);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        //}

        //[Route("api/CustomerData/GetAccounts")]
        [HttpPost]
        public string GetAccounts()
        {
            int branchID = 5; // Convert.ToInt32(Session["CurrentBranchID"].ToString());
            List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
            AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID).ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(AccountHeadList);// IEnumerable<AcHeadSelectAllVM>AccountHeadList;
        }


//        // GET api/<controller>
//        [HttpGet]
//        public IEnumerable<AcHeadSelectAllVM> GetAccounts()
//        {
//            int branchID = 5; // Convert.ToInt32(Session["CurrentBranchID"].ToString());
//              List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
//                AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID).ToList();
//            //List<AcHeadSelectAll_Result> AccountHeadList = new List<AcHeadSelectAll_Result>();
//            //AccountHeadList = db.AcHeadSelectAll(branchID).ToList();
//            return AccountHeadList; // Json(AccountHeadList, JsonRequestBehavior.AllowGet);
            
////            return new string[] { "value1", "value2" };
//        }
        
        //[HttpGet]        
        //public HttpResponseMessage GetAccount()
        //{
        //    int branchID = 5; // Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
        //    AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID).ToList();
        //    // Then I return the list
        //    return Request.CreateResponse(HttpStatusCode.OK, AccountHeadList);
        //}

        //[System.Web.Http.HttpGet("Search")]
        //public ActionResult Search()
        //{
        //    int branchID = 5; // Convert.ToInt32(Session["CurrentBranchID"].ToString());
        //    List<AcHeadSelectAllVM> AccountHeadList = new List<AcHeadSelectAllVM>();
        //    AccountHeadList = AccountsDAO.GetAcHeadSelectAll(branchID).ToList();
        //    return Ok(AccountHeadList);
        //}
        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}