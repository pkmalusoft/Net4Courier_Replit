using Net4Courier.DAL;
using Net4Courier.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class EmailController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SendEmail(string EmailType,string to,string customername, string subject, string body,string regards,string FileName)
        {

            //Update Email Template
            EmailTemplate emailmodel = new EmailTemplate();
            emailmodel = db.EmailTemplates.Where(cc => cc.EmailType == EmailType).FirstOrDefault();
            
            if (emailmodel!=null)
            {
                emailmodel.EmailSubject = subject;
                emailmodel.Content = body;
                emailmodel.RegardsBy = regards;
                db.Entry(emailmodel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                emailmodel = new EmailTemplate();
                emailmodel.EmailType = EmailType;
                emailmodel.EmailSubject = subject;
                emailmodel.Content = body;
                emailmodel.RegardsBy = regards;
                db.EmailTemplates.Add(emailmodel);
                db.SaveChanges();
            }

            // Similar to the previous example, configure and send the email using SmtpClient
            // ...
            


            // Example:
            string strbuilder = "";
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            string companyname = "";
            var branch = db.BranchMasters.Find(branchid);
            companyname = branch.BranchName;
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,                
                Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPAdminEmail"].ToString(), ConfigurationManager.AppSettings["SMTPPassword"].ToString()),
                EnableSsl = true,
            };

            strbuilder += "Dear " + customername + ",";
            strbuilder += "<br />";
            strbuilder += "<br />";
            strbuilder += "<p style='text-align:justify;margin-left:0px'>" + body + "</p>";
            //strbuilder += "Thank you for your order. Please find the enclosed PDF Invoice.";
            strbuilder += "<br />";
            strbuilder += "<br />";
            strbuilder += "Thank You";            
            strbuilder += "<br />";
            strbuilder += regards;
            strbuilder += "<br />";
            strbuilder += "<br />";
            strbuilder += "For <b>" + companyname + "</b>";
            

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"].ToString()),
                Subject = subject,
                Body = strbuilder,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            // Attach the file
            var attachmentPath = Server.MapPath(FileName);
            Attachment attachment = new Attachment(attachmentPath);
            mailMessage.Attachments.Add(attachment);
            smtpClient.Send(mailMessage);
            StatusModel obj = new StatusModel();
            obj.Status = "OK";
            obj.Message = "Email Send Succesfully!";
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmailTemplateDetail(string EmailType)
        {
            var emailtemplate = db.EmailTemplates.Where(cc => cc.EmailType == EmailType).FirstOrDefault();
            if (emailtemplate != null)
            {
                return Json(new { Status = "OK", data = emailtemplate }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                emailtemplate = new EmailTemplate();
                return Json(new { Status = "OK",data=emailtemplate}, JsonRequestBehavior.AllowGet);
            }
        }
    }
}