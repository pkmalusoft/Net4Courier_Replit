using Net4Courier.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    public class SkyLarkController : Controller
    {
        // GET: SkyLark
        public ActionResult VatInvoice(string id)
        {
            ViewBag.ReportName = "Invoice Printing";
            AccountsReportsDAO.SkylarkVATTaxInvoiceReport(id);
            string file = Session["ReportOutput"].ToString();
       
            // Specify the path to the PDF file
            string filePath = Server.MapPath(file);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("File not found");
            }

            // Read the file into a byte array
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Specify the file name for download
            string fileName = "VATInvoice_" + id +".pdf";

            // Return the file to be downloaded by the client
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}