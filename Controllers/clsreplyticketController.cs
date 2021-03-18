using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsreplyticketController : ApiController
    {
        public HttpResponseMessage postclsreplyticket()
        {
            HttpResponseMessage response;
            try
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
                using (var db = new CompanyFormation_dbEntities())
                {
                    string tn = httpRequest.Form["ticketno"].ToString();
                    cls_ticketreply_tbl tbl = new cls_ticketreply_tbl();
                    tbl.tciket_number = tn;
                    tbl.reply_email = httpRequest.Form["email"].ToString();
                    tbl.reply_name = httpRequest.Form["name"].ToString();
                    tbl.reply_details = httpRequest.Form["details"].ToString();
                    tbl.reply_files = filelist;
                    tbl.reply_status = httpRequest.Form["status"].ToString();
                    tbl.reply_date = DateTime.Now;
                    db.cls_ticketreply_tbl.Add(tbl);
                    if (db.SaveChanges() > 0)
                    {
                        var mst = db.cls_ticketmst_tbl.Where(x => x.ticket_number == tn).FirstOrDefault(); ;
                        if (mst != null)
                        {
                            mst.ticket_final_status = httpRequest.Form["status"].ToString();
                            mst.ticket_finaldate = DateTime.Now;
                            db.Entry(mst).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        response = Request.CreateResponse(HttpStatusCode.Created, "Successfull");
                        return response;
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Fail");
                        return response;
                    }

                }


            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Exception");
                return response;
            }


        }
        public HttpResponseMessage Get(string ticketno)
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    List<replydetails> replies = db.cls_ticketreply_tbl.Where(x => x.tciket_number == ticketno).Select(x => new replydetails
                    {
                        replayid = x.reply_id,
                        ticketno = x.tciket_number,
                        name = x.reply_name,
                        email = x.reply_email,
                        details = x.reply_details,
                        files = x.reply_files,
                        status = x.reply_status,
                        replydate = (DateTime)x.reply_date
                    }).ToList();

                    var ticket = db.cls_ticketmst_tbl.Where(x => x.ticket_number == ticketno).FirstOrDefault();
                    ticketviewmodel model = new ticketviewmodel();
                    model.ticketid = ticket.ticket_id;
                    model.ticketno = ticket.ticket_number;
                    model.name = ticket.ticket_name;
                    model.email = ticket.ticket_email;
                    model.business = ticket.ticket_business;
                    model.phoneno = ticket.ticket_phoneno;
                    model.extension = ticket.ticket_extension;
                    model.helptopics = ticket.ticket_helptopic;
                    model.issuesummary = ticket.ticket_issuesummary;
                    model.details = ticket.ticket_details;
                    model.files = ticket.ticket_files;
                    model.initialstatus = ticket.ticket_initial_status;
                    model.createdate = Convert.ToDateTime(ticket.ticket_createdate);
                    model.finalstatus = ticket.ticket_final_status;
                    model.finaldate = Convert.ToDateTime(ticket.ticket_finaldate);
                    model.replies = replies;
                    if (model != null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, model);
                    }
                    else
                    {
                        response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Record Found");
                    }
                    return response;
                }

            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }

            //return response;
        }
    }
}
