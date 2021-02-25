using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;
using System.Web.Configuration;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clscreateticketController : ApiController
    {
        public HttpResponseMessage postclscreateticket()
        {

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    var httpRequest = HttpContext.Current.Request;
                    DataSet dsexcelRecords = new DataSet();
                    //IExcelDataReader reader = null;
                    HttpPostedFile Inputfile = null;
                    Stream FileStream = null;
                    string filelist = string.Empty;
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        FileInfo fi = new FileInfo(httpRequest.Files[i].FileName);
                        if ((fi.Extension.ToLower() == ".doc") || (fi.Extension.ToLower() == ".docx") || (fi.Extension.ToLower() == ".pdf") || (fi.Extension.ToLower() == ".jpeg")
                            || (fi.Extension.ToLower() == ".jpg") || (fi.Extension.ToLower() == ".png"))
                        {
                            Stream input = httpRequest.Files[i].InputStream;
                            var fileName = Guid.NewGuid();
                            using (Stream file = File.OpenWrite(HttpContext.Current.Server.MapPath("~/TicketRepo/" + fileName + fi.Extension.ToString())))
                            {
                                input.CopyTo(file);
                                //close file  
                                file.Close();
                                input.Dispose();
                            }


                            if (filelist == "")
                                filelist = fileName + fi.Extension.ToString();
                            else
                                filelist += "," + fileName + fi.Extension.ToString();

                        }
                    }

                    string tn = generateticketno();
                    cls_ticketmst_tbl tbl = new cls_ticketmst_tbl();
                    tbl.ticket_number = tn;
                    tbl.ticket_email = httpRequest.Form["email"].ToString();
                    tbl.ticket_name = httpRequest.Form["name"].ToString();
                    tbl.ticket_business = httpRequest.Form["business"].ToString();
                    tbl.ticket_phoneno = httpRequest.Form["phoneno"].ToString();
                    tbl.ticket_extension = httpRequest.Form["extension"].ToString();
                    tbl.ticket_helptopic = httpRequest.Form["helptopics"].ToString();
                    tbl.ticket_issuesummary = httpRequest.Form["issuesummary"].ToString();
                    tbl.ticket_details = httpRequest.Form["details"].ToString();
                    tbl.ticket_files = filelist;
                    tbl.ticket_initial_status = "Open";
                    tbl.ticket_createdate = DateTime.Now;
                    tbl.ticket_final_status = "Open";
                    tbl.ticket_finaldate = DateTime.Now;
                    db.cls_ticketmst_tbl.Add(tbl);
                    if (db.SaveChanges() > 0)
                    {
                        email_send(httpRequest.Form["email"].ToString(), tn, httpRequest.Form["name"].ToString());
                        response = Request.CreateResponse(HttpStatusCode.Created, tn);
                        return response;
                    }
                    else
                    {
                        response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Fail to Generate Ticket");
                        return response;
                    }

                }

            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }


        }
        private string generateticketno()
        {
            string ticketno = string.Empty;
            using (var db = new CompanyFormation_dbEntities())
            {
                var lastticketno = db.cls_ticketmst_tbl.OrderByDescending(x => x.ticket_id).FirstOrDefault();
                if (lastticketno != null)
                {
                    string firststring = lastticketno.ticket_number.Substring(0, 8);
                    string secondstring = DateTime.Now.Year.ToString() + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd");
                    string lastthreedigit = lastticketno.ticket_number.Substring(8, 3);
                    if (firststring == secondstring)
                    {
                        ticketno = secondstring + (Convert.ToInt32(lastthreedigit) + Convert.ToInt32("1")).ToString("000");
                    }
                    else
                    {
                        ticketno = secondstring + "001";
                    }
                }
                else
                {
                    ticketno = DateTime.Now.Year.ToString() + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + "001";

                }
            }

            return ticketno;

        }
        private void email_send(string tomailid, string ticketno, string name)
        {

            //string fromaddr = WebConfigurationManager.AppSettings["username"].ToString();
            //string toaddr = tomailid;//TO ADDRESS HERE
            //string password = WebConfigurationManager.AppSettings["pwd"].ToString();

            //MailMessage msg = new MailMessage();
            //msg.Subject = "Ticket";
            //msg.From = new MailAddress(fromaddr);
            //msg.Body = "Ticket Number : " + ticketno.ToString();
            //msg.To.Add(new MailAddress(toaddr));
            //SmtpClient smtp = new SmtpClient();
            //smtp.Host = "smtp.gmail.com";
            //smtp.Port = 587;
            //smtp.UseDefaultCredentials = false;
            //smtp.EnableSsl = true;
            //NetworkCredential nc = new NetworkCredential(fromaddr, password);
            //smtp.Credentials = nc;
            //smtp.Send(msg);

            string body = "Hi " + name + "," + "<br><br>" +
                "An access link request for ticket " + ticketno + " has been submitted on your behalf for the helpdesk at http://soporte.fermar.com.mx." + "<br><br>" +

                "Follow the link below to check the status of the ticket " + ticketno + "." + "<br><br>" +

                "http://soporte.fermar.com.mx/view.php?auth=o1x4iaqaadacmaaaVckJsM2sIHfirg%3D%3D" + "<br><br>" +

                "If you did not make the request, please delete and disregard this email.Your account is still secure and no one has been given access to the ticket.Someone could have mistakenly entered your email address. " + "<br><br>" +

                "--" + "<br><br>" +
                "Via Ticket ";


            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("ajay.zala@archesoftronix.com");
            mail.To.Add(new MailAddress(tomailid));
            mail.Subject = "Ticket [" + ticketno + "] Access Link";
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "mail1.archesoftronix.com";
            smtp.Port = 587;
            smtp.EnableSsl = false;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new System.Net.NetworkCredential("ajay.zala@archesoftronix.com", "Ajay@2013", "");
            smtp.Send(mail);

        }
        public HttpResponseMessage Get(string status)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {

                    if (status == "All")
                    {
                        var ticketlist = db.cls_ticketmst_tbl.ToList();
                        if (ticketlist.Count > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, ticketlist.ToList());
                            return response;
                        }
                        else
                        {
                            response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");
                            return response;
                        }
                    }
                    else
                    {
                        var ticketlist = db.cls_ticketmst_tbl.Where(x => x.ticket_final_status == status).ToList();
                        if (ticketlist.Count > 0)
                        {
                            response = Request.CreateResponse(HttpStatusCode.OK, ticketlist.ToList());
                            return response;
                        }
                        else
                        {
                            response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");
                            return response;
                        }
                    }

                    
                }

                
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }
        }
        
    }

}
