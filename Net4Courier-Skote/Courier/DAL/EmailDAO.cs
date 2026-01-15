using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Web.Http;
using System.Configuration;

namespace Net4Courier.DAL
{
    public class EmailDAO
    {

        public string SendCustomerEmail(string customeremail,string username,string passwd)
        {
            bool boolMailSent = false;

            try
            {
                string StrHost = ConfigurationManager.AppSettings["MailServer"]; 
                string StrPort = ConfigurationManager.AppSettings["EMailPort"];
                string StrCredentialEmail = ConfigurationManager.AppSettings["SMTPAdminEmail"];
                string StrPassword = ConfigurationManager.AppSettings["SMTPPassword"];
                string SiteName = ConfigurationManager.AppSettings["SiteName"];
                using (MailMessage mail = new MailMessage())
                {
                    mail.To.Clear();
                    mail.To.Add(customeremail);
                    mail.From = new MailAddress(StrCredentialEmail);
                    mail.Subject = "User Login";
                    //  mail.Body = "Title : " + Title + "<BR/>" + "Description : " + Desc + "<BR/>"+"Requested On : " + Date ;
                    string linkstring = "";
                                        
                    mail.Body = "Hi " + username + ",<BR/><BR/>Welcome to www.malusoftindia.com Courier System! <BR/> Now you can Login to " + SiteName + " by using user:" + username + " and password:" + passwd;

                    //else
                    //{

                    //System.Net.Mail.Attachment attachment;
                    //attachment = new System.Net.Mail.Attachment(System.Web.HttpContext.Current.Server.MapPath("~/UploadedFiles/" + ds.Tables[2].Rows[0]["UploadFilename"].ToString()));
                    //mail.Attachments.Add(attachment);
                    //}                                                                                                             

                    //mail.CC.Add(StrCCEmail);
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = StrHost;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 2147483647;

                    smtp.Port = Convert.ToInt32(StrPort);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(StrCredentialEmail, StrPassword);
                    smtp.Send(mail);
                    smtp.Dispose();
                    boolMailSent = true;
                }
            }
            catch (Exception ex)
            {
                boolMailSent = false;
            }

             if (boolMailSent)
                return "Ok";
            else
                return "Failed";

        }


        public string SendForgotMail(string customeremail, string username, string passwd)
        {
            bool boolMailSent = false;

            try
            {
                string StrHost = ConfigurationManager.AppSettings["MailServer"];
                string StrPort = ConfigurationManager.AppSettings["EMailPort"];
                string StrCredentialEmail = ConfigurationManager.AppSettings["SMTPAdminEmail"];
                string StrPassword = ConfigurationManager.AppSettings["SMTPPassword"];
                string SiteName = ConfigurationManager.AppSettings["SiteName"];
                using (MailMessage mail = new MailMessage())
                {
                    mail.To.Clear();
                    mail.To.Add(customeremail);
                    mail.From = new MailAddress(StrCredentialEmail);
                    mail.Subject = "Reset Password ";
                    //  mail.Body = "Title : " + Title + "<BR/>" + "Description : " + Desc + "<BR/>"+"Requested On : " + Date ;
                    string linkstring = "";

                    mail.Body = "Hi " + username + ",<BR/><BR/>Welcome to www.malusoftindia.com Courier System! <BR/> Now you can Login to " + SiteName +  " by using user:" + customeremail + " and password:" + passwd;

                    //else
                    //{

                    //System.Net.Mail.Attachment attachment;
                    //attachment = new System.Net.Mail.Attachment(System.Web.HttpContext.Current.Server.MapPath("~/UploadedFiles/" + ds.Tables[2].Rows[0]["UploadFilename"].ToString()));
                    //mail.Attachments.Add(attachment);
                    //}                                                                                                             

                    //mail.CC.Add(StrCCEmail);
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = StrHost;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 2147483647;

                    smtp.Port = Convert.ToInt32(StrPort);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(StrCredentialEmail, StrPassword);
                    smtp.Send(mail);
                    smtp.Dispose();
                    boolMailSent = true;
                }
            }
            catch (Exception ex)
            {
                boolMailSent = false;
            }

            if (boolMailSent)
                return "Ok";
            else
                return "Failed";

        }


        public string SendCustomerAWBNoNotification(string customeremail, string customername,string mailmessage,string AWBNo)
        {
            bool boolMailSent = false;

            try
            {
                string StrHost = ConfigurationManager.AppSettings["MailServer"];
                string StrPort = ConfigurationManager.AppSettings["EMailPort"];
                string StrCredentialEmail = ConfigurationManager.AppSettings["SMTPAdminEmail"];
                string StrPassword = ConfigurationManager.AppSettings["SMTPPassword"];
                string SiteName = ConfigurationManager.AppSettings["SiteName"];
                using (MailMessage mail = new MailMessage())
                {
                    mail.To.Clear();
                    mail.To.Add(customeremail);
                    mail.From = new MailAddress(StrCredentialEmail);
                    mail.Subject = "Notification on AWBNO : " + AWBNo;
                    //  mail.Body = "Title : " + Title + "<BR/>" + "Description : " + Desc + "<BR/>"+"Requested On : " + Date ;
                    string linkstring = "";

                    mail.Body = "Hi " + customername + ",<BR/><BR/>" + mailmessage;
                                        
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = StrHost;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 2147483647;

                    smtp.Port = Convert.ToInt32(StrPort);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(StrCredentialEmail, StrPassword);
                    smtp.Send(mail);
                    smtp.Dispose();
                    boolMailSent = true;
                }
            }
            catch (Exception ex)
            {
                boolMailSent = false;
            }

            if (boolMailSent)
                return "Ok";
            else
                return "Failed";

        }
    }
}