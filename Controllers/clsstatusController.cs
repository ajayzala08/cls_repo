using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class clsstatusController : ApiController
    {
        public HttpResponseMessage GET()
        {
            HttpResponseMessage response;
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    List<statusmodel> model = db.cls_statusmst_tbl.Select(x => new statusmodel
                    {
                        cfid = x.cfid,
                        companyName = x.company_name,
                        status = x.form_status
                    }).ToList();
                    if (model.Count > 0)
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, model);
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "No Record Found");
                    }
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Excpetion");
            }
            return response;
        }

        public HttpResponseMessage put(decimal cfid, string status)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                using (var db = new CompanyFormation_dbEntities())
                {
                    var tbl = db.cls_statusmst_tbl.Where(x => x.cfid == cfid).FirstOrDefault();
                    if (tbl != null)
                    {

                        if (status == "Completed")
                        {
                            var sourcepath = tbl.pdf_filepath;
                            var destinationpath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Completed"), tbl.pdf_filename);
                            var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Completed"));
                            if (!exists)
                            {
                                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Completed/"));
                                File.Move(sourcepath, destinationpath);
                                //File.Copy(sourcepath, destinationpath);
                            }
                            else
                            {
                                File.Move(sourcepath, destinationpath);
                            }
                            tbl.form_status = status;
                            tbl.pdf_filepath = destinationpath;
                            tbl.createdon = DateTime.Now;
                            db.Entry(tbl).State = EntityState.Modified;
                            var result = db.SaveChanges();
                            if (result > 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK, "Status Updated");
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.NotModified, "Fail To Update");
                            }
                        }
                        if (status == "InProgress")
                        {
                            var sourcepath = tbl.pdf_filepath;
                            var destinationpath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/In Progress"), tbl.pdf_filename);
                            var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/In Progress"));
                            if (!exists)
                            {
                                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/In Progress/"));
                                File.Move(sourcepath, destinationpath);
                                //File.Copy(sourcepath, destinationpath);
                            }
                            else
                            {
                                File.Move(sourcepath, destinationpath);
                            }
                            tbl.form_status = status;
                            tbl.pdf_filepath = destinationpath;
                            tbl.createdon = DateTime.Now;
                            db.Entry(tbl).State = EntityState.Modified;
                            var result = db.SaveChanges();
                            if (result > 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK, "Status Updated");
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.NotModified, "Fail To Update");
                            }
                        }
                        if (status == "Submit")
                        {
                            var sourcepath = tbl.pdf_filepath;
                            var destinationpath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Submit"), tbl.pdf_filename);
                            var exists = Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Submit"));
                            if (!exists)
                            {
                                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/clscharteredsecretaries/" + tbl.company_name + "/Submit/"));
                                File.Move(sourcepath, destinationpath);
                                //File.Copy(sourcepath, destinationpath);
                            }
                            else
                            {
                                File.Move(sourcepath, destinationpath);
                            }
                            tbl.form_status = status;
                            tbl.pdf_filepath = destinationpath;
                            tbl.createdon = DateTime.Now;
                            db.Entry(tbl).State = EntityState.Modified;
                            var result = db.SaveChanges();
                            if (result > 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK, "Status Updated");
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.NotModified, "Fail To Update");
                            }
                        }
                        return response;
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "No Record Found");
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Exception");
                return response;
            }
        }
    }
}
