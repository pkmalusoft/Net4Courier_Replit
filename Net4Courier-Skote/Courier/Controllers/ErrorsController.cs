using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    //[Authorize]
    public class ErrorsController : Controller
    {
        //
        // GET: /Errors/

        public ActionResult Errors()
        {

            return View();
        }
        public ActionResult SessionTimeOut()
        {
            return View();
        }
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
