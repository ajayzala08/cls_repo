using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebApplication3.Controllers
{
    [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class clsforgotpasswordController : ApiController
    {
        public HttpResponseMessage post(string emailid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    var findusermail = db.cls_usermst_tbl.Where(x => x.user_emailid == emailid).FirstOrDefault();
                    if (findusermail != null)
                    {
                        string htmlbody = string.Empty;

                        htmlbody += "<b>Hello " + findusermail.user_fullname.ToString() + ",</b> <br><br>";
                        htmlbody += "Click here to reset password : " + "<a href='#' target='_blank'>Reset Password</a><br><br>";
                        htmlbody += "Thank You"; 


                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("ajay.zala@archesoftronix.com");
                        mail.To.Add(new MailAddress(emailid.ToString()));
                        mail.Subject = "Password reset link";
                        mail.Body = htmlbody;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "mail1.archesoftronix.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = false;
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = new System.Net.NetworkCredential("ajay.zala@archesoftronix.com", "Ajay@2013", "");
                        smtp.Send(mail);
                        response = Request.CreateResponse(HttpStatusCode.OK, "Password reset link send on your mail id");
                    }
                    else
                    {
                        response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "No account exists related to provided mailid.");
                    }
                }

                
                return response;
            }
            catch (Exception ex) {
                response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }

        }
        public HttpResponseMessage post(clspasswordresetmodel model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormationdbEntities())
                {
                    var finduser = db.cls_usermst_tbl.Where(x => x.user_username == model.username && x.user_emailid == model.email).FirstOrDefault();
                    if (finduser != null)
                    {
                        finduser.user_pwd = model.newpwd;
                        db.Entry(finduser).State = EntityState.Modified;
                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, "Password reset successfully.");
                        }
                        else
                        {
                            response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Fail to reset password");
                        }
                    }
                    else
                    {
                        response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Record not found");
                    }
                }
                
                return response;
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }

        }

    }
}
